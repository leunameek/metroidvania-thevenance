using System;
using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The persistent side of GameAudio: pooled one-shot voices (flat and spatial), registered loops,
// the music and ambience beds with crossfades, ducking (dialogue, voice listening, stingers),
// the pause mix (action sounds stop, music and beds soften and darken), the loading screen fade,
// scene changes (world sounds stop, unclaimed beds fade out), sound captions, and the player's
// footsteps component on every scene that has a player.
public sealed class GameAudioHost : MonoBehaviour
{
    public static GameAudioHost Current { get; private set; }
    // Every one-shot that really starts (id, channel): tests and debugging read the timeline.
    public static event Action<string, AudioChannel> Played;

    private const int FlatVoices = 14, SpatialVoices = 18;

    private sealed class Voice
    {
        public AudioSource Source;
        public string Id;
        public AudioChannel Channel;
        public float Started;
        public bool Paused;
    }

    private sealed class LoopEntry { public AudioChannel Channel; public float Volume; public bool Paused; }

    private sealed class Bed
    {
        public AudioSource[] Sources = new AudioSource[2];
        public AudioLowPassFilter[] Filters = new AudioLowPassFilter[2];
        public float[] Weight = new float[2], Target = new float[2], Rate = new float[2];
        public int Active;
        public float Volume;
        public bool Claimed = true;
        public AudioChannel Channel;
    }

    private readonly List<Voice> _flat = new List<Voice>(), _spatial = new List<Voice>();
    private readonly Dictionary<AudioSource, LoopEntry> _loops = new Dictionary<AudioSource, LoopEntry>();
    private readonly List<AudioSource> _deadLoops = new List<AudioSource>();
    private readonly Dictionary<string, DuckRequest> _ducks = new Dictionary<string, DuckRequest>();
    private readonly Dictionary<string, float> _duckExpiry = new Dictionary<string, float>();
    private readonly List<string> _expired = new List<string>();
    private Bed _music, _ambience;
    private float _duckMusic = 1, _duckAmbience = 1, _duckEffects = 1, _loadGain = 1, _pauseMix;
    private bool _paused;
    private float _claimDeadline = -1, _nextPlayerCheck;
    private RectTransform _captionRoot;
    private readonly List<(TMP_Text text, float until)> _captions = new List<(TMP_Text, float)>();

    public bool Paused => _paused;
    // The music now asked for (null when fading out), so a duel can restore it afterwards.
    public AudioClip MusicClip => _music.Target[_music.Active] > 0 ? _music.Sources[_music.Active].clip : null;
    public float MusicVolume => _music.Volume;
    public AudioClip AmbienceClip => _ambience.Target[_ambience.Active] > 0 ? _ambience.Sources[_ambience.Active].clip : null;
    public float DuckGain(AudioChannel channel) => ChannelDuck(channel);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Current = null; Played = null; }

    public static GameAudioHost Ensure()
    {
        if (Current != null) return Current;
        if (!Application.isPlaying) return null;
        var go = new GameObject("GameAudio");
        DontDestroyOnLoad(go);
        return go.AddComponent<GameAudioHost>();
    }

    private void Awake()
    {
        if (Current != null && Current != this) { Destroy(gameObject); return; }
        Current = this;
        for (int i = 0; i < FlatVoices; i++) _flat.Add(new Voice { Source = NewSource(gameObject, false) });
        for (int i = 0; i < SpatialVoices; i++)
        {
            var child = new GameObject("Voz3D_" + i);
            child.transform.SetParent(transform, false);
            _spatial.Add(new Voice { Source = NewSource(child, true) });
        }
        _music = NewBed("Musica", AudioChannel.Music);
        _ambience = NewBed("Ambiente", AudioChannel.Ambience);
        SceneManager.activeSceneChanged += OnSceneChanged;
        GameAudio.ReadSettings();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        if (Current == this) Current = null;
    }

    private static AudioSource NewSource(GameObject host, bool spatial)
    {
        var s = host.AddComponent<AudioSource>();
        s.playOnAwake = false; s.loop = false; s.dopplerLevel = 0;
        s.spatialBlend = spatial ? 1 : 0;
        s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 2.5f; s.maxDistance = 30; s.spread = 40;
        return s;
    }

    private Bed NewBed(string name, AudioChannel channel)
    {
        var bed = new Bed { Channel = channel };
        for (int i = 0; i < 2; i++)
        {
            var child = new GameObject(name + "_" + i);
            child.transform.SetParent(transform, false);
            var s = NewSource(child, false); s.loop = true; s.priority = channel == AudioChannel.Music ? 16 : 24;
            s.ignoreListenerPause = true;
            bed.Sources[i] = s;
            bed.Filters[i] = child.AddComponent<AudioLowPassFilter>();
            bed.Filters[i].cutoffFrequency = 22000;
        }
        return bed;
    }

    // ------------------------------------------------------------------ one-shots

    public AudioSource PlayOneShot(string id, AudioClip clip, Vector3? position, float volume, AudioChannel channel, float pitch,
        float spread, float minInterval, int maxVoices, float maxDistance)
    {
        if (clip == null) return null;
        var pool = position.HasValue ? _spatial : _flat;
        int playing = 0;
        foreach (var v in pool) if (v.Id == id && v.Source.isPlaying) playing++;
        if (!GameAudio.Limiter.Allow(id, Time.unscaledTime, minInterval, playing, maxVoices)) return null;
        if (_paused && AudioMix.PausesWithGame(channel)) return null; // nothing new starts behind the pause menu
        var voice = Free(pool, channel);
        if (voice == null) return null;
        var s = voice.Source;
        s.Stop();
        s.clip = clip;
        float level = spread > 0 ? AudioMix.DbToGain(GameAudio.Limiter.Range(-1.5f, 1f) * spread / .03f) : 1;
        s.volume = Mathf.Clamp01(volume * level * GameAudio.Volume(channel) * ChannelDuck(channel));
        s.pitch = pitch * (spread > 0 ? 1 + GameAudio.Limiter.Range(-spread, spread) : 1);
        s.priority = Priority(channel, id);
        s.ignoreListenerPause = channel == AudioChannel.Interface;
        if (position.HasValue)
        {
            s.transform.position = position.Value;
            s.maxDistance = Mathf.Max(4, maxDistance);
        }
        voice.Id = id; voice.Channel = channel; voice.Started = Time.unscaledTime; voice.Paused = false;
        s.Play();
        Played?.Invoke(id, channel);
        return s;
    }

    private static int Priority(AudioChannel channel, string id)
    {
        if (channel == AudioChannel.Interface) return 20;
        if (channel == AudioChannel.Music) return 30;
        if (id.Contains("aviso") || id.Contains("dano") || id.Contains("turno")) return 40; // warnings and hits first
        if (channel == AudioChannel.Voice) return 60;
        if (id.Contains("paso_")) return 170;
        if (channel == AudioChannel.Ambience) return 200;
        return 120;
    }

    private static Voice Free(List<Voice> pool, AudioChannel channel)
    {
        Voice oldest = null;
        foreach (var v in pool)
        {
            if (!v.Source.isPlaying && !v.Paused) return v;
            if (v.Channel == AudioChannel.Interface && channel != AudioChannel.Interface) continue;
            if (oldest == null || v.Started < oldest.Started) oldest = v;
        }
        return oldest; // the oldest sound gives its voice to the new one
    }

    public void Delay(float seconds, Action action) => StartCoroutine(DelayRoutine(seconds, action));

    private static IEnumerator DelayRoutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    // ------------------------------------------------------------------ loops

    public AudioSource AddLoop(GameObject owner, AudioClip clip, float volume, bool spatial, AudioChannel channel, float maxDistance)
    {
        if (owner == null) return null;
        var s = owner.AddComponent<AudioSource>();
        s.clip = clip; s.loop = true; s.playOnAwake = false; s.dopplerLevel = 0;
        s.spatialBlend = spatial ? 1 : 0; s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 2; s.maxDistance = maxDistance;
        s.priority = channel == AudioChannel.Ambience ? 150 : 100;
        _loops[s] = new LoopEntry { Channel = channel, Volume = volume };
        s.volume = LoopLevel(_loops[s]);
        if (clip != null)
        {
            // Beds and long loops start at a random point so two of them never phase together.
            if (clip.length > 4f) s.time = UnityEngine.Random.Range(0f, clip.length * .9f);
            s.Play();
            if (_paused && AudioMix.PausesWithGame(channel)) { s.Pause(); _loops[s].Paused = true; }
        }
        return s;
    }

    public void SetLoopVolume(AudioSource source, float volume)
    {
        if (source == null || !_loops.TryGetValue(source, out var entry)) return;
        entry.Volume = volume;
        source.volume = LoopLevel(entry);
    }

    private float LoopLevel(LoopEntry e)
        => e.Volume * GameAudio.Volume(e.Channel) * ChannelDuck(e.Channel) * AudioMix.PauseGain(e.Channel, _paused) * (e.Channel == AudioChannel.Interface ? 1 : _loadGain);

    // ------------------------------------------------------------------ beds

    public void SetBed(bool music, AudioClip clip, float volume, float fade)
    {
        var bed = music ? _music : _ambience;
        bed.Claimed = true;
        var active = bed.Sources[bed.Active];
        if (clip != null && active.clip == clip && bed.Target[bed.Active] > 0)
        {
            bed.Volume = volume;
            return;
        }
        float rate = 1f / Mathf.Max(.05f, fade);
        // The active source fades out; the other one takes the new clip and fades in.
        bed.Target[bed.Active] = 0; bed.Rate[bed.Active] = rate;
        if (clip == null) return;
        int next = 1 - bed.Active;
        var s = bed.Sources[next];
        s.Stop(); s.clip = clip; s.time = 0;
        bed.Weight[next] = 0; bed.Target[next] = 1; bed.Rate[next] = rate;
        bed.Active = next; bed.Volume = volume;
        s.Play();
    }

    private void UpdateBed(Bed bed, float dt)
    {
        float channel = GameAudio.Volume(bed.Channel) * ChannelDuck(bed.Channel) * AudioMix.PauseGain(bed.Channel, _paused) * _loadGain;
        for (int i = 0; i < 2; i++)
        {
            bed.Weight[i] = Mathf.MoveTowards(bed.Weight[i], bed.Target[i], bed.Rate[i] * dt);
            var s = bed.Sources[i];
            // Equal-power curve for the crossfade.
            float w = Mathf.Sin(bed.Weight[i] * Mathf.PI * .5f);
            s.volume = w * bed.Volume * channel;
            if (bed.Weight[i] <= 0 && bed.Target[i] <= 0 && s.isPlaying) s.Stop();
            bed.Filters[i].cutoffFrequency = Mathf.Lerp(22000, 1400, _pauseMix);
        }
    }

    // ------------------------------------------------------------------ ducking

    public void SetDuck(string key, DuckRequest request) { _ducks[key] = request; _duckExpiry.Remove(key); }
    public void ClearDuck(string key) { _ducks.Remove(key); _duckExpiry.Remove(key); }
    public void DuckFor(string key, DuckRequest request, float seconds) { _ducks[key] = request; _duckExpiry[key] = Time.unscaledTime + seconds; }

    private float ChannelDuck(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Music: return _duckMusic;
            case AudioChannel.Ambience: return _duckAmbience;
            case AudioChannel.Effects: case AudioChannel.Voice: return _duckEffects;
            default: return 1;
        }
    }

    // ------------------------------------------------------------------ frame

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        // Dialogue keeps music and beds under the text (guion C "Audio y lectura").
        if (StoryPlayer.Active) { if (!_ducks.ContainsKey("dialogo")) SetDuck("dialogo", DuckRequest.Dialogue); }
        else if (_ducks.ContainsKey("dialogo")) ClearDuck("dialogo");
        _expired.Clear();
        foreach (var pair in _duckExpiry) if (Time.unscaledTime > pair.Value) _expired.Add(pair.Key);
        foreach (var key in _expired) ClearDuck(key);
        _duckMusic = Mathf.MoveTowards(_duckMusic, AudioMix.DbToGain(AudioMix.DuckDb(AudioChannel.Music, _ducks.Values)), dt * 1.6f);
        _duckAmbience = Mathf.MoveTowards(_duckAmbience, AudioMix.DbToGain(AudioMix.DuckDb(AudioChannel.Ambience, _ducks.Values)), dt * 1.6f);
        _duckEffects = Mathf.MoveTowards(_duckEffects, AudioMix.DbToGain(AudioMix.DuckDb(AudioChannel.Effects, _ducks.Values)), dt * 1.6f);

        bool loading = SceneLoader.Loading;
        _loadGain = Mathf.MoveTowards(_loadGain, loading ? 0 : 1, dt * (loading ? 3f : 1.2f));
        SetPaused(!loading && Time.timeScale <= 0f);
        _pauseMix = Mathf.MoveTowards(_pauseMix, _paused ? 1 : 0, dt * 3f);

        if (_claimDeadline > 0 && Time.unscaledTime > _claimDeadline)
        {
            _claimDeadline = -1;
            foreach (var bed in new[] { _music, _ambience })
                if (!bed.Claimed) { bed.Target[bed.Active] = 0; bed.Rate[bed.Active] = 1f / 1.5f; }
        }
        UpdateBed(_music, dt);
        UpdateBed(_ambience, dt);

        _deadLoops.Clear();
        foreach (var pair in _loops)
        {
            if (pair.Key == null) { _deadLoops.Add(pair.Key); continue; }
            pair.Key.volume = LoopLevel(pair.Value);
        }
        foreach (var dead in _deadLoops) _loops.Remove(dead);

        UpdateCaptions();
        if (Time.unscaledTime > _nextPlayerCheck) { _nextPlayerCheck = Time.unscaledTime + 1f; EnsureFootsteps(); }
    }

    private void SetPaused(bool paused)
    {
        if (paused == _paused) return;
        _paused = paused;
        foreach (var pool in new[] { _flat, _spatial })
            foreach (var v in pool)
            {
                if (!AudioMix.PausesWithGame(v.Channel)) continue;
                if (paused && v.Source.isPlaying) { v.Source.Pause(); v.Paused = true; }
                else if (!paused && v.Paused) { v.Source.UnPause(); v.Paused = false; }
            }
        foreach (var pair in _loops)
        {
            if (pair.Key == null || !AudioMix.PausesWithGame(pair.Value.Channel)) continue;
            if (paused && pair.Key.isPlaying) { pair.Key.Pause(); pair.Value.Paused = true; }
            else if (!paused && pair.Value.Paused) { pair.Key.UnPause(); pair.Value.Paused = false; }
        }
    }

    // A new scene: the old world's sounds stop; music and beds wait one second for the new scene
    // to ask for them (same clip = continues seamlessly), otherwise they fade out.
    private void OnSceneChanged(Scene from, Scene to)
    {
        foreach (var pool in new[] { _flat, _spatial })
            foreach (var v in pool)
                if (v.Channel != AudioChannel.Interface && v.Channel != AudioChannel.Music) { v.Source.Stop(); v.Paused = false; }
        _music.Claimed = _ambience.Claimed = false;
        _claimDeadline = Time.unscaledTime + 1f;
        _ducks.Clear(); _duckExpiry.Clear();
        GameAudio.DefaultSurface = "piedra";
        foreach (var c in _captions) if (c.text != null) Destroy(c.text.gameObject);
        _captions.Clear();
        _nextPlayerCheck = 0;
    }

    private static void EnsureFootsteps()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null && player.GetComponent<PlayerFootsteps>() == null) player.gameObject.AddComponent<PlayerFootsteps>();
    }

    // ------------------------------------------------------------------ captions

    public void ShowCaption(string text)
    {
        if (_captionRoot == null)
        {
            var canvas = UIKit.ScreenCanvas(transform, "SubtitulosDeSonido", 880);
            _captionRoot = UIKit.Rect("Lineas", canvas);
            UIKit.Place(_captionRoot, new Vector2(0, 0), new Vector2(40, 150), new Vector2(620, 140));
            _captionRoot.pivot = new Vector2(0, 0);
            var layout = _captionRoot.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerLeft; layout.spacing = 4;
            layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
        }
        foreach (var c in _captions) if (c.text != null && c.text.text == "[" + text + "]") return;
        if (_captions.Count >= 3) { if (_captions[0].text != null) Destroy(_captions[0].text.gameObject); _captions.RemoveAt(0); }
        var label = UIKit.Label(_captionRoot, "[" + text + "]", 24, UIPalette.Ivory);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        UIKit.Shadow(label);
        _captions.Add((label, Time.unscaledTime + 2.8f));
    }

    private void UpdateCaptions()
    {
        for (int i = _captions.Count - 1; i >= 0; i--)
        {
            var (text, until) = _captions[i];
            if (text == null) { _captions.RemoveAt(i); continue; }
            float left = until - Time.unscaledTime;
            text.alpha = Mathf.Clamp01(left / .5f);
            if (left <= 0) { Destroy(text.gameObject); _captions.RemoveAt(i); }
        }
    }
}

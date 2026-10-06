using System;
using System.Collections.Generic;
using UnityEngine;

// Sound bank of the Mundo Superior (Resources/MSAudio, built by
// tools/audio/generate_mundo_superior_audio.py; guide 16.1 S01-S23). Volumes follow the channels
// of the game settings (effects, ambience, interface; music rides the ambience slider) read from
// the saved settings, master stays on the AudioListener. Every essential cue is also shown on
// screen (guide 16.2): sounds complement the level and it plays fine without them.
public static class MSAudio
{
    public enum Channel { Effects, Ambience, Music, Interface }

    [Serializable]
    private sealed class Mix { public float effects = .7f, ambience = .35f, uiVolume = .3f; }

    private const string SettingsKey = "Nemequene.UI.Settings.v1";
    private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
    // Looping sources with their channel and base volume, for the mix and the pause.
    private static readonly Dictionary<AudioSource, (Channel channel, float volume)> Loops = new Dictionary<AudioSource, (Channel, float)>();
    private static AudioSource _oneShots;
    private static Mix _mix;
    private static float _lastUnavailable = -10f;
    private static bool _actionsPaused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Clips.Clear(); Loops.Clear(); _oneShots = null; _mix = null; _lastUnavailable = -10f; _actionsPaused = false;
    }

    // Re-reads the settings (the director calls it on load and after the pause menu).
    public static void ReadSettings()
    {
        Mix mix = null;
        try { mix = JsonUtility.FromJson<Mix>(PlayerPrefs.GetString(SettingsKey, "{}")); } catch (Exception) { }
        _mix = mix ?? new Mix();
        foreach (var pair in Loops) if (pair.Key != null) pair.Key.volume = pair.Value.volume * Volume(pair.Value.channel);
    }

    public static float Volume(Channel channel)
    {
        if (_mix == null) ReadSettings();
        switch (channel)
        {
            case Channel.Ambience: return Mathf.Clamp01(_mix.ambience * 1.4f);
            case Channel.Music: return Mathf.Clamp01(_mix.ambience * 1.6f);
            case Channel.Interface: return Mathf.Clamp01(_mix.uiVolume * 2f);
            default: return Mathf.Clamp01(_mix.effects);
        }
    }

    public static AudioClip Clip(string name)
    {
        if (Clips.TryGetValue(name, out var clip) && clip != null) return clip;
        clip = Resources.Load<AudioClip>("MSAudio/" + name);
        Clips[name] = clip;
        return clip;
    }

    // Non-spatial cue (finds, wings, portals, interface). Interface cues ignore the pause.
    public static void Play(string name, float volume = 1f, float pitch = 1f, Channel channel = Channel.Effects)
    {
        var clip = Clip(name);
        if (clip == null) return;
        if (_oneShots == null)
        {
            var go = new GameObject("MSAudio");
            _oneShots = go.AddComponent<AudioSource>(); _oneShots.playOnAwake = false; _oneShots.spatialBlend = 0; _oneShots.ignoreListenerPause = true;
        }
        _oneShots.pitch = pitch;
        _oneShots.PlayOneShot(clip, volume * Volume(channel));
    }

    // One of the numbered variants (paso_piedra_1..4) with a small pitch spread.
    public static void PlayVariant(string prefix, int count, float volume = 1f, float pitchSpread = .06f)
        => Play(prefix + "_" + UnityEngine.Random.Range(1, count + 1), volume, 1f + UnityEngine.Random.Range(-pitchSpread, pitchSpread));

    // World cue at a position (lock, transport stops).
    public static void PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f, float maxDistance = 30f)
    {
        var clip = Clip(name);
        if (clip == null) return;
        var go = new GameObject("MSAudio_" + name);
        go.transform.position = position;
        var source = go.AddComponent<AudioSource>();
        source.clip = clip; source.volume = volume * Volume(Channel.Effects); source.pitch = pitch;
        source.spatialBlend = 1; source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 3; source.maxDistance = maxDistance;
        source.Play();
        UnityEngine.Object.Destroy(go, clip.length / Mathf.Max(.1f, pitch) + .1f);
    }

    // S23: a soft "not available", at most once every 0.6 s however often E is pressed.
    public static void Unavailable()
    {
        if (Time.unscaledTime - _lastUnavailable < .6f) return;
        _lastUnavailable = Time.unscaledTime;
        Play("no_disponible", .9f, 1f, Channel.Interface);
    }

    // Looping source owned by an object (wind, music layers, flight, portal hum, transport).
    public static AudioSource Loop(GameObject owner, string name, float volume, bool spatial, Channel channel, float maxDistance = 25f)
    {
        var source = owner.AddComponent<AudioSource>();
        source.clip = Clip(name); source.loop = true; source.playOnAwake = false;
        source.spatialBlend = spatial ? 1 : 0; source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 2; source.maxDistance = maxDistance;
        Loops[source] = (channel, volume);
        source.volume = volume * Volume(channel);
        if (source.clip != null) source.Play();
        if (_actionsPaused && channel == Channel.Effects) source.Pause();
        return source;
    }

    public static void SetLoopVolume(AudioSource source, float volume)
    {
        if (source == null || !Loops.TryGetValue(source, out var entry)) return;
        Loops[source] = (entry.channel, volume);
        source.volume = volume * Volume(entry.channel);
    }

    // Guide 16.2: on pause the continuous action sounds stop; music and wind keep a pause mix.
    public static void PauseActions(bool paused)
    {
        _actionsPaused = paused;
        foreach (var pair in Loops)
        {
            if (pair.Key == null || pair.Value.channel != Channel.Effects) continue;
            if (paused) pair.Key.Pause(); else pair.Key.UnPause();
        }
    }
}

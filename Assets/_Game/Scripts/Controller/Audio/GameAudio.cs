using System;
using System.Collections.Generic;
using UnityEngine;

public enum UICue { Focus, Confirm, Back, Open, Close, Blocked, Adjust, Tab, Objective, Dialogue }

// One mixer for the whole game (see Specs/Audio/Revision-de-audio.md). Clips come from Resources:
// "Foley/paso_piedra" means Resources/GameAudio/Foley/paso_piedra(_1.._n).wav; "MIAudio/caida"
// and other full Resources paths work too. Numbered variants are picked without repeating, with
// a small pitch and level spread. Every cue goes through a channel (effects, ambience, music,
// voices, interface) read from the saved settings; the persistent GameAudioHost owns the pooled
// sources, the music and ambience crossfades, the ducking, the pause mix and scene changes.
// MIAudio, MSAudio and PlazaAudio are thin facades over this class.
public static class GameAudio
{
    public const string Bank = "GameAudio/";
    private const string SettingsKey = "Nemequene.UI.Settings.v1";

    private static readonly Dictionary<string, AudioClip[]> Groups = new Dictionary<string, AudioClip[]>();
    private static AudioMixSettings _mix;
    private static bool _muted;

    public static readonly CueLimiter Limiter = new CueLimiter();
    public static event Action<string> CaptionRaised;

    // Floor used by the footsteps when a collider says nothing about its material.
    public static string DefaultSurface = "piedra";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Groups.Clear(); _mix = null; _muted = false; Limiter.Clear(); CaptionRaised = null; DefaultSurface = "piedra"; _lastOpen = -10f;
    }

    // ------------------------------------------------------------------ settings

    public static AudioMixSettings Mix { get { if (_mix == null) ReadSettings(); return _mix; } }

    // Settings changed (pause menus, title): re-read the JSON, master on the listener.
    public static void ReadSettings()
    {
        AudioMixSettings mix = null;
        string json = PlayerPrefs.GetString(SettingsKey, "");
        try { if (!string.IsNullOrEmpty(json)) mix = JsonUtility.FromJson<AudioMixSettings>(json); } catch (Exception) { }
        mix = mix ?? new AudioMixSettings();
        // Saves from before the music slider: the menu music value carries over (x2, its old scale).
        if (!string.IsNullOrEmpty(json) && !json.Contains("\"music\"")) mix.music = Mathf.Clamp01(mix.menuMusic * 2f);
        if (!string.IsNullOrEmpty(json) && !json.Contains("\"voices\"")) mix.voices = .8f;
        _mix = mix;
        AudioListener.volume = _muted ? 0 : Mathf.Clamp01(mix.master);
    }

    // The settings screens push their values directly (also before they are saved).
    public static void SetMix(AudioMixSettings mix)
    {
        if (mix == null) return;
        _mix = mix;
        AudioListener.volume = _muted ? 0 : Mathf.Clamp01(mix.master);
    }

    public static float Volume(AudioChannel channel) => AudioMix.Gain(Mix, channel);

    public static bool Muted
    {
        get => _muted;
        set { _muted = value; AudioListener.volume = value ? 0 : Mathf.Clamp01(Mix.master); }
    }

    // ------------------------------------------------------------------ clips

    public static AudioClip Clip(string id)
    {
        var group = Group(id);
        if (group == null || group.Length == 0) return null;
        return group[Limiter.NextVariant(id, group.Length)];
    }

    public static bool Has(string id) => Group(id) != null;

    private static AudioClip[] Group(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (Groups.TryGetValue(id, out var cached)) return cached;
        AudioClip[] found = null;
        foreach (var path in Candidates(id))
        {
            var single = Resources.Load<AudioClip>(path);
            if (single != null) { found = new[] { single }; break; }
            var list = new List<AudioClip>();
            for (int i = 1; i <= 12; i++)
            {
                var clip = Resources.Load<AudioClip>(path + "_" + i);
                if (clip == null) break;
                list.Add(clip);
            }
            if (list.Count > 0) { found = list.ToArray(); break; }
        }
        Groups[id] = found;
        return found;
    }

    private static IEnumerable<string> Candidates(string id)
    {
        yield return id;
        if (!id.StartsWith(Bank, StringComparison.Ordinal)) yield return Bank + id;
    }

    // ------------------------------------------------------------------ playback

    // Non-spatial cue (the player's body, interface, finds, warnings). Returns null when the cue
    // is missing or refused by the limiter (it already plays too often).
    public static AudioSource Play(string id, float volume = 1f, AudioChannel channel = AudioChannel.Effects, float pitch = 1f,
        float spread = .03f, float minInterval = .05f, int maxVoices = 3)
        => GameAudioHost.Ensure()?.PlayOneShot(id, Clip(id), null, volume, channel, pitch, spread, minInterval, maxVoices, 30f);

    // World cue at a position (doors, falling stones, creatures, fountains).
    public static AudioSource PlayAt(string id, Vector3 position, float volume = 1f, AudioChannel channel = AudioChannel.Effects,
        float maxDistance = 30f, float pitch = 1f, float spread = .03f, float minInterval = .05f, int maxVoices = 3)
        => GameAudioHost.Ensure()?.PlayOneShot(id, Clip(id), position, volume, channel, pitch, spread, minInterval, maxVoices, maxDistance);

    // A clip already referenced by a scene (PlazaAudio's serialized cues).
    public static AudioSource PlayClip(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.Effects, float pitch = 1f, float spread = .03f)
        => clip == null ? null : GameAudioHost.Ensure()?.PlayOneShot(clip.name, clip, null, volume, channel, pitch, spread, .05f, 3, 30f);

    public static void PlayDelayed(string id, float delay, float volume = 1f, AudioChannel channel = AudioChannel.Effects, Vector3? position = null)
        => GameAudioHost.Ensure()?.Delay(delay, () => { if (position.HasValue) PlayAt(id, position.Value, volume, channel); else Play(id, volume, channel); });

    // Looping source owned by an object (dies with it). Its volume follows the channel, the
    // ducking and the pause; SetLoopVolume changes its own level.
    public static AudioSource Loop(GameObject owner, string id, float volume, bool spatial, AudioChannel channel, float maxDistance = 25f)
        => GameAudioHost.Ensure()?.AddLoop(owner, Clip(id), volume, spatial, channel, maxDistance);

    public static AudioSource LoopClip(GameObject owner, AudioClip clip, float volume, bool spatial, AudioChannel channel, float maxDistance = 25f)
        => GameAudioHost.Ensure()?.AddLoop(owner, clip, volume, spatial, channel, maxDistance);

    public static void SetLoopVolume(AudioSource source, float volume) => GameAudioHost.Current?.SetLoopVolume(source, volume);

    // Music and ambience beds: one of each at a time, crossfaded, kept across a scene change only
    // when the new scene asks for the same id again.
    public static void Music(string id, float volume = 1f, float fade = 2f) => GameAudioHost.Ensure()?.SetBed(true, Clip(id), volume, fade);
    public static void MusicClip(AudioClip clip, float volume = 1f, float fade = 2f) => GameAudioHost.Ensure()?.SetBed(true, clip, volume, fade);
    public static void StopMusic(float fade = 2f) => GameAudioHost.Current?.SetBed(true, null, 0, fade);
    public static void Ambience(string id, float volume = 1f, float fade = 2f) => GameAudioHost.Ensure()?.SetBed(false, Clip(id), volume, fade);
    public static void AmbienceClip(AudioClip clip, float volume = 1f, float fade = 2f) => GameAudioHost.Ensure()?.SetBed(false, clip, volume, fade);
    public static void StopAmbience(float fade = 2f) => GameAudioHost.Current?.SetBed(false, null, 0, fade);

    // A short musical phrase (discovery, objective, release, defeat): music dips under it.
    public static void Stinger(string id, float volume = 1f)
    {
        var source = Play(id, volume, AudioChannel.Music, 1f, 0f, .5f, 1);
        if (source != null) GameAudioHost.Current?.DuckFor("stinger", DuckRequest.Stinger, source.clip.length * .8f);
    }

    private static float _lastOpen = -10f;

    public static void UI(UICue cue, float volume = 1f)
    {
        // A menu that opens selects its first button: that focus is part of the opening.
        if (cue == UICue.Open || cue == UICue.Tab) _lastOpen = Time.unscaledTime;
        else if (cue == UICue.Focus && Time.unscaledTime - _lastOpen < .25f) return;
        switch (cue)
        {
            case UICue.Focus: Play("UI/ui_foco", .55f * volume, AudioChannel.Interface, 1f, .02f, .06f, 2); break;
            case UICue.Confirm: Play("UI/ui_confirmar", .8f * volume, AudioChannel.Interface, 1f, .02f, .08f, 2); break;
            case UICue.Back: Play("UI/ui_atras", .75f * volume, AudioChannel.Interface, 1f, .02f, .08f, 2); break;
            case UICue.Open: Play("UI/ui_abrir", .8f * volume, AudioChannel.Interface, 1f, .02f, .15f, 1); break;
            case UICue.Close: Play("UI/ui_cerrar", .75f * volume, AudioChannel.Interface, 1f, .02f, .15f, 1); break;
            case UICue.Blocked: Play("UI/ui_bloqueado", .8f * volume, AudioChannel.Interface, 1f, .02f, .5f, 1); break;
            case UICue.Adjust: Play("UI/ui_ajuste", .5f * volume, AudioChannel.Interface, 1f, .05f, .06f, 1); break;
            case UICue.Tab: Play("UI/ui_pestana", .6f * volume, AudioChannel.Interface, 1f, .03f, .08f, 1); break;
            case UICue.Objective: Play("UI/ui_objetivo", .8f * volume, AudioChannel.Interface, 1f, 0f, 2.5f, 1); break;
            case UICue.Dialogue: Play("UI/ui_dialogo", .45f * volume, AudioChannel.Interface, 1f, .04f, .12f, 1); break;
        }
    }

    // ------------------------------------------------------------------ ducking and captions

    public static void SetDuck(string key, DuckRequest request) => GameAudioHost.Ensure()?.SetDuck(key, request);
    public static void ClearDuck(string key) => GameAudioHost.Current?.ClearDuck(key);

    // Essential sounds are also written on screen ("Cuerno responde", "Siseo · cabeza A") when
    // the sound captions setting is on (default). The plaza's subtitle view can subscribe.
    public static void Caption(string text)
    {
        if (string.IsNullOrEmpty(text) || !Mix.soundCaptions) return;
        if (CaptionRaised != null) CaptionRaised(text);
        else GameAudioHost.Ensure()?.ShowCaption(text);
    }
}

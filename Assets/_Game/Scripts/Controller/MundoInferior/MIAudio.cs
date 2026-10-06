using System.Collections.Generic;
using UnityEngine;

// Synthesised sound bank of the Mundo Inferior (Resources/MIAudio, built by
// tools/audio/generate_mundo_inferior_audio.py). Every essential cue is also shown on screen
// (guide 7.3); sounds are complementary and the level plays fine without them.
public static class MIAudio
{
    private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
    private static AudioSource _ui;
    public static float Volume = 0.8f;

    public static AudioClip Clip(string name)
    {
        if (Clips.TryGetValue(name, out var clip) && clip != null) return clip;
        clip = Resources.Load<AudioClip>("MIAudio/" + name);
        Clips[name] = clip;
        return clip;
    }

    // Non-spatial cue (finds, menus, rests).
    public static void Play(string name, float volume = 1f, float pitch = 1f)
    {
        var clip = Clip(name);
        if (clip == null) return;
        if (_ui == null)
        {
            var go = new GameObject("MIAudio_UI");
            _ui = go.AddComponent<AudioSource>(); _ui.playOnAwake = false; _ui.spatialBlend = 0; _ui.ignoreListenerPause = true;
        }
        _ui.pitch = pitch;
        _ui.PlayOneShot(clip, volume * Volume);
    }

    // World cue at a position (gates, rocks, slabs).
    public static void PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        var clip = Clip(name);
        if (clip == null) return;
        var go = new GameObject("MIAudio_" + name);
        go.transform.position = position;
        var source = go.AddComponent<AudioSource>();
        source.clip = clip; source.volume = volume * Volume; source.pitch = pitch;
        source.spatialBlend = 1; source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 3; source.maxDistance = 30;
        source.Play();
        Object.Destroy(go, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
    }

    // Looping source owned by an object (ambience, pendulum creak, boss music).
    public static AudioSource Loop(GameObject owner, string name, float volume, bool spatial, float maxDistance = 25f)
    {
        var clip = Clip(name);
        var source = owner.AddComponent<AudioSource>();
        source.clip = clip; source.loop = true; source.volume = volume * Volume; source.playOnAwake = false;
        source.spatialBlend = spatial ? 1 : 0; source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 2; source.maxDistance = maxDistance;
        if (clip != null) source.Play();
        return source;
    }
}

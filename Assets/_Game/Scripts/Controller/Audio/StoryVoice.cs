using System.Collections.Generic;
using UnityEngine;

// The voiced interjection that opens each story line (SpeechVoices): made once per speaker,
// mood and variant, kept for the session, played on the Voice channel.
public static class StoryVoice
{
    private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
    private static AudioSource _last;
    private static AudioClip _lastClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { Clips.Clear(); _last = null; _lastClip = null; }

    public static AudioClip Clip(string speaker, SpeechMood mood, int variant)
    {
        var voice = SpeechVoices.For(speaker);
        if (voice == null) return null;
        variant = ((variant % SpeechVoices.Variants) + SpeechVoices.Variants) % SpeechVoices.Variants;
        string key = "Voz_" + speaker + "_" + mood + "_" + variant;
        if (Clips.TryGetValue(key, out var clip) && clip != null) return clip;
        var samples = SpeechVoices.Render(voice, mood, variant);
        clip = AudioClip.Create(key, samples.Length, 1, SpeechVoices.SampleRate, false);
        clip.SetData(samples, 0);
        Clips[key] = clip;
        return clip;
    }

    // A new line cuts the previous interjection short, as when someone answers at once.
    public static void Say(string speaker, string text, int index)
    {
        var clip = Clip(speaker, SpeechVoices.Mood(text), index);
        // The pooled source may already carry another cue: only our own interjection is stopped.
        if (_last != null && _last.isPlaying && _last.clip == _lastClip) _last.Stop();
        _last = clip != null ? GameAudio.PlayClip(clip, .9f, AudioChannel.Voice, 1f, .02f) : null;
        _lastClip = clip;
    }
}

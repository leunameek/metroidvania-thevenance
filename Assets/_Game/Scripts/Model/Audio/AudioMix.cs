using System;
using System.Collections.Generic;

// Channels of the game mix. Master stays on the AudioListener; each channel has its own slider.
public enum AudioChannel { Effects, Ambience, Music, Voice, Interface }

// The audio part of the saved settings (same JSON as Nemequene.UI.UISettings, read on its own
// so the runtime does not depend on the UI assembly). Missing fields keep these defaults.
[Serializable]
public sealed class AudioMixSettings
{
    public float master = .8f, effects = .7f, ambience = .35f, uiVolume = .3f, music = .6f, voices = .8f;
    public float menuMusic = .3f;
    public bool soundCaptions = true;
}

// Pure mixing rules (EditMode-tested): channel gains, the ducking under dialogue and while the
// voice commands are listened to, the pause mix, and the limits against piling up the same cue.
public static class AudioMix
{
    // Trims keep the defaults balanced: ambience and interface sliders were tuned low on purpose.
    public static float Gain(AudioMixSettings s, AudioChannel channel)
    {
        if (s == null) s = new AudioMixSettings();
        switch (channel)
        {
            case AudioChannel.Ambience: return Clamp01(s.ambience * 1.4f);
            case AudioChannel.Music: return Clamp01(s.music);
            case AudioChannel.Voice: return Clamp01(s.voices);
            case AudioChannel.Interface: return Clamp01(s.uiVolume * 2f);
            default: return Clamp01(s.effects);
        }
    }

    public static float DbToGain(float db) => (float)Math.Pow(10, db / 20f);

    // Ducking in dB per channel. Dialogue keeps music under the text (guion C "Audio y lectura");
    // a listening window (voice duel turn) lowers music and beds further so the microphone hears
    // the player, not the game. The strongest request wins per channel; they do not add up.
    public static float DuckDb(AudioChannel channel, IEnumerable<DuckRequest> requests)
    {
        float db = 0;
        if (requests == null) return 0;
        foreach (var r in requests)
        {
            float value = channel == AudioChannel.Music ? r.MusicDb : channel == AudioChannel.Ambience ? r.AmbienceDb
                : channel == AudioChannel.Effects || channel == AudioChannel.Voice ? r.EffectsDb : 0;
            if (value < db) db = value;
        }
        return db;
    }

    // While paused: action sounds stop (handled by the host), music and beds stay softer.
    public static float PauseGain(AudioChannel channel, bool paused)
    {
        if (!paused) return 1;
        return channel == AudioChannel.Music ? .55f : channel == AudioChannel.Ambience ? .5f : 1;
    }

    // Channels whose sounds stop while the game is paused (the interface keeps answering).
    public static bool PausesWithGame(AudioChannel channel) => channel == AudioChannel.Effects || channel == AudioChannel.Voice;

    private static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
}

public struct DuckRequest
{
    public float MusicDb, AmbienceDb, EffectsDb;
    public DuckRequest(float musicDb, float ambienceDb, float effectsDb = 0) { MusicDb = musicDb; AmbienceDb = ambienceDb; EffectsDb = effectsDb; }

    public static readonly DuckRequest Dialogue = new DuckRequest(-6, -4);
    public static readonly DuckRequest Listening = new DuckRequest(-9, -6, -2);
    public static readonly DuckRequest Stinger = new DuckRequest(-8, -2);
}

// Picks numbered variants without repeating the last one, and refuses a cue that already plays
// too often (same id within minInterval, or more than maxVoices copies at once).
public sealed class CueLimiter
{
    private readonly Dictionary<string, float> _last = new Dictionary<string, float>();
    private readonly Dictionary<string, int> _lastVariant = new Dictionary<string, int>();
    private readonly Random _random;

    public CueLimiter(int seed = 1977) { _random = new Random(seed); }

    public bool Allow(string id, float now, float minInterval, int playing, int maxVoices)
    {
        if (playing >= maxVoices) return false;
        if (_last.TryGetValue(id, out var last) && now - last < minInterval && now >= last) return false;
        _last[id] = now;
        return true;
    }

    public int NextVariant(string id, int count)
    {
        if (count <= 1) return 0;
        int previous = _lastVariant.TryGetValue(id, out var p) && p < count ? p : -1;
        int next = previous < 0 ? _random.Next(count) : _random.Next(count - 1);
        if (previous >= 0 && next >= previous) next++;
        _lastVariant[id] = next;
        return next;
    }

    public float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

    public void Clear() { _last.Clear(); _lastVariant.Clear(); }
}

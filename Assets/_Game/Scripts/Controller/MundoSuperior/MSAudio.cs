using UnityEngine;

// Sound bank of the Mundo Superior (Resources/MSAudio, built by
// tools/audio/generate_mundo_superior_audio.py and revised by tools/audio/generate_game_audio.py;
// guide 16.1 S01-S23). Since the audio revision it is a facade over GameAudio: channels come
// from the game settings (music has its own slider now), the pause is handled by GameAudioHost
// (continuous action sounds stop, music and wind keep a softer, darker mix). Every essential cue
// is also shown on screen (guide 16.2): sounds complement the level and it plays fine without them.
public static class MSAudio
{
    public enum Channel { Effects, Ambience, Music, Interface }

    public const string Folder = "MSAudio/";

    private static AudioChannel Map(Channel channel)
    {
        switch (channel)
        {
            case Channel.Ambience: return AudioChannel.Ambience;
            case Channel.Music: return AudioChannel.Music;
            case Channel.Interface: return AudioChannel.Interface;
            default: return AudioChannel.Effects;
        }
    }

    public static void ReadSettings() => GameAudio.ReadSettings();

    public static float Volume(Channel channel) => GameAudio.Volume(Map(channel));

    public static AudioClip Clip(string name) => GameAudio.Clip(Folder + name);

    // Non-spatial cue (finds, wings, portals, interface). Interface cues ignore the pause.
    public static void Play(string name, float volume = 1f, float pitch = 1f, Channel channel = Channel.Effects)
    {
        if (name == "victoria" || name == "derrota" || name == "hallazgo_confirmar") { GameAudio.Stinger(Folder + name, volume); return; }
        GameAudio.Play(Folder + name, volume, Map(channel), pitch);
    }

    // One of the numbered variants (paso_piedra_1..4) with a small pitch spread.
    public static void PlayVariant(string prefix, int count, float volume = 1f, float pitchSpread = .06f)
        => GameAudio.Play(Folder + prefix, volume, AudioChannel.Effects, 1f, pitchSpread);

    // World cue at a position (lock, transport stops).
    public static void PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f, float maxDistance = 30f)
        => GameAudio.PlayAt(Folder + name, position, volume, AudioChannel.Effects, maxDistance, pitch);

    // S23: a soft "not available" (rate-limited by the mixer however often E is pressed).
    public static void Unavailable() => GameAudio.UI(UICue.Blocked);

    // Looping source owned by an object (wind, music layers, flight, portal hum, transport).
    public static AudioSource Loop(GameObject owner, string name, float volume, bool spatial, Channel channel, float maxDistance = 25f)
        => GameAudio.Loop(owner, Folder + name, volume, spatial, Map(channel), maxDistance);

    public static void SetLoopVolume(AudioSource source, float volume) => GameAudio.SetLoopVolume(source, volume);

    // Guide 16.2: kept for the director; GameAudioHost already pauses action sounds with the game.
    public static void PauseActions(bool paused) { }
}

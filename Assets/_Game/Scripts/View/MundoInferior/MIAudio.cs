using UnityEngine;

// Sound bank of the Mundo Inferior (Resources/MIAudio, built by
// tools/audio/generate_mundo_inferior_audio.py and revised by tools/audio/generate_game_audio.py).
// Since the audio revision it is a facade over GameAudio: the cues follow the game's channels
// (effects, ambience, music, interface), pool, vary and pause like every other sound. Every
// essential cue is also shown on screen (guide 7.3); the level plays fine without them.
public static class MIAudio
{
    public const string Folder = "MIAudio/";
    // Extra trim of this bank (kept from the first mix).
    public static float Volume = 0.8f;

    public static AudioClip Clip(string name) => GameAudio.Clip(Folder + name);

    // Phrases (finds confirmed, the guardian freed, defeat) dip the music; interface cues ignore the pause.
    public static void Play(string name, float volume = 1f, float pitch = 1f)
    {
        if (IsPhrase(name)) { GameAudio.Stinger(Folder + name, volume * Volume); return; }
        GameAudio.Play(Folder + name, volume * Volume, ChannelOf(name), pitch, name.StartsWith("ui_") ? .02f : .03f);
    }

    public static void PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f)
        => GameAudio.PlayAt(Folder + name, position, volume * Volume, ChannelOf(name), 30f, pitch);

    public static AudioSource Loop(GameObject owner, string name, float volume, bool spatial, float maxDistance = 25f)
        => GameAudio.Loop(owner, Folder + name, volume * Volume, spatial, ChannelOf(name), maxDistance);

    private static bool IsPhrase(string name) => name == "victoria" || name == "derrota" || name == "hallazgo_confirmar";

    private static AudioChannel ChannelOf(string name)
    {
        if (name.StartsWith("ui_")) return AudioChannel.Interface;
        if (name.StartsWith("ambiente") || name == "gota") return AudioChannel.Ambience;
        if (name.StartsWith("musica")) return AudioChannel.Music;
        return AudioChannel.Effects;
    }
}

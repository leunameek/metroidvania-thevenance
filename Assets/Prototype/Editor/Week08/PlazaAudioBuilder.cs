using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Original synthesized Foley and tonal cues. Deterministic, no external samples or downloads.
public static class PlazaAudioBuilder
{
    private const string Folder = "Assets/Art/Audio/PlazaNunez";
    private const int Rate = 44100;
    public static void Build(PlazaAudio target)
    {
        Directory.CreateDirectory(Folder);
        var serialized = new SerializedObject(target);
        var cues = serialized.FindProperty("cues");
        string[] names = Enum.GetNames(typeof(PlazaSound));
        cues.arraySize = names.Length;
        for (int i = 0; i < names.Length; i++)
        {
            int sound = i;
            float duration = i == 11 ? 1.7f : i == 12 ? 1.6f : i == 6 ? 0.9f : i == 9 ? 0.6f : i == 10 ? 0.5f : 0.32f;
            cues.GetArrayElementAtIndex(i).objectReferenceValue = Write(names[i], duration, false, (t, n) => Cue(sound, t, duration, n));
        }
        serialized.FindProperty("plazaAmbience").objectReferenceValue = Write("Ambience_Plaza", 16, true, (t, n) => Ambience(t, n, 0));
        serialized.FindProperty("lowerAmbience").objectReferenceValue = Write("Ambience_Inferior", 16, true, (t, n) => Ambience(t, n, -1));
        serialized.FindProperty("upperAmbience").objectReferenceValue = Write("Ambience_Superior", 16, true, (t, n) => Ambience(t, n, 1));
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
    private static float Sin(float frequency, float t) => Mathf.Sin(2 * Mathf.PI * frequency * t);
    private static float Cue(int sound, float t, float duration, float noise)
    {
        float u = t / duration;
        float env = Mathf.Min(1, t * 170) * Mathf.Pow(1 - u, 2);
        switch ((PlazaSound)sound)
        {
            case PlazaSound.Step: return env * (noise * 0.23f * Mathf.Exp(-t * 30) + Sin(95 - t * 80, t) * 0.25f * Mathf.Exp(-t * 24));
            case PlazaSound.Land: return env * (noise * 0.22f + Sin(72, t) * 0.3f) * Mathf.Exp(-t * 13);
            case PlazaSound.Jump: return env * (noise * 0.12f + Sin(220 + 260 * t, t) * 0.12f);
            case PlazaSound.Dash: return Mathf.Sin(u * Mathf.PI) * (noise * 0.38f + Sin(440 - 290 * u, t) * 0.08f);
            case PlazaSound.Inspect: return env * (Sin(587, t) * 0.25f + Sin(880, t) * 0.12f);
            case PlazaSound.Rotate: return env * (Sin(740, t) * 0.06f + noise * 0.03f);
            case PlazaSound.Complete: return env * (Sin(523, t) + Sin(659, t) * 0.7f + Sin(784, t) * 0.5f) * 0.2f;
            case PlazaSound.Attack: return env * (noise * 0.42f + Sin(190 - 130 * u, t) * 0.14f);
            case PlazaSound.Impact: return env * Mathf.Exp(-t * 8) * (noise * 0.35f + Sin(62, t) * 0.42f);
            case PlazaSound.Guard: return env * (Sin(395, t) + Sin(632, t) * 0.5f + noise * 0.18f) * 0.35f;
            case PlazaSound.Warning: return env * Sin(185, t) * (0.22f + 0.12f * Sin(7, t));
            case PlazaSound.Portal: return Mathf.Pow(Mathf.Sin(u * Mathf.PI), 1.5f) * (noise * 0.11f + Sin(110 + 100 * u, t) * 0.18f + Sin(330 + 280 * u, t) * 0.12f);
            default:
                float a = Mathf.Max(0, t - 0.18f), b = Mathf.Max(0, t - 0.36f);
                return env * (Sin(392, t) * Mathf.Exp(-t * 2) + Sin(494, a) * Mathf.Exp(-a * 2) + Sin(587, b) * Mathf.Exp(-b * 2)) * 0.23f;
        }
    }
    private static float Ambience(float t, float noise, int world)
    {
        // All carriers are integer cycles over 16 seconds; noise uses a seam envelope below.
        float wind = noise * (0.065f + 0.025f * Sin(0.125f, t));
        if (world < 0) return wind + Sin(55, t) * 0.13f + Sin(82.5f, t) * 0.055f + Sin(110, t) * 0.035f;
        if (world > 0) return wind * 0.45f + (Sin(220, t) + Sin(330, t) * 0.5f + Sin(440, t) * 0.2f) * 0.055f;
        float fountain = noise * (0.09f + 0.025f * Sin(0.5f, t));
        float birdTime = t % 4;
        float bird = birdTime < 0.38f ? Sin(1700 + 600 * Mathf.Sin(birdTime * 8), birdTime) * Mathf.Sin(birdTime / 0.38f * Mathf.PI) * 0.035f : 0;
        return wind + fountain + bird + Sin(146.8125f, t) * 0.012f;
    }
    private static AudioClip Write(string name, float seconds, bool loop, Func<float, float, float> sample)
    {
        string path = Folder + "/" + name + ".wav";
        int count = Mathf.RoundToInt(seconds * Rate);
        var random = new System.Random(4721);
        float filtered = 0;
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)1); writer.Write(Rate); writer.Write(Rate * 2);
            writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                filtered = Mathf.Lerp(filtered, (float)random.NextDouble() * 2 - 1, 0.23f);
                float value = sample(t, filtered);
                if (loop) value *= Mathf.Min(1, Mathf.Min(t, seconds - t) / 0.12f);
                writer.Write((short)(Mathf.Clamp(value, -0.95f, 0.95f) * 32767));
            }
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = loop ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.75f;
        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}

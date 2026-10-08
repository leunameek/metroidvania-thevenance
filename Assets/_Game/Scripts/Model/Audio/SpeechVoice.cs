using System;
using System.Collections.Generic;

// How the line is said, read from its punctuation: it picks the shape of the voiced sound.
public enum SpeechMood { Calm, Question, Exclaim, Trail }

// One character's timbre (2026-10-08 playtest: «que se sienta que habla alguien», Zelda style):
// no spoken words, one short voiced interjection at the start of each line — «hm», «¡ha!»,
// «¿eh?», «mmm…» — in a voice of its own. Pitch in Hz; formant 1 = an adult human throat
// (lower is a bigger body, higher a smaller one); breath and growl roughen it for spirits and creatures.
public sealed class SpeechVoice
{
    public readonly float Pitch, Formant, Breath, Growl, Vibrato, Pace;

    public SpeechVoice(float pitch, float formant, float breath = .06f, float growl = 0f, float vibrato = 0f, float pace = 1f)
    {
        Pitch = pitch; Formant = formant; Breath = breath; Growl = growl; Vibrato = vibrato; Pace = pace;
    }
}

public static class SpeechVoices
{
    public const int SampleRate = 22050;
    public const int Variants = 3;

    // Every speaker of historia.json and of the world encounters; anyone else gets a voice made
    // from the name, so no two characters ever share one by accident.
    private static readonly Dictionary<string, SpeechVoice> Cast = new Dictionary<string, SpeechVoice>(StringComparer.OrdinalIgnoreCase)
    {
        { "Nemequene", new SpeechVoice(132, 1.00f, .07f) },
        { "Bachué", new SpeechVoice(215, 1.13f, .16f, 0, .35f, .9f) },
        { "Tisquesusa", new SpeechVoice(104, .93f, .06f, .05f, 0, .9f) },
        { "Saguanmachica", new SpeechVoice(92, .89f, .10f, .12f, .15f, .85f) },
        { "Quimue", new SpeechVoice(84, .9f, .14f, .28f, .1f, .95f) },
        { "Furachogua", new SpeechVoice(238, 1.17f, .09f, 0, .15f, 1.1f) },
        { "Custodio de Raíces", new SpeechVoice(74, .82f, .32f, .15f, .45f, .75f) },
        { "Custodio", new SpeechVoice(78, .84f, .30f, .12f, .4f, .75f) },
        { "Guardián de entrenamiento", new SpeechVoice(112, .95f, .08f, .10f, 0, 1f) },
        { "Guardián caimán-murciélago", new SpeechVoice(70, .80f, .22f, .45f, 0, .9f) },
        { "Hombre-caimán", new SpeechVoice(80, .82f, .14f, .40f, 0, .85f) },
        { "Hombre-murciélago", new SpeechVoice(178, 1.08f, .30f, .10f, .2f, 1.3f) },
        { "Serpiente", new SpeechVoice(66, .80f, .60f, .30f, .25f, .7f) },
        { "Mujer-cóndor", new SpeechVoice(205, 1.06f, .20f, .05f, .1f, 1f) },
        { "Mujer-águila", new SpeechVoice(262, 1.20f, .10f, 0, .2f, 1.15f) },
        { "Cóndor", new SpeechVoice(320, 1.25f, .35f, .20f, 0, 1.3f) },
        { "Águila", new SpeechVoice(390, 1.32f, .25f, .10f, .1f, 1.4f) },
    };

    // The narrator («Ayuda», a help card) has no voice.
    public static bool Silent(string speaker) => string.IsNullOrEmpty(speaker) || speaker.Equals("Ayuda", StringComparison.OrdinalIgnoreCase);

    public static SpeechVoice For(string speaker)
    {
        if (Silent(speaker)) return null;
        if (Cast.TryGetValue(speaker.Trim(), out var voice)) return voice;
        uint h = 2166136261;
        foreach (char c in speaker) h = (h ^ c) * 16777619;
        float t = (h % 1000) / 1000f, u = (h / 1000 % 1000) / 1000f;
        return new SpeechVoice(100 + t * 150, .9f + u * .25f, .08f + u * .1f);
    }

    public static IEnumerable<string> Speakers => Cast.Keys;

    public static SpeechMood Mood(string text)
    {
        if (string.IsNullOrEmpty(text)) return SpeechMood.Calm;
        string s = text.Trim().TrimEnd('»', '"', '”', ')', ' ');
        if (s.EndsWith("...") || s.EndsWith("…")) return SpeechMood.Trail;
        if (s.EndsWith("?") || s.StartsWith("¿")) return SpeechMood.Question;
        if (s.EndsWith("!") || s.StartsWith("¡")) return SpeechMood.Exclaim;
        return SpeechMood.Calm;
    }

    // Vowel formants (Hz) of an adult voice: a, o, e, u — and the closed hum of «m».
    private static readonly float[][] Vowels = { new[] { 730f, 1090f, 2440f }, new[] { 570f, 840f, 2410f }, new[] { 530f, 1840f, 2480f }, new[] { 330f, 870f, 2240f } };
    private static readonly float[] Hum = { 260f, 1700f, 2500f };

    // One interjection as samples in [-1, 1]. variant picks the vowel and how many syllables, so
    // the lines of one conversation do not all sound alike.
    public static float[] Render(SpeechVoice v, SpeechMood mood, int variant)
    {
        variant = ((variant % Variants) + Variants) % Variants;
        // Shape per mood: syllables, length of each (s), pitch at start and end (× base), hum share.
        int syllables; float length, from, to, hum; int vowel;
        switch (mood)
        {
            case SpeechMood.Question: syllables = 1; length = .30f; from = .95f; to = 1.45f; hum = .25f; vowel = variant == 0 ? 2 : variant == 1 ? 0 : 3; break;
            case SpeechMood.Exclaim: syllables = variant == 2 ? 1 : 2; length = .15f; from = 1.35f; to = 1.05f; hum = .12f; vowel = variant == 1 ? 1 : 0; break;
            case SpeechMood.Trail: syllables = 1; length = .55f; from = 1.05f; to = .78f; hum = .65f; vowel = 3; break;
            default: syllables = variant == 1 ? 2 : 1; length = variant == 1 ? .16f : .24f; from = 1.08f; to = .9f; hum = .45f; vowel = variant == 2 ? 1 : 0; break;
        }
        length /= v.Pace;
        float gap = .045f / v.Pace;
        int perSyllable = (int)(length * SampleRate), gapSamples = (int)(gap * SampleRate);
        var output = new float[syllables * perSyllable + (syllables - 1) * gapSamples + SampleRate / 20];
        var rng = new Random(variant * 7919 + (int)mood * 104729 + (int)(v.Pitch * 13));
        int at = 0;
        for (int s = 0; s < syllables; s++)
        {
            // A second syllable answers the first a little lower («ha-ha», «hm-hm»).
            float drop = s == 0 ? 1f : .9f;
            Syllable(output, at, perSyllable, v, from * drop, to * drop, hum, Vowels[s == 1 && vowel == 0 ? 1 : vowel], rng);
            at += perSyllable + gapSamples;
        }
        // Same loudness for a deep and a high voice (by energy), never clipping.
        float peak = 0; double energy = 0;
        foreach (var x in output) { peak = Math.Max(peak, Math.Abs(x)); energy += x * x; }
        float rms = (float)Math.Sqrt(energy / output.Length);
        float gain = peak > 0 ? Math.Min(.9f / peak, .2f / Math.Max(rms, 1e-6f)) : 0;
        for (int i = 0; i < output.Length; i++) output[i] *= gain;
        return output;
    }

    private static void Syllable(float[] buffer, int start, int count, SpeechVoice v, float from, float to, float humShare, float[] vowel, Random rng)
    {
        var bands = new Resonator[3];
        for (int b = 0; b < 3; b++) bands[b] = new Resonator();
        float phase = 0, sub = 0, previous = 0;
        double[] gains = { 1, .8, .55 };
        for (int i = 0; i < count && start + i < buffer.Length; i++)
        {
            float t = i / (float)count;
            // Pitch glides along the mood's contour with a slight wobble.
            float contour = from + (to - from) * (t * t * (3 - 2 * t));
            float f0 = v.Pitch * contour * (1 + v.Vibrato * .035f * (float)Math.Sin(2 * Math.PI * 5.5 * i / SampleRate));
            phase += f0 / SampleRate;
            if (phase >= 1) { phase -= 1; sub = sub > 0 ? -1 : 1; }
            // Glottal pulse: a soft rising-falling shape every period (a raw saw would buzz).
            float glottal = phase < .6f ? (float)(.5 - .5 * Math.Cos(Math.PI * phase / .6f)) : (float)Math.Cos(Math.PI * (phase - .6f) / .8f);
            // The throat is driven by the change of the airflow: the sharp closing of each pulse
            // carries the bright harmonics a smooth pulse would lack (the voice sounded muffled).
            float flow = glottal; glottal = (flow - previous) * SampleRate / (f0 * 3f); previous = flow;
            // Growl: every other period louder (creatures, the old and the angry).
            glottal *= 1 + v.Growl * sub * .8f;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            float source = glottal * (1 - v.Breath) + noise * v.Breath * .9f;
            // From the closed «m» to the open vowel.
            float open = humShare <= 0 ? 1 : Clamp01((t - humShare * .6f) / Math.Max(.01f, humShare * .5f));
            double sum = 0;
            for (int b = 0; b < 3; b++)
            {
                float freq = (Hum[b] + (vowel[b] - Hum[b]) * open) * v.Formant;
                float width = 60 + 40 * b + v.Breath * 120;
                sum += bands[b].Step(source, freq, width) * gains[b] * (b == 0 ? 1 : .35 + .65 * open);
            }
            // Envelope: quick onset, a held middle, a soft end.
            float env = Math.Min(1f, t / .08f) * Math.Min(1f, (1 - t) / .3f);
            buffer[start + i] += (float)sum * env * (.55f + .45f * open);
        }
    }

    private static float Clamp01(float x) => x < 0 ? 0 : x > 1 ? 1 : x;

    // Two-pole resonant band-pass (one vocal-tract formant), zeros at DC and Nyquist so the
    // sound stays centred, unity gain at its peak.
    private sealed class Resonator
    {
        private double _x1, _x2, _y1, _y2;
        public double Step(float x, float freq, float bandwidth)
        {
            double r = Math.Exp(-Math.PI * bandwidth / SampleRate);
            double c = 2 * r * Math.Cos(2 * Math.PI * freq / SampleRate);
            double y = (1 - r * r) * .5 * (x - _x2) + c * _y1 - r * r * _y2;
            _x2 = _x1; _x1 = x; _y2 = _y1; _y1 = y;
            return y;
        }
    }
}

using System;
using System.Collections.Generic;

// How the line is said, read from its punctuation: it picks the shape of the voiced sound.
public enum SpeechMood { Calm, Question, Exclaim, Trail }

// One character's timbre (2026-10-08 playtest: «que se sienta que habla alguien», Zelda style):
// no spoken words, a short babbled phrase at the start of each line — longer for a long
// line, rising for a question, high for a cry, fading for «…» — in a voice of its own. Pitch in Hz; formant 1 = an adult human throat
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

    // Vowel formants (Hz) of an adult voice: a, o, e, u, i — pulled toward the neutral «schwa»
    // when spoken, so the babble never forms clear vowels (2026-10-08: «ia io» was too readable).
    private static readonly float[][] Vowels = { new[] { 730f, 1090f, 2440f }, new[] { 570f, 840f, 2410f }, new[] { 530f, 1840f, 2480f }, new[] { 330f, 870f, 2240f }, new[] { 300f, 2200f, 2900f } };
    private static readonly float[] Schwa = { 500f, 1500f, 2500f };
    private static readonly float[] Hum = { 260f, 1500f, 2500f };
    public const float Blur = .55f;

    // How many syllables a line gets: longer lines babble longer.
    public static int SyllablesFor(string text, SpeechMood mood)
    {
        int length = string.IsNullOrEmpty(text) ? 0 : text.Length;
        int n = 3 + length / 28;
        switch (mood)
        {
            case SpeechMood.Exclaim: return Math.Max(2, Math.Min(5, n - 1));
            case SpeechMood.Trail: return Math.Max(4, Math.Min(8, n + 1));
            default: return Math.Max(3, Math.Min(7, n));
        }
    }

    private enum Onset { Hum, Breath, Tap, Glide }

    // A babbled phrase as samples in [-1, 1]: one continuous voice going through blurred
    // syllables (m-, h-, soft taps), its pitch drawn by the mood. variant reshuffles the
    // syllables so the lines of one conversation do not all sound alike.
    public static float[] Render(SpeechVoice v, SpeechMood mood, int variant, int syllables = 0)
    {
        variant = ((variant % Variants) + Variants) % Variants;
        if (syllables <= 0) syllables = SyllablesFor(null, mood);
        var rng = new Random(variant * 7919 + (int)mood * 104729 + (int)(v.Pitch * 13) + syllables * 31);
        // Pitch at start and end of the phrase (× base); the last syllable carries the mood.
        float from, to, finalBend, beat;
        switch (mood)
        {
            case SpeechMood.Question: from = 1.0f; to = .95f; finalBend = 1.45f; beat = .13f; break;
            case SpeechMood.Exclaim: from = 1.35f; to = 1.1f; finalBend = .92f; beat = .11f; break;
            case SpeechMood.Trail: from = 1.05f; to = .85f; finalBend = .85f; beat = .17f; break;
            default: from = 1.1f; to = .9f; finalBend = .95f; beat = .135f; break;
        }
        beat = Math.Min(beat / v.Pace, .2f); // slow voices drawl, but a phrase stays under ~2 s
        var starts = new int[syllables + 1];
        var lengths = new int[syllables];
        var onsets = new Onset[syllables];
        var vowels = new int[syllables];
        var accents = new float[syllables];
        int at = 0;
        for (int k = 0; k < syllables; k++)
        {
            float stretch = .75f + (float)rng.NextDouble() * .5f;
            if (k == syllables - 1) stretch *= mood == SpeechMood.Trail ? 2.2f : mood == SpeechMood.Question ? 1.7f : 1.4f;
            lengths[k] = (int)(beat * stretch * SampleRate);
            onsets[k] = k == 0 ? (rng.NextDouble() < .5 ? Onset.Hum : Onset.Breath) : (Onset)rng.Next(4);
            int vowel; do vowel = rng.Next(Vowels.Length); while (k > 0 && vowel == vowels[k - 1]);
            vowels[k] = vowel;
            accents[k] = 1 + ((float)rng.NextDouble() - .5f) * .14f;
            starts[k] = at;
            // Now and then a short breath between words.
            at += lengths[k] + (k < syllables - 1 && rng.NextDouble() < .22 ? (int)(.06f / v.Pace * SampleRate) : 0);
        }
        starts[syllables] = at;
        var output = new float[at + SampleRate / 20];

        var bands = new Resonator[3];
        for (int b = 0; b < 3; b++) bands[b] = new Resonator();
        double[] gains = { 1, .7, .35 };
        float phase = 0, sub = 0, previous = 0, level = 0;
        float[] formant = { Hum[0], Hum[1], Hum[2] };
        float[] target = new float[3];
        float glide = 1 - (float)Math.Exp(-1.0 / (.018 * SampleRate));
        float follow = 1 - (float)Math.Exp(-1.0 / (.008 * SampleRate));
        int syllable = 0;
        for (int i = 0; i < at; i++)
        {
            while (syllable < syllables - 1 && i >= starts[syllable + 1]) syllable++;
            int local = i - starts[syllable];
            float t = Clamp01(local / (float)lengths[syllable]);
            bool voiced = local < lengths[syllable];
            float phrase = i / (float)at;
            // Pitch: the phrase falls (or rises) as a whole, each syllable has its own accent,
            // and the last one bends with the mood (up for a question).
            float contour = (from + (to - from) * phrase) * accents[syllable];
            if (syllable == syllables - 1) contour *= 1 + (finalBend - 1) * (t * t * (3 - 2 * t));
            float f0 = v.Pitch * contour * (1 + v.Vibrato * .035f * (float)Math.Sin(2 * Math.PI * 5.5 * i / SampleRate));
            phase += f0 / SampleRate;
            if (phase >= 1) { phase -= 1; sub = sub > 0 ? -1 : 1; }
            float glottal = phase < .6f ? (float)(.5 - .5 * Math.Cos(Math.PI * phase / .6f)) : (float)Math.Cos(Math.PI * (phase - .6f) / .8f);
            // Driven by the change of the airflow: the sharp closing carries the bright harmonics.
            float flow = glottal; glottal = (flow - previous) * SampleRate / (f0 * 3f); previous = flow;
            glottal *= 1 + v.Growl * sub * .8f;
            float noise = (float)(rng.NextDouble() * 2 - 1);

            // The consonant at the head of the syllable, then the blurred vowel.
            var onset = onsets[syllable];
            float head = onset == Onset.Glide ? .12f : onset == Onset.Tap ? .18f : .3f;
            float open = Clamp01((t - head * .5f) / (head * .6f));
            float breath = v.Breath, voicing = 1;
            var vowel = Vowels[vowels[syllable]];
            for (int b = 0; b < 3; b++)
            {
                float blurred = vowel[b] + (Schwa[b] - vowel[b]) * Blur;
                target[b] = onset == Onset.Hum ? Hum[b] + (blurred - Hum[b]) * open : blurred;
            }
            if (onset == Onset.Breath) { breath = Math.Max(breath, .7f * (1 - open)); voicing = .25f + .75f * open; }
            if (onset == Onset.Tap && t < head * .4f) voicing = .05f; // a soft closure, then release
            // Formants glide instead of jumping: the mouth never quite lands on a vowel.
            for (int b = 0; b < 3; b++) formant[b] += (target[b] - formant[b]) * glide;
            float source = glottal * (1 - breath) * voicing + noise * breath * .9f;
            double sum = 0;
            for (int b = 0; b < 3; b++)
            {
                float width = 80 + 50 * b + v.Breath * 120;
                sum += bands[b].Step(source, formant[b] * v.Formant, width) * gains[b] * (b == 0 ? 1 : .4 + .6 * open);
            }
            // Loudness: each syllable swells and dips a little into the next; silence in the
            // breaths between words; the phrase fades at its end.
            float swell = voiced ? (float)Math.Sin(Math.PI * Math.Min(1, t * 1.15f)) * .55f + .45f : 0;
            if (syllable == 0) swell *= Math.Min(1f, local / (.03f * SampleRate));
            if (syllable == syllables - 1) swell *= Math.Min(1f, (1 - t) / .35f);
            level += (swell - level) * follow;
            output[i] = (float)sum * level * (.6f + .4f * open);
        }
        // Same loudness for a deep and a high voice (by energy), never clipping.
        float peak = 0; double energy = 0;
        foreach (var x in output) { peak = Math.Max(peak, Math.Abs(x)); energy += x * x; }
        float rms = (float)Math.Sqrt(energy / output.Length);
        float gain = peak > 0 ? Math.Min(.9f / peak, .2f / Math.Max(rms, 1e-6f)) : 0;
        for (int i = 0; i < output.Length; i++) output[i] *= gain;
        return output;
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

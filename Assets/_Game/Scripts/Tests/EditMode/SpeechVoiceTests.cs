using System.Collections.Generic;
using NUnit.Framework;

// Story voices (2026-10-08): every character opens its lines with a voiced sound of its own.
public class SpeechVoiceTests
{
    // Every speaker of historia.json and of the upper-world encounters.
    private static readonly string[] Speakers =
    {
        "Nemequene", "Bachué", "Tisquesusa", "Saguanmachica", "Quimue", "Furachogua", "Custodio de Raíces", "Custodio",
        "Guardián de entrenamiento", "Guardián caimán-murciélago", "Hombre-caimán", "Hombre-murciélago", "Serpiente",
        "Mujer-cóndor", "Mujer-águila", "Cóndor", "Águila",
    };

    [Test]
    public void EveryCharacterHasAVoiceOfItsOwn()
    {
        var seen = new HashSet<string>();
        foreach (var name in Speakers)
        {
            var v = SpeechVoices.For(name);
            Assert.IsNotNull(v, name);
            Assert.IsTrue(seen.Add(v.Pitch + "/" + v.Formant + "/" + v.Breath + "/" + v.Growl), name + " shares a voice");
        }
    }

    [Test]
    public void TheHelpNarratorIsSilent()
    {
        Assert.IsNull(SpeechVoices.For("Ayuda"));
        Assert.IsNull(SpeechVoices.For(""));
        Assert.IsNotNull(SpeechVoices.For("Alguien nuevo"));
    }

    [TestCase("¿Quién eres?", SpeechMood.Question)]
    [TestCase("¡Cuidado!", SpeechMood.Exclaim)]
    [TestCase("No sé...", SpeechMood.Trail)]
    [TestCase("Ya llegó el día…", SpeechMood.Trail)]
    [TestCase("La plaza espera.", SpeechMood.Calm)]
    public void TheMoodComesFromThePunctuation(string text, SpeechMood mood) => Assert.AreEqual(mood, SpeechVoices.Mood(text));

    // A long line babbles longer than a short one, within a few syllables.
    [Test]
    public void LongerLinesBabbleLonger()
    {
        Assert.Less(SpeechVoices.SyllablesFor("Sí.", SpeechMood.Calm), SpeechVoices.SyllablesFor(new string('a', 140), SpeechMood.Calm));
        Assert.LessOrEqual(SpeechVoices.SyllablesFor(new string('a', 900), SpeechMood.Calm), 7);
        var v = SpeechVoices.For("Nemequene");
        Assert.Less(SpeechVoices.Render(v, SpeechMood.Calm, 0, 3).Length, SpeechVoices.Render(v, SpeechMood.Calm, 0, 7).Length);
    }

    [Test]
    public void EverySoundIsShortAndClean()
    {
        foreach (var name in Speakers)
            foreach (SpeechMood mood in System.Enum.GetValues(typeof(SpeechMood)))
                for (int variant = 0; variant < SpeechVoices.Variants; variant++)
                {
                    var samples = SpeechVoices.Render(SpeechVoices.For(name), mood, variant, 8);
                    Assert.That(samples.Length, Is.InRange(SpeechVoices.SampleRate / 4, SpeechVoices.SampleRate * 5 / 2), name + " " + mood);
                    float peak = 0;
                    foreach (var x in samples) { Assert.IsFalse(float.IsNaN(x), name); peak = System.Math.Max(peak, System.Math.Abs(x)); }
                    Assert.That(peak, Is.InRange(.05f, .91f), name + " " + mood);
                }
    }
}

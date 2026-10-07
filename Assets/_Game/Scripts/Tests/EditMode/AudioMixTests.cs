using System.Collections.Generic;
using NUnit.Framework;

public class AudioMixTests
{
    [Test]
    public void ChannelsFollowTheirOwnSliders()
    {
        var s = new AudioMixSettings { effects = .5f, ambience = .5f, music = .25f, voices = .4f, uiVolume = .3f };
        Assert.AreEqual(.5f, AudioMix.Gain(s, AudioChannel.Effects), 1e-4);
        Assert.AreEqual(.7f, AudioMix.Gain(s, AudioChannel.Ambience), 1e-4);
        Assert.AreEqual(.25f, AudioMix.Gain(s, AudioChannel.Music), 1e-4);
        Assert.AreEqual(.4f, AudioMix.Gain(s, AudioChannel.Voice), 1e-4);
        Assert.AreEqual(.6f, AudioMix.Gain(s, AudioChannel.Interface), 1e-4);
        s.music = 0;
        Assert.AreEqual(0f, AudioMix.Gain(s, AudioChannel.Music));
        Assert.AreEqual(.5f, AudioMix.Gain(s, AudioChannel.Effects), 1e-4, "muting music leaves the effects alone");
    }

    [Test]
    public void TheStrongestDuckWinsAndTheyDoNotAddUp()
    {
        var requests = new List<DuckRequest> { DuckRequest.Dialogue, DuckRequest.Listening };
        Assert.AreEqual(DuckRequest.Listening.MusicDb, AudioMix.DuckDb(AudioChannel.Music, requests));
        Assert.AreEqual(DuckRequest.Listening.AmbienceDb, AudioMix.DuckDb(AudioChannel.Ambience, requests));
        Assert.AreEqual(0f, AudioMix.DuckDb(AudioChannel.Interface, requests), "interface never ducks");
        Assert.AreEqual(0f, AudioMix.DuckDb(AudioChannel.Music, new List<DuckRequest>()));
    }

    [Test]
    public void ListeningLowersMusicMoreThanDialogue()
    {
        Assert.Less(DuckRequest.Listening.MusicDb, DuckRequest.Dialogue.MusicDb);
        Assert.Less(AudioMix.DbToGain(DuckRequest.Listening.MusicDb), .4f, "about -9 dB under the voice window");
    }

    [Test]
    public void PauseStopsActionsButKeepsInterfaceAndSoftensBeds()
    {
        Assert.IsTrue(AudioMix.PausesWithGame(AudioChannel.Effects));
        Assert.IsTrue(AudioMix.PausesWithGame(AudioChannel.Voice));
        Assert.IsFalse(AudioMix.PausesWithGame(AudioChannel.Interface));
        Assert.IsFalse(AudioMix.PausesWithGame(AudioChannel.Music));
        Assert.Less(AudioMix.PauseGain(AudioChannel.Music, true), 1f);
        Assert.AreEqual(1f, AudioMix.PauseGain(AudioChannel.Music, false));
        Assert.AreEqual(1f, AudioMix.PauseGain(AudioChannel.Interface, true));
    }

    [Test]
    public void TheSameCueCannotPileUp()
    {
        var limiter = new CueLimiter();
        Assert.IsTrue(limiter.Allow("paso", 1f, .1f, 0, 2));
        Assert.IsFalse(limiter.Allow("paso", 1.05f, .1f, 0, 2), "too soon");
        Assert.IsTrue(limiter.Allow("paso", 1.2f, .1f, 1, 2));
        Assert.IsFalse(limiter.Allow("paso", 2f, .1f, 2, 2), "already two copies playing");
        Assert.IsTrue(limiter.Allow("otro", 1.06f, .1f, 0, 2), "limits are per cue");
    }

    [Test]
    public void VariantsNeverRepeatBackToBack()
    {
        var limiter = new CueLimiter(7);
        int previous = limiter.NextVariant("paso", 4);
        var seen = new HashSet<int> { previous };
        for (int i = 0; i < 200; i++)
        {
            int next = limiter.NextVariant("paso", 4);
            Assert.AreNotEqual(previous, next);
            Assert.That(next, Is.InRange(0, 3));
            seen.Add(next); previous = next;
        }
        Assert.AreEqual(4, seen.Count, "every variant is used");
        Assert.AreEqual(0, limiter.NextVariant("unico", 1));
    }
}

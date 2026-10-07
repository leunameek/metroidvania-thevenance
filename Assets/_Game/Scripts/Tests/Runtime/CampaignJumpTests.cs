using NUnit.Framework;

public sealed class CampaignJumpTests
{
    [SetUp]
    public void NoSlot() => WorldTravel.SaveSlot = -1;

    [Test]
    public void EveryChapterStartsWhereItsButtonSays()
    {
        foreach (var entry in CampaignJump.Entries)
        {
            if (entry.chapter == CampaignChapter.Complete) continue;
            CampaignJump.Prepare(entry.chapter);
            Assert.AreEqual(entry.chapter, CampaignProgress.Chapter, entry.title);
        }
    }

    [Test]
    public void JumpingBackForgetsLaterProgress()
    {
        CampaignJump.Prepare(CampaignChapter.SolarReturn);
        Assert.IsTrue(MSProgress.Has(MSProgress.Sue));
        CampaignJump.Prepare(CampaignChapter.LowerWorld);
        Assert.AreEqual(CampaignChapter.LowerWorld, CampaignProgress.Chapter);
        Assert.IsFalse(MIProgress.Has(MIProgress.Seed));
        Assert.IsFalse(MSProgress.Has(MSProgress.Sue));
        Assert.IsTrue(CampaignProgress.Has(CampaignFlags.LessonsComplete));
    }

    [Test]
    public void EarlierDialoguesCountAsHeard()
    {
        CampaignJump.Prepare(CampaignChapter.Urn);
        Assert.IsTrue(CampaignProgress.Seen("H04"));
        Assert.IsTrue(CampaignProgress.Seen("H11"));
        Assert.IsFalse(CampaignProgress.Seen("H12"));
        foreach (var trigger in StoryTriggers.All)
            if (trigger.Sequence == "H05") Assert.IsTrue(CampaignProgress.Has(trigger.SeenFlag), trigger.Key);
    }

    [Test]
    public void PromptsNameTheWordToSay()
    {
        Assert.AreEqual("hablar", VoicePrompt.InteractWord("Hablar con Bachué"));
        Assert.AreEqual("examinar", VoicePrompt.InteractWord("Examinar la urna 2"));
        Assert.AreEqual("entrar", VoicePrompt.InteractWord("Viajar a Plaza Núñez"));
        Assert.AreEqual("usar", VoicePrompt.InteractWord("Descansar"));
        VoicePrompt.Enabled = true;
        Assert.AreEqual("«examinar»", VoicePrompt.Cap("examinar", "E"));
        VoicePrompt.Enabled = false;
        Assert.AreEqual("E", VoicePrompt.Cap("examinar", "E"));
    }
}

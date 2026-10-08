using NUnit.Framework;

// The signal language (2026-10-07): the warning never names the answer, the signal alone is
// enough to win every duel, and a mistake explains what the signal was.
public class DuelSignalTests
{
    private static readonly string[] Encounters =
        { HybridGuardianRules.EncounterId, SerpentRules.EncounterId, CondorRules.EncounterId, EagleRules.EncounterId, QuimueRules.EncounterId };

    // A player who only reads the signals (no text) answers every blow of every encounter.
    [TestCase(HybridGuardianRules.EncounterId)]
    [TestCase(SerpentRules.EncounterId)]
    [TestCase(CondorRules.EncounterId)]
    [TestCase(EagleRules.EncounterId)]
    [TestCase(QuimueRules.EncounterId)]
    public void ReadingTheSignalWinsEveryEncounter(string id)
    {
        var d = new TurnDuelModel(DuelEncounters.Create(id), 20, jaguar: true);
        for (int turn = 0; turn < 200 && !d.IsOver; turn++)
        {
            Assert.AreEqual(DuelInput.Accepted, TurnDuelModelTests.Choose(d), d.Message);
            if (d.IsOver) break;
            d.Advance();
            while (d.Phase == DuelPhase.Telegraph)
            {
                d.Advance();
                Assert.AreEqual(DuelInput.Accepted, d.Defend(DuelSignals.Answer(d.Move.Signal)));
                Assert.IsTrue(d.LastDefenseCorrect, d.Move.Id);
                d.Advance();
            }
            if (d.Phase == DuelPhase.Resolve) d.Advance();
        }
        Assert.AreEqual(DuelPhase.Won, d.Phase, id);
        Assert.AreEqual(100, d.PlayerHealth);
    }

    [Test]
    public void TheWarningDoesNotNameTheAnswer()
    {
        foreach (var id in Encounters)
            foreach (bool show in new[] { false, true })
            {
                var d = new TurnDuelModel(DuelEncounters.Create(id), 20, jaguar: true) { ShowAnswers = show };
                TurnDuelModelTests.Choose(d);
                d.Advance();
                if (d.Phase != DuelPhase.Telegraph) continue; // a cancelled round (anchored wind, interrupted charge)
                string message = d.Message;
                if (show) { StringAssert.Contains(d.Move.Verbs, message, id); continue; }
                foreach (DuelDefense defense in System.Enum.GetValues(typeof(DuelDefense)))
                    StringAssert.DoesNotContain(TurnDuelModel.DefenseName(defense), message, id);
                StringAssert.DoesNotContain(d.Move.Label, message, id);
            }
    }

    [Test]
    public void ParryAgainstABlowWithoutGlintIsAMistakeNotARefusal()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack); d.Advance(); d.Advance();         // golpe de ala (glint)
        Assert.IsTrue(d.Move.Glint);
        d.Defend(DuelDefense.Block); d.Advance();
        d.Act(DuelAction.Attack); d.Advance(); d.Advance();         // viento lateral, no glint
        Assert.IsFalse(d.Move.Glint);
        Assert.IsTrue(d.ParryOffered, "the button never gives the blow away");
        int concentration = d.Concentration;
        Assert.AreEqual(DuelInput.Accepted, d.Defend(DuelDefense.Parry));
        Assert.IsFalse(d.LastDefenseCorrect);
        Assert.AreEqual(85, d.PlayerHealth);
        Assert.AreEqual(concentration, d.Concentration, "a wrong parry spends nothing");
    }

    [Test]
    public void AMistakeExplainsTheSignal()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack); d.Advance(); d.Advance();
        d.Defend(DuelDefense.Cover);
        StringAssert.Contains("era Bloquear", d.Message);
        StringAssert.Contains("Parar", d.Message, "the glint is explained too");
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CampaignModelTests
{
    [Test]
    public void NewGameStartsInPrologueWithStaffObjective()
    {
        var c = new CampaignModel();
        Assert.AreEqual(CampaignChapter.Prologue, c.Chapter);
        StringAssert.Contains("bastón", c.Objective.Text);
        Assert.IsFalse(c.LowerWorldOpen);
        Assert.IsFalse(c.UpperWorldOpen);
    }

    [Test]
    public void ChaptersFollowTheCampaignOrder()
    {
        var c = new CampaignModel();
        var expected = new (string flag, CampaignChapter chapter)[]
        {
            (CampaignFlags.HubActive, CampaignChapter.PlazaTutorial),
            (CampaignFlags.MiUnlocked, CampaignChapter.LowerWorld),
            (CampaignFlags.ChiaReleased, CampaignChapter.LunarReturn),
            (CampaignFlags.GuacamayaAffinity, CampaignChapter.Urn),
            (CampaignFlags.MsUnlocked, CampaignChapter.UpperWorld),
            (CampaignFlags.SueReleased, CampaignChapter.SolarReturn),
            (CampaignFlags.QuimueDefeated, CampaignChapter.Ending),
            (CampaignFlags.CampaignComplete, CampaignChapter.Complete),
        };
        foreach (var (flag, chapter) in expected)
        {
            c.Set(flag);
            Assert.AreEqual(chapter, c.Chapter, flag);
        }
    }

    [Test]
    public void LaterFlagsImplyEarlierOnes()
    {
        var c = new CampaignModel();
        c.Set(CampaignFlags.MsUnlocked);
        Assert.IsTrue(c.Has(CampaignFlags.ChiaReleased));
        Assert.IsTrue(c.Has(CampaignFlags.MiGuardian));
        Assert.IsTrue(c.Has(CampaignFlags.MiUnlocked));
        Assert.IsTrue(c.Has(CampaignFlags.StaffOwned));
        Assert.IsTrue(c.LowerWorldOpen && c.UpperWorldOpen);
    }

    [Test]
    public void SetReportsOnlyNewFlagsAndRaisesChanged()
    {
        var c = new CampaignModel();
        var raised = new List<string>();
        c.Changed += raised.Add;
        Assert.IsTrue(c.Set(CampaignFlags.StaffOwned));
        Assert.IsFalse(c.Set(CampaignFlags.StaffOwned));
        Assert.IsFalse(c.Set(""));
        CollectionAssert.AreEqual(new[] { CampaignFlags.StaffOwned }, raised);
    }

    [Test]
    public void LowerWorldObjectiveWalksTheRoute()
    {
        var c = new CampaignModel();
        c.Set(CampaignFlags.MiUnlocked);
        StringAssert.Contains("semilla", c.Objective.Text);
        c.Set(CampaignFlags.MiSeed); c.Set(CampaignFlags.MiBracelets1);
        StringAssert.Contains("centinelas", c.Objective.Text);
        c.Set(CampaignFlags.MiBracelets2); c.Set(CampaignFlags.MiBracelets3);
        StringAssert.Contains("centinela", c.Objective.Text);
        c.Set(CampaignFlags.MiShield04);
        StringAssert.Contains("coca", c.Objective.Text);
        c.Set(CampaignFlags.CocaAffinity);
        StringAssert.Contains("cuerno", c.Objective.Text);
        c.Set(CampaignFlags.ChiaSealed);
        StringAssert.Contains("lazo", c.Objective.Text);
        Assert.AreEqual("Mundo inferior", c.Objective.Place);
    }

    [Test]
    public void JsonRoundTripKeepsFlags()
    {
        var c = new CampaignModel();
        c.Set(CampaignFlags.ChiaInCustody);
        var d = new CampaignModel();
        Assert.IsTrue(d.LoadJson(c.ToJson()));
        Assert.AreEqual(c.Count, d.Count);
        Assert.AreEqual(CampaignChapter.LunarReturn, d.Chapter);
    }

    [Test]
    public void BrokenOrForeignJsonLeavesAnEmptyCampaign()
    {
        var c = new CampaignModel();
        c.Set(CampaignFlags.HubActive);
        Assert.IsFalse(c.LoadJson("{not json"));
        Assert.AreEqual(0, c.Count);
        Assert.IsFalse(c.LoadJson("{\"version\":99,\"flags\":[\"hub_active\"]}"));
        Assert.AreEqual(0, c.Count);
    }

    [Test]
    public void WorldFlagTablesMapToDeclaredCampaignFlags()
    {
        var declared = DeclaredFlags();
        foreach (var table in new[] { CampaignWorldFlags.Lower, CampaignWorldFlags.Upper })
            for (int i = 0; i < table.GetLength(0); i++)
                Assert.IsTrue(declared.Contains(table[i, 1]), table[i, 1]);
        Assert.AreEqual(CampaignFlags.MiSeed, CampaignWorldFlags.ToCampaign(CampaignWorldFlags.Lower, "semilla"));
        Assert.AreEqual("ms_jefe_vencido", CampaignWorldFlags.ToWorld(CampaignWorldFlags.Upper, CampaignFlags.MsGuardian));
    }

    internal static HashSet<string> DeclaredFlags()
    {
        var set = new HashSet<string>();
        foreach (var f in typeof(CampaignFlags).GetFields(BindingFlags.Public | BindingFlags.Static))
            if (f.IsLiteral && f.FieldType == typeof(string)) set.Add((string)f.GetValue(null));
        return set;
    }
}

public class StoryScriptTests
{
    private static StoryScript Load()
    {
        string path = Path.Combine(Application.dataPath, "_Game/Resources/Narrative/historia.json");
        Assert.IsTrue(File.Exists(path), path);
        return StoryScript.Parse(File.ReadAllText(path));
    }

    [Test]
    public void ScriptHasTheTwentyOneSequencesInOrder()
    {
        var script = Load();
        Assert.AreEqual(21, script.sequences.Length);
        for (int i = 0; i < 21; i++) Assert.AreEqual($"H{i + 1:00}", script.sequences[i].id);
        Assert.AreEqual(12, script.contextual.Length);
    }

    [Test]
    public void EveryLineHasSpeakerAndTextAndEveryFlagIsDeclared()
    {
        var declared = CampaignModelTests.DeclaredFlags();
        foreach (var s in Load().sequences)
        {
            Assert.IsNotEmpty(s.lines, s.id);
            foreach (var line in s.lines)
            {
                Assert.IsFalse(string.IsNullOrEmpty(line.speaker), s.id);
                Assert.IsFalse(string.IsNullOrEmpty(line.text), s.id);
            }
            foreach (var f in s.requires) Assert.IsTrue(declared.Contains(f), s.id + " requires " + f);
            foreach (var f in s.sets) Assert.IsTrue(declared.Contains(f), s.id + " sets " + f);
        }
    }

    [Test]
    public void PlayingTheStoryScenesInOrderReachesTheEnd()
    {
        var script = Load();
        var c = new CampaignModel();
        // Gameplay flags the scenes wait for (finds, duels) are granted as the player would.
        var gameplay = new Dictionary<string, string[]>
        {
            { "H05", new[] { CampaignFlags.LessonsComplete, CampaignFlags.TrainingComplete, CampaignFlags.MiUnlocked } },
            { "H09", new[] { CampaignFlags.CocaAffinity, CampaignFlags.ChiaSealed } },
            { "H10", new[] { CampaignFlags.ChiaReleased } },
            { "H13", new[] { CampaignFlags.MsRune2 } },
            { "H14", new[] { CampaignFlags.EagleResolved } },
            { "H15", new[] { CampaignFlags.MsLockOpen } },
            { "H16", new[] { CampaignFlags.SueReleased } },
            { "H17", new[] { CampaignFlags.QuimueDefeated } },
        };
        foreach (var s in script.sequences)
        {
            Assert.IsTrue(s.Available(c), s.id + " not reachable");
            StoryScript.Complete(c, s);
            if (gameplay.TryGetValue(s.id, out var flags)) foreach (var f in flags) c.Set(f);
        }
        Assert.AreEqual(CampaignChapter.Complete, c.Chapter);
        Assert.AreEqual(21, script.Lived(c).Count);
    }

    [Test]
    public void CuesSplitScenesPlayedAtDifferentMoments()
    {
        var h05 = Load().Get("H05");
        Assert.AreEqual(1, h05.Cue("vasija").Count);
        Assert.AreEqual(h05.lines.Length, h05.Cue("").Count);
        var h10 = Load().Get("H10");
        Assert.AreEqual("Jaguar.", h10.Cue("jaguar")[0].text);
    }

    [Test]
    public void EveryTriggerPointsAtLinesOfTheScript()
    {
        var script = Load();
        var keys = new HashSet<string>();
        foreach (var trigger in StoryTriggers.All)
        {
            Assert.IsTrue(keys.Add(trigger.Key), "duplicate " + trigger.Key);
            var sequence = script.Get(trigger.Sequence);
            Assert.IsNotNull(sequence, trigger.Key);
            foreach (var cue in trigger.Cues) Assert.IsNotEmpty(sequence.Cue(cue), trigger.Key + " cue " + cue);
        }
        // Every cued line of a sequence is said by some trigger.
        var duelCues = new HashSet<string>();
        foreach (var s in script.sequences)
            foreach (var line in s.lines)
            {
                if (string.IsNullOrEmpty(line.cue) || duelCues.Contains(s.id + "/" + line.cue)) continue;
                Assert.IsTrue(System.Array.Exists(StoryTriggers.All, t => t.Sequence == s.id && System.Array.IndexOf(t.Cues, line.cue) >= 0),
                    s.id + " cue " + line.cue + " is never played");
            }
    }

    [Test]
    public void CanonIsTheHistoryText()
    {
        var h17 = Load().Get("H17");
        Assert.IsTrue(Array.Exists(h17.lines, l => l.text == "Por eso no me quedaré con su voluntad."));
    }
}

public class VoiceVocabularyTests
{
    [Test]
    public void EveryCommandHasSpanishAndEnglishWords()
    {
        var english = new HashSet<string> { "dodge", "attack", "block", "examine", "take", "back", "counter", "parry", "cover",
            "interrupt", "anchor", "jaguar", "horn", "bind", "moon", "sun", "head a", "head b" };
        foreach (VoiceCommand command in Enum.GetValues(typeof(VoiceCommand)))
        {
            var words = new List<string>(VoiceVocabulary.PhrasesFor(command));
            Assert.GreaterOrEqual(words.Count, 1, command.ToString());
            Assert.IsTrue(words.Exists(english.Contains), command + " has no English word");
        }
        Assert.IsTrue(VoiceVocabulary.TryParse("Contraataca", out var c) && c == VoiceCommand.Counter);
        Assert.IsTrue(VoiceVocabulary.TryParse("izquierda", out var a) && a == VoiceCommand.HeadA);
        Assert.IsFalse(VoiceVocabulary.TryParse("para", out _));
    }

    [Test]
    public void DuelMappingsCoverEveryDuelVerb()
    {
        foreach (DuelAction action in Enum.GetValues(typeof(DuelAction)))
        {
            bool found = false;
            foreach (VoiceCommand c in Enum.GetValues(typeof(VoiceCommand)))
                if (VoiceVocabulary.ToAction(c, out var a) && a == action) found = true;
            Assert.IsTrue(found, action.ToString());
        }
        foreach (DuelDefense defense in Enum.GetValues(typeof(DuelDefense)))
        {
            bool found = false;
            foreach (VoiceCommand c in Enum.GetValues(typeof(VoiceCommand)))
                if (VoiceVocabulary.ToDefense(c, out var d) && d == defense) found = true;
            Assert.IsTrue(found, defense.ToString());
        }
    }
}

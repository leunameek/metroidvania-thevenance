using System;
using NUnit.Framework;

public class TurnDuelModelTests
{
    // Plays the enemy turn after an action and leaves the duel in its response window.
    private static void ToResponse(TurnDuelModel d)
    {
        d.Advance(); // Execute -> Telegraph
        d.Advance(); // Telegraph -> Respond
        Assert.AreEqual(DuelPhase.Respond, d.Phase);
    }
    private static void Correct(TurnDuelModel d)
    {
        foreach (var a in d.Move.Answers) if (a != DuelDefense.Parry) { Assert.AreEqual(DuelInput.Accepted, d.Defend(a)); break; }
        d.Advance(); // Resolve -> Decide
    }

    [Test]
    public void DefenseDuringWarningIsRejectedWithoutPenalty()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack);
        d.Advance();
        Assert.AreEqual(DuelPhase.Telegraph, d.Phase);
        Assert.AreEqual(DuelInput.TooEarly, d.Defend(DuelDefense.Block));
        Assert.AreEqual(100, d.PlayerHealth);
        Assert.AreEqual(DuelPhase.Telegraph, d.Phase);
    }

    [Test]
    public void SilenceFailsOnlyWhenTheWindowIsTimed()
    {
        var timed = new TurnDuelModel(new CondorRules());
        timed.Act(DuelAction.Attack);
        ToResponse(timed);
        timed.Advance();
        Assert.AreEqual(85, timed.PlayerHealth);

        var untimed = new TurnDuelModel(new CondorRules()) { Untimed = true };
        untimed.Act(DuelAction.Attack);
        ToResponse(untimed);
        untimed.Tick(999f);
        Assert.AreEqual(DuelPhase.Respond, untimed.Phase);
        Assert.AreEqual(100, untimed.PlayerHealth);
    }

    [Test]
    public void OneActionPerTurnAndNoDefenseOutsideTheWindow()
    {
        var d = new TurnDuelModel(new CondorRules());
        Assert.AreEqual(DuelInput.Accepted, d.Act(DuelAction.Attack));
        Assert.AreEqual(DuelInput.WrongPhase, d.Act(DuelAction.Attack));
        Assert.AreEqual(40, d.EnemyHealth);
        var fresh = new TurnDuelModel(new CondorRules());
        Assert.AreEqual(DuelInput.WrongPhase, fresh.Defend(DuelDefense.Block));
    }

    [Test]
    public void CorrectDefenseRaisesConcentrationAndOpensCounter()
    {
        var d = new TurnDuelModel(new CondorRules());
        Assert.AreEqual(1, d.Concentration);
        d.Act(DuelAction.Attack);
        ToResponse(d);                    // golpe de ala, physical
        Correct(d);
        Assert.AreEqual(2, d.Concentration);
        Assert.AreEqual(CounterWindow.Normal, d.Counter);
        d.Act(DuelAction.Counter);
        Assert.AreEqual(60 - 20 - 30, d.EnemyHealth);
        Assert.AreEqual(1, d.Concentration);
    }

    [Test]
    public void ParrySpendsConcentrationAndReinforcesTheCounter()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack);
        ToResponse(d);
        Assert.IsTrue(d.ParryOffered);
        Assert.AreEqual(DuelInput.Accepted, d.Defend(DuelDefense.Parry));
        d.Advance();
        Assert.AreEqual(0, d.Concentration);
        Assert.AreEqual(CounterWindow.Reinforced, d.Counter);
        Assert.IsFalse(d.IsLegal(DuelAction.Counter, out _), "counter needs concentration");
    }

    [Test]
    public void CounterChanceIsLostWhenChoosingAnotherAction()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack);
        ToResponse(d);
        Correct(d);
        d.Act(DuelAction.Anchor);
        Assert.AreEqual(CounterWindow.None, d.Counter);
    }

    [Test]
    public void WrongDefenseCostsTheMoveDamage()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack);
        ToResponse(d);
        d.Defend(DuelDefense.Cover);
        Assert.AreEqual(85, d.PlayerHealth);
        Assert.IsFalse(d.LastDefenseCorrect);
    }

    [Test]
    public void YoposRaiseTheBaseUpToThirty()
    {
        Assert.AreEqual(30, new TurnDuelModel(new CondorRules(), 40).DamageBase);
        var d = new TurnDuelModel(new CondorRules(), 25);
        d.Act(DuelAction.Attack);
        Assert.AreEqual(35, d.EnemyHealth);
    }

    // Guion 07 "Jaguar sin reservas agotables".
    [Test]
    public void HybridGuardianNeedsTwoJaguarBreaks()
    {
        var rules = new HybridGuardianRules();
        var d = new TurnDuelModel(rules, 20, jaguar: true);
        Assert.IsFalse(d.IsLegal(DuelAction.Jaguar, out _), "core closed");
        d.Act(DuelAction.Attack);
        Assert.AreEqual(150, d.EnemyHealth, "closed core halves the hit");
        ToResponse(d);
        Correct(d);
        Assert.IsTrue(rules.CoreOpen);
        Assert.AreEqual(2, d.Concentration);
        d.Act(DuelAction.Jaguar);
        Assert.AreEqual(120, d.EnemyHealth);
        Assert.AreEqual(1, rules.BondsLeft);
        Assert.AreEqual(1, d.Concentration);
        // Second bond only at health <= 80.
        while (d.EnemyHealth > 80) { ToResponse(d); Correct(d); d.Act(DuelAction.Attack); }
        ToResponse(d); Correct(d);
        Assert.IsTrue(rules.JaguarBreaksBond(d));
        d.Act(DuelAction.Jaguar);
        Assert.AreEqual(0, rules.BondsLeft);
    }

    [Test]
    public void HybridGuardianStaysAtOneWhileABondRemains()
    {
        var rules = new HybridGuardianRules();
        var d = new TurnDuelModel(rules, 30, jaguar: true);
        for (int i = 0; i < 30 && !d.IsOver; i++)
        {
            d.Act(DuelAction.Attack);
            if (d.IsOver) break;
            ToResponse(d); Correct(d);
        }
        Assert.IsFalse(d.IsOver);
        Assert.AreEqual(1, d.EnemyHealth);
        Assert.AreEqual(2, rules.BondsLeft);
        Assert.IsTrue(d.IsLegal(DuelAction.Jaguar, out _), "the exposed core offers the window, no softlock");
    }

    [Test]
    public void HornOpensTheCoreForTheNextTurnEvenAfterAMistake()
    {
        var rules = new HybridGuardianRules();
        var d = new TurnDuelModel(rules, 20, jaguar: true);
        d.Act(DuelAction.Horn);
        Assert.AreEqual(160, d.EnemyHealth);
        ToResponse(d);
        d.Defend(DuelDefense.Cover); // wrong on purpose
        d.Advance();
        Assert.IsTrue(rules.CoreOpen);
    }

    // Guion 07 "Serpiente, leer origen".
    [Test]
    public void SerpentHeadsMustBeReadFromTheWarning()
    {
        var rules = new SerpentRules();
        var d = new TurnDuelModel(rules);
        Assert.AreEqual(DuelInput.NeedsTarget, d.Act(DuelAction.Attack));
        Assert.AreEqual(DuelPhase.Decide, d.Phase, "asking for the head does not spend the turn");
        d.SelectTarget(DuelTarget.HeadA);
        d.Act(DuelAction.Attack);
        Assert.AreEqual(20, rules.BondA);
        ToResponse(d);
        Assert.AreEqual(DuelTarget.HeadA, d.Move.Origin);
        Correct(d);
        d.SelectTarget(DuelTarget.HeadB);
        d.Act(DuelAction.Attack);
        Assert.AreEqual(40, rules.BondB, "B was not vulnerable");
        Assert.AreEqual(180 - 20 - 10, d.EnemyHealth);
        ToResponse(d);
        Assert.AreEqual(DuelTarget.HeadB, d.Move.Origin);
        Correct(d);
        Assert.AreEqual(DuelTarget.HeadB, rules.Vulnerable);
    }

    // Guion 07 "Quimue con daño mínimo".
    [Test]
    public void QuimueBindOnlyCutsTheActiveOrigin()
    {
        var rules = new QuimueRules();
        var d = new TurnDuelModel(rules);
        Assert.AreEqual(DuelTarget.Moon, rules.ActiveOrigin);
        d.SelectTarget(DuelTarget.Sun);
        d.Act(DuelAction.Bind);
        Assert.AreEqual(190, d.EnemyHealth);
        Assert.AreEqual(40, rules.SunAnchor);
        ToResponse(d);
        Assert.AreEqual(DuelTarget.Moon, d.Move.Origin);
        Correct(d);
        d.SelectTarget(DuelTarget.Moon);
        d.Act(DuelAction.Bind);
        Assert.AreEqual(20, rules.MoonAnchor);
        Assert.IsFalse(d.IsLegal(DuelAction.Jaguar, out _));
    }

    [Test]
    public void CondorAnchorCancelsTheNextWind()
    {
        var d = new TurnDuelModel(new CondorRules());
        d.Act(DuelAction.Attack);
        ToResponse(d); Correct(d);         // ala
        int conc = d.Concentration;
        d.Act(DuelAction.Anchor);
        d.Advance();                        // the wind is skipped
        Assert.AreEqual(DuelPhase.Resolve, d.Phase);
        Assert.AreEqual(conc + 1, d.Concentration);
        d.Advance();
        Assert.AreEqual(DuelPhase.Decide, d.Phase);
        Assert.AreEqual(100, d.PlayerHealth);
    }

    [Test]
    public void EagleInterruptGroundsHerAndCancelsTheCharge()
    {
        var rules = new EagleRules();
        var d = new TurnDuelModel(rules);
        d.Act(DuelAction.Attack);
        Assert.AreEqual(50, d.EnemyHealth, "elevated: half damage");
        ToResponse(d); Correct(d);         // charge, she stays up
        ToResponseFromDecide(d, DuelAction.Interrupt);
        Assert.IsFalse(rules.Elevated);
        Correct(d);                         // feathers
        d.Act(DuelAction.Attack);
        Assert.AreEqual(50 - 10 - 20, d.EnemyHealth);
        d.Advance();                        // the next charge is cancelled
        Assert.AreEqual(DuelPhase.Resolve, d.Phase);
    }
    private static void ToResponseFromDecide(TurnDuelModel d, DuelAction action)
    {
        d.Act(action);
        ToResponse(d);
    }

    // Guion 08 "Combate": every main encounter can be won with D=20, no yopo and no parry, and
    // never ends with a bond pending or without a legal action.
    [TestCase(HybridGuardianRules.EncounterId)]
    [TestCase(SerpentRules.EncounterId)]
    [TestCase(CondorRules.EncounterId)]
    [TestCase(EagleRules.EncounterId)]
    [TestCase(QuimueRules.EncounterId)]
    public void EveryEncounterIsWinnableWithBaseDamageAndNoParry(string id)
    {
        var rules = DuelEncounters.Create(id);
        var d = new TurnDuelModel(rules, 20, jaguar: true);
        for (int turn = 0; turn < 200 && !d.IsOver; turn++)
        {
            Assert.AreEqual(DuelPhase.Decide, d.Phase);
            Assert.IsNotEmpty(new System.Collections.Generic.List<DuelAction>(d.LegalActions()), id + " has no legal action");
            Assert.AreEqual(DuelInput.Accepted, Choose(d), d.Message);
            if (d.IsOver) break;
            d.Advance();
            if (d.Phase == DuelPhase.Telegraph) { d.Advance(); Correct(d); }
            else d.Advance();
            while (d.Phase == DuelPhase.Telegraph) { d.Advance(); Correct(d); }
        }
        Assert.AreEqual(DuelPhase.Won, d.Phase, id);
        Assert.AreEqual(100, d.PlayerHealth);
        if (rules is HybridGuardianRules h) Assert.AreEqual(0, h.BondsLeft);
        if (rules is SerpentRules s) Assert.AreEqual(0, s.BondA + s.BondB);
        if (rules is QuimueRules q) Assert.IsTrue(q.AnchorsCut);
    }

    // A simple player who reads the HUD: the action a human would pick from the script's tips.
    internal static DuelInput Choose(TurnDuelModel d)
    {
        switch (d.Rules)
        {
            case HybridGuardianRules h:
                if (h.JaguarBreaksBond(d) && d.IsLegal(DuelAction.Jaguar, out _)) return d.Act(DuelAction.Jaguar);
                if (h.BondsLeft > 0 && d.EnemyHealth == 1 && !h.CoreOpen) return d.Act(DuelAction.Horn);
                return d.Act(DuelAction.Attack);
            case SerpentRules s:
                d.SelectTarget(s.Vulnerable);
                return d.Act(DuelAction.Attack);
            case EagleRules e:
                return d.Act(e.Elevated ? DuelAction.Interrupt : DuelAction.Attack);
            case QuimueRules q:
                int active = q.ActiveOrigin == DuelTarget.Moon ? q.MoonAnchor : q.SunAnchor;
                if (active > 0 && d.Concentration >= 1) { d.SelectTarget(q.ActiveOrigin); return d.Act(DuelAction.Bind); }
                return d.Act(DuelAction.Attack);
            default:
                return d.Act(DuelAction.Attack);
        }
    }
}

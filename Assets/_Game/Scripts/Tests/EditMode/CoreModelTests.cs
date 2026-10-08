using NUnit.Framework;

// The pure models behind the components (2026-10-08 MVC review): vitality, a world's finds and
// what the player is doing, each announcing its changes to the views.
public class CoreModelTests
{
    [Test]
    public void HealthHonoursInvulnerabilityAndAnnouncesDeath()
    {
        var health = new HealthModel(100, .5f);
        float shown = -1; bool died = false;
        health.Changed += (current, max) => shown = current;
        health.Died += () => died = true;
        Assert.IsTrue(health.TakeDamage(30, 10f));
        Assert.AreEqual(70, shown);
        Assert.IsFalse(health.TakeDamage(30, 10.2f), "a second hit inside the invulnerability lands");
        Assert.IsTrue(health.TakeDamage(80, 11f));
        Assert.IsTrue(died && health.IsDead && health.Current == 0);
        health.Heal(50);
        Assert.AreEqual(0, health.Current, "the dead do not heal");
        health.Revive();
        Assert.IsFalse(health.IsDead);
        Assert.AreEqual(100, health.Current);
        Assert.IsTrue(health.TakeDamage(10, 11.1f), "revival clears the invulnerability");
    }

    [Test]
    public void AFindCountsOnceAndTheAbilitiesAreDerived()
    {
        var lower = new InventoryModel();
        int announced = 0;
        lower.Changed += id => announced++;
        Assert.IsTrue(lower.Add("brazaletes2"));
        Assert.IsFalse(lower.Add("brazaletes2"));
        Assert.AreEqual(1, announced);
        Assert.AreEqual(2, WorldInventoryRules.DashTier(lower));
        Assert.IsFalse(WorldInventoryRules.DoubleJump(lower));
        lower.Add("semilla"); lower.Add("ofrenda_01"); lower.Add("ofrenda_04");
        Assert.IsTrue(WorldInventoryRules.DoubleJump(lower));
        Assert.AreEqual(2, lower.Count(WorldInventoryRules.LowerFinds));
        Assert.AreEqual(2, lower.CountPrefix("ofrenda_"));
        lower.Load(new[] { "cuerno" });
        Assert.AreEqual(1, lower.Total);
        Assert.AreEqual(4, announced, "loading a save is silent");

        var upper = new InventoryModel();
        Assert.AreEqual(20f, WorldInventoryRules.AttackDamage(upper));
        upper.Add("ms_yopo_1"); upper.Add("ms_yopo_2");
        Assert.AreEqual(30f, WorldInventoryRules.AttackDamage(upper));
    }

    [Test]
    public void TheGameStateAnnouncesOnlyRealChanges()
    {
        var state = new GameState();
        int changes = 0;
        state.Changed += s => changes++;
        Assert.IsTrue(state.Exploring);
        Assert.IsFalse(state.Set(TechnicalDemoState.Exploration));
        Assert.IsTrue(state.Set(TechnicalDemoState.Combat));
        Assert.AreEqual(TechnicalDemoState.Combat, state.Current);
        Assert.AreEqual(1, changes);
    }
}

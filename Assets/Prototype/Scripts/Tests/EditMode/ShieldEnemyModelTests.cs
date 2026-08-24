using NUnit.Framework;
using UnityEngine;

public class ShieldEnemyModelTests
{
    private static ShieldEnemyModel NewModel()
    {
        return new ShieldEnemyModel(spawnPosition: Vector3.zero, detectionRange: 12f, minChargeRange: 2.5f,
            chargeSpeed: 18f, windupDuration: 0.9f, chargeDuration: 0.5f, cooldown: 6f, hitRadius: 1.2f,
            chargeDamage: 50f, patrolRadius: 4f, patrolSpeed: 1.5f, patrolPauseDuration: 1.5f);
    }

    [Test]
    public void TryStartWindup_TooClose_IsRejected()
    {
        ShieldEnemyModel model = NewModel();

        // Well inside detectionRange (12) but under minChargeRange (2.5) - this is the exact
        // interaction documented in Combat-Damage-Enemies.md: guarantees the player a safe
        // window to land a Tier-3 combo without the shield enemy interrupting with a charge.
        bool started = model.TryStartWindup(new Vector3(1f, 0f, 0f));

        Assert.IsFalse(started);
        Assert.AreEqual(ShieldEnemyModel.ChargeState.Idle, model.State);
    }

    [Test]
    public void TryStartWindup_InChargeBand_Starts()
    {
        ShieldEnemyModel model = NewModel();

        bool started = model.TryStartWindup(new Vector3(5f, 0f, 0f));

        Assert.IsTrue(started);
        Assert.AreEqual(ShieldEnemyModel.ChargeState.Windup, model.State);
    }

    [Test]
    public void TryStartWindup_BeyondDetectionRange_IsRejected()
    {
        ShieldEnemyModel model = NewModel();

        bool started = model.TryStartWindup(new Vector3(20f, 0f, 0f));

        Assert.IsFalse(started);
    }

    [Test]
    public void RegisterDashChainHit_BreaksShield_OnlyAtThreeChainedHits()
    {
        ShieldEnemyModel model = NewModel();

        Assert.IsFalse(model.RegisterDashChainHit(1));
        Assert.IsFalse(model.RegisterDashChainHit(2));
        Assert.IsTrue(model.IsShielded);

        bool broke = model.RegisterDashChainHit(3);

        Assert.IsTrue(broke);
        Assert.IsFalse(model.IsShielded);
    }

    [Test]
    public void RegisterDashChainHit_OnceBroken_DoesNotReportBreakingAgain()
    {
        ShieldEnemyModel model = NewModel();
        model.RegisterDashChainHit(3);

        bool brokeAgain = model.RegisterDashChainHit(3);

        Assert.IsFalse(brokeAgain, "caller uses this to gate one-time visual/collider side effects");
    }
}

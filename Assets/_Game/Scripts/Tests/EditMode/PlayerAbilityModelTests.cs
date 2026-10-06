using NUnit.Framework;
using UnityEngine;

public class PlayerAbilityModelTests
{
    private static PlayerAbilityModel NewModel(float chainWindow = 0.2f)
    {
        return new PlayerAbilityModel(dashSpeed: 12f, dashDuration: 0.2f, dashChainWindow: chainWindow,
            dodgeDistance: 4f, dodgeDuration: 0.25f);
    }

    [Test]
    public void TryPressDash_WithNoTier_IsIgnored()
    {
        PlayerAbilityModel model = NewModel();

        DashPressResult result = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0f);

        Assert.AreEqual(DashPressResult.Ignored, result);
    }

    [Test]
    public void TryPressDash_Tier1_CannotChain()
    {
        PlayerAbilityModel model = NewModel();
        model.GrantDash(1);

        DashPressResult first = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0f);
        DashPressResult second = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0.05f);

        Assert.AreEqual(DashPressResult.Started, first);
        Assert.AreEqual(DashPressResult.Ignored, second);
    }

    [Test]
    public void TryPressDash_AfterThreeUpgrades_ChainsUpToThreeWithinWindow()
    {
        PlayerAbilityModel model = NewModel(chainWindow: 0.2f);
        model.GrantDashUpgrade();
        model.GrantDashUpgrade();
        model.GrantDashUpgrade();

        DashPressResult first = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0f);
        DashPressResult second = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0.1f);
        DashPressResult third = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0.2f);
        DashPressResult fourth = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0.3f);

        Assert.AreEqual(DashPressResult.Started, first);
        Assert.AreEqual(DashPressResult.Chained, second);
        Assert.AreEqual(DashPressResult.Chained, third);
        Assert.AreEqual(DashPressResult.Ignored, fourth, "a 4th chain link should be rejected even though the window was hit, because DashChainCount already reached DashTier");
    }

    [Test]
    public void TryPressDash_ChainPressOutsideWindow_IsIgnored()
    {
        PlayerAbilityModel model = NewModel(chainWindow: 0.2f);
        model.GrantDash(3);

        model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0f);
        DashPressResult lateChain = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: false, time: 0.5f);

        Assert.AreEqual(DashPressResult.Ignored, lateChain);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void GrantDashUpgrade_UnlocksExactlyOneAdditionalChainLink(int existingTier)
    {
        PlayerAbilityModel model = NewModel();
        model.GrantDash(existingTier);

        model.GrantDashUpgrade();

        Assert.AreEqual(existingTier + 1, model.DashTier);
        Assert.AreEqual(DashPressResult.Started, model.TryPressDash(Vector3.forward, Vector3.forward, false, 0f));
        for (int link = 1; link <= existingTier; link++)
            Assert.AreEqual(DashPressResult.Chained, model.TryPressDash(Vector3.forward, Vector3.forward, false, link * .025f));
        Assert.AreEqual(DashPressResult.Ignored,
            model.TryPressDash(Vector3.forward, Vector3.forward, false, (existingTier + 1) * .025f),
            "A pickup must unlock only one extra dash, not its former fixed tier.");
    }

    [Test]
    public void TryPressDash_OnLadder_IsIgnored()
    {
        PlayerAbilityModel model = NewModel();
        model.GrantDash(1);

        DashPressResult result = model.TryPressDash(Vector3.forward, Vector3.forward, isOnLadder: true, time: 0f);

        Assert.AreEqual(DashPressResult.Ignored, result);
    }
}

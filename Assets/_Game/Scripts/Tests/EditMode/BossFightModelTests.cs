using NUnit.Framework;

public class BossFightModelTests
{
    private static BossFightModel NewModel()
    {
        return new BossFightModel(telegraphDuration: 1.2f, reactDuration: 1.5f, resolveDuration: 0.8f, raisedYThreshold: 0.4f);
    }

    [Test]
    public void StartFight_WhileAlreadyActive_IsRejected()
    {
        BossFightModel model = NewModel();
        model.StartFight();

        bool startedAgain = model.StartFight();

        Assert.IsFalse(startedAgain);
    }

    [Test]
    public void MissedReact_FiresOnceReactWindowExpiresWithNoDodge()
    {
        BossFightModel model = NewModel();
        model.StartFight();
        model.Tick(1.2f); // clears the telegraph, enters PlayerReact

        int missCount = 0;
        model.MissedReact += () => missCount++;

        model.Tick(1.5f); // exhausts the react window with no dodge requested

        Assert.AreEqual(1, missCount);
        Assert.AreEqual(BossFightModel.FightState.Resolve, model.State);
    }

    [Test]
    public void RequestDodge_DuringReactWindow_ResolvesImmediatelyWithoutMissing()
    {
        BossFightModel model = NewModel();
        model.StartFight();
        model.Tick(1.2f); // enters PlayerReact

        int missCount = 0;
        int? dodgedDirection = null;
        model.MissedReact += () => missCount++;
        model.DodgeResolved += direction => dodgedDirection = direction;

        model.RequestDodge(1);
        model.Tick(0.01f); // one frame is enough to consume the pending dodge request

        Assert.AreEqual(0, missCount);
        Assert.AreEqual(1, dodgedDirection);
        Assert.AreEqual(BossFightModel.FightState.Resolve, model.State);
    }

    [Test]
    public void RequestDodge_OutsidePlayerReact_IsIgnored()
    {
        BossFightModel model = NewModel();
        model.StartFight(); // still in Telegraph, not PlayerReact yet

        int? dodgedDirection = null;
        model.DodgeResolved += direction => dodgedDirection = direction;

        model.RequestDodge(1);
        model.Tick(1.2f); // advances into PlayerReact - the stale request must not have carried over

        Assert.IsNull(dodgedDirection);
    }
}

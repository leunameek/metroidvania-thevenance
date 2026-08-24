using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class HandGestureModelTests
{
    // Builds a 21-point landmark set with the wrist at the origin and the palm (index 9) at
    // the given position. When open, every finger's tip sits far past its pip (extended);
    // when closed, tip and pip sit at the same distance (curled in) - matches the
    // wrist-to-tip vs wrist-to-pip heuristic in HandGestureModel.IsFingerExtended.
    private static List<Vector2> BuildHand(Vector2 palm, bool open)
    {
        var points = new List<Vector2>();
        for (int i = 0; i < 21; i++) points.Add(Vector2.zero);

        points[0] = Vector2.zero; // wrist
        points[9] = palm;

        (int pip, int tip)[] fingers = { (6, 8), (10, 12), (14, 16), (18, 20) };
        foreach ((int pip, int tip) in fingers)
        {
            points[pip] = new Vector2(0.3f, 0f);
            points[tip] = open ? new Vector2(1f, 0f) : new Vector2(0.3f, 0f);
        }

        return points;
    }

    [Test]
    public void ClosedHand_FreezesDeltaAccumulation()
    {
        var model = new HandGestureModel(openFingerMargin: 1.2f);

        model.Update(BuildHand(new Vector2(0f, 0f), open: true));
        model.Update(BuildHand(new Vector2(0.1f, 0f), open: false)); // hand closes and moves
        model.Update(BuildHand(new Vector2(0.2f, 0f), open: false)); // still closed, keeps moving

        Assert.AreEqual(0f, model.ConsumeDeltaX(), 0.0001f, "delta must not accumulate while the hand is closed");
    }

    [Test]
    public void ReopeningHand_ResumesWithNoJump()
    {
        var model = new HandGestureModel(openFingerMargin: 1.2f);

        model.Update(BuildHand(new Vector2(0f, 0f), open: true));
        model.Update(BuildHand(new Vector2(0.5f, 0f), open: false)); // closed - moves far while frozen
        model.ConsumeDeltaX(); // clear whatever (should be ~0) accumulated so far

        model.Update(BuildHand(new Vector2(0.5f, 0f), open: true)); // reopens exactly where it left off
        model.Update(BuildHand(new Vector2(0.6f, 0f), open: true)); // then moves a further 0.1

        Assert.AreEqual(0.1f, model.ConsumeDeltaX(), 0.0001f, "only the post-reopen movement should count, not the frozen-period jump");
    }

    [Test]
    public void HandDisappearing_ResetsReferenceSoReappearingDoesNotJump()
    {
        var model = new HandGestureModel(openFingerMargin: 1.2f);

        model.Update(BuildHand(new Vector2(0f, 0f), open: true));
        model.MarkAbsent(); // hand leaves the frame
        model.Update(BuildHand(new Vector2(5f, 0f), open: true)); // reappears far away
        model.Update(BuildHand(new Vector2(5.1f, 0f), open: true));

        Assert.AreEqual(0.1f, model.ConsumeDeltaX(), 0.0001f, "the big jump on reappearance must not be counted as movement");
    }
}

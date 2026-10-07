using NUnit.Framework;
using UnityEngine;

public class HandGestureRecognizerModelTests
{
    private const float Frame = 1f / 30f; // MediaPipe live results arrive at about camera rate
    private HandGestureRecognizerModel _model;
    private float _time;

    [SetUp]
    public void SetUp()
    {
        _model = new HandGestureRecognizerModel();
        _model.SetDwellSeconds(1f);
        _time = 10f;
    }

    private static HandSample Open(float x, float y) => new HandSample(true, true, new Vector2(x, y));
    private static HandSample Fist(float x, float y) => new HandSample(true, false, new Vector2(x, y));

    private void Run(float seconds, HandSample left, HandSample right)
    {
        for (float t = 0; t < seconds; t += Frame) { _time += Frame; _model.Sample(_time, left, right); }
    }

    [Test]
    public void ClosingAnOpenHand_Strikes()
    {
        Run(.2f, default, Open(.5f, .5f));
        Run(.1f, default, Fist(.5f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.Strike, _time));
        Assert.IsFalse(_model.Consume(HandGesture.Strike, _time), "an event is consumed once");
    }

    [Test]
    public void HandEnteringClosed_DoesNotStrike()
    {
        Run(.3f, default, Fist(.5f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.Strike, _time));
    }

    [Test]
    public void SingleNoisyClosedFrame_DoesNotStrike()
    {
        Run(.2f, default, Open(.5f, .5f));
        Run(Frame * .5f, default, Fist(.5f, .5f));
        Run(.2f, default, Open(.5f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.Strike, _time));
    }

    [Test]
    public void StaleEvent_Expires()
    {
        Run(.2f, default, Open(.5f, .5f));
        Run(.1f, default, Fist(.5f, .5f));
        Run(1f, default, Fist(.5f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.Strike, _time));
    }

    [Test]
    public void TwoRaisedOpenPalms_HoldGuard()
    {
        Run(.1f, Open(.3f, .4f), Open(.7f, .4f));
        Assert.IsFalse(_model.GuardHeld, "the pose needs a moment");
        Run(.3f, Open(.3f, .4f), Open(.7f, .4f));
        Assert.IsTrue(_model.GuardHeld);
        Run(.1f, Open(.3f, .4f), Fist(.7f, .4f));
        Assert.IsFalse(_model.GuardHeld);
    }

    [Test]
    public void LoweredPalms_DoNotGuard()
    {
        Run(.6f, Open(.3f, .9f), Open(.7f, .9f));
        Assert.IsFalse(_model.GuardHeld);
    }

    [Test]
    public void FastSidewaysSweep_Swipes_WithMirroredDirection()
    {
        Run(.1f, default, Open(.7f, .5f));
        for (int i = 0; i < 6; i++) Run(Frame, default, Open(.7f - i * .06f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.Swipe, _time));
        // The hand moved to the image's left, which is the player's right in the mirror.
        Assert.AreEqual(1, _model.LastSwipeDirection);
    }

    [Test]
    public void SlowDrift_DoesNotSwipe()
    {
        for (int i = 0; i < 60; i++) Run(Frame, default, Open(.7f - i * .005f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.Swipe, _time));
    }

    [Test]
    public void StillOpenPalm_CompletesHold_ThenNeedsRearm()
    {
        Run(.5f, default, Open(.5f, .5f));
        Assert.That(_model.PalmHoldProgress, Is.InRange(.3f, .7f));
        Run(.6f, default, Open(.5f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.PalmHold, _time));
        Run(1.5f, default, Open(.5f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.PalmHold, _time), "the same raised hand does not repeat");
        Run(.1f, default, Fist(.5f, .5f));
        Run(1.1f, default, Open(.5f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.PalmHold, _time));
    }

    [Test]
    public void GuardPose_IsNotAPalmHold()
    {
        Run(1.5f, Open(.3f, .4f), Open(.7f, .4f));
        Assert.IsFalse(_model.Consume(HandGesture.PalmHold, _time));
    }

    [Test]
    public void HeldFist_Grabs_AfterOpening()
    {
        Run(.2f, default, Open(.5f, .5f));
        Run(.8f, default, Fist(.5f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.Grab, _time));
    }

    [Test]
    public void ClearPending_DropsEvents_AndDemandsAFreshFist()
    {
        Run(.2f, default, Open(.5f, .5f));
        Run(.3f, default, Fist(.5f, .5f));
        _model.ClearPending();
        Assert.IsFalse(_model.Consume(HandGesture.Strike, _time));
        Run(1f, default, Fist(.5f, .5f));
        Assert.IsFalse(_model.Consume(HandGesture.Grab, _time), "fists closed before the clear never grab");
        Run(.2f, default, Open(.5f, .5f));
        Run(.8f, default, Fist(.5f, .5f));
        Assert.IsTrue(_model.Consume(HandGesture.Grab, _time));
    }

    [Test]
    public void LosingTheHands_DropsStates()
    {
        Run(.5f, Open(.3f, .4f), Open(.7f, .4f));
        Assert.IsTrue(_model.GuardHeld);
        _model.MarkAbsent(_time + Frame);
        Assert.IsFalse(_model.GuardHeld);
        Assert.IsFalse(_model.AnyHandPresent);
    }
}

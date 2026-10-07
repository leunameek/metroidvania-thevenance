using System.Collections.Generic;
using UnityEngine;

public enum HandGesture { Strike, Swipe, PalmHold, Grab }

// One sampled hand for the recognizer: what HandGestureModel already knows plus the palm centre
// (landmark 9) in normalized image coordinates (x to the right of the image, y down).
public struct HandSample
{
    public bool present, open;
    public Vector2 palm;
    public HandSample(bool present, bool open, Vector2 palm) { this.present = present; this.open = open; this.palm = palm; }
}

// Turns the two tracked hands into the game's gestures. No MediaPipe/MonoBehaviour dependency:
// HandGestureTracker feeds one sample per camera result with a realtime timestamp.
//   Strike   - a hand closes into a fist (open -> closed for two samples): attack.
//   Guard    - both hands open, raised and still for guardSeconds: a held state, not an event.
//   Swipe    - an open hand sweeps sideways quickly: dodge, with the direction the player sees.
//   PalmHold - one open hand held still for the dwell time: interact.
//   Grab     - a fist held for grabSeconds after opening: take or confirm.
// Events stay pending until consumed or older than eventLifetime, so a gesture made while
// nothing listens never fires later. Each hold must be re-armed (hand opens or leaves).
public sealed class HandGestureRecognizerModel
{
    private sealed class HandTrack
    {
        public bool present, open, closedOnce;
        public int closedSamples;
        public float openSince = -1, closedSince = -1, stillSince = -1;
        public bool palmArmed = true, grabArmed;
        public readonly List<Vector3> history = new List<Vector3>(16); // x, y, time
    }

    public float GuardSeconds = .25f;
    public float GuardMaxY = .7f;
    public float StillSpeed = .35f;
    public float SwipeDistance = .17f;
    public float SwipeWindow = .32f;
    public float StrikeCooldown = .45f;
    public float SwipeCooldown = .6f;
    public float GrabSeconds = .7f;
    public float EventLifetime = .4f;
    // Viewer space mirrors the camera image (the player sees themselves as in a mirror).
    public bool MirrorX = true;

    private readonly HandTrack _left = new HandTrack(), _right = new HandTrack();
    private readonly float[] _eventTime = { -99, -99, -99, -99 };
    private float _now, _lastStrike = -99, _lastSwipe = -99, _guardSince = -1, _dwellSeconds = 1;

    public bool GuardHeld { get; private set; }
    public int LastSwipeDirection { get; private set; }
    public float PalmHoldProgress { get; private set; }
    public float GrabProgress { get; private set; }
    public bool AnyHandPresent => _left.present || _right.present;

    public void SetDwellSeconds(float seconds) { _dwellSeconds = Mathf.Max(.2f, seconds); }

    public void Sample(float time, HandSample left, HandSample right)
    {
        _now = time;
        Track(_left, left);
        Track(_right, right);
        DetectStrike(_left); DetectStrike(_right);
        DetectSwipe(_left); DetectSwipe(_right);
        DetectGuard();
        DetectHolds();
    }

    // Both hands lost (camera off or stale): every state drops, pending events stay to expire.
    public void MarkAbsent(float time)
    {
        Sample(time, default, default);
    }

    public bool Consume(HandGesture gesture, float now)
    {
        int i = (int)gesture;
        bool fresh = now - _eventTime[i] <= EventLifetime;
        _eventTime[i] = -99;
        return fresh;
    }

    public bool Peek(HandGesture gesture, float now) => now - _eventTime[(int)gesture] <= EventLifetime;

    // Drops pending events and demands a fresh gesture: used when a new turn or screen begins.
    public void ClearPending()
    {
        for (int i = 0; i < _eventTime.Length; i++) _eventTime[i] = -99;
        foreach (var hand in new[] { _left, _right })
        {
            hand.palmArmed = !hand.open;
            hand.grabArmed = false;
            hand.stillSince = -1;
        }
        PalmHoldProgress = GrabProgress = 0;
    }

    private void Fire(HandGesture gesture) { _eventTime[(int)gesture] = _now; }

    private void Track(HandTrack hand, HandSample sample)
    {
        if (!sample.present)
        {
            hand.present = hand.open = false;
            hand.closedSamples = 0; hand.closedOnce = false;
            hand.openSince = hand.closedSince = hand.stillSince = -1;
            hand.palmArmed = true; hand.grabArmed = false;
            hand.history.Clear();
            return;
        }
        hand.present = true;
        hand.open = sample.open;
        if (sample.open)
        {
            if (hand.openSince < 0) hand.openSince = _now;
            hand.closedSince = -1; hand.closedSamples = 0; hand.closedOnce = false;
            hand.grabArmed = true;
        }
        else
        {
            hand.openSince = -1;
            if (hand.closedSince < 0) hand.closedSince = _now;
            hand.closedSamples++;
            hand.palmArmed = true;
        }
        hand.history.Add(new Vector3(sample.palm.x, sample.palm.y, _now));
        while (hand.history.Count > 0 && _now - hand.history[0].z > .5f) hand.history.RemoveAt(0);
        float speed = Speed(hand, .2f);
        if (!sample.open || speed > StillSpeed) hand.stillSince = -1;
        else if (hand.stillSince < 0) hand.stillSince = _now;
    }

    // Palm speed (normalized image widths per second) over the last window seconds.
    private float Speed(HandTrack hand, float window)
    {
        if (hand.history.Count < 2) return 0;
        Vector3 last = hand.history[hand.history.Count - 1];
        for (int i = hand.history.Count - 2; i >= 0; i--)
        {
            Vector3 first = hand.history[i];
            if (last.z - first.z >= window || i == 0)
            {
                float dt = Mathf.Max(.016f, last.z - first.z);
                return new Vector2(last.x - first.x, last.y - first.y).magnitude / dt;
            }
        }
        return 0;
    }

    // Open -> fist, confirmed on the second closed sample so one noisy frame never attacks.
    private void DetectStrike(HandTrack hand)
    {
        if (!hand.present || hand.open || hand.closedSamples != 2 || hand.closedOnce) return;
        // grabArmed: the fist comes from an open hand, not a hand that entered the frame closed.
        if (hand.grabArmed && _now - _lastStrike >= StrikeCooldown)
        {
            _lastStrike = _now;
            Fire(HandGesture.Strike);
        }
        hand.closedOnce = true;
    }

    private void DetectSwipe(HandTrack hand)
    {
        if (!hand.present || !hand.open || hand.history.Count < 3 || _now - _lastSwipe < SwipeCooldown) return;
        Vector3 last = hand.history[hand.history.Count - 1];
        for (int i = hand.history.Count - 2; i >= 0; i--)
        {
            Vector3 first = hand.history[i];
            if (last.z - first.z > SwipeWindow) break;
            float dx = last.x - first.x, dy = last.y - first.y;
            if (Mathf.Abs(dx) >= SwipeDistance && Mathf.Abs(dx) > Mathf.Abs(dy) * 2f)
            {
                _lastSwipe = _now;
                LastSwipeDirection = (dx > 0 ? 1 : -1) * (MirrorX ? -1 : 1);
                Fire(HandGesture.Swipe);
                hand.history.Clear();
                hand.stillSince = -1;
                return;
            }
        }
    }

    private void DetectGuard()
    {
        bool pose = _left.present && _right.present && _left.open && _right.open
            && Palm(_left).y < GuardMaxY && Palm(_right).y < GuardMaxY
            && Speed(_left, .2f) < StillSpeed * 1.6f && Speed(_right, .2f) < StillSpeed * 1.6f;
        if (!pose) { _guardSince = -1; GuardHeld = false; return; }
        if (_guardSince < 0) _guardSince = _now;
        GuardHeld = _now - _guardSince >= GuardSeconds;
    }

    private static Vector2 Palm(HandTrack hand)
    {
        Vector3 last = hand.history.Count > 0 ? hand.history[hand.history.Count - 1] : Vector3.one;
        return new Vector2(last.x, last.y);
    }

    // Holds belong to one hand; the guard pose (two open hands) never counts as a palm hold.
    private void DetectHolds()
    {
        float palm = 0, grab = 0;
        bool twoOpen = _left.present && _right.present && _left.open && _right.open;
        foreach (var hand in new[] { _left, _right })
        {
            if (!hand.present) continue;
            if (hand.open && hand.palmArmed && hand.stillSince >= 0 && !twoOpen)
            {
                float p = Mathf.Clamp01((_now - hand.stillSince) / _dwellSeconds);
                if (p >= 1) { Fire(HandGesture.PalmHold); hand.palmArmed = false; hand.stillSince = -1; p = 0; }
                palm = Mathf.Max(palm, p);
            }
            if (!hand.open && hand.grabArmed && hand.closedSince >= 0)
            {
                float g = Mathf.Clamp01((_now - hand.closedSince) / GrabSeconds);
                if (g >= 1) { Fire(HandGesture.Grab); hand.grabArmed = false; g = 0; }
                grab = Mathf.Max(grab, g);
            }
        }
        PalmHoldProgress = palm; GrabProgress = grab;
    }
}

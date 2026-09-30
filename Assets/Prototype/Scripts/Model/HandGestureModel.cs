using System.Collections.Generic;
using UnityEngine;

// Pure open/closed-hand heuristic + delta-accumulate/freeze-on-close math for one tracked
// hand. No MediaPipe/MonoBehaviour dependency - HandGestureTracker converts each frame's
// landmark result into plain Vector2 points and feeds them in.
public class HandGestureModel
{
    private readonly float _openFingerMargin;

    private bool _hasReference;
    private Vector2 _lastPosition;
    private float _deltaXAccum;
    private float _deltaYAccum;

    private readonly List<Vector2> _points = new List<Vector2>(21);

    public bool IsPresent { get; private set; }
    public bool IsOpen { get; private set; }
    public IReadOnlyList<Vector2> Points => _points;

    public HandGestureModel(float openFingerMargin)
    {
        _openFingerMargin = openFingerMargin;
    }

    public void Update(IReadOnlyList<Vector2> landmarks)
    {
        if (landmarks == null || landmarks.Count < 21)
        {
            MarkAbsent();
            return;
        }

        IsPresent = true;
        Vector2 palm = landmarks[9];
        bool open = IsHandOpen(landmarks);

        if (_hasReference && open && IsOpen)
        {
            _deltaXAccum += palm.x - _lastPosition.x;
            _deltaYAccum += palm.y - _lastPosition.y;
        }
        _lastPosition = palm;
        _hasReference = true;
        IsOpen = open;

        _points.Clear();
        _points.AddRange(landmarks);
    }

    public void MarkAbsent()
    {
        IsPresent = false;
        IsOpen = false;
        _hasReference = false;
        _deltaXAccum = _deltaYAccum = 0f;
        _points.Clear();
    }

    public float ConsumeDeltaX()
    {
        float delta = _deltaXAccum;
        _deltaXAccum = 0f;
        return delta;
    }

    public float ConsumeDeltaY()
    {
        float delta = _deltaYAccum;
        _deltaYAccum = 0f;
        return delta;
    }

    private bool IsHandOpen(IReadOnlyList<Vector2> landmarks)
    {
        int extended = 0;
        if (IsFingerExtended(landmarks, 6, 8)) extended++;
        if (IsFingerExtended(landmarks, 10, 12)) extended++;
        if (IsFingerExtended(landmarks, 14, 16)) extended++;
        if (IsFingerExtended(landmarks, 18, 20)) extended++;
        return extended >= 3;
    }

    private bool IsFingerExtended(IReadOnlyList<Vector2> landmarks, int pipIndex, int tipIndex)
    {
        Vector2 wrist = landmarks[0];
        Vector2 pip = landmarks[pipIndex];
        Vector2 tip = landmarks[tipIndex];

        float wristToPip = Vector2.Distance(wrist, pip);
        float wristToTip = Vector2.Distance(wrist, tip);
        return wristToTip > wristToPip * _openFingerMargin;
    }
}

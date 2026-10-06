using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;

public class HandGestureTracker : MonoBehaviour
{
    [SerializeField] private HandLandmarkerRunner runner;
    [SerializeField] private float openFingerMargin = 1.2f;
    [SerializeField] private float connectionTimeout = 1f;

    public bool RightHandPresent => _rightHand.IsPresent;
    public bool LeftHandPresent => _leftHand.IsPresent;
    public bool RightHandOpen => _rightHand.IsOpen;
    public bool LeftHandOpen => _leftHand.IsOpen;
    public IReadOnlyList<Vector2> RightHandPoints => _rightHand.Points;
    public IReadOnlyList<Vector2> LeftHandPoints => _leftHand.Points;

    public bool IsConnected => Time.realtimeSinceStartup - _lastResultRealtime < connectionTimeout;
    // Strike, guard, swipe and holds built on both hands (see HandGestureRecognizerModel).
    public HandGestureRecognizerModel Gestures { get; } = new HandGestureRecognizerModel();

    private readonly object _lock = new object();
    private HandLandmarkerResult _latestResult;
    private bool _isStale;
    private float _lastResultRealtime = -999f;

    private HandGestureModel _rightHand;
    private HandGestureModel _leftHand;

    // Reused per-frame scratch buffers so converting MediaPipe's landmark list into plain
    // Vector2 points (what HandGestureModel expects) doesn't allocate a new List every frame.
    private readonly List<Vector2> _rightScratch = new List<Vector2>(21);
    private readonly List<Vector2> _leftScratch = new List<Vector2>(21);

    private bool _subscribed;

    private void Awake()
    {
        _rightHand = new HandGestureModel(openFingerMargin);
        _leftHand = new HandGestureModel(openFingerMargin);
    }

    private void OnDisable()
    {
        if (runner != null && _subscribed) runner.ResultUpdated -= OnResultUpdated;
        _subscribed = false;
        _rightHand.MarkAbsent();
        _leftHand.MarkAbsent();
        Gestures.MarkAbsent(Time.realtimeSinceStartup);
    }

    private void TryConnectToRunner()
    {
        if (_subscribed) return;

        if (runner == null) runner = FindFirstObjectByType<HandLandmarkerRunner>();
        if (runner == null) return;

        runner.ResultUpdated += OnResultUpdated;
        _subscribed = true;
    }

    private void OnResultUpdated(HandLandmarkerResult result)
    {
        lock (_lock)
        {
            result.CloneTo(ref _latestResult);
            _isStale = true;
        }
    }

    private void Update()
    {
        if (!_subscribed) TryConnectToRunner();

        bool hasNew;
        lock (_lock)
        {
            hasNew = _isStale;
            if (hasNew)
            {
                ProcessResult(_latestResult);
                _isStale = false;
            }
        }

        if (hasNew)
        {
            _lastResultRealtime = Time.realtimeSinceStartup;
            Gestures.Sample(_lastResultRealtime, Sample(_leftHand), Sample(_rightHand));
        }
        else if (!IsConnected)
        {
            _rightHand.MarkAbsent();
            _leftHand.MarkAbsent();
            if (Gestures.AnyHandPresent) Gestures.MarkAbsent(Time.realtimeSinceStartup);
        }
    }

    private static HandSample Sample(HandGestureModel hand) => new HandSample(hand.IsPresent, hand.IsOpen, hand.Palm);

    public bool ConsumeGesture(HandGesture gesture) => Gestures.Consume(gesture, Time.realtimeSinceStartup);

    // Open-hand movement of either hand since the last call (x right, y down in image space).
    public Vector2 ConsumeAnyHandDelta() => _leftHand.ConsumeDelta() + _rightHand.ConsumeDelta();

    public float ConsumeRightHandDeltaY() => _rightHand.ConsumeDeltaY();

    public float ConsumeLeftHandDeltaX() => _leftHand.ConsumeDeltaX();

    private void ProcessResult(HandLandmarkerResult result)
    {
        bool sawRight = false;
        bool sawLeft = false;

        if (result.handedness != null && result.handLandmarks != null)
        {
            int count = Mathf.Min(result.handedness.Count, result.handLandmarks.Count);
            for (int i = 0; i < count; i++)
            {
                List<Category> categories = result.handedness[i].categories;
                if (categories == null || categories.Count == 0) continue;

                string label = categories[0].categoryName ?? categories[0].displayName;
                List<NormalizedLandmark> landmarks = result.handLandmarks[i].landmarks;
                if (landmarks == null || landmarks.Count < 21) continue;

                if (label == "Right")
                {
                    sawRight = true;
                    FillPoints(_rightScratch, landmarks);
                    _rightHand.Update(_rightScratch);
                }
                else if (label == "Left")
                {
                    sawLeft = true;
                    FillPoints(_leftScratch, landmarks);
                    _leftHand.Update(_leftScratch);
                }
            }
        }

        if (!sawRight) _rightHand.MarkAbsent();
        if (!sawLeft) _leftHand.MarkAbsent();
    }

    private static void FillPoints(List<Vector2> points, List<NormalizedLandmark> landmarks)
    {
        points.Clear();
        foreach (NormalizedLandmark landmark in landmarks) points.Add(new Vector2(landmark.x, landmark.y));
    }
}

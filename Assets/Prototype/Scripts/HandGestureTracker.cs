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

    public bool RightHandPresent { get; private set; }
    public bool LeftHandPresent { get; private set; }

    public bool IsConnected => Time.realtimeSinceStartup - _lastResultRealtime < connectionTimeout;

    private readonly object _lock = new object();
    private HandLandmarkerResult _latestResult;
    private bool _isStale;
    private float _lastResultRealtime = -999f;

    private bool _hasRightReference;
    private Vector2 _rightLastPosition;
    private float _rightDeltaYAccum;

    private bool _hasLeftReference;
    private Vector2 _leftLastPosition;
    private float _leftDeltaXAccum;

    private readonly List<Vector2> _rightPoints = new List<Vector2>(21);
    private readonly List<Vector2> _leftPoints = new List<Vector2>(21);
    
    public IReadOnlyList<Vector2> RightHandPoints => _rightPoints;
    public IReadOnlyList<Vector2> LeftHandPoints => _leftPoints;

    private bool _subscribed;

    private void OnDisable()
    {
        if (runner != null && _subscribed) runner.ResultUpdated -= OnResultUpdated;
        _subscribed = false;
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

        if (hasNew) _lastResultRealtime = Time.realtimeSinceStartup;
    }
    
    public float ConsumeRightHandDeltaY()
    {
        float delta = _rightDeltaYAccum;
        _rightDeltaYAccum = 0f;
        return delta;
    }
    
    public float ConsumeLeftHandDeltaX()
    {
        float delta = _leftDeltaXAccum;
        _leftDeltaXAccum = 0f;
        return delta;
    }

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

                Vector2 palm = new Vector2(landmarks[9].x, landmarks[9].y);
                bool open = IsHandOpen(landmarks);

                if (label == "Right")
                {
                    sawRight = true;
                    RightHandPresent = true;

                    if (_hasRightReference && open) _rightDeltaYAccum += palm.y - _rightLastPosition.y;
                    _rightLastPosition = palm;
                    _hasRightReference = true;

                    _rightPoints.Clear();
                    foreach (NormalizedLandmark landmark in landmarks) _rightPoints.Add(new Vector2(landmark.x, landmark.y));
                }
                else if (label == "Left")
                {
                    sawLeft = true;
                    LeftHandPresent = true;

                    if (_hasLeftReference && open) _leftDeltaXAccum += palm.x - _leftLastPosition.x;
                    _leftLastPosition = palm;
                    _hasLeftReference = true;

                    _leftPoints.Clear();
                    foreach (NormalizedLandmark landmark in landmarks) _leftPoints.Add(new Vector2(landmark.x, landmark.y));
                }
            }
        }

        if (!sawRight)
        {
            RightHandPresent = false;
            _hasRightReference = false;
            _rightPoints.Clear();
        }

        if (!sawLeft)
        {
            LeftHandPresent = false;
            _hasLeftReference = false;
            _leftPoints.Clear();
        }
    }

    private bool IsHandOpen(List<NormalizedLandmark> landmarks)
    {
        int extended = 0;
        if (IsFingerExtended(landmarks, 6, 8)) extended++;
        if (IsFingerExtended(landmarks, 10, 12)) extended++;
        if (IsFingerExtended(landmarks, 14, 16)) extended++;
        if (IsFingerExtended(landmarks, 18, 20)) extended++;
        return extended >= 3;
    }

    private bool IsFingerExtended(List<NormalizedLandmark> landmarks, int pipIndex, int tipIndex)
    {
        Vector2 wrist = new Vector2(landmarks[0].x, landmarks[0].y);
        Vector2 pip = new Vector2(landmarks[pipIndex].x, landmarks[pipIndex].y);
        Vector2 tip = new Vector2(landmarks[tipIndex].x, landmarks[tipIndex].y);

        float wristToPip = Vector2.Distance(wrist, pip);
        float wristToTip = Vector2.Distance(wrist, tip);
        return wristToTip > wristToPip * openFingerMargin;
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum HandState { Inactive, Searching, Detected, GestureStarted, Confirmed, Lost, Selected, CalibrationRequired }
    public sealed class HandTrackingUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly RectTransform _cursor;
        private readonly TMP_Text _caption;
        private readonly Image _progress;
        private Vector2 _position = new Vector2(.5f,.5f), _velocity;
        private float _held, _lastSeen = -99, _next;
        // After a selection the hand must close or leave before the next hold counts.
        private bool _armed = true;
        private AnalyzableObject _target;
        public HandState State { get; private set; }
        public string StatusText => UIStrings.Get("hands.state." + State);
        public float GestureProgress => Mathf.Clamp01(_held / _ui.Settings.Values.dwellSeconds);
        public Vector2 Position => _position;
        public event Action Changed;
        public HandTrackingUIController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            _cursor = f.Rect("UI_Cursor_Hands", ui.Root, Vector2.zero, Vector2.zero);
            _cursor.sizeDelta = new Vector2(64,64); _cursor.pivot = Vector2.one * .5f;
            _cursor.gameObject.AddComponent<Image>().color = f.Theme.gold;
            // The caption floats over the camera image and the world, so it carries its own dark plate.
            var captionPlate = f.HudPanel("CaptionPlate", _cursor, new Vector2(-1.4f,-.95f), new Vector2(2.4f,-.1f));
            _caption = f.Label(captionPlate, "", new Vector2(.04f,.08f), new Vector2(.96f,.92f), 18); _caption.alignment = TextAlignmentOptions.Center;
            _progress = f.Bar(_cursor, "Gesture", new Vector2(0,0), new Vector2(1,.12f));
            _cursor.gameObject.SetActive(false);
        }
        private void Set(HandState state) { if (State == state) return; State = state; Changed?.Invoke(); }
        public void Stop() { if (_ui.Demo != null && _ui.Demo.Hands != null) _ui.Demo.Hands.Deactivate(); _held = 0; Set(HandState.Inactive); }
        public void Tick()
        {
            var d = _ui.Demo; var s = _ui.Settings.Values; var tracker = d.Hands.Tracker;
            bool calibration = _ui.Screens.Current == UIScreen.HandCalibration;
            bool canTrack = d.Hands.Requested && (calibration || _ui.Screens.Current == UIScreen.None && !d.MouseMode
                && (d.State == TechnicalDemoState.Exploration || d.State == TechnicalDemoState.Analyzing));
            IReadOnlyList<Vector2> points = tracker == null ? null : s.leftHand ? tracker.LeftHandPoints : tracker.RightHandPoints;
            bool present = tracker != null && d.Hands.Live && (s.leftHand ? tracker.LeftHandPresent : tracker.RightHandPresent) && points != null && points.Count > 8;
            // Something to use nearby: a station, an open or closed portal, or the training circle.
            bool interactable = d.State == TechnicalDemoState.Exploration && d.HasInteraction;
            bool visible = canTrack && present && s.handCursor && (calibration || interactable);
            _cursor.gameObject.SetActive(visible);
            if (!d.Hands.Requested) { _held = 0; Set(HandState.Inactive); return; }
            if (!canTrack) { _held = 0; return; }
            if (!present)
            { _held = 0; _armed = true; Set(Time.unscaledTime - _lastSeen < 5 ? HandState.Lost : HandState.Searching); return; }
            _lastSeen = Time.unscaledTime;
            Vector2 desired = new Vector2(1 - points[8].x, 1 - points[8].y);
            desired = Vector2.one * .5f + (desired - Vector2.one * .5f) * s.handSensitivity;
            desired.x = Mathf.Clamp(desired.x,.05f,.95f); desired.y = Mathf.Clamp(desired.y,.04f,.96f);
            if (Vector2.Distance(desired, _position) > s.handDeadZone)
                _position = Vector2.SmoothDamp(_position, desired, ref _velocity, s.handSmoothing, float.PositiveInfinity, Time.unscaledDeltaTime);
            _cursor.anchorMin = _cursor.anchorMax = _position; _cursor.anchoredPosition = Vector2.zero;
            _target = d.State == TechnicalDemoState.Exploration ? d.Nearby : null;
            bool open = s.leftHand ? tracker.LeftHandOpen : tracker.RightHandOpen;
            bool overTarget = false;
            if (_target != null && Camera.main != null)
            {
                Vector3 screen = Camera.main.WorldToViewportPoint(_target.transform.position);
                overTarget = screen.z > 0 && Vector2.Distance(_position, screen) < .1f;
                if (overTarget) _position = Vector2.Lerp(_position, screen, .08f);
            }
            // Pointing at the piece is not required: an open palm held near it is enough (the
            // cursor still snaps to the piece it is over).
            if (!open) _armed = true;
            if ((interactable || calibration) && open && _armed) _held += Time.unscaledDeltaTime;
            else _held = 0;
            Set(_held > 0 ? HandState.GestureStarted : HandState.Detected);
            UIFactory.Fill(_progress, GestureProgress);
            if (_held >= s.dwellSeconds)
            {
                Set(HandState.Confirmed); _held = 0; _armed = calibration;
                if (interactable && !d.HelpOpen && !d.MouseMode) { d.Interact(); Set(HandState.Selected); }
            }
            if (Time.unscaledTime > _next) { _next = Time.unscaledTime + .15f; _caption.text = s.handGuide ? StatusText : ""; _caption.transform.parent.gameObject.SetActive(_caption.text.Length > 0); }
        }
        public void Dispose() { Stop(); }
    }
}

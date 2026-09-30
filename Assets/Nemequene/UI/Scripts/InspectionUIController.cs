using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class InspectionUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly GameObject _root;
        private readonly TMP_Text _name, _instruction, _progressText;
        private readonly Button _close;
        private readonly Image _progress;
        private AnalyzableObject _selected;
        private float _next, _zoom = 1;
        private Vector3 _cameraAnchor, _focus;
        public InspectionUIController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            _root = f.Panel("UI_Panel_ObjectInspection", ui.Root, new Vector2(.65f,.18f), new Vector2(.95f,.78f), true).gameObject;
            // Pixel insets from the panel edges: the stone frame's corners and crest stay clear of text.
            var body = f.Scroll(_root.transform, Vector2.zero, Vector2.one);
            ((RectTransform)body.parent.parent).Inset(60, 170, 60, 80);
            _name = f.Caption(body, "", 34); _instruction = f.Text(body, "", 24);
            _progressText = UIFactory.Tone(f.Text(_root.transform, "", 20), UITone.Gold);
            _progressText.rectTransform.Band(128, 30, 64);
            _progress = f.Bar(_root.transform, "LessonProgress", Vector2.zero, Vector2.one);
            ((RectTransform)_progress.transform.parent.parent).Band(108, 16, 64);
            var bottom = f.Column(_root.transform, "Close"); bottom.Band(40, 58, 60);
            _close = f.Button(bottom, UIStrings.Get("inspection.close"), () => ui.Demo.EndAnalysis());
            ui.Demo.ViewChanged += Refresh; ui.Screens.Changed += OnScreen; Refresh();
        }
        private void OnScreen(UIScreen screen) { Refresh(); }
        private void Refresh()
        {
            bool show = _ui.Demo.State == TechnicalDemoState.Analyzing && _ui.Screens.Current == UIScreen.None;
            _root.SetActive(show);
        }
        public void Tick()
        {
            if (!_root.activeSelf || _ui.Demo.Selected == null) return;
            var item = _ui.Demo.Selected;
            if (_selected != item)
            {
                _selected = item; _zoom = 1;
                _name.text = item.Data.displayName;
                _cameraAnchor = Vector3.zero;
            }
            // Zoom operates on the existing inspection camera and never changes the cultural model.
            if (_cameraAnchor == Vector3.zero && _ui.Demo.Lesson != null)
            {
                _focus = item.transform.position + Vector3.right * .65f;
                _cameraAnchor = _focus + new Vector3(0,.7f,-3.8f);
            }
            // The camera is fixed: the wheel no longer zooms the inspection view.
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + .1f;
            var lesson = _ui.Demo.Lesson;
            ((RectTransform)_root.transform).anchorMin = new Vector2(.65f,.78f - (lesson.Complete ? .31f : .40f) * _ui.Settings.Values.textScale);
            _instruction.text = lesson.Complete ? UIStrings.Get("inspection.archived") : UIStrings.Get("lesson." + lesson.Lesson) + "\n\n"
                + UIStrings.Get((_ui.Demo.MouseMode ? "inspection.mouse." : "inspection.hands.") + lesson.Lesson);
            if (!_ui.Demo.MouseMode && !lesson.Complete && _ui.Hands != null && _ui.Hands.State != HandState.Detected)
                _instruction.text += "\n\n" + _ui.Hands.StatusText + "\n" + UIStrings.Get("inspection.fallback");
            _progressText.text = lesson.Complete ? UIStrings.Get("inspection.complete") : UIStrings.Get("inspection.progress", Mathf.RoundToInt(lesson.Progress * 100));
            _close.GetComponentInChildren<TMP_Text>().text = UIStrings.Get(lesson.Complete ? "inspection.return" : "inspection.close");
            UIFactory.Fill(_progress, lesson.Progress);
        }
        public static string CulturalDescription(AnalyzableObjectData data)
        {
            bool validated = data.evidenceStatus == CulturalEvidenceStatus.HistoricalValidated;
            return (validated ? UIStrings.Get("culture.heading") + "\n" + data.sourceOrValidationNote + "\n\n" : "")
                + UIStrings.Get("fiction.heading") + "\n" + data.description
                + (validated ? "" : "\n" + data.sourceOrValidationNote);
        }
        public void Dispose() { if (_ui.Demo != null) _ui.Demo.ViewChanged -= Refresh; _ui.Screens.Changed -= OnScreen; }
    }
}

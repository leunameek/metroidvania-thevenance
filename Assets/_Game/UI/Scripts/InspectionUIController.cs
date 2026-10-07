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
        private readonly TMP_Text _name, _instruction, _progressText, _heading;
        private readonly RectTransform _panel, _body;
        private readonly Button _close;
        private readonly Image _progress;
        private AnalyzableObject _selected;
        private float _next, _zoom = 1;
        private string _closeText;
        private Vector3 _cameraAnchor, _focus;
        public InspectionUIController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            // Screens 10–12: lesson title in the upper left, the 3D piece keeps the left of the view,
            // the lesson panel sits on the right with name, step, instruction, progress and actions.
            _root = f.Rect("UI_ObjectInspection", ui.Root, Vector2.zero, Vector2.one).gameObject;
            var veil = _root.AddComponent<TitleMenuBackdrop>(); veil.raycastTarget = false; veil.fromTop = true; veil.readingEdge = .30f; veil.strength = .80f;
            // The lesson title sits over the sky and the trees: a smoked backdrop behind it.
            var scrim = UIKit.Scrim(_root.transform, "TitleScrim", new Vector2(0, 1), new Vector2(0, 1), .6f);
            UIKit.Place(scrim.rectTransform, new Vector2(0, 1), new Vector2(20, -40), new Vector2(1240, 170));
            _heading = UIFactory.Shadow(f.Heading(_root.transform, "", new Vector2(.04f, .865f), new Vector2(.62f, .95f), 56));
            _heading.alignment = TextAlignmentOptions.BottomLeft;
            var subtitle = UIFactory.Shadow(f.Label(_root.transform, UIStrings.Get("inspection.subtitle"), new Vector2(.04f, .825f), new Vector2(.62f, .865f), 22));
            subtitle.alignment = TextAlignmentOptions.TopLeft;
            f.Hint(_root.transform, UIStrings.Get("footer.lesson"), Vector2.zero);
            _panel = f.Panel("UI_Panel_ObjectInspection", _root.transform, new Vector2(.60f,.13f), new Vector2(.94f,.78f), true);
            // Pixel insets from the panel edges: the rim, the emblem and the feather stay clear of text.
            var body = f.Scroll(_panel, Vector2.zero, Vector2.one); _body = body;
            ((RectTransform)body.parent.parent).Inset(60, 196, 60, 76);
            _name = f.Caption(body, "", 44);
            _progressText = UIFactory.Tone(f.Text(body, "", 20), UITone.Success); _progressText.fontStyle = FontStyles.UpperCase;
            _progressText.characterSpacing = 2;
            _instruction = f.Text(body, "", 26);
            _progress = f.Bar(_panel, "LessonProgress", Vector2.zero, Vector2.one);
            ((RectTransform)_progress.transform.parent.parent).Band(132, 18, 64);
            var bottom = f.Column(_panel, "Close"); bottom.Band(48, 60, 60);
            _close = f.Button(bottom, UIStrings.Get("inspection.close", VoicePrompt.Cap("salir", "Esc")), () => ui.Demo.EndAnalysis());
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
                _heading.text = UIStrings.Get("inspection.heading", item.Data.displayName);
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
            _instruction.text = lesson.Complete ? UIStrings.Get("inspection.archived") : UIStrings.Get("lesson." + lesson.Lesson) + "\n\n"
                + UIStrings.Get((_ui.Demo.MouseMode ? "inspection.mouse." : "inspection.hands.") + lesson.Lesson);
            if (lesson.Complete && !_ui.Demo.MouseMode && _ui.Demo.Hands.Live)
                _instruction.text += "\n\n" + UIStrings.Get("inspection.handsReturn");
            if (!_ui.Demo.MouseMode && !lesson.Complete && _ui.Hands != null && _ui.Hands.State != HandState.Detected)
                _instruction.text += "\n\n" + _ui.Hands.StatusText + "\n" + UIStrings.Get("inspection.fallback");
            _progressText.text = lesson.Complete ? UIStrings.Get("inspection.complete") : UIStrings.Get("inspection.progress", Mathf.RoundToInt(lesson.Progress * 100));
            // Set only when it changes: the key-cap helper splits the label once per change, so
            // rewriting it each tick showed the key twice («Cerrar · Esc» beside its cap).
            string close = lesson.Complete ? UIStrings.Get("inspection.return", VoicePrompt.Cap("volver", "E"))
                : UIStrings.Get("inspection.close", VoicePrompt.Cap("salir", "Esc"));
            if (close != _closeText) { _closeText = close; _close.GetComponentInChildren<TMP_Text>().text = close; }
            UIFactory.Fill(_progress, lesson.Progress);
            FitPanel();
        }

        // The panel holds all its text without scrolling (2026-10-07 playtest: the text had to be
        // scrolled, before and after the lesson): it stands tall on the right, and if the text
        // still does not fit its viewport, the instruction shrinks until it does.
        private void FitPanel()
        {
            _panel.anchorMin = new Vector2(.60f, .10f);
            var viewport = (RectTransform)_body.parent;
            Canvas.ForceUpdateCanvases();
            float room = viewport.rect.height;
            if (room <= 0) return;
            for (int size = 26; size >= 17; size--)
            {
                _instruction.fontSize = size;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_body);
                if (LayoutUtility.GetPreferredHeight(_body) <= room) break;
            }
            _body.anchoredPosition = Vector2.zero;
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

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nemequene.UI
{
    public sealed class NotificationManager : IDisposable
    {
        private readonly UIManager _ui;
        private readonly GameObject _panel;
        private readonly TMP_Text _title, _text;
        private readonly UIIconGraphic _icon;
        private readonly Queue<string> _queue = new Queue<string>();
        private readonly CanvasGroup _group;
        private string _last;
        private float _until;
        public NotificationManager(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            // Reference toast: glyph, gold title and a short ivory description, top right under the device status.
            _panel = f.HudPanel("UI_Toast_Pooled", ui.Root, new Vector2(.66f,.765f), new Vector2(.95f,.875f)).gameObject;
            _icon = f.Icon(_panel.transform, UIIcon.Diamond, new Vector2(0,.5f), new Vector2(0,.5f), f.Theme.gold);
            _icon.rectTransform.sizeDelta = new Vector2(40,40); _icon.rectTransform.anchoredPosition = new Vector2(48,0);
            _title = f.Caption(_panel.transform, "", 20); _title.rectTransform.SetAnchors(new Vector2(.16f,.52f), new Vector2(.95f,.86f));
            _text = f.Label(_panel.transform, "", new Vector2(.16f,.12f), new Vector2(.95f,.52f), 20);
            _group = _panel.AddComponent<CanvasGroup>(); _group.blocksRaycasts = false;
            _panel.SetActive(false);
        }
        private void Present(string message)
        {
            int split = message.IndexOf(" · ", System.StringComparison.Ordinal);
            bool titled = split > 0 && split < 40;
            _title.gameObject.SetActive(titled);
            _title.text = titled ? message.Substring(0, split) : "";
            _text.text = titled ? message.Substring(split + 3) : message;
            _text.rectTransform.SetAnchors(new Vector2(.16f,.12f), new Vector2(.95f, titled ? .52f : .88f));
            _text.alignment = titled ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            bool saved = message == UIStrings.Get("save.saved") || message == UIStrings.Get("settings.saved");
            _icon.SetIcon(saved ? UIIcon.Check : UIIcon.Diamond);
            // Larger text grows the plate downwards instead of spilling over its frame.
            ((RectTransform)_panel.transform).anchorMin = new Vector2(.66f, .875f - .11f * _ui.Settings.Values.textScale);
        }
        public void Post(string text, bool replaceCurrent = false)
        {
            if (replaceCurrent) { _last = null; _until = 0; _queue.Clear(); }
            if (string.IsNullOrWhiteSpace(text) || (text == _last && Time.unscaledTime < _until) || _queue.Contains(text)) return;
            _queue.Clear(); _queue.Enqueue(text);
        }
        public void Tick()
        {
            bool visible = _ui.Screens.Current == UIScreen.None && _ui.Demo.State == TechnicalDemoState.Exploration && !_ui.ModalOpen && !(_ui.Dialogue?.Active ?? false);
            if (!visible) _until = 0;
            if (Time.unscaledTime >= _until && _queue.Count > 0 && visible)
            { _last = _queue.Dequeue(); Present(_last); _until = Time.unscaledTime + _ui.Settings.Values.noticeSeconds; }
            _panel.SetActive(visible && Time.unscaledTime < _until);
            if (_panel.activeSelf) _group.alpha = _ui.Settings.Values.reducedMotion ? 1 : Mathf.Clamp01((_until - Time.unscaledTime) / .3f);
        }
        public void Dispose() { _queue.Clear(); }
    }
}

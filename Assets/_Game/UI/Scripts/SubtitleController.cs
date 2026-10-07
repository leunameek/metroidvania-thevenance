using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class SubtitleController
    {
        private readonly UIManager _ui;
        private readonly GameObject _root;
        private readonly TMP_Text _text;
        private readonly Image _background;
        private float _remaining, _minimumSeconds;
        private int _page;
        private float _layoutFontSize;
        private Vector2 _layoutSize;
        public bool Active => _root.activeSelf && _remaining > 0;
        public SubtitleController(UIManager ui)
        {
            _ui = ui; _root = ui.Factory.HudPanel("UI_Subtitle", ui.Root, new Vector2(.18f,.04f), new Vector2(.82f,.20f)).gameObject;
            _background = _root.GetComponent<Image>(); _background.GetComponent<UIStyleBinding>().surface = false;
            _text = ui.Factory.Label(_root.transform, "", new Vector2(.04f,.08f), new Vector2(.96f,.92f), 28);
            _text.alignment = TextAlignmentOptions.Center; _text.overflowMode = TextOverflowModes.Page;
            _text.GetComponent<UIStyleBinding>().subtitle = true;
            _root.SetActive(false);
        }
        public void Show(string speaker, string text, float seconds = 5, bool sound = false)
        {
            var s = _ui.Settings.Values;
            if (!s.subtitles || string.IsNullOrEmpty(text) || (sound && !s.soundCaptions)) return;
            _text.text = (s.speakerNames && !string.IsNullOrEmpty(speaker) ? speaker + ": " : "") + text;
            _root.SetActive(true); PreparePages(); _page = 1;
            _minimumSeconds = Mathf.Max(2, seconds / Mathf.Max(1,_text.textInfo.pageCount)); BeginPage();
        }
        private void PreparePages()
        {
            _text.fontSize = 28 * _ui.Settings.Values.subtitleScale;
            Canvas.ForceUpdateCanvases();
            // Bound the text box to three lines; larger type naturally uses fewer lines per page.
            var rect = _text.rectTransform;
            rect.anchorMin = new Vector2(.04f,.5f); rect.anchorMax = new Vector2(.96f,.5f);
            rect.offsetMin = new Vector2(0,-Mathf.Min(72, _text.fontSize * 1.8f));
            rect.offsetMax = new Vector2(0,Mathf.Min(72, _text.fontSize * 1.8f));
            _text.ForceMeshUpdate(); _layoutFontSize = _text.fontSize;
            _layoutSize = ((RectTransform)_root.transform).rect.size;
        }
        private void BeginPage()
        {
            _text.pageToDisplay = _page;
            var page = _text.textInfo.pageInfo[_page - 1];
            _remaining = Mathf.Max(_minimumSeconds, (page.lastCharacterIndex - page.firstCharacterIndex + 1) / 14f);
        }
        public void Tick()
        {
            bool show = _ui.Settings.Values.subtitles && _remaining > 0 && _ui.Screens.Current == UIScreen.None && !_ui.ModalOpen && !_ui.Dialogue.Active;
            _root.SetActive(show);
            if (!show) return;
            if (!Mathf.Approximately(_layoutFontSize,28 * _ui.Settings.Values.subtitleScale) || _layoutSize != ((RectTransform)_root.transform).rect.size)
            {
                PreparePages(); _page = Mathf.Clamp(_page,1,Mathf.Max(1,_text.textInfo.pageCount)); BeginPage();
            }
            _background.color = new Color(.086f,.075f,.059f,_ui.Settings.Values.subtitleOpacity);
            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0 && _page < _text.textInfo.pageCount) { _page++; BeginPage(); }
        }
    }
}

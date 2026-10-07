using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class DialogueUIController
    {
        private readonly UIManager _ui;
        private readonly GameObject _panel;
        private readonly TMP_Text _speaker, _text, _pageLabel;
        private readonly Button _next;
        private readonly GameObject _speakerPlate;
        private DialogueData _data;
        private GameObject _previousFocus;
        private int _line, _page = 1, _pageEnd, _lastAdvanceFrame = -1;
        private float _revealed, _readTime, _layoutFontSize;
        private Vector2 _layoutSize;
        public bool Active => _data != null;
        public DialogueUIController(UIManager ui)
        {
            _ui = ui;
            _panel = ui.Factory.Panel("UI_Dialogue", ui.Root, new Vector2(.14f,.04f), new Vector2(.86f,.34f), true, true, false).gameObject;
            // Screen 23 «Diálogo»: the speaker's name on a short crimson ribbon across the top rim;
            // two lines per segment, the scene stays visible above.
            var plate = ui.Factory.Rect("SpeakerPlate", _panel.transform, new Vector2(0,1), new Vector2(0,1));
            plate.pivot = new Vector2(0,.5f); plate.sizeDelta = new Vector2(380,60); plate.anchoredPosition = new Vector2(72,-6);
            UIBacata.Skin(plate, "Controls/Ribbon");
            _speakerPlate = plate.gameObject;
            _speaker = ui.Factory.Heading(plate, "", Vector2.zero, Vector2.one, 26, UITone.GoldLight);
            _speaker.margin = new Vector4(20,4,20,4);
            _text = UIFactory.Tone(ui.Factory.Label(_panel.transform, "", new Vector2(.06f,.30f), new Vector2(.94f,.82f), 28), UITone.Ink);
            _text.overflowMode = TextOverflowModes.Page;
            _pageLabel = UIFactory.Tone(ui.Factory.Label(_panel.transform, "", new Vector2(.06f,.22f), new Vector2(.94f,.29f), 18), UITone.InkMuted);
            _pageLabel.alignment = TextAlignmentOptions.MidlineRight;
            var controls = ui.Factory.Rect("DialogueActions", _panel.transform, new Vector2(.09f,.09f), new Vector2(.91f,.21f));
            var row = controls.gameObject.AddComponent<HorizontalLayoutGroup>(); row.spacing = 24;
            row.childControlHeight = row.childControlWidth = true; row.childForceExpandWidth = true;
            _next = ui.Factory.Button(controls, UIStrings.Get("dialogue.next"), Next, true);
            ui.Factory.Button(controls, UIStrings.Get("dialogue.skip"), Close);
            UIFactory.CenterAll(controls);
            _panel.SetActive(false);
        }
        public void Show(DialogueData data)
        {
            if (data == null || data.lines == null || data.lines.Length == 0 || !_ui.SessionStarted ||
                _ui.Screens.Current != UIScreen.None || _ui.ModalOpen || _ui.Demo.State != TechnicalDemoState.Exploration) return;
            foreach (var line in data.lines) if (line == null || string.IsNullOrEmpty(line.textKey)) return;
            _previousFocus = EventSystem.current?.currentSelectedGameObject;
            _data = data; _line = 0; _panel.SetActive(true); _ui.RefreshPause(); RenderLine();
            EventSystem.current?.SetSelectedGameObject(_next.gameObject);
        }
        private void RenderLine()
        {
            var line = _data.lines[_line];
            _speaker.text = UIStrings.Get(line.speakerKey);
            _speakerPlate.SetActive(_ui.Settings.Values.speakerNames);
            _text.text = UIStrings.Get(line.textKey); _page = 1;
            _text.fontSize = 28 * _ui.Settings.Values.textScale;
            Canvas.ForceUpdateCanvases(); _text.ForceMeshUpdate(); BeginPage();
        }
        private void BeginPage()
        {
            _text.pageToDisplay = _page;
            var page = _text.textInfo.pageInfo[_page - 1];
            _pageEnd = page.lastCharacterIndex + 1;
            _revealed = _ui.Settings.Values.dialogueInstant ? _pageEnd : page.firstCharacterIndex;
            _text.maxVisibleCharacters = Mathf.FloorToInt(_revealed);
            _layoutFontSize = _text.fontSize; _layoutSize = _text.rectTransform.rect.size;
            _readTime = Mathf.Max(2, (_pageEnd - page.firstCharacterIndex) / 14f);
            _pageLabel.text = UIStrings.Get("dialogue.page", _page, Mathf.Max(1,_text.textInfo.pageCount));
            _pageLabel.gameObject.SetActive(_text.textInfo.pageCount > 1);
        }
        public void Next()
        {
            if (!Active || _ui.Screens.Current != UIScreen.None || _ui.ModalOpen || _lastAdvanceFrame == Time.frameCount) return;
            _lastAdvanceFrame = Time.frameCount;
            // EventSystem and the keyboard adapter can both submit in the same frame.
            if (_revealed < _pageEnd) { _revealed = _pageEnd; _text.maxVisibleCharacters = _pageEnd; return; }
            if (_page < _text.textInfo.pageCount) { _page++; BeginPage(); }
            else if (++_line >= _data.lines.Length) Close();
            else RenderLine();
        }
        public void Close()
        {
            _data = null; _panel.SetActive(false); _ui.RefreshPause();
            if (_previousFocus != null && _previousFocus.activeInHierarchy)
                EventSystem.current?.SetSelectedGameObject(_previousFocus);
            else EventSystem.current?.SetSelectedGameObject(null);
        }
        public void Tick()
        {
            if (!Active) return;
            bool visible = _ui.Screens.Current == UIScreen.None && !_ui.ModalOpen;
            bool wasVisible = _panel.activeSelf; _panel.SetActive(visible);
            if (!visible) return;
            if (!wasVisible) EventSystem.current?.SetSelectedGameObject(_next.gameObject);
            var settings = _ui.Settings.Values;
            if (!Mathf.Approximately(_layoutFontSize,_text.fontSize) || _layoutSize != _text.rectTransform.rect.size)
            {
                _text.ForceMeshUpdate(); _page = Mathf.Clamp(_page,1,Mathf.Max(1,_text.textInfo.pageCount)); BeginPage();
            }
            _speakerPlate.SetActive(settings.speakerNames);
            if (_revealed < _pageEnd)
            {
                _revealed = settings.dialogueInstant ? _pageEnd : Mathf.Min(_pageEnd, _revealed + Time.unscaledDeltaTime * settings.dialogueSpeed);
                _text.maxVisibleCharacters = Mathf.FloorToInt(_revealed);
            }
            else if (settings.dialogueAuto)
            {
                _readTime -= Time.unscaledDeltaTime;
                if (_readTime <= 0) Next();
            }
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) Next();
        }
    }
}

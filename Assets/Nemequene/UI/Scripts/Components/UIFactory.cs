using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class UIFactory
    {
        public readonly UITheme Theme;
        private readonly Sprite _stonePanel, _parchmentPanel, _stoneButton;
        public UIFactory(UITheme theme)
        {
            Theme = theme;
            _stonePanel = theme.stonePanel != null ? theme.stonePanel : FirstSprite("Nemequene/StonePanel-v2");
            _parchmentPanel = theme.parchmentPanel != null ? theme.parchmentPanel : FirstSprite("Nemequene/ParchmentPanel-v2");
            _stoneButton = theme.stoneButton != null ? theme.stoneButton : FirstSprite("Nemequene/Menu_StoneButton");
        }
        private static Sprite FirstSprite(string path)
        {
            var sprites = Resources.LoadAll<Sprite>(path);
            return sprites.Length > 0 ? sprites[0] : null;
        }
        public RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 inset = default)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = inset; rect.offsetMax = -inset; return rect;
        }
        public RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, bool blocks = false)
        {
            var r = Rect(name, parent, min, max);
            var image = r.gameObject.AddComponent<Image>(); image.color = Theme.panel; image.raycastTarget = blocks;
            if (_stonePanel != null)
            { image.sprite = _stonePanel; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 3.2f; image.color = Color.white; }
            r.gameObject.AddComponent<UIStyleBinding>().surface = true;
            Frame(r, false).gameObject.SetActive(_stonePanel == null);
            return r;
        }
        // Tints the stone sprite (e.g. ceremonial red for the defeat screen) and keeps it through accessibility refreshes.
        public static void Tint(RectTransform panel, Color tint)
        {
            var binding = panel.GetComponent<UIStyleBinding>(); if (binding != null) binding.tint = tint;
            var image = panel.GetComponent<Image>(); if (image != null && image.sprite != null) image.color = tint;
        }
        public RectTransform Parchment(string name, Transform parent, Vector2 min, Vector2 max, bool blocks = false)
        {
            var r = Rect(name, parent, min, max);
            var image = r.gameObject.AddComponent<Image>(); image.raycastTarget = blocks;
            image.color = _parchmentPanel == null ? Theme.parchment : Color.white;
            if (_parchmentPanel != null)
            { image.sprite = _parchmentPanel; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 3.2f; }
            var binding = r.gameObject.AddComponent<UIStyleBinding>(); binding.surface = true; binding.parchment = true;
            Frame(r, false).gameObject.SetActive(_parchmentPanel == null);
            return r;
        }
        public RectTransform HudPanel(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var r = Rect(name, parent, min, max);
            var background = Theme.background; background.a = .86f;
            r.gameObject.AddComponent<Image>().color = background;
            var binding = r.gameObject.AddComponent<UIStyleBinding>(); binding.surface = true; binding.hud = true;
            Frame(r, true);
            return r;
        }
        public UIFrameGraphic Frame(Transform parent, bool compact)
        {
            var rect = Rect("Stonework", parent, Vector2.zero, Vector2.one);
            var frame = rect.gameObject.AddComponent<UIFrameGraphic>(); frame.compact = compact; frame.raycastTarget = false;
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return frame;
        }
        public UIPlateGraphic Plate(Transform parent, UIPlateKind kind, float chamfer = 10)
        {
            var rect = Rect("Plate", parent, Vector2.zero, Vector2.one);
            var plate = rect.gameObject.AddComponent<UIPlateGraphic>(); plate.kind = kind; plate.chamfer = chamfer; plate.raycastTarget = false;
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            rect.SetAsFirstSibling();
            return plate;
        }
        public UIIconGraphic Icon(Transform parent, UIIcon icon, Vector2 min, Vector2 max, Color color)
        {
            var rect = Rect("Icon_" + icon, parent, min, max);
            var graphic = rect.gameObject.AddComponent<UIIconGraphic>(); graphic.icon = icon; graphic.color = color; graphic.raycastTarget = false;
            return graphic;
        }
        // Icon inside a layout group, with a fixed square footprint.
        public UIIconGraphic Icon(Transform parent, UIIcon icon, float size, Color color)
        {
            var graphic = Icon(parent, icon, Vector2.zero, Vector2.one, color);
            var le = graphic.gameObject.AddComponent<LayoutElement>(); le.minWidth = le.preferredWidth = size; le.minHeight = le.preferredHeight = size;
            return graphic;
        }
        public RectTransform Divider(Transform parent, Vector2 min, Vector2 max)
        {
            var graphic = Icon(parent, UIIcon.Divider, min, max, Theme.gold);
            graphic.gameObject.name = "Divider";
            return graphic.rectTransform;
        }
        public RectTransform Column(Transform parent, string name, float spacing = 16)
        {
            var r = Rect(name, parent, Vector2.zero, Vector2.one);
            var layout = r.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing; layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return r;
        }
        public RectTransform Row(Transform parent, string name, float spacing = 24)
        {
            var r = Rect(name, parent, Vector2.zero, Vector2.one);
            var layout = r.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing; layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true; layout.childAlignment = TextAnchor.MiddleCenter;
            return r;
        }
        public RectTransform Scroll(Transform parent, Vector2 min, Vector2 max)
        {
            var root = Rect("Scroll", parent, min, max);
            var viewport = Rect("Viewport", root, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var content = Column(viewport, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1);
            // Room for the focus glow of the plates, which is drawn just outside each control.
            content.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(6, 6, 6, 6);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = root.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 48;
            return content;
        }
        public TMP_Text Text(Transform parent, string value, float size = 24, bool title = false)
        {
            var r = Rect("Text", parent, Vector2.zero, Vector2.one);
            var text = r.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Theme.titleFont; text.fontSize = size;
            text.color = Theme.ivory; text.text = value; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow;
            text.richText = false;
            var binding = r.gameObject.AddComponent<UIStyleBinding>(); binding.baseSize = size; binding.title = title;
            return text;
        }
        public TMP_Text Label(Transform parent, string value, Vector2 min, Vector2 max, float size = 24, bool title = false)
        {
            var text = Text(parent, value, size, title); text.rectTransform.anchorMin = min; text.rectTransform.anchorMax = max;
            return text;
        }
        public static TMP_Text Tone(TMP_Text text, UITone tone)
        {
            var binding = text.GetComponent<UIStyleBinding>(); if (binding != null) binding.tone = tone;
            text.color = UIStyleBinding.ToneColor(tone); return text;
        }
        // Carved capitals (Cinzel) for screen and section titles, as in the reference headers.
        public TMP_Text Display(TMP_Text text, UITone tone = UITone.Gold, float spacing = 4)
        {
            var binding = text.GetComponent<UIStyleBinding>(); binding.display = true; binding.title = true;
            text.font = Theme.Display; text.fontStyle = FontStyles.UpperCase; text.characterSpacing = spacing;
            // Long titles shrink before they wrap into the content below.
            text.enableAutoSizing = true; text.fontSizeMax = binding.baseSize; text.fontSizeMin = Mathf.Max(16, binding.baseSize * .6f);
            return Tone(text, tone);
        }
        public TMP_Text Heading(Transform parent, string value, Vector2 min, Vector2 max, float size = 48, UITone tone = UITone.Gold)
        {
            var text = Display(Label(parent, value, min, max, size, true), tone);
            text.alignment = TextAlignmentOptions.Center; return text;
        }
        // Heading inside a layout group (panel bodies, stat blocks).
        public TMP_Text Caption(Transform parent, string value, float size = 22, UITone tone = UITone.Gold)
        {
            var text = Display(Text(parent, value, size, true), tone, 2);
            text.enableAutoSizing = false; return text;
        }
        public void Rule(Transform parent)
        {
            var r = Divider(parent, Vector2.zero, Vector2.one);
            r.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
        }
        private void Style(Selectable control, Graphic graphic)
        {
            control.targetGraphic = graphic;
            var colors = control.colors; colors.normalColor = Theme.panel; colors.highlightedColor = Theme.forest;
            colors.selectedColor = Theme.forest; colors.pressedColor = Theme.ochre;
            colors.disabledColor = Theme.background; colors.fadeDuration = .12f; control.colors = colors;
            var outline = control.gameObject.AddComponent<Outline>(); outline.effectColor = Theme.selected;
            outline.effectDistance = new Vector2(2, -2); outline.enabled = false;
            control.gameObject.AddComponent<UIControlFeedback>().focus = outline;
        }
        public RectTransform KeyCap(Transform parent, string key, float size = 20)
        {
            var cap = Rect("KeyCap", parent, Vector2.zero, Vector2.one);
            Plate(cap, UIPlateKind.Key, 6);
            var layout = cap.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(12, 12, 4, 4);
            layout.childAlignment = TextAnchor.MiddleCenter; layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var le = cap.gameObject.AddComponent<LayoutElement>(); le.minWidth = 40; le.minHeight = 38;
            var text = Tone(Text(cap, key, size, true), UITone.GoldLight);
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap;
            return cap;
        }
        // Attach a key cap that follows "Acción · Tecla" labels (see UIKeyHint).
        public UIKeyHint KeyHint(TMP_Text label, Transform capParent, bool first = false)
        {
            var cap = KeyCap(capParent, "E");
            if (first) cap.SetAsFirstSibling();
            var hint = label.gameObject.AddComponent<UIKeyHint>();
            hint.label = label; hint.cap = cap.gameObject; hint.capText = cap.GetComponentInChildren<TMP_Text>();
            cap.gameObject.SetActive(false);
            return hint;
        }
        public Button Button(Transform parent, string label, UnityAction action, bool primary = false)
        {
            var r = Rect("UI_Button_" + label, parent, Vector2.zero, Vector2.one);
            // Transparent hit area; the plate child carries every visible state.
            var img = r.gameObject.AddComponent<Image>(); img.color = Color.clear;
            var button = r.gameObject.AddComponent<Button>(); Style(button, img);
            Plate(r, primary ? UIPlateKind.Primary : UIPlateKind.Secondary);
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>(); h.padding = new RectOffset(52, 24, 12, 12); h.spacing = 16;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlHeight = h.childControlWidth = true; h.childForceExpandHeight = false; h.childForceExpandWidth = false;
            var text = Text(r, label, 26); text.alignment = TextAlignmentOptions.MidlineLeft;
            text.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            KeyHint(text, r);
            var le = r.gameObject.AddComponent<LayoutElement>(); le.minHeight = 56;
            button.onClick.AddListener(() =>
            {
                if (UIManager.Instance != null && !UIManager.Instance.CanActivate) return;
                UIManager.Instance?.Sound(primary ? PlazaSound.Inspect : PlazaSound.Rotate); action?.Invoke();
            });
            button.gameObject.AddComponent<TitleMenuButton>().Initialize(primary);
            return button;
        }
        // Centred labels for short action stacks (pause, modal, results), as in the reference menus.
        public static void Center(Button button)
        {
            var label = button.GetComponentInChildren<TMP_Text>(); if (label != null) label.alignment = TextAlignmentOptions.Center;
            var h = button.GetComponent<HorizontalLayoutGroup>(); if (h != null) { h.padding.left = h.padding.right = 52; }
        }
        public static void CenterAll(Transform root) { foreach (var b in root.GetComponentsInChildren<Button>(true)) Center(b); }
        public Button Toggle(Transform parent, string key, Func<bool> get, Action<bool> set)
        {
            Button button = null;
            Action refresh = () => button.GetComponentInChildren<TMP_Text>().text = UIStrings.Get(key) + "   " + UIStrings.Get(get() ? "on" : "off");
            button = Button(parent, "", () => { set(!get()); refresh(); }); refresh();
            if (UIManager.Instance != null) button.gameObject.AddComponent<UIValueBinding>().Bind(UIManager.Instance.Settings, refresh);
            return button;
        }
        public Button Choice(Transform parent, string key, string[] values, Func<int> get, Action<int> set)
        {
            Button button = null;
            Action refresh = () => button.GetComponentInChildren<TMP_Text>().text = UIStrings.Get(key) + "   " + values[Mathf.Clamp(get(), 0, values.Length - 1)];
            button = Button(parent, "", () => { set((get() + 1) % values.Length); refresh(); }); refresh();
            if (UIManager.Instance != null) button.gameObject.AddComponent<UIValueBinding>().Bind(UIManager.Instance.Settings, refresh);
            return button;
        }
        public Slider Slider(Transform parent, string key, float min, float max, float value, Action<float> set)
        {
            var row = Column(parent, "UI_Slider", 8);
            var title = Text(row, UIStrings.Get(key) + "   " + value.ToString("0.00"), 22);
            var track = Rect("Track", row, Vector2.zero, Vector2.one); track.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            var hit = track.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            // Gold fill on a carved rail with a gold knob (reference block 1, «Slider»).
            var rail = Rect("Rail", track, new Vector2(0, .5f), new Vector2(1, .5f));
            rail.offsetMin = new Vector2(14, -7); rail.offsetMax = new Vector2(-14, 7);
            var railPlate = rail.gameObject.AddComponent<UIPlateGraphic>(); railPlate.kind = UIPlateKind.Rail; railPlate.chamfer = 4; railPlate.raycastTarget = false;
            var fillArea = Rect("FillArea", rail, Vector2.zero, Vector2.one, new Vector2(2, 2));
            var fill = Rect("Fill", fillArea, Vector2.zero, Vector2.one);
            var fillImage = fill.gameObject.AddComponent<Image>(); fillImage.color = Theme.gold; fillImage.raycastTarget = false;
            var area = Rect("HandleArea", track, Vector2.zero, Vector2.one, new Vector2(14, 0));
            var handle = Rect("Handle", area, Vector2.zero, Vector2.one); handle.sizeDelta = new Vector2(30, -18);
            var knob = handle.gameObject.AddComponent<UIPlateGraphic>(); knob.kind = UIPlateKind.Primary; knob.chamfer = 9;
            var slider = track.gameObject.AddComponent<Slider>(); Style(slider, knob);
            slider.transition = Selectable.Transition.None;
            // Focus is shown on the knob itself: the hit area is transparent.
            var rootOutline = track.GetComponent<Outline>(); if (rootOutline != null) rootOutline.enabled = false;
            var focus = handle.gameObject.AddComponent<Outline>(); focus.effectColor = Color.white; focus.effectDistance = new Vector2(2, -2); focus.enabled = false;
            track.GetComponent<UIControlFeedback>().focus = focus;
            slider.fillRect = fill; slider.handleRect = handle; slider.minValue = min; slider.maxValue = max; slider.value = value;
            slider.onValueChanged.AddListener(v => { title.text = UIStrings.Get(key) + "   " + v.ToString("0.00"); set(v); }); return slider;
        }
        public Image Bar(Transform parent, string name, Vector2 min, Vector2 max) => Bar(parent, name, min, max, Theme.gold);
        // Carved rail with gold rim; the fill keeps a faint top sheen like the reference bars.
        public Image Bar(Transform parent, string name, Vector2 min, Vector2 max, Color fill)
        {
            var rail = Rect(name, parent, min, max);
            var plate = rail.gameObject.AddComponent<UIPlateGraphic>(); plate.kind = UIPlateKind.Rail; plate.chamfer = 5; plate.raycastTarget = false;
            var inner = Rect("Inner", rail, Vector2.zero, Vector2.one, new Vector2(3, 3));
            var fillRect = Rect("Fill", inner, Vector2.zero, Vector2.one);
            var image = fillRect.gameObject.AddComponent<Image>(); image.color = fill; image.raycastTarget = false;
            var sheen = Rect("Sheen", fillRect, new Vector2(0, .62f), Vector2.one);
            var gloss = sheen.gameObject.AddComponent<Image>(); gloss.color = new Color(1, .95f, .8f, .14f); gloss.raycastTarget = false;
            return image;
        }
        public static void Fill(Image image, float value) { image.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(value), 1); }
    }

    public static class UIRectExtensions
    {
        public static RectTransform SetAnchors(this RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        // Stretch to the parent with pixel margins (left, bottom, right, top).
        public static RectTransform Inset(this RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top); return rect;
        }
        // Full-width band at a fixed height above the parent's bottom edge.
        public static RectTransform Band(this RectTransform rect, float bottom, float height, float side)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = new Vector2(side, bottom); rect.offsetMax = new Vector2(-side, bottom + height); return rect;
        }
    }
}

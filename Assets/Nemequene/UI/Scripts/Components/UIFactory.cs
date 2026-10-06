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
        // Smoked-parchment panel of the Bacatá kit: aged-gold rim, bird emblem on top, dark
        // feather in the lower right (section 3, «Materiales y acabado»).
        public RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, bool blocks = false, bool emblem = true, bool feather = true)
        {
            var r = Rect(name, parent, min, max);
            var image = r.gameObject.AddComponent<Image>(); image.color = Theme.panel; image.raycastTarget = blocks;
            bool painted = UIBacata.Available;
            if (painted) UIBacata.Frame(r.gameObject, .6f, emblem, feather);
            else if (_stonePanel != null)
            { image.sprite = _stonePanel; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 3.2f; image.color = Color.white; }
            r.gameObject.AddComponent<UIStyleBinding>().surface = true;
            Frame(r, false).gameObject.SetActive(!painted && _stonePanel == null);
            return r;
        }
        // Tints the stone sprite (e.g. ceremonial red for the defeat screen) and keeps it through accessibility refreshes.
        public static void Tint(RectTransform panel, Color tint)
        {
            var painted = panel.GetComponent<UIBacataFrame>(); if (painted != null) { painted.SetTint(tint); return; }
            var binding = panel.GetComponent<UIStyleBinding>(); if (binding != null) binding.tint = tint;
            var image = panel.GetComponent<Image>(); if (image != null && image.sprite != null) image.color = tint;
        }
        // Former parchment sheets (modals, dialogue, map, tutorial cards). The kit keeps every
        // reading layer near-black, so they share the panel frame.
        public RectTransform Parchment(string name, Transform parent, Vector2 min, Vector2 max, bool blocks = false)
        {
            if (UIBacata.Available) return Panel(name, parent, min, max, blocks);
            var r = Rect(name, parent, min, max);
            var image = r.gameObject.AddComponent<Image>(); image.raycastTarget = blocks;
            image.color = _parchmentPanel == null ? Theme.parchment : Color.white;
            if (_parchmentPanel != null)
            { image.sprite = _parchmentPanel; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 3.2f; }
            var binding = r.gameObject.AddComponent<UIStyleBinding>(); binding.surface = true; binding.parchment = true;
            Frame(r, false).gameObject.SetActive(_parchmentPanel == null);
            return r;
        }
        // Light HUD plate: thin rim, no emblem or feather, so the scene stays visible.
        public RectTransform HudPanel(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var r = Rect(name, parent, min, max);
            var background = Theme.background; background.a = .86f;
            var image = r.gameObject.AddComponent<Image>(); image.color = background; image.raycastTarget = false;
            bool painted = UIBacata.Available;
            if (painted) UIBacata.Frame(r.gameObject, .34f, false, false).tint = new Color(1, 1, 1, .94f);
            var binding = r.gameObject.AddComponent<UIStyleBinding>(); binding.surface = true; binding.hud = true;
            Frame(r, true).gameObject.SetActive(!painted);
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
            text.font = title ? Theme.Display : Theme.bodyFont; text.fontSize = size;
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
        // Monumental serif (Cinzel) for screen and section titles, in solar gold. Capitals are
        // limited to short states; titles keep their written case (document, «Tipografía»).
        public TMP_Text Display(TMP_Text text, UITone tone = UITone.Gold, float spacing = 1)
        {
            var binding = text.GetComponent<UIStyleBinding>(); binding.display = true; binding.title = true;
            text.font = Theme.Display; text.fontStyle = FontStyles.Normal; text.characterSpacing = spacing;
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
            var text = Display(Text(parent, value, size, true), tone, 1);
            text.enableAutoSizing = false; return text;
        }
        // Long reading text (cultural archive): serif face, readable mode turns it sans.
        public static TMP_Text Serif(TMP_Text text)
        {
            var binding = text.GetComponent<UIStyleBinding>(); if (binding != null) binding.serif = true;
            var theme = UIKit.Theme; if (theme != null && theme.titleFont != null) text.font = theme.titleFont;
            return text;
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
            if (UIBacata.Available) UIBacata.Skin(cap, "Frames/Frame_Key", .76f);
            else Plate(cap, UIPlateKind.Key, 6);
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
            // Primary actions are a crimson ribbon; secondary ones are text over a soft gold line
            // that turns into the ribbon on focus (document, «Materiales y acabado»).
            if (UIBacata.Available) UIBacata.Skin(r, primary ? "Controls/Ribbon" : "Controls/Line");
            else Plate(r, primary ? UIPlateKind.Primary : UIPlateKind.Secondary);
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>(); h.padding = new RectOffset(76, 40, 10, 10); h.spacing = 16;
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
        // Key hint in a safe-area corner ("Esc  Volver"): key cap first, then the action in parchment.
        public RectTransform Hint(Transform parent, string label, Vector2 corner)
        {
            var row = Row(parent, "Hint", 10); row.anchorMin = row.anchorMax = row.pivot = corner;
            row.anchoredPosition = new Vector2(corner.x < .5f ? 76 : -76, corner.y < .5f ? 46 : -46); row.sizeDelta = new Vector2(620, 44);
            var layout = row.GetComponent<HorizontalLayoutGroup>(); layout.childForceExpandWidth = false;
            layout.childAlignment = corner.x < .5f ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            foreach (var part in label.Split(new[] { "   " }, StringSplitOptions.RemoveEmptyEntries))
            {
                UIKeyHint.Split(part, out var action, out var key);
                if (key != null) KeyCap(row, key, 18);
                var text = Tone(Text(row, action, 22), UITone.Muted); text.textWrappingMode = TextWrappingModes.NoWrap;
            }
            return row;
        }
        // Footer action ("Esc  Volver"): plain text in the corner, ribbon on focus, key cap first.
        public Button FooterButton(Transform parent, string label, UnityAction action, Vector2 corner)
        {
            var host = Rect("Footer", parent, corner, corner); host.pivot = corner;
            host.anchoredPosition = new Vector2(corner.x < .5f ? 60 : -60, corner.y < .5f ? 38 : -38); host.sizeDelta = new Vector2(320, 56);
            var button = Button(host, label, action);
            var h = button.GetComponent<HorizontalLayoutGroup>(); h.padding.left = h.padding.right = 24;
            h.childAlignment = corner.x < .5f ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            var cap = button.transform.Find("KeyCap"); if (cap != null) cap.SetSiblingIndex(1);
            var label0 = button.GetComponentInChildren<TMP_Text>(); label0.GetComponent<LayoutElement>().flexibleWidth = 0;
            var style = button.GetComponent<TitleMenuButton>(); style.bare = true; style.Refresh();
            return button;
        }
        // Soft dark underlay so HUD text reads over a bright sky without a panel behind it.
        public static TMP_Text Shadow(TMP_Text text)
        {
            var binding = text.GetComponent<UIStyleBinding>(); if (binding != null) binding.shadow = true;
            UIStyleBinding.ApplyShadow(text); return text;
        }
        // Centred labels for short action stacks (pause, modal, results), as in the reference menus.
        public static void Center(Button button)
        {
            var label = button.GetComponentInChildren<TMP_Text>(); if (label != null) label.alignment = TextAlignmentOptions.Center;
            var h = button.GetComponent<HorizontalLayoutGroup>(); if (h != null) { h.padding.left = h.padding.right = 76; }
        }
        // Tab of a section row: short padding, one line, the label shrinks before it wraps.
        public static Button Tab(Button button)
        {
            Center(button);
            var h = button.GetComponent<HorizontalLayoutGroup>(); if (h != null) { h.padding.left = h.padding.right = 30; }
            var label = button.GetComponentInChildren<TMP_Text>();
            label.textWrappingMode = TextWrappingModes.NoWrap; label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize; label.fontSizeMin = 14;
            return button;
        }
        public static TMP_Text Fit(TMP_Text text)
        {
            text.textWrappingMode = TextWrappingModes.NoWrap; text.enableAutoSizing = true;
            text.fontSizeMax = text.fontSize; text.fontSizeMin = 14; return text;
        }
        public static void CenterAll(Transform root) { foreach (var b in root.GetComponentsInChildren<Button>(true)) Center(b); }
        // Settings row: label on the left, current value in solar gold on the right; the whole row
        // takes the crimson ribbon when focused (screen 24).
        public TMP_Text Value(Button button, string value)
        {
            var text = Tone(Text(button.transform, value, 24), UITone.GoldLight); text.name = "Value";
            text.alignment = TextAlignmentOptions.MidlineRight; text.textWrappingMode = TextWrappingModes.NoWrap;
            text.gameObject.AddComponent<LayoutElement>().minWidth = 120;
            return text;
        }
        public static void SetValue(Component control, string value)
        {
            foreach (var text in control.GetComponentsInChildren<TMP_Text>(true)) if (text.name == "Value") { text.text = value; return; }
        }
        public Button Toggle(Transform parent, string key, Func<bool> get, Action<bool> set, SettingsManager settings = null)
        {
            Button button = null; TMP_Text value = null; Image knob = null;
            Action refresh = () =>
            {
                bool on = get(); value.text = UIStrings.Get(on ? "on" : "off");
                if (knob != null) knob.sprite = UIBacata.Get(on ? "Controls/Toggle_On" : "Controls/Toggle_Off");
            };
            button = Button(parent, UIStrings.Get(key), () => { set(!get()); refresh(); });
            value = Value(button, "");
            if (UIBacata.Available)
            {
                var rect = Rect("Switch", button.transform, Vector2.zero, Vector2.one);
                knob = rect.gameObject.AddComponent<Image>(); knob.raycastTarget = false; knob.preserveAspect = true;
                var le = rect.gameObject.AddComponent<LayoutElement>(); le.preferredWidth = le.minWidth = 68; le.preferredHeight = 34;
            }
            refresh();
            settings = settings ?? UIManager.Instance?.Settings;
            if (settings != null) button.gameObject.AddComponent<UIValueBinding>().Bind(settings, refresh);
            return button;
        }
        public Button Choice(Transform parent, string key, string[] values, Func<int> get, Action<int> set, SettingsManager settings = null)
        {
            Button button = null; TMP_Text value = null;
            Action refresh = () => value.text = values[Mathf.Clamp(get(), 0, values.Length - 1)];
            button = Button(parent, UIStrings.Get(key), () => { set((get() + 1) % values.Length); refresh(); });
            value = Value(button, ""); refresh();
            settings = settings ?? UIManager.Instance?.Settings;
            if (settings != null) button.gameObject.AddComponent<UIValueBinding>().Bind(settings, refresh);
            return button;
        }
        public static string Format(float value, float min, float max)
            => min >= 0 && max <= 1.0001f ? Mathf.RoundToInt(value * 100) + " %" : value.ToString("0.00");
        public Slider Slider(Transform parent, string key, float min, float max, float value, Action<float> set)
        {
            var row = Column(parent, "UI_Slider", 4);
            // Same text inset as the setting rows above and below it.
            row.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(76, 40, 2, 2);
            var header = Row(row, "Header", 16); header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
            header.gameObject.AddComponent<LayoutElement>().minHeight = 34;
            var title = Text(header, UIStrings.Get(key), 24); title.alignment = TextAlignmentOptions.MidlineLeft;
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var shown = Tone(Text(header, Format(value, min, max), 24), UITone.GoldLight); shown.name = "Value";
            shown.alignment = TextAlignmentOptions.MidlineRight; shown.textWrappingMode = TextWrappingModes.NoWrap;
            var track = Rect("Track", row, Vector2.zero, Vector2.one); track.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            var hit = track.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            // Dark rail, aged-gold fill and a diamond thumb (component INPUT_slider).
            var rail = Rect("Rail", track, new Vector2(0, .5f), new Vector2(1, .5f));
            rail.offsetMin = new Vector2(14, -7); rail.offsetMax = new Vector2(-14, 7);
            Image fillImage;
            RectTransform fill;
            if (UIBacata.Available)
            {
                var railImage = rail.gameObject.AddComponent<Image>(); railImage.sprite = UIBacata.Get("Controls/Bar_Track");
                railImage.type = Image.Type.Sliced; railImage.raycastTarget = false; UIBacata.FitSelf(railImage);
                var fillArea = Rect("FillArea", rail, new Vector2(0, .25f), new Vector2(1, .75f)); fillArea.offsetMin = new Vector2(4, 0); fillArea.offsetMax = new Vector2(-4, 0);
                fill = Rect("Fill", fillArea, Vector2.zero, Vector2.one);
                fillImage = fill.gameObject.AddComponent<Image>(); fillImage.sprite = UIBacata.Get("Controls/Bar_Fill"); fillImage.type = Image.Type.Sliced;
            }
            else
            {
                var railPlate = rail.gameObject.AddComponent<UIPlateGraphic>(); railPlate.kind = UIPlateKind.Rail; railPlate.chamfer = 4; railPlate.raycastTarget = false;
                var fillArea = Rect("FillArea", rail, Vector2.zero, Vector2.one, new Vector2(2, 2));
                fill = Rect("Fill", fillArea, Vector2.zero, Vector2.one);
                fillImage = fill.gameObject.AddComponent<Image>();
            }
            fillImage.color = Theme.gold; fillImage.raycastTarget = false;
            var area = Rect("HandleArea", track, Vector2.zero, Vector2.one, new Vector2(14, 0));
            var handle = Rect("Handle", area, Vector2.zero, Vector2.one); handle.sizeDelta = new Vector2(30, -10);
            var knob = handle.gameObject.AddComponent<UIIconGraphic>(); knob.icon = UIIcon.Diamond; knob.color = Theme.paleGold;
            var slider = track.gameObject.AddComponent<Slider>(); Style(slider, knob);
            slider.transition = Selectable.Transition.None;
            // Focus is shown on the thumb itself: the hit area is transparent.
            var rootOutline = track.GetComponent<Outline>(); if (rootOutline != null) rootOutline.enabled = false;
            var focus = handle.gameObject.AddComponent<Outline>(); focus.effectColor = Color.white; focus.effectDistance = new Vector2(2, -2); focus.enabled = false;
            track.GetComponent<UIControlFeedback>().focus = focus;
            slider.fillRect = fill; slider.handleRect = handle; slider.minValue = min; slider.maxValue = max; slider.value = value;
            slider.onValueChanged.AddListener(v => { shown.text = Format(v, min, max); set(v); }); return slider;
        }
        public Image Bar(Transform parent, string name, Vector2 min, Vector2 max) => Bar(parent, name, min, max, Theme.gold);
        // Dark rail with a separate fill (components BAR_track and BAR_fill), tinted by role:
        // crimson for health, aged gold for progress, jade for voice.
        public Image Bar(Transform parent, string name, Vector2 min, Vector2 max, Color fill)
        {
            var rail = Rect(name, parent, min, max);
            if (UIBacata.Available)
            {
                var track = rail.gameObject.AddComponent<Image>(); track.sprite = UIBacata.Get("Controls/Bar_Track"); track.type = Image.Type.Sliced;
                track.raycastTarget = false; UIBacata.FitSelf(track);
                var inner = Rect("Inner", rail, new Vector2(0, .24f), new Vector2(1, .76f)); inner.offsetMin = new Vector2(5, 0); inner.offsetMax = new Vector2(-5, 0);
                var bar = Rect("Fill", inner, Vector2.zero, Vector2.one);
                var image = bar.gameObject.AddComponent<Image>(); image.sprite = UIBacata.Get("Controls/Bar_Fill"); image.type = Image.Type.Sliced;
                image.color = fill; image.raycastTarget = false;
                return image;
            }
            var plate = rail.gameObject.AddComponent<UIPlateGraphic>(); plate.kind = UIPlateKind.Rail; plate.chamfer = 5; plate.raycastTarget = false;
            var innerRect = Rect("Inner", rail, Vector2.zero, Vector2.one, new Vector2(3, 3));
            var fillRect = Rect("Fill", innerRect, Vector2.zero, Vector2.one);
            var fallback = fillRect.gameObject.AddComponent<Image>(); fallback.color = fill; fallback.raycastTarget = false;
            return fallback;
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

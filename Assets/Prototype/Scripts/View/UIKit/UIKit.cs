using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Minimal mirror of Nemequene's UIFactory for the prototype scenes (Movement), which live in
    // Prototype.Runtime and cannot reference the Nemequene UI assembly. Same theme, palette,
    // fonts and carved plates, so every screen of the game reads as one system.
    public static class UIKit
    {
        private static UITheme _theme;
        public static UITheme Theme
        {
            get { if (_theme == null) _theme = Resources.Load<UITheme>("Nemequene/Theme"); return _theme; }
        }
        private static TMP_FontAsset Font(bool display)
        {
            var theme = Theme;
            if (theme == null) return TMP_Settings.defaultFontAsset;
            return display ? theme.Display : theme.bodyFont;
        }
        public static RectTransform ScreenCanvas(Transform parent, string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            return (RectTransform)go.transform;
        }
        public static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        // Fixed-size rect placed at an anchor of its parent (e.g. top-left with a pixel margin).
        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        // Light HUD plate of the Bacatá kit: smoked panel, thin aged-gold rim, no emblem.
        public static RectTransform HudPanel(Transform parent, string name)
        {
            var rect = Rect(name, parent);
            var background = UIPalette.Deep; background.a = .86f;
            var image = rect.gameObject.AddComponent<Image>(); image.color = background; image.raycastTarget = false;
            if (UIBacata.Available) { UIBacata.Frame(rect.gameObject, .34f, false, false).SetTint(new Color(1, 1, 1, .94f)); return rect; }
            var frame = Rect("Stonework", rect).gameObject.AddComponent<UIFrameGraphic>();
            frame.compact = true; frame.raycastTarget = false;
            return rect;
        }
        // Short crimson ribbon behind a state or a contextual action (turn banner, prompts).
        public static RectTransform Ribbon(Transform parent, string name, bool focus = false)
        {
            var rect = Rect(name, parent);
            if (UIBacata.Available) UIBacata.Skin(rect, focus ? "Controls/Ribbon_Focus" : "Controls/Ribbon");
            else HudPanel(rect, "Plate");
            return rect;
        }
        // Soft dark underlay so HUD text reads over a bright sky without a panel behind it.
        public static TMP_Text Shadow(TMP_Text text)
        {
            var material = text.fontMaterial;
            material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0, 0, 0, .85f));
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, .7f);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, .35f);
            return text;
        }
        // Player vitality of screen 08: portrait in the ring of Marco_HUD, name and value above
        // the crimson bar. The fill is an Image.Filled driven by fillAmount.
        public static Image HealthFrame(Transform canvas, string caption, out TMP_Text value)
        {
            var root = Place(Rect("Vitality", canvas), new Vector2(0, 1), new Vector2(64, -40), new Vector2(452, 138));
            var ring = Rect("Portrait", root); ring.anchorMin = ring.anchorMax = new Vector2(.151f, .5f); ring.sizeDelta = new Vector2(104, 104);
            var portrait = ring.gameObject.AddComponent<RawImage>(); portrait.texture = UIBacata.Art("Retrato_HUD"); portrait.raycastTarget = false;
            var frame = root.gameObject.AddComponent<Image>(); frame.sprite = UIBacata.Get("Frames/Frame_HUD"); frame.raycastTarget = false;
            frame.preserveAspect = true; if (frame.sprite == null) frame.color = Color.clear;
            var channel = Rect("Inner", root); channel.anchorMin = new Vector2(.336f, .35f); channel.anchorMax = new Vector2(.904f, .60f);
            var fill = Rect("Fill", channel).gameObject.AddComponent<Image>();
            fill.sprite = UIBacata.Get("Controls/Bar_Fill") ?? WhiteSprite(); fill.color = UIPalette.Crimson; fill.raycastTarget = false;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            var name = Shadow(Label(root, caption, 22, UIPalette.Ivory)); name.fontStyle = FontStyles.Bold;
            name.alignment = TextAlignmentOptions.BottomLeft; name.rectTransform.anchorMin = new Vector2(.34f, .64f); name.rectTransform.anchorMax = new Vector2(.70f, .98f);
            value = Shadow(Label(root, "", 20, UIPalette.Muted));
            value.alignment = TextAlignmentOptions.BottomRight; value.rectTransform.anchorMin = new Vector2(.62f, .64f); value.rectTransform.anchorMax = new Vector2(.90f, .98f);
            return fill;
        }
        // Key hint in a safe-area corner ("Esc  Pausa"): key cap first, then the action.
        public static RectTransform Hint(Transform canvas, string key, string action, Vector2 corner)
        {
            var row = Place(Rect("Hint", canvas), corner, new Vector2(corner.x < .5f ? 76 : -76, corner.y < .5f ? 46 : -46), new Vector2(560, 40));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 10;
            layout.childAlignment = corner.x < .5f ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var cap = Rect("KeyCap", row);
            if (UIBacata.Available) UIBacata.Skin(cap, "Frames/Frame_Key", .76f);
            var capLayout = cap.gameObject.AddComponent<HorizontalLayoutGroup>(); capLayout.padding = new RectOffset(12, 12, 4, 4);
            capLayout.childControlWidth = capLayout.childControlHeight = true;
            var capText = Label(cap, key, 18, UIPalette.GoldLight); capText.textWrappingMode = TextWrappingModes.NoWrap;
            var capSize = cap.gameObject.AddComponent<LayoutElement>(); capSize.minHeight = capSize.preferredHeight = 36;
            capSize.preferredWidth = capSize.minWidth = Mathf.Max(40, capText.GetPreferredValues(key).x + 26); capSize.flexibleWidth = 0;
            capLayout.childForceExpandWidth = capLayout.childForceExpandHeight = false;
            var text = Label(row, action, 22, UIPalette.Muted); text.textWrappingMode = TextWrappingModes.NoWrap;
            var textSize = text.gameObject.AddComponent<LayoutElement>(); textSize.preferredWidth = text.GetPreferredValues(action).x + 4; textSize.flexibleWidth = 0;
            return row;
        }
        public static TMP_Text Label(Transform parent, string value, float size, Color color, bool display = false)
        {
            var text = Rect("Text", parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Font(display); text.fontSize = size; text.color = color; text.text = value;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; text.richText = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            if (display) text.characterSpacing = 1;
            return text;
        }
        public static UIIconGraphic Icon(Transform parent, UIIcon icon, Color color)
        {
            var graphic = Rect("Icon_" + icon, parent).gameObject.AddComponent<UIIconGraphic>();
            graphic.icon = icon; graphic.color = color; graphic.raycastTarget = false; return graphic;
        }
        // Carved rail with a gold rim and a horizontally filled bar (fillAmount drives it).
        public static Image Bar(RectTransform parent, Vector2 size, Color fill)
        {
            var rail = Rect("HealthBar", parent);
            rail.anchorMin = rail.anchorMax = new Vector2(.5f, .5f); rail.sizeDelta = size;
            var inner = Rect("Inner", rail);
            Image image;
            if (UIBacata.Available)
            {
                // Dark track and separate fill of the kit (BAR_track, BAR_fill).
                var track = rail.gameObject.AddComponent<Image>(); track.sprite = UIBacata.Get("Controls/Bar_Track");
                track.type = Image.Type.Sliced; track.raycastTarget = false; UIBacata.FitSelf(track);
                inner.anchorMin = new Vector2(0, .24f); inner.anchorMax = new Vector2(1, .76f); inner.offsetMin = new Vector2(5, 0); inner.offsetMax = new Vector2(-5, 0);
                image = Rect("Fill", inner).gameObject.AddComponent<Image>(); image.sprite = UIBacata.Get("Controls/Bar_Fill");
            }
            else
            {
                var plate = rail.gameObject.AddComponent<UIPlateGraphic>(); plate.kind = UIPlateKind.Rail; plate.chamfer = 4; plate.raycastTarget = false;
                float inset = size.y < 16 ? 2 : 3;
                inner.offsetMin = new Vector2(inset, inset); inner.offsetMax = new Vector2(-inset, -inset);
                image = Rect("Fill", inner).gameObject.AddComponent<Image>(); image.sprite = WhiteSprite();
            }
            image.color = fill; image.raycastTarget = false;
            image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal; image.fillOrigin = (int)Image.OriginHorizontal.Left;
            return image;
        }
        // Telegraph alert: sun medallion with "!" and a written cue, so the warning never relies on colour alone.
        public static GameObject Alert(Transform parent, string name, int order, string caption, float top)
        {
            var canvas = ScreenCanvas(parent, name, order);
            var panel = Place(HudPanel(canvas, "Alert"), new Vector2(.5f, 1), new Vector2(0, -top), new Vector2(240, 150));
            var icon = Icon(panel, UIIcon.Alert, UIPalette.Danger);
            icon.rectTransform.anchorMin = new Vector2(.5f, .42f); icon.rectTransform.anchorMax = new Vector2(.5f, .42f);
            icon.rectTransform.sizeDelta = new Vector2(72, 72); icon.rectTransform.anchoredPosition = new Vector2(0, 14);
            var label = Label(panel, caption, 24, UIPalette.GoldLight, true);
            label.rectTransform.anchorMin = new Vector2(.06f, .06f); label.rectTransform.anchorMax = new Vector2(.94f, .32f);
            canvas.gameObject.SetActive(false);
            return canvas.gameObject;
        }
        private static Sprite _white;
        private static Sprite WhiteSprite()
        {
            if (_white == null)
            {
                var texture = Texture2D.whiteTexture;
                _white = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            }
            return _white;
        }
    }
}

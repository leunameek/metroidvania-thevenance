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
            return display ? theme.Display : theme.titleFont;
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
        public static RectTransform HudPanel(Transform parent, string name)
        {
            var rect = Rect(name, parent);
            var background = UIPalette.Deep; background.a = .86f;
            var image = rect.gameObject.AddComponent<Image>(); image.color = background; image.raycastTarget = false;
            var frame = Rect("Stonework", rect).gameObject.AddComponent<UIFrameGraphic>();
            frame.compact = true; frame.raycastTarget = false;
            return rect;
        }
        public static TMP_Text Label(Transform parent, string value, float size, Color color, bool display = false)
        {
            var text = Rect("Text", parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Font(display); text.fontSize = size; text.color = color; text.text = value;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; text.richText = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            if (display) { text.fontStyle = FontStyles.UpperCase; text.characterSpacing = 4; }
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
            var plate = rail.gameObject.AddComponent<UIPlateGraphic>(); plate.kind = UIPlateKind.Rail; plate.chamfer = 4; plate.raycastTarget = false;
            var inner = Rect("Inner", rail); float inset = size.y < 16 ? 2 : 3;
            inner.offsetMin = new Vector2(inset, inset); inner.offsetMax = new Vector2(-inset, -inset);
            var image = Rect("Fill", inner).gameObject.AddComponent<Image>();
            image.sprite = WhiteSprite(); image.color = fill; image.raycastTarget = false;
            image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal; image.fillOrigin = (int)Image.OriginHorizontal.Left;
            return image;
        }
        // Telegraph alert: sun medallion with "!" and a written cue, so the warning never relies on colour alone.
        public static GameObject Alert(Transform parent, string name, int order, string caption, float top)
        {
            var canvas = ScreenCanvas(parent, name, order);
            var panel = Place(HudPanel(canvas, "Alert"), new Vector2(.5f, 1), new Vector2(0, -top), new Vector2(240, 150));
            var icon = Icon(panel, UIIcon.Sun, UITheme.Hex("E4604F"));
            icon.rectTransform.anchorMin = new Vector2(.5f, .42f); icon.rectTransform.anchorMax = new Vector2(.5f, .42f);
            icon.rectTransform.sizeDelta = new Vector2(72, 72); icon.rectTransform.anchoredPosition = new Vector2(0, 14);
            var label = Label(panel, caption, 22, UIPalette.GoldLight, true);
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

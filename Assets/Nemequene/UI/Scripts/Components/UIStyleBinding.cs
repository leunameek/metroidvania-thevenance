using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Text roles. Each is paired with the surface it may sit on; see UIPalette for the ratios.
    public enum UITone { Default, Gold, Muted, Ink, InkMuted, Danger, Success, GoldLight }

    public sealed class UIStyleBinding : MonoBehaviour
    {
        public float baseSize = 24;
        public bool title, surface, subtitle, parchment, hud, ink;
        // Carved display face (Cinzel) for headings; readable-font mode falls back to Noto.
        public bool display;
        public UITone tone;
        public Color tint = Color.white;
        private Sprite _sourceSprite;
        public static Color ToneColor(UITone tone)
        {
            switch (tone)
            {
                case UITone.Gold: return UIPalette.GoldText;
                case UITone.GoldLight: return UIPalette.GoldLight;
                case UITone.Muted: return UIPalette.Muted;
                case UITone.Ink: return UIPalette.Ink;
                case UITone.InkMuted: return UIPalette.InkMuted;
                case UITone.Danger: return UIPalette.Danger;
                case UITone.Success: return UIPalette.Success;
                default: return UIPalette.Ivory;
            }
        }
        public void Apply(UITheme theme, UISettings settings)
        {
            var text = GetComponent<TMP_Text>();
            if (text != null)
            {
                text.font = display && !settings.readableFont ? theme.Display : title || !settings.readableFont ? theme.titleFont : theme.bodyFont;
                float size = baseSize * (subtitle ? settings.subtitleScale : settings.textScale);
                text.fontSize = size;
                if (text.enableAutoSizing) { text.fontSizeMax = size; text.fontSizeMin = Mathf.Max(16, size * .6f); }
                var role = ink && tone == UITone.Default ? UITone.Ink : tone;
                text.color = settings.highContrast ? Color.white : ToneColor(role);
            }
            if (surface)
            {
                var image = GetComponent<Image>();
                if (_sourceSprite == null && image.sprite != null) _sourceSprite = image.sprite;
                image.sprite = settings.highContrast ? null : _sourceSprite;
                var color = settings.highContrast ? UIPalette.Charcoal
                    : _sourceSprite != null ? tint : parchment ? theme.parchment : hud ? theme.background : theme.panel;
                color.a = settings.highContrast ? 1 : hud ? .86f : color.a;
                image.color = color;
                var frame = GetComponentInChildren<UIFrameGraphic>(true);
                if (frame != null)
                { frame.gameObject.SetActive(hud || settings.highContrast || _sourceSprite == null); frame.SetState(false, false, settings.highContrast); }
            }
        }
    }
}

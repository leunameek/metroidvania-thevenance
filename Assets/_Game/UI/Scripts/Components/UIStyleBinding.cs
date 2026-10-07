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
        // Cinzel for titles, Noto Serif for long cultural reading, Noto Sans for everything else;
        // readable-font mode turns every face into Noto Sans.
        public bool display, serif, shadow;
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
        public static void ApplyShadow(TMP_Text text) { UIKit.Shadow(text); }
        public void Apply(UITheme theme, UISettings settings)
        {
            var text = GetComponent<TMP_Text>();
            if (text != null)
            {
                text.font = settings.readableFont ? theme.bodyFont : display || title ? theme.Display : serif ? theme.titleFont : theme.bodyFont;
                float size = baseSize * (subtitle ? settings.subtitleScale : settings.textScale);
                text.fontSize = size;
                if (text.enableAutoSizing) { text.fontSizeMax = size; text.fontSizeMin = Mathf.Min(text.fontSizeMin > 0 ? text.fontSizeMin : 16, Mathf.Max(14, size * .6f)); }
                var role = ink && tone == UITone.Default ? UITone.Ink : tone;
                text.color = settings.highContrast ? Color.white : ToneColor(role);
                if (shadow) ApplyShadow(text);
            }
            var painted = surface ? GetComponent<UIBacataFrame>() : null;
            if (painted != null)
            {
                painted.SetContrast(settings.highContrast);
                var outline = GetComponentInChildren<UIFrameGraphic>(true);
                if (outline != null) { outline.gameObject.SetActive(settings.highContrast); outline.SetState(false, false, true); }
                return;
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

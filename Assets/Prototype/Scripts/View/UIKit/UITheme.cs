using TMPro;
using UnityEngine;

namespace Nemequene.UI
{
    [CreateAssetMenu(menuName = "Nemequene/UI/Theme")]
    public sealed class UITheme : ScriptableObject
    {
        public TMP_FontAsset bodyFont;
        public TMP_FontAsset titleFont;
        // Carved display face for headings only; reading text stays on titleFont/bodyFont.
        public TMP_FontAsset displayFont;
        public Sprite stonePanel, parchmentPanel, stoneButton;
        // «Interfaces de El Asedio de Bacatá», 3 Sistema visual: near-black smoked parchment, aged
        // gold for rims and titles, crimson for selection and danger, jade for confirmation.
        public Color background = Hex("080A0B"), panel = Hex("151719"), forest = Hex("32171F");
        public Color gold = Hex("B79557"), paleGold = Hex("E0BD74"), ivory = Hex("F1E5CF");
        public Color stone = Hex("BCB4A6"), terracotta = Hex("B32940"), ochre = Hex("B79557");
        public Color success = Hex("70B2A1"), warning = Hex("E0BD74"), error = Hex("B32940");
        public Color information = Hex("BCB4A6"), disabled = Hex("8A847A"), selected = Hex("E0BD74");
        public Color parchment = Hex("151719"), ink = Hex("F1E5CF");
        public TMP_FontAsset Display => displayFont != null ? displayFont : titleFont;
        public Color WorldAccent(int world) => world < 0 ? UIPalette.Nature : world > 0 ? paleGold : gold;
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }

    // Palette of the design document (section 3). Ratios measured on Panel #151719, the darkest
    // reading surface's lighter neighbour; every reading text clears 4.5:1, headings 3:1.
    public static class UIPalette
    {
        public static readonly Color Deep = UITheme.Hex("080A0B");        // Fondo profundo: menus, reading layers
        public static readonly Color Surface = UITheme.Hex("111516");     // Superficie: secondary layers
        public static readonly Color Stone = UITheme.Hex("151719");       // Panel: containers and buttons
        public static readonly Color Elevated = UITheme.Hex("32171F");    // Elevado: selection and detail
        public static readonly Color Gold = UITheme.Hex("B79557");        // Oro envejecido: rims, hierarchy (6.4:1)
        public static readonly Color GoldLight = UITheme.Hex("E0BD74");   // Oro solar: selection, focus (10.0:1)
        public static readonly Color GoldText = UITheme.Hex("E0BD74");    // titles
        public static readonly Color GoldDeep = UITheme.Hex("8E7444");
        public static readonly Color Ivory = UITheme.Hex("F1E5CF");       // Hueso: main text (14.4:1)
        public static readonly Color Muted = UITheme.Hex("BCB4A6");       // Pergamino: secondary text (8.7:1)
        public static readonly Color Disabled = UITheme.Hex("8A847A");    // 4.8:1, label stays readable
        public static readonly Color Crimson = UITheme.Hex("B32940");     // Carmesí: damage, destructive (surfaces only)
        public static readonly Color Ceremonial = Crimson;
        public static readonly Color Danger = UITheme.Hex("E86A7C");      // crimson for text, 5.8:1
        public static readonly Color Jade = UITheme.Hex("70B2A1");        // Jade: voice, achievement, confirmation (7.3:1)
        public static readonly Color Success = Jade;
        public static readonly Color Nature = Jade;
        public static readonly Color Charcoal = UITheme.Hex("080A0B");
        public static readonly Color Wood = Elevated;
        public static readonly Color Obsidian = Stone;
        // The kit has no light surfaces: former parchment roles read as bone and parchment text.
        public static readonly Color Parchment = Muted;
        public static readonly Color Ink = Ivory;
        public static readonly Color InkMuted = Muted;
        public static readonly Color OnGold = Deep;
    }
}

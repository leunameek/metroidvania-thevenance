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
        public Color background = Hex("16130F"), panel = Hex("2C1F17"), forest = Hex("4A3523");
        public Color gold = Hex("D4A342"), paleGold = Hex("F4D18C"), ivory = Hex("EFE3C8");
        public Color stone = Hex("A99F8E"), terracotta = Hex("8B2A2A"), ochre = Hex("C28A3E");
        public Color success = Hex("4B7A3A"), warning = Hex("C28A3E"), error = Hex("8B2A2A");
        public Color information = Hex("C9B89A"), disabled = Hex("A99F8E"), selected = Hex("F4D18C");
        public Color parchment = Hex("DCC29A"), ink = Hex("2A1D12");
        public TMP_FontAsset Display => displayFont != null ? displayFont : titleFont;
        public Color WorldAccent(int world) => world < 0 ? UIPalette.Nature : world > 0 ? paleGold : gold;
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }

    // Reference palette "El asedio de Bacatá" (bloques 1-4). Every text/surface pair below is
    // checked for WCAG contrast: 4.5:1 for reading text, 3:1 for headings of 28 px or more.
    public static class UIPalette
    {
        public static readonly Color Gold = UITheme.Hex("D4A342");        // Oro Muisca
        public static readonly Color GoldLight = UITheme.Hex("F4D18C");   // Dorado claro
        public static readonly Color GoldDeep = UITheme.Hex("B8862F");
        public static readonly Color GoldText = UITheme.Hex("E6B85C");     // headings, 8.6:1 on stone
        public static readonly Color Parchment = UITheme.Hex("DCC29A");   // Pergamino avejentado
        public static readonly Color Wood = UITheme.Hex("4A3523");        // Madera oscura
        public static readonly Color Obsidian = UITheme.Hex("1C1C1C");    // Piedra obsidiana
        public static readonly Color Ceremonial = UITheme.Hex("8B2A2A");  // Rojo ceremonial
        public static readonly Color Nature = UITheme.Hex("4B7A3A");      // Verde naturaleza
        public static readonly Color Charcoal = UITheme.Hex("0E0C09");
        public static readonly Color Deep = UITheme.Hex("16130F");
        public static readonly Color Stone = UITheme.Hex("2C1F17");       // measured centre of StonePanel-v2
        // Text roles.
        public static readonly Color Ivory = UITheme.Hex("EFE3C8");       // 12.5:1 on stone, 9.7:1 on the HUD veil
        public static readonly Color Muted = UITheme.Hex("C9B89A");       // 8.2:1 on stone, 6.4:1 on the HUD veil
        public static readonly Color Disabled = UITheme.Hex("A99F8E");    // 5.9:1 on disabled plate
        public static readonly Color Ink = UITheme.Hex("2A1D12");         // 11.3:1 on parchment
        public static readonly Color InkMuted = UITheme.Hex("5A4228");    // 6.4:1 on parchment
        public static readonly Color OnGold = UITheme.Hex("1E140A");      // 5.6:1 on the darkest gold
        public static readonly Color Danger = UITheme.Hex("E4604F");      // 4.6:1 on stone, headings only
        public static readonly Color Success = UITheme.Hex("9CC47A");     // 8.0:1 on stone
    }
}

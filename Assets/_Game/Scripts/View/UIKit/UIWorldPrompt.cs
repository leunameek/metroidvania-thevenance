using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Contextual action of screens 08 and 18 for scenes without the Nemequene HUD: one crimson
    // ribbon at the lower centre, key cap first, then the action. The last caller owns it.
    public static class UIWorldPrompt
    {
        private static GameObject _root, _cap;
        private static TMP_Text _key, _label;
        private static object _owner;
        private static LayoutElement _capSize, _labelSize;
        private static RectTransform _ribbon;
        // Widest the ribbon gets before the action text shrinks.
        private const float MaxWidth = 1150, Ends = 160, Gap = 14;

        public static void Show(object owner, string key, string action)
        {
            Ensure();
            _owner = owner;
            bool changed = false;
            bool hasKey = !string.IsNullOrEmpty(key);
            if (_cap.activeSelf != hasKey) { _cap.SetActive(hasKey); changed = true; }
            if (_key.text != (key ?? "")) { _key.text = key ?? ""; _capSize.preferredWidth = _capSize.minWidth = Mathf.Max(40, _key.GetPreferredValues(_key.text).x + 26); changed = true; }
            if (_label.text != action) { _label.text = action; changed = true; }
            if (changed) Fit();
            if (!_root.activeSelf) _root.SetActive(true);
        }

        public static void Hide(object owner)
        {
            if (_root != null && _owner == owner && _root.activeSelf) _root.SetActive(false);
        }

        // The ribbon grows with its text so the action never runs over the key cap or out of the
        // ribbon (2026-10-07 playtest: «Responder a la prueba de la mujer-cóndor» spilled out).
        private static void Fit()
        {
            float cap = _cap.activeSelf ? _capSize.preferredWidth + Gap : 0;
            _label.enableAutoSizing = false; _label.fontSize = 26;
            float text = _label.GetPreferredValues(_label.text, 10000, 60).x;
            float width = Mathf.Min(MaxWidth, Mathf.Max(720, cap + text + Ends + 8));
            float room = width - Ends - cap;
            if (text > room) { _label.enableAutoSizing = true; _label.fontSizeMin = 16; _label.fontSizeMax = 26; }
            _labelSize.preferredWidth = Mathf.Min(text, room);
            _ribbon.sizeDelta = new Vector2(width, _ribbon.sizeDelta.y);
        }

        private static void Ensure()
        {
            if (_root != null) return;
            var canvas = UIKit.ScreenCanvas(null, "WorldPromptCanvas", 970);
            _root = canvas.gameObject;
            var ribbon = UIKit.Place(UIKit.Ribbon(canvas, "Prompt"), new Vector2(.5f, 0), new Vector2(0, 190), new Vector2(720, 70)); _ribbon = ribbon;
            var row = UIKit.Rect("Row", ribbon); row.offsetMin = new Vector2(80, 6); row.offsetMax = new Vector2(-80, -6);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 14; layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var cap = UIKit.Rect("KeyCap", row); _cap = cap.gameObject;
            if (UIBacata.Available) UIBacata.Skin(cap, "Frames/Frame_Key", .76f);
            var capLayout = cap.gameObject.AddComponent<HorizontalLayoutGroup>(); capLayout.padding = new RectOffset(12, 12, 4, 4);
            capLayout.childControlWidth = capLayout.childControlHeight = true;
            capLayout.childForceExpandWidth = capLayout.childForceExpandHeight = false;
            _capSize = cap.gameObject.AddComponent<LayoutElement>(); _capSize.minHeight = _capSize.preferredHeight = 38; _capSize.flexibleWidth = 0;
            _key = UIKit.Label(cap, "", 20, UIPalette.GoldLight); _key.textWrappingMode = TextWrappingModes.NoWrap;
            _label = UIKit.Label(row, "", 26, UIPalette.Ivory); _label.fontStyle = FontStyles.Bold; _label.textWrappingMode = TextWrappingModes.NoWrap;
            _labelSize = _label.gameObject.AddComponent<LayoutElement>(); _labelSize.flexibleWidth = 0;
            _label.overflowMode = TextOverflowModes.Ellipsis;
            _root.SetActive(false);
        }
    }
}

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
        private static LayoutElement _capSize;

        public static void Show(object owner, string key, string action)
        {
            Ensure();
            _owner = owner;
            _cap.SetActive(!string.IsNullOrEmpty(key));
            if (_key.text != (key ?? "")) { _key.text = key ?? ""; _capSize.preferredWidth = _capSize.minWidth = Mathf.Max(40, _key.GetPreferredValues(_key.text).x + 26); }
            if (_label.text != action) _label.text = action;
            if (!_root.activeSelf) _root.SetActive(true);
        }

        public static void Hide(object owner)
        {
            if (_root != null && _owner == owner && _root.activeSelf) _root.SetActive(false);
        }

        private static void Ensure()
        {
            if (_root != null) return;
            var canvas = UIKit.ScreenCanvas(null, "WorldPromptCanvas", 970);
            _root = canvas.gameObject;
            var ribbon = UIKit.Place(UIKit.Ribbon(canvas, "Prompt"), new Vector2(.5f, 0), new Vector2(0, 190), new Vector2(720, 70));
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
            _root.SetActive(false);
        }
    }
}

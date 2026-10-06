using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Button of the Bacatá kit for scenes without the Nemequene UI (Mundo Inferior): text over
    // the gold line, crimson ribbon with the gold marker on focus or hover, same as UIFactory.
    public sealed class UIKitButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _skin;
        private TMP_Text _label;
        private Button _button;
        private bool _focus, _hover;
        public bool primary;

        public static Button Create(Transform parent, string label, Action action, bool primary = false, float height = 56)
        {
            var rect = UIKit.Rect("UI_Button_" + label, parent);
            var hit = rect.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>(); button.transition = Selectable.Transition.None; button.targetGraphic = hit;
            var skin = UIBacata.Available ? UIBacata.Skin(rect, primary ? "Controls/Ribbon" : "Controls/Line") : null;
            var text = UIKit.Label(rect, label, 24, UIPalette.Ivory);
            text.rectTransform.offsetMin = new Vector2(72, 4); text.rectTransform.offsetMax = new Vector2(-72, -4);
            text.textWrappingMode = TextWrappingModes.NoWrap; text.enableAutoSizing = true; text.fontSizeMax = 24; text.fontSizeMin = 16;
            var element = rect.gameObject.AddComponent<LayoutElement>(); element.minHeight = element.preferredHeight = height;
            var style = rect.gameObject.AddComponent<UIKitButton>();
            style._skin = skin; style._label = text; style._button = button; style.primary = primary;
            button.onClick.AddListener(() => { MIAudio.Play("ui_confirmar", .7f); action?.Invoke(); });
            style.Refresh();
            return button;
        }

        public void OnSelect(BaseEventData e) { _focus = true; Refresh(); MIAudio.Play("ui_foco", .35f); }
        public void OnDeselect(BaseEventData e) { _focus = false; Refresh(); }
        public void OnPointerEnter(PointerEventData e) { _hover = true; Refresh(); }
        public void OnPointerExit(PointerEventData e) { _hover = false; Refresh(); }
        private void OnDisable() { _focus = _hover = false; Refresh(); }

        private void Refresh()
        {
            if (_label == null) return;
            bool enabled = _button == null || _button.IsInteractable();
            bool ribbon = enabled && (_focus || _hover);
            if (_skin != null)
            {
                string sprite = !enabled ? (primary ? "Controls/Ribbon_Disabled" : "Controls/Line_Disabled") : ribbon ? "Controls/Ribbon_Focus" : primary ? "Controls/Ribbon" : "Controls/Line";
                _skin.sprite = UIBacata.Get(sprite);
            }
            _label.color = enabled ? UIPalette.Ivory : UIPalette.Disabled;
            _label.fontStyle = ribbon || primary ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}

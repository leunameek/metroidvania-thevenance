using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum UIControlState { Normal, Hover, Focus, Pressed, Selected, Disabled, Loading, Success, Warning, Error }
    public sealed class UIControlFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Outline focus;
        public void OnSelect(BaseEventData e) { SetFocus(true); ScrollIntoView(); }
        public void OnDeselect(BaseEventData e) { SetFocus(false); }
        public void OnPointerEnter(PointerEventData e) { if (GetComponent<Selectable>().IsInteractable()) SetFocus(true); }
        public void OnPointerExit(PointerEventData e) { SetFocus(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject); }
        private void SetFocus(bool active) { if (focus != null) focus.enabled = active; }
        private void ScrollIntoView()
        {
            var scroll = GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content == null) return;
            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, transform);
            var view = scroll.viewport.rect;
            float delta = bounds.max.y > view.yMax ? view.yMax - bounds.max.y : bounds.min.y < view.yMin ? view.yMin - bounds.min.y : 0;
            scroll.content.anchoredPosition += new Vector2(0, delta);
        }
    }
}

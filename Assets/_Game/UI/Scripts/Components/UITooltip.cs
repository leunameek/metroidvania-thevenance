using UnityEngine;
using UnityEngine.EventSystems;

namespace Nemequene.UI
{
    public sealed class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GameObject content;
        public void Configure(GameObject view) { content=view; content.SetActive(false); }
        public void OnPointerEnter(PointerEventData e) { Set(true); }
        public void OnPointerExit(PointerEventData e) { Set(false); }
        public void OnSelect(BaseEventData e) { Set(true); }
        public void OnDeselect(BaseEventData e) { Set(false); }
        private void Set(bool visible) { if(content!=null) content.SetActive(visible); }
        private void OnDisable() { Set(false); }
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nemequene.UI
{
    // A settings row that explains itself: when it gets focus or the pointer, its text goes to the
    // page's description panel (what it changes and what it costs, CPU or GPU).
    public sealed class UIDescribed : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        public string text;
        public Action<string> sink;
        public void OnSelect(BaseEventData data) => sink?.Invoke(text);
        public void OnPointerEnter(PointerEventData data) => sink?.Invoke(text);
    }
}

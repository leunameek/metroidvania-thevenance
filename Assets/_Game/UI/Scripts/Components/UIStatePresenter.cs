using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class UIStatePresenter : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image surface;
        [SerializeField] private string labelKey;
        public UIControlState State { get; private set; }
        public void Configure(UITheme colors,TMP_Text text,Image background,string key)
        { theme=colors; label=text; surface=background; labelKey=key; }
        public void SetState(UIControlState state)
        {
            State=state;
            if(theme==null) return;
            Color color=state==UIControlState.Error?theme.error:state==UIControlState.Warning?theme.warning:
                state==UIControlState.Success?theme.success:state==UIControlState.Disabled?theme.disabled:theme.panel;
            if(surface!=null) surface.color=color;
            var control=GetComponent<Selectable>();
            if(control!=null)
            {
                control.interactable=state!=UIControlState.Disabled && state!=UIControlState.Loading;
                var colors=control.colors; colors.normalColor=color; control.colors=colors;
            }
            if(label!=null) label.text=UIStrings.Get(labelKey)+(state==UIControlState.Normal?"":" · "+UIStrings.Get("component.state."+state));
        }
    }
}

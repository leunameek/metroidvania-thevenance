using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nemequene.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class TitleMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action focused;
        public bool tabSelected;
        public bool danger;
        public bool primary;
        private Button _button;
        private Image _surface;
        private TMP_Text _label;
        private Outline _outline;
        private UIFrameGraphic _frame;
        private UIPlateGraphic _plate;
        private GameObject _mark;
        private bool _hover, _focused, _pressed;
        private Sprite _stone;
        public void UseStoneSkin(Sprite sprite,TMP_FontAsset font,bool wide=false)
        {
            if(sprite==null)return;
            _stone=sprite;_surface.sprite=sprite;_surface.type=Image.Type.Sliced;_surface.pixelsPerUnitMultiplier=3.5f;
            _label.font=font;_label.fontStyle=FontStyles.UpperCase|FontStyles.Bold;_label.alignment=TextAlignmentOptions.Center;
            _label.characterSpacing=2;
            var layout=GetComponent<HorizontalLayoutGroup>();layout.padding=new RectOffset(wide?92:52,wide?92:52,14,14);
            layout.childAlignment=TextAnchor.MiddleCenter;layout.childForceExpandHeight=true;
            if (_frame != null) _frame.gameObject.SetActive(false);
            if (_plate != null) _plate.gameObject.SetActive(false);
            Refresh();
        }
        public void Initialize(bool main)
        {
            primary = main;
            if (_button != null) { GetComponent<HorizontalLayoutGroup>().padding.left=main?64:52; Refresh(); return; }
            _button=GetComponent<Button>(); _button.transition=Selectable.Transition.None;
            _surface=GetComponent<Image>(); _label=GetComponentInChildren<TMP_Text>();
            _outline=GetComponent<Outline>(); _outline.effectDistance=new Vector2(2,-2);
            _frame=GetComponentInChildren<UIFrameGraphic>(true);
            _plate=GetComponentInChildren<UIPlateGraphic>(true);
            _outline.effectColor=UIPalette.GoldLight;
            var existing=transform.Find("FocusMarker");
            var mark=existing!=null?existing.gameObject:new GameObject("FocusMarker",typeof(RectTransform),typeof(Image));
            mark.transform.SetParent(transform,false);
            var rect=(RectTransform)mark.transform; rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);
            rect.sizeDelta=new Vector2(12,12); rect.anchoredPosition=new Vector2(27,0); rect.localRotation=Quaternion.Euler(0,0,45);
            mark.GetComponent<Image>().color=UIPalette.GoldLight; mark.GetComponent<Image>().raycastTarget=false;
            (mark.GetComponent<LayoutElement>()??mark.AddComponent<LayoutElement>()).ignoreLayout=true;
            _mark=mark; GetComponent<HorizontalLayoutGroup>().padding.left=main?64:52;
            Refresh();
        }
        public void Refresh()
        {
            if (_button==null) return;
            bool enabled=_button.IsInteractable();
            bool focus=_focused && enabled;
            bool selected=enabled && (focus||_hover);
            bool contrast=TitleMenuController.Instance!=null && TitleMenuController.Instance.Settings!=null
                ? TitleMenuController.Instance.Settings.Values.highContrast
                : UIManager.Instance!=null && UIManager.Instance.Settings!=null && UIManager.Instance.Settings.Values.highContrast;
            if(_stone!=null)
            {
                _surface.sprite=contrast?null:_stone;
                if (_frame != null) { _frame.gameObject.SetActive(contrast); _frame.SetState(selected,danger,contrast); }
                _surface.color=contrast?UIPalette.Deep:danger?UITheme.Hex(selected?"C78773":"A46A58"):Color.white;
                _surface.CrossFadeColor(Color.white,0,true,true);_surface.canvasRenderer.SetColor(Color.white);
                _label.color=!enabled?UIPalette.Disabled:UIPalette.Ivory;
                _outline.effectColor=danger?UITheme.Hex("D9645A"):UIPalette.GoldLight;_outline.effectDistance=new Vector2(3,-3);
                _outline.enabled=selected;_mark.SetActive(focus);
                return;
            }
            if (_plate != null)
            {
                var kind=!enabled?UIPlateKind.Disabled:danger?UIPlateKind.Danger:primary||tabSelected?UIPlateKind.Primary:UIPlateKind.Secondary;
                _plate.gameObject.SetActive(true); _plate.Set(kind,selected,_pressed&&enabled,contrast);
                if (_frame != null) _frame.gameObject.SetActive(false);
                _surface.color=Color.clear; _surface.CrossFadeColor(Color.white,0,true,true); _surface.canvasRenderer.SetColor(Color.white);
                // Dark ink on gold (5.6:1 minimum), ivory on stone and red (5.2:1 minimum).
                _label.color=kind==UIPlateKind.Disabled?UIPalette.Disabled:kind==UIPlateKind.Primary?(contrast?Color.black:UIPalette.OnGold):contrast?Color.white:UIPalette.Ivory;
                _outline.enabled=false;
                _mark.GetComponent<Image>().color=kind==UIPlateKind.Primary?UIPalette.OnGold:UIPalette.GoldLight;
                _mark.SetActive(focus);
                return;
            }
            _surface.color=danger?(selected?UIPalette.Ceremonial:UITheme.Hex("6E1F1B"))
                :selected||tabSelected?UIPalette.Wood:UIPalette.Stone;
            // Stop the factory's previous ColorTint tween; otherwise it multiplies the authored surface.
            _surface.CrossFadeColor(Color.white,0,true,true);
            _surface.canvasRenderer.SetColor(Color.white);
            _label.color=!enabled?UIPalette.Disabled:UIPalette.Ivory;
            _outline.effectColor=danger?UITheme.Hex("D9645A"):UIPalette.GoldLight;
            _outline.enabled=focus; _mark.SetActive(selected);
            if (_frame != null) _frame.SetState(selected||tabSelected,danger,contrast);
        }
        public void OnSelect(BaseEventData data) { _focused=true;Refresh(); focused?.Invoke(); TitleMenuController.Instance?.PlayCue(false); }
        public void OnDeselect(BaseEventData data) { _focused=false;Refresh(); }
        public void OnPointerEnter(PointerEventData data) { _hover=true; Refresh(); focused?.Invoke(); }
        public void OnPointerExit(PointerEventData data) { _hover=false; Refresh(); }
        public void OnPointerDown(PointerEventData data) { _pressed=true; Refresh(); }
        public void OnPointerUp(PointerEventData data) { _pressed=false; Refresh(); }
        private void LateUpdate()
        {
            if (_button == null) return;
            bool reduced = TitleMenuController.Instance!=null && TitleMenuController.Instance.Settings!=null
                ? TitleMenuController.Instance.Settings.Values.reducedMotion
                : UIManager.Instance!=null && UIManager.Instance.Settings!=null && UIManager.Instance.Settings.Values.reducedMotion;
            float target = reduced || !_button.IsInteractable() ? 1 : _pressed ? .98f : _hover || _focused ? 1.02f : 1;
            float next = reduced ? target : Mathf.MoveTowards(transform.localScale.x,target,Time.unscaledDeltaTime*.25f);
            transform.localScale = Vector3.one * next;
        }
        private void OnDisable() { _hover=false;_focused=false;_pressed=false;transform.localScale=Vector3.one; }
        private void Awake() { Initialize(primary); }
        private void OnEnable() { Refresh(); }
    }
}

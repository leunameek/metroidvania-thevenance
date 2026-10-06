using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // One material animates the painted lagoon; the emblem and controls never move.
    [RequireComponent(typeof(RawImage))]
    public sealed class TitleMenuAtmosphere : MonoBehaviour
    {
        private static readonly int ClockId=Shader.PropertyToID("_AmbientTime");
        private Material _material;
        private bool _focused=true;
        public float Elapsed { get; private set; }
        public bool IsAnimating { get; private set; }

        private void Awake()
        {
            var shader=Resources.Load<Shader>("Nemequene/MenuAtmosphere");
            if(shader==null||!shader.isSupported)return;
            _material=new Material(shader){name="Title atmosphere (instance)"};
            GetComponent<RawImage>().material=_material;
        }
        private void Update()
        {
            var title=TitleMenuController.Instance;
            IsAnimating=_material!=null&&_focused&&title!=null&&title.CurrentView==TitleView.Home
                &&!title.IsLoading&&!title.Settings.Values.reducedMotion;
            if(!IsAnimating)return;
            // Clamp a resumed frame so alt-tab cannot jump the painting forward.
            Elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f);
            _material.SetFloat(ClockId,Elapsed);
        }
        private void OnApplicationFocus(bool focused) { _focused=focused; }
        private void OnDisable() { IsAnimating=false; }
        private void OnDestroy()
        {
            if(_material!=null)Destroy(_material);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Painted kit of «Interfaces de El Asedio de Bacatá» (Resources/Nemequene/Bacata, built by
    // tools/ui/build_bacata_ui_sprites.py). Components are authored at twice their logical size
    // and imported at 200 ppu, so a multiplier of 1 draws them at their 1080p size.
    public static class UIBacata
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        public static Sprite Get(string path)
        {
            if (Cache.TryGetValue(path, out var sprite) && sprite != null) return sprite;
            sprite = Resources.Load<Sprite>("Nemequene/Bacata/" + path);
            Cache[path] = sprite;
            return sprite;
        }
        public static Texture2D Art(string name) => Resources.Load<Texture2D>("Nemequene/Bacata/Art/" + name);
        public static bool Available => Get("Frames/Frame_Panel") != null;

        // Ribbons and lines keep their pointed ends: the height drives the scale and the painted
        // body (55 % of the sprite) matches the control; the rest overhangs as transparent margin.
        public static Image Skin(Transform parent, string sprite, float bodyFraction = .55f)
        {
            var rect = new GameObject("Skin", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; rect.SetAsFirstSibling();
            var image = rect.gameObject.AddComponent<Image>(); image.raycastTarget = false;
            image.sprite = Get(sprite); image.type = Image.Type.Sliced;
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var fit = rect.gameObject.AddComponent<UISpriteFit>(); fit.bodyFraction = bodyFraction; fit.maxBody = 72;
            return image;
        }
        public static void FitSelf(Image image) { var fit = image.gameObject.AddComponent<UISpriteFit>(); fit.self = true; fit.Fit(); }
        public static UIBacataFrame Frame(GameObject target, float scale, bool emblem, bool feather)
        {
            var frame = target.GetComponent<UIBacataFrame>() ?? target.AddComponent<UIBacataFrame>();
            frame.scale = scale; frame.emblem = emblem; frame.feather = feather; frame.Build();
            return frame;
        }
    }

    // Scales a sliced sprite from the rect height so its ends never squash (ribbons, rails, keys).
    [ExecuteAlways]
    public sealed class UISpriteFit : MonoBehaviour
    {
        public float bodyFraction = 1;
        // Fit to this rect's own height (bar tracks) instead of overhanging the parent.
        public bool self;
        // Tall rows (save slots) keep a ribbon of normal height, centred on the row.
        public float maxBody = 0;
        private Image _image;
        private bool _fitting;
        private void OnEnable() { Fit(); }
        private void OnRectTransformDimensionsChange() { Fit(); }
        public void Fit()
        {
            // Moving our own offsets raises this callback again; one pass is enough.
            if (_fitting) return;
            _fitting = true;
            try { FitNow(); } finally { _fitting = false; }
        }
        private void FitNow()
        {
            if (_image == null) _image = GetComponent<Image>();
            var rect = (RectTransform)transform;
            var parent = self ? rect : rect.parent as RectTransform;
            if (_image == null || _image.sprite == null || parent == null) return;
            float height = parent.rect.height;
            if (height < 1) return;
            float body = maxBody > 0 ? Mathf.Min(height, maxBody) : height;
            float extra = self ? 0 : (body / Mathf.Max(.1f, bodyFraction) - height) * .5f;
            // Only a skin stretched over its parent's height overhangs it; a bar track sized by its own
            // rect (self, or fixed anchors) keeps its size.
            bool stretched = !Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y);
            if (!self && stretched && (Mathf.Abs(rect.offsetMax.y - extra) > .5f || Mathf.Abs(rect.offsetMin.y + extra) > .5f))
            {
                rect.offsetMin = new Vector2(rect.offsetMin.x, -extra); rect.offsetMax = new Vector2(rect.offsetMax.x, extra);
            }
            float native = _image.sprite.rect.height / (_image.sprite.pixelsPerUnit / 100f);
            float multiplier = native / (height + extra * 2);
            // Very narrow controls: keep the two ends from overlapping.
            float ends = (_image.sprite.border.x + _image.sprite.border.z) / (_image.sprite.pixelsPerUnit / 100f);
            if (ends > 0 && parent.rect.width > 1) multiplier = Mathf.Max(multiplier, ends / parent.rect.width);
            if (Mathf.Abs(_image.pixelsPerUnitMultiplier - multiplier) > .001f) _image.pixelsPerUnitMultiplier = multiplier;
        }
    }

    // Smoked-parchment panel with an aged-gold rim. The bird emblem and the dark feather are
    // separate images so slicing never stretches them; the feather stays in the lower right,
    // away from the reading area, and fades when the panel is too small for it.
    [RequireComponent(typeof(Image))]
    public sealed class UIBacataFrame : MonoBehaviour
    {
        // 1 draws the rim at its painted 1080p thickness; HUD plates use about half.
        public float scale = .6f;
        public bool emblem = true, feather = true;
        [Range(0, 1)] public float featherAlpha = .55f;
        public Color tint = Color.white;
        private Image _image, _emblem, _feather;
        private bool _contrast;
        private static readonly Vector2 EmblemSize = new Vector2(271, 69), FeatherSize = new Vector2(435, 525);
        private const float EmblemTop = 2, FeatherInset = 45;
        public void Build()
        {
            _image = GetComponent<Image>();
            _image.sprite = UIBacata.Get("Frames/Frame_Panel"); _image.type = Image.Type.Sliced; _image.fillCenter = true;
            _image.pixelsPerUnitMultiplier = 1 / Mathf.Max(.1f, scale);
            if (emblem && _emblem == null) _emblem = Child("Emblem", "Frames/Frame_Emblem", new Vector2(.5f, 1));
            if (feather && _feather == null) _feather = Child("Feather", "Frames/Frame_Feather", Vector2.right);
            if (_emblem != null) _emblem.gameObject.SetActive(emblem);
            if (_feather != null) _feather.gameObject.SetActive(feather);
            Apply(); Layout();
        }
        private Image Child(string name, string sprite, Vector2 anchor)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(transform, false); rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = UIBacata.Get(sprite); image.raycastTarget = false;
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            rect.SetSiblingIndex(0);
            return image;
        }
        public void SetContrast(bool value) { _contrast = value; Apply(); }
        public void SetTint(Color value) { tint = value; Apply(); Layout(); }
        private void Apply()
        {
            if (_image == null) return;
            _image.color = _contrast ? UIPalette.Charcoal : tint;
            if (_emblem != null) { _emblem.gameObject.SetActive(emblem); _emblem.color = _contrast ? UIPalette.Gold : tint; }
            if (_feather != null) _feather.gameObject.SetActive(feather && !_contrast);
        }
        private void OnRectTransformDimensionsChange() { Layout(); }
        private void Layout()
        {
            var rect = (RectTransform)transform;
            float s = Mathf.Max(.1f, scale);
            if (_emblem != null)
            {
                float e = Mathf.Min(s, rect.rect.width * .42f / EmblemSize.x);
                _emblem.rectTransform.sizeDelta = EmblemSize * Mathf.Max(.05f, e);
                _emblem.rectTransform.anchoredPosition = new Vector2(0, -EmblemTop * s);
            }
            if (_feather != null)
            {
                float f = Mathf.Min(s * 1.1f, rect.rect.height * .86f / FeatherSize.y, rect.rect.width * .55f / FeatherSize.x);
                _feather.rectTransform.sizeDelta = FeatherSize * Mathf.Max(.05f, f);
                _feather.rectTransform.anchoredPosition = new Vector2(-FeatherInset * s, FeatherInset * s);
                var c = tint; c.a *= featherAlpha; _feather.color = c;
            }
        }
    }
}

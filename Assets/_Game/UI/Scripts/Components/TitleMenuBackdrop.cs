using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TitleMenuBackdrop : MaskableGraphic
    {
        public bool highContrast;
        public float readingEdge = .40f;
        // Centred screens (results, defeat) veil the whole scene evenly; tint colours the veil.
        public bool even;
        public float strength = .84f;
        // Top veil: darkest at the top edge, clear at readingEdge of the height (HUD, lessons).
        public bool fromTop;
        // Fondo profundo #080A0B of the document palette.
        public Color tint = new Color(.031f,.039f,.043f);
        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect r = rectTransform.rect;
            if (fromTop)
            {
                float y = Mathf.Lerp(r.yMax, r.yMin, readingEdge);
                Color dark = new Color(tint.r, tint.g, tint.b, highContrast ? .95f : strength), clear = new Color(tint.r, tint.g, tint.b, 0);
                vertices.AddVert(new Vector3(r.xMin, y), clear, Vector2.zero); vertices.AddVert(new Vector3(r.xMin, r.yMax), dark, Vector2.up);
                vertices.AddVert(new Vector3(r.xMax, y), clear, Vector2.right); vertices.AddVert(new Vector3(r.xMax, r.yMax), dark, Vector2.one);
                vertices.AddTriangle(0, 1, 2); vertices.AddTriangle(2, 1, 3);
                return;
            }
            float[] stops = {0, readingEdge, Mathf.Min(.90f, readingEdge + .20f), 1};
            float[] alpha = even ? (highContrast ? new[]{.96f,.96f,.96f,.96f} : new[]{strength,strength,strength,strength})
                : highContrast ? new[]{1f,1f,.85f,.35f} : new[]{.98f,.88f,.28f,.06f};
            for (int i=0; i<stops.Length; i++)
            {
                float x = Mathf.Lerp(r.xMin,r.xMax,stops[i]);
                Color shade = new Color(tint.r,tint.g,tint.b,alpha[i]);
                vertices.AddVert(new Vector3(x,r.yMin),shade,Vector2.zero);
                vertices.AddVert(new Vector3(x,r.yMax),shade,Vector2.up);
                if (i==0) continue;
                int n=i*2; vertices.AddTriangle(n-2,n-1,n); vertices.AddTriangle(n,n-1,n+1);
            }
        }
        public void SetContrast(bool value) { highContrast=value; SetVerticesDirty(); }
    }
}

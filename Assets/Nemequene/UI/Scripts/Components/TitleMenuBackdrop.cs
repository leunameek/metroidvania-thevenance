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
        public Color tint = new Color(.086f,.075f,.059f);
        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect r = rectTransform.rect;
            float[] stops = {0, readingEdge, Mathf.Min(.90f, readingEdge + .20f), 1};
            float[] alpha = even ? (highContrast ? new[]{.96f,.96f,.96f,.96f} : new[]{.84f,.84f,.84f,.84f})
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

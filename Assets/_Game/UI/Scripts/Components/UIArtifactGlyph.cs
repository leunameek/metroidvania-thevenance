using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Original abstract silhouettes for the three inspected objects in this slice.
    // They identify UI records and do not claim to reproduce a historical artifact.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIArtifactGlyph : MaskableGraphic
    {
        public int design;
        public bool unlocked;
        public bool selected;
        // Drawn on a gold (selected) plate: dark ink keeps the silhouette readable.
        public bool onGold;
        public void SetState(int index, bool available, bool active)
        { design = index; unlocked = available; selected = active; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            float scale = Mathf.Min(r.width, r.height) * .38f;
            var c = r.center;
            Color ink = !unlocked ? UIPalette.Disabled : onGold ? UIPalette.OnGold : selected ? UIPalette.GoldLight : UIPalette.Gold;
            if (!unlocked)
            {
                Polyline(mesh, c, scale, ink, new Vector2(0,.8f),new Vector2(.6f,0),new Vector2(0,-.8f),new Vector2(-.6f,0),new Vector2(0,.8f));
                return;
            }
            if (design == 0)
            {
                Polyline(mesh,c,scale,ink,new Vector2(-.27f,.86f),new Vector2(.27f,.86f),new Vector2(.27f,.52f),
                    new Vector2(.68f,.29f),new Vector2(.55f,-.60f),new Vector2(.31f,-.82f),new Vector2(-.31f,-.82f),
                    new Vector2(-.55f,-.60f),new Vector2(-.68f,.29f),new Vector2(-.27f,.52f),new Vector2(-.27f,.86f));
                Polyline(mesh,c,scale,ink,new Vector2(-.48f,-.38f),new Vector2(.48f,-.38f));
            }
            else if (design == 1)
            {
                for (int i=0;i<24;i++)
                {
                    float a=i*Mathf.PI/12, b=(i+1)*Mathf.PI/12;
                    Line(mesh,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*scale*.65f,
                        c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*scale*.65f,ink);
                }
                for (int i=0;i<8;i++)
                {
                    float a=i*Mathf.PI/4;
                    Line(mesh,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*scale*.72f,
                        c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*scale,ink);
                }
                Diamond(mesh,c,scale*.20f,ink);
            }
            else
            {
                Polyline(mesh,c,scale,ink,new Vector2(0,1),new Vector2(.60f,.57f),new Vector2(.60f,-.32f),
                    new Vector2(0,-1),new Vector2(-.60f,-.32f),new Vector2(-.60f,.57f),new Vector2(0,1));
                Polyline(mesh,c,scale,ink,new Vector2(-.3f,.20f),new Vector2(0,.47f),new Vector2(.3f,.20f));
                Polyline(mesh,c,scale,ink,new Vector2(-.32f,-.35f),new Vector2(.32f,-.35f));
            }
        }
        private static void Polyline(VertexHelper mesh, Vector2 center, float scale, Color ink, params Vector2[] points)
        { for (int i=1;i<points.Length;i++) Line(mesh,center+points[i-1]*scale,center+points[i]*scale,ink); }
        private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,Color ink)
        {
            var n=new Vector2(-(b-a).y,(b-a).x).normalized*1.7f;
            int i=mesh.currentVertCount;
            mesh.AddVert(a-n,ink,Vector2.zero);mesh.AddVert(a+n,ink,Vector2.zero);
            mesh.AddVert(b+n,ink,Vector2.zero);mesh.AddVert(b-n,ink,Vector2.zero);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
        private static void Diamond(VertexHelper mesh,Vector2 c,float radius,Color ink)
        {
            int i=mesh.currentVertCount;
            mesh.AddVert(c+Vector2.up*radius,ink,Vector2.zero);mesh.AddVert(c+Vector2.right*radius,ink,Vector2.zero);
            mesh.AddVert(c+Vector2.down*radius,ink,Vector2.zero);mesh.AddVert(c+Vector2.left*radius,ink,Vector2.zero);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
    }
}

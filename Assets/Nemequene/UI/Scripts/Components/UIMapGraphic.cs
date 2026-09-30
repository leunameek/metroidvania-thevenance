using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Only receives discovered points. This component never queries the world or reveals geometry.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIMapGraphic : MaskableGraphic
    {
        private readonly List<Vector2> _trail = new List<Vector2>();
        private readonly List<Vector2> _markers = new List<Vector2>();
        private Vector2 _player;
        private bool _highContrast;
        public void SetContrast(bool value) { _highContrast = value; SetVerticesDirty(); }
        public void SetMap(IEnumerable<Vector2> trail, IEnumerable<Vector2> markers, Vector2 player)
        { _trail.Clear(); _trail.AddRange(trail); _markers.Clear(); _markers.AddRange(markers); _player=player; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect=rectTransform.rect; var min=_player-Vector2.one*10; var max=_player+Vector2.one*10;
            foreach(var p in _trail) { min=Vector2.Min(min,p); max=Vector2.Max(max,p); }
            foreach(var p in _markers) { min=Vector2.Min(min,p); max=Vector2.Max(max,p); }
            float scale=Mathf.Min((rect.width-64)/Mathf.Max(1,max.x-min.x),(rect.height-64)/Mathf.Max(1,max.y-min.y));
            Vector2 middle=(min+max)*.5f;
            Vector2 Project(Vector2 p)=>rect.center+(p-middle)*scale;
            Color ink=UITheme.Hex(_highContrast?"E8D8B8":"473225");
            Color highlight=UITheme.Hex(_highContrast?"D0A45B":"9B463A");
            for(int i=1;i<_trail.Count;i++) Line(vh,Project(_trail[i-1]),Project(_trail[i]),3,ink);
            foreach(var point in _markers) Ring(vh,Project(point),8,highlight);
            Ring(vh,Project(_player),13,ink); Ring(vh,Project(_player),5,highlight);
        }
        private static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color color)
        {
            Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
            int i=vh.currentVertCount;
            vh.AddVert(a-n,color,Vector2.zero); vh.AddVert(a+n,color,Vector2.zero); vh.AddVert(b+n,color,Vector2.zero); vh.AddVert(b-n,color,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2); vh.AddTriangle(i,i+2,i+3);
        }
        private static void Ring(VertexHelper vh,Vector2 center,float radius,Color color)
        {
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;
                Line(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,2,color);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Quiet, resolution-independent stonework for every reading surface and control.
    // The stepped corners are an original geometric device, not a historical symbol.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIFrameGraphic : MaskableGraphic
    {
        public bool compact;
        public bool selected;
        public bool danger;
        public bool highContrast;
        public void SetState(bool isSelected, bool isDanger, bool contrast)
        {
            if (selected == isSelected && danger == isDanger && highContrast == contrast) return;
            selected = isSelected; danger = isDanger; highContrast = contrast;
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            if (r.width < 24 || r.height < 24) return;
            Color dark = UIPalette.Charcoal;
            Color gold = danger ? UITheme.Hex("D9645A") : selected ? UIPalette.GoldLight : UIPalette.Gold;
            Color muted = highContrast ? UIPalette.Ivory : UITheme.Hex("8A6A3A");
            float edge = compact ? 4 : 8;
            float corner = Mathf.Min(compact ? 20 : 32, Mathf.Min(r.width, r.height) * .24f);
            Outline(mesh, r, 1, 3, dark);
            Outline(mesh, r, edge, selected || highContrast ? 2 : 1.5f, compact ? muted : gold);
            if (compact)
            {
                // Stepped gold brackets on each corner, as on the reference HUD plates.
                float step = Mathf.Min(10, corner * .5f);
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sy = -1; sy <= 1; sy += 2)
                    {
                        float x = sx < 0 ? r.xMin + edge : r.xMax - edge;
                        float y = sy < 0 ? r.yMin + edge : r.yMax - edge;
                        Line(mesh, new Vector2(x, y), new Vector2(x - sx * corner, y), 2.5f, gold);
                        Line(mesh, new Vector2(x, y), new Vector2(x, y - sy * corner), 2.5f, gold);
                        Line(mesh, new Vector2(x - sx * step, y - sy * step), new Vector2(x - sx * (step + 8), y - sy * step), 2, gold);
                        Line(mesh, new Vector2(x - sx * step, y - sy * step), new Vector2(x - sx * step, y - sy * (step + 8)), 2, gold);
                    }
                if (r.width > 200)
                {
                    Diamond(mesh, new Vector2(r.center.x, r.yMax - edge), 4, gold);
                    Diamond(mesh, new Vector2(r.center.x, r.yMin + edge), 4, gold);
                }
                return;
            }
            float inset = edge + 7;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    float x = sx < 0 ? r.xMin + inset : r.xMax - inset;
                    float y = sy < 0 ? r.yMin + inset : r.yMax - inset;
                    Line(mesh, new Vector2(x, y - sy * corner), new Vector2(x, y), 1.5f, gold);
                    Line(mesh, new Vector2(x, y), new Vector2(x - sx * corner, y), 1.5f, gold);
                    Diamond(mesh, new Vector2(x - sx * corner * .45f, y - sy * corner * .45f), 3, muted);
                }
            if (r.width > 280)
            {
                float y = r.yMax - edge;
                Line(mesh, new Vector2(r.center.x - 44, y), new Vector2(r.center.x - 12, y), 2, gold);
                Line(mesh, new Vector2(r.center.x + 12, y), new Vector2(r.center.x + 44, y), 2, gold);
                Diamond(mesh, new Vector2(r.center.x, y), 5, gold);
            }
        }
        private static void Outline(VertexHelper mesh, Rect r, float inset, float thickness, Color color)
        {
            var a = new Vector2(r.xMin + inset, r.yMin + inset);
            var b = new Vector2(r.xMax - inset, r.yMax - inset);
            Line(mesh, new Vector2(a.x, a.y), new Vector2(b.x, a.y), thickness, color);
            Line(mesh, new Vector2(b.x, a.y), new Vector2(b.x, b.y), thickness, color);
            Line(mesh, new Vector2(b.x, b.y), new Vector2(a.x, b.y), thickness, color);
            Line(mesh, new Vector2(a.x, b.y), new Vector2(a.x, a.y), thickness, color);
        }
        private static void Diamond(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(center + Vector2.up * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.down * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.left * radius, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }
        private static void Line(VertexHelper mesh, Vector2 start, Vector2 end, float thickness, Color color)
        {
            var direction = (end - start).normalized;
            start -= direction * thickness * .5f; end += direction * thickness * .5f;
            var normal = new Vector2(-direction.y, direction.x) * thickness * .5f;
            int i = mesh.currentVertCount;
            mesh.AddVert(start - normal, color, Vector2.zero);
            mesh.AddVert(start + normal, color, Vector2.zero);
            mesh.AddVert(end + normal, color, Vector2.zero);
            mesh.AddVert(end - normal, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }
    }
}

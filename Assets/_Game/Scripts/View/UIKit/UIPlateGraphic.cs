using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum UIPlateKind { Secondary, Primary, Danger, Disabled, Rail, Key, Border }

    // Chamfered plate used by buttons, tabs, key caps and bar rails (reference block 1):
    // gold plate for the main action, carved dark stone for normal actions, ceremonial red for
    // destructive ones. Vector geometry keeps the edge crisp at 720p and at 150 % text.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIPlateGraphic : MaskableGraphic
    {
        public UIPlateKind kind;
        public bool selected, pressed, highContrast;
        public float chamfer = 10;
        public void Set(UIPlateKind value, bool isSelected, bool isPressed, bool contrast)
        {
            if (kind == value && selected == isSelected && pressed == isPressed && highContrast == contrast) return;
            kind = value; selected = isSelected; pressed = isPressed; highContrast = contrast; SetVerticesDirty();
        }
        private void Colors(out Color top, out Color bottom, out Color border)
        {
            if (highContrast)
            {
                bool gold = kind == UIPlateKind.Primary;
                top = bottom = gold ? UIPalette.GoldLight : kind == UIPlateKind.Danger ? UITheme.Hex("4A111C") : UIPalette.Charcoal;
                border = selected ? UIPalette.GoldLight : kind == UIPlateKind.Disabled ? UIPalette.Disabled : Color.white;
                if (gold) border = selected ? Color.white : UIPalette.Charcoal;
                return;
            }
            switch (kind)
            {
                // Fallback colours of the painted kit: crimson ribbon, smoked panel, aged gold rims.
                case UIPlateKind.Primary:
                    top = UITheme.Hex(selected ? "6A2030" : "561A27"); bottom = UITheme.Hex(selected ? "3A1219" : "2C0E15");
                    border = selected ? UIPalette.GoldLight : UIPalette.Gold; break;
                case UIPlateKind.Danger:
                    top = UITheme.Hex(selected ? "8A2235" : "6E1B2A"); bottom = UITheme.Hex(selected ? "4A111C" : "3A0D16");
                    border = selected ? UIPalette.Danger : UIPalette.Crimson; break;
                case UIPlateKind.Disabled:
                    top = UITheme.Hex("17191B"); bottom = UITheme.Hex("0F1112"); border = UITheme.Hex("4A463F"); break;
                case UIPlateKind.Rail:
                    top = UITheme.Hex("060708"); bottom = UITheme.Hex("111516"); border = UIPalette.GoldDeep; break;
                case UIPlateKind.Key:
                    top = UITheme.Hex("1B1E20"); bottom = UITheme.Hex("0E1011"); border = UIPalette.Gold; break;
                case UIPlateKind.Border:
                    top = bottom = Color.clear; border = UIPalette.Gold; break;
                default:
                    top = UITheme.Hex(selected ? "32171F" : "181B1D"); bottom = UITheme.Hex(selected ? "1E0E13" : "0E1011");
                    border = selected ? UIPalette.GoldLight : UIPalette.GoldDeep; break;
            }
            if (pressed) { var swap = top; top = bottom; bottom = swap; }
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            if (r.width < 4 || r.height < 4) return;
            Colors(out var top, out var bottom, out var border);
            float tint = color.a;
            top.a *= tint; bottom.a *= tint; border.a *= tint;
            float c = Mathf.Min(chamfer, Mathf.Min(r.width, r.height) * .3f);
            bool thin = r.height < 20;
            if (selected && !thin) Outline(mesh, r, -3, c + 2, 2, new Color(border.r, border.g, border.b, .55f * tint));
            if (kind != UIPlateKind.Border)
            {
                var dark = UIPalette.Charcoal; dark.a = kind == UIPlateKind.Rail ? .92f * tint : tint;
                Fill(mesh, r, 0, c, dark, dark);
                Fill(mesh, r, thin ? 1 : 2, c, top, bottom);
            }
            float inset = thin ? .5f : kind == UIPlateKind.Key ? 2 : 3;
            Outline(mesh, r, inset, Mathf.Max(0, c - inset * .4f), thin ? 1 : selected ? 2 : 1.5f, border);
            if (kind == UIPlateKind.Primary && !highContrast && !thin)
            {
                var sheen = new Color(1, .96f, .82f, .45f * tint);
                Line(mesh, new Vector2(r.xMin + c + 2, r.yMax - inset - 2.5f), new Vector2(r.xMax - c - 2, r.yMax - inset - 2.5f), 1, sheen);
            }
            if ((kind == UIPlateKind.Primary || kind == UIPlateKind.Secondary || kind == UIPlateKind.Danger) && r.width > 180 && r.height >= 40)
            {
                Diamond(mesh, new Vector2(r.xMin + inset, r.center.y), 4, border);
                Diamond(mesh, new Vector2(r.xMax - inset, r.center.y), 4, border);
            }
        }
        private static Vector2[] Octagon(Rect r, float inset, float c)
        {
            float x0 = r.xMin + inset, x1 = r.xMax - inset, y0 = r.yMin + inset, y1 = r.yMax - inset;
            return new[]
            {
                new Vector2(x0 + c, y0), new Vector2(x1 - c, y0), new Vector2(x1, y0 + c), new Vector2(x1, y1 - c),
                new Vector2(x1 - c, y1), new Vector2(x0 + c, y1), new Vector2(x0, y1 - c), new Vector2(x0, y0 + c)
            };
        }
        private static void Fill(VertexHelper mesh, Rect r, float inset, float c, Color top, Color bottom)
        {
            var points = Octagon(r, inset, c);
            float y0 = r.yMin + inset, y1 = r.yMax - inset;
            int start = mesh.currentVertCount;
            mesh.AddVert(r.center, Color.Lerp(bottom, top, .5f), Vector2.zero);
            foreach (var p in points) mesh.AddVert(p, Color.Lerp(bottom, top, Mathf.InverseLerp(y0, y1, p.y)), Vector2.zero);
            for (int i = 0; i < points.Length; i++) mesh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % points.Length);
        }
        private static void Outline(VertexHelper mesh, Rect r, float inset, float c, float thickness, Color color)
        {
            var points = Octagon(r, inset, Mathf.Max(0, c));
            for (int i = 0; i < points.Length; i++) Line(mesh, points[i], points[(i + 1) % points.Length], thickness, color);
        }
        public static void Diamond(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(center + Vector2.up * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.down * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.left * radius, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
        }
        public static void Line(VertexHelper mesh, Vector2 start, Vector2 end, float thickness, Color color)
        {
            var direction = (end - start).normalized;
            // Extend each segment by half its width so chamfered joints close without gaps.
            start -= direction * thickness * .5f; end += direction * thickness * .5f;
            var normal = new Vector2(-direction.y, direction.x) * thickness * .5f;
            int i = mesh.currentVertCount;
            mesh.AddVert(start - normal, color, Vector2.zero); mesh.AddVert(start + normal, color, Vector2.zero);
            mesh.AddVert(end + normal, color, Vector2.zero); mesh.AddVert(end - normal, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
        }
    }
}

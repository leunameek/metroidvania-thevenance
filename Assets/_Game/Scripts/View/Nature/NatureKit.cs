using System.Collections.Generic;
using UnityEngine;

// Shared pieces of the open places: the stylized nature shaders (Resources/Nature), their
// palettes and the meshes built from code (blade clumps, flowers, the water sheet).
public static class NatureKit
{
    private static readonly Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

    // Resources/Nature/<file> first (always in the build), then by name.
    public static Shader Shader(string file, string name)
    {
        if (Shaders.TryGetValue(file, out var s) && s != null) return s;
        s = Resources.Load<Shader>("Nature/" + file);
        if (s == null) s = UnityEngine.Shader.Find(name);
        Shaders[file] = s;
        return s;
    }

    public static Material Ground() => Make("StylizedGround", "Nemequene/Stylized Ground");
    public static Material Grass() { var m = Make("StylizedGrass", "Nemequene/Stylized Grass"); if (m != null) m.enableInstancing = true; return m; }
    public static Material Water() => Make("StylizedWater", "Nemequene/Stylized Water");
    public static Material Foliage() => Make("StylizedFoliage", "Nemequene/Stylized Foliage");

    private static Material Make(string file, string name)
    {
        var shader = Shader(file, name);
        if (shader == null) { Debug.LogWarning("Falta el shader " + name); return null; }
        return new Material(shader) { name = file };
    }

    // ---------------------------------------------------------------- palettes

    public struct Palette
    {
        public Color GrassA, GrassB, Dry, Dirt, DirtDark, Slope, Shore, Tip, TipDry;
        public float DryAmount;
    }

    // The savanna of Bacatá: deep fresh green with sunny patches of straw.
    public static readonly Palette Savanna = new Palette
    {
        GrassA = new Color(.30f, .47f, .19f), GrassB = new Color(.50f, .62f, .25f), Dry = new Color(.72f, .66f, .36f),
        Dirt = new Color(.60f, .46f, .30f), DirtDark = new Color(.47f, .35f, .23f), Slope = new Color(.52f, .42f, .29f),
        Shore = new Color(.40f, .34f, .25f), Tip = new Color(.60f, .76f, .30f), TipDry = new Color(.84f, .80f, .46f), DryAmount = .4f,
    };

    // The páramo around Iguaque: olive green and golden straw (pajonal).
    public static readonly Palette Paramo = new Palette
    {
        GrassA = new Color(.36f, .44f, .22f), GrassB = new Color(.55f, .58f, .30f), Dry = new Color(.76f, .66f, .38f),
        Dirt = new Color(.52f, .44f, .32f), DirtDark = new Color(.42f, .35f, .26f), Slope = new Color(.48f, .44f, .36f),
        Shore = new Color(.36f, .31f, .24f), Tip = new Color(.74f, .76f, .40f), TipDry = new Color(.90f, .80f, .50f), DryAmount = .7f,
    };

    // The sky world's gardens: bright, lush and flowered.
    public static readonly Palette SkyGarden = new Palette
    {
        GrassA = new Color(.30f, .52f, .24f), GrassB = new Color(.52f, .70f, .30f), Dry = new Color(.70f, .70f, .38f),
        Dirt = new Color(.62f, .50f, .34f), DirtDark = new Color(.50f, .40f, .27f), Slope = new Color(.55f, .46f, .32f),
        Shore = new Color(.42f, .36f, .27f), Tip = new Color(.74f, .88f, .40f), TipDry = new Color(.90f, .86f, .52f), DryAmount = .2f,
    };

    public static void Apply(Material m, Palette p)
    {
        if (m == null) return;
        m.SetColor("_GrassA", p.GrassA); m.SetColor("_GrassB", p.GrassB); m.SetColor("_Dry", p.Dry);
        m.SetFloat("_DryAmount", p.DryAmount);
        if (m.HasProperty("_Dirt")) { m.SetColor("_Dirt", p.Dirt); m.SetColor("_DirtDark", p.DirtDark); m.SetColor("_Slope", p.Slope); m.SetColor("_Shore", p.Shore); }
        if (m.HasProperty("_Tip")) { m.SetColor("_Tip", p.Tip); m.SetColor("_TipDry", p.TipDry); }
    }

    // ---------------------------------------------------------------- meshes

    // A clump of tapered blades leaning out from one root. Vertex colour r = height along the
    // blade, g = random per blade (wind phase and tint); normals point up for soft lighting.
    public static Mesh Clump(int blades, float minHeight, float maxHeight, float spread, float width, int seed)
    {
        var random = new System.Random(seed);
        float R() => (float)random.NextDouble();
        var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
        const int segments = 3;
        for (int b = 0; b < blades; b++)
        {
            float yaw = R() * Mathf.PI * 2, lean = .15f + R() * .5f, height = Mathf.Lerp(minHeight, maxHeight, R());
            float w = width * (.7f + R() * .6f), g = R();
            var root = new Vector3(Mathf.Cos(yaw), 0, Mathf.Sin(yaw)) * spread * Mathf.Sqrt(R());
            var outward = new Vector3(Mathf.Cos(yaw + .4f), 0, Mathf.Sin(yaw + .4f));
            var side = new Vector3(-outward.z, 0, outward.x) * (R() < .5f ? 1 : -1);
            side = Quaternion.AngleAxis((R() - .5f) * 80f, Vector3.up) * side;
            int start = vertices.Count;
            for (int s = 0; s <= segments; s++)
            {
                float t = s / (float)segments;
                // Curved blade: rises, then leans out more towards the tip.
                Vector3 c = root + Vector3.up * height * t + outward * lean * height * t * t;
                float half = w * .5f * (1 - t * .92f);
                if (s < segments)
                {
                    vertices.Add(c - side * half); vertices.Add(c + side * half);
                    colors.Add(new Color(t, g, 0)); colors.Add(new Color(t, g, 0));
                }
                else { vertices.Add(c); colors.Add(new Color(1, g, 0)); }
            }
            for (int s = 0; s < segments - 1; s++)
            {
                int a = start + s * 2;
                triangles.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
            }
            int last = start + (segments - 1) * 2;
            triangles.AddRange(new[] { last, last + 2, last + 1 });
        }
        return Finish("Mata", vertices, colors, triangles);
    }

    // Wild flowers: thin stems, each topped by a small star of petals (vertex colour b = petal).
    public static Mesh Flowers(int count, float minHeight, float maxHeight, float spread, float size, int seed)
    {
        var random = new System.Random(seed);
        float R() => (float)random.NextDouble();
        var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
        for (int f = 0; f < count; f++)
        {
            float yaw = R() * Mathf.PI * 2, height = Mathf.Lerp(minHeight, maxHeight, R()), g = R();
            var root = new Vector3(Mathf.Cos(yaw), 0, Mathf.Sin(yaw)) * spread * Mathf.Sqrt(R());
            var lean = new Vector3(R() - .5f, 0, R() - .5f) * .12f;
            var top = root + Vector3.up * height + lean;
            var side = new Vector3(Mathf.Cos(yaw + 1.3f), 0, Mathf.Sin(yaw + 1.3f)) * .008f;
            int s0 = vertices.Count;
            vertices.Add(root - side); vertices.Add(root + side); vertices.Add(top);
            colors.Add(new Color(0, g, 0)); colors.Add(new Color(0, g, 0)); colors.Add(new Color(1, g, 0));
            triangles.AddRange(new[] { s0, s0 + 2, s0 + 1 });
            // Five petals around the head, tilted up a little.
            int centre = vertices.Count;
            vertices.Add(top + Vector3.up * .01f); colors.Add(new Color(1, g, 1));
            for (int p = 0; p < 5; p++)
            {
                float a = p * Mathf.PI * 2 / 5 + yaw, b = a + Mathf.PI / 5;
                vertices.Add(top + new Vector3(Mathf.Cos(a), .35f, Mathf.Sin(a)) * size); colors.Add(new Color(1, g, 1));
                vertices.Add(top + new Vector3(Mathf.Cos(b), .2f, Mathf.Sin(b)) * size * .45f); colors.Add(new Color(1, g, 1));
            }
            for (int p = 0; p < 10; p++)
                triangles.AddRange(new[] { centre, centre + 1 + p, centre + 1 + (p + 1) % 10 });
        }
        return Finish("Flores", vertices, colors, triangles);
    }

    private static Mesh Finish(string name, List<Vector3> vertices, List<Color> colors, List<int> triangles)
    {
        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0);
        var normals = new Vector3[vertices.Count];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        return mesh;
    }

    // A flat elliptical sheet (the water) in rings, dense enough for the gentle vertex swell.
    public static Mesh Ellipse(float halfX, float halfZ, int rings, int segments)
    {
        var vertices = new List<Vector3> { Vector3.zero };
        var triangles = new List<int>();
        for (int k = 1; k <= rings; k++)
        {
            float r = k / (float)rings;
            for (int s = 0; s < segments; s++)
            {
                float a = s * Mathf.PI * 2 / segments;
                vertices.Add(new Vector3(Mathf.Cos(a) * halfX * r, 0, Mathf.Sin(a) * halfZ * r));
            }
        }
        for (int s = 0; s < segments; s++) triangles.AddRange(new[] { 0, 1 + (s + 1) % segments, 1 + s });
        for (int k = 1; k < rings; k++)
        {
            int inner = 1 + (k - 1) * segments, outer = 1 + k * segments;
            for (int s = 0; s < segments; s++)
            {
                int s1 = (s + 1) % segments;
                triangles.AddRange(new[] { inner + s, inner + s1, outer + s, outer + s, inner + s1, outer + s1 });
            }
        }
        var mesh = new Mesh { name = "Agua" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }
}

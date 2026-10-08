using System;
using System.Collections.Generic;
using UnityEngine;

// The ground of an open place: a round sheet of terrain shaped by a height function (flat where
// the scenes are acted, rolling hills and cerros beyond), painted by Stylized Ground with its dirt
// paths and clearings. The same height and dirt are read from code to plant grass, rocks and
// trees on the surface and to keep them off the paths.
public sealed class NatureGround
{
    public readonly Func<float, float, float> Height;
    public float ShoreLevel = -100f;
    public Material Material { get; private set; }

    private readonly List<Vector4> _paths = new List<Vector4>();
    private readonly List<Vector4> _pathInfo = new List<Vector4>();
    private readonly List<Vector4> _clearings = new List<Vector4>();
    private readonly List<Vector3> _bare = new List<Vector3>();

    public NatureGround(Func<float, float, float> height) { Height = height; }

    public float At(Vector3 p) => Height(p.x, p.z);
    public Vector3 On(Vector3 p) => new Vector3(p.x, Height(p.x, p.z), p.z);

    // A trodden path between two points (world xz).
    public void Path(Vector3 a, Vector3 b, float width, float strength = 1f)
    {
        _paths.Add(new Vector4(a.x, a.z, b.x, b.z));
        _pathInfo.Add(new Vector4(width, strength, 0, 0));
    }

    // A round patch of earth (a yard, a meeting ground).
    public void Clearing(Vector3 centre, float radius, float strength = 1f) => _clearings.Add(new Vector4(centre.x, centre.z, radius, strength));

    // No grass here (inside houses, under rocks), without painting earth.
    public void Bare(Vector3 centre, float radius) => _bare.Add(new Vector3(centre.x, centre.z, radius));

    // Dirt amount 0..1 at a point, as the shader draws it (without the ragged edge).
    public float Dirt(float x, float z)
    {
        float m = 0;
        var p = new Vector2(x, z);
        for (int i = 0; i < _paths.Count; i++)
        {
            var a = new Vector2(_paths[i].x, _paths[i].y); var b = new Vector2(_paths[i].z, _paths[i].w);
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Mathf.Max(ba.sqrMagnitude, 1e-4f));
            float d = (pa - ba * h).magnitude, w = _pathInfo[i].x * .5f;
            m = Mathf.Max(m, (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(w * .5f, w * 1.2f, d))) * _pathInfo[i].y);
        }
        foreach (var c in _clearings)
        {
            float d = Vector2.Distance(p, new Vector2(c.x, c.y));
            m = Mathf.Max(m, (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(c.z * .55f, c.z * 1.05f, d))) * c.w);
        }
        return m;
    }

    // Whether grass grows here: not on paths, not in bare spots, not under the water.
    public bool Grassy(float x, float z)
    {
        if (Dirt(x, z) > .2f) return false;
        foreach (var b in _bare) if ((new Vector2(x, z) - new Vector2(b.x, b.y)).sqrMagnitude < b.z * b.z) return false;
        return Height(x, z) > ShoreLevel + .12f;
    }

    // Builds the terrain sheet: rings that tighten towards the centre, where the scenes are.
    public GameObject Build(Transform parent, string name, Vector3 centre, float radius, NatureKit.Palette palette, int rings = 96, int segments = 180)
    {
        var vertices = new List<Vector3>(); var triangles = new List<int>();
        vertices.Add(new Vector3(0, Height(centre.x, centre.z), 0));
        for (int k = 1; k <= rings; k++)
        {
            float r = radius * Mathf.Pow(k / (float)rings, 1.7f);
            for (int s = 0; s < segments; s++)
            {
                float a = s * Mathf.PI * 2 / segments;
                float x = centre.x + Mathf.Cos(a) * r, z = centre.z + Mathf.Sin(a) * r;
                vertices.Add(new Vector3(x - centre.x, Height(x, z), z - centre.z));
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
        var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(centre.x, 0, centre.z);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        Material = NatureKit.Ground();
        if (Material != null)
        {
            NatureKit.Apply(Material, palette);
            Material.SetFloat("_ShoreLevel", ShoreLevel);
            var paths = new Vector4[8]; var info = new Vector4[8]; var clearings = new Vector4[16];
            for (int i = 0; i < Mathf.Min(8, _paths.Count); i++) { paths[i] = _paths[i]; info[i] = _pathInfo[i]; }
            for (int i = 0; i < Mathf.Min(16, _clearings.Count); i++) clearings[i] = _clearings[i];
            Material.SetVectorArray("_Paths", paths); Material.SetVectorArray("_PathInfo", info);
            Material.SetVectorArray("_Clearings", clearings);
            Material.SetFloat("_PathCount", Mathf.Min(8, _paths.Count)); Material.SetFloat("_ClearingCount", Mathf.Min(16, _clearings.Count));
            renderer.sharedMaterial = Material;
        }
        return go;
    }

    // ---------------------------------------------------------------- shaping helpers

    public static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));

    // Rolling relief 0..1 (two octaves of Perlin noise, offset so places do not repeat).
    public static float Rolling(float x, float z, float scale, float seed)
    {
        return Mathf.PerlinNoise(x * scale + seed, z * scale - seed) * .65f + Mathf.PerlinNoise(x * scale * 2.7f - seed, z * scale * 2.7f + seed * .5f) * .35f;
    }
}

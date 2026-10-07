using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// A field of grass clumps or flowers drawn with GPU instancing (no GameObject per clump). The
// instances are grouped in square cells so each batch is culled on its own when off camera.
public sealed class NatureGrass : MonoBehaviour
{
    private const float Cell = 14f;
    private Mesh _mesh;
    private Material _material;
    private readonly List<(Matrix4x4[] matrices, Bounds bounds)> _batches = new List<(Matrix4x4[], Bounds)>();

    public int Count { get; private set; }

    public static NatureGrass Create(Transform parent, string name, Mesh mesh, Material material, IList<Matrix4x4> instances)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var field = go.AddComponent<NatureGrass>();
        field._mesh = mesh; field._material = material;
        field.Build(instances);
        return field;
    }

    private void Build(IList<Matrix4x4> instances)
    {
        Count = instances.Count;
        var cells = new Dictionary<Vector2Int, List<Matrix4x4>>();
        foreach (var m in instances)
        {
            var key = new Vector2Int(Mathf.FloorToInt(m.m03 / Cell), Mathf.FloorToInt(m.m23 / Cell));
            if (!cells.TryGetValue(key, out var list)) cells[key] = list = new List<Matrix4x4>();
            list.Add(m);
        }
        float reach = _mesh != null ? _mesh.bounds.extents.magnitude * 2f + .6f : 2f;
        foreach (var list in cells.Values)
            for (int start = 0; start < list.Count; start += 1023)
            {
                int n = Mathf.Min(1023, list.Count - start);
                var batch = list.GetRange(start, n).ToArray();
                var b = new Bounds(batch[0].GetColumn(3), Vector3.zero);
                foreach (var m in batch) b.Encapsulate((Vector3)m.GetColumn(3));
                b.Expand(reach);
                _batches.Add((batch, b));
            }
    }

    private void Update()
    {
        if (_mesh == null || _material == null) return;
        foreach (var (matrices, bounds) in _batches)
        {
            var rp = new RenderParams(_material)
            {
                worldBounds = bounds, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = gameObject.layer,
            };
            Graphics.RenderMeshInstanced(rp, _mesh, 0, matrices);
        }
    }

    // Jittered-grid scatter over a disc: keep(x, z) gives the chance (0..1) of a clump there,
    // height(x, z) the ground under it. Random yaw, size within the range.
    public static List<Matrix4x4> Scatter(Vector3 centre, float radius, float spacing, Func<float, float, float> keep, Func<float, float, float> height,
        Vector2 size, int seed)
    {
        var random = new System.Random(seed);
        float R() => (float)random.NextDouble();
        var result = new List<Matrix4x4>();
        int n = Mathf.CeilToInt(radius / spacing);
        for (int i = -n; i <= n; i++)
            for (int j = -n; j <= n; j++)
            {
                float x = centre.x + (i + R() - .5f) * spacing, z = centre.z + (j + R() - .5f) * spacing;
                float dx = x - centre.x, dz = z - centre.z;
                if (dx * dx + dz * dz > radius * radius) { R(); R(); R(); continue; }
                float chance = keep(x, z);
                float roll = R(), yaw = R() * 360f, s = Mathf.Lerp(size.x, size.y, R());
                if (roll >= chance) continue;
                result.Add(Matrix4x4.TRS(new Vector3(x, height(x, z) - .02f, z), Quaternion.Euler(0, yaw, 0), Vector3.one * s));
            }
        return result;
    }
}

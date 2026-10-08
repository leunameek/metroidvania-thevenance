using System.Collections.Generic;
using UnityEngine;

// The low vegetation of the islands (E06, built as grey "Mata" spheres by the builder) becomes
// living garden beds at load: instanced grass clumps and wild flowers swaying in the high wind.
// The grey stand-in stays in the scene, hidden, so the builder's inventory is unchanged.
public static class MSGardens
{
    private const float Radius = .85f;

    public static void Dress(Transform parent)
    {
        var grass = new List<Matrix4x4>(); var flowers = new List<Matrix4x4>();
        var random = new System.Random(5);
        float R() => (float)random.NextDouble();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!t.name.StartsWith("MS_E06")) continue;
            foreach (var r in t.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            Vector3 centre = t.position;
            float scale = Mathf.Max(.6f, t.lossyScale.x);
            // A dense heart thinning to the rim, and a ring of flowers through it.
            for (int i = 0; i < 26; i++)
            {
                float a = R() * Mathf.PI * 2, d = Mathf.Sqrt(R()) * Radius * scale;
                float s = Mathf.Lerp(1.25f, .7f, d / (Radius * scale)) * (.85f + R() * .3f);
                grass.Add(Matrix4x4.TRS(centre + new Vector3(Mathf.Cos(a) * d, -.02f, Mathf.Sin(a) * d), Quaternion.Euler(0, R() * 360, 0), Vector3.one * s));
            }
            for (int i = 0; i < 7; i++)
            {
                float a = R() * Mathf.PI * 2, d = (.25f + R() * .7f) * Radius * scale;
                flowers.Add(Matrix4x4.TRS(centre + new Vector3(Mathf.Cos(a) * d, -.02f, Mathf.Sin(a) * d), Quaternion.Euler(0, R() * 360, 0), Vector3.one * (.9f + R() * .4f)));
            }
        }
        if (grass.Count == 0) return;
        var root = new GameObject("MS_Jardines").transform;
        root.SetParent(parent, false);
        Field(root, "Matas", NatureKit.Clump(9, .3f, .62f, .16f, .06f, 41), grass, .45f);
        Field(root, "Flores", NatureKit.Flowers(4, .3f, .5f, .2f, .05f, 42), flowers, .4f);
    }

    private static void Field(Transform root, string name, Mesh mesh, List<Matrix4x4> instances, float wind)
    {
        var material = NatureKit.Grass();
        if (material == null) return;
        NatureKit.Apply(material, NatureKit.SkyGarden);
        material.SetFloat("_WindStrength", wind);
        material.SetFloat("_WindSpeed", 1.3f);
        material.SetColor("_FlowerA", new Color(.98f, .80f, .28f));
        material.SetColor("_FlowerB", new Color(.96f, .55f, .62f));
        material.SetColor("_FlowerC", new Color(.98f, .96f, .90f));
        NatureGrass.Create(root, name, mesh, material, instances);
    }
}

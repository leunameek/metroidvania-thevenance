using System.Collections.Generic;
using UnityEngine;

// The nature of the plaza, dressed at load like the sky gardens (2026-10-07): the fountain's
// basins take the living water of the lagoon (refraction, sky reflection, foam on the rims), the
// soil of the four planters grows instanced grass and wild flowers, and the trees and shrubs
// sway in the wind.
public static class PlazaGardens
{
    public static void Dress(Transform parent)
    {
        var root = new GameObject("Plaza_Jardines").transform;
        root.SetParent(parent, false);
        Water();
        var grass = new List<Matrix4x4>(); var flowers = new List<Matrix4x4>();
        var random = new System.Random(11);
        float R() => (float)random.NextDouble();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            string n = t.name;
            if (n.StartsWith("MODEL_Arbol")) NatureFoliage.Sway(t, .025f, .012f);
            else if (n == "Shrub") NatureFoliage.Sway(t, .05f, .015f);
            else if (n.StartsWith("MODEL_Jardinera")) Bed(t, grass, flowers, R);
        }
        if (grass.Count > 0) Field(root, "Cesped", NatureKit.Clump(9, .16f, .34f, .12f, .045f, 61), grass, .35f);
        if (flowers.Count > 0) Field(root, "Flores", NatureKit.Flowers(4, .22f, .4f, .16f, .045f, 62), flowers, .3f);
    }

    // The fountain's still surfaces (Water, Upper water) become clear water over the stone.
    private static void Water()
    {
        Material material = null;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (t.name != "Water" && t.name != "Upper water") continue;
            if (t.parent == null || !t.parent.name.Contains("Fountain")) continue;
            var renderer = t.GetComponent<MeshRenderer>();
            if (renderer == null) continue;
            if (material == null)
            {
                material = NatureKit.Water();
                if (material == null) return;
                material.SetColor("_ShallowColor", new Color(.58f, .88f, .86f));
                material.SetColor("_DeepColor", new Color(.07f, .32f, .40f));
                material.SetFloat("_Clarity", .55f);
                material.SetFloat("_FoamDepth", .12f);
                material.SetFloat("_RippleScale", 2.6f);
                material.SetFloat("_RippleStrength", .3f);
                material.SetFloat("_FlowSpeed", .45f);
                material.SetFloat("_Reflection", .5f);
            }
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    // The soil of a planter: the Tripo planters share one shape (soil at 43.5 % of their height,
    // an open ring of 27 % of their width around the tree), measured once by casting onto the
    // mesh; their meshes are not readable in a build, so the numbers stand here.
    private const float SoilLevel = .435f, SoilRadius = .27f, TrunkClear = .5f;

    private static void Bed(Transform planter, List<Matrix4x4> grass, List<Matrix4x4> flowers, System.Func<float> R)
    {
        Bounds bounds = default; bool any = false;
        foreach (var r in planter.GetComponentsInChildren<Renderer>())
            if (any) bounds.Encapsulate(r.bounds); else { bounds = r.bounds; any = true; }
        if (!any) return;
        float soil = bounds.min.y + bounds.size.y * SoilLevel;
        float radius = Mathf.Min(bounds.size.x, bounds.size.z) * SoilRadius;
        Vector3 centre = bounds.center;
        const float step = .13f;
        for (float x = -radius; x <= radius; x += step)
            for (float z = -radius; z <= radius; z += step)
            {
                float px = x + (R() - .5f) * step, pz = z + (R() - .5f) * step;
                float d = Mathf.Sqrt(px * px + pz * pz);
                if (d > radius || d < TrunkClear) { R(); R(); continue; }
                var at = new Vector3(centre.x + px, soil, centre.z + pz);
                // Thinner and shorter toward the stone rim.
                float edge = Mathf.InverseLerp(radius, radius * .7f, d);
                if (R() < .8f * (.4f + .6f * edge)) grass.Add(Matrix4x4.TRS(at, Quaternion.Euler(0, R() * 360, 0), Vector3.one * (.75f + R() * .45f) * (.7f + .3f * edge)));
                if (R() < .09f) flowers.Add(Matrix4x4.TRS(at, Quaternion.Euler(0, R() * 360, 0), Vector3.one * (.85f + R() * .35f)));
            }
    }

    private static void Field(Transform root, string name, Mesh mesh, List<Matrix4x4> instances, float wind)
    {
        var material = NatureKit.Grass();
        if (material == null) return;
        NatureKit.Apply(material, NatureKit.Savanna);
        material.SetFloat("_WindStrength", wind);
        material.SetFloat("_WindSpeed", 1.1f);
        material.SetColor("_FlowerA", new Color(.98f, .78f, .25f));
        material.SetColor("_FlowerB", new Color(.92f, .36f, .42f));
        material.SetColor("_FlowerC", new Color(.98f, .96f, .90f));
        NatureGrass.Create(root, name, mesh, material, instances);
    }
}

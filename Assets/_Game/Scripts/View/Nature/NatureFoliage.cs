using System.Collections.Generic;
using UnityEngine;

// Puts a textured plant (reeds, frailejón, tree, bush) on the Stylized Foliage shader so it
// sways with the grass: its own albedo, the sway measured from its base to its top.
public static class NatureFoliage
{
    private static readonly Dictionary<Material, Material> Cache = new Dictionary<Material, Material>();
    private static readonly int SwayBase = Shader.PropertyToID("_SwayBase"), SwayHeight = Shader.PropertyToID("_SwayHeight");
    private static readonly int WindStrength = Shader.PropertyToID("_WindStrength"), Flutter = Shader.PropertyToID("_Flutter");

    public static void Sway(Transform plant, float strength, float flutter = .015f)
    {
        if (plant == null) return;
        foreach (var renderer in plant.GetComponentsInChildren<MeshRenderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = Swaying(materials[i]);
            renderer.sharedMaterials = materials;
            var b = renderer.bounds;
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat(SwayBase, b.min.y);
            block.SetFloat(SwayHeight, Mathf.Max(.2f, b.size.y));
            block.SetFloat(WindStrength, strength);
            block.SetFloat(Flutter, flutter);
            renderer.SetPropertyBlock(block);
        }
    }

    private static Material Swaying(Material source)
    {
        if (source == null) return null;
        if (Cache.TryGetValue(source, out var m) && m != null) return m;
        m = NatureKit.Foliage();
        if (m == null) return source;
        m.name = source.name + " (viento)";
        var albedo = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
        if (albedo != null) m.SetTexture("_BaseMap", albedo);
        if (source.HasProperty("_BaseColor")) m.SetColor("_BaseColor", source.GetColor("_BaseColor"));
        Cache[source] = m;
        return m;
    }
}

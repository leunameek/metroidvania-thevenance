using UnityEngine;

// The fountain of the plaza takes the living water of the lagoon at load (refraction, sky
// reflection, foam where the jets fall). The planter grass and the swaying trees were taken out
// (2026-10-07 playtest: white glows on some GPUs around them; the plaza's own plants stay as
// modelled).
public static class PlazaGardens
{
    public static void Dress(Transform parent) => Water();

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
}

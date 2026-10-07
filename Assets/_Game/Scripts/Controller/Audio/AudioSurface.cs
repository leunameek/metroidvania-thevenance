using UnityEngine;

// Optional marker for floors whose material the footsteps cannot guess from the names
// ("piedra", "cueva", "tierra", "madera", "agua").
public sealed class AudioSurface : MonoBehaviour
{
    public string surface = "piedra";

    public static string Of(Collider collider)
    {
        if (collider == null) return GameAudio.DefaultSurface;
        var marker = collider.GetComponentInParent<AudioSurface>();
        if (marker != null && !string.IsNullOrEmpty(marker.surface)) return marker.surface;
        string name = collider.name.ToLowerInvariant();
        var renderer = collider.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null) name += " " + renderer.sharedMaterial.name.ToLowerInvariant();
        if (Contains(name, "agua", "water", "charco", "laguna")) return "agua";
        if (Contains(name, "madera", "wood", "tabla", "puente", "plank", "tronco")) return "madera";
        if (Contains(name, "pasto", "grass", "cesped", "césped", "hierba", "tierra", "dirt", "earth", "barro")) return "tierra";
        if (Contains(name, "cueva", "caverna", "cave")) return "cueva";
        return GameAudio.DefaultSurface;
    }

    private static bool Contains(string text, params string[] keys)
    {
        foreach (var k in keys) if (text.Contains(k)) return true;
        return false;
    }
}

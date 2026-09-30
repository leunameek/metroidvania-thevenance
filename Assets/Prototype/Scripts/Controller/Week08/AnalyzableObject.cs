using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class AnalyzableObject : MonoBehaviour
{
    [SerializeField] private AnalyzableObjectData data;
    public AnalyzableObjectData Data => data;
    public bool Completed { get; private set; }
    private Renderer[] _surfaces;
    private MaterialPropertyBlock _properties;

    public void SetCompleted() { Completed = true; SetHighlighted(false); }

    public void SetHighlighted(bool highlighted)
    {
        if (_surfaces == null) _surfaces = GetComponentsInChildren<Renderer>();
        if (_properties == null) _properties = new MaterialPropertyBlock();
        foreach (Renderer surface in _surfaces)
        {
            if (surface.sharedMaterial == null || !surface.sharedMaterial.HasProperty("_BaseColor")) continue;
            Color baseColor = surface.sharedMaterial.GetColor("_BaseColor");
            _properties.SetColor("_BaseColor", highlighted ? Color.Lerp(baseColor, Color.white, 0.28f) : baseColor);
            _properties.SetColor("_EmissionColor", highlighted ? new Color(0.3f, 0.18f, 0.04f) : Completed ? new Color(0.03f, 0.16f, 0.12f) : Color.black);
            surface.SetPropertyBlock(_properties);
        }
    }
}

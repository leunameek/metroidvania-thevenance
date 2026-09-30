using UnityEngine;

public sealed class PlazaPortal : MonoBehaviour
{
    public string destinationName;
    public Transform destination;
    public int world;
    public bool requiresTraining = true;
    public Transform halo;
    public Renderer veil;
    public Color color = Color.cyan;
    private TechnicalDemoController _demo;
    private MaterialPropertyBlock _properties;
    public bool Available => !requiresTraining || (_demo != null && _demo.PortalsUnlocked);
    private void Start()
    {
        _demo = FindFirstObjectByType<TechnicalDemoController>();
        _properties = new MaterialPropertyBlock();
    }
    private void Update()
    {
        if (halo != null) halo.Rotate(0, 0, (Available ? 9f : 2f) * Time.deltaTime, Space.Self);
        if (veil == null) return;
        _properties.SetColor("_BaseColor", color * (Available ? 0.75f + 0.12f * Mathf.Sin(Time.time * 2) : 0.22f));
        _properties.SetColor("_EmissionColor", color * (Available ? 1.2f : 0.05f));
        veil.SetPropertyBlock(_properties);
    }
}

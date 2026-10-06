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
    // The lower world opens with the training; the upper one waits for the story (Chía freed and
    // the empty urn answered). Scenes opened straight from the editor travel freely.
    public bool Available => (!requiresTraining || (_demo != null && _demo.PortalsUnlocked))
        && (world <= 0 || CampaignProgress.FreeTravel || CampaignProgress.Model.UpperWorldOpen);
    public string LockedReason => world > 0 && (!requiresTraining || (_demo != null && _demo.PortalsUnlocked))
        ? "El portal superior sigue apagado: espera la respuesta de Chía."
        : "Activa las tres estaciones y supera el entrenamiento para abrir los portales.";
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

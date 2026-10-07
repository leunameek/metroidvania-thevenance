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
    private AudioSource _hum;
    private float _humLevel;
    private int _wasAvailable = -1;
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
        // An open threshold hums where it stands; a closed one is silent (guion C05: no access sound).
        _hum = GameAudio.Loop(gameObject, "MSAudio/portal_zumbido", 0f, true, AudioChannel.Effects, 12f);
    }
    private void Update()
    {
        int available = Available ? 1 : 0;
        // C05 "pulso grave": the moment a threshold opens during play, not when the scene loads.
        if (_wasAvailable == 0 && available == 1 && Time.timeSinceLevelLoad > 2f)
        {
            GameAudio.PlayAt("MIAudio/nucleo_abre", transform.position + Vector3.up * 2f, 1f, AudioChannel.Effects, 40f);
            GameAudio.Caption("Pulso grave · " + (string.IsNullOrEmpty(destinationName) ? "umbral activo" : destinationName + " abierto"));
        }
        _wasAvailable = available;
        _humLevel = Mathf.MoveTowards(_humLevel, available * .45f, Time.deltaTime * .5f);
        GameAudio.SetLoopVolume(_hum, _humLevel);
        if (halo != null) halo.Rotate(0, 0, (Available ? 9f : 2f) * Time.deltaTime, Space.Self);
        if (veil == null) return;
        _properties.SetColor("_BaseColor", color * (Available ? 0.75f + 0.12f * Mathf.Sin(Time.time * 2) : 0.22f));
        _properties.SetColor("_EmissionColor", color * (Available ? 1.2f : 0.05f));
        veil.SetPropertyBlock(_properties);
    }
}

using UnityEngine;

// A02 barred leaf: the root sits at the centre of the passage, the hinge child about 1.1 m to
// the left; opening turns the leaf (and its collider) ~95 degrees into a free niche. It reads
// its flag from MIProgress (lever, shield, horn); the arena closure is driven by the guardian.
public sealed class MIGate : MonoBehaviour
{
    [SerializeField] private string flagId;
    [SerializeField] private Transform hinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField, Min(0.05f)] private float duration = 0.8f;
    [SerializeField] private bool startOpen;
    private float _t;
    private bool _open, _manual;

    public string FlagId => flagId;
    public bool IsOpen => _open;

    private void Awake()
    {
        _open = startOpen || MIProgress.Has(flagId);
        _t = _open ? 1 : 0;
        Apply();
    }
    private void OnEnable() { MIProgress.Changed += OnChanged; }
    private void OnDisable() { MIProgress.Changed -= OnChanged; }
    private void OnChanged(string id) { if (!_manual && id == flagId) Open(true); }

    // Debug key G and the guardian's arena use this; flags drive every other gate.
    public void SetOpen(bool open) { _manual = true; Open(open); }
    public void Release() { _manual = false; Open(startOpen || MIProgress.Has(flagId)); }

    private void Open(bool open)
    {
        if (_open == open) return;
        _open = open;
        MIAudio.PlayAt(open ? "reja_abre" : "reja_cierra", transform.position + Vector3.up * 1.5f);
    }

    private void Update()
    {
        float target = _open ? 1 : 0;
        if (Mathf.Approximately(_t, target)) return;
        _t = Mathf.MoveTowards(_t, target, Time.deltaTime / duration);
        Apply();
    }
    private void Apply()
    {
        if (hinge != null) hinge.localRotation = Quaternion.Euler(0, Mathf.SmoothStep(0, openAngle, _t), 0);
    }
}

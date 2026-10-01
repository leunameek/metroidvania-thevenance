using UnityEngine;

// A02 barred leaf: the root sits at the centre of the passage, the hinge child about 1.1 m to
// the left; opening turns the leaf (and its collider) ~95 degrees into a free niche.
// What opens each gate (lever, shield, horn, arena) is wired in a later stage.
public sealed class MIGate : MonoBehaviour
{
    [SerializeField] private string flagId;
    [SerializeField] private Transform hinge;
    [SerializeField] private float openAngle = -95f;
    [SerializeField, Min(0.05f)] private float duration = 0.8f;
    [SerializeField] private bool startOpen;

    private float _t;
    private bool _open;

    public string FlagId => flagId;
    public bool IsOpen => _open;

    private void Awake()
    {
        _open = startOpen;
        _t = _open ? 1 : 0;
        Apply();
    }

    public void SetOpen(bool open) => _open = open;

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

using UnityEngine;

// Finds hover and turn slowly above their support so they read as pickups, not decoration.
public sealed class MIFloat : MonoBehaviour
{
    [SerializeField] private float height = .06f, period = 2.6f, spin = 28f;
    private Vector3 _base;
    private float _phase;
    private void OnEnable() { _base = transform.localPosition; _phase = Random.value * 10f; }
    private void OnDisable() { transform.localPosition = _base; }
    private void Update()
    {
        transform.localPosition = _base + Vector3.up * height * Mathf.Sin((Time.time + _phase) * Mathf.PI * 2f / period);
        transform.Rotate(Vector3.up, spin * Time.deltaTime, Space.World);
    }
}

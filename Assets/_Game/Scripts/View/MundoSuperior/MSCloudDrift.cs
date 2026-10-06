using UnityEngine;

// Slow drift of a cloud cluster (guide 6.3: wind effects never push the player): a long
// back-and-forth glide along one horizontal axis plus a gentle bob. The builder keeps every
// cluster clear of landings and flight corridors over its whole travel.
public sealed class MSCloudDrift : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.right;
    [SerializeField] private float amplitude = 8f, period = 90f, bob = .5f, phase;

    private Vector3 _origin;

    private void Awake() => _origin = transform.localPosition;

    private void Update()
    {
        float t = Time.time * Mathf.PI * 2f / Mathf.Max(1f, period) + phase;
        transform.localPosition = _origin + axis * (Mathf.Sin(t) * amplitude) + Vector3.up * (Mathf.Sin(t * 2.7f + phase) * bob);
    }
}

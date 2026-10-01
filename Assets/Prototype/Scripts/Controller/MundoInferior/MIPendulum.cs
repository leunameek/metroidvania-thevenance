using UnityEngine;

// H02 swing: the moving part turns around the pivot's local Z axis, so the weight sweeps across
// the lane (local X). Weight mesh and its hit volume are children of the same transform.
// Starting values of the guide: ~35 degrees, 3 s period, second pendulum half a swing behind.
public sealed class MIPendulum : MonoBehaviour
{
    [SerializeField] private float amplitude = 35f;
    [SerializeField, Min(0.1f)] private float period = 3f;
    [SerializeField, Range(0f, 1f)] private float phase;

    private void Update()
    {
        float angle = amplitude * Mathf.Sin((Time.time / period + phase) * Mathf.PI * 2f);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}

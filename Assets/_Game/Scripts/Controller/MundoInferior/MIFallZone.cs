using UnityEngine;

// Pit, spike or pendulum volume: one damage event and the player back on a stable anchor of the
// same room (guide 5.5: a single event per fall, 1 s of protection after reappearing).
[RequireComponent(typeof(BoxCollider))]
public sealed class MIFallZone : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private float damage = 10f;
    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || MundoInferiorBlockout.Instance == null) return;
        MundoInferiorBlockout.Instance.Recover(anchor, damage);
    }
}

using UnityEngine;

// Pit volume: falling into it returns the player to a stable anchor of the same room
// (guide 1.2: fall volumes per pit instead of the global PlayerRespawn threshold).
[RequireComponent(typeof(BoxCollider))]
public sealed class MIFallZone : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || MundoInferiorBlockout.Instance == null) return;
        MundoInferiorBlockout.Instance.Recover(anchor);
    }
}

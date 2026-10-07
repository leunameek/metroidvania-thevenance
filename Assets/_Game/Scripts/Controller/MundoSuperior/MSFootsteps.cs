using UnityEngine;

// Kept so the Mundo Superior scene's component reference stays valid. The player's body sounds
// (steps by surface following the rig's feet, jump, weighted landing, dash) now come from
// PlayerFootsteps in every scene; this only makes sure it is present.
[RequireComponent(typeof(PlayerController))]
public sealed class MSFootsteps : MonoBehaviour
{
    private void Awake()
    {
        if (GetComponent<PlayerFootsteps>() == null) gameObject.AddComponent<PlayerFootsteps>();
    }
}

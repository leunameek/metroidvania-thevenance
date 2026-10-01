using UnityEngine;

// Room volume of the Mundo Inferior: turns the fixed exploration camera to the room's
// direction of travel (entry -> back) and tells the blockout harness which room is active.
[RequireComponent(typeof(BoxCollider))]
public sealed class MICameraZone : MonoBehaviour
{
    [SerializeField] private int roomIndex;
    [SerializeField] private float yaw;

    public float Yaw => yaw;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        var camera = FindFirstObjectByType<ExplorationOrbitCamera>();
        if (camera != null) camera.SetYaw(yaw);
        if (MundoInferiorBlockout.Instance != null) MundoInferiorBlockout.Instance.EnterRoom(roomIndex);
    }
}

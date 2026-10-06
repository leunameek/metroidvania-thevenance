using UnityEngine;

// Room volume of the Mundo Inferior: turns the fixed exploration camera to the room's
// direction of travel (entry -> back) and tells the blockout harness which room is active.
[RequireComponent(typeof(BoxCollider))]
public sealed class MICameraZone : MonoBehaviour
{
    [SerializeField] private int roomIndex;
    [SerializeField] private float yaw;
    // Framing of the room: 0 keeps the default 8 m at 30 degrees.
    [SerializeField] private float distance, pitch;

    public float Yaw => yaw;
    public float Distance => distance;
    public float Pitch => pitch;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        var camera = FindFirstObjectByType<ExplorationOrbitCamera>();
        if (camera != null) { camera.SetYaw(yaw); camera.SetFraming(distance, pitch); }
        if (MundoInferiorBlockout.Instance != null) MundoInferiorBlockout.Instance.EnterRoom(roomIndex);
    }
}

using UnityEngine;

// Discovery and framing volume of a zone (guide 12.2 and 13.1): about 6 m tall over its floor.
// Entering it discovers the zone and eases the fixed camera to the zone preset; the five
// subzones of 06 share one zone index. Index 8 is the optional Yopo 2 branch.
[RequireComponent(typeof(BoxCollider))]
public sealed class MSZone : MonoBehaviour
{
    [SerializeField] private int zoneIndex;
    [SerializeField] private string title = "";
    [SerializeField] private float yaw, distance = 10f, pitch = 30f;

    public int Index => zoneIndex;
    public string Title => title;
    public float Yaw => yaw;
    public float Distance => distance;
    public float Pitch => pitch;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || MundoSuperiorDirector.Instance == null) return;
        MundoSuperiorDirector.Instance.EnterZone(this);
    }
}

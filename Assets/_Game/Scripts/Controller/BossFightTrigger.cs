using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] private BossFightController bossFight;

    private void Awake()
    {
        if (bossFight == null) bossFight = FindFirstObjectByType<BossFightController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (bossFight != null) bossFight.StartFight();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (bossFight != null) bossFight.EndFight();
    }
}

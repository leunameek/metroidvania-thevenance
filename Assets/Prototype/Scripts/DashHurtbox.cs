using UnityEngine;

[RequireComponent(typeof(Health))]
public class DashHurtbox : MonoBehaviour
{
    private Health _health;
    private int _lastHitDashInstanceId = -1;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApplyDashDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryApplyDashDamage(other);
    }

    private void TryApplyDashDamage(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsDashing) return;
        if (player.DashInstanceId == _lastHitDashInstanceId) return;

        _lastHitDashInstanceId = player.DashInstanceId;
        _health.TakeDamage(player.DashDamage);
    }
}

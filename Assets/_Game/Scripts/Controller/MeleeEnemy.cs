using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.2f;

    private PlayerController _player;
    private MeleeModel _melee;

    private void Awake()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _melee = new MeleeModel(chaseRange, attackRange, attackCooldown);
    }

    private void Update()
    {
        if (_player == null) return;

        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (!_melee.IsPlayerInChaseRange(distance)) return;

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        _melee.TickCooldown(Time.deltaTime);

        if (!_melee.IsPlayerInAttackRange(distance))
        {
            transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
            return;
        }

        if (_melee.TryConsumeAttack())
        {
            Health health = _player.GetComponent<Health>();
            if (health != null) health.TakeDamage(attackDamage);
        }
    }
}

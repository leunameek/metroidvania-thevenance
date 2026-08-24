// Pure chase-range/attack-range/cooldown rules for the melee enemy. No MonoBehaviour/
// Transform dependency - MeleeEnemy drives the actual movement/TakeDamage call from this.
public class MeleeModel
{
    private readonly float _chaseRange;
    private readonly float _attackRange;
    private readonly float _attackCooldown;
    private float _cooldownRemaining;

    public MeleeModel(float chaseRange, float attackRange, float attackCooldown)
    {
        _chaseRange = chaseRange;
        _attackRange = attackRange;
        _attackCooldown = attackCooldown;
    }

    public bool IsPlayerInChaseRange(float distance) => distance <= _chaseRange;

    public bool IsPlayerInAttackRange(float distance) => distance <= _attackRange;

    public void TickCooldown(float deltaTime) => _cooldownRemaining -= deltaTime;

    // Returns true exactly on the frame an attack should land (and resets the cooldown).
    public bool TryConsumeAttack()
    {
        if (_cooldownRemaining > 0f) return false;

        _cooldownRemaining = _attackCooldown;
        return true;
    }
}

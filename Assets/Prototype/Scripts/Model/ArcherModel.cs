// Pure detection-range/cooldown rules for the archer's ranged attack. No MonoBehaviour/
// Transform dependency - ArcherEnemy drives the actual rotation/Instantiate from this.
public class ArcherModel
{
    private readonly float _detectionRange;
    private readonly float _fireCooldown;
    private float _cooldownRemaining;

    public ArcherModel(float detectionRange, float fireCooldown)
    {
        _detectionRange = detectionRange;
        _fireCooldown = fireCooldown;
    }

    public bool IsPlayerInRange(float distance) => distance <= _detectionRange;

    // Ticks the fire cooldown; returns true exactly on the frame a shot should fire.
    public bool TickShouldFire(float deltaTime)
    {
        _cooldownRemaining -= deltaTime;
        if (_cooldownRemaining > 0f) return false;

        _cooldownRemaining = _fireCooldown;
        return true;
    }
}

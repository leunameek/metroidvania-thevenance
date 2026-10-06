using UnityEngine;

// Pure state machine + rules for the shield enemy: the Idle/Windup/Charging/Recovering
// charge-attack FSM, patrol point selection, and the shield-break condition. No
// MonoBehaviour/Transform/Collider dependency - ShieldEnemy applies the actual
// transform/collider/UI side effects from what this computes.
public class ShieldEnemyModel
{
    public enum ChargeState { Idle, Windup, Charging, Recovering }

    private readonly Vector3 _spawnPosition;
    private readonly float _detectionRange;
    private readonly float _minChargeRange;
    private readonly float _chargeSpeed;
    private readonly float _windupDuration;
    private readonly float _chargeDuration;
    private readonly float _cooldown;
    private readonly float _hitRadius;
    private readonly float _patrolRadius;
    private readonly float _patrolSpeed;
    private readonly float _patrolPauseDuration;

    private Vector3 _patrolTarget;
    private float _patrolPauseRemaining;
    private float _stateTimeRemaining;

    public ChargeState State { get; private set; } = ChargeState.Idle;
    public bool IsShielded { get; private set; } = true;
    public Vector3 ChargeDirection { get; private set; }
    public float ChargeDamage { get; }

    public ShieldEnemyModel(Vector3 spawnPosition, float detectionRange, float minChargeRange, float chargeSpeed,
        float windupDuration, float chargeDuration, float cooldown, float hitRadius, float chargeDamage,
        float patrolRadius, float patrolSpeed, float patrolPauseDuration)
    {
        _spawnPosition = spawnPosition;
        _patrolTarget = spawnPosition;
        _detectionRange = detectionRange;
        _minChargeRange = minChargeRange;
        _chargeSpeed = chargeSpeed;
        _windupDuration = windupDuration;
        _chargeDuration = chargeDuration;
        _cooldown = cooldown;
        _hitRadius = hitRadius;
        ChargeDamage = chargeDamage;
        _patrolRadius = patrolRadius;
        _patrolSpeed = patrolSpeed;
        _patrolPauseDuration = patrolPauseDuration;
    }

    // Returns true the instant the shield breaks - caller applies the one-time
    // visual/collider/hurtbox side effects only when this is true.
    public bool RegisterDashChainHit(int chainCount)
    {
        if (!IsShielded || chainCount < 3) return false;
        IsShielded = false;
        return true;
    }

    // toPlayerFlat = player position - this position, Y zeroed. Returns true if windup started.
    public bool TryStartWindup(Vector3 toPlayerFlat)
    {
        float distance = toPlayerFlat.magnitude;
        if (distance > _detectionRange || distance < _minChargeRange) return false;

        ChargeDirection = toPlayerFlat.normalized;
        State = ChargeState.Windup;
        _stateTimeRemaining = _windupDuration;
        return true;
    }

    // Returns the flat-plane movement delta to apply this frame (zero while paused at the target).
    public Vector3 TickPatrol(Vector3 currentPosition, float deltaTime)
    {
        Vector3 toTarget = _patrolTarget - currentPosition;
        toTarget.y = 0f;

        if (toTarget.magnitude < 0.3f)
        {
            _patrolPauseRemaining -= deltaTime;
            if (_patrolPauseRemaining <= 0f) PickNewPatrolTarget();
            return Vector3.zero;
        }

        Vector3 delta = toTarget.normalized * _patrolSpeed * deltaTime;
        delta.y = 0f;
        return delta;
    }

    private void PickNewPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * _patrolRadius;
        _patrolTarget = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
        _patrolPauseRemaining = _patrolPauseDuration;
    }

    // Returns true the frame windup finishes and charging begins.
    public bool TickWindup(float deltaTime)
    {
        _stateTimeRemaining -= deltaTime;
        if (_stateTimeRemaining > 0f) return false;

        State = ChargeState.Charging;
        _stateTimeRemaining = _chargeDuration;
        return true;
    }

    public Vector3 GetChargeMoveDelta(float deltaTime) => ChargeDirection * _chargeSpeed * deltaTime;

    // toPlayerFlat must be computed by the caller AFTER applying GetChargeMoveDelta. Returns
    // true if the charge connected (and ends the charge into Recovering).
    public bool CheckChargeHit(Vector3 toPlayerFlat)
    {
        if (toPlayerFlat.magnitude > _hitRadius) return false;

        EndCharge();
        return true;
    }

    // Call only when CheckChargeHit returned false. Ends the charge into Recovering once the
    // duration runs out.
    public void TickChargeExpiry(float deltaTime)
    {
        _stateTimeRemaining -= deltaTime;
        if (_stateTimeRemaining <= 0f) EndCharge();
    }

    public void TickRecovery(float deltaTime)
    {
        _stateTimeRemaining -= deltaTime;
        if (_stateTimeRemaining <= 0f) State = ChargeState.Idle;
    }

    private void EndCharge()
    {
        State = ChargeState.Recovering;
        _stateTimeRemaining = _cooldown;
    }
}

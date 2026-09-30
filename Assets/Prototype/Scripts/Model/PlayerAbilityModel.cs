using UnityEngine;

public enum DashPressResult { Ignored, Started, Chained }

// Pure game-rule state for the player's unlockable abilities: dash tier/chaining, the
// boss-fight dodge, and double jump. No MonoBehaviour/Transform/Component dependency -
// PlayerController owns one instance and drives CharacterController motion from the
// values this computes; it never touches Time.deltaTime/Time.time itself.
public class PlayerAbilityModel
{
    private readonly float _dashSpeed;
    private readonly float _dashDuration;
    private readonly float _dashChainWindow;
    private readonly float _dodgeDistance;
    private readonly float _dodgeDuration;

    private float _lastDashPressTime = -999f;

    public int DashTier { get; private set; }
    public bool HasDoubleJump { get; private set; }
    public bool AirJumpAvailable { get; private set; }

    public int DashChainCount { get; private set; }
    public int DashInstanceId { get; private set; }
    public float DashTimeRemaining { get; private set; }
    public bool IsDashing => DashTimeRemaining > 0f;
    public Vector3 DashVelocity { get; private set; }
    public Vector3 LastDashDirection { get; private set; }

    public float DodgeTimeRemaining { get; private set; }
    public bool IsDodging => DodgeTimeRemaining > 0f;
    public Vector3 DodgeVelocity { get; private set; }

    public PlayerAbilityModel(float dashSpeed, float dashDuration, float dashChainWindow, float dodgeDistance, float dodgeDuration)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _dashChainWindow = dashChainWindow;
        _dodgeDistance = dodgeDistance;
        _dodgeDuration = dodgeDuration;
    }

    public void GrantDash(int tier)
    {
        if (tier > DashTier) DashTier = tier;
    }

    // Collectible rewards add one chain link regardless of which pickup was found first.
    public void GrantDashUpgrade()
    {
        DashTier++;
    }

    public void GrantDoubleJump()
    {
        HasDoubleJump = true;
    }

    // Clears all in-flight ability state (dash/dodge/air-jump) - called when input gets
    // locked or the player teleports, mirroring the resets PlayerController used to inline.
    public void ResetTransient()
    {
        DashTimeRemaining = 0f;
        DashChainCount = 0;
        DodgeTimeRemaining = 0f;
        AirJumpAvailable = false;
    }

    public void CancelDash()
    {
        DashTimeRemaining = 0f;
        DashChainCount = 0;
    }

    public void OnGrounded()
    {
        AirJumpAvailable = HasDoubleJump;
    }

    public bool TryConsumeAirJump()
    {
        if (!AirJumpAvailable) return false;
        AirJumpAvailable = false;
        return true;
    }

    // inputDirection: raw WASD direction (may be zero). forwardFallback: used when no
    // directional input is held. isOnLadder gates dashing off entirely, matching the
    // original HandleDashPress guard.
    public DashPressResult TryPressDash(Vector3 inputDirection, Vector3 forwardFallback, bool isOnLadder, float time)
    {
        if (DashTier <= 0 || isOnLadder) return DashPressResult.Ignored;

        bool withinChainWindow = time - _lastDashPressTime <= _dashChainWindow;

        if (IsDashing)
        {
            if (!withinChainWindow || DashChainCount >= DashTier) return DashPressResult.Ignored;

            DashChainCount++;
            DashVelocity += LastDashDirection * _dashSpeed;
            DashTimeRemaining = _dashDuration;
            _lastDashPressTime = time;
            DashInstanceId++;
            return DashPressResult.Chained;
        }

        Vector3 direction = inputDirection.sqrMagnitude > 0.0001f ? inputDirection.normalized : forwardFallback;
        LastDashDirection = direction;
        DashVelocity = direction * _dashSpeed;
        DashChainCount = 1;
        DashTimeRemaining = _dashDuration;
        _lastDashPressTime = time;
        DashInstanceId++;
        return DashPressResult.Started;
    }

    public void TickDash(float deltaTime)
    {
        if (DashTimeRemaining <= 0f) return;

        DashTimeRemaining -= deltaTime;
        if (DashTimeRemaining <= 0f) DashChainCount = 0;
    }

    public bool TryStartDodge(int direction, Vector3 right)
    {
        if (direction == 0 || IsDodging) return false;

        DodgeVelocity = right * Mathf.Sign(direction) * (_dodgeDistance / _dodgeDuration);
        DodgeTimeRemaining = _dodgeDuration;
        return true;
    }

    public void TickDodge(float deltaTime)
    {
        if (DodgeTimeRemaining <= 0f) return;
        DodgeTimeRemaining -= deltaTime;
    }
}

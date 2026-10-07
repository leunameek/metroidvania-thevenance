using UnityEngine;

// Drives the Nemequene Animator from PlayerController state. Movement stays on the
// CharacterController: root motion is off and clips only animate the body.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int DashingHash = Animator.StringToHash("Dashing");
    private static readonly int OnLadderHash = Animator.StringToHash("OnLadder");
    private static readonly int ClimbSpeedHash = Animator.StringToHash("ClimbSpeed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int AirJumpHash = Animator.StringToHash("AirJump");
    private static readonly int HardLandHash = Animator.StringToHash("HardLand");
    private static readonly int FlyingHash = Animator.StringToHash("Flying");
    private static readonly int ClimbTopHash = Animator.StringToHash("ClimbTop");
    private static readonly int ClimbStillHash = Animator.StringToHash("ClimbStill");

    [SerializeField] private PlayerController player;
    [Tooltip("Seconds the controller may report not-grounded before the Fall state kicks in (steps, ramps).")]
    [SerializeField] private float groundedGrace = 0.1f;
    [Tooltip("Landing faster than this (m/s, downward) plays the hard landing clip.")]
    [SerializeField] private float hardLandSpeed = 14f;
    [SerializeField] private float speedDamping = 0.08f;
    [SerializeField] private float climbReferenceSpeed = 4f;

    private Animator _animator;
    private Vector3 _lastPosition;
    private float _airTime;
    private float _minAirVelocity;
    private bool _wasGrounded = true;
    private bool _hasFlying, _hasClimbExtras;
    private float _climbStillTime;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        // Same height in every scene, whatever scale the scene saved (CharacterScale).
        CharacterScale.Fit(transform, CharacterScale.Nemequene);
        if (player == null) player = GetComponentInParent<PlayerController>();
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.nameHash == FlyingHash) _hasFlying = true;
            if (parameter.nameHash == ClimbStillHash) _hasClimbExtras = true;
        }
    }

    private void OnEnable()
    {
        if (player != null) player.Jumped += OnJumped;
        _lastPosition = player != null ? player.transform.position : transform.position;
    }

    private void OnDisable()
    {
        if (player != null) player.Jumped -= OnJumped;
    }

    private void OnJumped(bool airJump)
    {
        // Leave the ground immediately so the grace window does not bounce Jump back to Locomotion.
        _airTime = groundedGrace;
        _wasGrounded = false;
        _minAirVelocity = 0f;
        _animator.SetTrigger(airJump ? AirJumpHash : JumpHash);
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float dt = Time.deltaTime;
        Vector3 position = player.transform.position;
        Vector3 delta = dt > 0f ? (position - _lastPosition) / dt : Vector3.zero;
        _lastPosition = position;

        // Teleports produce huge deltas; ignore them.
        if (delta.sqrMagnitude > 900f) delta = Vector3.zero;

        float horizontalSpeed = new Vector2(delta.x, delta.z).magnitude;
        _animator.SetFloat(SpeedHash, horizontalSpeed, speedDamping, dt);

        // Climbing and flight motors (Mundo Superior) reuse the ladder clip and the Flying state.
        var pose = player.Motor is IPlayerMotorPose motorPose ? motorPose.Pose : (PlayerMotorPose?)null;
        bool flying = pose == PlayerMotorPose.Fly;
        bool onLadder = player.IsOnLadder || pose == PlayerMotorPose.Climb;
        if (_hasFlying) _animator.SetBool(FlyingHash, flying);
        if (_hasClimbExtras)
        {
            // Hanging Idle after a short pause on the wall (hysteresis avoids flicker between clips).
            _climbStillTime = onLadder && Mathf.Abs(delta.y) < .15f ? _climbStillTime + dt : 0f;
            _animator.SetBool(ClimbStillHash, onLadder && _climbStillTime > .12f);
            _animator.SetBool(ClimbTopHash, pose == PlayerMotorPose.ClimbTop);
        }
        _animator.SetBool(OnLadderHash, onLadder);
        _animator.SetFloat(ClimbSpeedHash, onLadder ? delta.y / climbReferenceSpeed : 1f);
        _animator.SetBool(DashingHash, player.IsDashing);

        float verticalVelocity = player.VerticalVelocity;
        _animator.SetFloat(VerticalVelocityHash, verticalVelocity);

        _airTime = player.IsGrounded || onLadder ? 0f : _airTime + dt;
        bool grounded = _airTime < groundedGrace;
        if (!grounded) _minAirVelocity = flying ? 0f : Mathf.Min(_minAirVelocity, verticalVelocity);

        if (grounded && !_wasGrounded)
        {
            if (_minAirVelocity < -hardLandSpeed) _animator.SetTrigger(HardLandHash);
            _minAirVelocity = 0f;
        }
        else if (grounded)
        {
            _animator.ResetTrigger(JumpHash);
            _animator.ResetTrigger(AirJumpHash);
        }

        _wasGrounded = grounded;
        _animator.SetBool(GroundedHash, grounded);
    }
}

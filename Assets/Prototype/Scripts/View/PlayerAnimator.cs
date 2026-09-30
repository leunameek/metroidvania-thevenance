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

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        if (player == null) player = GetComponentInParent<PlayerController>();
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

        bool onLadder = player.IsOnLadder;
        _animator.SetBool(OnLadderHash, onLadder);
        _animator.SetFloat(ClimbSpeedHash, onLadder ? delta.y / climbReferenceSpeed : 1f);
        _animator.SetBool(DashingHash, player.IsDashing);

        float verticalVelocity = player.VerticalVelocity;
        _animator.SetFloat(VerticalVelocityHash, verticalVelocity);

        _airTime = player.IsGrounded || onLadder ? 0f : _airTime + dt;
        bool grounded = _airTime < groundedGrace;
        if (!grounded) _minAirVelocity = Mathf.Min(_minAirVelocity, verticalVelocity);

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

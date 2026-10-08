using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [Header("Optional exploration controls")]
    [SerializeField] private bool enableSprint;
    [SerializeField] private bool enableExplorationDash;
    [SerializeField, Min(1f)] private float sprintMultiplier = 1.6f;
    [SerializeField] private Transform movementReference;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private Transform faceAnchor;
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashChainWindow = 0.5f;
    [SerializeField] private float dashDamage = 20f;

    [Header("Boss Dodge")]
    [SerializeField] private float dodgeDistance = 4f;
    [SerializeField] private float dodgeDuration = 0.25f;

    [Tooltip("Downward speed applied while grounded to keep the controller stuck to descending ramps. Must stay ahead of moveSpeed/dashSpeed or isGrounded flickers false when going downhill, blocking jump.")]
    [SerializeField] private float groundStickSpeed = 15f;
    // Mundo Superior local profile (guide 1.4): the dash neither starts nor chains in the air.
    [SerializeField] private bool allowAirDash = true;

    private CharacterController _controller;
    private PlayerAbilityModel _abilities;
    private Vector3 _verticalVelocity;
    private int _laddersTouching;
    // Climbing or flight: while set, it moves the capsule instead of the walk/jump code.
    private IPlayerMotor _motor;
    private bool _inputLocked;
    // Dash asked by a gesture or a voice command; taken on the next locomotion frame like the key.
    private bool _dashRequested;

    public Transform FaceAnchor => faceAnchor;
    public float DashDamage => dashDamage;
    public int DashTier => _abilities.DashTier;
    public int DashChainCount => _abilities.DashChainCount;
    public int DashInstanceId => _abilities.DashInstanceId;
    public bool IsDashing => _abilities.IsDashing;
    public bool IsGrounded => _controller.isGrounded;
    public bool HasDoubleJump => _abilities.HasDoubleJump;
    public bool InputLocked => _inputLocked;
    public float VerticalVelocity => _verticalVelocity.y;
    public bool IsOnLadder => _laddersTouching > 0;
    public bool HasMotor => _motor != null;
    public IPlayerMotor Motor => _motor;

    // Fired when a jump starts; the argument is true for the air (double) jump.
    public event System.Action<bool> Jumped;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _abilities = new PlayerAbilityModel(dashSpeed, dashDuration, dashChainWindow, dodgeDistance, dodgeDuration);
    }

    public void GrantDash(int tier) => _abilities.GrantDash(tier);

    public void GrantDashUpgrade() => _abilities.GrantDashUpgrade();

    public void GrantDoubleJump() => _abilities.GrantDoubleJump();

    // One authority over the CharacterController (guide 4.2): a motor replaces the locomotion
    // of this script until it is cleared, so the capsule never moves twice in a frame.
    public void SetMotor(IPlayerMotor motor)
    {
        _motor = motor;
        _verticalVelocity = Vector3.zero;
        _abilities.ResetTransient();
    }

    public void ClearMotor(IPlayerMotor motor, float verticalVelocity = 0f)
    {
        if (_motor != motor) return;
        _motor = null;
        _verticalVelocity = new Vector3(0f, verticalVelocity, 0f);
    }

    // Moving support (transport): its displacement is applied before this frame's locomotion. The
    // move also presses down a little, so the capsule stays grounded on the platform: a sideways
    // move alone left isGrounded false and the jump was refused (2026-10-07 playtest). The carried
    // distance is kept apart, so the animation does not take it for walking.
    public void Carry(Vector3 delta)
    {
        if (!_controller.enabled) return;
        Vector3 before = transform.position;
        _controller.Move(delta + Vector3.down * .02f);
        Carried += transform.position - before;
    }

    // Displacement given by moving supports since the animation last read it.
    public Vector3 Carried { get; set; }

    public void RequestDash() => _dashRequested = true;

    public void SetInputLocked(bool locked)
    {
        _inputLocked = locked;
        if (locked)
        {
            _verticalVelocity = Vector3.zero;
            _abilities.ResetTransient();
        }
    }

    public void Teleport(Vector3 position)
    {
        _controller.enabled = false;
        transform.position = position;
        _verticalVelocity = Vector3.zero;
        _abilities.ResetTransient();
        _controller.enabled = true;
    }

    // Boss-fight reactive dodge: a short lateral burst, independent of the platformer dash
    // (no chaining, no DashHurtbox) so it works while normal input is locked.
    public void PerformDodge(int direction)
    {
        _abilities.TryStartDodge(direction, transform.right);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsDashing) return;

        ShieldEnemy shieldEnemy = hit.collider.GetComponent<ShieldEnemy>();
        if (shieldEnemy != null) shieldEnemy.RegisterDashChainHit(DashChainCount);
    }

    private void Update()
    {
        if (_abilities.IsDodging)
        {
            _abilities.TickDodge(Time.deltaTime);
            UpdateDodgeMotion();
            return;
        }

        bool dashRequested = _dashRequested;
        _dashRequested = false;
        if (_inputLocked) return;

        if (_motor != null)
        {
            _motor.Tick(_controller, Time.deltaTime);
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float z = 0f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;

        if (!enableSprint && keyboard.leftShiftKey.wasPressedThisFrame)
            HandleDashPress(x, z);
        else if (enableSprint && enableExplorationDash && keyboard.qKey.wasPressedThisFrame)
            HandleDashPress(x, z);
        else if (dashRequested)
            HandleDashPress(x, z);

        if (IsDashing)
        {
            UpdateDashMotion();
            return;
        }

        if (IsOnLadder)
        {
            _verticalVelocity = Vector3.zero;
            Vector3 climbMotion = new Vector3(x * moveSpeed, z * climbSpeed, 0f);
            _controller.Move(climbMotion * Time.deltaTime);
            return;
        }

        Vector3 move = new Vector3(x, 0f, z);
        if (movementReference != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(movementReference.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            move = right * x + forward * z;
        }
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (_controller.isGrounded)
        {
            if (_verticalVelocity.y < 0f) _verticalVelocity.y = -groundStickSpeed;
            _abilities.OnGrounded();

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Jumped?.Invoke(false);
            }
        }
        else if (keyboard.spaceKey.wasPressedThisFrame && _abilities.TryConsumeAirJump())
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Jumped?.Invoke(true);
        }

        _verticalVelocity.y += gravity * Time.deltaTime;

        float speed = moveSpeed * (enableSprint && keyboard.leftShiftKey.isPressed ? sprintMultiplier : 1f);
        Vector3 motion = move * speed + Vector3.up * _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);
    }

    private void HandleDashPress(float x, float z)
    {
        if (!allowAirDash && !_controller.isGrounded) return;
        Vector3 inputDirection = new Vector3(x, 0f, z);
        if (movementReference != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(movementReference.forward, Vector3.up).normalized;
            inputDirection = Vector3.Cross(Vector3.up, forward) * x + forward * z;
        }
        DashPressResult result = _abilities.TryPressDash(inputDirection, transform.forward, IsOnLadder, Time.time);

        if (result == DashPressResult.Started)
            transform.rotation = Quaternion.LookRotation(_abilities.LastDashDirection, Vector3.up);
    }

    private void UpdateDashMotion()
    {
        _abilities.TickDash(Time.deltaTime);

        if (_controller.isGrounded && _verticalVelocity.y < 0f) _verticalVelocity.y = -groundStickSpeed;
        _verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 motion = _abilities.DashVelocity;
        motion.y = _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);
    }

    private void UpdateDodgeMotion()
    {
        if (_controller.isGrounded && _verticalVelocity.y < 0f) _verticalVelocity.y = -groundStickSpeed;
        _verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 motion = _abilities.DodgeVelocity;
        motion.y = _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            _laddersTouching++;
            _abilities.CancelDash();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder")) _laddersTouching--;
    }
}

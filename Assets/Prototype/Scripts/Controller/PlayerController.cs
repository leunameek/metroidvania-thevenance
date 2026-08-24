using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
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

    private CharacterController _controller;
    private PlayerAbilityModel _abilities;
    private Vector3 _verticalVelocity;
    private int _laddersTouching;
    private bool _inputLocked;

    public Transform FaceAnchor => faceAnchor;
    public float DashDamage => dashDamage;
    public int DashTier => _abilities.DashTier;
    public int DashChainCount => _abilities.DashChainCount;
    public int DashInstanceId => _abilities.DashInstanceId;
    public bool IsDashing => _abilities.IsDashing;
    public bool IsGrounded => _controller.isGrounded;
    public bool HasDoubleJump => _abilities.HasDoubleJump;

    private bool IsOnLadder => _laddersTouching > 0;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _abilities = new PlayerAbilityModel(dashSpeed, dashDuration, dashChainWindow, dodgeDistance, dodgeDuration);
    }

    public void GrantDash(int tier) => _abilities.GrantDash(tier);

    public void GrantDoubleJump() => _abilities.GrantDoubleJump();

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

        if (_inputLocked) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float z = 0f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;

        if (keyboard.leftShiftKey.wasPressedThisFrame)
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
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (keyboard.spaceKey.wasPressedThisFrame && _abilities.TryConsumeAirJump())
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 motion = move * moveSpeed + Vector3.up * _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);
    }

    private void HandleDashPress(float x, float z)
    {
        Vector3 inputDirection = new Vector3(x, 0f, z);
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

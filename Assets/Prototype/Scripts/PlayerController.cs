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
    [SerializeField] private float dashChainWindow = 0.2f;

    private CharacterController _controller;
    private Vector3 _verticalVelocity;
    private int _laddersTouching;
    private bool _inputLocked;

    private int _dashChainCount;
    private float _dashTimeRemaining;
    private float _lastDashPressTime = -999f;
    private Vector3 _dashDirection;
    private Vector3 _dashVelocity;

    public int DashTier { get; private set; }
    public bool HasDash => DashTier > 0;
    public Transform FaceAnchor => faceAnchor;

    private bool IsOnLadder => _laddersTouching > 0;
    private bool IsDashing => _dashTimeRemaining > 0f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void GrantDash(int tier)
    {
        if (tier > DashTier) DashTier = tier;
    }

    public void SetInputLocked(bool locked)
    {
        _inputLocked = locked;
        if (locked)
        {
            _verticalVelocity = Vector3.zero;
            _dashTimeRemaining = 0f;
            _dashChainCount = 0;
        }
    }

    private void Update()
    {
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
            if (_verticalVelocity.y < 0f) _verticalVelocity.y = -2f;

            if (keyboard.spaceKey.wasPressedThisFrame)
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 motion = move * moveSpeed + Vector3.up * _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);
    }

    private void HandleDashPress(float x, float z)
    {
        if (DashTier <= 0 || IsOnLadder) return;

        bool withinChainWindow = Time.time - _lastDashPressTime <= dashChainWindow;

        if (IsDashing)
        {
            if (withinChainWindow && _dashChainCount < DashTier)
            {
                _dashChainCount++;
                _dashVelocity += _dashDirection * dashSpeed;
                _dashTimeRemaining = dashDuration;
                _lastDashPressTime = Time.time;
            }

            return;
        }

        Vector3 inputDirection = new Vector3(x, 0f, z);
        _dashDirection = inputDirection.sqrMagnitude > 0.0001f ? inputDirection.normalized : transform.forward;
        _dashVelocity = _dashDirection * dashSpeed;
        _dashChainCount = 1;
        _dashTimeRemaining = dashDuration;
        _lastDashPressTime = Time.time;

        transform.rotation = Quaternion.LookRotation(_dashDirection, Vector3.up);
    }

    private void UpdateDashMotion()
    {
        _dashTimeRemaining -= Time.deltaTime;

        if (_controller.isGrounded && _verticalVelocity.y < 0f) _verticalVelocity.y = -2f;
        _verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 motion = _dashVelocity;
        motion.y = _verticalVelocity.y;
        _controller.Move(motion * Time.deltaTime);

        if (_dashTimeRemaining <= 0f) _dashChainCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ladder>() != null)
        {
            _laddersTouching++;
            _dashTimeRemaining = 0f;
            _dashChainCount = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Ladder>() != null) _laddersTouching--;
    }
}

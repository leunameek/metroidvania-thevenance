using UnityEngine;
using UnityEngine.InputSystem;

public class InspectablePickup : MonoBehaviour
{
    [SerializeField] private float inspectDistance = 1.5f;
    [SerializeField] private float rotationSensitivity = 0.3f;
    [SerializeField] private float transitionDuration = 0.4f;

    [Header("Hand tracking (optional - falls back to mouse if not connected)")]
    [SerializeField] private float handRotationSensitivity = 400f;
    [SerializeField] private bool invertVertical;
    [SerializeField] private bool invertHorizontal;

    private enum State { World, EnteringInspect, Inspecting }

    private State _state = State.World;
    private PlayerController _player;
    private CameraFollow _cameraFollow;
    private PulsingOrb _pulsingOrb;
    private IPickupReward _reward;
    private HandGestureTracker _handTracker;
    private Transform _cameraTransform;

    private Vector3 _transitionStartPos;
    private Quaternion _transitionStartRot;
    private Vector3 _inspectCameraPos;
    private Quaternion _inspectCameraRot;
    private float _transitionTime;

    private float _yaw;
    private float _pitch;

    private bool _playerInRange;
    private bool _collected;

    private void Awake()
    {
        _pulsingOrb = GetComponent<PulsingOrb>();
        _reward = GetComponent<IPickupReward>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        _player = player;
        _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == _player)
            _playerInRange = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        switch (_state)
        {
            case State.World:
                if (_playerInRange && keyboard.eKey.wasPressedThisFrame)
                    BeginInspect();
                break;

            case State.EnteringInspect:
                BlendIntoInspect();
                break;

            case State.Inspecting:
                UpdateInspectRotation();
                if (keyboard.eKey.wasPressedThisFrame) Claim();
                else if (keyboard.escapeKey.wasPressedThisFrame) EndInspect();
                break;
        }
    }

    private void BeginInspect()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        _cameraTransform = cam.transform;
        _cameraFollow = cam.GetComponent<CameraFollow>();
        if (_cameraFollow != null) _cameraFollow.enabled = false;
        if (_pulsingOrb != null) _pulsingOrb.SetFloating(false);
        if (_handTracker == null) _handTracker = FindFirstObjectByType<HandGestureTracker>();

        _player.SetInputLocked(true);

        Vector3 eulerAngles = transform.eulerAngles;
        _yaw = eulerAngles.y;
        _pitch = eulerAngles.x;

        Transform anchor = _player.FaceAnchor;
        _inspectCameraPos = anchor != null ? anchor.position : transform.position + Vector3.back * inspectDistance;
        _inspectCameraRot = Quaternion.LookRotation(transform.position - _inspectCameraPos);

        _transitionStartPos = _cameraTransform.position;
        _transitionStartRot = _cameraTransform.rotation;
        _transitionTime = 0f;
        _state = State.EnteringInspect;
    }

    private void BlendIntoInspect()
    {
        _transitionTime += Time.deltaTime;
        float t = transitionDuration <= 0f ? 1f : Mathf.Clamp01(_transitionTime / transitionDuration);

        _cameraTransform.position = Vector3.Lerp(_transitionStartPos, _inspectCameraPos, t);
        _cameraTransform.rotation = Quaternion.Slerp(_transitionStartRot, _inspectCameraRot, t);

        if (t >= 1f) _state = State.Inspecting;
    }

    private void UpdateInspectRotation()
    {
        if (_handTracker != null && _handTracker.IsConnected)
        {
            float handDeltaY = _handTracker.ConsumeRightHandDeltaY();
            float pitchDelta = handDeltaY * handRotationSensitivity * (invertVertical ? -1f : 1f);
            _pitch += pitchDelta;
            
            float handDeltaX = _handTracker.ConsumeLeftHandDeltaX();
            float yawDelta = handDeltaX * handRotationSensitivity * (invertHorizontal ? -1f : 1f);
            _yaw += yawDelta;
        }
        else
        {
            Vector2 delta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            _yaw += delta.x * rotationSensitivity;
            _pitch -= delta.y * rotationSensitivity;
        }

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void Claim()
    {
        _collected = true;
        if (_reward != null) _reward.Grant(_player);
        EndInspect();
        gameObject.SetActive(false);
    }

    private void EndInspect()
    {
        if (_cameraTransform != null)
        {
            _cameraTransform.position = _transitionStartPos;
            _cameraTransform.rotation = _transitionStartRot;
        }

        if (_cameraFollow != null) _cameraFollow.enabled = true;
        if (_pulsingOrb != null) _pulsingOrb.SetFloating(true);
        if (_player != null) _player.SetInputLocked(false);
        _state = State.World;
    }
}

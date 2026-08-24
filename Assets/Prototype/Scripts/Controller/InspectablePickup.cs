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

    private InspectionModel _inspection;
    private PlayerController _player;
    private CameraFollow _cameraFollow;
    private PulsingOrb _pulsingOrb;
    private IPickupReward _reward;
    private HandGestureTracker _handTracker;
    private Transform _cameraTransform;

    private bool _playerInRange;
    private bool _collected;

    private void Awake()
    {
        _pulsingOrb = GetComponent<PulsingOrb>();
        _reward = GetComponent<IPickupReward>();
        _inspection = new InspectionModel(transitionDuration, rotationSensitivity, handRotationSensitivity, invertVertical, invertHorizontal);
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

        switch (_inspection.CurrentState)
        {
            case InspectionModel.State.World:
                if (_playerInRange && keyboard.eKey.wasPressedThisFrame)
                    BeginInspect();
                break;

            case InspectionModel.State.EnteringInspect:
                BlendIntoInspect();
                break;

            case InspectionModel.State.Inspecting:
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

        Transform anchor = _player.FaceAnchor;
        Vector3 inspectCameraPos = anchor != null ? anchor.position : transform.position + Vector3.back * inspectDistance;
        Quaternion inspectCameraRot = Quaternion.LookRotation(transform.position - inspectCameraPos);

        _inspection.BeginInspect(transform.eulerAngles, _cameraTransform.position, _cameraTransform.rotation, inspectCameraPos, inspectCameraRot);
    }

    private void BlendIntoInspect()
    {
        (Vector3 position, Quaternion rotation) = _inspection.TickBlend(Time.deltaTime);
        _cameraTransform.position = position;
        _cameraTransform.rotation = rotation;
    }

    private void UpdateInspectRotation()
    {
        Quaternion rotation;
        if (_handTracker != null && _handTracker.IsConnected)
        {
            rotation = _inspection.ApplyHandRotation(_handTracker.ConsumeRightHandDeltaY(), _handTracker.ConsumeLeftHandDeltaX());
        }
        else
        {
            Vector2 delta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            rotation = _inspection.ApplyMouseRotation(delta);
        }

        transform.rotation = rotation;
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
            _cameraTransform.position = _inspection.TransitionStartPosition;
            _cameraTransform.rotation = _inspection.TransitionStartRotation;
        }

        if (_cameraFollow != null) _cameraFollow.enabled = true;
        if (_pulsingOrb != null) _pulsingOrb.SetFloating(true);
        if (_player != null) _player.SetInputLocked(false);
        _inspection.EndInspect();
    }
}

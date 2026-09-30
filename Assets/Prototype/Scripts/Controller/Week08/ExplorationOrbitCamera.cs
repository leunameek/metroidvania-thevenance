using UnityEngine;
using UnityEngine.InputSystem;

// Separate camera experiment: existing CameraFollow remains the legacy camera.
[RequireComponent(typeof(Camera))]
public sealed class ExplorationOrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(1f)] private float distance = 7f;
    [SerializeField] private float pivotHeight = 1.3f;
    [SerializeField] private float sensitivity = 0.16f;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;
    [SerializeField] private float minPitch = 15f;
    [SerializeField] private float maxPitch = 65f;
    [SerializeField] private LayerMask obstructionMask = 1; // Graybox geometry on Default.
    // The player cannot turn the camera: it keeps a fixed angle behind the character.
    [SerializeField] private bool allowPlayerOrbit;
    private float _yaw;
    private float _pitch = 30f;
    private float _currentDistance;
    private float _distanceVelocity;
    private bool _initialized;
    private float _uiSensitivity = 1, _motionScale = 1;
    private bool _invertY;

    public void ConfigurePresentation(float multiplier, bool invert, float motion)
    {
        _uiSensitivity = Mathf.Clamp(multiplier, .25f, 2); _invertY = invert; _motionScale = Mathf.Clamp01(motion);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Mouse mouse = Mouse.current;
        bool orbiting = allowPlayerOrbit && mouse != null && mouse.rightButton.isPressed;
        if (allowPlayerOrbit)
        {
            Cursor.lockState = orbiting ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !orbiting;
        }
        if (orbiting)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * sensitivity * _uiSensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * sensitivity * _uiSensitivity * (_invertY ? -1 : 1), minPitch, maxPitch);
        }

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 direction = rotation * Vector3.back;
        float desiredDistance = distance;
        if (Physics.SphereCast(pivot, 0.25f, direction, out RaycastHit hit, distance,
            obstructionMask, QueryTriggerInteraction.Ignore))
            desiredDistance = Mathf.Max(0.1f, hit.distance - 0.15f);

        // Move inward immediately to avoid clipping; ease back out when the obstacle clears.
        if (!_initialized || desiredDistance < _currentDistance)
        {
            _currentDistance = desiredDistance;
            _distanceVelocity = 0f;
            _initialized = true;
        }
        else _currentDistance = Mathf.SmoothDamp(_currentDistance, desiredDistance,
            ref _distanceVelocity, Mathf.Max(.001f, smoothTime * _motionScale));
        transform.SetPositionAndRotation(pivot + direction * _currentDistance, rotation);
    }

    public void SnapAfterTeleport()
    {
        _initialized = false;
        _distanceVelocity = 0f;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

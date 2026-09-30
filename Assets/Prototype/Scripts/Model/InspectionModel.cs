using UnityEngine;

// Pure state machine + math for the pickup inspection flow: World -> EnteringInspect ->
// Inspecting -> World, plus the yaw/pitch accumulation driven by mouse or hand-gesture
// input. No MonoBehaviour/Transform/Camera dependency - InspectablePickup drives the actual
// camera/object transforms from the values this computes.
public class InspectionModel
{
    public enum State { World, EnteringInspect, Inspecting }

    private readonly float _transitionDuration;
    private readonly float _rotationSensitivity;
    private readonly float _handRotationSensitivity;
    private readonly bool _invertVertical;
    private readonly bool _invertHorizontal;

    private float _transitionTime;
    private Vector3 _inspectCameraPos;
    private Quaternion _inspectCameraRot;

    public State CurrentState { get; private set; } = State.World;
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public Vector3 TransitionStartPosition { get; private set; }
    public Quaternion TransitionStartRotation { get; private set; }

    public InspectionModel(float transitionDuration, float rotationSensitivity, float handRotationSensitivity, bool invertVertical, bool invertHorizontal)
    {
        _transitionDuration = transitionDuration;
        _rotationSensitivity = rotationSensitivity;
        _handRotationSensitivity = handRotationSensitivity;
        _invertVertical = invertVertical;
        _invertHorizontal = invertHorizontal;
    }

    public void BeginInspect(Vector3 objectEulerAngles, Vector3 cameraStartPos, Quaternion cameraStartRot, Vector3 inspectCameraPos, Quaternion inspectCameraRot)
    {
        Yaw = objectEulerAngles.y;
        Pitch = objectEulerAngles.x;
        TransitionStartPosition = cameraStartPos;
        TransitionStartRotation = cameraStartRot;
        _inspectCameraPos = inspectCameraPos;
        _inspectCameraRot = inspectCameraRot;
        _transitionTime = 0f;
        CurrentState = State.EnteringInspect;
    }

    // Returns the blended camera pose for this frame; advances to Inspecting once t reaches 1.
    public (Vector3 position, Quaternion rotation) TickBlend(float deltaTime)
    {
        _transitionTime += deltaTime;
        float t = _transitionDuration <= 0f ? 1f : Mathf.Clamp01(_transitionTime / _transitionDuration);

        Vector3 position = Vector3.Lerp(TransitionStartPosition, _inspectCameraPos, t);
        Quaternion rotation = Quaternion.Slerp(TransitionStartRotation, _inspectCameraRot, t);

        if (t >= 1f) CurrentState = State.Inspecting;
        return (position, rotation);
    }

    public Quaternion ApplyMouseRotation(Vector2 mouseDelta)
    {
        Yaw += mouseDelta.x * _rotationSensitivity;
        Pitch -= mouseDelta.y * _rotationSensitivity;
        return Quaternion.Euler(Pitch, Yaw, 0f);
    }

    // rightHandDeltaY drives pitch and leftHandDeltaX drives yaw; each axis can be inverted independently.
    public Quaternion ApplyHandRotation(float rightHandDeltaY, float leftHandDeltaX)
    {
        Pitch += rightHandDeltaY * _handRotationSensitivity * (_invertVertical ? -1f : 1f);
        Yaw += leftHandDeltaX * _handRotationSensitivity * (_invertHorizontal ? -1f : 1f);
        return Quaternion.Euler(Pitch, Yaw, 0f);
    }

    public void EndInspect()
    {
        CurrentState = State.World;
    }
}

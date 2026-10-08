using UnityEngine;
using UnityEngine.InputSystem;

// Climbing lane of the 02-04 tower (guide 5.4 and 6.2). Its access points validate Runa 2; the
// motor snaps the player to the lane, moves it with W/S at 4 m/s and A/D within ±0.4 m, releases
// on Space without a jump, and at the top runs a short controlled exit onto 04. It never counts as
// ground, never recharges the wings and clears its state on any interruption.
public sealed class MSClimbWall : MonoBehaviour, IPlayerMotor, IPlayerMotorPose
{
    private enum State { Idle, Engage, Climb, ExitUp, ExitDown }

    // Feet markers: bottom and top of the lane, and the floor point of 04 the exit lands on.
    [SerializeField] private Transform bottom, top, exit, baseSafe;
    [SerializeField] private float speed = 4f, lateralLimit = .4f, exitDuration = .35f;
    [SerializeField] private float cameraYaw = 270f, cameraPitch = 22f, cameraDistance = 9f;
    // The Mixamo "Climbing Ladder" clip is authored facing backwards: turning the root 180 degrees
    // makes the animated body face the wall.
    [SerializeField] private float bodyYawOffset = 180f;
    // Long enough to read the "Climbing To Top" clip (4 s, played at 2.5x by the Animator).
    [SerializeField] private float topExitDuration = 1.6f;

    private State _state;
    private PlayerController _player;
    private float _t, _lateral;
    private Vector3 _from;
    private Quaternion _facing;
    private float _gripTravel;

    public bool Climbing => _state != State.Idle;
    // The exit over the top plays "Climbing To Top"; the rest of the lane uses the ladder clip.
    public PlayerMotorPose Pose => _state == State.ExitUp ? PlayerMotorPose.ClimbTop : PlayerMotorPose.Climb;
    public Transform BaseSafe => baseSafe;
    public float CameraYaw => cameraYaw;
    public float CameraPitch => cameraPitch;
    public float CameraDistance => cameraDistance;

    // The root faces out of the wall (+X on the tower); the lane runs along its side axis.
    private Vector3 Normal => transform.forward;
    private Vector3 Side => Vector3.Cross(Vector3.up, Normal);

    public void Begin(PlayerController player, bool fromTop)
    {
        if (Climbing || player == null) return;
        _player = player;
        _lateral = 0;
        _facing = Quaternion.LookRotation(-Normal) * Quaternion.Euler(0, bodyYawOffset, 0);
        _from = Feet(player.GetComponent<CharacterController>());
        _t = 0;
        _state = fromTop ? State.ExitDown : State.Engage;
        player.SetMotor(this);
        MundoSuperiorDirector.Instance?.OnClimbChanged(true, this);
        MSAudio.PlayVariant("escalada_agarre", 3, .7f);
        _gripTravel = 0;
    }

    // Recovery, portals, inspection or defeat interrupt the climb without any leftover state.
    public void Cancel()
    {
        if (!Climbing) return;
        _state = State.Idle;
        if (_player != null) _player.ClearMotor(this);
        MundoSuperiorDirector.Instance?.OnClimbChanged(false, this);
    }

    private static Vector3 Feet(CharacterController c) => c.transform.position + Vector3.up * (c.center.y - c.height * .5f);

    private Vector3 Lane(float feetY) => new Vector3(bottom.position.x, feetY, bottom.position.z) + Side * _lateral;

    public void Tick(CharacterController controller, float dt)
    {
        if (dt <= 0f) return;
        var keyboard = Keyboard.current;
        Vector3 feet = Feet(controller);
        controller.transform.rotation = Quaternion.RotateTowards(controller.transform.rotation, _facing, 720f * dt);
        switch (_state)
        {
            case State.Engage:
            {
                // 0.25 s onto the lane at the height of the feet.
                _t += dt / .25f;
                Vector3 target = Lane(Mathf.Clamp(_from.y + .05f, bottom.position.y + .05f, top.position.y));
                controller.Move(Vector3.Lerp(_from, target, Mathf.Clamp01(_t)) - feet);
                if (_t >= 1f) _state = State.Climb;
                break;
            }
            case State.Climb:
            {
                if (keyboard == null) return;
                if (GameBindings.Pressed(GameAction.Jump)) { End(0f); return; }
                float v = GameBindings.Axis(GameAction.MoveBack, GameAction.MoveForward);
                float h = GameBindings.Axis(GameAction.MoveLeft, GameAction.MoveRight);
                // The camera faces the wall, so screen right is the negative side axis.
                _lateral = Mathf.Clamp(_lateral - h * 1.5f * dt, -lateralLimit, lateralLimit);
                float y = Mathf.Clamp(feet.y + v * speed * dt, bottom.position.y, top.position.y);
                controller.Move(Lane(y) - feet);
                // S07: a hand / foot grip every half metre of climbing.
                _gripTravel += Mathf.Abs(y - feet.y);
                if (_gripTravel >= .5f) { _gripTravel = 0; MSAudio.PlayVariant("escalada_agarre", 3, .6f); }
                if (v > 0f && y >= top.position.y - .01f) { _state = State.ExitUp; _t = 0; _from = Lane(y); MSAudio.Play("escalada_salida", .7f); }
                else if (v < 0f && y <= bottom.position.y + .01f) End(-1f);
                break;
            }
            case State.ExitUp:
            {
                // Rise above the lip, then step onto the floor of 04: a scripted, collision-free hop.
                _t += dt / topExitDuration;
                Vector3 over = new Vector3(_from.x, exit.position.y + .15f, _from.z);
                Vector3 target = _t < .45f ? Vector3.Lerp(_from, over, _t / .45f)
                    : Vector3.Lerp(over, exit.position + Vector3.up * .15f, (_t - .45f) / .55f);
                controller.transform.position += target - feet;
                Physics.SyncTransforms();
                if (_t >= 1f) End(-1f);
                break;
            }
            case State.ExitDown:
            {
                // From the edge of 04: over the lip and down to the top of the lane.
                _t += dt / (exitDuration * 1.4f);
                Vector3 start = new Vector3(_from.x, exit.position.y + .15f, _from.z);
                Vector3 over = new Vector3(bottom.position.x, exit.position.y + .15f, bottom.position.z);
                Vector3 target = _t < .55f ? Vector3.Lerp(start, over, _t / .55f)
                    : Vector3.Lerp(over, Lane(top.position.y), (_t - .55f) / .45f);
                controller.transform.position += target - feet;
                Physics.SyncTransforms();
                if (_t >= 1f) _state = State.Climb;
                break;
            }
        }
    }

    private void End(float verticalVelocity)
    {
        _state = State.Idle;
        _player.ClearMotor(this, verticalVelocity);
        MundoSuperiorDirector.Instance?.OnClimbChanged(false, this);
    }
}

using UnityEngine;

// Body sounds of the player in every scene (added by GameAudioHost): a step when a foot of the
// humanoid rig plants on the ground (so steps follow the walk and run clips), by distance when
// there is no rig; the surface under the player picks the set (stone, cave stone, earth, wood,
// water); jump, landing weighted by the fall, and the dash. Climbing and flight have their own
// sounds (MSClimbWall, MSWings), so nothing plays under a motor or with input locked.
[RequireComponent(typeof(PlayerController))]
public sealed class PlayerFootsteps : MonoBehaviour
{
    private const float Stride = 1.6f, HardLanding = -12f, Refractory = .2f;

    private PlayerController _player;
    private Animator _animator;
    private Transform _leftFoot, _rightFoot;
    private float _leftLow, _rightLow, _leftPrev, _rightPrev, _lastStep;
    private bool _leftUp, _rightUp;
    private Vector3 _last;
    private float _travel, _airTime, _minVelocity;
    private int _dash;
    private Collider _surfaceCollider;
    private string _surface;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _dash = _player.DashInstanceId;
    }

    private void OnEnable() { _player.Jumped += OnJumped; _last = transform.position; }
    private void OnDisable() => _player.Jumped -= OnJumped;

    private void OnJumped(bool airJump)
    {
        if (_player.InputLocked) return;
        GameAudio.Play("Foley/salto", airJump ? .5f : .7f, AudioChannel.Effects, airJump ? 1.12f : 1f, .05f, .1f, 1);
    }

    private void FindFeet()
    {
        if (_animator != null && _animator.isActiveAndEnabled) return;
        _animator = GetComponentInChildren<Animator>();
        if (_animator != null && _animator.isHuman && _animator.avatar != null)
        {
            _leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        else _leftFoot = _rightFoot = null;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 position = transform.position;
        Vector3 delta = position - _last; _last = position;
        if (_player.DashInstanceId != _dash)
        {
            _dash = _player.DashInstanceId;
            if (!_player.InputLocked) GameAudio.Play("Foley/impulso", .8f, AudioChannel.Effects, 1f, .05f, .15f, 1);
        }
        if (delta.sqrMagnitude > 9f || _player.HasMotor || _player.InputLocked) { _travel = 0; _airTime = 0; _minVelocity = 0; return; }
        if (_player.IsGrounded)
        {
            if (_airTime > .25f)
            {
                bool hard = _minVelocity < HardLanding;
                GameAudio.Play(hard ? "Foley/aterrizaje_fuerte" : "Foley/aterrizaje_" + Surface(), hard ? .9f : .6f, AudioChannel.Effects, 1f, .04f, .15f, 1);
                _lastStep = Time.time;
            }
            _airTime = 0; _minVelocity = 0;
            float speed = new Vector2(delta.x, delta.z).magnitude / dt;
            if (speed < .3f || _player.IsDashing) { _travel = 0; return; }
            FindFeet();
            if (_leftFoot != null && _rightFoot != null)
            {
                FootPlants(position.y, speed);
                // A clip without clear foot lifts (sliding, a placeholder) still gets steps.
                if (Time.time - _lastStep > Stride / Mathf.Max(1f, speed) * 1.6f) Step(speed);
            }
            else
            {
                _travel += speed * dt;
                if (_travel >= Stride) { _travel = 0; Step(speed); }
            }
        }
        else
        {
            _airTime += dt;
            _minVelocity = Mathf.Min(_minVelocity, _player.VerticalVelocity);
        }
    }

    // A foot plants when it comes down to near its lowest height of the recent strides.
    private void FootPlants(float rootY, float speed)
    {
        float l = _leftFoot.position.y - rootY, r = _rightFoot.position.y - rootY;
        _leftLow = Mathf.Min(Mathf.Lerp(_leftLow, l, Time.deltaTime * .5f), l);
        _rightLow = Mathf.Min(Mathf.Lerp(_rightLow, r, Time.deltaTime * .5f), r);
        Check(l, ref _leftPrev, ref _leftUp, _leftLow, speed);
        Check(r, ref _rightPrev, ref _rightUp, _rightLow, speed);
    }

    private void Check(float height, ref float previous, ref bool up, float low, float speed)
    {
        const float lift = .06f;
        if (height > low + lift) up = true;
        else if (up && height <= low + lift * .4f && height <= previous && Time.time - _lastStep > Refractory)
        {
            up = false;
            Step(speed);
        }
        previous = height;
    }

    private void Step(float speed)
    {
        _lastStep = Time.time;
        float level = Mathf.Lerp(.45f, .7f, Mathf.InverseLerp(1.5f, 6f, speed));
        GameAudio.Play("Foley/paso_" + Surface(), level, AudioChannel.Effects, 1f, .05f, .12f, 2);
    }

    // The collider under the player: an AudioSurface says it directly; otherwise its name or
    // material name; otherwise the scene's default (GameAudio.DefaultSurface).
    private string Surface()
    {
        if (!Physics.Raycast(transform.position + Vector3.up * .3f, Vector3.down, out var hit, 1.6f, ~0, QueryTriggerInteraction.Ignore))
            return GameAudio.DefaultSurface;
        if (hit.collider == _surfaceCollider && _surface != null) return _surface;
        _surfaceCollider = hit.collider;
        _surface = AudioSurface.Of(hit.collider);
        return _surface;
    }
}

using UnityEngine;

// A08 moving platform between the docks of 06 and 07 (guide 5.9 and 6.4): a straight, flat run
// at 3 m/s with 0.8 s ramps and a 4 s wait at each end, always cycling while the scene runs. The
// player standing on it is carried by the platform delta (Y included) through PlayerController,
// never parented. Time scale 0 (pause) freezes route, wait and phase.
[DefaultExecutionOrder(-50)]
public sealed class MSTransport : MonoBehaviour
{
    [SerializeField] private Vector3 start, end; // top-centre positions at each dock
    [SerializeField] private float speed = 3f, ramp = .8f, wait = 4f;
    [SerializeField] private Vector2 halfSize = new Vector2(2f, 3f);

    private PlayerController _player;
    private CharacterController _controller;
    private float _clock; // time inside the current cycle
    private float _travel;
    private AudioSource _hum;
    private bool _wasMoving;

    public bool Moving { get; private set; }
    public bool AtStart => _clock < wait;

    private void Awake()
    {
        float length = Vector3.Distance(start, end);
        // Trapezoid profile: L = v * (T - ramp), so T = L / v + ramp.
        _travel = length / Mathf.Max(.1f, speed) + ramp;
        transform.position = start;
    }

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null) _controller = _player.GetComponent<CharacterController>();
        // S17: a spatial hum that the player hears approach the dock, louder while moving.
        _hum = MSAudio.Loop(gameObject, "transporte_bucle", .25f, true, MSAudio.Channel.Effects, 30f);
    }

    // Recovery or reload: back to a valid end, the start dock (guide 6.4).
    public void ResetToStart()
    {
        _clock = 0;
        transform.position = start;
        Physics.SyncTransforms();
    }

    private float Fraction(float t)
    {
        // Distance covered after t seconds of a run with linear ramps of `ramp` seconds.
        float cruise = _travel - 2f * ramp;
        float a = speed / ramp, d;
        if (t <= 0) d = 0;
        else if (t < ramp) d = .5f * a * t * t;
        else if (t < ramp + cruise) d = .5f * speed * ramp + speed * (t - ramp);
        else if (t < _travel) { float r = _travel - t; d = speed * (_travel - ramp) - .5f * a * r * r; }
        else d = speed * (_travel - ramp);
        return Mathf.Clamp01(d / Mathf.Max(.001f, speed * (_travel - ramp)));
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        float cycle = 2f * (wait + _travel);
        _clock = (_clock + dt) % cycle;
        Vector3 target;
        if (_clock < wait) { target = start; Moving = false; }
        else if (_clock < wait + _travel) { target = Vector3.Lerp(start, end, Fraction(_clock - wait)); Moving = true; }
        else if (_clock < 2f * wait + _travel) { target = end; Moving = false; }
        else { target = Vector3.Lerp(end, start, Fraction(_clock - 2f * wait - _travel)); Moving = true; }

        if (Moving != _wasMoving)
        {
            MSAudio.PlayAt(Moving ? "transporte_arranque" : "transporte_parada", transform.position, .9f);
            MSAudio.SetLoopVolume(_hum, Moving ? .8f : .25f);
            _wasMoving = Moving;
        }
        Vector3 delta = target - transform.position;
        if (delta.sqrMagnitude < 1e-10f) return;
        bool carry = Supporting();
        // Rising: lift the player first so the platform never pushes into the capsule;
        // sinking: lower the platform first so the capsule has room to follow.
        if (carry && delta.y >= 0f) _player.Carry(delta);
        transform.position = target;
        Physics.SyncTransforms();
        if (carry && delta.y < 0f) _player.Carry(delta);
    }

    // Real support only: grounded, feet on the top face and inside its footprint.
    private bool Supporting()
    {
        if (_player == null || _controller == null || !_player.IsGrounded || _player.HasMotor) return false;
        Vector3 feet = _player.transform.position + Vector3.up * (_controller.center.y - _controller.height * .5f);
        Vector3 local = feet - transform.position;
        return Mathf.Abs(local.x) <= halfSize.x + .2f && Mathf.Abs(local.z) <= halfSize.y + .2f && local.y > -.2f && local.y < .45f;
    }
}

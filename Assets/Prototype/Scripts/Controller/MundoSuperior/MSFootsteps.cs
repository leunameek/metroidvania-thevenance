using UnityEngine;

// Body sounds of the player in the upper world (guide 16.1 S04-S06): a stone step every stride
// on the ground (four variants), the jump, and a landing whose weight follows the fall speed.
// Climbing and flight have their own sounds (MSClimbWall, MSWings), so steps stop under a motor.
[RequireComponent(typeof(PlayerController))]
public sealed class MSFootsteps : MonoBehaviour
{
    [SerializeField] private float stride = 1.7f, hardLanding = -12f;

    private PlayerController _player;
    private Vector3 _last;
    private float _travel, _airTime, _minVelocity;

    private void Awake() => _player = GetComponent<PlayerController>();
    private void OnEnable() { _player.Jumped += OnJumped; _last = transform.position; }
    private void OnDisable() => _player.Jumped -= OnJumped;

    private void OnJumped(bool airJump) => MSAudio.Play("salto", airJump ? .5f : .7f, airJump ? 1.15f : 1f);

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 position = transform.position;
        Vector3 delta = position - _last; _last = position;
        if (delta.sqrMagnitude > 9f || _player.HasMotor || _player.InputLocked) { _travel = 0; _airTime = 0; _minVelocity = 0; return; }
        if (_player.IsGrounded)
        {
            if (_airTime > .25f) MSAudio.Play(_minVelocity < hardLanding ? "aterrizaje_fuerte" : "aterrizaje", _minVelocity < hardLanding ? .9f : .6f);
            _airTime = 0; _minVelocity = 0;
            _travel += new Vector2(delta.x, delta.z).magnitude;
            if (_travel >= stride) { _travel = 0; MSAudio.PlayVariant("paso_piedra", 4, .55f); }
        }
        else
        {
            _airTime += dt;
            _minVelocity = Mathf.Min(_minVelocity, _player.VerticalVelocity);
        }
    }
}

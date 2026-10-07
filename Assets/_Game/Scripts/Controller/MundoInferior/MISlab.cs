using UnityEngine;

// H03 slab that gives way (guide zone 06): intact -> occupied and warning (1.1 s, the last
// 0.25 s shaking hard) -> detached -> broken -> restored when the attempt resets (a fall, a
// defeat) or after a pause with nobody on it. The solid collider and the visual are separate.
public sealed class MISlab : MonoBehaviour
{
    [SerializeField] private Collider solid;
    [SerializeField] private Transform visual;
    [SerializeField] private Vector2 footprint = new Vector2(2, 2);
    [SerializeField] private float warning = 1.1f, restoreAfter = 4f;
    private enum State { Intact, Warning, Fallen }
    private State _state;
    private float _timer, _fallSpeed;
    private Vector3 _visualHome;
    private Quaternion _visualRotation;
    private PlayerController _player;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (visual != null) { _visualHome = visual.localPosition; _visualRotation = visual.localRotation; }
        MundoInferiorBlockout.AttemptReset += Restore;
    }
    private void OnDestroy() { MundoInferiorBlockout.AttemptReset -= Restore; }

    private bool PlayerOnTop()
    {
        if (_player == null || !_player.IsGrounded) return false;
        Vector3 local = transform.InverseTransformPoint(_player.transform.position);
        return Mathf.Abs(local.x) < footprint.x * .5f + .3f && Mathf.Abs(local.z) < footprint.y * .5f + .3f && local.y > 0 && local.y < 1.6f;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Intact:
                if (PlayerOnTop())
                {
                    _state = State.Warning; _timer = 0;
                    MIAudio.PlayAt("losa_grieta", transform.position);
                    MIParticles.Burst(transform.position, new Color(.55f, .52f, .48f, .7f), 18, .8f, .08f, .6f);
                }
                break;
            case State.Warning:
                _timer += Time.deltaTime;
                float strength = _timer > warning - .25f ? .07f : .02f;
                if (visual != null) visual.localPosition = _visualHome + new Vector3(Random.Range(-1f, 1f), Random.Range(-.5f, .2f), Random.Range(-1f, 1f)) * strength;
                if (_timer >= warning) Detach();
                break;
            case State.Fallen:
                _timer += Time.deltaTime;
                if (visual != null && visual.gameObject.activeSelf)
                {
                    _fallSpeed += 18f * Time.deltaTime;
                    visual.localPosition += Vector3.down * _fallSpeed * Time.deltaTime;
                    visual.Rotate(Vector3.right, 40 * Time.deltaTime, Space.Self);
                    if (visual.localPosition.y < _visualHome.y - 5f) visual.gameObject.SetActive(false);
                }
                if (_timer >= restoreAfter && !PlayerNear()) Restore();
                break;
        }
    }

    private bool PlayerNear() => _player != null && Vector3.Distance(_player.transform.position, transform.position) < 2.2f;

    private void Detach()
    {
        _state = State.Fallen; _timer = 0; _fallSpeed = 0;
        if (solid != null) solid.enabled = false;
        MIAudio.PlayAt("losa_cae", transform.position);
        MIParticles.Burst(transform.position, new Color(.5f, .48f, .45f, .8f), 40, 1.6f, .14f, 1f);
    }

    public void Restore()
    {
        _state = State.Intact; _timer = 0;
        if (solid != null) solid.enabled = true;
        if (visual != null) { visual.gameObject.SetActive(true); visual.localPosition = _visualHome; visual.localRotation = _visualRotation; }
    }
}

using UnityEngine;

// H04 falling stalactite (guide zone 06 and attack 3 of 09): anchored -> the player enters its
// authorised stretch -> shadow and sound warning (0.9 s) -> fall -> impact (damage only during
// the impact) -> rubble -> re-armed. It falls where it was announced; it never follows the player.
public sealed class MIStalactite : MonoBehaviour
{
    [SerializeField] private Transform rock;
    [SerializeField] private Transform shadow;
    [SerializeField] private Vector3 triggerCenter;
    [SerializeField] private Vector3 triggerSize = new Vector3(4, 3, 3);
    [SerializeField] private float warning = .9f, damage = 15f, radius = 1.1f, rearm = 3.5f;
    [SerializeField] private bool automatic = true;
    private enum State { Armed, Warning, Falling, Rubble }
    private State _state;
    private float _timer, _speed;
    private Vector3 _rockHome;
    private Quaternion _rockRotation;
    private PlayerController _player;
    private Renderer _shadowRenderer;
    public bool Busy => _state != State.Armed;
    // Between the rock and the floor it will strike: what the story camera looks at.
    public Vector3 Focus => rock != null ? Vector3.Lerp(transform.position, rock.position, .45f) : transform.position + Vector3.up * 2f;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (rock != null) { _rockHome = rock.localPosition; _rockRotation = rock.localRotation; }
        if (shadow != null) { _shadowRenderer = shadow.GetComponent<Renderer>(); shadow.gameObject.SetActive(false); }
        MundoInferiorBlockout.AttemptReset += ResetRock;
    }
    private void OnDestroy() { MundoInferiorBlockout.AttemptReset -= ResetRock; }

    // Boss attacks and zone triggers both start the same announced fall.
    public void Trigger()
    {
        if (_state != State.Armed) return;
        _state = State.Warning; _timer = 0;
        if (shadow != null) shadow.gameObject.SetActive(true);
        MIAudio.PlayAt("estalactita_aviso", transform.position + Vector3.up * 4);
        if (rock != null) MIParticles.Burst(rock.position, new Color(.6f, .58f, .55f, .6f), 14, .5f, .06f, 1f);
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Armed:
                if (automatic && _player != null)
                {
                    Vector3 local = transform.InverseTransformPoint(_player.transform.position) - triggerCenter;
                    if (Mathf.Abs(local.x) < triggerSize.x * .5f && Mathf.Abs(local.y) < triggerSize.y * .5f && Mathf.Abs(local.z) < triggerSize.z * .5f) Trigger();
                }
                break;
            case State.Warning:
                _timer += Time.deltaTime;
                float pulse = .75f + .25f * Mathf.Sin(_timer * 30f);
                if (shadow != null) shadow.localScale = new Vector3(Mathf.Lerp(.6f, 1f, _timer / warning) * pulse + .1f, 1, Mathf.Lerp(.6f, 1f, _timer / warning) * pulse + .1f);
                if (rock != null) rock.localPosition = _rockHome + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * .03f;
                if (_timer >= warning) { _state = State.Falling; _speed = 0; if (rock != null) rock.localPosition = _rockHome; }
                break;
            case State.Falling:
                _speed += 30f * Time.deltaTime;
                if (rock != null)
                {
                    rock.localPosition += Vector3.down * _speed * Time.deltaTime;
                    if (rock.position.y <= transform.position.y + .2f) Impact();
                }
                else Impact();
                break;
            case State.Rubble:
                _timer += Time.deltaTime;
                if (_timer >= rearm) ResetRock();
                break;
        }
    }

    private void Impact()
    {
        _state = State.Rubble; _timer = 0;
        if (shadow != null) shadow.gameObject.SetActive(false);
        if (rock != null) rock.gameObject.SetActive(false);
        MIAudio.PlayAt("estalactita_golpe", transform.position);
        MIParticles.Burst(transform.position + Vector3.up * .2f, new Color(.55f, .53f, .5f, .85f), 55, 3f, .16f, 1.2f);
        if (_player == null) return;
        Vector3 d = _player.transform.position - transform.position;
        if (new Vector2(d.x, d.z).magnitude <= radius && d.y < 2.4f && d.y > -.5f)
            MundoInferiorBlockout.Instance?.Damage(damage, transform.position);
    }

    public void ResetRock()
    {
        _state = State.Armed; _timer = 0;
        if (shadow != null) shadow.gameObject.SetActive(false);
        if (rock != null) { rock.gameObject.SetActive(true); rock.localPosition = _rockHome; rock.localRotation = _rockRotation; }
    }
}

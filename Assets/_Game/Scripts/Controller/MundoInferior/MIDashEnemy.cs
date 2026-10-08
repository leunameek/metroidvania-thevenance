using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;

// The common creatures of the lower world (guion E03, E04, E06 and the patio guards), fought
// with the dash as decided on 2026-10-06: every blow is announced on the floor before it lands,
// the dash (Q, «impulso» or a fist) is the only damage, and a beaten creature yields and kneels,
// it never dies. A yielded creature stays yielded in the save (MIProgress flag).
//  - Caimán (E03, MI03): frontal guard halves a dash from the front; bite (lunge at the marked
//    spot) and tail sweep (ring around it).
//  - Centinela (patio of MI04): a lighter caimán that only bites.
//  - Vigía (E04, MI04): hovers out of reach and shoots (hide behind the pillars); then dives at a
//    marked spot and stays on the ground, open to the dash, before rising again.
//  - Vigía del cuerno (E06, MI07): a charged scream (growing ring) that a dash during the charge
//    interrupts and stuns; dives like the vigía.
public sealed class MIDashEnemy : MonoBehaviour
{
    public enum Kind { Caiman, Guard, Bat, HornBat }
    private enum Phase { Idle, Cooldown, Telegraph, Strike, Recover, Yielded }
    private enum Move { Bite, Tail, Shoot, Dive, Scream }

    private const float HoverHeight = 2.6f, FailDamage = 15f, Leash = 8f;
    private static readonly List<MIDashEnemy> All = new List<MIDashEnemy>();
    public static MIDashEnemy Engaged { get; private set; }

    private Kind _kind;
    private int _room;
    private string _flag, _name, _greeting, _farewell;
    private float _max, _health, _t, _hover, _engageRange;
    private int _step, _lastDash = -1;
    private bool _greeted, _guardNoticeShown, _interrupted, _engaged;
    private float _telegraphLength = 1;
    private Phase _phase;
    private Move _move;
    private Vector3 _home, _target, _strikeFrom;
    private Quaternion _homeRotation;
    private CharacterActions _actor;
    private CapsuleCollider _hit;
    private TelegraphMark _warning;
    private PlayerController _player;
    // The signal language of every fight (DuelSignalCues): a bite, a dart and a scream come
    // straight on (Front), the tail sweeps (Sweep), the dive falls from above (Above); a golden
    // flare marks the moment to dash into it (a scream that can be interrupted, a creature on the
    // ground after its dive).
    private DuelSignalCues _cues;

    public string DisplayName => _name;
    public float Health01 => _max > 0 ? Mathf.Clamp01(_health / _max) : 0;
    public bool Yielded => _phase == Phase.Yielded;
    public int Room => _room;
    public static IReadOnlyList<MIDashEnemy> Active => All;
    public static event System.Action<MIDashEnemy> YieldedEvent;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { All.Clear(); Engaged = null; }

    public static MIDashEnemy Create(Kind kind, Transform marker, int room, string flag, string name, float health,
        string greeting, string farewell)
    {
        var go = new GameObject(name);
        go.transform.SetParent(marker.parent, false);
        go.transform.SetPositionAndRotation(marker.position, marker.rotation);
        var e = go.AddComponent<MIDashEnemy>();
        e._kind = kind; e._room = room; e._flag = flag; e._name = name; e._max = e._health = health;
        e._greeting = greeting; e._farewell = farewell;
        e._engageRange = kind == Kind.Bat ? 15f : kind == Kind.HornBat ? 13f : 10f;
        e._home = go.transform.position; e._homeRotation = go.transform.rotation;
        string model = kind == Kind.Bat || kind == Kind.HornBat ? "HombreMurcielago" : "HombreCaiman";
        e._actor = CharacterModels.Spawn(model, go.transform, Vector3.zero, Quaternion.identity, kind == Kind.Guard ? .9f : 1f);
        if (e._actor == null) StoryProps.Figure(name, go.transform, go.transform.position, new Color(.2f, .4f, .25f), new Color(.8f, .7f, .4f), 2f);
        e._hit = go.AddComponent<CapsuleCollider>();
        e._hit.isTrigger = true; e._hit.radius = .8f; e._hit.height = 2.4f; e._hit.center = new Vector3(0, 1.2f, 0);
        var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        e._warning = TelegraphMark.Create(go.transform.parent);
        StoryActor.Ensure(go, name, 1.8f);
        return e;
    }

    private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    private void OnDisable() { All.Remove(this); if (Engaged == this) Engaged = null; }

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null) _cues = new DuelSignalCues(_player.transform, transform);
        MundoInferiorBlockout.AttemptReset += Disengage;
        MundoInferiorBlockout.DefeatReset += ResetEncounter;
        if (MIProgress.Has(_flag)) { Yield(true); return; }
        ResetEncounter();
    }
    private void OnDestroy()
    {
        MundoInferiorBlockout.AttemptReset -= Disengage;
        MundoInferiorBlockout.DefeatReset -= ResetEncounter;
        _cues?.Dispose();
    }

    // Defeat of Nemequene: the creature is whole again.
    private void ResetEncounter()
    {
        if (_phase == Phase.Yielded) return;
        _health = _max; _step = 0;
        Disengage();
    }

    // A fall, leaving the room or a step too far: the creature returns to its post and keeps the
    // wounds already dealt (a missed dash over the pit no longer restarts the fight).
    private void Disengage()
    {
        if (_phase == Phase.Yielded) return;
        StopAllCoroutines();
        _phase = Phase.Idle; _t = 0; _interrupted = false; _engaged = false;
        _cues?.Clear();
        _hover = IsFlyer ? HoverHeight : 0;
        transform.SetPositionAndRotation(_home + Vector3.up * _hover, _homeRotation);
        ShowWarning(false);
        if (_actor != null) { if (IsFlyer) _actor.PlayAny("Fly"); else _actor.Rest(); }
    }

    private bool IsFlyer => _kind == Kind.Bat || _kind == Kind.HornBat;

    private void Update()
    {
        if (_phase == Phase.Yielded || _player == null) return;
        var director = MundoInferiorBlockout.Instance;
        if (director == null || director.Busy || TurnDuelController.Running) return;
        Vector3 toPlayer = _player.transform.position - _home; toPlayer.y = 0;
        // Engages at its range and lets go only well beyond it (hysteresis), so the dashes of the
        // fight itself never carry Nemequene out of it.
        float reach = _engaged ? _engageRange + Leash : _engageRange;
        float dy = _player.transform.position.y - _home.y;
        bool here = director.CurrentRoom == _room && toPlayer.magnitude < reach && dy < 6f && dy > -4f;
        if (!here)
        {
            if (_phase != Phase.Idle || _engaged) Disengage();
            if (Engaged == this) Engaged = null;
            return;
        }
        _engaged = true;
        // Several can be awake in the patio: the bar follows the nearest.
        if (Engaged == null || Engaged == this || Engaged.Yielded || Distance(this) < Distance(Engaged)) Engaged = this;
        if (!_greeted)
        {
            _greeted = true;
            director.Hud?.Notify(_name, _greeting, UIIcon.Guardian, UIPalette.GoldLight);
        }
        if (_phase != Phase.Strike) Face(_player.transform.position);
        _t -= Time.deltaTime;
        switch (_phase)
        {
            case Phase.Idle: _phase = Phase.Cooldown; _t = 1.2f; break;
            case Phase.Cooldown: if (_t <= 0) BeginMove(); break;
            case Phase.Telegraph: UpdateTelegraph(); if (_t <= 0) Strike(); break;
            case Phase.Strike: UpdateStrike(); break;
            case Phase.Recover:
                if (_t <= 0)
                {
                    if (IsFlyer && _hover < HoverHeight) { _hover = Mathf.MoveTowards(_hover, HoverHeight, Time.deltaTime * 4f); Place(); if (_hover < HoverHeight) break; _actor?.PlayAny("Fly"); }
                    _phase = Phase.Cooldown; _t = IsFlyer ? 1.4f : 1.8f;
                }
                break;
        }
    }

    // ------------------------------------------------------------------ moves

    private void BeginMove()
    {
        switch (_kind)
        {
            case Kind.Caiman: _move = _step % 2 == 0 ? Move.Bite : Move.Tail; break;
            case Kind.Guard: _move = Move.Bite; break;
            case Kind.Bat: _move = _step % 2 == 0 ? Move.Shoot : Move.Dive; break;
            default: _move = _step % 2 == 0 ? Move.Scream : Move.Dive; break;
        }
        _step++;
        _phase = Phase.Telegraph; _interrupted = false;
        _target = Ground(_player.transform.position);
        switch (_move)
        {
            case Move.Bite: _t = .9f; Warn(_target, 1.5f); _actor?.PlayAny("PowerUp"); MIAudio.PlayAt("guardian_carga", transform.position, .6f); break;
            case Move.Tail: _t = 1f; Warn(Ground(transform.position), 2.6f); _actor?.PlayAny("Crouch", "PowerUp"); MIAudio.PlayAt("guardian_carga", transform.position, .6f); break;
            case Move.Shoot: _t = .7f; ShowWarning(false); _actor?.PlayAny("Reach", "Point"); break;
            case Move.Dive: _t = 1.1f; Warn(_target, 1.8f); break;
            case Move.Scream: _t = 1.6f; Warn(Ground(transform.position), .5f); _actor?.PlayAny("PowerUp"); MIAudio.PlayAt("guardian_carga", transform.position, .9f, 1.4f); break;
        }
        _telegraphLength = Mathf.Max(.1f, _t);
        _cues?.Warn(Signal);
        if (_move == Move.Scream) _cues?.Open(true, false); // gold from the start: a dash interrupts it
    }

    private DuelSignal Signal => _move == Move.Tail ? DuelSignal.Sweep : _move == Move.Dive ? DuelSignal.Above : DuelSignal.Front;
    private Color SignalColor => Signal == DuelSignal.Front ? new Color(1f, .45f, .12f) : Signal == DuelSignal.Sweep ? new Color(.62f, .9f, 1f) : new Color(.62f, .38f, 1f);

    private void UpdateTelegraph()
    {
        // The fill closes on the ring as the blow approaches; the scream ring also widens toward its
        // reach. The dive mark follows nothing (it was announced).
        float k = 1 - Mathf.Clamp01(_t / _telegraphLength);
        _cues?.Progress(k);
        if (_warning == null || !_warning.Visible) return;
        if (_move == Move.Scream) _warning.SetRadius(Mathf.Lerp(.5f, 4.5f, k));
        _warning.SetProgress(k);
    }

    private void Strike()
    {
        _phase = Phase.Strike; _strikeFrom = transform.position;
        switch (_move)
        {
            case Move.Bite: _t = .3f; _actor?.PlayAny("Attack"); break;
            case Move.Tail: _t = .35f; _actor?.PlayAny("Spin", "Attack"); break;
            case Move.Shoot: _t = .2f; _cues?.Clear(); _actor?.PlayAny("Attack"); MIProjectile.Fire(transform.position + Vector3.up * 1.4f, _player, FailDamage); break;
            case Move.Dive: _t = .4f; _actor?.PlayAny("Fall", "Attack"); break;
            case Move.Scream:
                _t = .2f; ShowWarning(false); _cues?.Clear();
                if (_interrupted) break;
                _actor?.PlayAny("Spin", "Attack");
                MIAudio.PlayAt("guardian_golpe", transform.position, 1f, 1.5f);
                MIParticles.Burst(transform.position + Vector3.up, new Color(.75f, .6f, 1f, .8f), 60, 6f, .12f, .8f);
                if (PlayerWithin(Ground(transform.position), 4.5f)) Hurt();
                break;
        }
    }

    private void UpdateStrike()
    {
        float duration = _move == Move.Dive ? .4f : _move == Move.Bite ? .3f : .2f;
        float k = 1 - Mathf.Clamp01(_t / duration);
        if (_move == Move.Bite)
        {
            Vector3 d = _target - _home; d.y = 0;
            Vector3 end = _home + Vector3.ClampMagnitude(d, 2.4f);
            transform.position = Vector3.Lerp(_strikeFrom, end, k);
        }
        else if (_move == Move.Dive)
        {
            _hover = Mathf.Lerp(HoverHeight, 0, k);
            transform.position = Vector3.Lerp(_strikeFrom, _target, k);
        }
        if (_t > 0) return;
        ShowWarning(false);
        // Landed after a dive: the golden flare says it is open to the dash now.
        if (_move == Move.Dive) _cues?.Open(true, false);
        _cues?.Clear();
        switch (_move)
        {
            case Move.Bite: if (PlayerWithin(transform.position, 1.6f)) Hurt(); MIAudio.PlayAt("guardian_golpe", transform.position, .7f); break;
            case Move.Tail:
                MIParticles.Burst(transform.position + Vector3.up * .2f, new Color(.55f, .6f, .45f, .8f), 40, 4f, .12f, .6f);
                if (PlayerWithin(transform.position, 2.6f)) Hurt();
                MIAudio.PlayAt("guardian_golpe", transform.position, .8f);
                break;
            case Move.Dive:
                _hover = 0;
                MIParticles.Burst(transform.position + Vector3.up * .2f, new Color(.6f, .58f, .55f, .9f), 50, 4f, .15f, 1f);
                if (PlayerWithin(transform.position, 1.8f)) Hurt();
                _actor?.PlayAny("Land", "HitGut");
                MIAudio.PlayAt("guardian_golpe", transform.position, .8f);
                break;
        }
        _phase = Phase.Recover;
        // On the ground after a dive the creature is open to the dash; a stunned scream too.
        _t = _move == Move.Dive ? 2.8f : _interrupted ? 2.2f : .6f;
        // No notice: the golden flare on landing is the signal to dash (taught in the plaza training).
        if (_move == Move.Bite) StartCoroutine(StepBack());
    }

    private System.Collections.IEnumerator StepBack()
    {
        yield return new WaitForSeconds(.4f);
        Vector3 from = transform.position;
        for (float t = 0; t < .5f && _phase != Phase.Yielded; t += Time.deltaTime)
        { transform.position = Vector3.Lerp(from, _home, t / .5f); yield return null; }
    }

    // ------------------------------------------------------------------ the dash

    private void OnTriggerEnter(Collider other) => Hit(other);
    private void OnTriggerStay(Collider other) => Hit(other);

    private void Hit(Collider other)
    {
        if (_phase == Phase.Yielded) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsDashing || player.DashInstanceId == _lastDash) return;
        _lastDash = player.DashInstanceId;
        float damage = player.DashDamage;
        var hud = MundoInferiorBlockout.Instance?.Hud;
        if (_kind == Kind.Caiman)
        {
            // Frontal guard: from the front only half; from the side or behind (flanking) all.
            Vector3 from = player.transform.position - transform.position; from.y = 0;
            if (Vector3.Dot(transform.forward, from.normalized) > .45f)
            {
                damage *= .5f; _actor?.PlayAny("Block");
                if (!_guardNoticeShown) { _guardNoticeShown = true; hud?.Notify("Guardia frontal", "Por delante resiste: rodéalo e impúlsate por un costado.", UIIcon.Shield, UIPalette.Muted); }
            }
        }
        if (_move == Move.Scream && _phase == Phase.Telegraph)
        {
            _interrupted = true; ShowWarning(false);
            hud?.Notify("Grito interrumpido", "El impulso cortó la carga: está aturdido.", UIIcon.Check, UIPalette.Jade);
            _t = 0;
        }
        _health -= damage;
        MIAudio.PlayAt("golpe_piedra", transform.position);
        MIParticles.Burst(transform.position + Vector3.up * 1.2f, new Color(.6f, .85f, 1f), 24, 2.5f, .08f, .4f);
        if (_health <= 0) { Yield(false); return; }
        _actor?.PlayAny(_step % 2 == 0 ? "HitLeft" : "HitRight", "HitGut");
    }

    // Beaten: it kneels for a moment and goes out in motes, leaving nothing behind (2026-10-06
    // playtest). A creature already beaten in the save is simply absent.
    private void Yield(bool loaded)
    {
        StopAllCoroutines();
        _phase = Phase.Yielded; _hover = 0;
        ShowWarning(false);
        _cues?.Clear();
        if (_hit != null) _hit.enabled = false;
        if (Engaged == this) Engaged = null;
        if (loaded) { HideBody(); YieldedEvent?.Invoke(this); return; }
        transform.position = Ground(transform.position);
        _actor?.PlayAny("Kneel");
        CreatureDissolve.Run(Body, IsFlyer ? new Color(.75f, .65f, 1f) : new Color(.6f, .95f, .75f), .7f);
        MIProgress.Set(_flag);
        YieldedEvent?.Invoke(this);
        MIAudio.PlayAt("guardian_cae", transform.position);
        MundoInferiorBlockout.Instance?.Hud?.Notify(_name + " cede el paso", _farewell, UIIcon.Check, UIPalette.Jade);
        MundoInferiorBlockout.Instance?.RefreshObjective();
    }

    // ------------------------------------------------------------------ helpers

    private float Distance(MIDashEnemy e) => Vector3.Distance(e.transform.position, _player.transform.position);

    private void Hurt() => MundoInferiorBlockout.Instance?.Damage(FailDamage, transform.position);

    private bool PlayerWithin(Vector3 center, float radius)
    {
        Vector3 d = _player.transform.position - center; d.y = 0;
        return d.magnitude <= radius + .3f && Mathf.Abs(_player.transform.position.y - center.y) < 2.5f;
    }

    private void Face(Vector3 point)
    {
        Vector3 d = point - transform.position; d.y = 0;
        if (d.sqrMagnitude < .01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 6f);
    }

    private void Place()
    {
        Vector3 p = transform.position; p.y = _home.y + _hover; transform.position = p;
    }

    private Vector3 Ground(Vector3 at) => TelegraphMark.Floor(at, _home.y);

    // The visible body (the model, or the provisional figure's parts).
    private Transform Body => _actor != null ? _actor.transform : transform;
    private void HideBody()
    {
        if (_actor != null) { CreatureDissolve.HideNow(_actor.transform); return; }
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
    }

    private void Warn(Vector3 at, float radius) { if (_warning != null) _warning.Show(at, radius, SignalColor); }

    private void ShowWarning(bool visible) { if (_warning != null && !visible) _warning.Hide(); }
}

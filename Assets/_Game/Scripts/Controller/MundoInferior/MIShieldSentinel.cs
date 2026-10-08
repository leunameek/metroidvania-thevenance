using Nemequene.UI;
using UnityEngine;

// Provisional C03a of zone 04 until the character exists: a stone sentinel with a visible shield
// (guide «Resolución del escudo»). Single dashes and chains of two only clang; the third link of
// one chain cracks and breaks the defence; then valid dash hits (20) take its 40 points. Every
// few seconds it charges a ground pulse that the player must leave. Its defeat opens 04 -> 05.
public sealed class MIShieldSentinel : MonoBehaviour
{
    [SerializeField] private GameObject shield;
    [SerializeField] private Transform body;
    [SerializeField] private Light core;
    [SerializeField] private Transform pulseRing;
    [SerializeField] private int room = 3;
    [SerializeField] private float maxHealth = 40f, pulseEvery = 4.5f, pulseWarning = .9f, pulseRadius = 3.2f, pulseDamage = 20f;
    private float _health, _timer, _pulseT = -1, _flash;
    private bool _shieldUp = true, _dead;
    private int _lastDash = -1;
    private float _lastBlockedNotice = -99;
    private PlayerController _player;
    // Its pulse is a wave along the ground toward Nemequene: the Front signal of every fight.
    private DuelSignalCues _cues;
    private Vector3 _bodyHome;

    public bool Defeated => _dead;
    public bool ShieldUp => _shieldUp;
    public float Health01 => _dead ? 0 : _shieldUp ? 1 : _health / maxHealth;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null) _cues = new DuelSignalCues(_player.transform, transform);
        if (body != null) _bodyHome = body.localPosition;
        MundoInferiorBlockout.AttemptReset += OnAttemptReset;
        MundoInferiorBlockout.DefeatReset += OnDefeatReset;
        if (MIProgress.Has(MIProgress.Shield04)) { _dead = true; gameObject.SetActive(false); return; }
        // The rigged hombre-caimán (with and without shield) replaces the stone stand-in; the
        // shield object keeps its logic, the core stays as the reading of the pulse.
        if (body != null && CharacterModels.Exists("HombreCaimanEscudo"))
        {
            CharacterModels.Hide(body, core != null ? core.transform : null);
            if (shield != null) CharacterModels.Hide(shield.transform);
            _shielded = CharacterModels.Spawn("HombreCaimanEscudo", body, Vector3.zero, Quaternion.identity);
            _bare = CharacterModels.Spawn("HombreCaiman", body, Vector3.zero, Quaternion.identity);
        }
        ResetEncounter();
    }

    private CharacterActions _shielded, _bare;
    private CharacterActions Actor => _shieldUp || _bare == null ? _shielded : _bare;
    private void ShowShield(bool up)
    {
        if (_shielded != null) _shielded.gameObject.SetActive(up || _bare == null);
        if (_bare != null) _bare.gameObject.SetActive(!up);
    }
    private void OnDestroy()
    {
        MundoInferiorBlockout.AttemptReset -= OnAttemptReset;
        MundoInferiorBlockout.DefeatReset -= OnDefeatReset;
        _cues?.Dispose();
    }
    // A fall only cancels the pulse in progress; the broken shield and the wounds stay. A defeat
    // of Nemequene makes it whole again.
    private void OnAttemptReset()
    {
        if (_dead) return;
        _timer = 0; _pulseT = -1;
        if (pulseRing != null) pulseRing.gameObject.SetActive(false);
        _cues?.Clear();
    }
    private void OnDefeatReset() { if (!_dead) ResetEncounter(); }

    private void ResetEncounter()
    {
        _health = maxHealth; _shieldUp = true; _timer = 0; _pulseT = -1;
        if (shield != null) shield.SetActive(true);
        if (pulseRing != null) pulseRing.gameObject.SetActive(false);
        ShowShield(true);
    }

    private bool PlayerHere => MundoInferiorBlockout.Instance != null && MundoInferiorBlockout.Instance.CurrentRoom == room
        && _player != null && Mathf.Abs(_player.transform.position.y - transform.position.y - 1f) < 1.8f;

    private void Update()
    {
        if (_dead) return;
        if (core != null) core.intensity = Mathf.Lerp(core.intensity, (_pulseT >= 0 ? 6f : 2.2f) + _flash * 8f, Time.deltaTime * 10f);
        _flash = Mathf.MoveTowards(_flash, 0, Time.deltaTime * 3f);
        if (body != null) body.localPosition = _bodyHome + (_flash > 0 ? Random.insideUnitSphere * .04f * _flash : Vector3.zero);
        if (!PlayerHere) { _timer = 0; if (_pulseT >= 0) { _pulseT = -1; _cues?.Clear(); if (pulseRing != null) pulseRing.gameObject.SetActive(false); } return; }
        if (_pulseT < 0)
        {
            _timer += Time.deltaTime;
            if (_timer >= pulseEvery)
            {
                _pulseT = 0; _timer = 0;
                if (core != null) core.color = new Color(1f, .35f, .3f);
                if (pulseRing != null) { pulseRing.gameObject.SetActive(true); pulseRing.localScale = Vector3.one * .2f; }
                Actor?.PlayAny("PowerUp", "Attack");
                MIAudio.PlayAt("guardian_carga", transform.position, .8f);
                _cues?.Warn(DuelSignal.Front);
            }
        }
        else
        {
            _pulseT += Time.deltaTime;
            if (pulseRing != null) pulseRing.localScale = Vector3.one * Mathf.Lerp(.2f, pulseRadius, Mathf.Clamp01(_pulseT / pulseWarning));
            _cues?.Progress(_pulseT / pulseWarning);
            if (_pulseT >= pulseWarning)
            {
                _pulseT = -1;
                _cues?.Clear();
                if (core != null) core.color = new Color(.45f, .8f, 1f);
                if (pulseRing != null) pulseRing.gameObject.SetActive(false);
                MIAudio.PlayAt("guardian_golpe", transform.position);
                Actor?.PlayAny("Spin", "Attack");
                MIParticles.Burst(transform.position + Vector3.up * .2f, new Color(.6f, .58f, .55f, .8f), 50, 4f, .15f, 1f);
                Vector3 d = _player.transform.position - transform.position;
                if (new Vector2(d.x, d.z).magnitude <= pulseRadius) MundoInferiorBlockout.Instance?.Damage(pulseDamage, transform.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other) => Hit(other);
    private void OnTriggerStay(Collider other) => Hit(other);

    private void Hit(Collider other)
    {
        if (_dead) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsDashing || player.DashInstanceId == _lastDash) return;
        _lastDash = player.DashInstanceId;
        var hud = MundoInferiorBlockout.Instance?.Hud;
        if (_shieldUp)
        {
            if (player.DashChainCount >= 3)
            {
                _shieldUp = false; _flash = 1;
                if (shield != null) { MIParticles.Burst(shield.transform.position, new Color(.75f, .7f, .6f), 70, 4f, .16f, 1.4f); shield.SetActive(false); }
                MIAudio.PlayAt("escudo_rompe", transform.position);
                ShowShield(false); Actor?.PlayAny("HitGut");
                hud?.Notify("Defensa rota", "La tercera cadena quebró el escudo. Ahora cada impulso lo daña.", UIIcon.Shield, UIPalette.GoldLight);
            }
            else
            {
                MIAudio.PlayAt("escudo_bloquea", transform.position);
                Actor?.PlayAny("Block");
                MIParticles.Burst(other.transform.position + Vector3.up, new Color(1f, .85f, .5f), 16, 3f, .06f, .2f);
                if (Time.time - _lastBlockedNotice > 6f)
                {
                    _lastBlockedNotice = Time.time;
                    hud?.Notify("La defensa resiste", VoicePrompt.Enabled ? "Encadena tres impulsos seguidos (di «impulso» tres veces, o Q Q Q) para quebrarla." : "Encadena tres impulsos seguidos (Q Q Q) para quebrarla.", UIIcon.Shield, UIPalette.Danger);
                }
            }
            return;
        }
        _health -= player.DashDamage; _flash = 1;
        MIAudio.PlayAt("golpe_piedra", transform.position);
        Actor?.PlayAny(_health % 2 < 1 ? "HitLeft" : "HitRight", "HitGut");
        MIParticles.Burst(transform.position + Vector3.up * 1.2f, new Color(.6f, .85f, 1f), 24, 2.5f, .08f, .4f);
        if (_health > 0) return;
        _dead = true;
        MIProgress.Set(MIProgress.Shield04);
        MIAudio.PlayAt("guardian_cae", transform.position);
        MIParticles.Burst(transform.position + Vector3.up, new Color(.55f, .53f, .5f, .9f), 120, 4f, .2f, 1.2f);
        hud?.Notify("Centinela vencido", "La salida hacia el paso de péndulos queda abierta.", UIIcon.Check, UIPalette.Jade);
        MundoInferiorBlockout.Instance?.RefreshObjective();
        // It kneels and goes out in motes; nothing of it stays in the patio.
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (pulseRing != null) pulseRing.gameObject.SetActive(false);
        Actor?.PlayAny("Kneel");
        CreatureDissolve.Run(transform, new Color(.6f, .95f, .75f), .7f);
    }
}

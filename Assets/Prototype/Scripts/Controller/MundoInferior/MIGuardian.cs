using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;

// Provisional C05 of zone 09 until the character exists: a 4.2 m stone idol with a crystal core,
// running the guide's encounter. Entering the start volume saves checkpoint 08, closes the arena
// and starts a visible preparation. Attacks: 1 frontal sweep of one half, 2 slam and ring wave to
// jump over, 3 two announced stones. After each one the core is exposed and takes dash hits (20 of
// 200). From half health it chains two attacks with shorter recovery. Victory opens arena and portal.
public sealed class MIGuardian : MonoBehaviour
{
    [SerializeField] private string displayName = "Guardián del fondo";
    [SerializeField] private Transform body;
    [SerializeField] private Transform core;
    [SerializeField] private Light coreLight;
    [SerializeField] private Collider coreHit;
    [SerializeField] private MIGate arenaGate;
    [SerializeField] private Transform sweepLeft, sweepRight;
    [SerializeField] private LineRenderer wave;
    [SerializeField] private MIStalactite[] stones = new MIStalactite[0];
    [SerializeField] private Vector3 startCenter = new Vector3(0, 1, 3), startSize = new Vector3(4, 3, 2);
    [SerializeField] private Vector2 arenaHalf = new Vector2(8, 7.5f);
    [SerializeField] private float maxHealth = 200f, attackDamage = 20f, waveSpeed = 7f;

    private enum Phase { Waiting, Intro, Telegraph, Execute, Exposed, Defeated }
    private enum Attack { Sweep, Wave, Stones }
    private Phase _phase;
    private Attack _attack;
    private readonly Queue<Attack> _queue = new Queue<Attack>();
    private float _health, _timer, _waveRadius, _flash;
    private bool _sweepRightSide, _waveHit;
    private int _lastDash = -1, _cycle;
    private PlayerController _player;
    private Vector3 _bodyHome;
    private AudioSource _music;

    public bool Fighting => _phase != Phase.Waiting && _phase != Phase.Defeated;
    public float Health01 => _health / maxHealth;
    public string DisplayName => displayName;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (body != null) _bodyHome = body.localPosition;
        _music = MIAudio.Loop(gameObject, "musica_guardian", .55f, false); _music.Stop();
        if (MIProgress.Has(MIProgress.Guardian)) { Defeat(true); return; }
        ResetEncounter();
    }

    // Player defeat (guide «Derrota del jugador»): clear attacks, open the arena, full health.
    public void ResetEncounter()
    {
        if (_phase == Phase.Defeated) return;
        _phase = Phase.Waiting; _health = maxHealth; _queue.Clear(); _cycle = 0;
        ClearEffects();
        foreach (var s in stones) if (s != null) s.ResetRock();
        if (arenaGate != null) arenaGate.Release();
        if (_music != null) _music.Stop();
        SetCore(false);
        if (body != null) body.localPosition = _bodyHome;
    }

    private void ClearEffects()
    {
        if (sweepLeft != null) sweepLeft.gameObject.SetActive(false);
        if (sweepRight != null) sweepRight.gameObject.SetActive(false);
        if (wave != null) wave.gameObject.SetActive(false);
    }

    private void SetCore(bool exposed)
    {
        if (coreHit != null) coreHit.enabled = exposed;
        if (coreLight != null) { coreLight.color = exposed ? new Color(1f, .82f, .45f) : new Color(.4f, .7f, 1f); }
    }

    private Vector3 Local(Vector3 world) => transform.InverseTransformPoint(world);

    private void Update()
    {
        if (_phase == Phase.Defeated || _player == null) return;
        _flash = Mathf.MoveTowards(_flash, 0, Time.deltaTime * 2.5f);
        if (coreLight != null)
        {
            float target = _phase == Phase.Exposed ? 9f : _phase == Phase.Telegraph ? 4f : 2f;
            coreLight.intensity = Mathf.Lerp(coreLight.intensity, target + _flash * 10f, Time.deltaTime * 8f);
        }
        switch (_phase)
        {
            case Phase.Waiting:
                Vector3 p = Local(_player.transform.position) - startCenter;
                if (Mathf.Abs(p.x) < startSize.x * .5f && Mathf.Abs(p.y) < startSize.y * .5f && Mathf.Abs(p.z) < startSize.z * .5f) Begin();
                break;
            case Phase.Intro:
                _timer += Time.deltaTime;
                if (body != null) body.localPosition = _bodyHome + Vector3.up * Mathf.Sin(Mathf.Clamp01(_timer / 1.4f) * Mathf.PI) * .3f;
                if (_timer >= 1.6f) NextAttack();
                break;
            case Phase.Telegraph: Telegraph(); break;
            case Phase.Execute: Execute(); break;
            case Phase.Exposed:
                _timer += Time.deltaTime;
                if (core != null) core.localScale = Vector3.one * (1f + .08f * Mathf.Sin(Time.time * 12f));
                if (_timer >= (_health <= maxHealth * .5f ? 1.5f : 2f)) { SetCore(false); if (core != null) core.localScale = Vector3.one; NextAttack(); }
                break;
        }
    }

    private void Begin()
    {
        _phase = Phase.Intro; _timer = 0;
        MIProgress.SetCheckpoint(7);
        if (arenaGate != null) arenaGate.SetOpen(false);
        if (_music != null) _music.Play();
        MIAudio.Play("guardian_despierta", 1f);
        MundoInferiorBlockout.Instance?.Hud?.Notify(displayName, "Evita sus ataques y golpea el núcleo cuando brille con un impulso.", UIIcon.Guardian, UIPalette.GoldLight);
    }

    private void NextAttack()
    {
        if (_queue.Count == 0)
        {
            _cycle++;
            var order = new[] { Attack.Sweep, Attack.Wave, Attack.Stones };
            int start = _cycle % 3;
            _queue.Enqueue(order[start]);
            if (_health <= maxHealth * .5f) _queue.Enqueue(order[(start + 1) % 3]); // second half: two seen attacks
        }
        _attack = _queue.Dequeue();
        _phase = Phase.Telegraph; _timer = 0;
        switch (_attack)
        {
            case Attack.Sweep:
                _sweepRightSide = Local(_player.transform.position).x > 0;
                var marker = _sweepRightSide ? sweepRight : sweepLeft;
                if (marker != null) marker.gameObject.SetActive(true);
                MIAudio.PlayAt("guardian_carga", transform.position, .9f);
                break;
            case Attack.Wave:
                MIAudio.PlayAt("guardian_carga", transform.position, .9f, .8f);
                break;
            case Attack.Stones:
                Vector3 target = Clamp(Local(_player.transform.position));
                Vector3 other = Clamp(target + new Vector3(target.x > 0 ? -4.5f : 4.5f, 0, Random.Range(-2.5f, 2.5f)));
                Place(0, target); Place(1, other);
                break;
        }
    }

    private Vector3 Clamp(Vector3 local) => new Vector3(Mathf.Clamp(local.x, -arenaHalf.x + 1, arenaHalf.x - 1), 0, Mathf.Clamp(local.z, 2.5f, arenaHalf.y * 2 - 4.5f));

    private void Place(int i, Vector3 local)
    {
        if (i >= stones.Length || stones[i] == null) return;
        stones[i].transform.position = transform.TransformPoint(local);
        stones[i].Trigger();
    }

    private float Preparation => _health <= maxHealth * .5f ? .85f : 1.1f;

    private void Telegraph()
    {
        _timer += Time.deltaTime;
        if (body != null)
        {
            float lean = Mathf.Clamp01(_timer / Preparation);
            body.localPosition = _bodyHome + (_attack == Attack.Wave ? Vector3.up * lean * .6f : Vector3.zero);
            body.localRotation = Quaternion.Euler(0, _attack == Attack.Sweep ? (_sweepRightSide ? -1 : 1) * lean * 18f : 0, 0);
        }
        var marker = _sweepRightSide ? sweepRight : sweepLeft;
        if (_attack == Attack.Sweep && marker != null) marker.localScale = new Vector3(1, 1, .6f + .4f * Mathf.PingPong(_timer * 4f, 1f));
        if (_attack == Attack.Stones) { if (_timer >= 1.2f) { _phase = Phase.Execute; _timer = 0; } return; }
        if (_timer < Preparation) return;
        _phase = Phase.Execute; _timer = 0;
        if (_attack == Attack.Sweep)
        {
            if (marker != null) marker.gameObject.SetActive(false);
            MIAudio.PlayAt("guardian_barrido", transform.position);
            Vector3 p = Local(_player.transform.position);
            MIParticles.Burst(transform.TransformPoint(new Vector3(_sweepRightSide ? 4 : -4, .3f, 5)), new Color(.6f, .58f, .55f, .8f), 60, 4f, .16f, 1f);
            if ((p.x > 0) == _sweepRightSide && p.z < 12f && p.y < 2.2f) MundoInferiorBlockout.Instance?.Damage(attackDamage, body != null ? body.position : transform.position);
        }
        if (_attack == Attack.Wave)
        {
            _waveRadius = .5f; _waveHit = false;
            MIAudio.PlayAt("guardian_golpe", transform.position);
            MIParticles.Burst(transform.TransformPoint(new Vector3(0, .3f, 11)), new Color(.6f, .58f, .55f, .8f), 80, 5f, .18f, 1.2f);
            if (wave != null) wave.gameObject.SetActive(true);
        }
    }

    private void Execute()
    {
        _timer += Time.deltaTime;
        if (body != null) { body.localPosition = Vector3.Lerp(body.localPosition, _bodyHome, Time.deltaTime * 6f); body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.identity, Time.deltaTime * 6f); }
        if (_attack == Attack.Wave)
        {
            _waveRadius += waveSpeed * Time.deltaTime;
            DrawWave();
            Vector3 p = Local(_player.transform.position);
            float distance = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(0, 11));
            // The front hurts only where it is and only on the ground: jumping over it is safe.
            if (!_waveHit && Mathf.Abs(distance - _waveRadius) < .55f && p.y < 1.45f) { _waveHit = true; MundoInferiorBlockout.Instance?.Damage(attackDamage, transform.position); }
            if (_waveRadius > 16f) { if (wave != null) wave.gameObject.SetActive(false); Expose(); }
            return;
        }
        if (_attack == Attack.Stones)
        {
            bool busy = false; foreach (var s in stones) if (s != null && s.Busy) busy = true;
            if (!busy || _timer > 4f) Expose();
            return;
        }
        if (_timer > .5f) Expose();
    }

    private void DrawWave()
    {
        if (wave == null) return;
        const int segments = 64;
        wave.positionCount = segments + 1; wave.useWorldSpace = false;
        for (int i = 0; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2 / segments;
            wave.SetPosition(i, new Vector3(Mathf.Cos(a) * _waveRadius, .25f, 11 + Mathf.Sin(a) * _waveRadius));
        }
    }

    private void Expose()
    {
        _phase = Phase.Exposed; _timer = 0;
        SetCore(true);
        MIAudio.PlayAt("nucleo_abre", core != null ? core.position : transform.position);
    }

    private void OnTriggerEnter(Collider other) => Hit(other);
    private void OnTriggerStay(Collider other) => Hit(other);

    // The core volume is the only one that takes damage, once per dash link.
    private void Hit(Collider other)
    {
        if (_phase != Phase.Exposed) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsDashing || player.DashInstanceId == _lastDash) return;
        _lastDash = player.DashInstanceId;
        _health -= player.DashDamage; _flash = 1;
        MIAudio.PlayAt("golpe_nucleo", core != null ? core.position : transform.position);
        MIParticles.Burst(core != null ? core.position : transform.position, new Color(1f, .82f, .45f), 30, 3f, .08f, .2f);
        if (_health <= 0) Defeat(false);
    }

    private void Defeat(bool loaded)
    {
        _phase = Phase.Defeated; _health = 0;
        ClearEffects(); SetCore(false);
        if (coreLight != null) coreLight.intensity = .2f;
        if (body != null) body.localPosition = _bodyHome + Vector3.down * 1.2f;
        if (body != null) body.localRotation = Quaternion.Euler(8, 0, 4);
        if (arenaGate != null) arenaGate.Release();
        if (_music != null) _music.Stop();
        if (loaded) return;
        MIProgress.Set(MIProgress.Guardian);
        MIAudio.Play("victoria", 1f);
        MIParticles.Burst(transform.TransformPoint(new Vector3(0, 2, 11)), new Color(.6f, .58f, .55f, .9f), 160, 5f, .22f, 1.1f);
        var director = MundoInferiorBlockout.Instance;
        director?.Hud?.Notify("Guardián vencido", "El portal de regreso se enciende a la derecha de la cámara.", UIIcon.Portal, UIPalette.Jade);
        director?.RefreshObjective();
    }
}

using Nemequene.UI;
using UnityEngine;

// E07, the caimán-murciélago of MI09 (provisional stone idol until its model exists). Entering the
// start volume with Chía sealed and the jaguar affinity saves checkpoint 08, closes the arena, plays
// C07 (first attempt only) and starts the turn duel of HybridGuardianRules: two bonds broken by
// Jaguar while the core is exposed. The sweep marker, the raised slam, the wave ring and the core
// light only present what the model decided; nothing here deals damage. Victory: guardian and
// Chía freed (flags first), arena and portal open, C09 homage.
public sealed class MIGuardian : MonoBehaviour, IDuelStage
{
    [SerializeField] private string displayName = "Guardián caimán-murciélago";
    [SerializeField] private Transform body;
    [SerializeField] private Transform core;
    [SerializeField] private Light coreLight;
    [SerializeField] private Collider coreHit;
    [SerializeField] private MIGate arenaGate;
    [SerializeField] private Transform sweepLeft, sweepRight;
    [SerializeField] private LineRenderer wave;
    [SerializeField] private MIStalactite[] stones = new MIStalactite[0];
    [SerializeField] private Vector3 startCenter = new Vector3(0, 1, 3), startSize = new Vector3(4, 3, 2);
    [SerializeField] private Vector3 playerMark = new Vector3(0, 0, 4.5f);

    private enum Phase { Waiting, Intro, Duel, Defeated }
    private Phase _phase;
    private PlayerController _player;
    private Vector3 _bodyHome;
    private AudioSource _music;
    private TurnDuelController _duel;
    private DuelMove _move;
    private float _moveTime, _flash;
    private bool _inside, _sweepRight;

    public bool Fighting => _phase == Phase.Intro || _phase == Phase.Duel;
    public float Health01 => _duel != null ? (float)_duel.Model.EnemyHealth / _duel.Model.EnemyMaxHealth : 1f;
    public string DisplayName => displayName;
    public Transform Focus => body != null ? body : transform;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (body != null) _bodyHome = body.localPosition;
        if (coreHit != null) coreHit.enabled = false;
        StoryActor.Ensure(gameObject, "Guardián caimán-murciélago", 4f);
        _music = MIAudio.Loop(gameObject, "musica_guardian", .55f, false); _music.Stop();
        if (MIProgress.Has(MIProgress.Guardian)) { ShowDefeated(); return; }
        ResetEncounter();
    }

    // Player defeat or leaving: no duel, arena open, idol at rest. Finds and flags are kept.
    public void ResetEncounter()
    {
        if (_phase == Phase.Defeated) return;
        if (_duel != null) { var d = _duel; _duel = null; d.Abort(); }
        _phase = Phase.Waiting; _move = null;
        ClearEffects();
        if (arenaGate != null) arenaGate.Release();
        if (_music != null) _music.Stop();
        SetCore(false);
        if (body != null) { body.localPosition = _bodyHome; body.localRotation = Quaternion.identity; }
    }

    private void Update()
    {
        if (_phase == Phase.Defeated || _player == null) return;
        Animate();
        if (_phase != Phase.Waiting) return;
        Vector3 p = transform.InverseTransformPoint(_player.transform.position) - startCenter;
        bool inside = Mathf.Abs(p.x) < startSize.x * .5f && Mathf.Abs(p.y) < startSize.y * .5f && Mathf.Abs(p.z) < startSize.z * .5f;
        if (inside && !_inside) TryBegin();
        _inside = inside;
    }

    // Guion T-I08: the duel needs the sealed mask and the jaguar; otherwise a clear warning.
    private void TryBegin()
    {
        var hud = MundoInferiorBlockout.Instance?.Hud;
        if (!CampaignProgress.FreeTravel)
        {
            if (!CampaignProgress.Has(CampaignFlags.ChiaSealed))
            { hud?.Notify(displayName, "El guardián retiene a Chía. Antes examina la máscara sellada del nicho lunar.", UIIcon.Guardian, UIPalette.GoldLight); return; }
            if (!CampaignProgress.Has(CampaignFlags.CocaAffinity))
            { hud?.Notify(displayName, "Sin la afinidad del jaguar no podrás cortar su lazo. La coca está en el nicho del tercer par.", UIIcon.Guardian, UIPalette.GoldLight); return; }
        }
        Begin();
    }

    private void Begin()
    {
        _phase = Phase.Intro;
        MIProgress.SetCheckpoint(7);
        if (arenaGate != null) arenaGate.SetOpen(false);
        if (_music != null) _music.Play();
        MIAudio.Play("guardian_despierta", 1f);
        _player.SetInputLocked(true);
        _player.Teleport(transform.TransformPoint(playerMark) + Vector3.up * 1.05f);
        _player.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
        // C07 only on the first attempt; a retry starts the duel at once.
        if (!StoryPlayer.Trigger("duel:E07:intro", StartDuel)) StartDuel();
    }

    private void StartDuel()
    {
        if (_phase != Phase.Intro) return;
        _phase = Phase.Duel;
        var director = MundoInferiorBlockout.Instance;
        _duel = TurnDuelController.Run(new HybridGuardianRules(), this, 20, _player.GetComponent<Health>(),
            director != null ? director.Natural : null, director != null && director.Natural != null ? director.Natural.ReactionScale : 1f, OnDuelEnded);
    }

    private void OnDuelEnded(bool victory)
    {
        _duel = null;
        if (!victory) return; // the director's defeat flow resets the encounter
        Defeat();
    }

    // ---------- IDuelStage ----------
    public void OnTelegraph(DuelMove move)
    {
        _move = move; _moveTime = 0;
        ClearEffects();
        if (move.Id == "barrido")
        {
            _sweepRight = !_sweepRight;
            var marker = _sweepRight ? sweepRight : sweepLeft;
            if (marker != null) marker.gameObject.SetActive(true);
            MIAudio.PlayAt("guardian_carga", transform.position, .9f);
        }
        else if (move.Id == "onda") MIAudio.PlayAt("guardian_carga", transform.position, .9f, .8f);
        else if (move.Id == "piedras")
        {
            MIAudio.PlayAt("guardian_carga", transform.position, .8f, 1.2f);
            MIParticles.Burst(transform.TransformPoint(playerMark) + Vector3.up * 7f, new Color(.6f, .58f, .55f, .8f), 40, 1.5f, .14f, .8f);
        }
    }

    public void OnResolved(DuelMove move, bool correct)
    {
        ClearEffects();
        Vector3 mark = transform.TransformPoint(playerMark);
        if (move.Id == "barrido")
        {
            MIAudio.PlayAt("guardian_barrido", transform.position);
            MIParticles.Burst(mark + Vector3.up * .3f, new Color(.6f, .58f, .55f, .8f), 60, 4f, .16f, 1f);
            if (correct && _player != null) _player.PerformDodge(_sweepRight ? -1 : 1);
        }
        else if (move.Id == "onda")
        {
            MIAudio.PlayAt("guardian_golpe", transform.position);
            if (wave != null) { wave.gameObject.SetActive(true); DrawWave(6f); }
        }
        else MIParticles.Burst(mark + Vector3.up * .4f, new Color(.6f, .58f, .55f, .9f), 80, 3f, .2f, 1.1f);
        if (!correct) MIAudio.Play("dano", .8f);
        _move = null;
    }

    public void OnPlayerAction(DuelAction action, DuelTarget target, string result)
    {
        _flash = 1;
        Vector3 at = core != null ? core.position : transform.position + Vector3.up * 3;
        if (action == DuelAction.Horn) { MIAudio.Play("cuerno", .9f); return; }
        MIAudio.PlayAt("golpe_nucleo", at);
        if (action == DuelAction.Jaguar)
        {
            // C08 stand-in until the jaguar model exists: a gold burst along the bond.
            MIParticles.Burst(at, new Color(1f, .78f, .3f), 140, 6f, .2f, .9f);
            StoryPlayer.Trigger("duel:E07:jaguar");
        }
        else MIParticles.Burst(at, new Color(1f, .82f, .45f), 30, 3f, .08f, .2f);
    }

    public void OnDecide()
    {
        var rules = _duel != null ? _duel.Model.Rules as HybridGuardianRules : null;
        SetCore(rules != null && rules.CoreOpen);
    }

    // ---------- presentation ----------
    private void Animate()
    {
        _flash = Mathf.MoveTowards(_flash, 0, Time.deltaTime * 2.5f);
        if (coreLight != null)
        {
            float target = _move != null ? 4f : 2f;
            var rules = _duel != null ? _duel.Model.Rules as HybridGuardianRules : null;
            if (rules != null && rules.CoreOpen && _move == null) target = 9f;
            coreLight.intensity = Mathf.Lerp(coreLight.intensity, target + _flash * 10f, Time.deltaTime * 8f);
        }
        if (body == null) return;
        if (_move != null)
        {
            _moveTime += Time.deltaTime;
            float lean = Mathf.Clamp01(_moveTime / TurnDuelModel.TelegraphSeconds);
            body.localPosition = _bodyHome + (_move.Id == "onda" ? Vector3.up * lean * .6f : Vector3.zero);
            body.localRotation = Quaternion.Euler(0, _move.Id == "barrido" ? (_sweepRight ? -1 : 1) * lean * 18f : 0, 0);
            var marker = _sweepRight ? sweepRight : sweepLeft;
            if (_move.Id == "barrido" && marker != null) marker.localScale = new Vector3(1, 1, .6f + .4f * Mathf.PingPong(_moveTime * 4f, 1f));
        }
        else
        {
            body.localPosition = Vector3.Lerp(body.localPosition, _bodyHome, Time.deltaTime * 6f);
            body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.identity, Time.deltaTime * 6f);
            if (wave != null && wave.gameObject.activeSelf) wave.gameObject.SetActive(false);
        }
        if (core != null) core.localScale = Vector3.one * (1f + (_flash > 0 ? .1f * _flash : 0));
    }

    private void ClearEffects()
    {
        if (sweepLeft != null) sweepLeft.gameObject.SetActive(false);
        if (sweepRight != null) sweepRight.gameObject.SetActive(false);
        if (wave != null) wave.gameObject.SetActive(false);
    }

    private void SetCore(bool exposed)
    {
        if (coreLight != null) coreLight.color = exposed ? new Color(1f, .82f, .45f) : new Color(.4f, .7f, 1f);
    }

    private void DrawWave(float radius)
    {
        const int segments = 64;
        wave.positionCount = segments + 1; wave.useWorldSpace = false;
        for (int i = 0; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2 / segments;
            wave.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, .25f, 11 + Mathf.Sin(a) * radius));
        }
    }

    // Victory (guion H10/C09): flags first, then the presentation and the homage.
    private void Defeat()
    {
        MIProgress.Set(MIProgress.Guardian);
        MIProgress.Set(MIProgress.ChiaReleased);
        ShowDefeated();
        if (_player != null) _player.SetInputLocked(false);
        MIAudio.Play("victoria", 1f);
        MIParticles.Burst(transform.TransformPoint(new Vector3(0, 2, 11)), new Color(.6f, .58f, .55f, .9f), 160, 5f, .22f, 1.1f);
        var director = MundoInferiorBlockout.Instance;
        director?.Hud?.Notify("Chía vuelve a responder", "El guardián está libre. El portal de regreso se enciende a la derecha de la cámara.", UIIcon.Portal, UIPalette.Jade);
        director?.RefreshObjective();
        StoryPlayer.Trigger("duel:E07:won");
    }

    private void ShowDefeated()
    {
        _phase = Phase.Defeated; _move = null;
        ClearEffects(); SetCore(false);
        if (coreLight != null) coreLight.intensity = .2f;
        if (body != null) { body.localPosition = _bodyHome + Vector3.down * 1.2f; body.localRotation = Quaternion.Euler(8, 0, 4); }
        if (arenaGate != null) arenaGate.Release();
        if (_music != null) _music.Stop();
    }
}

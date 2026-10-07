using Nemequene.UI;
using UnityEngine;

// E08, the two-headed serpent of the summit (guion H16), on the provisional guardian model until
// the serpent exists: its left arm stands for head A and its right arm for head B. E on the mark
// starts it (checkpoint 07), C14 plays on the first attempt, then the turn duel of SerpentRules:
// a correct defense makes the announcing head vulnerable; hitting it cuts its bond. The arms, the
// fragments and the core only present what the model decided. Victory writes ms_jefe_vencido first.
public sealed class MSGuardian : MIInteractable, IDuelStage
{
    [SerializeField] private Transform body, leftArm, rightArm, playerMark;
    [SerializeField] private Renderer core;
    [SerializeField] private Transform[] fragments = new Transform[0];
    // Provisional model (hub training guardian): Hit, Attack, Die and Reset triggers.
    [SerializeField] private Animator animator;

    private bool _fighting;
    private TurnDuelController _duel;
    private DuelMove _move;
    private float _moveTime;
    private readonly Color _coreBase = new Color(1f, .7f, .3f);
    private PlayerController _player;

    public bool Fighting => _fighting;
    public bool Defeated => MSProgress.Has(MSProgress.Guardian);
    public float Health01 => _duel != null ? (float)_duel.Model.EnemyHealth / _duel.Model.EnemyMaxHealth : 1f;
    public override bool Available => base.Available && !_fighting && !Defeated;
    public override string Prompt => "Enfrentar a la serpiente";
    public Transform Focus => body != null ? body : transform;
    public Transform PlayerMark => playerMark;

    private CharacterActions _serpent;

    private void Start()
    {
        StoryActor.Ensure(gameObject, "Serpiente", 5f);
        UseSerpentModel();
        if (Defeated) { ShowDefeated(); Vanish(true); }
    }

    // Freed, the serpent goes out in motes once its last words are said: nothing stays at the summit.
    private void Vanish(bool instant)
    {
        Transform shape = _serpent != null ? _serpent.transform : body;
        if (core != null) core.gameObject.SetActive(false);
        // The stand-in pieces kept their colliders under the model: they go too.
        CreatureDissolve.Unblock(body != null ? body.parent : null);
        if (instant) { CreatureDissolve.HideNow(shape); return; }
        CreatureDissolve.Run(shape, new Color(.95f, .85f, .55f), .3f);
    }

    // The rigged serpent (Resources/Characters/Serpiente) replaces the stand-in: the grey pieces
    // and the training-guardian model hide (colliders stay), the core keeps floating on its chest.
    private void UseSerpentModel()
    {
        var root = body != null ? body.parent : null;
        if (root == null || !CharacterModels.Exists("Serpiente")) return;
        CharacterModels.Hide(root, core != null ? core.transform : null);
        _serpent = CharacterModels.Spawn("Serpiente", root, Vector3.zero, Quaternion.identity);
        animator = null;
        if (_serpent != null) _serpent.PlayAny("Emerger");
    }

    public override void Interact(PlayerController player)
    {
        if (!Available) return;
        _player = player;
        _fighting = true;
        MundoSuperiorDirector.Instance?.BeginCombat(this, playerMark);
        // C14: the two-headed serpent rises hissing (the stone stand-in keeps its rumble).
        if (_serpent != null) GameAudio.PlayAt("Criaturas/serpiente_despierta", transform.position + Vector3.up * 3f, 1f, AudioChannel.Voice, 50f);
        else MSAudio.Play("jefe_despierta", .9f);
        // C14 only on the first attempt.
        if (!StoryPlayer.Trigger(StoryTriggers.Duel("E08", "intro"), StartDuel)) StartDuel();
    }

    private void StartDuel()
    {
        if (!_fighting) return;
        var director = MundoSuperiorDirector.Instance;
        _duel = TurnDuelController.Run(new SerpentRules(), this, Mathf.RoundToInt(MSProgress.AttackDamage), _player.GetComponent<Health>(),
            director != null ? director.Natural : null, director != null ? director.ReactionMultiplier : 1f, OnDuelEnded);
    }

    // Defeat or abandon: no duel, model at rest; the next attempt starts over.
    public void ResetEncounter()
    {
        if (_duel != null) { var d = _duel; _duel = null; d.Abort(); }
        _fighting = false; _move = null;
        Pose(0, 0, 0);
        SetCore(_coreBase, 1);
        foreach (var f in fragments) if (f != null) f.gameObject.SetActive(false);
        if (!Defeated) Trigger("Reset");
        UIWorldPrompt.Hide(this);
    }

    private void OnDuelEnded(bool victory)
    {
        _duel = null;
        if (!victory) return; // the director's defeat flow resets the encounter
        MSProgress.Set(MSProgress.Guardian);
        MSAudio.Play("victoria", 1f);
        ShowDefeated();
        _fighting = false;
        MundoSuperiorDirector.Instance?.EndCombat(true);
        if (!StoryPlayer.Trigger(StoryTriggers.Duel("E08", "won"), () => Vanish(false))) Vanish(false);
    }

    // ---------- IDuelStage ----------
    public void OnTelegraph(DuelMove move)
    {
        _move = move; _moveTime = 0;
        Trigger("Attack");
        if (_serpent == null) MSAudio.Play("jefe_aviso", .5f, move.Id == "barrido" ? 1.1f : move.Id == "pulso" ? 1.25f : 1f);
    }

    public void OnResolved(DuelMove move, bool correct)
    {
        if (move.Id == "fragmentos")
            for (int i = 0; i < fragments.Length && i < 2; i++)
                if (fragments[i] != null && playerMark != null) fragments[i].position = playerMark.position + new Vector3(i == 0 ? -1.4f : 1.4f, .4f, i == 0 ? .6f : -.6f);
        if (move.Id == "fragmentos") MSAudio.Play("jefe_golpe", .6f); // the falling stone fragments
        if (correct && move.Id == "barrido" && _player != null) _player.PerformDodge(move.Origin == DuelTarget.HeadA ? 1 : -1);
        _move = null;
    }

    public void OnPlayerAction(DuelAction action, DuelTarget target, string result)
    {
        MSAudio.Play("golpe_nucleo", .3f); // the core rings under the blow DuelAudio plays
        Vector3 at = target == DuelTarget.HeadA && leftArm != null ? leftArm.position
            : target == DuelTarget.HeadB && rightArm != null ? rightArm.position
            : core != null ? core.transform.position : transform.position + Vector3.up * 4;
        MIBurst.Spawn(at, action == DuelAction.Jaguar ? new Color(1f, .78f, .3f) : new Color(1f, .8f, .4f));
        SetCore(Color.white, 3f);
        Trigger("Hit");
    }

    public void OnDecide()
    {
        foreach (var f in fragments) if (f != null) f.gameObject.SetActive(false);
        SetCore(_coreBase, 1);
    }

    // Readable silhouettes: head A (left arm) rises for the frontal press and shakes fragments;
    // head B (right arm) opens for the sweep and swells the core for the pulse.
    private void Update()
    {
        if (!_fighting) return;
        if (_move == null) { Pose(0, 0, 0); return; }
        _moveTime += Time.deltaTime;
        float k = Mathf.Clamp01(_moveTime / TurnDuelModel.TelegraphSeconds);
        switch (_move.Id)
        {
            case "presion": Pose(-110f * k, 0, 0); break;
            case "barrido": Pose(0, 0, 80f * k); break;
            case "pulso": SetCore(new Color(1f, .45f, .2f), 1 + 3 * k); break;
            case "fragmentos":
                Pose(-50f * k, 0, 0);
                for (int i = 0; i < fragments.Length && i < 2; i++)
                {
                    var f = fragments[i]; if (f == null || playerMark == null) continue;
                    f.gameObject.SetActive(true);
                    f.position = playerMark.position + new Vector3(i == 0 ? -.8f : .8f, 9f - 2f * k, i == 0 ? .3f : -.4f);
                }
                break;
        }
    }

    private void Pose(float raiseA, float raiseB, float openB)
    {
        if (leftArm != null) leftArm.localRotation = Quaternion.Euler(raiseA, 0, 0);
        if (rightArm != null) rightArm.localRotation = Quaternion.Euler(raiseB, 0, openB);
    }

    private void Trigger(string name)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var t in new[] { "Hit", "Attack", "Die", "Reset" }) animator.ResetTrigger(t);
        animator.SetTrigger(name);
    }

    private void SetCore(Color color, float intensity)
    {
        if (core == null) return;
        var m = core.material;
        m.SetColor("_BaseColor", color);
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * intensity);
    }

    // The freed serpent rests, its core dark: no hostile logic remains.
    private void ShowDefeated()
    {
        Trigger("Die");
        if (_serpent != null) _serpent.PlayAny("Reposo");
        if (body != null) body.localRotation = Quaternion.Euler(18f, 0, 0);
        Pose(30f, 30f, 0);
        SetCore(new Color(.25f, .22f, .2f), 0);
    }
}

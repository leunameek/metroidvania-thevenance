using UnityEngine;

// Drives the training guardian's Animator from the duel phases (PlazaCombatModel):
// a player hit plays the block reaction, the windup plays the attack so it lands on «¡AHORA!»,
// victory plays the death, and leaving or restarting the duel returns it to idle.
[RequireComponent(typeof(Animator))]
public sealed class GuardianAnimator : MonoBehaviour
{
    private static readonly int HitId = Animator.StringToHash("Hit");
    private static readonly int AttackId = Animator.StringToHash("Attack");
    private static readonly int DieId = Animator.StringToHash("Die");
    private static readonly int ResetId = Animator.StringToHash("Reset");

    [SerializeField] private float attackDelay = 0.7f;
    private Animator _animator;
    private PlazaCombatController _combat;
    private PlazaCombatPhase _phase;
    private int _hits;
    private float _attackAt = -1;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _combat = GetComponentInParent<PlazaCombatController>();
    }
    private void Start()
    {
        if (_combat != null && _combat.Model != null) _combat.Model.Changed += OnChanged;
    }
    private void OnDestroy()
    {
        if (_combat != null && _combat.Model != null) _combat.Model.Changed -= OnChanged;
    }
    private void OnChanged()
    {
        var model = _combat.Model;
        var phase = model.Phase;
        if (phase == _phase && model.Hits == _hits) return;
        bool wasHit = model.Hits > _hits;
        _hits = model.Hits;
        if (phase == PlazaCombatPhase.Idle || (phase == PlazaCombatPhase.Attack && _phase == PlazaCombatPhase.Idle))
        {
            _attackAt = -1; _hits = model.Hits;
            _animator.ResetTrigger(HitId); _animator.ResetTrigger(AttackId); _animator.ResetTrigger(DieId);
            _animator.SetTrigger(ResetId);
        }
        else if (phase == PlazaCombatPhase.Won) { _attackAt = -1; _animator.SetTrigger(DieId); }
        else if (phase == PlazaCombatPhase.Telegraph)
        {
            if (wasHit) _animator.SetTrigger(HitId);
            // After a hit reaction the windup starts a little later; a repeated lesson attacks at once.
            _attackAt = Time.time + (wasHit ? attackDelay : 0.1f);
        }
        _phase = phase;
    }
    private void Update()
    {
        if (_attackAt < 0 || Time.time < _attackAt) return;
        _attackAt = -1;
        if (_combat != null && _combat.Model.Phase == PlazaCombatPhase.Telegraph) _animator.SetTrigger(AttackId);
    }
}

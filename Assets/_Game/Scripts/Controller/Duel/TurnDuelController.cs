using System;
using UnityEngine;
using UnityEngine.InputSystem;

// What a duel scene shows: the encounter object animates warnings, blows and bond breaks from
// these calls. None of them deal damage; TurnDuelModel already decided everything.
public interface IDuelStage
{
    Transform Focus { get; }
    void OnTelegraph(DuelMove move);
    void OnResolved(DuelMove move, bool correct);
    void OnPlayerAction(DuelAction action, DuelTarget target, string result);
    void OnDecide();
}

// Runs one TurnDuelModel in the scene (guion 07): reads keys, buttons, voice words (Spanish and
// English) and hand gestures, mirrors the player's lost health to their Health component (so the
// world's own defeat flow runs), drives the HUD and calls the stage for the presentation.
// Keys: 1 Atacar, 2 Contraatacar, 3 Jaguar, 4 Cuerno, 5 Anclar, 6 Interrumpir, 7 Vincular,
// Z / X or arrows choose head A/B or Luna/Sol; F Bloquear, Space Esquivar, G Cubrir, R Parar.
public sealed class TurnDuelController : MonoBehaviour
{
    public static TurnDuelController Current { get; private set; }

    private TurnDuelModel _model;
    private IDuelStage _stage;
    private Health _health;
    private WorldNaturalInput _natural;
    private TurnDuelHUD _hud;
    private Action<bool> _ended;
    private DuelPhase _lastPhase = (DuelPhase)(-1);
    private int _lastRound = -1;
    private bool _over;
    // Without the worlds' natural input (the plaza), the scene's recognizers are heard directly.
    private readonly System.Collections.Generic.List<VoiceCommandRecognizer> _voices = new System.Collections.Generic.List<VoiceCommandRecognizer>();
    private VoiceCommand? _heard;

    public TurnDuelModel Model => _model;
    public static bool Running => Current != null && !Current._over;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Current = null;

    // ended(true) after victory, ended(false) when the player falls or leaves.
    public static TurnDuelController Run(DuelRules rules, IDuelStage stage, int damageBase, Health playerHealth,
        WorldNaturalInput natural, float reactionScale, Action<bool> ended)
    {
        if (Current != null) Current.Abort();
        var go = new GameObject("Duelo_" + rules.Id);
        var duel = go.AddComponent<TurnDuelController>();
        int health = playerHealth != null ? Mathf.CeilToInt(playerHealth.CurrentHealth) : TurnDuelModel.PlayerMaxHealth;
        // Scenes opened from the editor (no save) test the duels with every affinity.
        bool jaguar = CampaignProgress.Has(CampaignFlags.CocaAffinity) || CampaignProgress.FreeTravel;
        duel._model = new TurnDuelModel(rules, damageBase, jaguar, health);
        // The plaza has no world input: its voice setting decides the longer window there.
        bool voice = natural != null ? natural.VoiceOn : VoicePrompt.Enabled;
        duel._model.ResponseSeconds = (voice ? TurnDuelModel.VoiceWindow : TurnDuelModel.KeyboardWindow) * Mathf.Clamp(reactionScale, 1f, 3f);
        duel._stage = stage; duel._health = playerHealth; duel._natural = natural; duel._ended = ended;
        duel._hud = new TurnDuelHUD(go.transform, rules.Name, duel.Act, duel.Target, duel.Defend);
        if (playerHealth != null) playerHealth.Died += duel.OnPlayerDied;
        Current = duel;
        natural?.ClearPending();
        if (natural == null)
            foreach (var recognizer in FindObjectsByType<VoiceCommandRecognizer>(FindObjectsSortMode.None))
            { recognizer.CommandRecognized += duel.OnVoice; duel._voices.Add(recognizer); }
        return duel;
    }

    // Leaving (pause → plaza, player defeat): nothing is awarded.
    public void Abort()
    {
        if (_over) return;
        Finish(false);
    }

    private void OnPlayerDied() => Abort();

    private void OnVoice(VoiceCommand command, string phrase) => _heard = command;

    private void OnDestroy()
    {
        foreach (var voice in _voices) if (voice != null) voice.CommandRecognized -= OnVoice;
        if (_health != null) _health.Died -= OnPlayerDied;
        if (Current == this) Current = null;
    }

    private void Update()
    {
        if (_over || _model == null) return;
        if (Time.timeScale <= 0f || StoryPlayer.Active) { _hud.Show(false); return; }
        _hud.Show(true);
        ReadInput();
        if (_over) return;
        int before = _model.PlayerHealth;
        _model.Tick(Time.deltaTime);
        Present(before);
    }

    private void ReadInput()
    {
        var k = Keyboard.current;
        int before = _model.PlayerHealth;
        switch (_model.Phase)
        {
            case DuelPhase.Decide:
                _natural?.SetContext(NaturalContext.Duel, "Atacar");
                if (k != null)
                {
                    if (k.zKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) Target(0);
                    if (k.xKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) Target(1);
                    if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) Act(DuelAction.Attack);
                    else if (k.digit2Key.wasPressedThisFrame) Act(DuelAction.Counter);
                    else if (k.digit3Key.wasPressedThisFrame) Act(DuelAction.Jaguar);
                    else if (k.digit4Key.wasPressedThisFrame) Act(DuelAction.Horn);
                    else if (k.digit5Key.wasPressedThisFrame) Act(DuelAction.Anchor);
                    else if (k.digit6Key.wasPressedThisFrame) Act(DuelAction.Interrupt);
                    else if (k.digit7Key.wasPressedThisFrame) Act(DuelAction.Bind);
                }
                if (_heard.HasValue)
                {
                    var word = _heard.Value; _heard = null;
                    if (VoiceVocabulary.ToTarget(word, out var target)) _model.SelectTarget(target);
                    else if (VoiceVocabulary.ToAction(word, out var action)) Act(action);
                }
                if (_natural != null)
                {
                    if (_natural.ConsumeVoice(c => VoiceVocabulary.ToAction(c, out _) || VoiceVocabulary.ToTarget(c, out _), out var word, out _))
                    {
                        if (VoiceVocabulary.ToTarget(word, out var target)) _model.SelectTarget(target);
                        else if (VoiceVocabulary.ToAction(word, out var action)) Act(action);
                    }
                    else if (_natural.ConsumeGesture(HandGesture.Strike, "Puño · Atacar")) Act(DuelAction.Attack);
                }
                break;
            case DuelPhase.Telegraph:
            case DuelPhase.Respond:
                _natural?.SetContext(NaturalContext.Defend, _model.Move != null ? _model.Move.Verbs : "");
                DuelDefense? defense = null;
                if (k != null)
                {
                    if (k.fKey.wasPressedThisFrame) defense = DuelDefense.Block;
                    else if (k.spaceKey.wasPressedThisFrame) defense = DuelDefense.Dodge;
                    else if (k.gKey.wasPressedThisFrame) defense = DuelDefense.Cover;
                    else if (k.rKey.wasPressedThisFrame) defense = DuelDefense.Parry;
                }
                if (defense == null && _heard.HasValue)
                {
                    if (_model.Phase == DuelPhase.Respond && VoiceVocabulary.ToDefense(_heard.Value, out var said)) defense = said;
                    _heard = null;
                }
                if (defense == null && _natural != null)
                {
                    if (_natural.ConsumeVoice(c => VoiceVocabulary.ToDefense(c, out _), out var word, out _) && VoiceVocabulary.ToDefense(word, out var d)) defense = d;
                    else if (_model.Phase == DuelPhase.Respond && _natural.GuardPoseHeld) defense = DuelDefense.Block;
                    else if (_natural.ConsumeGesture(HandGesture.Swipe, "Barrido · Esquivar")) defense = DuelDefense.Dodge;
                }
                if (defense != null) Defend(defense.Value);
                break;
        }
        if (_model.PlayerHealth != before) MirrorDamage(before);
    }

    // Buttons and keys share these entries.
    public void Act(DuelAction action)
    {
        if (_model.Phase != DuelPhase.Decide) return;
        var target = _model.Target;
        var result = _model.Act(action);
        if (result == DuelInput.Accepted) _stage?.OnPlayerAction(action, target, _model.Message);
        _hud.Refresh(_model, true);
    }
    public void Target(int index)
    {
        var targets = _model.Rules.Targets;
        if (index >= 0 && index < targets.Length) _model.SelectTarget(targets[index]);
        _hud.Refresh(_model, true);
    }
    public void Defend(DuelDefense defense)
    {
        int before = _model.PlayerHealth;
        _model.Defend(defense);
        if (_model.PlayerHealth != before) MirrorDamage(before);
        _hud.Refresh(_model, true);
    }

    private void MirrorDamage(int before)
    {
        int lost = before - _model.PlayerHealth;
        if (lost > 0 && _health != null) _health.TakeDamage(lost);
    }

    private void Present(int healthBefore)
    {
        if (_model.PlayerHealth != healthBefore) MirrorDamage(healthBefore);
        if (_model.Phase != _lastPhase || _model.Round != _lastRound)
        {
            var previous = _lastPhase;
            _lastPhase = _model.Phase; _lastRound = _model.Round;
            switch (_model.Phase)
            {
                case DuelPhase.Telegraph: _natural?.ClearPending(); _stage?.OnTelegraph(_model.Move); break;
                case DuelPhase.Respond: _natural?.ClearPending(); break;
                case DuelPhase.Resolve: if (previous == DuelPhase.Respond && _model.Move != null) _stage?.OnResolved(_model.Move, _model.LastDefenseCorrect); break;
                case DuelPhase.Decide: _natural?.ClearPending(); _stage?.OnDecide(); break;
                case DuelPhase.Won: Finish(true); return;
                case DuelPhase.Lost: Finish(false); return;
            }
        }
        _hud.Refresh(_model, false);
    }

    private void Finish(bool victory)
    {
        if (_over && _ended == null) return;
        _over = true;
        _natural?.SetContext(NaturalContext.None);
        if (Current == this) Current = null;
        var ended = _ended; _ended = null;
        Destroy(gameObject);
        ended?.Invoke(victory);
    }
}

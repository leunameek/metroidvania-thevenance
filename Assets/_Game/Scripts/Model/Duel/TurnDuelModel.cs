using System;
using System.Collections.Generic;

// Turn-based reactive duel shared by every main encounter (guion 07 "Contrato de combate").
// Order: Decide -> Execute -> Telegraph (PREPARA) -> Respond (RESPONDE) -> Resolve -> Decide.
// Only this model deals damage; voice, keys, hands and animations just call it. A phase only
// accepts its own kind of input, so a late or repeated command can never act twice.
public enum DuelPhase { Decide, Execute, Telegraph, Respond, Resolve, Won, Lost }
public enum DuelAction { Attack, Counter, Jaguar, Horn, Anchor, Interrupt, Bind }
public enum DuelDefense { Block, Dodge, Cover, Parry }
public enum DuelTarget { None, HeadA, HeadB, Moon, Sun }
public enum DuelInput { Accepted, WrongPhase, TooEarly, NotLegal, NeedsTarget }
public enum CounterWindow { None, Normal, Reinforced }
// The shared language of the warnings (2026-10-07: the player reads the enemy instead of being
// told the answer). Each defense has one signal in every duel: a blow that comes straight on is
// blocked, one that sweeps or rushes is dodged, one that falls from above is covered; a golden
// glint of tumbaga marks a physical blow that can also be parried.
public enum DuelSignal { Front, Sweep, Above }

public sealed class DuelMove
{
    public string Id;
    public string Label;               // shown and read in the warning, e.g. "Barrido"
    public DuelDefense[] Answers;      // defenses that resolve it (Parry is added only if Physical)
    public bool Physical;              // a defended physical blow opens the counter window
    public int FailDamage = 20;
    public DuelTarget Origin;          // head or moon/sun that announces it
    public bool Wind;                  // nullified by a previous Anclar
    public bool Charge;                // cancelled by Interrumpir

    // The signal follows the main answer (the first one that is not Parar).
    public DuelSignal Signal
    {
        get
        {
            foreach (var d in Answers)
                if (d == DuelDefense.Block) return DuelSignal.Front;
                else if (d == DuelDefense.Dodge) return DuelSignal.Sweep;
                else if (d == DuelDefense.Cover) return DuelSignal.Above;
            return DuelSignal.Front;
        }
    }
    public bool Glint => Physical && Array.IndexOf(Answers, DuelDefense.Parry) >= 0;

    public bool Accepts(DuelDefense defense)
    {
        if (defense == DuelDefense.Parry) return Physical && Array.IndexOf(Answers, DuelDefense.Parry) >= 0;
        return Array.IndexOf(Answers, defense) >= 0;
    }
    public string Verbs
    {
        get
        {
            var names = new List<string>();
            foreach (var d in Answers) names.Add(TurnDuelModel.DefenseName(d));
            return string.Join(" o ", names);
        }
    }
}

public sealed class TurnDuelModel
{
    public const int PlayerMaxHealth = 100, MaxConcentration = 3;
    public const float TelegraphSeconds = 1.8f, ExecuteSeconds = 0.9f, ResolveSeconds = 1.1f;
    // The jaguar roars, springs and lands before the enemy answers (JaguarSpirit).
    public const float JaguarExecuteSeconds = 2.7f;
    public const float KeyboardWindow = 2.4f, VoiceWindow = 4.8f;

    public readonly DuelRules Rules;
    public DuelPhase Phase { get; private set; }
    public int PlayerHealth { get; private set; } = PlayerMaxHealth;
    public int Concentration { get; private set; } = 1;
    public CounterWindow Counter { get; private set; }
    public int EnemyHealth { get; private set; }
    public int EnemyMaxHealth => Rules.MaxHealth;
    public int DamageBase { get; }
    public bool JaguarAvailable { get; }
    public DuelMove Move { get; private set; }
    public DuelTarget Target { get; private set; }
    public float Remaining { get; private set; }
    public float ResponseSeconds { get; set; } = KeyboardWindow;
    public bool Untimed { get; set; }
    // Accessibility option: the warning names the answer instead of leaving it to the signals.
    public bool ShowAnswers { get; set; }
    public int Turn { get; private set; }
    public int Round { get; private set; }
    public bool LastDefenseCorrect { get; private set; }
    public string Message { get; private set; } = "";
    public bool PhaseTwo => Rules.PhaseTwoAt > 0 && EnemyHealth <= Rules.PhaseTwoAt;
    public bool IsOver => Phase == DuelPhase.Won || Phase == DuelPhase.Lost;
    public event Action Changed;

    // damageBase: 20 + 5 per optional yopo, up to 30 (guion 07 "Recursos de combate").
    public TurnDuelModel(DuelRules rules, int damageBase = 20, bool jaguar = false, int playerHealth = PlayerMaxHealth)
    {
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        PlayerHealth = Math.Max(1, Math.Min(PlayerMaxHealth, playerHealth));
        DamageBase = Math.Max(1, Math.Min(30, damageBase));
        JaguarAvailable = jaguar;
        EnemyHealth = rules.MaxHealth;
        Rules.Begin(this);
        Phase = DuelPhase.Decide; Turn = 1;
        Message = rules.Opening;
    }

    public static string DefenseName(DuelDefense d) =>
        d == DuelDefense.Block ? "Bloquear" : d == DuelDefense.Dodge ? "Esquivar" : d == DuelDefense.Cover ? "Cubrir" : "Parar";
    public static string ActionName(DuelAction a)
    {
        switch (a)
        {
            case DuelAction.Attack: return "Atacar";
            case DuelAction.Counter: return "Contraatacar";
            case DuelAction.Jaguar: return "Jaguar";
            case DuelAction.Horn: return "Cuerno";
            case DuelAction.Anchor: return "Anclar";
            case DuelAction.Interrupt: return "Interrumpir";
            default: return "Vincular";
        }
    }
    public static string TargetName(DuelTarget t) =>
        t == DuelTarget.HeadA ? "Cabeza A" : t == DuelTarget.HeadB ? "Cabeza B" : t == DuelTarget.Moon ? "Luna" : t == DuelTarget.Sun ? "Sol" : "";

    // ---------- Decision ----------
    public IEnumerable<DuelAction> LegalActions()
    {
        foreach (DuelAction a in Enum.GetValues(typeof(DuelAction)))
            if (IsLegal(a, out _)) yield return a;
    }
    public bool IsLegal(DuelAction action, out string reason)
    {
        reason = "";
        if (Phase != DuelPhase.Decide) { reason = "Espera tu turno."; return false; }
        if (action == DuelAction.Counter)
        {
            if (Counter == CounterWindow.None) { reason = "Contraatacar necesita una defensa correcta ante un golpe físico."; return false; }
            if (Concentration < 1) { reason = "Sin concentración."; return false; }
        }
        if (action == DuelAction.Jaguar && !JaguarAvailable) { reason = "Aún no tienes la afinidad del jaguar."; return false; }
        if ((action == DuelAction.Jaguar || action == DuelAction.Bind) && Concentration < 1)
        { reason = "Sin concentración: defiende bien para recuperarla."; return false; }
        return Rules.IsLegal(this, action, ref reason);
    }

    // Choosing a head / moon / sun never spends the turn (guion 07 "Objetivos").
    public DuelInput SelectTarget(DuelTarget target)
    {
        if (Phase != DuelPhase.Decide) return DuelInput.WrongPhase;
        if (Array.IndexOf(Rules.Targets, target) < 0) return DuelInput.NotLegal;
        Target = target; Message = TargetName(target) + " elegida."; Changed?.Invoke();
        return DuelInput.Accepted;
    }

    public DuelInput Act(DuelAction action)
    {
        if (Phase != DuelPhase.Decide) return DuelInput.WrongPhase;
        if (!IsLegal(action, out string reason)) { Message = reason; Changed?.Invoke(); return DuelInput.NotLegal; }
        if (Rules.NeedsTarget(action) && Target == DuelTarget.None)
        { Message = "Elige primero: " + TargetNames(); Changed?.Invoke(); return DuelInput.NeedsTarget; }

        int bonus = 0;
        if (action == DuelAction.Counter) bonus = Counter == CounterWindow.Reinforced ? 20 : 10;
        if (action == DuelAction.Jaguar) bonus = 10;
        if (action == DuelAction.Counter || action == DuelAction.Jaguar || action == DuelAction.Bind) Concentration--;
        Counter = CounterWindow.None; // used now or lost by choosing anything else

        var hit = new DuelHit(action, Target, DamageBase, bonus);
        Rules.Apply(this, ref hit);
        EnemyHealth = Math.Max(Rules.MinimumHealth(this), EnemyHealth - Math.Max(0, hit.Damage));
        Message = hit.Text;
        Target = DuelTarget.None;
        if (EnemyHealth <= 0) { Phase = DuelPhase.Won; Message = Rules.Victory; Changed?.Invoke(); return DuelInput.Accepted; }
        Set(DuelPhase.Execute, action == DuelAction.Jaguar ? JaguarExecuteSeconds : ExecuteSeconds);
        return DuelInput.Accepted;
    }
    private string TargetNames()
    {
        var names = new List<string>();
        foreach (var t in Rules.Targets) names.Add(TargetName(t));
        return string.Join(" / ", names);
    }

    // ---------- Defense ----------
    public DuelInput Defend(DuelDefense defense)
    {
        if (Phase == DuelPhase.Telegraph) { Message = "Repite al abrir la señal."; Changed?.Invoke(); return DuelInput.TooEarly; }
        if (Phase != DuelPhase.Respond) return DuelInput.WrongPhase;
        // Parar needs concentration; against a blow without the glint it is simply a wrong answer
        // (refusing it would tell the player what the blow was).
        if (defense == DuelDefense.Parry && Concentration < 1)
        { Message = "Sin concentración para Parar."; Changed?.Invoke(); return DuelInput.NotLegal; }
        Resolve(defense);
        return DuelInput.Accepted;
    }
    public bool ParryOffered => Move != null && Concentration >= 1;

    private void Resolve(DuelDefense? defense)
    {
        bool correct = defense.HasValue && Move.Accepts(defense.Value);
        LastDefenseCorrect = correct;
        if (correct)
        {
            if (defense == DuelDefense.Parry) { Concentration--; Counter = CounterWindow.Reinforced; Message = "¡Parada! Contraataque reforzado listo."; }
            else
            {
                Concentration = Math.Min(MaxConcentration, Concentration + 1);
                Counter = Move.Physical ? CounterWindow.Normal : CounterWindow.None;
                Message = "Defensa correcta. +1 concentración.";
            }
        }
        else
        {
            Counter = CounterWindow.None;
            PlayerHealth = Math.Max(0, PlayerHealth - Move.FailDamage);
            Message = (defense.HasValue ? "Defensa equivocada" : "Sin respuesta") + $": -{Move.FailDamage} vida. " + DuelSignals.Reading(Move);
        }
        Rules.OnResolved(this, Move, correct);
        if (PlayerHealth <= 0) { Phase = DuelPhase.Lost; Message = "Has caído. Tus hallazgos se conservan."; Changed?.Invoke(); return; }
        Set(DuelPhase.Resolve, ResolveSeconds);
    }

    // Anclar cancels a wind round: the warning is shown, no answer is asked and it returns 1.
    internal void GrantConcentration(int amount) => Concentration = Math.Max(0, Math.Min(MaxConcentration, Concentration + amount));
    internal void Say(string text) => Message = text;
    internal void HealEnemy(int toAtLeast) => EnemyHealth = Math.Max(EnemyHealth, toAtLeast);

    // ---------- Time ----------
    public void Tick(float dt)
    {
        if (IsOver || Phase == DuelPhase.Decide) return;
        if (Phase == DuelPhase.Respond && Untimed) return;
        Remaining -= Math.Max(0f, dt);
        if (Remaining > 0) return;
        switch (Phase)
        {
            case DuelPhase.Execute: NextEnemyMove(Rules.NextMove(this)); break;
            case DuelPhase.Telegraph: Set(DuelPhase.Respond, ResponseSeconds); break;
            case DuelPhase.Respond: Resolve(null); break;
            case DuelPhase.Resolve:
                var follow = Rules.FollowUp(this);
                if (follow != null) NextEnemyMove(follow);
                else { Turn++; Move = null; Set(DuelPhase.Decide, 0); if (string.IsNullOrEmpty(Message)) Message = "Tu turno."; }
                break;
        }
    }

    private void NextEnemyMove(DuelMove move)
    {
        Round++;
        if (move == null) { Turn++; Move = null; Set(DuelPhase.Decide, 0); return; }
        Move = move;
        if (Rules.Skip(this, move, out string why))
        {
            Message = why; Set(DuelPhase.Resolve, ResolveSeconds); return;
        }
        string origin = move.Origin != DuelTarget.None ? TargetName(move.Origin) + ": " : "";
        Message = ShowAnswers ? origin + move.Label + ". Prepara: " + move.Verbs + "."
            : origin + "prepara un golpe. Lee su cuerpo y escucha.";
        Set(DuelPhase.Telegraph, TelegraphSeconds);
    }

    // Skips the timers, for tests and for the "reduce movement" option.
    public void Advance()
    {
        int guard = 0;
        var start = Phase;
        while (!IsOver && Phase == start && Phase != DuelPhase.Decide && guard++ < 4) Tick(Math.Max(Remaining, 0.001f) + 0.001f);
    }

    private void Set(DuelPhase phase, float duration)
    {
        Phase = phase; Remaining = duration; Changed?.Invoke();
    }
}

// One offensive decision being resolved by the encounter rules.
public struct DuelHit
{
    public readonly DuelAction Action;
    public readonly DuelTarget Target;
    public readonly int Base, Bonus;
    public int Damage;
    public string Text;
    public DuelHit(DuelAction action, DuelTarget target, int damageBase, int bonus)
    {
        Action = action; Target = target; Base = damageBase; Bonus = bonus;
        Damage = action == DuelAction.Attack || action == DuelAction.Counter || action == DuelAction.Jaguar || action == DuelAction.Bind
            ? damageBase + bonus : 0;
        Text = "";
    }
}

// Per-encounter behaviour: patterns, special verbs, bonds and when the duel may end.
public abstract class DuelRules
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract int MaxHealth { get; }
    public virtual int PhaseTwoAt => 0;
    public virtual string Opening => "Tu turno. Decide sin prisa.";
    public virtual string Victory => "Prueba superada.";
    public virtual DuelTarget[] Targets => new DuelTarget[0];
    // One line for the HUD under the enemy name (bonds, exposed core, posture...).
    public virtual string Status(TurnDuelModel duel) => "";

    public virtual void Begin(TurnDuelModel duel) { }
    public virtual bool IsLegal(TurnDuelModel duel, DuelAction action, ref string reason)
    {
        if (action == DuelAction.Attack || action == DuelAction.Counter || action == DuelAction.Jaguar) return true;
        reason = TurnDuelModel.ActionName(action) + " no sirve en este encuentro.";
        return false;
    }
    public virtual bool NeedsTarget(DuelAction action) => false;
    public virtual void Apply(TurnDuelModel duel, ref DuelHit hit)
    {
        hit.Text = $"{TurnDuelModel.ActionName(hit.Action)}: {hit.Damage} de daño.";
    }
    public virtual int MinimumHealth(TurnDuelModel duel) => 0;
    public abstract DuelMove NextMove(TurnDuelModel duel);
    public virtual DuelMove FollowUp(TurnDuelModel duel) => null;
    public virtual bool Skip(TurnDuelModel duel, DuelMove move, out string why) { why = ""; return false; }
    public virtual void OnResolved(TurnDuelModel duel, DuelMove move, bool correct) { }

    protected static DuelMove M(string id, string label, bool physical, int fail, params DuelDefense[] answers) =>
        new DuelMove { Id = id, Label = label, Physical = physical, FailDamage = fail, Answers = answers };
}

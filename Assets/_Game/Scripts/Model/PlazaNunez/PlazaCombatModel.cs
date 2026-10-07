using System;

public enum PlazaCombatPhase { Idle, Attack, Telegraph, React, Feedback, Won }
public enum PlazaDefense { Dodge, Guard }

// Guided turns: 3 attacks, a dodge and a guard. Failed reactions repeat the same lesson.
public sealed class PlazaCombatModel
{
    public PlazaCombatPhase Phase { get; private set; }
    public PlazaDefense Expected => Hits == 1 ? PlazaDefense.Dodge : PlazaDefense.Guard;
    public int Hits { get; private set; }
    public int Mistakes { get; private set; }
    public float Remaining { get; private set; }
    public bool LastDefenseSucceeded { get; private set; }
    public bool IsActive => Phase != PlazaCombatPhase.Idle && Phase != PlazaCombatPhase.Won;
    public float TelegraphSeconds { get; } = 1.8f;
    public float ReactionSeconds { get; private set; } = 2.4f;
    public void SetReactionScale(float scale) { ReactionSeconds = 2.4f * Math.Max(1f, Math.Min(3f, scale)); }
    public event Action Changed;

    public void Start() { Hits = Mistakes = 0; Set(PlazaCombatPhase.Attack); }
    public void Cancel() { Set(PlazaCombatPhase.Idle); }
    public bool Attack()
    {
        if (Phase != PlazaCombatPhase.Attack) return false;
        Hits++;
        Set(Hits == 3 ? PlazaCombatPhase.Won : PlazaCombatPhase.Telegraph, TelegraphSeconds);
        return true;
    }
    public bool Defend(PlazaDefense defense)
    {
        if (Phase != PlazaCombatPhase.React) return false;
        Resolve(defense == Expected);
        return true;
    }
    public void Tick(float dt)
    {
        if (Phase != PlazaCombatPhase.Telegraph && Phase != PlazaCombatPhase.React && Phase != PlazaCombatPhase.Feedback) return;
        Remaining -= Math.Max(0, dt);
        if (Remaining > 0) return;
        if (Phase == PlazaCombatPhase.Telegraph) Set(PlazaCombatPhase.React, ReactionSeconds);
        else if (Phase == PlazaCombatPhase.React) Resolve(false);
        else Set(LastDefenseSucceeded ? PlazaCombatPhase.Attack : PlazaCombatPhase.Telegraph, TelegraphSeconds);
    }
    private void Resolve(bool success)
    {
        LastDefenseSucceeded = success;
        if (!success) Mistakes++;
        Set(PlazaCombatPhase.Feedback, 1.2f);
    }
    private void Set(PlazaCombatPhase phase, float duration = 0)
    {
        Phase = phase; Remaining = duration; Changed?.Invoke();
    }
}

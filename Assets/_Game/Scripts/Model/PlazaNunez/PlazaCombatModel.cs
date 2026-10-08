using System;

public enum PlazaCombatPhase { Idle, Attack, Telegraph, React, Feedback, Won }
public enum PlazaDefense { Dodge, Guard }

// Guided turns that teach to read the guardian (the signal language of every duel, DuelSignal):
// the first two blows are explained (a sweep is dodged, a blow from the front is blocked), the
// next two are only shown — the player reads them. Five attacks win. A failed reaction repeats the
// same blow and explains the signal it had.
public sealed class PlazaCombatModel
{
    // The blow after each attack: two guided, then two to read (not a pattern to memorize).
    private static readonly PlazaDefense[] Lessons = { PlazaDefense.Dodge, PlazaDefense.Guard, PlazaDefense.Guard, PlazaDefense.Dodge };
    public const int GuidedLessons = 2;
    public static int AttacksToWin => Lessons.Length + 1;

    public PlazaCombatPhase Phase { get; private set; }
    public PlazaDefense Expected => Lessons[Math.Max(0, Math.Min(Lessons.Length - 1, Hits - 1))];
    public DuelSignal Signal => Expected == PlazaDefense.Dodge ? DuelSignal.Sweep : DuelSignal.Front;
    // Guided blows name their answer; the rest only when the player asked for it (Accesibilidad).
    public bool Guided => Hits <= GuidedLessons;
    public bool ShowAnswers { get; set; }
    public bool ShowsAnswer => Guided || ShowAnswers;
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
        Set(Hits == AttacksToWin ? PlazaCombatPhase.Won : PlazaCombatPhase.Telegraph, TelegraphSeconds);
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

    // What the guardian's body says, for the guided blows and the panels.
    public string Tell => Signal == DuelSignal.Sweep
        ? "Gira el cuerpo y recoge el brazo hacia un lado; el aire silba: viene barriendo."
        : "Se planta, echa el peso atrás y retumba un tambor grave: viene de frente.";
    // After a mistake: what it was and why.
    public string Reading => (Signal == DuelSignal.Sweep ? "Recogió el brazo a un lado y silbó el aire: era Esquivar."
        : "Se plantó de frente con el tambor grave: era Bloquear.") + " Repetimos el mismo golpe.";

    private void Resolve(bool success)
    {
        LastDefenseSucceeded = success;
        if (!success) Mistakes++;
        // A mistake stays longer on screen: its explanation is the lesson.
        Set(PlazaCombatPhase.Feedback, success ? 1.2f : 2.8f);
    }
    private void Set(PlazaCombatPhase phase, float duration = 0)
    {
        Phase = phase; Remaining = duration; Changed?.Invoke();
    }
}

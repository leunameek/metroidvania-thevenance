using System.Collections.Generic;

// The words of the signal language (DuelSignal): what each warning looks and sounds like, and the
// explanation given after a wrong answer, so a mistake teaches how to read the next one. The duel
// never names the answer before the blow unless the player asked for it (ShowAnswers) or the
// signal is new to them (first-time lesson, DuelSignalMemory).
public static class DuelSignals
{
    // The memory keys: one per signal plus the glint, which can come with any physical blow.
    public const string GlintKey = "Glint";
    public static string Key(DuelSignal signal) => signal.ToString();

    public static DuelDefense Answer(DuelSignal signal) =>
        signal == DuelSignal.Front ? DuelDefense.Block : signal == DuelSignal.Sweep ? DuelDefense.Dodge : DuelDefense.Cover;

    // What the player sees and hears during the warning.
    public static string Tell(DuelSignal signal)
    {
        switch (signal)
        {
            case DuelSignal.Front: return "Se planta, echa el peso atrás y retumba un tambor grave: el golpe viene de frente.";
            case DuelSignal.Sweep: return "Recoge el golpe hacia un lado y silba el aire: el golpe viene barriendo.";
            default: return "Una sombra crece bajo tus pies y algo cruje arriba: el golpe cae desde lo alto.";
        }
    }
    public const string GlintTell = "Un destello dorado de tumbaga antes del golpe: es un golpe físico, también puedes Parar.";

    // After a wrong or missing answer: what the signal was and what it asked for.
    public static string Reading(DuelMove move)
    {
        if (move == null) return "";
        string seen;
        switch (move.Signal)
        {
            case DuelSignal.Front: seen = "Se plantó de frente con el tambor grave"; break;
            case DuelSignal.Sweep: seen = "Recogió el golpe a un lado con un silbido"; break;
            default: seen = "La sombra crecía bajo tus pies"; break;
        }
        string text = seen + ": era " + TurnDuelModel.DefenseName(Answer(move.Signal)) + ".";
        if (move.Glint) text += " Con el destello dorado también valía Parar.";
        return text;
    }

    // The one-time card for a signal the player has not learned yet; empty when there is none.
    public static string Lesson(DuelMove move, IReadOnlyCollection<string> learned)
    {
        if (move == null) return "";
        var lines = new List<string>();
        if (!Contains(learned, Key(move.Signal)))
            lines.Add(Tell(move.Signal) + "  →  " + TurnDuelModel.DefenseName(Answer(move.Signal)));
        if (move.Glint && !Contains(learned, GlintKey)) lines.Add(GlintTell);
        return string.Join("\n", lines);
    }

    private static bool Contains(IReadOnlyCollection<string> set, string key)
    {
        if (set == null) return false;
        foreach (var k in set) if (k == key) return true;
        return false;
    }
}

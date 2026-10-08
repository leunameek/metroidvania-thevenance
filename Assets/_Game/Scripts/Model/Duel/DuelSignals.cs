// The words of the signal language (DuelSignal): the answer each signal asks for and the
// explanation of a wrong answer (shown only with the accessibility option; the plaza training is
// where the language is taught). The duel never names the answer before the blow.
public static class DuelSignals
{
    public static DuelDefense Answer(DuelSignal signal) =>
        signal == DuelSignal.Front ? DuelDefense.Block : signal == DuelSignal.Sweep ? DuelDefense.Dodge : DuelDefense.Cover;

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
}

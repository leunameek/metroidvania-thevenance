using System.Collections.Generic;

// Words the game listens for, in Spanish and English (decision of 2026-10-06). The first six are
// the original verbs (kept in this order, saved settings and scenes use their values); the rest
// are the duel verbs and targets of guion 07 "Comandos".
public enum VoiceCommand
{
    Dodge, Attack, Guard, Interact, Confirm, Back,
    Counter, Parry, Cover, Interrupt, Anchor, Jaguar, Horn, Bind, Moon, Sun, HeadA, HeadB
}

public static class VoiceVocabulary
{
    // "para" is left out on purpose: as a common Spanish word it would trigger Parry by accident.
    public static readonly Dictionary<string, VoiceCommand> Phrases = new Dictionary<string, VoiceCommand>
    {
        { "esquiva", VoiceCommand.Dodge }, { "esquivar", VoiceCommand.Dodge }, { "dodge", VoiceCommand.Dodge }, { "evade", VoiceCommand.Dodge },
        { "atacar", VoiceCommand.Attack }, { "ataca", VoiceCommand.Attack }, { "golpe", VoiceCommand.Attack },
        { "impulso", VoiceCommand.Attack }, { "attack", VoiceCommand.Attack }, { "strike", VoiceCommand.Attack },
        { "bloquear", VoiceCommand.Guard }, { "bloquea", VoiceCommand.Guard }, { "escudo", VoiceCommand.Guard },
        { "block", VoiceCommand.Guard }, { "guard", VoiceCommand.Guard },
        { "examinar", VoiceCommand.Interact }, { "examina", VoiceCommand.Interact }, { "usar", VoiceCommand.Interact },
        { "activar", VoiceCommand.Interact }, { "entrar", VoiceCommand.Interact }, { "enfrentar", VoiceCommand.Interact },
        { "hablar", VoiceCommand.Interact }, { "examine", VoiceCommand.Interact }, { "use", VoiceCommand.Interact },
        { "activate", VoiceCommand.Interact }, { "enter", VoiceCommand.Interact }, { "talk", VoiceCommand.Interact },
        { "tomar", VoiceCommand.Confirm }, { "recoger", VoiceCommand.Confirm }, { "confirmar", VoiceCommand.Confirm },
        { "siguiente", VoiceCommand.Confirm }, { "continuar", VoiceCommand.Confirm },
        { "take", VoiceCommand.Confirm }, { "confirm", VoiceCommand.Confirm }, { "next", VoiceCommand.Confirm }, { "continue", VoiceCommand.Confirm },
        { "salir", VoiceCommand.Back }, { "volver", VoiceCommand.Back }, { "cerrar", VoiceCommand.Back },
        { "back", VoiceCommand.Back }, { "exit", VoiceCommand.Back }, { "close", VoiceCommand.Back },
        { "contraatacar", VoiceCommand.Counter }, { "contraataca", VoiceCommand.Counter }, { "contraataque", VoiceCommand.Counter },
        { "counter", VoiceCommand.Counter }, { "counterattack", VoiceCommand.Counter },
        { "parar", VoiceCommand.Parry }, { "parry", VoiceCommand.Parry },
        { "cubrir", VoiceCommand.Cover }, { "cúbrete", VoiceCommand.Cover }, { "cover", VoiceCommand.Cover },
        { "interrumpir", VoiceCommand.Interrupt }, { "interrumpe", VoiceCommand.Interrupt }, { "interrupt", VoiceCommand.Interrupt },
        { "anclar", VoiceCommand.Anchor }, { "ancla", VoiceCommand.Anchor }, { "anchor", VoiceCommand.Anchor },
        { "jaguar", VoiceCommand.Jaguar },
        { "cuerno", VoiceCommand.Horn }, { "horn", VoiceCommand.Horn },
        { "vincular", VoiceCommand.Bind }, { "vincula", VoiceCommand.Bind }, { "bind", VoiceCommand.Bind },
        { "luna", VoiceCommand.Moon }, { "chía", VoiceCommand.Moon }, { "moon", VoiceCommand.Moon },
        { "sol", VoiceCommand.Sun }, { "sué", VoiceCommand.Sun }, { "sun", VoiceCommand.Sun },
        { "cabeza a", VoiceCommand.HeadA }, { "izquierda", VoiceCommand.HeadA }, { "head a", VoiceCommand.HeadA }, { "left", VoiceCommand.HeadA },
        { "cabeza b", VoiceCommand.HeadB }, { "derecha", VoiceCommand.HeadB }, { "head b", VoiceCommand.HeadB }, { "right", VoiceCommand.HeadB },
    };

    public static bool TryParse(string phrase, out VoiceCommand command)
    {
        command = default;
        return !string.IsNullOrEmpty(phrase) && Phrases.TryGetValue(phrase.Trim().ToLowerInvariant(), out command);
    }

    public static IEnumerable<string> PhrasesFor(VoiceCommand command)
    {
        foreach (var pair in Phrases) if (pair.Value == command) yield return pair.Key;
    }

    // Duel mapping: offensive verbs, defenses and target choices.
    public static bool ToAction(VoiceCommand c, out DuelAction action)
    {
        switch (c)
        {
            case VoiceCommand.Attack: action = DuelAction.Attack; return true;
            case VoiceCommand.Counter: action = DuelAction.Counter; return true;
            case VoiceCommand.Jaguar: action = DuelAction.Jaguar; return true;
            case VoiceCommand.Horn: action = DuelAction.Horn; return true;
            case VoiceCommand.Anchor: action = DuelAction.Anchor; return true;
            case VoiceCommand.Interrupt: action = DuelAction.Interrupt; return true;
            case VoiceCommand.Bind: action = DuelAction.Bind; return true;
            default: action = default; return false;
        }
    }
    public static bool ToDefense(VoiceCommand c, out DuelDefense defense)
    {
        switch (c)
        {
            case VoiceCommand.Guard: defense = DuelDefense.Block; return true;
            case VoiceCommand.Dodge: defense = DuelDefense.Dodge; return true;
            case VoiceCommand.Cover: defense = DuelDefense.Cover; return true;
            case VoiceCommand.Parry: defense = DuelDefense.Parry; return true;
            default: defense = default; return false;
        }
    }
    public static bool ToTarget(VoiceCommand c, out DuelTarget target)
    {
        switch (c)
        {
            case VoiceCommand.HeadA: target = DuelTarget.HeadA; return true;
            case VoiceCommand.HeadB: target = DuelTarget.HeadB; return true;
            case VoiceCommand.Moon: target = DuelTarget.Moon; return true;
            case VoiceCommand.Sun: target = DuelTarget.Sun; return true;
            default: target = DuelTarget.None; return false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Every rebindable keyboard action of the game. The order is the order of the Controles page.
public enum GameAction
{
    MoveForward, MoveBack, MoveLeft, MoveRight, Jump, Sprint, Dash, Interact, Wings, Descend,
    Attack, Dodge, Guard, Cover, Parry,
    Hands, Talk, Journal, Help, InputMode, Mute, Restart,
}

// Which actions may not share a key: those read in the same moment of play. A key taken by
// another action of the same group is swapped; across groups (E to interact and to attack) it is shared.
public enum BindingGroup { Exploration, Combat, System }

// Keyboard bindings, two keys per action, stored apart from the UI settings so the world scenes
// (which cannot reference the UI assembly) read the same choice. Esc and Enter stay fixed: they
// open the pause and confirm in every menu.
public static class GameBindings
{
    public const string StorageKey = "Nemequene.Input.Bindings.v1";
    public const int Slots = 2;

    [Serializable]
    private sealed class Stored { public int version = 1; public string[] keys; }

    private static readonly Key[,] Defaults =
    {
        { Key.W, Key.UpArrow }, { Key.S, Key.DownArrow }, { Key.A, Key.LeftArrow }, { Key.D, Key.RightArrow },
        { Key.Space, Key.None }, { Key.LeftShift, Key.None }, { Key.Q, Key.None }, { Key.E, Key.None },
        { Key.F, Key.None }, { Key.LeftCtrl, Key.None },
        { Key.E, Key.None }, { Key.Space, Key.None }, { Key.F, Key.None }, { Key.G, Key.None }, { Key.R, Key.None },
        { Key.C, Key.None }, { Key.LeftCtrl, Key.None }, { Key.Tab, Key.None }, { Key.H, Key.None },
        { Key.M, Key.None }, { Key.V, Key.None }, { Key.R, Key.None },
    };

    private static readonly string[] Names =
    {
        "Avanzar", "Retroceder", "Moverse a la izquierda", "Moverse a la derecha", "Saltar / doble salto", "Correr (mantener)",
        "Dash / impulso", "Interactuar / agarrar / examinar", "Desplegar alas", "Descender en vuelo",
        "Atacar", "Esquivar", "Bloquear / defender", "Cubrirse (duelo)", "Desviar (duelo)",
        "Activar cámara de manos", "Hablar (pulsar para hablar)", "Diario y mapa", "Ayuda de controles",
        "Cambiar a ratón / manos", "Silenciar audio", "Reiniciar (pide confirmación)",
    };

    public static readonly Key[] Reserved = { Key.Escape, Key.Enter, Key.NumpadEnter };

    private static Key[,] _keys;
    public static event Action Changed;

    // True while a menu waits for the player to press the new key: gameplay ignores the keyboard.
    public static bool Listening { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { _keys = null; Listening = false; Changed = null; }

    public static int Count => Defaults.GetLength(0);
    public static string Name(GameAction action) => Names[(int)action];

    public static BindingGroup Group(GameAction action) =>
        action <= GameAction.Descend ? BindingGroup.Exploration : action <= GameAction.Parry ? BindingGroup.Combat : BindingGroup.System;

    public static string GroupName(BindingGroup group) =>
        group == BindingGroup.Exploration ? "Movimiento e interacción" : group == BindingGroup.Combat ? "Combate" : "Sistema";

    private static Key[,] Keys
    {
        get
        {
            if (_keys == null) Load();
            return _keys;
        }
    }

    public static Key Get(GameAction action, int slot) => Keys[(int)action, slot];

    private static void Load()
    {
        _keys = (Key[,])Defaults.Clone();
        Stored stored = null;
        try { stored = JsonUtility.FromJson<Stored>(PlayerPrefs.GetString(StorageKey, "")); }
        catch (Exception) { stored = null; }
        if (stored?.keys == null) return;
        // Stored by name ("MoveForward=W,UpArrow"), so adding actions later keeps older choices.
        foreach (var entry in stored.keys)
        {
            int split = entry.IndexOf('=');
            if (split <= 0 || !Enum.TryParse(entry.Substring(0, split), out GameAction action)) continue;
            string[] parts = entry.Substring(split + 1).Split(',');
            for (int slot = 0; slot < Slots && slot < parts.Length; slot++)
                if (Enum.TryParse(parts[slot], out Key key) && Array.IndexOf(Reserved, key) < 0) _keys[(int)action, slot] = key;
        }
    }

    private static void Save()
    {
        var entries = new List<string>();
        for (int i = 0; i < Count; i++) entries.Add((GameAction)i + "=" + _keys[i, 0] + "," + _keys[i, 1]);
        PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(new Stored { keys = entries.ToArray() }));
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    // Assigns a key to one slot. Returns the action that gave the key up (swapped), if any.
    public static GameAction? Set(GameAction action, int slot, Key key)
    {
        if (Array.IndexOf(Reserved, key) >= 0) return null;
        var keys = Keys;
        Key previous = keys[(int)action, slot];
        GameAction? swapped = null;
        if (key != Key.None)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Group((GameAction)i) != Group(action)) continue;
                for (int s = 0; s < Slots; s++)
                {
                    if (keys[i, s] != key || (i == (int)action && s == slot)) continue;
                    // Same action, other slot: just drop the duplicate.
                    keys[i, s] = i == (int)action ? Key.None : previous;
                    if (i != (int)action) swapped = (GameAction)i;
                }
            }
        }
        keys[(int)action, slot] = key;
        Save();
        return swapped;
    }

    public static void ResetDefaults() { _keys = (Key[,])Defaults.Clone(); Save(); }

    // Other actions (any group) that also answer to this key; shown next to the binding.
    public static List<GameAction> SharedWith(GameAction action, Key key)
    {
        var shared = new List<GameAction>();
        if (key == Key.None) return shared;
        for (int i = 0; i < Count; i++)
            if (i != (int)action && (Keys[i, 0] == key || Keys[i, 1] == key)) shared.Add((GameAction)i);
        return shared;
    }

    private static bool Ready(Key key, Keyboard keyboard) => key != Key.None && keyboard != null;

    public static bool Held(GameAction action)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || Listening) return false;
        for (int slot = 0; slot < Slots; slot++)
        { var key = Get(action, slot); if (Ready(key, keyboard) && keyboard[key].isPressed) return true; }
        return false;
    }

    public static bool Pressed(GameAction action)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || Listening) return false;
        for (int slot = 0; slot < Slots; slot++)
        { var key = Get(action, slot); if (Ready(key, keyboard) && keyboard[key].wasPressedThisFrame) return true; }
        return false;
    }

    // -1..1 from two opposite actions (left/right, back/forward).
    public static float Axis(GameAction negative, GameAction positive) => (Held(positive) ? 1f : 0f) - (Held(negative) ? 1f : 0f);

    // Short name for key caps and prompts; the letter keys follow the active keyboard layout.
    public static string KeyLabel(Key key)
    {
        switch (key)
        {
            case Key.None: return "—";
            case Key.Space: return "Espacio";
            case Key.LeftShift: return "Shift";
            case Key.RightShift: return "Shift der.";
            case Key.LeftCtrl: return "Ctrl";
            case Key.RightCtrl: return "Ctrl der.";
            case Key.LeftAlt: return "Alt";
            case Key.RightAlt: return "Alt Gr";
            case Key.Tab: return "Tab";
            case Key.Backspace: return "Retroceso";
            case Key.Delete: return "Supr";
            case Key.Insert: return "Insert";
            case Key.Home: return "Inicio";
            case Key.End: return "Fin";
            case Key.PageUp: return "Re Pág";
            case Key.PageDown: return "Av Pág";
            case Key.CapsLock: return "Bloq Mayús";
            case Key.UpArrow: return "Arriba";
            case Key.DownArrow: return "Abajo";
            case Key.LeftArrow: return "Izquierda";
            case Key.RightArrow: return "Derecha";
        }
        if (key >= Key.Numpad0 && key <= Key.Numpad9) return "Num " + (key - Key.Numpad0);
        if (key >= Key.F1 && key <= Key.F12) return "F" + (key - Key.F1 + 1);
        if (key >= Key.Digit1 && key <= Key.Digit0) return key == Key.Digit0 ? "0" : ((int)(key - Key.Digit1) + 1).ToString();
        var keyboard = Keyboard.current;
        string shown = null;
        try { if (keyboard != null) shown = keyboard[key].displayName; }
        catch (ArgumentException) { shown = null; }
        if (!string.IsNullOrEmpty(shown) && shown.Length <= 3) return shown.ToUpperInvariant();
        return key.ToString();
    }

    // "W · Arriba", or the single key, for menus and help text.
    public static string Label(GameAction action)
    {
        Key first = Get(action, 0), second = Get(action, 1);
        if (first == Key.None && second == Key.None) return "Sin asignar";
        if (first == Key.None) return KeyLabel(second);
        return second == Key.None ? KeyLabel(first) : KeyLabel(first) + " · " + KeyLabel(second);
    }

    // The key shown in a key cap: the first assigned one.
    public static string Cap(GameAction action)
    {
        Key first = Get(action, 0);
        return KeyLabel(first != Key.None ? first : Get(action, 1));
    }

    // Names UIKeyHint accepts in a cap besides single letters and digits.
    public static bool IsKeyLabel(string candidate)
    {
        if (string.IsNullOrEmpty(candidate)) return false;
        foreach (Key key in Enum.GetValues(typeof(Key)))
            if (key != Key.None && KeyLabel(key) == candidate) return true;
        return false;
    }
}

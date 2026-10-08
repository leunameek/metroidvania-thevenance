using System;
using System.Collections.Generic;
using UnityEngine;

// Persistent state of the Mundo Inferior (guide 6.1): finds, shortcuts, gates, defeated
// guardians, the safe checkpoint and the discovered rooms. Written to the active save slot
// immediately on every change; with no slot (editor test of the scene) it lives in memory.
// The player's abilities are derived from it, never accumulated, so a find counts once.
// The finds themselves live in an InventoryModel (Model); this class only saves and loads it.
public static class MIProgress
{
    public const string Seed = "semilla", Bracelets1 = "brazaletes1", Bracelets2 = "brazaletes2", Bracelets3 = "brazaletes3";
    public const string Horn = "cuerno", Shortcut03 = "atajo_03_01", Corridor06 = "corredor_06", Shield04 = "salida_04";
    public const string HornGate = "reja_cuerno", Guardian = "guardian";
    // Story pieces of the campaign (guion O-N03, O-N04, C09).
    public const string Coca = "coca", ChiaSealed = "chia_sellada", ChiaReleased = "chia_libre";
    public const string OfferingPrefix = "ofrenda_";
    public const int OfferingTotal = 6;
    // Optional pieces of the lower world, in route order, for the plaza's cultural archive.
    public static readonly string[,] Offerings =
    {
        { "ofrenda_01", "Fragmento tallado", "Umbral" }, { "ofrenda_02", "Disco agrietado", "Santuario de raíces" },
        { "ofrenda_03", "Medallón calado", "Galería de brazaletes" }, { "ofrenda_04", "Fragmento del muro", "Patio de centinelas" },
        { "ofrenda_05", "Raíz petrificada", "Galería del derrumbe" }, { "ofrenda_06", "Sello de piedra", "Antesala" },
    };
    private const string Prefix = "Bacata.MI.v1.";

    [Serializable]
    private sealed class Data
    {
        public int version = 1;
        public List<string> flags = new List<string>();
        public int checkpoint;
        public int rooms = 1;
    }

    public static readonly InventoryModel Inventory = new InventoryModel();
    private static int _loadedSlot = int.MinValue;
    public static int Checkpoint { get; private set; }
    public static int RoomsMask { get; private set; } = 1;
    public static event Action<string> Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Inventory.Clear(); Checkpoint = 0; RoomsMask = 1; _loadedSlot = int.MinValue; Changed = null;
    }

    public static bool Has(string id) => Inventory.Has(id);
    public static bool HasDoubleJump => WorldInventoryRules.DoubleJump(Inventory);
    // Each find guarantees a minimum level; picking them in any order never exceeds three.
    public static int DashTier => WorldInventoryRules.DashTier(Inventory);
    public static int FindsCount => Inventory.Count(WorldInventoryRules.LowerFinds);
    public static int OfferingsCount => Inventory.CountPrefix(OfferingPrefix);
    public static IEnumerable<string> All => Inventory.All;
    public static int DiscoveredCount
    {
        get { int n = 0; for (int i = 0; i < 9; i++) if ((RoomsMask & (1 << i)) != 0) n++; return n; }
    }

    // Reads the slot chosen in the title menu (WorldTravel.SaveSlot); the same slot keeps memory.
    public static void Load()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot == _loadedSlot) return;
        _loadedSlot = slot;
        Inventory.Clear(); Checkpoint = 0; RoomsMask = 1;
        if (slot < 0) return;
        string json = PlayerPrefs.GetString(Prefix + slot, "");
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var data = JsonUtility.FromJson<Data>(json);
            if (data == null || data.version != 1) return;
            Inventory.Load(data.flags);
            Checkpoint = Mathf.Clamp(data.checkpoint, 0, 8); RoomsMask = data.rooms | 1;
        }
        catch (Exception) { Inventory.Clear(); }
    }

    public static bool Set(string id)
    {
        if (!Inventory.Add(id)) return false;
        Save(); Changed?.Invoke(id); return true;
    }
    public static void SetCheckpoint(int room)
    {
        if (Checkpoint == room) return;
        Checkpoint = room; Save(); Changed?.Invoke("checkpoint");
    }
    public static void Discover(int room)
    {
        int mask = RoomsMask | (1 << room);
        if (mask == RoomsMask) return;
        RoomsMask = mask; Save(); Changed?.Invoke("rooms");
    }

    private static void Save()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot < 0) return;
        var data = new Data { checkpoint = Checkpoint, rooms = RoomsMask, flags = new List<string>(Inventory.All) };
        PlayerPrefs.SetString(Prefix + slot, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    // A new game in a slot, or a deleted slot, starts the lower world over.
    public static void Erase(int slot)
    {
        PlayerPrefs.DeleteKey(Prefix + slot); PlayerPrefs.Save();
        if (slot == _loadedSlot) { Inventory.Clear(); Checkpoint = 0; RoomsMask = 1; }
    }
}

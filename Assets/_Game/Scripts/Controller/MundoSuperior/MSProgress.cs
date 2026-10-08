using System;
using System.Collections.Generic;
using UnityEngine;

// Persistent state of the Mundo Superior (guide 8.1): six finds, the open lock, the defeated
// guardian, the discovered zones, the optional branch and the rest checkpoint. Written to the
// active save slot immediately on every change, in its own namespace (never Bacata.MI); with no
// slot (editor test of the scene) it lives in memory. Abilities are derived from it.
public static class MSProgress
{
    public const string RunePortals = "ms_runa_portales", RuneClimb = "ms_runa_escalada", Wings = "ms_alas";
    public const string Yopo1 = "ms_yopo_1", Yopo2 = "ms_yopo_2", Key = "ms_llave";
    public const string LockOpen = "ms_cierre_abierto", Guardian = "ms_jefe_vencido", Branch = "ms_ramal_yopo2";
    // Story pieces of the campaign (guion E09, E10, C13, O-N07).
    public const string Condor = "ms_condor", Eagle = "ms_aguila", QuimueEcho = "ms_eco_quimue", Sue = "ms_sue";
    // Rest checkpoints by zone index: D01, D04 and D07.
    public const int Rest01 = 0, Rest04 = 3, Rest07 = 6;
    public static readonly string[] Finds = { RunePortals, RuneClimb, Wings, Yopo1, Yopo2, Key };
    private const string Prefix = "Bacata.MS.v1.";

    [Serializable]
    private sealed class Data
    {
        public int version = 1;
        public List<string> flags = new List<string>();
        public int checkpoint;
        public int zones = 1;
    }

    // The finds live in an InventoryModel (Model); this class only saves and loads it.
    public static readonly InventoryModel Inventory = new InventoryModel();
    private static int _loadedSlot = int.MinValue;
    public static int Checkpoint { get; private set; }
    public static int ZonesMask { get; private set; } = 1;
    public static event Action<string> Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Inventory.Clear(); Checkpoint = 0; ZonesMask = 1; _loadedSlot = int.MinValue; Changed = null;
    }

    public static bool Has(string id) => Inventory.Has(id);
    public static int FindsCount => Inventory.Count(Finds);
    // Guide 6.6: each yopo adds 5 to the base 20, computed from the flags, never accumulated.
    public static float AttackDamage => WorldInventoryRules.AttackDamage(Inventory);
    public static int DiscoveredCount { get { int n = 0; for (int i = 0; i < 8; i++) if ((ZonesMask & (1 << i)) != 0) n++; return n; } }

    // Reads the slot chosen in the title menu (WorldTravel.SaveSlot); the same slot keeps memory.
    public static void Load()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot == _loadedSlot) return;
        _loadedSlot = slot;
        Inventory.Clear(); Checkpoint = 0; ZonesMask = 1;
        if (slot < 0) return;
        string json = PlayerPrefs.GetString(Prefix + slot, "");
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var data = JsonUtility.FromJson<Data>(json);
            if (data == null || data.version != 1) return;
            var loaded = new List<string>(data.flags);
            Checkpoint = data.checkpoint == Rest04 || data.checkpoint == Rest07 ? data.checkpoint : Rest01;
            ZonesMask = data.zones | 1;
            // Guide 8.4: keep the requirements consistent with what the data says was reached.
            if (loaded.Contains(LockOpen)) loaded.Add(Key);
            if (loaded.Contains(Wings)) { loaded.Add(RunePortals); loaded.Add(RuneClimb); }
            Inventory.Load(loaded);
        }
        catch (Exception) { Inventory.Clear(); Checkpoint = 0; ZonesMask = 1; }
    }

    public static bool Set(string id)
    {
        if (!Inventory.Add(id)) return false;
        Save(); Changed?.Invoke(id); return true;
    }
    public static void SetCheckpoint(int zone)
    {
        if (Checkpoint == zone) return;
        Checkpoint = zone; Save(); Changed?.Invoke("checkpoint");
    }
    public static void Discover(int zone)
    {
        int mask = ZonesMask | (1 << zone);
        if (mask == ZonesMask) return;
        ZonesMask = mask; Save(); Changed?.Invoke("zones");
    }

    private static void Save()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot < 0) return;
        var data = new Data { checkpoint = Checkpoint, zones = ZonesMask, flags = new List<string>(Inventory.All) };
        PlayerPrefs.SetString(Prefix + slot, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    // A new game in a slot, or a deleted slot, starts the upper world over.
    public static void Erase(int slot)
    {
        PlayerPrefs.DeleteKey(Prefix + slot); PlayerPrefs.Save();
        if (slot == _loadedSlot) { Inventory.Clear(); Checkpoint = 0; ZonesMask = 1; }
    }
}

using System;
using System.Collections.Generic;

// What the player has found and opened in a world: finds (seed, bracelets, horn, runes, wings,
// yopo, key), offerings, shortcuts, freed guardians. Each id counts once. Pure C#: MIProgress and
// MSProgress (Controller) keep one each and write it to the save slot; HUD counters and the
// abilities derived from it read this model.
public sealed class InventoryModel
{
    private readonly HashSet<string> _items = new HashSet<string>();

    public event Action<string> Changed;

    public IEnumerable<string> All => _items;
    public int Total => _items.Count;

    public bool Has(string id) => !string.IsNullOrEmpty(id) && _items.Contains(id);

    // True when the id is new (it is then announced).
    public bool Add(string id)
    {
        if (string.IsNullOrEmpty(id) || !_items.Add(id)) return false;
        Changed?.Invoke(id);
        return true;
    }

    // Replaces the contents silently (a save being read).
    public void Load(IEnumerable<string> ids)
    {
        _items.Clear();
        if (ids == null) return;
        foreach (var id in ids) if (!string.IsNullOrEmpty(id)) _items.Add(id);
    }

    public void Clear() => _items.Clear();

    public int Count(IEnumerable<string> ids)
    {
        int n = 0;
        foreach (var id in ids) if (Has(id)) n++;
        return n;
    }

    public int CountPrefix(string prefix)
    {
        int n = 0;
        foreach (var id in _items) if (id.StartsWith(prefix, StringComparison.Ordinal)) n++;
        return n;
    }
}

// The rules each world derives from its inventory (the abilities are derived, never accumulated).
public static class WorldInventoryRules
{
    // Lower world: the seed gives the double jump; each pair of bracelets guarantees a dash level.
    public static readonly string[] LowerFinds = { "semilla", "brazaletes1", "brazaletes2", "brazaletes3", "cuerno" };
    public static bool DoubleJump(InventoryModel lower) => lower.Has("semilla");
    public static int DashTier(InventoryModel lower) =>
        lower.Has("brazaletes3") ? 3 : lower.Has("brazaletes2") ? 2 : lower.Has("brazaletes1") ? 1 : 0;

    // Upper world: each yopo strengthens the attack in the duels.
    public static float AttackDamage(InventoryModel upper) => 20f + (upper.Has("ms_yopo_1") ? 5f : 0f) + (upper.Has("ms_yopo_2") ? 5f : 0f);
}

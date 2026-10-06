using System;
using UnityEngine;

// The campaign of the active save slot (CampaignModel), stored next to the world records in its
// own key. The worlds keep writing their own flags (MIProgress/MSProgress); they are mirrored
// here under the campaign ids, so a save made before the campaign existed still lands in the
// right chapter. With no slot (a scene opened from the editor) it lives in memory.
public static class CampaignProgress
{
    private const string Prefix = "Bacata.Campaign.v1.";
    private static readonly CampaignModel State = new CampaignModel();
    private static int _loadedSlot = int.MinValue;
    private static StoryScript _script;
    public static event Action<string> Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        State.Clear(); _loadedSlot = int.MinValue; _script = null; Changed = null;
        MIProgress.Changed -= OnLowerChanged; MSProgress.Changed -= OnUpperChanged;
    }

    public static CampaignModel Model { get { Load(); return State; } }
    // A scene opened straight from the editor (no save slot) is a test: every portal is open
    // and nothing is stored (guion 08: "pruebas de editor pueden viajar libremente").
    public static bool FreeTravel => WorldTravel.SaveSlot < 0;
    public static bool Has(string id) => Model.Has(id);
    public static CampaignChapter Chapter => Model.Chapter;
    public static CampaignObjective Objective => Model.Objective;

    // The story text, read once from Resources/Narrative/historia.json.
    public static StoryScript Script
    {
        get
        {
            if (_script != null) return _script;
            var asset = Resources.Load<TextAsset>("Narrative/historia");
            _script = StoryScript.Parse(asset != null ? asset.text : null);
            return _script;
        }
    }

    public static void Load()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot == _loadedSlot) return;
        _loadedSlot = slot;
        State.Clear();
        if (slot >= 0) State.LoadJson(PlayerPrefs.GetString(Prefix + slot, ""));
        MIProgress.Changed -= OnLowerChanged; MIProgress.Changed += OnLowerChanged;
        MSProgress.Changed -= OnUpperChanged; MSProgress.Changed += OnUpperChanged;
        MIProgress.Load(); MSProgress.Load();
        bool imported = false;
        foreach (var f in MIProgress.All) imported |= Import(CampaignWorldFlags.Lower, f);
        foreach (var f in CampaignWorldFlags.WorldIds(CampaignWorldFlags.Upper)) if (MSProgress.Has(f)) imported |= Import(CampaignWorldFlags.Upper, f);
        // A slot with world progress but no story record comes from before the campaign:
        // its player already crossed the plaza, so the prologue is not replayed.
        if (imported) State.Set(CampaignFlags.MiUnlocked);
        if (imported) Save();
    }

    // True when the flag (or anything it implies) was new. Saves at once, like the world records.
    public static bool Set(string id)
    {
        Load();
        if (!State.Set(id)) return false;
        Save();
        Changed?.Invoke(id);
        return true;
    }

    // The plaza's own record (lessons analysed, training won) predates the campaign.
    public static void ImportPlaza(bool lessonsComplete, bool trainingComplete)
    {
        Set(CampaignFlags.HubActive);
        if (lessonsComplete) Set(CampaignFlags.LessonsComplete);
        if (trainingComplete) Set(CampaignFlags.TrainingComplete);
        if (lessonsComplete && trainingComplete) Set(CampaignFlags.MiUnlocked);
    }

    // Plays nothing: marks a story sequence as lived (or skipped) and applies its flags.
    public static void CompleteSequence(string sequenceId)
    {
        var sequence = Script.Get(sequenceId);
        if (sequence == null) { Debug.LogWarning("[Campaign] Unknown sequence " + sequenceId); return; }
        foreach (var f in sequence.sets) Set(f);
        Set(StorySequence.SeenFlag(sequenceId));
    }
    public static bool Seen(string sequenceId) => Has(StorySequence.SeenFlag(sequenceId));

    public static void Erase(int slot)
    {
        PlayerPrefs.DeleteKey(Prefix + slot); PlayerPrefs.Save();
        if (slot == _loadedSlot) { State.Clear(); _loadedSlot = int.MinValue; }
    }

    private static bool Import(string[,] table, string worldId)
    {
        string id = CampaignWorldFlags.ToCampaign(table, worldId);
        return id != null && State.Set(id);
    }
    private static void OnLowerChanged(string worldId)
    {
        if (Import(CampaignWorldFlags.Lower, worldId)) { Save(); Changed?.Invoke(CampaignWorldFlags.ToCampaign(CampaignWorldFlags.Lower, worldId)); }
    }
    private static void OnUpperChanged(string worldId)
    {
        if (Import(CampaignWorldFlags.Upper, worldId)) { Save(); Changed?.Invoke(CampaignWorldFlags.ToCampaign(CampaignWorldFlags.Upper, worldId)); }
    }

    private static void Save()
    {
        int slot = WorldTravel.SaveSlot;
        if (slot < 0) return;
        PlayerPrefs.SetString(Prefix + slot, State.ToJson());
        PlayerPrefs.Save();
    }
}

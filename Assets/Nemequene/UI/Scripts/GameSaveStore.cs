using System;
using UnityEngine;

namespace Nemequene.UI
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int version = 1;
        public int slot;
        public string savedAtUtc;
        public float playSeconds;
        public string[] completedObjectIds = Array.Empty<string>();
        public bool combatCompleted;
        public int visitedWorlds;
        public string checkpoint = "Plaza Núñez";
        public int CompletionPercent => Mathf.Clamp(Mathf.RoundToInt((completedObjectIds.Length + (combatCompleted ? 1 : 0)) * 25f), 0, 100);
    }

    // The current slice has one safe checkpoint. Saves restore learned lessons and the
    // training result at Plaza Núñez, never a position inside a transition or duel.
    public static class GameSaveStore
    {
        public const int SlotCount = 3;
        private const string Prefix = "Bacata.Save.v1.";
        public static int ActiveSlot { get; private set; } = -1;
        public static bool LoadOnNextScene { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { ActiveSlot = -1; LoadOnNextScene = false; }

        public static void Begin(int slot, bool load)
        {
            ActiveSlot = Mathf.Clamp(slot, 0, SlotCount - 1);
            LoadOnNextScene = load;
        }
        public static void ClearPendingLoad() { LoadOnNextScene = false; }
        public static bool TryRead(int slot, out GameSaveData data)
        {
            data = null;
            if (slot < 0 || slot >= SlotCount) return false;
            string json = PlayerPrefs.GetString(Prefix + slot, "");
            if (string.IsNullOrEmpty(json)) return false;
            try { data = JsonUtility.FromJson<GameSaveData>(json); }
            catch (Exception) { data = null; }
            if (data == null || data.version != 1 || data.slot != slot || data.completedObjectIds == null
                || data.playSeconds < 0 || float.IsNaN(data.playSeconds) || float.IsInfinity(data.playSeconds)
                || !DateTime.TryParse(data.savedAtUtc, out _))
            { data = null; return false; }
            return true;
        }
        public static bool IsOccupied(int slot) => PlayerPrefs.HasKey(Prefix + slot);
        public static int LatestSlot()
        {
            int latest = -1;
            string date = "";
            for (int i = 0; i < SlotCount; i++)
                if (TryRead(i, out var save) && string.CompareOrdinal(save.savedAtUtc, date) > 0)
                { latest = i; date = save.savedAtUtc; }
            return latest;
        }
        public static void Write(GameSaveData data)
        {
            if (data == null || data.slot < 0 || data.slot >= SlotCount) return;
            data.version = 1;
            data.savedAtUtc = DateTime.UtcNow.ToString("o");
            PlayerPrefs.SetString(Prefix + data.slot, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        public static void Delete(int slot)
        {
            if (slot < 0 || slot >= SlotCount) return;
            PlayerPrefs.DeleteKey(Prefix + slot);
            PlayerPrefs.Save();
            if (slot == ActiveSlot) { ActiveSlot = -1; LoadOnNextScene = false; }
        }
    }
}

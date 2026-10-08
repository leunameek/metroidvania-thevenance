using System;
using UnityEngine;

// The voice and hand options the player set in Plaza Núñez (Nemequene UI settings), read back
// in the world scenes, which cannot reference the UI assembly. Same PlayerPrefs entry and field
// names, so JsonUtility fills only what is listed here.
[Serializable]
public sealed class NaturalInputPrefs
{
    public const string StorageKey = "Nemequene.UI.Settings.v1";

    public bool voiceEnabled;
    public int confidence = 1;
    public float dwellSeconds = 1, handSensitivity = 1, reactionScale = 1;
    // Accesibilidad: the duels name the right defense instead of leaving it to the signals.
    public bool combatAnswers;
    public string camera = "";

    public static NaturalInputPrefs Load()
    {
        NaturalInputPrefs prefs = null;
        try { prefs = JsonUtility.FromJson<NaturalInputPrefs>(PlayerPrefs.GetString(StorageKey, "")); }
        catch (Exception) { prefs = null; }
        prefs ??= new NaturalInputPrefs();
        // Settings stored before the voice became the default (2026-10-06) count as voice on.
        if (!PlayerPrefs.GetString(StorageKey, "").Contains("\"voiceDefaults\"")) prefs.voiceEnabled = true;
        prefs.confidence = Mathf.Clamp(prefs.confidence, 0, 2);
        prefs.dwellSeconds = Mathf.Clamp(prefs.dwellSeconds, .5f, 3);
        prefs.handSensitivity = Mathf.Clamp(prefs.handSensitivity, .5f, 2);
        prefs.reactionScale = Mathf.Clamp(prefs.reactionScale, 1, 3);
        return prefs;
    }
}

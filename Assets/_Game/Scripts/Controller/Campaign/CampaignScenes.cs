using UnityEngine;
using UnityEngine.SceneManagement;

// Where the campaign happens: the prologue and the epilogue live in the Bacatá scene (BacataDirector),
// everything between them in Plaza Núñez and the two worlds.
public static class CampaignScenes
{
    public const string Title = "Assets/_Game/Scenes/MainMenu.unity";
    public const string Plaza = "Assets/_Game/Scenes/PlazaNunez.unity";
    public const string Bacata = "Assets/_Game/Scenes/Bacata.unity";

    public enum BacataMode { Prologue, Epilogue }
    public static BacataMode NextBacataMode { get; set; } = BacataMode.Prologue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() => NextBacataMode = BacataMode.Prologue;

    // Scene a new game or a loaded slot opens with (WorldTravel.SaveSlot already chosen).
    public static string EntryScene()
    {
        bool prologue = CampaignProgress.Chapter == CampaignChapter.Prologue;
        if (prologue && Application.CanStreamedLevelBeLoaded(Bacata)) { NextBacataMode = BacataMode.Prologue; return Bacata; }
        return Plaza;
    }
}

// H18 → C19-C21: from the plaza back to Bacatá. The death and the legacy are cinematic only.
public static class CampaignEpilogue
{
    public static void Begin()
    {
        Time.timeScale = 1;
        if (!Application.CanStreamedLevelBeLoaded(CampaignScenes.Bacata))
        {
            Debug.LogWarning("[Campaign] Falta la escena de Bacatá en Build Settings; el epílogo se marca como visto.");
            foreach (var id in new[] { "H19", "H20", "H21" }) CampaignProgress.CompleteSequence(id);
            SceneLoader.Load(CampaignScenes.Title);
            return;
        }
        CampaignScenes.NextBacataMode = CampaignScenes.BacataMode.Epilogue;
        SceneLoader.Load(CampaignScenes.Bacata);
    }
}

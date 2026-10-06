using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Carries the plaza session across the scene change to a real world level and back: the plaza
// portal of a world with its own scene loads it, and returning restores the lessons and the
// training result and places the player in front of that portal (not inside its range).
public static class WorldTravel
{
    public const string PlazaScene = "Assets/_Game/Scenes/PlazaNunez.unity";
    public const string LowerWorldScene = "Assets/_Game/Scenes/MundoInferior.unity";
    public const string UpperWorldScene = "Assets/_Game/Scenes/MundoSuperior.unity";

    public static string[] CompletedObjectIds { get; private set; } = Array.Empty<string>();
    public static bool CombatCompleted { get; private set; }
    // Filled by the plaza UI when it exists (map and play time); read back on return.
    public static int VisitedMask { get; set; }
    public static float PlaySeconds { get; set; }
    // World the player is coming back from (0 = not returning).
    public static int ReturningFrom { get; private set; }
    // Save slot chosen in the title menu (-1: none); worlds keep their own state per slot.
    public static int SaveSlot { get; set; } = -1;
    public static event Action Leaving;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        CompletedObjectIds = Array.Empty<string>(); CombatCompleted = false;
        VisitedMask = 0; PlaySeconds = 0; ReturningFrom = 0; Leaving = null; SaveSlot = -1;
    }

    // Scene of a world reached through a plaza portal; null keeps the in-scene threshold.
    public static string SceneFor(int world)
    {
        string scene = world < 0 ? LowerWorldScene : world > 0 ? UpperWorldScene : null;
        return scene != null && Application.CanStreamedLevelBeLoaded(scene) ? scene : null;
    }

    public static void LeavePlaza(int world, string[] completedObjectIds, bool combatCompleted)
    {
        CompletedObjectIds = completedObjectIds ?? Array.Empty<string>();
        CombatCompleted = combatCompleted;
        ReturningFrom = 0;
        Leaving?.Invoke();
        SceneLoader.Load(SceneFor(world));
    }

    public static void ReturnToPlaza(int fromWorld)
    {
        ReturningFrom = fromWorld == 0 ? -1 : fromWorld;
        SceneLoader.Load(PlazaScene);
    }

    public static void ClearReturn() => ReturningFrom = 0;
}

using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Adds the Mundo Superior motor clips to the existing Nemequene Animator Controller once
// ("Flying"; "Hanging Idle" and "Climbing To Top"), without rebuilding the controller, the
// prefab or the scenes that use it.
[InitializeOnLoad]
public static class NemequeneFlightAnimation
{
    private const string ControllerPath = NemequeneImportSettings.ModelFolder + "/Nemequene_Player.controller";
    private const string ClipPath = NemequeneImportSettings.AnimationFolder + "/Flying.fbx";

    static NemequeneFlightAnimation() => EditorApplication.delayCall += Apply;

    [MenuItem("Tools/Nemequene/Add flight animation")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) return;
        if (!controller.parameters.Any(p => p.name == "Flying"))
        {
            Reimport(ClipPath); // loop on
            NemequenePlayerSetup.AddFlight(controller);
            AssetDatabase.SaveAssets();
            Debug.Log(controller.parameters.Any(p => p.name == "Flying") ? "NEMEQUENE_FLIGHT_OK" : "NEMEQUENE_FLIGHT_FAILED: falta " + ClipPath);
        }
        // Re-import the turned climbing clips once if they predate their rotation offset.
        foreach (string clip in new[] { "Hanging Idle", "Climbing To Top" })
        {
            string path = NemequeneImportSettings.AnimationFolder + "/" + clip + ".fbx";
            if (AssetImporter.GetAtPath(path) is ModelImporter importer && importer.clipAnimations.Length > 0
                && Mathf.Abs(importer.clipAnimations[0].rotationOffset - 180f) > .1f)
                Reimport(path);
        }
        if (!controller.parameters.Any(p => p.name == "ClimbStill"))
        {
            Reimport(NemequeneImportSettings.AnimationFolder + "/Hanging Idle.fbx"); // loop on
            NemequenePlayerSetup.AddClimbExtras(controller);
            AssetDatabase.SaveAssets();
            Debug.Log(controller.parameters.Any(p => p.name == "ClimbStill") ? "NEMEQUENE_CLIMB_OK" : "NEMEQUENE_CLIMB_FAILED: faltan Hanging Idle / Climbing To Top");
        }
    }

    private static void Reimport(string path)
    {
        if (AssetImporter.GetAtPath(path) is ModelImporter)
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

// Import rules for the campaign cast in Assets/_Game/Art/Characters/<Personaje>/ (Nemequene and
// Legacy keep their own rules):
//  - Mixamo-rigged models (bones "mixamorig:*") import as Humanoid, so they all share Nemequene's
//    Mixamo clips through Bacata_Humanoid.controller.
//  - Tripo humanoid rigs (mujer-cóndor: Hip/Pelvis/L_Thigh...) import as Generic; the
//    library setup builds their Humanoid avatar with an explicit bone map (CharacterLibrarySetup).
//  - The three creatures use Animations/<Name>_Animado.fbx (tools/Blender/animate_creatures.py):
//    Generic, takes renamed to the clip name after the last "|", idles and gaits looping.
// No materials are imported: the setup builds URP Lit materials from the .fbm textures.
public class CharacterImportSettings : AssetPostprocessor
{
    public const string Root = "Assets/_Game/Art/Characters/";
    private static readonly string[] Own = { Root + "Nemequene/", Root + "Legacy/" };
    // Folders whose model has the Tripo humanoid rig instead of Mixamo's.
    public static readonly string[] TripoHumanoids = { "Mujer+condor" };
    public static readonly string[] Creatures = { "Serpiente+Bicéfala", "Jaguar+(Transformación)", "Guacamaya+(Transformación)" };
    private static readonly string[] LoopingTakes = { "Idle", "Caminar", "Correr", "Vuelo", "Planeo", "Reposo" };

    private static bool InCast(string path, out string folder)
    {
        folder = null;
        path = path.Replace('\\', '/');
        if (!path.StartsWith(Root)) return false;
        foreach (var own in Own) if (path.StartsWith(own)) return false;
        folder = path.Substring(Root.Length).Split('/')[0];
        return true;
    }

    private static bool IsCreatureAnimation(string path) => path.Replace('\\', '/').Contains("/Animations/") && path.EndsWith("_Animado.fbx");

    private void OnPreprocessModel()
    {
        if (!InCast(assetPath, out string folder)) return;
        var importer = (ModelImporter)assetImporter;
        importer.importCameras = false;
        importer.importLights = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
        bool creature = System.Array.IndexOf(Creatures, folder) >= 0;
        bool tripoHuman = System.Array.IndexOf(TripoHumanoids, folder) >= 0;
        if (creature || tripoHuman)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = IsCreatureAnimation(assetPath);
        }
        else
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
        }
    }

    private void OnPreprocessAnimation()
    {
        if (!InCast(assetPath, out _) || !IsCreatureAnimation(assetPath)) return;
        var importer = (ModelImporter)assetImporter;
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            string name = clip.takeName;
            int bar = name.LastIndexOf('|');
            if (bar >= 0) name = name.Substring(bar + 1);
            clip.name = name;
            bool loop = System.Array.IndexOf(LoopingTakes, name) >= 0;
            clip.loopTime = loop; clip.loopPose = loop;
        }
        importer.clipAnimations = clips;
    }

    private void OnPreprocessTexture()
    {
        if (!InCast(assetPath, out _)) return;
        var importer = (TextureImporter)assetImporter;
        string name = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        if (name.Contains("normal")) importer.textureType = TextureImporterType.NormalMap;
        else if (name.Contains("metallic") || name.Contains("roughness") || name.EndsWith("_rm")) importer.sRGBTexture = false;
        importer.maxTextureSize = 2048;
    }
}

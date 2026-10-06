using System.IO;
using UnityEditor;
using UnityEngine;

// Import rules for the Mixamo-rigged Nemequene model and its animation FBX files.
// Everything under Assets/_Game/Art/Characters/Nemequene is imported as Humanoid so the clips
// retarget onto the character; clips never move the root (CharacterController does).
public class NemequeneImportSettings : AssetPostprocessor
{
    public const string ModelFolder = "Assets/_Game/Art/Characters/Nemequene";
    public const string AnimationFolder = ModelFolder + "/Animations";

    private static readonly string[] LoopingClips =
    {
        "Orc Idle", "Walking", "Running", "Running Backward", "Falling Idle", "Climbing Ladder", "Flying", "Hanging Idle"
    };

    private static bool IsNemequeneAsset(string path) => path.Replace('\\', '/').StartsWith(ModelFolder + "/");

    private void OnPreprocessModel()
    {
        if (!IsNemequeneAsset(assetPath)) return;
        var importer = (ModelImporter)assetImporter;
        bool isAnimation = assetPath.StartsWith(AnimationFolder + "/");

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = !isAnimation;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.importAnimation = isAnimation;
        importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
    }

    private void OnPreprocessAnimation()
    {
        if (!IsNemequeneAsset(assetPath) || !assetPath.StartsWith(AnimationFolder + "/")) return;
        var importer = (ModelImporter)assetImporter;
        string clipName = Path.GetFileNameWithoutExtension(assetPath);
        bool loop = System.Array.IndexOf(LoopingClips, clipName) >= 0;

        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            clip.name = clipName;
            clip.loopTime = loop;
            clip.loopPose = loop;
            // Bake rotation so clips never turn the character; leave XZ/Y unbaked so the
            // travel in non in-place clips is discarded (applyRootMotion is off).
            clip.lockRootRotation = true;
            clip.keepOriginalOrientation = true;
            clip.lockRootHeightY = false;
            clip.keepOriginalPositionY = true;
            clip.lockRootPositionXZ = false;
            // "Hanging Idle" and "Climbing To Top" face the opposite way from "Climbing Ladder";
            // turning their root keeps the body facing the wall in the whole climb.
            clip.rotationOffset = clipName == "Hanging Idle" || clipName == "Climbing To Top" ? 180f : 0f;
        }
        importer.clipAnimations = clips;
    }

    private void OnPreprocessTexture()
    {
        if (!IsNemequeneAsset(assetPath)) return;
        var importer = (TextureImporter)assetImporter;
        string name = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        if (name.EndsWith("_normal"))
        {
            importer.textureType = TextureImporterType.NormalMap;
        }
        else if (!name.EndsWith("_basecolor"))
        {
            importer.sRGBTexture = false;
        }
        importer.maxTextureSize = 2048;
    }
}

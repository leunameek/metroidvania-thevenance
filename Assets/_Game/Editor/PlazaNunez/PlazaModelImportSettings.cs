using System.IO;
using UnityEditor;
using UnityEngine;

// Import rules for the Tripo props of Plaza Núñez in Assets/_Game/Art/Environments/PlazaNunez/Models. Textures get the right
// colour space and normal-map type; the training guardian and its Mixamo clips are Humanoid so
// the clips (and Nemequene's idle) retarget onto it. Materials are built by PlazaModelSetup.
public class PlazaModelImportSettings : AssetPostprocessor
{
    public const string Root = "Assets/_Game/Art/Environments/PlazaNunez/Models/";
    public const string GuardianFolder = Root + "Guardian+de+entrenamiento";
    private static readonly string[] Folders =
    {
        "Jarron", "Disco+del+alba", "Guardián+de+Jade", "Pedestal", "Base_entrenamiento",
        "Guardian+de+entrenamiento", "Heaven+Portal", "Portal_Underworld", "Portal1",
        "Arbol", "Arbustos", "Banca", "Entrada+del+archivo", "Escaleras", "Farol", "Fuente", "Jardinera", "Muro",
        "Pavimento", "Muros", "Marca+de+llegada", "Umbral+mundo+inferior", "Umbral+mundo+superior"
    };

    private static bool IsProp(string path)
    {
        path = path.Replace('\\', '/');
        foreach (string folder in Folders) if (path.StartsWith(Root + folder + "/")) return true;
        return false;
    }
    private static bool IsGuardian(string path) => path.Replace('\\', '/').StartsWith(GuardianFolder + "/");
    private static bool IsGuardianClip(string path) => IsGuardian(path) && !Path.GetFileName(path).StartsWith("tripo_convert");

    private void OnPreprocessModel()
    {
        if (!IsProp(assetPath)) return;
        var importer = (ModelImporter)assetImporter;
        importer.importCameras = false;
        importer.importLights = false;
        if (!IsGuardian(assetPath)) return;
        bool clip = IsGuardianClip(assetPath);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = clip;
        importer.materialImportMode = clip ? ModelImporterMaterialImportMode.None : importer.materialImportMode;
        importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
    }

    private void OnPreprocessAnimation()
    {
        if (!IsGuardianClip(assetPath)) return;
        var importer = (ModelImporter)assetImporter;
        string clipName = Path.GetFileNameWithoutExtension(assetPath);
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            clip.name = clipName;
            clip.loopTime = false;
            // The guardian stays on its mark: no root rotation or travel from the clips.
            clip.lockRootRotation = true; clip.keepOriginalOrientation = true;
            clip.lockRootHeightY = false; clip.keepOriginalPositionY = true;
            clip.lockRootPositionXZ = true; clip.keepOriginalPositionXZ = true;
        }
        importer.clipAnimations = clips;
    }

    private void OnPreprocessTexture()
    {
        if (!IsProp(assetPath)) return;
        var importer = (TextureImporter)assetImporter;
        string name = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        if (name.EndsWith("_normal")) importer.textureType = TextureImporterType.NormalMap;
        else if (!name.EndsWith("_basecolor")) importer.sRGBTexture = false;
        importer.maxTextureSize = 2048;
    }
}

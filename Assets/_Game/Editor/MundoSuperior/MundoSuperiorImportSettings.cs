using System.IO;
using UnityEditor;

// Import rules for the Tripo kit of "Assets/_Game/Art/Environments/MundoSuperior/Models": no cameras, lights or
// animation; the decimated MS_*_Optimizado.fbx bring no materials (MundoSuperiorBuilder assigns
// URP ones); textures get their colour space, normal-map type and a 2048 cap.
public class MundoSuperiorImportSettings : AssetPostprocessor
{
    public const string Root = "Assets/_Game/Art/Environments/MundoSuperior/Models/";

    private static bool InKit(string path) => path.Replace('\\', '/').StartsWith(Root);

    private void OnPreprocessModel()
    {
        if (!InKit(assetPath)) return;
        var importer = (ModelImporter)assetImporter;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importAnimation = false;
        if (Path.GetFileName(assetPath).EndsWith("_Optimizado.fbx")) importer.materialImportMode = ModelImporterMaterialImportMode.None;
    }

    private void OnPreprocessTexture()
    {
        if (!InKit(assetPath)) return;
        var importer = (TextureImporter)assetImporter;
        string name = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        if (name.EndsWith("_normal")) importer.textureType = TextureImporterType.NormalMap;
        else if (!name.EndsWith("_basecolor")) importer.sRGBTexture = false;
        importer.maxTextureSize = 2048;
    }
}

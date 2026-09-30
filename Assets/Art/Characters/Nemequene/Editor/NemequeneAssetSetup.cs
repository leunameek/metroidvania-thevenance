using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Explicit, scoped setup. Does not modify scenes or the gameplay player.
public static class NemequeneAssetSetup
{
    private const string Folder = "Assets/Art/Characters/Nemequene";

    [MenuItem("Tools/Nemequene/Prepare character assets")]
    public static void Build()
    {
        var modelPath = Folder + "/Nemequene.fbx";
        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Nemequene.fbx is missing.");
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.motionNodeName = "Root";
        importer.importAnimation = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.globalScale = 1;
        importer.isReadable = true;
        importer.meshCompression = ModelImporterMeshCompression.Off;
        importer.animationCompression = ModelImporterAnimationCompression.Off;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.SaveAndReimport();
        var clips = importer.defaultClipAnimations;
        if (clips.Length == 0) throw new InvalidOperationException("FBX contains no animation.");
        foreach (var clip in clips)
        {
            clip.name = "Idle_Solar";
            clip.loopTime = true;
            clip.loopPose = true;
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        var albedo = ConfigureTexture("Nemequene_BaseColor", true, false);
        var metallic = ConfigureTexture("Nemequene_MetallicSmoothness", false, true);
        ConfigureTexture("Nemequene_Roughness", false, false);
        ConfigureTexture("Nemequene_Metallic", false, false);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
        var materialPath = Folder + "/Nemequene_URP.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        material.shader = shader;
        material.SetColor("_BaseColor", Color.white);
        material.SetTexture("_BaseMap", albedo);
        material.SetTexture("_MetallicGlossMap", metallic);
        material.SetFloat("_Metallic", 1);
        material.SetFloat("_Smoothness", 1);
        material.SetFloat("_SmoothnessTextureChannel", 0);
        material.EnableKeyword("_METALLICSPECGLOSSMAP");
        EditorUtility.SetDirty(material);

        var clipAsset = AssetDatabase.LoadAllAssetsAtPath(modelPath)
            .OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
        var controllerPath = Folder + "/Nemequene_Idle.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var stateMachine = controller.layers[0].stateMachine;
        var state = stateMachine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle_Solar")
            ?? stateMachine.AddState("Idle_Solar");
        state.motion = clipAsset;
        stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var root = new GameObject("Nemequene");
        try
        {
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            var animator = visual.GetComponent<Animator>() ?? visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            foreach (var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.sharedMaterials = new[] { material };
            PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Nemequene.prefab");

            var skinned = visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinned.Length != 1) throw new InvalidOperationException("Expected one skinned renderer.");
            if (animator.avatar == null || !animator.avatar.isValid)
                throw new InvalidOperationException("Generic avatar is not valid.");
            var rendererCheck = skinned[0];
            var mesh = rendererCheck.sharedMesh;
            if (mesh.uv.Length != mesh.vertexCount) throw new InvalidOperationException("UVs were not imported.");
            if (clipAsset.length < 2.95f || clipAsset.length > 3.05f)
                throw new InvalidOperationException("Expected a 3 second animation.");
            if (rendererCheck.bounds.size.y < 2.2f || rendererCheck.bounds.size.y > 2.6f)
                throw new InvalidOperationException("Unexpected character height / FBX unit conversion.");
            var sample = new Mesh();
            try
            {
                clipAsset.SampleAnimation(visual, 0);
                rendererCheck.BakeMesh(sample);
                var first = sample.vertices;
                clipAsset.SampleAnimation(visual, 1.5f);
                rendererCheck.BakeMesh(sample);
                var middle = sample.vertices;
                var motion = first.Zip(middle, (a, b) => Vector3.Distance(a, b)).Max();
                if (motion < .001f) throw new InvalidOperationException("Imported animation is static.");
                clipAsset.SampleAnimation(visual, clipAsset.length);
                rendererCheck.BakeMesh(sample);
                var seam = first.Zip(sample.vertices, (a, b) => Vector3.Distance(a, b)).Max();
                if (seam > .001f) throw new InvalidOperationException("Animation loop has a visible discontinuity.");
                var report = "UNITY_CHARACTER_OK\n" +
                    $"Unity: {Application.unityVersion}\nVertices: {mesh.vertexCount}\n" +
                    $"Triangles: {mesh.triangles.Length / 3}\nBones: {rendererCheck.bones.Length}\n" +
                    $"Height: {rendererCheck.bounds.size.y:F4} m\nClip: {clipAsset.name}\n" +
                    $"Duration: {clipAsset.length:F4} s\nMotion: {motion:F6} m\nLoop seam: {seam:F8} m\n" +
                    $"Avatar valid: {animator.avatar.isValid}\nMaterial: {material.shader.name}\n";
                File.WriteAllText("ArtSource/Characters/Nemequene/unity-validation.txt", report);
                Debug.Log(report);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sample);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
    }

    private static Texture2D ConfigureTexture(string name, bool srgb, bool alpha)
    {
        var path = Folder + "/Textures/" + name + ".png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.sRGBTexture = srgb;
        importer.alphaSource = alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = true;
        importer.maxTextureSize = 2048;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}

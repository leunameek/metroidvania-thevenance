using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlazaPresentationPolish
{
    public static void Apply()
    {
        var scene = EditorSceneManager.OpenScene(Week08SceneBuilder.ScenePath);
        var root = GameObject.Find("PLAZA_NUNEZ_Lobby");
        if (root == null) throw new System.InvalidOperationException("Build the plaza first.");
        PrepareBundledHandModel();
        foreach (var label in root.GetComponentsInChildren<TextMesh>())
            label.characterSize = Mathf.Min(label.characterSize, 0.055f);
        foreach (var portal in root.GetComponentsInChildren<PlazaPortal>())
        {
            bool lower = portal.color.b > portal.color.g;
            string path = "Assets/Art/Environments/PlazaNunez/Materials/Veil_" + (lower ? "Inferior" : "Superior") + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Nemequene/Portal Veil"));
                mat.SetColor("_BaseColor", portal.color); mat.SetColor("_EmissionColor", portal.color);
                AssetDatabase.CreateAsset(mat, path);
            }
            portal.veil.sharedMaterial = mat;
            if (portal.world == 0)
            {
                float x = lower ? -85 : 85, y = lower ? 0 : 8;
                portal.transform.position = new Vector3(x - 7.8f, y, 0);
                portal.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
        var sun = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(x => x.type == LightType.Directional);
        sun.color = new Color(1, 0.96f, 0.88f); sun.intensity = 1.35f;
        RenderSettings.ambientSkyColor = new Color(0.65f, 0.74f, 0.79f);
        RenderSettings.ambientEquatorColor = new Color(0.58f, 0.58f, 0.52f);
        RenderSettings.ambientGroundColor = new Color(0.32f, 0.30f, 0.25f);
        var cave = root.GetComponentsInChildren<Transform>().First(x => x.name == "MundoInferior_Umbral");
        if (cave.Find("Cavern vault") == null)
        {
            var basalt = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Environments/PlazaNunez/Materials/Basalt.mat");
            for (int i = 0; i < 8; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = i == 0 ? "Cavern vault" : "Cave rock";
                rock.transform.SetParent(cave, false);
                rock.transform.localPosition = new Vector3(-10 + i * 3, 9, 6);
                rock.transform.localScale = new Vector3(7, 5, 23);
                rock.GetComponent<Renderer>().sharedMaterial = basalt;
                Object.DestroyImmediate(rock.GetComponent<Collider>());
            }
            var glow = new GameObject("Cavern violet bounce", typeof(Light)).GetComponent<Light>();
            glow.transform.SetParent(cave, false); glow.transform.localPosition = new Vector3(0, 4, 4);
            glow.type = LightType.Point; glow.color = new Color(0.45f, 0.3f, 1); glow.range = 18; glow.intensity = 3;
        }
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("PLAZA_PRESENTATION_POLISH_PASS");
    }
    private static void PrepareBundledHandModel()
    {
        const string source = "Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/";
        const string settingsPath = "Assets/Prototype/Data/Week08/PlazaMediaPipeSettings.asset";
        const string prefabPath = "Assets/Prototype/Prefabs/PlazaHandBootstrap.prefab";
        Directory.CreateDirectory("Assets/StreamingAssets");
        AssetDatabase.Refresh();
        const string modelPath = "Assets/StreamingAssets/hand_landmarker.bytes";
        if (!File.Exists(modelPath))
        {
            if (!AssetDatabase.CopyAsset("Packages/com.github.homuler.mediapipe/PackageResources/MediaPipe/hand_landmarker.bytes", modelPath))
                throw new System.InvalidOperationException("Cannot bundle installed MediaPipe hand model.");
        }
        var settings = AssetDatabase.LoadAssetAtPath<Mediapipe.Unity.Sample.AppSettings>(settingsPath);
        if (settings == null)
        {
            settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mediapipe.Unity.Sample.AppSettings>(source + "Scenes/AppSettings.asset"));
            AssetDatabase.CreateAsset(settings, settingsPath);
        }
        var options = new SerializedObject(settings);
        options.FindProperty("_assetLoaderType").enumValueIndex = (int)Mediapipe.Unity.Sample.AppSettings.AssetLoaderType.StreamingAssets;
        options.FindProperty("_preferredDefaultWebCamWidth").intValue = 640;
        options.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            var contents = PrefabUtility.LoadPrefabContents(source + "Resources/Bootstrap.prefab");
            var s = new SerializedObject(contents.GetComponent<Mediapipe.Unity.Sample.Bootstrap>());
            s.FindProperty("_appSettings").objectReferenceValue = settings; s.ApplyModifiedPropertiesWithoutUndo();
            prefab = PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }
        var session = Object.FindFirstObjectByType<PlazaHandSession>();
        var serialized = new SerializedObject(session);
        serialized.FindProperty("bootstrapPrefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Week08SceneBuilder
{
    public const string ScenePath = "Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity";
    private const string DataPath = "Assets/Prototype/Data/Week08";
    private const string MaterialPath = "Assets/Prototype/Materials/Week08";

    [MenuItem("Nemequene/Week08/Create missing technical demo")]
    public static void Create()
    {
        if (File.Exists(ScenePath))
        {
            Debug.Log("Week08: la escena ya existe; se conserva. Abre " + ScenePath);
            return;
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(MaterialPath);
        AssetDatabase.Refresh();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "TechnicalDemo_Week08";
        var geometry = new GameObject("GRAYBOX_PlazaNunez_PLACEHOLDER").transform;
        var markers = new GameObject("SIGNAGE_ProvisionalFiction").transform;
        Material ground = Material("Ground", new Color(0.3f, 0.33f, 0.34f));
        Material path = Material("Path", new Color(0.52f, 0.48f, 0.35f));
        Material block = Material("Block", new Color(0.4f, 0.44f, 0.45f));
        Material itemMaterial = Material("Objects", new Color(0.85f, 0.65f, 0.15f));
        Material placeholder = Material("Placeholder", new Color(0.42f, 0.6f, 0.67f));
        Cube("Ground", new Vector3(0, -0.5f, 0), new Vector3(40, 1, 40), ground, geometry);
        Cube("Path_NS", new Vector3(0, 0.02f, -5), new Vector3(4, 0.04f, 22), path, geometry);
        Cube("Path_EW", new Vector3(0, 0.02f, 0), new Vector3(30, 0.04f, 3), path, geometry);
        Cube("Boundary_W", new Vector3(-20, 1.5f, 0), new Vector3(1, 3, 40), block, geometry);
        Cube("Boundary_E", new Vector3(20, 1.5f, 0), new Vector3(1, 3, 40), block, geometry);
        Cube("Boundary_N", new Vector3(0, 1.5f, 20), new Vector3(40, 3, 1), block, geometry);
        Cube("Boundary_S_Left", new Vector3(-12, 1.5f, -20), new Vector3(16, 3, 1), block, geometry);
        Cube("Boundary_S_Right", new Vector3(12, 1.5f, -20), new Vector3(16, 3, 1), block, geometry);
        Label("Prueba de caída y retorno", new Vector3(0, 2, -18), markers);
        Cube("RaisedPlatform", new Vector3(0, 1.25f, 12), new Vector3(8, 2.5f, 8), block, geometry);
        var ramp = Cube("Ramp_20degrees", new Vector3(0, 1.15f, 5.4f), new Vector3(4, 0.4f, 8), path, geometry);
        ramp.transform.rotation = Quaternion.Euler(-20, 0, 0);
        for (int i = 0; i < 4; i++)
            Cube("Step_" + i, new Vector3(-8 + i * 1.5f, (i + 1) * 0.3f, 10),
                new Vector3(1.5f, (i + 1) * 0.6f, 3), block, geometry);
        Cube("JumpPlatform_A", new Vector3(-12, 0.35f, 2), new Vector3(2, 0.7f, 2), block, geometry);
        Cube("JumpPlatform_B", new Vector3(-12, 0.75f, 5), new Vector3(2, 1.5f, 2), block, geometry);
        Cube("CollisionWall", new Vector3(12, 1.5f, -8), new Vector3(6, 3, 1), block, geometry);
        Cube("FutureGate_BLOCKED", new Vector3(0, 4, 17), new Vector3(6, 3, 1), block, geometry);
        Label("ACCESO BLOQUEADO / contenido futuro", new Vector3(0, 5.8f, 17), markers);
        Cube("PortalReserve_Underworld", new Vector3(-16, 0.08f, 12), new Vector3(4, 0.16f, 4), path, geometry);
        Cube("PortalReserve_UpperWorld", new Vector3(16, 0.08f, 12), new Vector3(4, 0.16f, 4), path, geometry);
        Label("INFRAMUNDO / reserva de acceso", new Vector3(-15, 2, 12), markers);
        Label("MUNDO SUPERIOR / reserva de acceso", new Vector3(15, 2, 12), markers);
        Cube("CombatArea_RESERVED", new Vector3(11, 0.06f, 6), new Vector3(7, 0.12f, 6), path, geometry);
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy_PLACEHOLDER_Inert_Module02Pending";
        enemy.transform.SetParent(geometry);
        enemy.transform.position = new Vector3(11, 1, 7);
        enemy.GetComponent<Renderer>().sharedMaterial = placeholder;
        Label("COMBATE PENDIENTE / cápsula temporal", new Vector3(11, 3, 7), markers);

        var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = "Player_PLACEHOLDER";
        playerObject.layer = 2; // Ignore Raycast: exclude own capsule from orbit and interaction rays.
        playerObject.transform.position = new Vector3(0, 1.1f, -13);
        UnityEngine.Object.DestroyImmediate(playerObject.GetComponent<CapsuleCollider>());
        playerObject.GetComponent<Renderer>().sharedMaterial = placeholder;
        var character = playerObject.AddComponent<CharacterController>();
        character.height = 2;
        character.radius = 0.45f;
        character.stepOffset = 0.35f;
        character.slopeLimit = 45;
        var player = playerObject.AddComponent<PlayerController>();
        var health = playerObject.AddComponent<Health>();
        Set(player, "enableSprint", true);

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ExplorationOrbitCamera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 150;
        camera.fieldOfView = 60;
        camera.backgroundColor = new Color(0.12f, 0.17f, 0.2f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0, 6, -20);
        cameraObject.transform.LookAt(playerObject.transform.position + Vector3.up);
        var orbit = cameraObject.GetComponent<ExplorationOrbitCamera>();
        Set(orbit, "target", playerObject.transform);
        Set(player, "movementReference", cameraObject.transform);
        var sun = new GameObject("Light_Temporary", typeof(Light)).GetComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.2f;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        RenderSettings.ambientLight = new Color(0.65f, 0.65f, 0.65f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        string[] texts = {
            "Este objeto conserva una parte de la memoria del territorio.",
            "Su significado todavía debe ser interpretado.",
            "Algunas piezas permiten comprender cómo se conectan los tres mundos."
        };
        Vector3[] positions = { new Vector3(-7, 1.5f, -6), new Vector3(8, 1.5f, 0), new Vector3(0, 4, 12) };
        var items = new AnalyzableObject[3];
        for (int i = 0; i < 3; i++)
        {
            string id = "placeholder_" + (i + 1);
            var data = LoadOrCreate<AnalyzableObjectData>(DataPath + "/Object_" + (i + 1) + ".asset", value => {
                value.objectId = id;
                value.displayName = "Objeto temporal " + (i + 1) + " — cubo placeholder";
                value.description = texts[i] + "\nTexto ficticio y provisional; no describe una pieza histórica.";
                value.evidenceStatus = CulturalEvidenceStatus.Fiction;
            });
            Cube("ObjectZone_" + (i + 1), positions[i] - Vector3.up,
                new Vector3(1.5f, 1, 1.5f), block, geometry);
            var item = Cube("Object_" + (i + 1) + "_PLACEHOLDER", positions[i],
                Vector3.one * 0.8f, itemMaterial, geometry);
            items[i] = item.AddComponent<AnalyzableObject>();
            Set(items[i], "data", data);
            Label("OBJETO " + (i + 1) + " / [E]", positions[i] + Vector3.up, markers);
        }
        var config = LoadOrCreate<TechnicalDemoConfig>(DataPath + "/TechnicalDemo.asset", value => {});
        var systems = new GameObject("SYSTEMS_Week08", typeof(TechnicalDemoController), typeof(TechnicalDemoHUD));
        var demo = systems.GetComponent<TechnicalDemoController>();
        Set(demo, "player", player);
        Set(demo, "orbitCamera", orbit);
        Set(demo, "config", config);
        var serialized = new SerializedObject(demo);
        var array = serialized.FindProperty("objects");
        array.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Set(systems.GetComponent<TechnicalDemoHUD>(), "demo", demo);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        if (!EditorBuildSettings.scenes.Any(entry => entry.path == ScenePath))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) }).ToArray();
        Debug.Log("WEEK08_SCENE_CREATED " + ScenePath);
    }

    private static T LoadOrCreate<T>(string path, Action<T> initialize) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        initialize(asset);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static Material Material(string name, Color color)
    {
        string path = MaterialPath + "/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static void Label(string text, Vector3 position, Transform parent)
    {
        var obj = new GameObject("LABEL_" + text, typeof(TextMesh));
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        var mesh = obj.GetComponent<TextMesh>();
        mesh.text = text;
        mesh.characterSize = 0.15f;
        mesh.fontSize = 40;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.color = Color.white;
    }

    private static void Set(UnityEngine.Object obj, string property, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(obj);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Set(UnityEngine.Object obj, string property, bool value)
    {
        var serialized = new SerializedObject(obj);
        serialized.FindProperty(property).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

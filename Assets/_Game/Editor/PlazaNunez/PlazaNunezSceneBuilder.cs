using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

// Explicit migration of the Week08 graybox. Existing scene GUID/player/config are retained.
public static class PlazaNunezSceneBuilder
{
    private const string Art = "Assets/_Game/Art/Environments/PlazaNunez";
    private static Transform _root;
    private static Material _stone, _lightStone, _darkStone, _gold, _wood, _leaf, _water, _teal, _terra;
    private static readonly List<PlazaPortal> Portals = new List<PlazaPortal>();
    private static int _meshIndex;

    [MenuItem("Nemequene/Plaza Núñez/Upgrade lobby and tutorial")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(Week08SceneBuilder.ScenePath);
        if (scene.GetRootGameObjects().Any(x => x.name == "PLAZA_NUNEZ_Lobby"))
        {
            Debug.Log("PLAZA_ALREADY_BUILT: se conservan los cambios manuales de la plaza."); return;
        }
        Directory.CreateDirectory(Art + "/Materials");
        Directory.CreateDirectory(Art + "/Meshes");
        Directory.CreateDirectory("Assets/_Game/Scenes/Dev");
        AssetDatabase.Refresh();
        string backup = "Assets/_Game/Scenes/Dev/PlazaNunez_Graybox.unity";
        if (!File.Exists(backup)) AssetDatabase.CopyAsset(Week08SceneBuilder.ScenePath, backup);
        var demo = Object.FindFirstObjectByType<TechnicalDemoController>(FindObjectsInactive.Include);
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (demo == null || player == null) throw new InvalidOperationException("La escena de base no contiene el jugador y controlador esperados.");
        foreach (GameObject obj in scene.GetRootGameObjects())
            if (obj.name == "GRAYBOX_PlazaNunez_PLACEHOLDER" || obj.name == "SIGNAGE_ProvisionalFiction") Object.DestroyImmediate(obj);
        _root = new GameObject("PLAZA_NUNEZ_Lobby").transform;
        Portals.Clear(); _meshIndex = 0;
        Palette();
        Ground();
        Architecture();
        Gardens();
        Fountain();
        var objects = Objects();
        var combat = Arena(demo);
        Gateways();
        Lighting();
        player.name = "Nemequene_Player";
        player.transform.position = new Vector3(0, 1.12f, -13);
        player.transform.rotation = Quaternion.identity;
        Set(player, "enableSprint", true);
        Set(player, "enableExplorationDash", true);
        var capsule = player.GetComponent<Renderer>();
        var characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Characters/Legacy/Nemequene/Nemequene.prefab");
        if (characterPrefab != null)
        {
            if (capsule != null) capsule.enabled = false;
            var character = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab, player.transform);
            character.name = "Nemequene_Visual";
            character.transform.localPosition = new Vector3(0, -1, 0);
            character.transform.localRotation = Quaternion.identity;
            foreach (Transform child in character.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
            var animator = character.GetComponentInChildren<Animator>();
            if (animator != null) animator.applyRootMotion = false;
        }
        var systems = demo.gameObject;
        systems.SetActive(true);
        systems.name = "SYSTEMS_PlazaNunez";
        var tracker = systems.AddComponent<HandGestureTracker>();
        tracker.enabled = false;
        systems.AddComponent<PlazaHandSession>();
        var audio = systems.AddComponent<PlazaAudio>();
        PlazaAudioBuilder.Build(audio);
        Set(demo, "combat", combat);
        SetArray(demo, "objects", objects);
        SetArray(demo, "portals", Portals.ToArray());
        var config = AssetDatabase.LoadAssetAtPath<TechnicalDemoConfig>("Assets/_Game/Data/PlazaNunez/TechnicalDemo.asset");
        config.introduction = "Entre la piedra y la memoria, dos caminos esperan. Aprende a mirar, defenderte y cruzar el umbral.";
        config.objective = "Activa las tres estaciones y completa el duelo de entrenamiento.";
        config.completion = "La plaza te ha preparado. Elige el mundo inferior o el superior.";
        config.interactionDistance = 3.4f;
        EditorUtility.SetDirty(config);
        var buildScenes = EditorBuildSettings.scenes.Where(x => x.path != Week08SceneBuilder.ScenePath).ToList();
        buildScenes.Insert(0, new EditorBuildSettingsScene(Week08SceneBuilder.ScenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("PLAZA_BUILD_PASS: lobby, 3 gesture lessons, guided combat, 2 outbound/2 return portals, architecture and 16 original audio clips.");
    }
    private static void Palette()
    {
        _stone = Mat("Sandstone", new Color(0.55f, 0.47f, 0.34f));
        _lightStone = Mat("Limestone", new Color(0.83f, 0.77f, 0.6f));
        _darkStone = Mat("Basalt", new Color(0.14f, 0.21f, 0.23f));
        _gold = Mat("AgedGold", new Color(0.72f, 0.47f, 0.12f), 0.68f);
        _wood = Mat("Cedar", new Color(0.22f, 0.10f, 0.055f));
        _leaf = Mat("Laurel", new Color(0.10f, 0.29f, 0.18f));
        _water = Mat("FountainWater", new Color(0.13f, 0.48f, 0.46f), 0.4f, 0.4f);
        _teal = Mat("Turquoise", new Color(0.12f, 0.6f, 0.51f), 0.3f, 0.2f);
        _terra = Mat("Terracotta", new Color(0.54f, 0.23f, 0.12f));
    }
    private static Material Mat(string name, Color color, float metal = 0, float glow = 0)
    {
        string path = Art + "/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metal);
        mat.SetFloat("_Smoothness", metal > 0 ? 0.55f : 0.22f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * glow);
        AssetDatabase.CreateAsset(mat, path); return mat;
    }
    private static Transform Group(string name, Vector3 position, Transform parent = null)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent != null ? parent : _root, false); t.localPosition = position; return t;
    }
    private static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material mat, Transform parent, bool solid = false)
    {
        var obj = GameObject.CreatePrimitive(type); obj.name = name;
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
    private static GameObject Box(string name, Vector3 p, Vector3 s, Material m, Transform parent, bool solid = false)
        => Shape(name, PrimitiveType.Cube, p, s, m, parent, solid);
    private static GameObject Cylinder(string name, Vector3 p, float radius, float height, Material m, Transform parent, bool solid = false)
        => Shape(name, PrimitiveType.Cylinder, p, new Vector3(radius * 2, height * 0.5f, radius * 2), m, parent, solid);

    private static void Ground()
    {
        var ground = Group("01_Pavement", Vector3.zero);
        Box("Plaza foundation", new Vector3(0, -0.5f, 0), new Vector3(42, 1, 42), _stone, ground, true);
        var tileA = Mat("Paving_Ochre", new Color(0.64f, 0.57f, 0.43f));
        var tileB = Mat("Paving_Sand", new Color(0.69f, 0.63f, 0.5f));
        for (int x = -10; x <= 10; x++) for (int z = -10; z <= 10; z++)
            Box("Stone paving", new Vector3(x * 1.9f, 0.012f, z * 1.9f), new Vector3(1.85f, 0.024f, 1.85f), (x + z) % 3 == 0 ? tileA : tileB, ground);
        Box("Central promenade", new Vector3(0, 0.04f, -5), new Vector3(4.8f, 0.06f, 29), _lightStone, ground);
        for (int s = -1; s <= 1; s += 2)
        {
            Box("Promenade brass edge", new Vector3(s * 2.3f, 0.08f, -5), new Vector3(0.08f, 0.02f, 29), _gold, ground);
            Box("East west walkway", new Vector3(s * 9, 0.04f, -2), new Vector3(14, 0.06f, 3), _lightStone, ground);
        }
        // Closed perimeter: accidental falls return safely, while the arrival zones remain separate.
        Box("West boundary", new Vector3(-21, 1, 0), new Vector3(0.8f, 2, 42), _stone, ground, true);
        Box("East boundary", new Vector3(21, 1, 0), new Vector3(0.8f, 2, 42), _stone, ground, true);
        Box("South boundary", new Vector3(0, 1, -21), new Vector3(42, 2, 0.8f), _stone, ground, true);
        Box("North boundary", new Vector3(0, 1, 21), new Vector3(42, 2, 0.8f), _stone, ground, true);
        for (int i = 0; i < 3; i++)
            Cylinder("Arrival inlay", new Vector3(0, 0.09f + i * 0.008f, -13), 2.5f - i * 0.25f, 0.02f, i % 2 == 0 ? _gold : _darkStone, ground);
    }

    private static void Architecture()
    {
        var district = Group("02_Architecture", Vector3.zero);
        Facade("North museum", new Vector3(0, 0, 23), 0, district);
        Facade("West cloister", new Vector3(-24, 0, 0), 90, district);
        Facade("East cloister", new Vector3(24, 0, 0), -90, district);
        // The north terrace replaces the elevated test block, retaining a navigable approach.
        Box("Terrace", new Vector3(0, 0.46f, 16.3f), new Vector3(11, 0.92f, 5.6f), _lightStone, district, true);
        for (int i = 0; i < 4; i++)
            Box("Terrace stair " + i, new Vector3(0, 0.12f * (i + 1), 11.5f + i * 0.55f), new Vector3(7, 0.24f * (i + 1), 0.6f), _stone, district, true);
        var door = Group("Northern archive entrance", new Vector3(0, 1, 20), district);
        Box("Cedar doors", new Vector3(0, 2.3f, 0), new Vector3(3.4f, 4.6f, 0.28f), _wood, door);
        for (int side = -1; side <= 1; side += 2)
        {
            Cylinder("Archive column", new Vector3(side * 2.35f, 2.1f, -0.7f), 0.33f, 4.2f, _lightStone, door);
            Box("Column capital", new Vector3(side * 2.35f, 4.2f, -0.7f), new Vector3(0.95f, 0.35f, 0.95f), _lightStone, door);
        }
        Box("Archive lintel", new Vector3(0, 4.65f, -0.5f), new Vector3(6, 0.65f, 1.3f), _lightStone, door);
        Label("CASA DE LA MEMORIA", new Vector3(0, 6.8f, 19.7f), 0.1f, district, _gold.color);
        var mountainMat = Mat("DistantAndes", new Color(0.26f, 0.4f, 0.37f));
        for (int i = 0; i < 14; i++)
        {
            var mountain = Shape("Andean ridge", PrimitiveType.Sphere, new Vector3(-80 + i * 12, 1 + i % 3 * 2, 65 + i % 2 * 10),
                new Vector3(35, 29 + i % 4 * 5, 24), mountainMat, district);
            mountain.transform.rotation = Quaternion.Euler(0, i * 25, 12);
        }
    }
    private static void Facade(string name, Vector3 p, float yaw, Transform parent)
    {
        var group = Group(name, p, parent); group.localRotation = Quaternion.Euler(0, yaw, 0);
        Box("Limestone facade", new Vector3(0, 4.6f, 0), new Vector3(42, 9.2f, 3), _lightStone, group, true);
        Box("Stone plinth", new Vector3(0, 0.7f, -1.62f), new Vector3(42.4f, 1.4f, 0.45f), _stone, group);
        foreach (float y in new[] { 1.45f, 5.1f, 8.9f, 9.3f })
            Box("Cornice", new Vector3(0, y, -1.65f), new Vector3(42.6f, 0.22f, 0.55f), _stone, group);
        Box("Terracotta roof", new Vector3(0, 9.65f, 0.25f), new Vector3(43, 0.55f, 4), _terra, group);
        for (int i = -5; i <= 5; i++)
        {
            float x = i * 3.65f;
            Box("Pilaster", new Vector3(x - 1.5f, 4.7f, -1.7f), new Vector3(0.48f, 7.4f, 0.4f), _stone, group);
            foreach (float y in new[] { 3.1f, 6.9f })
            {
                Box("Recessed window", new Vector3(x, y, -1.57f), new Vector3(1.4f, 2.2f, 0.1f), _darkStone, group);
                Box("Window sill", new Vector3(x, y - 1.17f, -1.83f), new Vector3(1.8f, 0.17f, 0.42f), _stone, group);
                Box("Window mullion", new Vector3(x, y, -1.67f), new Vector3(0.06f, 2.25f, 0.12f), _gold, group);
                Box("Window crossbar", new Vector3(x, y, -1.67f), new Vector3(1.4f, 0.06f, 0.12f), _gold, group);
            }
        }
    }

    private static void Gardens()
    {
        var gardens = Group("03_GardensAndFurniture", Vector3.zero);
        foreach (Vector3 p in new[] { new Vector3(-15, 0, -12), new Vector3(14, 0, -12), new Vector3(-16, 0, 4), new Vector3(5, 0, 12) })
        {
            Box("Garden curb", p + Vector3.up * 0.25f, new Vector3(5, 0.5f, 4), _stone, gardens, true);
            Box("Garden soil", p + Vector3.up * 0.53f, new Vector3(4.6f, 0.08f, 3.6f), _wood, gardens);
            Tree(p + new Vector3(0, 0.55f, 0), gardens);
            for (int i = 0; i < 7; i++)
                Shape("Shrub", PrimitiveType.Sphere, p + new Vector3(Mathf.Sin(i * 3) * 1.7f, 0.8f, Mathf.Cos(i * 3) * 1.2f), new Vector3(1.25f, 0.8f, 1.1f), _leaf, gardens);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = new Vector3(side * 4.2f, 0, -16 + i * 4);
                Cylinder("Lamp foot", p + Vector3.up * 0.2f, 0.32f, 0.4f, _darkStone, gardens, true);
                Cylinder("Lamp post", p + Vector3.up * 1.8f, 0.075f, 3.2f, _darkStone, gardens);
                Box("Lantern cap", p + Vector3.up * 3.8f, new Vector3(0.7f, 0.12f, 0.7f), _darkStone, gardens);
                Shape("Lantern", PrimitiveType.Sphere, p + Vector3.up * 3.45f, new Vector3(0.42f, 0.7f, 0.42f), Mat("LanternGlass", new Color(1, 0.71f, 0.28f), 0, 1.5f), gardens);
            }
            for (int i = 0; i < 2; i++)
            {
                var bench = Group("Cedar bench", new Vector3(side * 8.2f, 0, -12 + i * 4), gardens);
                for (int plank = 0; plank < 3; plank++)
                    Box("Seat slat", new Vector3(0, 0.65f, -0.3f + plank * 0.3f), new Vector3(3, 0.14f, 0.24f), _wood, bench);
                Box("Seat collision", new Vector3(0, 0.4f, 0), new Vector3(3, 0.8f, 0.9f), _darkStone, bench, true).GetComponent<Renderer>().enabled = false;
                for (int leg = -1; leg <= 1; leg += 2)
                    Box("Stone leg", new Vector3(leg, 0.28f, 0), new Vector3(0.3f, 0.56f, 0.85f), _stone, bench);
            }
        }
        Label("PLAZA NÚÑEZ", new Vector3(0, 3.6f, -16.7f), 0.18f, gardens, new Color(1, 0.9f, 0.61f));
        Label("APRENDE  ·  PROTEGE  ·  EXPLORA", new Vector3(0, 3.05f, -16.7f), 0.075f, gardens, Color.white);
    }
    private static void Tree(Vector3 p, Transform parent)
    {
        Cylinder("Laurel trunk", p + Vector3.up * 2, 0.22f, 4, _wood, parent);
        for (int i = 0; i < 5; i++)
        {
            float a = i * 2.4f;
            Shape("Laurel crown", PrimitiveType.Sphere, p + new Vector3(Mathf.Sin(a) * 1.1f, 4 + i % 2 * 1.1f, Mathf.Cos(a) * 0.9f),
                new Vector3(3.2f, 2.6f, 2.8f), i % 2 == 0 ? _leaf : Mat("SunlitLeaves", new Color(0.24f, 0.39f, 0.15f)), parent);
        }
    }

    private static void Fountain()
    {
        var g = Group("04_FountainOfMemory", new Vector3(0, 0, 1));
        Cylinder("Round stepped base", new Vector3(0, 0.12f, 0), 3.1f, 0.24f, _darkStone, g, true);
        Cylinder("Basin", new Vector3(0, 0.44f, 0), 2.65f, 0.65f, _stone, g, true);
        Cylinder("Water", new Vector3(0, 0.79f, 0), 2.38f, 0.03f, _water, g);
        Ring("Basin rim", 2.5f, 0.13f, _lightStone, g, new Vector3(0, 0.78f, 0), false);
        Cylinder("Fountain stem", new Vector3(0, 1.25f, 0), 0.38f, 1.7f, _darkStone, g);
        Cylinder("Upper bowl", new Vector3(0, 1.94f, 0), 1.15f, 0.23f, _gold, g);
        Cylinder("Upper water", new Vector3(0, 2.07f, 0), 1.06f, 0.02f, _water, g);
        Shape("Solar finial", PrimitiveType.Sphere, new Vector3(0, 2.62f, 0), new Vector3(0.7f, 0.7f, 0.7f), _gold, g);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6;
            Cylinder("Falling water", new Vector3(Mathf.Sin(a) * 0.99f, 1.42f, Mathf.Cos(a) * 0.99f), 0.028f, 1.12f, _water, g);
        }
    }

    private static AnalyzableObject[] Objects()
    {
        var g = Group("05_HandTutorialStations", Vector3.zero);
        Vector3[] points = { new Vector3(-7, 0, -6), new Vector3(-11, 0, 0), new Vector3(-7, 0, 6) };
        string[] names = { "Vasija del eco", "Disco del alba", "Guardián de jade" };
        string[] descriptions = { "Sigue las líneas que rodean la vasija.", "Observa su relieve desde otro ángulo.", "Dos movimientos; un momento de quietud." };
        string[] tasks = { "01  /  GIRAR", "02  /  INCLINAR", "03  /  DETENER" };
        var result = new AnalyzableObject[3];
        for (int i = 0; i < 3; i++)
        {
            var station = Group("Station_0" + (i + 1), points[i], g);
            Cylinder("Station mosaic", new Vector3(0, 0.08f, 0), 1.8f, 0.14f, _darkStone, station);
            Ring("Brass inlay", 1.7f, 0.05f, _gold, station, new Vector3(0, 0.17f, 0), false);
            Cylinder("Pedestal base", new Vector3(0, 0.23f, 0), 0.83f, 0.3f, _lightStone, station, true);
            Box("Carved pedestal", new Vector3(0, 0.77f, 0), new Vector3(0.96f, 0.83f, 0.96f), _darkStone, station, true);
            Cylinder("Gold table edge", new Vector3(0, 1.26f, 0), 0.71f, 0.16f, _gold, station);
            var item = Group("Artifact_0" + (i + 1), new Vector3(0, 2.05f, 0), station);
            var collider = item.gameObject.AddComponent<SphereCollider>(); collider.radius = 0.8f;
            result[i] = item.gameObject.AddComponent<AnalyzableObject>();
            if (i == 0) Vessel(item);
            else if (i == 1) Disc(item);
            else Idol(item);
            var data = AssetDatabase.LoadAssetAtPath<AnalyzableObjectData>("Assets/_Game/Data/PlazaNunez/Object_" + (i + 1) + ".asset");
            data.displayName = names[i]; data.description = descriptions[i];
            data.evidenceStatus = CulturalEvidenceStatus.ArtisticInterpretation;
            data.sourceOrValidationNote = "Pieza original de ficción creada para enseñar interacción. No es una reproducción arqueológica.";
            EditorUtility.SetDirty(data); Set(result[i], "data", data);
            Label(tasks[i], points[i] + Vector3.up * 3.2f, 0.105f, g, new Color(1, 0.87f, 0.48f));
        }
        return result;
    }
    private static void Vessel(Transform g)
    {
        Lathe("Terracotta vessel", new[] { new Vector2(0.25f, -0.62f), new Vector2(0.43f, -0.48f), new Vector2(0.59f, -0.13f), new Vector2(0.53f, 0.22f), new Vector2(0.25f, 0.4f), new Vector2(0.24f, 0.59f), new Vector2(0.32f, 0.65f) }, _terra, g);
        Ring("Lip", 0.3f, 0.045f, _gold, g, new Vector3(0, 0.64f, 0), false);
        Ring("Gold equator", 0.584f, 0.033f, _gold, g, new Vector3(0, -0.05f, 0), false);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6;
            var mark = Box("Inlaid chevron", new Vector3(Mathf.Sin(a) * 0.55f, 0.13f, Mathf.Cos(a) * 0.55f), new Vector3(0.09f, 0.16f, 0.025f), _gold, g);
            mark.transform.rotation = Quaternion.Euler(0, i * 30, 35);
        }
    }
    private static void Disc(Transform g)
    {
        var disc = Cylinder("Solar disk", Vector3.zero, 0.64f, 0.13f, _gold, g);
        disc.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Ring("Turquoise inner ring", 0.42f, 0.04f, _teal, g, new Vector3(0, 0, -0.1f), true);
        Shape("Central cabochon", PrimitiveType.Sphere, new Vector3(0, 0, -0.12f), new Vector3(0.36f, 0.36f, 0.17f), _teal, g);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6;
            var ray = Box("Sun ray", new Vector3(Mathf.Sin(a) * 0.74f, Mathf.Cos(a) * 0.74f, 0), new Vector3(0.065f, 0.23f, 0.07f), _gold, g);
            ray.transform.localRotation = Quaternion.Euler(0, 0, -i * 30);
        }
    }
    private static void Idol(Transform g)
    {
        Shape("Jade head", PrimitiveType.Sphere, new Vector3(0, 0.3f, 0), new Vector3(0.78f, 0.8f, 0.46f), _teal, g);
        Box("Carved body", new Vector3(0, -0.29f, 0), new Vector3(0.5f, 0.65f, 0.37f), _teal, g);
        for (int s = -1; s <= 1; s += 2)
        {
            Box("Gold eye", new Vector3(s * 0.18f, 0.36f, -0.205f), new Vector3(0.15f, 0.07f, 0.035f), _gold, g);
            Ring("Ear ornament", 0.15f, 0.035f, _gold, g, new Vector3(s * 0.46f, 0.25f, 0), true);
            Box("Foot", new Vector3(s * 0.2f, -0.67f, -0.05f), new Vector3(0.22f, 0.15f, 0.46f), _teal, g);
        }
        Box("Crown", new Vector3(0, 0.7f, 0), new Vector3(0.95f, 0.15f, 0.45f), _gold, g);
        Box("Mouth inlay", new Vector3(0, 0.11f, -0.23f), new Vector3(0.26f, 0.04f, 0.02f), _gold, g);
        Ring("Chest medallion", 0.15f, 0.03f, _gold, g, new Vector3(0, -0.25f, -0.21f), true);
    }

    private static PlazaCombatController Arena(TechnicalDemoController demo)
    {
        var g = Group("06_TrainingCircle", new Vector3(11, 0, 6));
        Cylinder("Arena floor", new Vector3(0, 0.06f, 0), 4.7f, 0.12f, _darkStone, g);
        Cylinder("Arena inner mosaic", new Vector3(0, 0.13f, 0), 4.3f, 0.025f, _stone, g);
        var ring = Ring("Defense signal", 4.4f, 0.075f, _teal, g, new Vector3(0, 0.16f, 0), false);
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI / 8;
            var dash = Box("Arena radial inlay", new Vector3(Mathf.Sin(a) * 3.9f, 0.16f, Mathf.Cos(a) * 3.9f), new Vector3(0.1f, 0.02f, 0.35f), _gold, g);
            dash.transform.localRotation = Quaternion.Euler(0, i * 22.5f, 0);
        }
        var guardian = Group("Training guardian", new Vector3(0, 0.17f, 2), g);
        guardian.localRotation = Quaternion.Euler(0, 180, 0);
        Box("Guardian torso", new Vector3(0, 1.55f, 0), new Vector3(1.05f, 1.3f, 0.65f), _darkStone, guardian);
        Shape("Guardian head", PrimitiveType.Sphere, new Vector3(0, 2.6f, 0), new Vector3(0.75f, 0.87f, 0.6f), _gold, guardian);
        Box("Eyes", new Vector3(0, 2.65f, 0.3f), new Vector3(0.5f, 0.065f, 0.05f), _teal, guardian);
        Box("Chest plate", new Vector3(0, 1.7f, 0.34f), new Vector3(0.7f, 0.6f, 0.07f), _gold, guardian);
        for (int s = -1; s <= 1; s += 2)
        {
            Cylinder("Leg", new Vector3(s * 0.3f, 0.55f, 0), 0.2f, 1.1f, _darkStone, guardian);
            Shape("Shoulder", PrimitiveType.Sphere, new Vector3(s * 0.72f, 2, 0), Vector3.one * 0.53f, _gold, guardian);
            Cylinder("Arm", new Vector3(s * 0.77f, 1.48f, 0), 0.14f, 0.85f, _darkStone, guardian);
            Box("Foot", new Vector3(s * 0.3f, 0.16f, 0.2f), new Vector3(0.4f, 0.3f, 0.72f), _gold, guardian);
        }
        Cylinder("Training staff", new Vector3(0.95f, 1.3f, 0.23f), 0.045f, 2.6f, _wood, guardian);
        Shape("Staff crystal", PrimitiveType.Sphere, new Vector3(0.95f, 2.7f, 0.23f), new Vector3(0.22f, 0.4f, 0.22f), _teal, guardian);
        var entry = Group("Training entry", new Vector3(0, 1.1f, -4.4f), g);
        var mark = Group("Player combat mark", new Vector3(0, 1.15f, -1.8f), g);
        Label("04  /  ENTRENAMIENTO", new Vector3(11, 3.9f, 8), 0.11f, _root, new Color(1, 0.88f, 0.53f));
        var combat = g.gameObject.AddComponent<PlazaCombatController>();
        Set(combat, "demo", demo); Set(combat, "entry", entry); Set(combat, "playerMark", mark);
        Set(combat, "guardian", guardian); Set(combat, "warningRing", ring.GetComponent<Renderer>());
        return combat;
    }

    private static void Gateways()
    {
        var g = Group("07_PortalsAndWorldThresholds", Vector3.zero);
        var lower = Mat("LowerPortal", new Color(0.3f, 0.2f, 0.7f), 0.5f, 1);
        var upper = Mat("UpperPortal", new Color(0.3f, 0.78f, 0.82f), 0.5f, 1);
        Transform lowerArrival = Threshold(-1, new Vector3(-85, 0, 0), lower, g);
        Transform upperArrival = Threshold(1, new Vector3(85, 8, 0), upper, g);
        var lowerReturn = Group("Lower return destination", new Vector3(-15, 1.12f, 9), g);
        var upperReturn = Group("Upper return destination", new Vector3(15, 1.12f, 10), g);
        Portal("Mundo inferior", new Vector3(-15, 0, 13), -1, lowerArrival, lower, true, g);
        Portal("Mundo superior", new Vector3(15, 0, 15), 1, upperArrival, upper, true, g);
        Portal("Plaza Núñez", new Vector3(-85, 0, -5), 0, lowerReturn, lower, false, g);
        Portal("Plaza Núñez", new Vector3(85, 8, -5), 0, upperReturn, upper, false, g);
    }
    private static Transform Threshold(int world, Vector3 p, Material accent, Transform parent)
    {
        var g = Group(world < 0 ? "MundoInferior_Umbral" : "MundoSuperior_Umbral", p, parent);
        Material floor = world < 0 ? _darkStone : _lightStone;
        Box("Threshold floor", new Vector3(0, -0.5f, 1), new Vector3(23, 1, 24), floor, g, true);
        for (int s = -1; s <= 1; s += 2)
        {
            Box("Threshold parapet", new Vector3(s * 11.2f, 0.7f, 1), new Vector3(0.7f, 1.4f, 24), floor, g, true);
            Box("Threshold end wall", new Vector3(0, 0.7f, 1 + s * 11.7f), new Vector3(23, 1.4f, 0.7f), floor, g, true);
            for (int i = 0; i < 4; i++)
            {
                float z = i * 4 - 4;
                if (world < 0)
                {
                    var crystal = Shape("Amethyst crystal", PrimitiveType.Cube, new Vector3(s * (7.4f + i % 2), 1.8f, z), new Vector3(1.3f, 3.5f + i % 2, 1.3f), accent, g);
                    crystal.transform.localRotation = Quaternion.Euler(8, 45, s * 12);
                }
                else
                {
                    Cylinder("Sky column", new Vector3(s * 8, 2.6f, z), 0.42f, 5.2f, _lightStone, g);
                    Cylinder("Column crown", new Vector3(s * 8, 5.3f, z), 0.67f, 0.23f, _gold, g);
                }
            }
        }
        for (int i = 0; i < 7; i++)
            Box("Wayfinding inlay", new Vector3(0, 0.04f, -2 + i * 1.5f), new Vector3(0.7f, 0.07f, 0.3f), accent, g);
        Cylinder("Destination altar", new Vector3(0, 0.38f, 8), 2.4f, 0.76f, floor, g, true);
        Ring("Destination aureole", 1.75f, 0.11f, accent, g, new Vector3(0, 2.4f, 8), true);
        Label(world < 0 ? "MUNDO INFERIOR" : "MUNDO SUPERIOR", p + new Vector3(0, 5.2f, 8), 0.15f, parent, world < 0 ? new Color(0.8f, 0.65f, 1) : new Color(1, 0.9f, 0.6f));
        Label("UMBRAL  /  REGRESA POR EL ARCO", p + new Vector3(0, 4.65f, 8), 0.065f, parent, Color.white);
        return Group("Arrival", new Vector3(0, 1.12f, 0), g);
    }
    private static void Portal(string name, Vector3 p, int world, Transform arrival, Material accent, bool locked, Transform parent)
    {
        var g = Group("Portal_" + name, p, parent);
        Cylinder("Gate mosaic", new Vector3(0, 0.06f, 0), 2.7f, 0.12f, _darkStone, g);
        for (int s = -1; s <= 1; s += 2)
        {
            Box("Gate pillar", new Vector3(s * 1.85f, 1.8f, 0), new Vector3(0.65f, 3.6f, 0.85f), _darkStone, g, true);
            Box("Pillar inlay", new Vector3(s * 1.85f, 1.9f, -0.46f), new Vector3(0.12f, 2.8f, 0.06f), accent, g);
            Box("Gate footing", new Vector3(s * 1.85f, 0.2f, 0), new Vector3(1, 0.4f, 1.1f), _gold, g);
        }
        Ring("Stone archivolt", 1.98f, 0.22f, _stone, g, new Vector3(0, 2.65f, 0), true);
        var halo = Ring("Orbiting gold halo", 1.64f, 0.055f, _gold, g, new Vector3(0, 2.65f, -0.15f), true);
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4;
            var rune = Box("Halo jewel", new Vector3(Mathf.Sin(a) * 1.64f, Mathf.Cos(a) * 1.64f, 0), Vector3.one * 0.12f, accent, halo.transform);
            rune.transform.localRotation = Quaternion.Euler(0, 0, 45);
        }
        var veil = Shape("Portal surface", PrimitiveType.Sphere, new Vector3(0, 2.5f, 0.06f), new Vector3(2.9f, 3.7f, 0.14f), accent, g);
        Ring("Inner light", 1.27f, 0.035f, accent, g, new Vector3(0, 2.65f, -0.15f), true);
        Label(name.ToUpperInvariant(), p + Vector3.up * 5.2f, 0.11f, parent, new Color(1, 0.87f, 0.58f));
        var portal = g.gameObject.AddComponent<PlazaPortal>();
        portal.destinationName = name; portal.destination = arrival; portal.world = world; portal.requiresTraining = locked;
        portal.halo = halo.transform; portal.veil = veil.GetComponent<Renderer>(); portal.color = accent.GetColor("_BaseColor");
        Portals.Add(portal);
    }

    private static void Lighting()
    {
        var light = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(x => x.type == LightType.Directional);
        if (light != null)
        {
            light.name = "Late afternoon sun"; light.color = new Color(1, 0.89f, 0.7f); light.intensity = 1.65f;
            light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(42, -35, 0);
        }
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.56f, 0.67f, 0.71f);
        RenderSettings.ambientEquatorColor = new Color(0.50f, 0.49f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.23f, 0.18f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 38; RenderSettings.fogEndDistance = 95;
        RenderSettings.fogColor = new Color(0.64f, 0.69f, 0.67f);
        Camera.main.backgroundColor = RenderSettings.fogColor;
        Camera.main.farClipPlane = 130;
        var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) cameraData = Camera.main.gameObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var volume = Group("08_ColorAndAtmosphere", Vector3.zero).gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = profile.Add<Bloom>(); bloom.intensity.Override(0.25f); bloom.threshold.Override(1.05f);
        var tone = profile.Add<Tonemapping>(); tone.mode.Override(TonemappingMode.ACES);
        AssetDatabase.CreateAsset(profile, Art + "/PlazaAtmosphere.asset");
        foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
        volume.sharedProfile = profile;
    }
    private static void Label(string text, Vector3 p, float size, Transform parent, Color color)
    {
        var g = Group("Sign_" + text, p, parent);
        var mesh = g.gameObject.AddComponent<TextMesh>(); mesh.text = text; mesh.characterSize = size;
        mesh.fontSize = 64; mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = color;
        g.gameObject.AddComponent<PlazaWorldLabel>();
    }
    private static GameObject Ring(string name, float radius, float tube, Material mat, Transform parent, Vector3 p, bool vertical)
    {
        const int segments = 64, sides = 8;
        var vertices = new Vector3[segments * sides]; var triangles = new int[segments * sides * 6];
        for (int i = 0; i < segments; i++) for (int j = 0; j < sides; j++)
        {
            float a = i * Mathf.PI * 2 / segments, b = j * Mathf.PI * 2 / sides;
            float r = radius + tube * Mathf.Cos(b);
            vertices[i * sides + j] = vertical ? new Vector3(r * Mathf.Sin(a), r * Mathf.Cos(a), tube * Mathf.Sin(b)) : new Vector3(r * Mathf.Sin(a), tube * Mathf.Sin(b), r * Mathf.Cos(a));
            int n = (i * sides + j) * 6, v = i * sides + j, v1 = i * sides + (j + 1) % sides;
            int v2 = ((i + 1) % segments) * sides + j, v3 = ((i + 1) % segments) * sides + (j + 1) % sides;
            triangles[n] = v; triangles[n + 1] = vertical ? v1 : v2; triangles[n + 2] = vertical ? v2 : v1;
            triangles[n + 3] = v1; triangles[n + 4] = vertical ? v3 : v2; triangles[n + 5] = vertical ? v2 : v3;
        }
        return MeshObject(name, vertices, triangles, mat, parent, p);
    }
    private static void Lathe(string name, Vector2[] profile, Material mat, Transform parent)
    {
        const int segments = 40;
        var vertices = new Vector3[segments * profile.Length]; var triangles = new List<int>();
        for (int i = 0; i < segments; i++) for (int j = 0; j < profile.Length; j++)
        {
            float a = i * 2 * Mathf.PI / segments;
            vertices[i * profile.Length + j] = new Vector3(Mathf.Sin(a) * profile[j].x, profile[j].y, Mathf.Cos(a) * profile[j].x);
            if (j == profile.Length - 1) continue;
            int v = i * profile.Length + j, n = ((i + 1) % segments) * profile.Length + j;
            triangles.AddRange(new[] { v, n, v + 1, v + 1, n, n + 1 });
        }
        MeshObject(name, vertices, triangles.ToArray(), mat, parent, Vector3.zero);
    }
    private static GameObject MeshObject(string name, Vector3[] vertices, int[] triangles, Material mat, Transform parent, Vector3 p)
    {
        var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Art + "/Meshes/" + (_meshIndex++).ToString("D3") + ".asset");
        var g = Group(name, p, parent).gameObject;
        g.AddComponent<MeshFilter>().sharedMesh = mesh; g.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return g;
    }
    private static void Set(Object target, string property, Object value)
    {
        var s = new SerializedObject(target); s.FindProperty(property).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Set(Object target, string property, bool value)
    {
        var s = new SerializedObject(target); s.FindProperty(property).boolValue = value; s.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void SetArray<T>(Object target, string property, T[] values) where T : Object
    {
        var s = new SerializedObject(target); var p = s.FindProperty(property); p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        s.ApplyModifiedPropertiesWithoutUndo();
    }
}

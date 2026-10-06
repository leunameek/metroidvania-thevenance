using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the Mundo Superior blockout (Guía completa del mundo superior, phase A plus the playable
// systems): eight zones, the Yopo 2 branch and the far islands at the guide's global coordinates,
// every catalog piece (T01..C06) as a grey block with the catalog size and pivot, stairs with ramp
// colliders, the climbable tower, docks and transport, portals, altars, rests, the lock, the arena
// and its guardian, zone volumes, markers and the sky.
//
// Model swap: when Assets/_Game/Art/Environments/MundoSuperior/Models/**/MS_<ID>*.fbx exists (an *_Optimizado.fbx is
// preferred) it is placed inside the same catalog box and pivot and the grey block hides; the
// collider always stays the grey one. Drop a model in and run the menu again.
// The scene is regenerated from scratch on each run: change this builder, not the scene.
[InitializeOnLoad]
public static class MundoSuperiorBuilder
{
    public const string ScenePath = WorldTravel.UpperWorldScene;
    public const string KitRoot = "Assets/_Game/Art/Environments/MundoSuperior/Models";
    private const string KitMaterialFolder = "Assets/_Game/Art/Environments/MundoSuperior/Materials/Kit";
    private const string MaterialFolder = "Assets/_Game/Art/Environments/MundoSuperior/Materials";
    private const string TextureFolder = MaterialFolder + "/Textures";
    private const string PlayerVisual = "Assets/_Game/Art/Characters/Nemequene/Nemequene_Player_Visual.prefab";
    // Enemy base model of the project (hub training guardian) until the summit guardian exists.
    private const string EnemyModel = "Assets/_Game/Art/Environments/PlazaNunez/Models/Guardian+de+entrenamiento/tripo_convert_4d40c251-6ffc-4813-a701-a4ed6a33f78b.fbx";
    private const string EnemyController = "Assets/_Game/Art/Environments/PlazaNunez/Models/Guardian+de+entrenamiento/Guardian_Entrenamiento.controller";
    private const string AutoRunKey = "MundoSuperior.Blockout.v1";
    // Rigged equipped wings (MSWingRig); without it the O03 "Equipables" export is used.
    private const string WingsModelFolder = "Assets/_Game/Art/Equipment/Alas";
    private const int IgnoreRaycast = 2;

    private enum Pivot { Top, Bottom, Center, Back, HingeLeft, StairEdge }

    private sealed class Spec { public string Name; public Vector3 Size; public Pivot Pivot; public int Expected; public Material Top, Side; }

    private static readonly Dictionary<string, Spec> Catalog = new Dictionary<string, Spec>();
    private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>();
    private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    private static readonly List<Vector3> Route = new List<Vector3>();
    private static Dictionary<string, string> _kit;
    // Models delivered already light and with their own materials (e.g. E04/E04.fbx).
    private static readonly HashSet<string> KeepMaterials = new HashSet<string>();
    private static Material _sand, _ochre, _dark, _metal, _wood, _ceramic, _plant, _cloth, _cloud, _pink, _blue, _grey, _core, _marker, _grip, _far;
    private static Transform _labels;
    // Cross-zone references wired after every zone exists.
    private static MSPortal _p1, _p2, _p3, _p4;
    private static Transform _entry, _baseSafe;
    private static readonly Transform[] TestSpawns = new Transform[8];

    static MundoSuperiorBuilder()
    {
        if (Application.isBatchMode || EditorPrefs.GetBool(Key)) return;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) TryAutoRun(); };
        EditorApplication.delayCall += TryAutoRun;
    }
    private static string Key => AutoRunKey + ":" + Application.dataPath;
    private static void TryAutoRun()
    {
        if (EditorPrefs.GetBool(Key) || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        EditorPrefs.SetBool(Key, true);
        Run();
    }

    [MenuItem("Nemequene/Mundo Superior/Construir nivel")]
    public static void Run()
    {
        if (!Application.isBatchMode && File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Reconstruir el mundo superior",
                "Esto genera la escena desde cero y descarta los ajustes hechos a mano en ella (escaleras, bandas...).\n\n" +
                "Para nubes o modelos nuevos usa «Nemequene > Mundo Superior > Actualizar nubes y modelos».", "Reconstruir", "Cancelar")) return;
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var open = Enumerable.Range(0, EditorSceneManager.sceneCount).Select(i => EditorSceneManager.GetSceneAt(i).path)
            .Where(p => !string.IsNullOrEmpty(p)).ToArray();
        try
        {
            Build();
            Debug.Log("MS_BLOCKOUT_OK: " + ScenePath);
        }
        catch (Exception e) { Debug.LogError("MS_BLOCKOUT_FAILED: " + e); }
        finally
        {
            if (!Application.isBatchMode && open.Length > 0 && open[0] != ScenePath)
            {
                EditorSceneManager.OpenScene(open[0], OpenSceneMode.Single);
                for (int i = 1; i < open.Length; i++) EditorSceneManager.OpenScene(open[i], OpenSceneMode.Additive);
            }
        }
    }

    private static void Build()
    {
        Catalog.Clear(); Counts.Clear(); Meshes.Clear(); Route.Clear(); _kit = null; _jars = 0; KeepMaterials.Clear();
        _p1 = _p2 = _p3 = _p4 = null; _entry = _baseSafe = null; Array.Clear(TestSpawns, 0, TestSpawns.Length);
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        Directory.CreateDirectory(TextureFolder);
        Directory.CreateDirectory(KitMaterialFolder);
        AssetDatabase.Refresh();
        Materials();
        DefineCatalog();
        MIParticles.SharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Resources/MIParticulas.mat");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var world = new GameObject("MundoSuperior").transform;
        var zones = Group(world, "Zonas");
        var mechanisms = Group(world, "Mechanisms");
        var environment = Group(world, "Environment");
        _labels = Group(world, "Etiquetas (F12)");

        // Zone roots at the centre of their main surface (guide 2.2); presets of guide 13.1.
        Umbral(Zone(zones, 0, "MS01_Umbral", "Umbral", new Vector3(0, 0, 0), Vector3.up * 2.5f, new Vector3(14, 6, 14), 0, 30, 9));
        PrimeraRuna(Zone(zones, 1, "MS02_RunaPortales", "Primera runa", new Vector3(0, 4, 19), new Vector3(2, 2.5f, 0), new Vector3(18, 6, 14), 25, 35, 10));
        RunaEscalada(Zone(zones, 2, "MS03_RunaEscalada", "Runa de escalada", new Vector3(38, 6, 19), Vector3.up * 2.5f, new Vector3(14, 6, 14), 0, 32, 10));
        Alas(Zone(zones, 3, "MS04_Alas", "Alas", new Vector3(0, 24, 19), new Vector3(2, 2.5f, 0), new Vector3(18, 6, 14), 90, 30, 11));
        RamalYopo(Zone(zones, 8, "RamalYopo2", "Isla del yopo", new Vector3(-29, 26, 22), Vector3.up * 2.5f, new Vector3(5, 6, 5), 270, 35, 9));
        PortalAzul(Zone(zones, 4, "MS05_PortalAzul", "Isla del portal azul", new Vector3(34, 28, 19), Vector3.up * 2.5f, new Vector3(8, 6, 8), 90, 35, 10));
        CaminoLlave(Group(zones, "MS06_CaminoLlave", new Vector3(60, 42, 19)));
        Antesala(Zone(zones, 6, "MS07_Antesala", "Antesala", new Vector3(132, 64, 68), new Vector3(0, 2.5f, -2), new Vector3(14, 6, 18), 0, 30, 10));
        Cima(Zone(zones, 7, "MS08_Cima", "Cima", new Vector3(132, 70, 96.5f), Vector3.up * 2.5f, new Vector3(26, 6, 26), 0, 25, 14));
        Wall(mechanisms);
        Transport(mechanisms);
        WirePortals();

        Sky(environment);
        FarIslands(environment);
        Clouds(environment, Route);
        Player(world);
        _labels.gameObject.SetActive(false);
        ReportInventory();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        // The plaza portal loads it by path (WorldTravel), which needs it in the build list.
        if (!EditorBuildSettings.scenes.Any(entry => entry.path == ScenePath))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) }).ToArray();
    }

    // ---------------------------------------------------------------- catalog (guide 9.2)

    private static void DefineCatalog()
    {
        void Add(string id, string name, float x, float y, float z, Pivot pivot, int expected, Material side, Material top = null)
            => Catalog[id] = new Spec { Name = name, Size = new Vector3(x, y, z), Pivot = pivot, Expected = expected, Side = side, Top = top ?? side };
        Add("T01", "Losa de terraza", 6, 1.2f, 6, Pivot.Top, 45, _ochre, _sand);
        Add("T02", "Anexo / isla pequeña", 4, 1.2f, 4, Pivot.Top, 3, _ochre, _sand);
        Add("T03", "Cuerpo de pilar", 4, 8, 4, Pivot.Top, 25, _ochre);
        Add("T04", "Remate corto de pilar", 4, 4, 4, Pivot.Top, 11, _ochre);
        Add("T05", "Escalera de diez peldaños", 3.5f, 2, 3.5f, Pivot.StairEdge, 5, _ochre, _sand);
        Add("T06", "Banda de borde", 6, 1.2f, .3f, Pivot.Top, 84, _dark, _ochre);
        Add("T07", "Remate inferior roto", 4, 2, 4, Pivot.Top, 20, _ochre);
        Add("T08", "Panel de muro", 4, 4, .6f, Pivot.Bottom, 19, _ochre, _sand);
        Add("T09", "Roca decorativa", 4, 3, 4, Pivot.Bottom, 12, _ochre);
        Add("A01", "Marco de portal", 3, 3.8f, .6f, Pivot.Bottom, 6, _ochre, _metal);
        Add("A02", "Base de portal", 4, .3f, 3, Pivot.Top, 6, _ochre, _sand);
        Add("A03", "Marco del cierre de llave", 4.4f, 4, .8f, Pivot.Bottom, 1, _ochre, _metal);
        Add("A04", "Hoja del cierre", 3.2f, 3, .4f, Pivot.HingeLeft, 1, _dark, _ochre);
        Add("A05", "Pretil", 3, 1.1f, .4f, Pivot.Bottom, 54, _ochre, _sand);
        Add("A06", "Apoyo de escalada", 1.2f, .3f, .6f, Pivot.Back, 20, _ochre, _grip);
        Add("A07", "Muelle", 4, .8f, 4, Pivot.Top, 2, _wood, _sand);
        Add("A08", "Plataforma móvil", 4, .6f, 6, Pivot.Top, 1, _metal, _sand);
        Add("O01", "Runa de portales", .35f, .35f, .06f, Pivot.Center, 1, _metal);
        Add("O02", "Runa de escalada", .35f, .35f, .06f, Pivot.Center, 1, _metal);
        Add("O03", "Par de alas", .65f, .4f, .2f, Pivot.Center, 1, _metal);
        Add("O04", "Yopo 1", .25f, .35f, .25f, Pivot.Bottom, 1, _ceramic);
        Add("O05", "Yopo 2", .25f, .35f, .25f, Pivot.Bottom, 1, _ceramic);
        Add("O06", "Llave medallón", .28f, .28f, .05f, Pivot.Center, 1, _metal);
        Add("O07", "Pedestal de hallazgo", 1.5f, .9f, 1.2f, Pivot.Bottom, 4, _ochre, _sand);
        Add("O08", "Soporte intercambiable", .4f, .2f, .4f, Pivot.Bottom, 4, _metal);
        Add("O09", "Disco de descanso", 2, .15f, 2, Pivot.Bottom, 3, _sand);
        Add("O10", "Receptáculo de llave", .45f, .7f, .2f, Pivot.Back, 1, _metal);
        Add("E01", "Vasija decorativa", .35f, .5f, .35f, Pivot.Bottom, 18, _ceramic);
        Add("E02", "Paño tejido", .9f, 2, .03f, Pivot.Top, 14, _cloth);
        Add("E03", "Poste de abrigo circular", .2f, 3, .2f, Pivot.Bottom, 8, _wood);
        Add("E04", "Cubierta vegetal circular", 8, 3, 8, Pivot.Bottom, 1, _plant);
        Add("E05", "Panel curvo del abrigo", 1.86f, 2.4f, .15f, Pivot.Bottom, 6, _wood);
        Add("E06", "Grupo de vegetación baja", 1, .6f, 1, Pivot.Bottom, 24, _plant);
        Add("E07", "Enredadera colgante", .4f, 4, .3f, Pivot.Top, 28, _plant);
        Add("E08", "Isla lejana", 8, 20, 8, Pivot.Top, 6, _far, _sand);
        Add("C01", "Torso del guardián", 3.5f, 3, 2, Pivot.Bottom, 1, _ochre, _metal);
        Add("C02", "Cabeza del guardián", 1.3f, 1.2f, 1.2f, Pivot.Bottom, 1, _ochre, _metal);
        Add("C03", "Brazo del guardián", 1, 2.6f, 1, Pivot.Top, 2, _ochre, _metal);
        Add("C04", "Pierna del guardián", 1, 2.2f, 1, Pivot.Top, 2, _ochre);
        Add("C05", "Núcleo del guardián", .65f, .65f, .65f, Pivot.Center, 1, _core);
        Add("C06", "Fragmento de piedra", .7f, .6f, .7f, Pivot.Center, 6, _ochre);
    }

    private static Vector3 Anchor(Pivot pivot) => pivot switch
    {
        Pivot.Top => new Vector3(.5f, 1f, .5f),
        Pivot.Bottom => new Vector3(.5f, 0f, .5f),
        Pivot.Back => new Vector3(.5f, .5f, 0f),
        Pivot.HingeLeft => new Vector3(0f, 0f, .5f),
        Pivot.StairEdge => new Vector3(.5f, 0f, 0f),
        _ => new Vector3(.5f, .5f, .5f),
    };

    // One catalog instance: a root on the pivot, the grey block (collider) and, when the model
    // exists, the model fitted into the same box.
    private static Transform PieceCore(string id, Transform parent, string label, Vector3 local, float yaw, bool collider, float scale, bool count, string variant)
    {
        var spec = Catalog[id];
        var root = new GameObject("MS_" + id + " " + label).transform;
        root.SetParent(parent, false);
        root.localPosition = local; root.localRotation = Quaternion.Euler(0, yaw, 0);
        if (count) Counts[id] = (Counts.TryGetValue(id, out int n) ? n : 0) + 1;
        Vector3 size = spec.Size * scale;
        Vector3 center = Vector3.Scale(Vector3.one * .5f - Anchor(spec.Pivot), size);
        var grey = Group(root, "Gris");
        switch (id)
        {
            case "T05": Stairs(grey, size, spec); break;
            case "A01": Frame(grey, size, 2f, 3f, spec, collider); break;
            case "A03": Frame(grey, size, 3.2f, 3f, spec, collider); break;
            case "O09": Cylinder(grey, "Disco", center, size, spec.Top, false); break;
            case "E01": case "O04": case "O05": case "E03":
                Cylinder(grey, "Cuerpo", center, size, spec.Side, collider); break;
            case "O01": case "O02": case "O06":
                Cylinder(grey, "Medallón", center, new Vector3(size.x, size.z, size.y), spec.Side, false).localRotation = Quaternion.Euler(90, 0, 0); break;
            case "C05": Sphere(grey, "Núcleo", center, size, spec.Side); break;
            case "E06": Sphere(grey, "Mata", center, size, spec.Side); break;
            case "E04":
                Cylinder(grey, "Faldón", new Vector3(0, .5f, 0), new Vector3(size.x, 1f, size.z), spec.Side, false);
                Cylinder(grey, "Cuerpo", new Vector3(0, 1.5f, 0), new Vector3(size.x * .72f, 1f, size.z * .72f), spec.Side, false);
                Cylinder(grey, "Cumbrera", new Vector3(0, 2.5f, 0), new Vector3(size.x * .38f, 1f, size.z * .38f), spec.Side, false);
                break;
            case "O03":
                Block(grey, "Ala izquierda", new Vector3(-.17f, 0, 0), new Vector3(.3f, .4f, .06f), spec.Side, false).transform.localRotation = Quaternion.Euler(0, 0, 12);
                Block(grey, "Ala derecha", new Vector3(.17f, 0, 0), new Vector3(.3f, .4f, .06f), spec.Side, false).transform.localRotation = Quaternion.Euler(0, 0, -12);
                Block(grey, "Unión", Vector3.zero, new Vector3(.08f, .2f, .2f), _dark, false);
                break;
            default: Block(grey, "Bloque", center, size, spec.Side, collider, spec.Top); break;
        }
        Model(id, variant, root, grey, size, spec.Pivot);
        return root;
    }

    // Ten 0.2 x 0.35 m steps for the eye, one smooth ramp for the CharacterController (guide T05).
    private static void Stairs(Transform grey, Vector3 size, Spec spec)
    {
        for (int k = 0; k < 10; k++)
        {
            float h = .2f * (k + 1);
            Block(grey, "Peldaño " + (k + 1), new Vector3(0, h * .5f, .35f * k + .175f), new Vector3(size.x, h, .35f), spec.Side, false, spec.Top);
        }
        float length = Mathf.Sqrt(size.y * size.y + size.z * size.z), angle = Mathf.Atan2(size.y, size.z) * Mathf.Rad2Deg;
        var ramp = new GameObject("Rampa (colisión)").transform; ramp.SetParent(grey, false);
        ramp.localRotation = Quaternion.Euler(-angle, 0, 0);
        ramp.localPosition = new Vector3(0, size.y * .5f, size.z * .5f) - ramp.localRotation * Vector3.up * .15f;
        ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(size.x, .3f, length + .1f);
    }

    // Portal and lock frames: two jambs and a lintel around an empty passage (no box over the hole).
    private static void Frame(Transform grey, Vector3 size, float holeWidth, float holeHeight, Spec spec, bool collider)
    {
        float jamb = (size.x - holeWidth) * .5f;
        foreach (int side in new[] { -1, 1 })
            Block(grey, side < 0 ? "Jamba izquierda" : "Jamba derecha", new Vector3(side * (holeWidth + jamb) * .5f, size.y * .5f, 0), new Vector3(jamb, size.y, size.z), spec.Side, collider, spec.Top);
        Block(grey, "Dintel", new Vector3(0, (holeHeight + size.y) * .5f, 0), new Vector3(size.x, size.y - holeHeight, size.z), spec.Side, collider, spec.Top);
    }

    // ---------------------------------------------------------------- zones (guide 5)

    private static Transform Zone(Transform parent, int index, string name, string title, Vector3 root, Vector3 volumeCenter, Vector3 volumeSize, float yaw, float pitch, float distance)
    {
        var t = Group(parent, name, root);
        var box = t.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true; box.center = volumeCenter; box.size = volumeSize;
        var zone = t.gameObject.AddComponent<MSZone>();
        var so = new SerializedObject(zone);
        so.FindProperty("zoneIndex").intValue = index; so.FindProperty("title").stringValue = title;
        so.FindProperty("yaw").floatValue = yaw; so.FindProperty("pitch").floatValue = pitch; so.FindProperty("distance").floatValue = distance;
        so.ApplyModifiedPropertiesWithoutUndo();
        Route.Add(root);
        Label(t, (index == 8 ? "Ramal" : (index + 1).ToString("00")) + " · " + title, new Vector3(0, 7, 0), yaw);
        return t;
    }

    private static void Umbral(Transform z)
    {
        var spawn = Mark(z, "Spawn de entrada · Recovery 01", new Vector3(0, 0, -2), 0);
        _entry = spawn; TestSpawns[0] = spawn;
        Floor(z, "Suelo 01", spawn, 6, 6);
        Bands(z, 6, 6);
        Column(z, -2, 0, -1.2f, "T03", "T07"); Column(z, 2, 0, -1.2f, "T03", "T07");
        Portal(z, "Retorno inicial", new Vector3(-3, 0, -2), 90, 0, _grey, new Color(.85f, .85f, .8f), "", "", "Plaza Núñez", toPlaza: true);
        Rest(z, "D01", MSProgress.Rest01, new Vector3(3, 0, -1));
        Mark(z, "Mirador 01", new Vector3(0, 0, 3), 0);
        Mark(z, "Inicio de escalera", new Vector3(0, 0, 6), 0);
        Rail(z, -4.5f, -5.8f, 0); Rail(z, 4.5f, -5.8f, 0);
        Rail(z, -5.8f, 1.5f, 90); Rail(z, -5.8f, 4.5f, 90); Rail(z, 5.8f, 1.5f, 90); Rail(z, 5.8f, 4.5f, 90);
        Decor("E01", z, new Vector3(-5f, 0, 5.2f), 20); Decor("E01", z, new Vector3(5f, 0, 5.2f), 200);
        Decor("E02", z, new Vector3(0, -.05f, -6.33f), 180);
        Decor("E06", z, new Vector3(-5f, 0, -5f), 40); Decor("E06", z, new Vector3(5.1f, 0, -5f), 130);
        Decor("E07", z, new Vector3(6.2f, -1.2f, -3), 90); Decor("E07", z, new Vector3(-6.2f, -1.2f, 3), 270);
    }

    private static void PrimeraRuna(Transform z)
    {
        var recovery = Mark(z, "Recovery 02", new Vector3(0, 0, -3), 0); TestSpawns[1] = recovery;
        Floor(z, "Suelo 02", recovery, 6, 6);
        Bands(z, 6, 6);
        Column(z, -3, -3, -1.2f, "T03", "T07"); Column(z, 3, -3, -1.2f, "T03", "T07");
        // Stairs 01 -> 02, counted in 02 (guide 5.2): pivots (0;0;6) and (0;2;9.5) global.
        Piece("T05", z, "Escalera 01-02 baja", new Vector3(0, -4, -13), 0, true);
        Piece("T05", z, "Escalera 01-02 alta", new Vector3(0, -2, -9.5f), 0, true);
        // Base annex of the climb in the free strip Z 19-21 (its own safe support: the wall base).
        _baseSafe = Mark(z, "Base segura de la pared", new Vector3(8, 0, 2), 270);
        SafeGround(z, "Anexo de escalada", _baseSafe).Also(g => Piece("T02", g, "Anexo base de escalada", new Vector3(8, 0, 2), 0, true));
        Altar(z, "Altar Runa 1", new Vector3(-2, 0, -1), 180, "O01", MSProgress.RunePortals, "Runa de portales", "Hallazgo",
            "Medallón de piedra con el relieve de un paso. Pieza de fantasía creada para el juego: al confirmarla se encienden los portales de este mundo.",
            "Runa de portales", "La runa activa los portales. Acércate al portal rosado y pulsa E.");
        GroundFind(z, "O04", "Yopo 1", new Vector3(-4, 0, -3), 150, MSProgress.Yopo1, "Yopo",
            "Vasija pequeña, marcador provisional de hallazgo. Mejora de ficción del juego, no una práctica histórica: suma 5 al ataque contra el guardián.",
            "Yopo · mejora opcional", "+5 de daño de ataque en el combate de la cima.");
        _p1 = Portal(z, "P1 rosado", new Vector3(3, 0, -1), 180, 2, _pink, new Color(.85f, .28f, .77f), MSProgress.RunePortals,
            "La runa de portales activa este paso", "Runa de escalada");
        Mark(z, "Llegada desde P2", new Vector3(3, 0, -4), 180).name = "ArrivalFeet P1";
        Rail(z, -4.5f, -5.8f, 0); Rail(z, 4.5f, -5.8f, 0);
        Rail(z, -5.8f, -4.5f, 90); Rail(z, -5.8f, -1.5f, 90); Rail(z, 5.8f, -4.5f, 90); Rail(z, 5.8f, -1.5f, 90);
        // Cladding on the south face of the tower of 04, which stands on the back strip of 02.
        Piece("T08", z, "Revestimiento de la torre", new Vector3(-4, 0, 1.7f), 180, layer: true);
        Piece("T08", z, "Revestimiento de la torre", new Vector3(0, 0, 1.7f), 180, layer: true);
        Piece("T08", z, "Muro lateral", new Vector3(-5.7f, 0, 2), 90, layer: true);
        Decor("T09", z, new Vector3(-8.2f, -2.2f, -4), 30, .9f); Decor("T09", z, new Vector3(8.2f, -2.2f, -5), 200, .8f);
        Decor("E01", z, new Vector3(-5f, 0, -5f), 0); Decor("E01", z, new Vector3(5f, 0, -5f), 90);
        Decor("E02", z, new Vector3(-3, -.05f, -6.33f), 180); Decor("E02", z, new Vector3(3, -.05f, -6.33f), 180);
        Decor("E06", z, new Vector3(-4.8f, 0, 1), 0); Decor("E06", z, new Vector3(1, 0, 1.2f), 70); Decor("E06", z, new Vector3(-3f, 0, -5f), 150);
        Decor("E07", z, new Vector3(-6.2f, -1.2f, -3), 270); Decor("E07", z, new Vector3(-6.2f, -1.2f, 3), 270);
        Decor("E07", z, new Vector3(6.2f, -1.2f, -4), 90); Decor("E07", z, new Vector3(2, -1.2f, -6.2f), 180);
    }

    private static void RunaEscalada(Transform z)
    {
        var recovery = Mark(z, "Recovery 03", new Vector3(0, 0, -2), 40); TestSpawns[2] = recovery;
        Floor(z, "Suelo 03", recovery, 6, 6);
        Bands(z, 6, 6);
        Column(z, -2, 0, -1.2f, "T03", "T07"); Column(z, 2, 0, -1.2f, "T03", "T07");
        _p2 = Portal(z, "P2 rosado", new Vector3(-2, 0, -1), 180, 2, _pink, new Color(.85f, .28f, .77f), MSProgress.RunePortals,
            "La runa de portales activa este paso", "Primera runa");
        // Spawn P2 looks at the altar (guide 5.3).
        Mark(z, "ArrivalFeet P2", new Vector3(-2, 0, -4), Mathf.Atan2(4, 5) * Mathf.Rad2Deg);
        Altar(z, "Altar Runa 2", new Vector3(2, 0, 1), Mathf.Atan2(-4, -5) * Mathf.Rad2Deg, "O02", MSProgress.RuneClimb, "Runa de escalada", "Hallazgo",
            "Medallón con el relieve de escalones y apoyos. Pieza de fantasía creada para el juego: permite subir por las paredes con apoyos.",
            "Runa de escalada", "Permite subir por las paredes con apoyos. Vuelve por el portal rosado a la primera terraza.");
        Mark(z, "Mirador hacia la pared", new Vector3(-4, 0, 3), 270);
        for (int i = 0; i < 4; i++) Rail(z, 5.8f, -4.5f + 3 * i, 90);
        Piece("T08", z, "Fondo del altar", new Vector3(0, 0, 5.7f), 180, layer: true);
        Piece("T08", z, "Fondo del altar", new Vector3(4, 0, 5.7f), 180, layer: true);
        Decor("T09", z, new Vector3(8.2f, -2.2f, -4), 120);
        Decor("E01", z, new Vector3(3.6f, 0, 3.4f), 0); Decor("E01", z, new Vector3(.8f, 0, 3.6f), 60);
        Decor("E02", z, new Vector3(0, 3.4f, 5.37f), 180);
        Decor("E06", z, new Vector3(-5f, 0, -5f), 0); Decor("E06", z, new Vector3(4.8f, 0, -5f), 90);
        Decor("E07", z, new Vector3(-6.2f, -1.2f, 0), 270); Decor("E07", z, new Vector3(6.2f, -1.2f, -2), 90);
    }

    private static void Alas(Transform z)
    {
        var recovery = Mark(z, "Recovery 04", new Vector3(0, 0, -2), 90); TestSpawns[3] = recovery;
        Floor(z, "Suelo 04", recovery, 6, 6);
        Bands(z, 6, 6);
        // Launch annex (guide 5.5). Moved from local (8;0;2) to (8;0;-2): at (8;0;2) it would hang
        // right over the climbing lane (X 6.8, Z 22) and the climber would hit its underside.
        SafeGround(z, "Anexo de lanzamiento", recovery).Also(g => Piece("T02", g, "Anexo de lanzamiento", new Vector3(8, 0, -2), 0, true));
        Mark(z, "Punto de ensayo · mira a 05", new Vector3(8, 0, -2), 90);
        // Tower of 04 over the back strip of 02: X -6..6, Y 4..24, Z 21..25, three columns.
        foreach (float x in new[] { -4f, 0f, 4f })
        {
            Piece("T04", z, "Torre · remate", new Vector3(x, 0, 4), 0, true);
            Piece("T03", z, "Torre · cuerpo alto", new Vector3(x, -4, 4), 0, true);
            Piece("T03", z, "Torre · cuerpo bajo", new Vector3(x, -12, 4), 0, true);
        }
        Piece("T07", z, "Remate colgante", new Vector3(-3, -1.2f, -4), 0); Piece("T07", z, "Remate colgante", new Vector3(3, -1.2f, -4), 90);
        for (int i = 0; i < 20; i++) Piece("A06", z, "Apoyo " + (i + 1).ToString("00"), new Vector3(6, -19.5f + i, 3), 90);
        Altar(z, "Altar de alas", new Vector3(-2, 0, 0), 90, "O03", MSProgress.Wings, "Alas", "Hallazgo",
            "Par de alas estilizadas de metal cálido. Pieza de fantasía creada para el juego: permiten un vuelo corto y controlado entre islas.",
            "Alas", "F · Abrir alas   ·   Espacio / Ctrl · Subir o bajar.\nSe recargan al aterrizar. Vuela desde el anexo hacia la isla del portal azul.");
        Rest(z, "D04", MSProgress.Rest04, new Vector3(-3, 0, -3));
        Mark(z, "Despegue opcional · mira a Yopo 2", new Vector3(-4, 0, 3), 270);
        Mark(z, "Retorno desde Yopo 2", new Vector3(-4, 0, 3), 90);
        for (int i = 0; i < 4; i++) Rail(z, -4.5f + 3 * i, -5.8f, 0);
        foreach (float x in new[] { -4f, 0f, 4f }) Piece("T08", z, "Fondo de la terraza", new Vector3(x, 0, 5.7f), 180, layer: true);
        Decor("T09", z, new Vector3(-5, -2.2f, 8.2f), 10); Decor("T09", z, new Vector3(5, -2.2f, 8.2f), 250, .85f);
        Decor("E01", z, new Vector3(-5.2f, 0, 4.6f), 0); Decor("E01", z, new Vector3(1.8f, 0, 4.6f), 40);
        Decor("E02", z, new Vector3(-4, 3.4f, 5.37f), 180); Decor("E02", z, new Vector3(0, 3.4f, 5.37f), 180);
        Decor("E06", z, new Vector3(-5f, 0, -5f), 0); Decor("E06", z, new Vector3(5f, 0, -4.7f), 60); Decor("E06", z, new Vector3(-5.3f, 0, -1), 120);
        Decor("E07", z, new Vector3(-3, -1.2f, -6.2f), 180); Decor("E07", z, new Vector3(3, -1.2f, -6.2f), 180);
        Decor("E07", z, new Vector3(-6.2f, -1.2f, -3), 270); Decor("E07", z, new Vector3(-6.2f, -1.2f, 2), 270);
    }

    private static void RamalYopo(Transform z)
    {
        var recovery = Mark(z, "Llegada / Recovery Yopo 2", new Vector3(0, 0, -.5f), 90);
        SafeGround(z, "Suelo Yopo 2", recovery).Also(g => Piece("T02", g, "Isla de Yopo 2", Vector3.zero, 0, true));
        GroundFind(z, "O05", "Yopo 2", new Vector3(0, 0, 1), 200, MSProgress.Yopo2, "Segundo yopo",
            "Segunda vasija, variante del primer marcador. Mejora de ficción del juego, no una práctica histórica: suma otros 5 al ataque contra el guardián.",
            "Segundo yopo · mejora opcional", "+5 de daño de ataque en el combate de la cima.");
    }

    private static void PortalAzul(Transform z)
    {
        var recovery = Mark(z, "Recovery 05 · llegada del vuelo", new Vector3(-1.5f, 0, -1.6f), 90); TestSpawns[4] = recovery;
        Floor(z, "Suelo 05", recovery, 3, 3);
        Bands(z, 3, 3);
        Column(z, 0, 0, -1.2f, "T03", "T07");
        _p3 = Portal(z, "P3 azul", new Vector3(0, 0, 1), 180, 3, _blue, new Color(.21f, .48f, 1f), MSProgress.RunePortals,
            "La runa de portales activa este paso", "Camino de la llave");
        Mark(z, "ArrivalFeet P3", new Vector3(0, 0, -1.5f), 270);
        Decor("E01", z, new Vector3(2.5f, 0, 2.5f), 0);
        Decor("E06", z, new Vector3(2.4f, 0, -2.4f), 0);
        Decor("E07", z, new Vector3(3.2f, -1.2f, 0), 90); Decor("E07", z, new Vector3(0, -1.2f, 3.2f), 0);
    }

    // 06: one container (root at Inicio P4) with five surfaces and five framing subzones.
    private static void CaminoLlave(Transform c)
    {
        Route.Add(c.position);
        Label(c, "06 · Camino de la llave", new Vector3(49, 18, 0), 90);
        var inicio = Zone(c, 5, "InicioP4", "Camino de la llave", c.position, Vector3.up * 2.5f, new Vector3(8, 6, 8), 90, 30, 12);
        var a = Zone(c, 5, "ApoyoA", "Camino de la llave", c.position + new Vector3(25, 4, 0), Vector3.up * 2.5f, new Vector3(8, 6, 8), 90, 30, 12);
        var b = Zone(c, 5, "ApoyoB", "Camino de la llave", c.position + new Vector3(50, 8, 6), Vector3.up * 2.5f, new Vector3(8, 6, 8), 90, 30, 12);
        var cc = Zone(c, 5, "ApoyoC", "Camino de la llave", c.position + new Vector3(75, 12, 0), Vector3.up * 2.5f, new Vector3(8, 6, 8), 90, 30, 12);
        var llave = Zone(c, 5, "TerrazaLlave", "Terraza de la llave", c.position + new Vector3(98, 16, 0), new Vector3(0, 2.5f, 2), new Vector3(14, 6, 18), 0, 35, 11);

        var spawnP4 = Mark(inicio, "ArrivalFeet P4 · Recovery", new Vector3(0, 0, -1.5f), 90); TestSpawns[5] = spawnP4;
        Floor(inicio, "Suelo Inicio P4", spawnP4, 3, 3); Bands(inicio, 3, 3); Column(inicio, 0, 0, -1.2f, "T03", "T07");
        _p4 = Portal(inicio, "P4 azul", new Vector3(0, 0, 1), 180, 3, _blue, new Color(.21f, .48f, 1f), MSProgress.RunePortals,
            "La runa de portales activa este paso", "Isla del portal azul");
        Decor("T09", inicio, new Vector3(-5.2f, -2.2f, 0), 60);
        Decor("E06", inicio, new Vector3(2.4f, 0, -2.4f), 0); Decor("E07", inicio, new Vector3(0, -1.2f, -3.2f), 180);
        foreach (var (t, name) in new[] { (a, "A"), (b, "B"), (cc, "C") })
        {
            var r = Mark(t, "Recovery apoyo " + name, Vector3.zero, 90);
            Floor(t, "Suelo apoyo " + name, r, 3, 3); Bands(t, 3, 3); Column(t, 0, 0, -1.2f, "T03", "T07");
            Decor("E06", t, new Vector3(2.2f, 0, 2.2f), 30); Decor("E07", t, new Vector3(0, -1.2f, -3.2f), 180);
        }

        var recovery = Mark(llave, "Recovery Llave", new Vector3(-2, 0, -2), 0);
        Floor(llave, "Suelo Llave", recovery, 6, 6); Bands(llave, 6, 6);
        Column(llave, -3, 0, -1.2f, "T03", "T07"); Column(llave, 3, 0, -1.2f, "T03");
        foreach (var (x, zz) in new[] { (-4f, -4f), (4f, -4f), (-4f, 4f), (4f, 4f) }) Piece("T04", llave, "Remate de esquina", new Vector3(x, -1.2f, zz), 0);
        Altar(llave, "Altar de la llave", new Vector3(0, 0, 1), 270, "O06", MSProgress.Key, "Llave medallón", "Hallazgo",
            "Medallón circular con una muesca de orientación. Pieza de fantasía creada para el juego: abre el cierre de la antesala de la cima.",
            "Llave medallón", "Lleva el medallón al cierre de la cima. El transporte espera junto al muelle.");
        // Departure dock: pivot (158;58;27), edge Z 25-29, its own safe support.
        var dockSafe = Mark(llave, "DockSafeFeet 06", new Vector3(0, 0, 7), 0);
        SafeGround(llave, "Muelle de salida", dockSafe).Also(g => Piece("A07", g, "Muelle de salida", new Vector3(0, 0, 8), 0, true));
        Rail(llave, -1.5f, -5.8f, 0); Rail(llave, 1.5f, -5.8f, 0); Rail(llave, -4.5f, 5.8f, 0); Rail(llave, 4.5f, 5.8f, 0);
        foreach (float zz in new[] { -4f, 0f, 4f }) Piece("T08", llave, "Fondo este", new Vector3(5.7f, 0, zz), 270, layer: true);
        Decor("T09", llave, new Vector3(8.2f, -2.2f, -4), 300);
        Decor("E01", llave, new Vector3(-4.6f, 0, -4.6f), 0); Decor("E01", llave, new Vector3(4.6f, 0, -4.6f), 80); Decor("E01", llave, new Vector3(-4.6f, 0, 4.2f), 160);
        Decor("E02", llave, new Vector3(5.37f, 3.4f, -4), 270); Decor("E02", llave, new Vector3(5.37f, 3.4f, 4), 270);
        Decor("E06", llave, new Vector3(4.6f, 0, -1.8f), 0); Decor("E06", llave, new Vector3(4.6f, 0, 1.8f), 90);
        Decor("E07", llave, new Vector3(-3, -1.2f, -6.2f), 180); Decor("E07", llave, new Vector3(3, -1.2f, -6.2f), 180);
    }

    private static void Antesala(Transform z)
    {
        var recovery = Mark(z, "Recovery 07 · derrota del jefe", new Vector3(0, 0, -2), 0);
        Mark(z, "Llegada segura", new Vector3(0, 0, -3), 0);
        Floor(z, "Suelo 07", recovery, 6, 6); Bands(z, 6, 6);
        Column(z, -2, 0, -1.2f, "T03", "T07"); Column(z, 2, 0, -1.2f, "T03", "T07");
        var dockSafe = Mark(z, "DockSafeFeet 07", new Vector3(0, 0, -7.5f), 0); TestSpawns[6] = dockSafe;
        SafeGround(z, "Muelle de llegada", dockSafe).Also(g => Piece("A07", g, "Muelle de llegada", new Vector3(0, 0, -8), 0, true));
        Rest(z, "D07", MSProgress.Rest07, new Vector3(-3, 0, -1));
        Lock(z);
        for (int i = 0; i < 3; i++) { Rail(z, -5.8f, -4.5f + 3 * i, 90); Rail(z, 5.8f, -4.5f + 3 * i, 90); }
        Piece("T08", z, "Muro del cierre", new Vector3(-4.2f, 0, 5.7f), 180, layer: true);
        Piece("T08", z, "Muro del cierre", new Vector3(4.2f, 0, 5.7f), 180, layer: true);
        Decor("T09", z, new Vector3(-8.2f, -2.2f, 0), 70); Decor("T09", z, new Vector3(8.2f, -2.2f, 3), 160, .9f);
        Decor("E01", z, new Vector3(-5f, 0, -5f), 0); Decor("E01", z, new Vector3(5f, 0, -5f), 120);
        Decor("E02", z, new Vector3(-4.2f, 3.4f, 5.37f), 180); Decor("E02", z, new Vector3(4.2f, 3.4f, 5.37f), 180);
        Decor("E06", z, new Vector3(-4.8f, 0, 4.5f), 0); Decor("E06", z, new Vector3(4.8f, 0, 4.5f), 60);
        Decor("E07", z, new Vector3(-6.2f, -1.2f, -2), 270); Decor("E07", z, new Vector3(6.2f, -1.2f, -2), 90);
    }

    // A03 frame across the exit to the stairs, A04 leaf on a hinge 1.6 m left of the passage,
    // O10 on the right jamb with the inserted medallion, E from the floor in front (guide 5.10).
    private static void Lock(Transform z)
    {
        var root = Group(z, "CierreLlave07", z.position + new Vector3(0, 0, 6));
        Piece("A03", root, "Marco del cierre", Vector3.zero, 0, true, layer: true);
        var hinge = Group(root, "Bisagra", root.position + new Vector3(-1.6f, 0, 0));
        var leaf = Piece("A04", hinge, "Hoja del cierre", Vector3.zero, 0, true);
        Dynamic(hinge);
        foreach (var t in leaf.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = IgnoreRaycast;
        var receptacle = Piece("O10", z, "Receptáculo", new Vector3(2.1f, 1.2f, 5.6f), 180, layer: true);
        var cavity = Cylinder(receptacle, "Cavidad circular", new Vector3(0, 0, .21f), new Vector3(.3f, .02f, .3f), _dark, false);
        cavity.localRotation = Quaternion.Euler(90, 0, 0);
        // Derived visual instance of O06 (guide 10.2): not a new reference nor a second find.
        var placed = Piece("O06", receptacle, "Medallón insertado", new Vector3(0, 0, .23f), 0, count: false);
        Dynamic(placed);
        placed.gameObject.SetActive(false);
        var point = Group(z, "Punto de interacción del receptáculo", z.position + new Vector3(2, 0, 3.5f));
        var keyLock = point.gameObject.AddComponent<MSKeyLock>();
        var so = new SerializedObject(keyLock);
        so.FindProperty("hinge").objectReferenceValue = hinge; so.FindProperty("placedMedallion").objectReferenceValue = placed.gameObject;
        so.FindProperty("displayName").stringValue = "Receptáculo"; so.FindProperty("range").floatValue = 2.2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        AddLight(receptacle, "Luz del receptáculo", new Vector3(0, .4f, .8f), new Color(1f, .8f, .5f), 3.5f, 1.2f);
    }

    private static void Cima(Transform z)
    {
        var observe = Mark(z, "Punto de observación · Recovery 08", new Vector3(0, 0, -8), 0); TestSpawns[7] = observe;
        Floor(z, "Suelo 08 · arena", observe, 12, 12); Bands(z, 12, 12);
        foreach (var (x, zz) in new[] { (-6f, -6f), (6f, -6f), (-6f, 6f), (6f, 6f) }) Column(z, x, zz, -1.2f, "T03", "T07");
        foreach (var (x, zz) in new[] { (-10f, -10f), (10f, -10f), (-10f, 10f), (10f, 10f) }) Piece("T04", z, "Remate de esquina", new Vector3(x, -1.2f, zz), 0);
        // Stairs 07 -> 08, counted in 08: pivots (132;64;74), (132;66;77.5), (132;68;81).
        Piece("T05", z, "Escalera 07-08 baja", new Vector3(0, -6, -22.5f), 0, true);
        Piece("T05", z, "Escalera 07-08 media", new Vector3(0, -4, -19), 0, true);
        Piece("T05", z, "Escalera 07-08 alta", new Vector3(0, -2, -15.5f), 0, true);
        Mark(z, "Acceso de escalera", new Vector3(0, 0, -12), 0);
        Portal(z, "Salida de la cima", new Vector3(8, 0, 0), Mathf.Atan2(-8, -5) * Mathf.Rad2Deg, 0, _grey, new Color(.9f, .85f, .7f),
            MSProgress.Guardian, "La salida se activará al vencer al guardián", "Plaza Núñez", toPlaza: true);
        Mark(z, "Retorno de victoria (suelo libre)", new Vector3(5.5f, 0, -1.6f), Mathf.Atan2(8, 5) * Mathf.Rad2Deg);
        Guardian(z);
        Abrigo(z);
        // Perimeter: 24 railings (south with the stair gap, east and west, two framing the access).
        foreach (float x in new[] { -10.5f, -7.5f, -4.5f, 4.5f, 7.5f, 10.5f }) Rail(z, x, -11.8f, 0);
        foreach (float zz in new[] { -10.5f, -7.5f, -4.5f, -1.5f, 1.5f, 4.5f, 7.5f, 10.5f }) { Rail(z, -11.8f, zz, 90); Rail(z, 11.8f, zz, 90); }
        Rail(z, -3.25f, -10.6f, 90); Rail(z, 3.25f, -10.6f, 90);
        foreach (float x in new[] { -10f, -6f, -2f, 2f, 6f, 10f }) Piece("T08", z, "Muro de fondo", new Vector3(x, 0, 11.7f), 180, true, layer: true);
        Decor("T09", z, new Vector3(-9, 0, -9), 20, .8f, true); Decor("T09", z, new Vector3(9, 0, 8.6f), 140, .8f, true); Decor("T09", z, new Vector3(-9, 0, 8.6f), 260, .8f, true);
        Decor("E01", z, new Vector3(-10.8f, 0, -5.5f), 0); Decor("E01", z, new Vector3(10.8f, 0, -5.5f), 90);
        Decor("E01", z, new Vector3(-6f, 0, 10.8f), 180); Decor("E01", z, new Vector3(6f, 0, 10.8f), 270);
        foreach (float x in new[] { -10f, -6f, 6f, 10f }) Decor("E02", z, new Vector3(x, 3.4f, 11.37f), 180);
        Decor("E06", z, new Vector3(-10.8f, 0, 4), 0); Decor("E06", z, new Vector3(10.8f, 0, 4), 50); Decor("E06", z, new Vector3(-5f, 0, -10.5f), 100);
        Decor("E06", z, new Vector3(5f, 0, -10.5f), 150); Decor("E06", z, new Vector3(-6.5f, 0, 7f), 200);
        foreach (var (x, zz, yaw) in new[] { (-12.2f, -6f, 270f), (-12.2f, 6f, 270f), (12.2f, -6f, 90f), (12.2f, 6f, 90f), (-6f, 12.2f, 0f), (6f, 12.2f, 0f) })
            Decor("E07", z, new Vector3(x, -1.2f, zz), yaw);
    }

    // Guardian (guide 7 and 11.4): legs from the hips, torso from the pelvis, head on the neck,
    // arms from the shoulders (±1.75 m, Y 4.6), the core on the chest; six fragments in reserve.
    private static void Guardian(Transform z)
    {
        var root = Group(z, "Guardián de la cima", z.position + new Vector3(0, 0, 1));
        root.rotation = Quaternion.Euler(0, 180, 0);
        Label(z, "Guardián de la cima (C01-C06)", new Vector3(0, 8, 1), 0);
        var body = Group(root, "Cuerpo (pelvis)", root.TransformPoint(new Vector3(0, 2.2f, 0)));
        Piece("C04", root, "Pierna izquierda", new Vector3(-.8f, 2.2f, 0), 0, true);
        Piece("C04", root, "Pierna derecha", new Vector3(.8f, 2.2f, 0), 0, true);
        Piece("C01", body, "Torso", Vector3.zero, 0, true);
        Piece("C02", body, "Cabeza", new Vector3(0, 3, 0), 0);
        var leftShoulder = Group(body, "Hombro izquierdo", body.TransformPoint(new Vector3(-2.25f, 2.4f, 0)));
        var rightShoulder = Group(body, "Hombro derecho", body.TransformPoint(new Vector3(2.25f, 2.4f, 0)));
        Piece("C03", leftShoulder, "Brazo izquierdo", Vector3.zero, 0);
        Piece("C03", rightShoulder, "Brazo derecho", Vector3.zero, 0);
        var core = Piece("C05", body, "Núcleo", new Vector3(0, 1.7f, 1.15f), 0);
        AddLight(core, "Luz del núcleo", new Vector3(0, 0, .6f), new Color(1f, .7f, .35f), 6, 2f);
        var pool = Group(root, "Reserva de fragmentos", root.position);
        var fragments = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            var f = Piece("C06", pool, "Fragmento " + (i + 1), new Vector3(-1.5f + .6f * i, .35f, -3), i * 50, scale: .8f + .05f * i);
            f.gameObject.SetActive(false); fragments.Add(f);
        }
        var animator = StandIn(root, body, core);
        Dynamic(root);

        var mark = Mark(z, "PlayerMark", new Vector3(0, 0, -5), 0);
        Mark(z, "GuardianHome", new Vector3(0, 0, 1), 180);
        var start = Group(z, "Inicio del encuentro (IntroMark)", z.position + new Vector3(0, 0, -6));
        var guardian = start.gameObject.AddComponent<MSGuardian>();
        var so = new SerializedObject(guardian);
        so.FindProperty("body").objectReferenceValue = body;
        so.FindProperty("leftArm").objectReferenceValue = leftShoulder; so.FindProperty("rightArm").objectReferenceValue = rightShoulder;
        so.FindProperty("playerMark").objectReferenceValue = mark;
        so.FindProperty("core").objectReferenceValue = core.GetComponentInChildren<Renderer>();
        var list = so.FindProperty("fragments"); list.arraySize = fragments.Count;
        for (int i = 0; i < fragments.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = fragments[i];
        so.FindProperty("displayName").stringValue = "Guardián"; so.FindProperty("range").floatValue = 2f;
        so.FindProperty("animator").objectReferenceValue = animator;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Provisional guardian (user's rule: missing characters use the hub's enemy base model):
    // the training guardian, Humanoid with its Hit/Attack/Die/Reset controller, scaled to the
    // 6 m silhouette. The grey C01-C04 hide but keep their colliders; the C05 core stays on the
    // chest as the reading of every warning.
    private static Animator StandIn(Transform root, Transform body, Transform core)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyModel);
        if (prefab == null) { Debug.LogWarning("MS_BLOCKOUT: falta " + EnemyModel + "; el guardián queda en gris."); return null; }
        var holder = Group(root, "Modelo provisional · guardián de entrenamiento", root.position);
        holder.localRotation = Quaternion.identity;
        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);
        model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity; model.transform.localScale = Vector3.one;
        var b = LocalBounds(holder, model);
        if (b.size.y > .01f) model.transform.localScale = Vector3.one * (6.2f / b.size.y);
        b = LocalBounds(holder, model);
        model.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
        var animator = model.GetComponentInChildren<Animator>();
        if (animator == null) animator = model.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(EnemyController);
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        foreach (var r in body.GetComponentsInChildren<Renderer>(true)) if (!r.transform.IsChildOf(core)) r.enabled = false;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            if (r.transform.parent != null && r.transform.parent.name == "Gris" && r.transform.parent.parent.name.StartsWith("MS_C04")) r.enabled = false;
        // The core floats in front of the model's chest (about 2/3 of its height).
        core.position = root.TransformPoint(new Vector3(0, b.size.y * .62f, Mathf.Max(.9f, b.size.z * .55f)));
        foreach (var t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = IgnoreRaycast;
        return animator;
    }

    // Circular shelter behind the guardian: 8 posts and 6 curved panels on radius 3.6 around
    // local (0;0;8), the front panel with its entrance gap, and the round roof at Y 3.
    private static void Abrigo(Transform z)
    {
        var root = Group(z, "Abrigo circular", z.position + new Vector3(0, 0, 8));
        for (int i = 0; i < 8; i++)
        {
            float a = (22.5f + 45f * i) * Mathf.Deg2Rad;
            Piece("E03", root, "Poste " + (i + 1), new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * 3.6f, 0, true, layer: true);
        }
        for (int i = 0; i < 6; i++)
        {
            float mid = 60f * i; // 180 faces the arena (-Z): that panel keeps the entrance open
            var arc = Group(root, "MS_E05 Panel curvo " + (i + 1) + (Mathf.Approximately(mid, 180) ? " (entrada)" : ""), root.position);
            arc.localRotation = Quaternion.Euler(0, mid, 0);
            Counts["E05"] = (Counts.TryGetValue("E05", out int n) ? n : 0) + 1;
            var offsets = Mathf.Approximately(mid, 180) ? new[] { -22.5f, 22.5f } : new[] { -15f, 15f };
            float span = Mathf.Approximately(mid, 180) ? 15f : 30f;
            foreach (float o in offsets)
            {
                float rad = o * Mathf.Deg2Rad;
                var chord = Block(arc, "Tramo", new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * 3.6f + Vector3.up * 1.2f,
                    new Vector3(2 * 3.6f * Mathf.Sin(span * .5f * Mathf.Deg2Rad), 2.4f, .15f), _wood, true);
                chord.transform.localRotation = Quaternion.Euler(0, o, 0);
                chord.layer = IgnoreRaycast;
            }
            if (ModelInto("E05", Mathf.Approximately(mid, 180) ? "Entrada" : null, arc, new Vector3(0, 0, 3.6f)))
                foreach (Transform chord in arc) if (chord.name == "Tramo") chord.GetComponent<Renderer>().enabled = false;
        }
        Piece("E04", root, "Cubierta vegetal", new Vector3(0, 3, 0), 0);
    }

    // ---------------------------------------------------------------- mechanisms (guide 6)

    // East face of the tower (X = 6), lane centred on Z = 22 from Y 4 to 24 (guide 5.4).
    private static void Wall(Transform parent)
    {
        var root = Group(parent, "Pared02_04", new Vector3(6, 4, 22));
        root.rotation = Quaternion.Euler(0, 90, 0);
        var bottom = Group(parent, "Enganche (pies)", new Vector3(6.8f, 4, 22));
        var top = Group(parent, "Límite superior del carril (pies)", new Vector3(6.8f, 23.7f, 22));
        var exit = Group(parent, "Salida superior (pies)", new Vector3(4.8f, 24, 22));
        bottom.SetParent(root, true); top.SetParent(root, true); exit.SetParent(root, true);
        Group(root, "Referencia intermedia 1", new Vector3(6.8f, 10, 22)); Group(root, "Referencia intermedia 2", new Vector3(6.8f, 17, 22));
        var wall = root.gameObject.AddComponent<MSClimbWall>();
        var so = new SerializedObject(wall);
        so.FindProperty("bottom").objectReferenceValue = bottom; so.FindProperty("top").objectReferenceValue = top;
        so.FindProperty("exit").objectReferenceValue = exit; so.FindProperty("baseSafe").objectReferenceValue = _baseSafe;
        so.ApplyModifiedPropertiesWithoutUndo();
        foreach (var (pos, fromTop) in new[] { (new Vector3(6.8f, 4, 22), false), (new Vector3(4.8f, 24, 22), true) })
        {
            var access = Group(root, fromTop ? "Acceso superior (descender)" : "Acceso inferior (escalar)", pos);
            var a = access.gameObject.AddComponent<MSClimbAccess>();
            var aso = new SerializedObject(a);
            aso.FindProperty("wall").objectReferenceValue = wall; aso.FindProperty("fromTop").boolValue = fromTop;
            aso.FindProperty("displayName").stringValue = "Pared"; aso.FindProperty("range").floatValue = 1.6f;
            aso.ApplyModifiedPropertiesWithoutUndo();
        }
        // Painted guide strip on the face so the lane reads from 02 (not a collider).
        var strip = Block(root, "Franja del carril", new Vector3(0, 10, .02f), new Vector3(1.6f, 20, .04f), _grip, false);
        strip.layer = IgnoreRaycast;
        Label(root, "Pared escalable 02→04 (A06 ×20)", new Vector3(0, 21, 2), 270);
    }

    // A08 between (158;58;32) and (132;64;55), top centre, 3 m/s, 4 s waits (guide 5.9).
    private static void Transport(Transform parent)
    {
        var start = new Vector3(158, 58, 32); var end = new Vector3(132, 64, 55);
        var platform = Piece("A08", parent, "Transporte06_07", start, 0, true);
        platform.gameObject.AddComponent<MSSafeGround>(); // recharges the wings; not a recovery point
        var transport = platform.gameObject.AddComponent<MSTransport>();
        var so = new SerializedObject(transport);
        so.FindProperty("start").vector3Value = start; so.FindProperty("end").vector3Value = end;
        so.ApplyModifiedPropertiesWithoutUndo();
        // Low edges on the long sides only, so both short ends stay open for boarding.
        foreach (int side in new[] { -1, 1 })
            Block(platform, "Borde bajo", new Vector3(side * 1.9f, .15f, 0), new Vector3(.2f, .3f, 5.6f), _metal, false).isStatic = false;
        Group(parent, "Extremo de ruta 06", start); Group(parent, "Extremo de ruta 07", end);
        Dynamic(platform);
        Label(platform, "A08 Transporte", new Vector3(0, 3, 0), 312);
        Route.Add(start); Route.Add(end); Route.Add((start + end) * .5f);
    }

    private static void WirePortals()
    {
        void Pair(MSPortal from, MSPortal to)
        {
            var so = new SerializedObject(from);
            so.FindProperty("pair").objectReferenceValue = to;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        // Arrival markers are siblings named "ArrivalFeet <portal>" in the destination zone.
        void Arrival(MSPortal portal, string marker)
        {
            var t = portal.transform.parent.Find(marker);
            var so = new SerializedObject(portal);
            so.FindProperty("arrival").objectReferenceValue = t;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        Pair(_p1, _p2); Pair(_p2, _p1); Pair(_p3, _p4); Pair(_p4, _p3);
        Arrival(_p1, "ArrivalFeet P1"); Arrival(_p2, "ArrivalFeet P2"); Arrival(_p3, "ArrivalFeet P3"); Arrival(_p4, "ArrivalFeet P4 · Recovery");
    }

    // ---------------------------------------------------------------- assemblies (guide 11.3)

    // T01 slabs under one safe-support group; half extents 3 (one slab), 6 (four) or 12 (sixteen).
    private static void Floor(Transform zone, string name, Transform recovery, float hx, float hz)
    {
        var g = SafeGround(zone, name, recovery);
        int k = Mathf.Abs(Mathf.RoundToInt(zone.position.x * 7 + zone.position.z * 3));
        for (float x = -hx + 3; x < hx; x += 6)
            for (float z = -hz + 3; z < hz; z += 6, k++)
            {
                // Broken edges only on the perimeter; cracks anywhere; a quarter turn for variety.
                bool edge = Mathf.Abs(x) + 3 >= hx || Mathf.Abs(z) + 3 >= hz;
                string variant = k % 5 == 1 && edge ? "BordeRoto" : k % 4 == 2 ? "Grieta" : null;
                Piece("T01", g, "Losa", new Vector3(x, 0, z), 90 * (k % 4), true, variant: variant);
            }
    }

    private static Transform SafeGround(Transform zone, string name, Transform recovery)
    {
        var g = Group(zone, name, zone.position);
        var safe = g.gameObject.AddComponent<MSSafeGround>();
        var so = new SerializedObject(safe); so.FindProperty("recovery").objectReferenceValue = recovery; so.ApplyModifiedPropertiesWithoutUndo();
        return g;
    }

    // T06 bands on the outer perimeter, one per 6 m, decorated face outwards.
    private static void Bands(Transform zone, float hx, float hz)
    {
        var g = Group(zone, "Bandas de borde", zone.position);
        for (float x = -hx + 3; x < hx; x += 6)
        {
            Piece("T06", g, "Banda norte", new Vector3(x, 0, hz + .15f), 0);
            Piece("T06", g, "Banda sur", new Vector3(x, 0, -hz - .15f), 180);
        }
        for (float z = -hz + 3; z < hz; z += 6)
        {
            Piece("T06", g, "Banda este", new Vector3(hx + .15f, 0, z), 90);
            Piece("T06", g, "Banda oeste", new Vector3(-hx - .15f, 0, z), 270);
        }
    }

    // Support under a surface: pieces stacked downwards from `top` (visual only, no collider).
    private static void Column(Transform zone, float x, float z, float top, params string[] ids)
    {
        float y = top;
        foreach (var id in ids)
        {
            Piece(id, zone, "Soporte", new Vector3(x, y, z), 0);
            y -= Catalog[id].Size.y;
        }
    }

    private static void Rail(Transform zone, float x, float z, float yaw) => Piece("A05", zone, "Pretil", new Vector3(x, 0, z), yaw, true, layer: true);

    private static Transform Piece(string id, Transform parent, string label, Vector3 local, float yaw, bool collider = false, float scale = 1f, bool count = true, bool layer = false, string variant = null)
    {
        var t = PieceCore(id, parent, label, local, yaw, collider, scale, count, variant);
        if (layer) foreach (var c in t.GetComponentsInChildren<Transform>(true)) c.gameObject.layer = IgnoreRaycast;
        return t;
    }

    // Decoration: never interactive, never a safe support, no collider unless asked.
    private static int _jars;
    private static void Decor(string id, Transform zone, Vector3 local, float yaw, float scale = 1f, bool collider = false)
    {
        string variant = id == "E01" ? new[] { null, "Baja", "Rota" }[_jars++ % 3] : null;
        Piece(id, zone, Catalog[id].Name, local, yaw, collider, scale, layer: true, variant: variant);
    }

    // O07 pedestal + O08 support + the find as an independent child (guide 4.3 and 11.4).
    private static void Altar(Transform zone, string name, Vector3 local, float yaw, string itemId, string findId, string displayName, string kindLabel,
        string description, string rewardTitle, string rewardText)
    {
        var root = Group(zone, name, zone.TransformPoint(local));
        root.localRotation = Quaternion.Euler(0, yaw, 0);
        Piece("O07", root, "Pedestal", Vector3.zero, 0, true, layer: true);
        Piece("O08", root, "Soporte", new Vector3(0, .9f, 0), 0, layer: true,
            variant: itemId == "O03" ? "Alas" : itemId == "O06" ? "Medallon" : null);
        var pickup = Group(root, "Visual_Item (pieza independiente)", root.TransformPoint(new Vector3(0, 1.33f, 0)));
        Piece(itemId, pickup, Catalog[itemId].Name, Vector3.zero, 0, layer: true);
        pickup.gameObject.AddComponent<MIFloat>();
        Dynamic(pickup);
        Group(root, "SafeApproach", root.TransformPoint(new Vector3(0, 0, 1.6f)));
        Find(root, pickup, findId, displayName, kindLabel, description, rewardTitle, rewardText, 2.4f);
        Label(zone, name + " · O07+O08+" + itemId, local + Vector3.up * 3, 0);
    }

    // Yopo on the floor with its integrated support (guide 4.3: no extra pedestal).
    private static void GroundFind(Transform zone, string itemId, string name, Vector3 local, float yaw, string findId, string displayName,
        string description, string rewardTitle, string rewardText)
    {
        var root = Group(zone, name, zone.TransformPoint(local));
        root.localRotation = Quaternion.Euler(0, yaw, 0);
        var piece = Piece(itemId, root, Catalog[itemId].Name, Vector3.zero, 0, layer: true);
        Dynamic(piece);
        Find(root, piece, findId, displayName, "Mejora opcional", description, rewardTitle, rewardText, 1.4f);
    }

    private static void Find(Transform root, Transform item, string findId, string displayName, string kindLabel, string description,
        string rewardTitle, string rewardText, float haloHeight)
    {
        var halo = AddLight(root, "Halo del hallazgo", new Vector3(0, haloHeight, 0), new Color(1f, .8f, .45f), 5, 2f);
        halo.gameObject.AddComponent<MIGlow>();
        var sparks = MIParticles.Sparks(root, item.localPosition + Vector3.up * .1f, new Color(1f, .85f, .5f, .8f));
        var find = root.gameObject.AddComponent<MSFind>();
        var so = new SerializedObject(find);
        so.FindProperty("findId").stringValue = findId; so.FindProperty("item").objectReferenceValue = item;
        so.FindProperty("halo").objectReferenceValue = halo; so.FindProperty("sparks").objectReferenceValue = sparks;
        so.FindProperty("displayName").stringValue = displayName; so.FindProperty("kindLabel").stringValue = kindLabel;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("rewardTitle").stringValue = rewardTitle; so.FindProperty("rewardText").stringValue = rewardText;
        so.FindProperty("range").floatValue = 2.2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Rest(Transform zone, string id, int zoneIndex, Vector3 local)
    {
        var root = Group(zone, "Descanso " + id, zone.TransformPoint(local));
        Piece("O09", root, "Disco de descanso", Vector3.zero, 0, layer: true);
        var spawn = Mark(root, "CheckpointFeet " + id, new Vector3(0, 0, -1.6f), 0);
        var glow = AddLight(root, "Luz del descanso", new Vector3(0, 1.4f, 0), new Color(1f, .86f, .55f), 4.5f, 1.2f);
        glow.gameObject.AddComponent<MIGlow>();
        MIParticles.Sparks(root, new Vector3(0, .4f, 0), new Color(1f, .9f, .6f, .55f));
        var rest = root.gameObject.AddComponent<MSRest>();
        var so = new SerializedObject(rest);
        so.FindProperty("zone").intValue = zoneIndex; so.FindProperty("spawn").objectReferenceValue = spawn;
        so.FindProperty("glow").objectReferenceValue = glow; so.FindProperty("displayName").stringValue = "Descanso";
        so.FindProperty("range").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        Label(zone, "Descanso " + id + " (O09)", local + Vector3.up * 2.5f, 0);
    }

    // A02 base + A01 frame + effect plane + light; pairs are told apart by colour, name and the
    // number of notches on the frame (two for P1/P2, three for P3/P4).
    private static MSPortal Portal(Transform zone, string name, Vector3 local, float yaw, int notches, Material veilMaterial, Color color,
        string requiredFlag, string lockedText, string destination, bool toPlaza = false)
    {
        var root = Group(zone, "Portal " + name, zone.TransformPoint(local));
        root.localRotation = Quaternion.Euler(0, yaw, 0);
        Piece("A02", root, "Base", new Vector3(0, .3f, 0), 0, true);
        var frame = Piece("A01", root, "Marco", new Vector3(0, .3f, 0), 0, true, layer: true, variant: notches == 3 ? "TresMuescas" : null);
        bool modelled = frame.Find("Modelo MS_A01") != null || frame.Find("Modelo MS_A01_TresMuescas") != null;
        for (int i = 0; i < notches && !modelled; i++)
            Block(root, "Muesca " + (i + 1), new Vector3((i - (notches - 1) * .5f) * .5f, 4.25f, 0), new Vector3(.22f, .3f, .3f), _metal, false).layer = IgnoreRaycast;
        var veil = GameObject.CreatePrimitive(PrimitiveType.Quad);
        veil.name = "EffectPlane"; veil.transform.SetParent(root, false);
        veil.transform.localPosition = new Vector3(0, 1.8f, 0); veil.transform.localScale = new Vector3(2, 3, 1);
        UnityEngine.Object.DestroyImmediate(veil.GetComponent<Collider>());
        veil.GetComponent<Renderer>().sharedMaterial = veilMaterial;
        veil.layer = IgnoreRaycast;
        var glow = AddLight(root, "Luz del portal", new Vector3(0, 1.8f, 1f), color, 7, 2.2f);
        MIParticles.Sparks(root, new Vector3(0, 1.6f, .2f), new Color(color.r, color.g, color.b, .6f));
        var portal = root.gameObject.AddComponent<MSPortal>();
        var so = new SerializedObject(portal);
        so.FindProperty("requiredFlag").stringValue = requiredFlag; so.FindProperty("lockedText").stringValue = lockedText;
        so.FindProperty("destinationName").stringValue = destination; so.FindProperty("toPlaza").boolValue = toPlaza;
        so.FindProperty("veil").objectReferenceValue = veil.GetComponent<Renderer>(); so.FindProperty("glow").objectReferenceValue = glow;
        so.FindProperty("color").colorValue = color;
        so.FindProperty("displayName").stringValue = "Portal " + name; so.FindProperty("range").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        Label(zone, "Portal " + name + " (A01+A02)", local + Vector3.up * 5.2f, 0);
        return portal;
    }

    // ---------------------------------------------------------------- environment (guide 14)

    private static void Sky(Transform parent)
    {
        var sun = new GameObject("Sol", typeof(Light)).GetComponent<Light>();
        sun.transform.SetParent(parent, false);
        // The sun stands over the summit seen from 01 (guide 13.4): yaw ~54 degrees, 38 high.
        sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(38, 54 + 180, 0);
        sun.color = new Color(1f, .95f, .84f); sun.intensity = 1.7f; sun.shadows = LightShadows.Soft; sun.shadowStrength = .7f;
        RenderSettings.sun = sun;
        var shader = Shader.Find("Nemequene/Andean Sky");
        var haze = new Color(.71f, .90f, .87f); // turquoise #B5E6DF
        if (shader != null)
        {
            string path = MaterialFolder + "/MS_Cielo.mat";
            var sky = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (sky == null) { sky = new Material(shader); AssetDatabase.CreateAsset(sky, path); }
            sky.shader = shader;
            sky.SetColor("_ZenithColor", new Color(.30f, .70f, .78f));
            sky.SetColor("_SkyColor", new Color(.55f, .85f, .84f));
            sky.SetColor("_HorizonColor", new Color(.93f, .97f, .92f));
            sky.SetColor("_FogColor", new Color(1f, .95f, .85f));
            sky.SetColor("_SunColor", new Color(1f, .93f, .75f));
            sky.SetColor("_CloudColor", new Color(1f, .95f, .85f));
            // The "mountains" of the shader become a cream sea of clouds below the horizon.
            sky.SetColor("_FarMountain", new Color(.95f, .93f, .88f)); sky.SetColor("_MidMountain", new Color(1f, .96f, .88f));
            sky.SetColor("_NearMountain", new Color(1f, .97f, .9f)); sky.SetColor("_Rock", new Color(.93f, .88f, .8f)); sky.SetColor("_Snow", Color.white);
            sky.SetFloat("_CloudCover", .45f); sky.SetFloat("_MountainScale", .35f); sky.SetFloat("_Haze", .3f); sky.SetFloat("_SunSize", .9990f);
            sky.SetVector("_SunDirection", (-sun.transform.forward).normalized);
            EditorUtility.SetDirty(sky);
            RenderSettings.skybox = sky;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.66f, .82f, .84f);
        RenderSettings.ambientEquatorColor = new Color(.80f, .74f, .62f);
        RenderSettings.ambientGroundColor = new Color(.45f, .38f, .30f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = haze; RenderSettings.fogStartDistance = 70; RenderSettings.fogEndDistance = 300;
    }

    private static void FarIslands(Transform parent)
    {
        var g = Group(parent, "IslasLejanas");
        var spots = new[] { new Vector3(-55, 10, 65), new Vector3(90, 15, -30), new Vector3(190, 35, 70), new Vector3(50, 55, 110), new Vector3(170, 70, 150), new Vector3(-25, 40, 120) };
        for (int i = 0; i < spots.Length; i++) Piece("E08", g, "Isla lejana " + (i + 1), spots[i], i * 37);
    }

    // Cumulus clusters beside and below the islands plus a sea of clouds far below, all drifting
    // slowly (MSCloudDrift). Every cluster stays clear of landings and flight corridors over its
    // whole travel (guide 6.3: no walkable clouds, no opaque cloud hiding the next edge).
    private static void Clouds(Transform parent, List<Vector3> route)
    {
        var samples = new List<Vector3>(route);
        // Flights and walks join nearby surfaces: sample the straight line between them.
        for (int i = 0; i < route.Count; i++)
            for (int j = i + 1; j < route.Count; j++)
                if (Vector3.Distance(route[i], route[j]) < 42f)
                    for (int k = 1; k < 4; k++) samples.Add(Vector3.Lerp(route[i], route[j], k / 4f));
        var g = Group(parent, "Nubes");
        var material = CloudMaterial();
        var random = new System.Random(11);
        Func<float, float, float> R = (a, b) => a + (float)random.NextDouble() * (b - a);
        int placed = 0;
        for (int attempt = 0; attempt < 900 && placed < 44; attempt++)
        {
            float size = R(4.5f, 9f), amplitude = R(5f, 12f), yaw = R(0f, 360f) * Mathf.Deg2Rad;
            var axis = new Vector3(Mathf.Sin(yaw), 0, Mathf.Cos(yaw));
            var p = new Vector3(R(-70, 230), R(-24, 64), R(-50, 170));
            Vector3 a = p - axis * amplitude, b = p + axis * amplitude;
            bool clear = samples.All(s =>
            {
                // Horizontal distance from the sample to the cluster's travel segment.
                var flatS = new Vector2(s.x, s.z); var fa = new Vector2(a.x, a.z); var fb = new Vector2(b.x, b.z);
                var ab = fb - fa; float t = Mathf.Clamp01(Vector2.Dot(flatS - fa, ab) / Mathf.Max(.001f, ab.sqrMagnitude));
                float flat = Vector2.Distance(flatS, fa + ab * t);
                return flat > 20f + size * 1.6f || p.y + size * 1.4f < s.y - 10f;
            });
            if (!clear) continue;
            Cluster(g, "Nube " + (++placed), p, size, material, R, axis, amplitude, R(70f, 140f), R(.3f, .8f));
        }
        // Sea of clouds: wide, slow clusters well under every island.
        for (int i = 0; i < 24; i++)
        {
            float yaw = R(0f, 360f) * Mathf.Deg2Rad;
            Cluster(g, "Mar de nubes " + (i + 1), new Vector3(R(-90, 250), R(-42, -30), R(-70, 190)), R(13f, 22f), material, R,
                new Vector3(Mathf.Sin(yaw), 0, Mathf.Cos(yaw)), R(6f, 14f), R(120f, 220f), R(.4f, 1f));
        }
    }

    // One cumulus: a flat wide base and puffs that rise towards the middle.
    private static void Cluster(Transform parent, string name, Vector3 position, float size, Material material, Func<float, float, float> R,
        Vector3 axis, float amplitude, float period, float bob)
    {
        var cloud = Group(parent, name, position);
        Sphere(cloud, "Base", Vector3.zero, new Vector3(size * 2.6f, size * .5f, size * 1.7f), material);
        int puffs = 4 + (int)R(0, 4.99f);
        for (int k = 0; k < puffs; k++)
        {
            float s = size * R(.55f, 1.05f), x = R(-1.1f, 1.1f) * size, z = R(-.55f, .55f) * size;
            float y = s * .2f + (1f - Mathf.Abs(x) / (size * 1.25f)) * size * .4f;
            Sphere(cloud, "Bocanada", new Vector3(x, y, z), new Vector3(s * 1.25f, s, s * 1.1f), material);
        }
        foreach (var r in cloud.GetComponentsInChildren<Renderer>(true))
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            r.gameObject.layer = IgnoreRaycast;
        }
        Dynamic(cloud);
        var drift = cloud.gameObject.AddComponent<MSCloudDrift>();
        var so = new SerializedObject(drift);
        so.FindProperty("axis").vector3Value = axis; so.FindProperty("amplitude").floatValue = amplitude;
        so.FindProperty("period").floatValue = period; so.FindProperty("bob").floatValue = bob;
        so.FindProperty("phase").floatValue = R(0f, 6.28f);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Toon cloud material (Nemequene/Cloud): warm cream lit side, turquoise shade, bright rim.
    private static Material CloudMaterial()
    {
        var shader = Shader.Find("Nemequene/Cloud");
        if (shader == null) return _cloud;
        string path = MaterialFolder + "/MS_NubesSuaves.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader;
        material.SetColor("_LitColor", Hex("FFF6E3"));
        material.SetColor("_ShadeColor", new Color(.74f, .87f, .87f));
        material.SetColor("_RimColor", Color.white);
        material.SetFloat("_RimPower", 2.6f); material.SetFloat("_RimStrength", .55f);
        material.SetFloat("_Wobble", .18f); material.SetFloat("_WobbleSpeed", .5f);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- partial update

    // Applies new clouds and any newly delivered models to the SAVED scene without rebuilding it,
    // so hand adjustments made in the editor (stairs, bands...) are kept.
    [MenuItem("Nemequene/Mundo Superior/Actualizar nubes y modelos")]
    public static void UpdateScene()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        try
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Catalog.Clear(); Meshes.Clear(); _kit = null; KeepMaterials.Clear();
            Directory.CreateDirectory(KitMaterialFolder);
            Materials();
            DefineCatalog();
            var world = scene.GetRootGameObjects().First(go => go.name == "MundoSuperior").transform;
            var environment = world.Find("Environment");
            var old = environment.Find("Nubes");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var route = world.GetComponentsInChildren<MSZone>(true).Select(z => z.transform.position).ToList();
            foreach (var transport in world.GetComponentsInChildren<MSTransport>(true))
            {
                var so = new SerializedObject(transport);
                Vector3 s = so.FindProperty("start").vector3Value, e = so.FindProperty("end").vector3Value;
                route.Add(s); route.Add(e); route.Add((s + e) * .5f);
            }
            Clouds(environment, route);
            var added = AddMissingModels(world);
            var playerRoot = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Player");
            if (playerRoot != null && playerRoot.GetComponent<MSFootsteps>() == null) { playerRoot.AddComponent<MSFootsteps>(); added.Add("pasos"); }
            var wingsVisual = playerRoot != null ? playerRoot.transform.Find("Visual_Alas") : null;
            // The equipped wings are always rebuilt, so size or rig tuning reaches the saved scene.
            if (wingsVisual != null && Directory.Exists(WingsModelFolder))
            {
                bool had = wingsVisual.Find("Alas (rig)") != null;
                BuildWings(wingsVisual);
                if (!had) added.Add("Alas con rig");
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("MS_UPDATE_OK: nubes regeneradas; modelos nuevos: " + (added.Count == 0 ? "ninguno" : string.Join(", ", added)));
        }
        catch (Exception e) { Debug.LogError("MS_UPDATE_FAILED: " + e); }
    }

    // Grey pieces whose ID now has a model get it, fitted to the size of their grey block.
    private static List<string> AddMissingModels(Transform world)
    {
        var added = new List<string>();
        foreach (var root in world.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("MS_") && t.name.Length > 6).ToArray())
        {
            var grey = root.Find("Gris");
            if (grey == null || root.Cast<Transform>().Any(c => c.name.StartsWith("Modelo"))) continue;
            string id = root.name.Substring(3, 3);
            if (!Catalog.TryGetValue(id, out var spec) || KitPath(id) == null) continue;
            var b = LocalBounds(root, grey.gameObject);
            var size = b.size.sqrMagnitude > 1e-4f ? b.size : spec.Size;
            if (Model(id, null, root, grey, size, spec.Pivot)) added.Add(id);
        }
        return added.Distinct().ToList();
    }

    // ---------------------------------------------------------------- player and systems

    private static void Player(Transform world)
    {
        var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = "Player";
        playerObject.layer = IgnoreRaycast; // kept out of the camera obstruction test
        playerObject.transform.position = _entry.position + Vector3.up * 1.05f;
        UnityEngine.Object.DestroyImmediate(playerObject.GetComponent<CapsuleCollider>());
        playerObject.GetComponent<Renderer>().sharedMaterial = _marker;
        var character = playerObject.AddComponent<CharacterController>();
        character.height = 2; character.radius = .45f; character.stepOffset = .35f; character.slopeLimit = 45;
        var player = playerObject.AddComponent<PlayerController>();
        playerObject.AddComponent<Health>();
        playerObject.AddComponent<MSFootsteps>();

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ExplorationOrbitCamera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.nearClipPlane = .1f; camera.farClipPlane = 450; camera.fieldOfView = 58;
        camera.clearFlags = CameraClearFlags.Skybox; camera.backgroundColor = RenderSettings.fogColor;
        var orbit = cameraObject.GetComponent<ExplorationOrbitCamera>();
        var so = new SerializedObject(orbit); so.FindProperty("target").objectReferenceValue = playerObject.transform; so.FindProperty("distance").floatValue = 9;
        so.ApplyModifiedPropertiesWithoutUndo();
        cameraObject.transform.SetPositionAndRotation(playerObject.transform.position + new Vector3(0, 5, -8), Quaternion.Euler(30, 0, 0));
        so = new SerializedObject(player);
        so.FindProperty("enableSprint").boolValue = true;
        so.FindProperty("enableExplorationDash").boolValue = true; // Q ground dash, Shift runs
        so.FindProperty("allowAirDash").boolValue = false;          // local profile (guide 1.4)
        so.FindProperty("movementReference").objectReferenceValue = cameraObject.transform;
        so.ApplyModifiedPropertiesWithoutUndo();
        Visual(playerObject);

        // Visual_Alas: the equipped wings (derived instance of O03, not counted), off until ms_alas.
        var wingsVisual = Group(playerObject.transform, "Visual_Alas", playerObject.transform.TransformPoint(new Vector3(0, .45f, -.3f)));
        BuildWings(wingsVisual);
        var wings = playerObject.AddComponent<MSWings>();
        so = new SerializedObject(wings);
        so.FindProperty("movementReference").objectReferenceValue = cameraObject.transform;
        so.FindProperty("visual").objectReferenceValue = wingsVisual.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        BuildSystems(world, player, orbit, wings);
    }

    // Equipped wings: the rigged ALAS model animated by MSWingRig, else the O03 export, else grey
    // plates. Replaces whatever the container held, so the update can swap them in place.
    private static void BuildWings(Transform wingsVisual)
    {
        foreach (var child in wingsVisual.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        string rigged = Directory.Exists(WingsModelFolder)
            ? Directory.GetFiles(WingsModelFolder, "*.fbx").Select(f => f.Replace('\\', '/')).FirstOrDefault() : null;
        var prefab = rigged != null ? AssetDatabase.LoadAssetAtPath<GameObject>(rigged) : null;
        if (prefab != null)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, wingsVisual);
            model.name = "Alas (rig)";
            model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity; model.transform.localScale = Vector3.one;
            foreach (var c in model.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
            var b = LocalBounds(wingsVisual, model);
            float span = Mathf.Max(b.size.x, b.size.z);
            if (span > .01f) model.transform.localScale = Vector3.one * (2.3f / span);
            b = LocalBounds(wingsVisual, model);
            model.transform.localPosition -= new Vector3(b.center.x, b.center.y - .1f, b.center.z);
            foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>(true)) { r.updateWhenOffscreen = true; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
            model.AddComponent<MSWingRig>();
        }
        else
        {
            foreach (int side in new[] { -1, 1 })
            {
                var w = Block(wingsVisual, side < 0 ? "Ala izquierda" : "Ala derecha", new Vector3(side * .45f, .1f, 0), new Vector3(.85f, .55f, .05f), _metal, false);
                w.transform.localRotation = Quaternion.Euler(0, side * 20, side * -12);
                w.layer = IgnoreRaycast; w.isStatic = false;
            }
            if (Model("O03", "Equipables", wingsVisual, wingsVisual, new Vector3(1.8f, .9f, .4f), Pivot.Center))
                foreach (Transform c in wingsVisual) if (c.name.StartsWith("Modelo")) c.localPosition += new Vector3(0, .1f, -.1f);
        }
        Dynamic(wingsVisual);
        foreach (var t in wingsVisual.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = IgnoreRaycast;
        wingsVisual.gameObject.SetActive(false);
    }

    private static void BuildSystems(Transform world, PlayerController player, ExplorationOrbitCamera orbit, MSWings wings)
    {
        SerializedObject so;

        var systems = new GameObject("Systems", typeof(MundoSuperiorDirector));
        systems.transform.SetParent(world, false);
        so = new SerializedObject(systems.GetComponent<MundoSuperiorDirector>());
        so.FindProperty("player").objectReferenceValue = player;
        so.FindProperty("orbitCamera").objectReferenceValue = orbit;
        so.FindProperty("wings").objectReferenceValue = wings;
        so.FindProperty("entrySpawn").objectReferenceValue = _entry;
        so.FindProperty("teamLabels").objectReferenceValue = _labels.gameObject;
        var spawns = so.FindProperty("zoneTestSpawns"); spawns.arraySize = TestSpawns.Length;
        for (int i = 0; i < TestSpawns.Length; i++) spawns.GetArrayElementAtIndex(i).objectReferenceValue = TestSpawns[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Same fitting as the lower world: model height = capsule, Animator origin on its bottom.
    private static void Visual(GameObject player)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerVisual);
        if (prefab == null) return;
        player.GetComponent<MeshRenderer>().enabled = false;
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
        visual.name = "Nemequene_Visual";
        foreach (Transform t in visual.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = player.layer;
        var cc = player.GetComponent<CharacterController>();
        var renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
        if (b.size.y > .01f) visual.transform.localScale = Vector3.one * (cc.height * .98f / b.size.y);
        float bottom = player.transform.TransformPoint(cc.center).y - cc.height * .5f - cc.skinWidth;
        visual.transform.position = new Vector3(visual.transform.position.x, bottom, visual.transform.position.z);
    }

    // Inventory check against the guide table (10.1): a mismatch is a warning, not a failure.
    private static void ReportInventory()
    {
        int total = 0, expected = 0;
        var lines = new List<string>();
        foreach (var pair in Catalog)
        {
            int n = Counts.TryGetValue(pair.Key, out int c) ? c : 0;
            total += n; expected += pair.Value.Expected;
            if (n != pair.Value.Expected) lines.Add(pair.Key + " " + n + "/" + pair.Value.Expected);
        }
        if (lines.Count == 0) Debug.Log("MS_INVENTORY_OK: " + total + " instancias de " + Catalog.Count + " referencias");
        else Debug.LogWarning("MS_INVENTORY: " + total + "/" + expected + " · diferencias: " + string.Join(", ", lines));
        var models = Catalog.Keys.Where(id => KitPath(id) != null).ToArray();
        Debug.Log("MS_KIT: " + models.Length + " / " + Catalog.Count + " piezas con modelo" + (models.Length > 0 ? " (" + string.Join(", ", models) + ")" : ""));
    }

    // ---------------------------------------------------------------- kit models

    // Optimized kit files by name: "MS_T01", "MS_T01_Grieta", "MS_O08_Medallon"... (file name
    // without "_Optimizado"). Only the decimated files are used: the Tripo originals are ~2M tris.
    private static string KitFile(string key)
    {
        if (_kit == null)
        {
            _kit = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(KitRoot))
            {
                foreach (var file in Directory.GetFiles(KitRoot, "MS_*_Optimizado.fbx", SearchOption.AllDirectories).Select(f => f.Replace('\\', '/')))
                    _kit[Path.GetFileNameWithoutExtension(file).Replace("_Optimizado", "")] = file;
                // A light export named after its ID (E04.fbx) is used as is, with its materials.
                foreach (var file in Directory.GetFiles(KitRoot, "*.fbx", SearchOption.AllDirectories).Select(f => f.Replace('\\', '/')))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (!Regex.IsMatch(name, "^[TAOEC][0-9][0-9]$") || _kit.ContainsKey("MS_" + name)) continue;
                    _kit["MS_" + name] = file; KeepMaterials.Add(file);
                }
            }
        }
        return key != null && _kit.TryGetValue(key, out var path) ? path : null;
    }

    // The base model of an ID, or one of its variants when the base export is missing.
    private static string KitPath(string id)
    {
        var path = KitFile("MS_" + id);
        if (path != null) return path;
        return _kit.Where(k => k.Key.StartsWith("MS_" + id + "_", StringComparison.OrdinalIgnoreCase)).Select(k => k.Value).FirstOrDefault();
    }

    // Modular and architectural pieces fill their catalog box exactly (slabs must tile without
    // gaps); props keep their proportions inside it.
    private static readonly HashSet<string> Stretch = new HashSet<string>
    { "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "A01", "A02", "A03", "A04", "A05", "A06", "A07", "A08", "O07", "O08", "O09", "O10", "E03", "E04", "E05", "E07" };

    // Fits the model into the catalog box with the pivot's anchor on the root and hides the grey
    // renderers; the grey colliders stay as the authority (guide 19.7: check after swapping).
    private static bool Model(string id, string variant, Transform root, Transform grey, Vector3 size, Pivot pivot)
    {
        string path = variant != null ? KitFile("MS_" + id + "_" + variant) ?? KitPath(id) : KitPath(id);
        var prefab = path != null ? AssetDatabase.LoadAssetAtPath<GameObject>(path) : null;
        if (prefab == null) return false;
        var greyRenderers = grey.GetComponentsInChildren<Renderer>(true);
        string key = Path.GetFileNameWithoutExtension(path).Replace("_Optimizado", "");
        var container = new GameObject("Modelo " + key).transform; container.SetParent(root, false);
        var scaler = new GameObject("Scale").transform; scaler.SetParent(container, false);
        var orientation = new GameObject("Orientation").transform; orientation.SetParent(scaler, false);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, orientation);
        model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity; model.transform.localScale = Vector3.one;
        foreach (var c in model.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
        var material = KeepMaterials.Contains(path) ? null : KitMaterial(key, Path.GetDirectoryName(path).Replace('\\', '/'));
        if (material != null)
            foreach (var r in model.GetComponentsInChildren<Renderer>(true)) r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
        var b = LocalBounds(container, model);
        if (b.size.sqrMagnitude < 1e-6f) { UnityEngine.Object.DestroyImmediate(container.gameObject); return false; }
        // Long pieces modelled along the other horizontal axis turn a quarter (bands, rails, panels).
        bool boxWide = size.x > size.z * 1.4f, boxDeep = size.z > size.x * 1.4f;
        bool modelWide = b.size.x > b.size.z * 1.4f, modelDeep = b.size.z > b.size.x * 1.4f;
        if ((boxWide && modelDeep) || (boxDeep && modelWide)) { orientation.localRotation = Quaternion.Euler(0, 90, 0); b = LocalBounds(container, model); }
        Vector3 ratio;
        if (Stretch.Contains(id)) ratio = new Vector3(size.x / b.size.x, size.y / b.size.y, size.z / b.size.z);
        // Props keep their modelled pose (a medallion may lie flat): the largest side matches.
        else ratio = Vector3.one * (Mathf.Max(size.x, size.y, size.z) / Mathf.Max(b.size.x, b.size.y, b.size.z));
        scaler.localScale = ratio;
        b = LocalBounds(container, model);
        scaler.localPosition = -(b.min + Vector3.Scale(Anchor(pivot), b.size));
        foreach (var r in greyRenderers) r.enabled = false;
        foreach (var t in container.GetComponentsInChildren<Transform>(true)) { t.gameObject.layer = root.gameObject.layer; t.gameObject.isStatic = true; }
        return true;
    }

    // URP Lit from the Tripo textures next to the original model (base colour, normal, and a
    // metallic/smoothness map composed from the metallic and roughness maps), as in the lower world.
    private static Material KitMaterial(string key, string folder)
    {
        string path = KitMaterialFolder + "/" + key + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.shader = Shader.Find("Universal Render Pipeline/Lit");
        string textures = Directory.GetDirectories(folder, "*.fbm").FirstOrDefault()?.Replace('\\', '/');
        Func<string, string> find = suffix => textures == null ? null : Directory.GetFiles(textures)
            .Where(f => !f.EndsWith(".meta")).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLowerInvariant().EndsWith(suffix))?.Replace('\\', '/');
        material.SetColor("_BaseColor", Color.white);
        string baseMap = find("_basecolor");
        if (baseMap != null) material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(baseMap));
        string normal = find("_normal");
        if (normal != null)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(normal);
            if (importer != null && importer.textureType != TextureImporterType.NormalMap) { importer.textureType = TextureImporterType.NormalMap; importer.SaveAndReimport(); }
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal)); material.EnableKeyword("_NORMALMAP");
        }
        string metallic = find("_metallic"), roughness = find("_roughness");
        if (metallic != null && roughness != null)
        {
            string packed = KitMaterialFolder + "/" + key + "_MetallicSmoothness.png";
            if (!File.Exists(packed)) Pack(metallic, roughness, packed);
            AssetDatabase.ImportAsset(packed);
            var importer = (TextureImporter)AssetImporter.GetAtPath(packed);
            if (importer.sRGBTexture || importer.maxTextureSize != 1024) { importer.sRGBTexture = false; importer.maxTextureSize = 1024; importer.SaveAndReimport(); }
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(packed));
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Metallic", 1); material.SetFloat("_Smoothness", 1); material.SetFloat("_SmoothnessTextureChannel", 0);
        }
        else { material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", .3f); }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void Pack(string metallicPath, string roughnessPath, string output)
    {
        var metallic = new Texture2D(2, 2); metallic.LoadImage(File.ReadAllBytes(metallicPath));
        var roughness = new Texture2D(2, 2); roughness.LoadImage(File.ReadAllBytes(roughnessPath));
        int w = Mathf.Min(metallic.width, 1024), h = Mathf.Min(metallic.height, 1024);
        var result = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        var pixels = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x + .5f) / w, v = (y + .5f) / h;
                byte m = (byte)(metallic.GetPixelBilinear(u, v).r * 255);
                byte sm = (byte)((1 - roughness.GetPixelBilinear(u, v).r) * 255);
                pixels[y * w + x] = new Color32(m, m, m, sm);
            }
        result.SetPixels32(pixels); result.Apply();
        File.WriteAllBytes(output, result.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(metallic); UnityEngine.Object.DestroyImmediate(roughness); UnityEngine.Object.DestroyImmediate(result);
    }

    // E05 is built from chords in place; its model (pivot at the bottom centre of the arc) goes
    // on the circle at the arc's middle.
    private static bool ModelInto(string id, string variant, Transform arc, Vector3 local)
    {
        if (KitPath(id) == null) return false;
        var holder = Group(arc, "Modelo E05", arc.TransformPoint(local));
        holder.localRotation = Quaternion.identity;
        return Model(id, variant, holder, arc, Catalog[id].Size, Pivot.Bottom);
    }

    // Moving objects (pickups, leaf, platform, guardian) must not be static-batched.
    private static void Dynamic(Transform t)
    {
        foreach (var c in t.GetComponentsInChildren<Transform>(true)) c.gameObject.isStatic = false;
    }

    private static Bounds LocalBounds(Transform space, GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        bool any = false; var b = new Bounds();
        foreach (var r in renderers)
        {
            var mesh = r is SkinnedMeshRenderer s ? s.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null) continue;
            var mb = mesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var p = space.InverseTransformPoint(r.transform.TransformPoint(corner));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; } else b.Encapsulate(p);
            }
        }
        return b;
    }

    // ---------------------------------------------------------------- graybox helpers

    private static Transform Group(Transform parent, string name) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }

    private static Transform Group(Transform parent, string name, Vector3 worldPosition)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = worldPosition; return t;
    }

    private static Transform Mark(Transform parent, string name, Vector3 local, float yaw)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false);
        t.localPosition = local; t.rotation = Quaternion.Euler(0, yaw, 0);
        return t;
    }

    private static void Label(Transform parent, string text, Vector3 local, float yaw)
    {
        var go = new GameObject("Etiqueta " + text);
        go.transform.SetParent(_labels, false);
        go.transform.position = parent.TransformPoint(local);
        go.transform.rotation = Quaternion.Euler(0, yaw, 0);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text; tmp.fontSize = 9; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(.22f, .18f, .14f); tmp.rectTransform.sizeDelta = new Vector2(16, 3);
        tmp.textWrappingMode = TextWrappingModes.Normal;
    }

    private static Light AddLight(Transform parent, string name, Vector3 local, Color color, float range, float intensity)
    {
        var light = new GameObject(name, typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(parent, false); light.transform.localPosition = local;
        light.type = LightType.Point; light.color = color; light.range = range; light.intensity = intensity; light.shadows = LightShadows.None;
        return light;
    }

    private static GameObject Block(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool collider, Material top = null)
    {
        var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.isStatic = true;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = center;
        obj.GetComponent<MeshFilter>().sharedMesh = BoxMesh(size);
        obj.GetComponent<MeshRenderer>().sharedMaterials = new[] { top ?? material, material };
        if (collider) obj.AddComponent<BoxCollider>().size = size;
        obj.layer = parent.gameObject.layer;
        return obj;
    }

    private static Transform Cylinder(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool collider)
        => Primitive(PrimitiveType.Cylinder, parent, name, center, new Vector3(size.x, size.y * .5f, size.z), material, collider);

    private static Transform Sphere(Transform parent, string name, Vector3 center, Vector3 size, Material material)
        => Primitive(PrimitiveType.Sphere, parent, name, center, size, material, false);

    private static Transform Primitive(PrimitiveType type, Transform parent, string name, Vector3 center, Vector3 scale, Material material, bool collider)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name; go.isStatic = true;
        go.transform.SetParent(parent, false); go.transform.localPosition = center; go.transform.localScale = scale;
        if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.layer = parent.gameObject.layer;
        return go.transform;
    }

    // Box with UVs in metres (2 m per texture tile) so the floor grid keeps its scale; the top
    // face uses its own material (walkable surface).
    private static Mesh BoxMesh(Vector3 size)
    {
        string key = size.x.ToString("0.###") + "x" + size.y.ToString("0.###") + "x" + size.z.ToString("0.###");
        if (Meshes.TryGetValue(key, out var cached)) return cached;
        var h = size * .5f;
        var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>();
        var top = new List<int>(); var sides = new List<int>();
        void Face(Vector3 n, Vector3 u, Vector3 v, float du, float dv, List<int> tris)
        {
            int i = vertices.Count;
            Vector3 c = Vector3.Scale(n, h);
            Vector3 hu = u * Vector3.Scale(u, h).magnitude, hv = v * Vector3.Scale(v, h).magnitude;
            vertices.Add(c - hu - hv); vertices.Add(c - hu + hv); vertices.Add(c + hu + hv); vertices.Add(c + hu - hv);
            for (int k = 0; k < 4; k++) normals.Add(n);
            const float tile = .5f;
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(0, dv * tile)); uvs.Add(new Vector2(du * tile, dv * tile)); uvs.Add(new Vector2(du * tile, 0));
            tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z, top);
        Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z, sides);
        Face(Vector3.forward, Vector3.left, Vector3.up, size.x, size.y, sides);
        Face(Vector3.back, Vector3.right, Vector3.up, size.x, size.y, sides);
        Face(Vector3.right, Vector3.forward, Vector3.up, size.z, size.y, sides);
        Face(Vector3.left, Vector3.back, Vector3.up, size.z, size.y, sides);
        var mesh = new Mesh { name = "MS_Bloque_" + key };
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2; mesh.SetTriangles(top, 0); mesh.SetTriangles(sides, 1);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        Meshes[key] = mesh;
        return mesh;
    }

    // ---------------------------------------------------------------- materials (guide 14.1)

    private static void Materials()
    {
        _sand = Flat("MS_SueloArena", Hex("E8C98B"), texture: GridTexture());
        _ochre = Flat("MS_PiedraOcre", Hex("C49A55"));
        _dark = Flat("MS_Contorno", Hex("392E23"));
        _metal = Flat("MS_MetalMate", Hex("B78D35"), smoothness: .45f, metallic: .7f);
        _wood = Flat("MS_Madera", Hex("70543B"));
        _ceramic = Flat("MS_Ceramica", Hex("AA6849"));
        _plant = Flat("MS_Vegetacion", Hex("6A8051"));
        _cloth = Flat("MS_Tejido", Hex("9E4A3A"));
        _cloud = Flat("MS_Nubes", Hex("FFF3D9"), emission: Hex("FFF3D9") * .35f);
        _pink = Flat("MS_Portal_Rosa", Hex("D948C5"), emission: Hex("D948C5") * 1.6f, transparent: .7f);
        _blue = Flat("MS_Portal_Azul", Hex("357BFF"), emission: Hex("357BFF") * 1.6f, transparent: .7f);
        _grey = Flat("MS_Portal_Gris", Hex("E6E0D2"), emission: Hex("E6E0D2") * 1.2f, transparent: .65f);
        _core = Flat("MS_Nucleo", new Color(1f, .7f, .3f), emission: new Color(1f, .7f, .3f) * 1.5f);
        _marker = Flat("MS_Marca", new Color(.86f, .70f, .32f));
        _grip = Flat("MS_Agarre", Hex("F4E2B0"));
        _far = Flat("MS_IslaLejana", Hex("D2B27A"));
    }

    private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

    private static Material Flat(string name, Color color, Color? emission = null, float transparent = 0, float smoothness = .12f, float metallic = 0, Texture2D texture = null)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", transparent > 0 ? new Color(color.r, color.g, color.b, transparent) : color);
        material.SetTexture("_BaseMap", texture);
        material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Metallic", metallic);
        if (emission.HasValue) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission.Value); material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
        if (transparent > 0)
        {
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetFloat("_Cull", 0); // portal planes read from both sides
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    // 2 m measuring grid for walkable tops: thin darker lines, so distances read while testing.
    private static Texture2D GridTexture()
    {
        string path = TextureFolder + "/MS_Reticula.png";
        if (!File.Exists(path))
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    bool line = x < 2 || y < 2;
                    bool half = x == n / 2 || y == n / 2;
                    float v = line ? .82f : half ? .93f : 1f;
                    tex.SetPixel(x, y, new Color(v, v, v, 1));
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // Small fluent helper so a safe-support group can receive its piece inline.
    private static Transform Also(this Transform t, Action<Transform> action) { action(t); return t; }
}

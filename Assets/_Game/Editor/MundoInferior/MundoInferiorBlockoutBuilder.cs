using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the Mundo Inferior experience (Specs/Worlds/Guia-completa-mundo-inferior): nine room roots
// with their origin at the centre of the entry edge, floors with real colliders at the guide's
// heights and textured faces (blue-green walkable tops, blue-grey stone sides), physical
// connectors, pits with fall volumes, and every functional instance: finds with inspection,
// optional offerings, levers, rests, the horn socket, slabs that give way, falling stalactites,
// pendulums, gates, portals and the provisional guardians of 04 and 09. The Tripo kit
// (Assets/_Game/Art/Environments/MundoInferior/Models/*/<ID>_Optimizado.fbx) dresses it; characters come later.
// The scene is regenerated from scratch on each run: change this builder, not the scene.
[InitializeOnLoad]
public static class MundoInferiorBlockoutBuilder
{
    public const string ScenePath = WorldTravel.LowerWorldScene;
    private const string KitRoot = "Assets/_Game/Art/Environments/MundoInferior/Models";
    private const string MaterialFolder = "Assets/_Game/Art/Environments/MundoInferior/Materials";
    private const string KitMaterialFolder = MaterialFolder + "/Kit";
    private const string TextureFolder = MaterialFolder + "/Textures";
    private const string PlayerVisual = "Assets/_Game/Art/Characters/Nemequene/Nemequene_Player_Visual.prefab";
    private const string AutoRunKey = "MundoInferior.Experience.v2";

    private enum Anchor { Bottom, Center, Top }

    private sealed class Room
    {
        public int Index; public string Name; public Transform Root; public float Yaw; public Transform Spawn;
        public float Width, Depth;
    }

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    private static Dictionary<string, string> _kit;
    private static Material _top, _trialTop, _stone, _depth, _hazard, _frame, _veil, _marker, _warning, _wave, _metal;
    private static readonly List<Room> Rooms = new List<Room>();
    private static Transform _decor;

    static MundoInferiorBlockoutBuilder()
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

    [MenuItem("Nemequene/Mundo Inferior/Construir nivel")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var open = Enumerable.Range(0, EditorSceneManager.sceneCount).Select(i => EditorSceneManager.GetSceneAt(i).path)
            .Where(p => !string.IsNullOrEmpty(p)).ToArray();
        try
        {
            Build();
            Debug.Log("MI_BLOCKOUT_OK: " + ScenePath);
        }
        catch (Exception e) { Debug.LogError("MI_BLOCKOUT_FAILED: " + e); }
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
        Materials.Clear(); Meshes.Clear(); Rooms.Clear(); _kit = null;
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        Directory.CreateDirectory(KitMaterialFolder);
        AssetDatabase.Refresh();
        // Shared materials of the guide (5.1): walkable blue-green, stone blue-grey, depth.
        _top = Textured("MI_Suelo", "MI_Suelo", Color.white, 0.18f);
        _trialTop = Textured("MI_Suelo_Prueba", "MI_Suelo", new Color(1.08f, 1.02f, .9f), 0.18f);
        _stone = Textured("MI_Roca", "MI_Piedra", Color.white, 0.1f);
        _depth = Textured("MI_Fondo", "MI_Fondo", Color.white, 0.05f);
        _frame = _stone;
        _hazard = Flat("MI_Peligro", new Color(0.30f, 0.07f, 0.07f), emission: new Color(0.35f, 0.03f, 0.02f));
        _warning = Flat("MI_Aviso", new Color(0.55f, 0.06f, 0.05f), emission: new Color(1.6f, 0.2f, 0.12f), transparent: .72f);
        _wave = Flat("MI_Onda", new Color(1f, .7f, .35f), emission: new Color(1.6f, .9f, .35f));
        _metal = Flat("MI_Metal", new Color(0.65f, 0.55f, 0.31f), smoothness: .45f, metallic: .8f);
        _marker = Flat("MI_Marca", new Color(0.86f, 0.70f, 0.32f));
        _veil = Flat("MI_Velo_Portal", new Color(0.35f, 0.75f, 1f), emission: new Color(0.25f, 0.6f, 1f) * 1.6f, transparent: .75f);
        MIParticles.SharedMaterial = ParticleMaterial();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var world = new GameObject("MI_WorldRoot").transform;
        var rooms = Group(world, "Rooms");
        var traversal = Group(world, "Traversal");
        var atmosphere = Group(world, "Atmosphere");
        _decor = Group(world, "Decoración");

        // Global heights of the guide (1.2): 01 at 0; 02 -2; 03 +2; 04-05 -6; 06 -10; 07-09 -14.
        Umbral(NewRoom(rooms, 0, "01 Umbral", new Vector3(110, 0, 80), -90, 12, 10, new Vector3(0, 0, 3), cameraTurn: 75));
        Santuario(NewRoom(rooms, 1, "02 Santuario de raíces", new Vector3(94, -2, 80), -90, 14, 13));
        Brazaletes(NewRoom(rooms, 2, "03 Galería de brazaletes", new Vector3(75, 2, 80), -90, 12, 18));
        Centinelas(NewRoom(rooms, 3, "04 Patio de centinelas", new Vector3(31.5f, -6, 78), 180, 18, 44));
        Pendulos(NewRoom(rooms, 4, "05 Paso de péndulos", new Vector3(36, -6, 28), 90, 10, 24));
        Derrumbe(NewRoom(rooms, 5, "06 Galería del derrumbe", new Vector3(64, -10, 5), -90, 12, 26));
        Cuerno(NewRoom(rooms, 6, "07 Cámara del cuerno", new Vector3(24, -14, 5), -90, 14, 18));
        Antesala(NewRoom(rooms, 7, "08 Antesala", new Vector3(28, -14, 26), -90, 14, 12, new Vector3(0, 0, 3)));
        // The arena camera rises to show floor, core and every warning at once (guide 7.1).
        Guardian(NewRoom(rooms, 8, "09 Cámara del guardián", new Vector3(16, -14, 26), -90, 18, 16, framing: new Vector2(12, 52)));

        // Physical connections (guide 1.2): every arrow is walkable geometry, both ways.
        Walkway(traversal, "01-02 Rampa", 3, new Vector3(100, 0, 80), new Vector3(98, 0, 80), new Vector3(94, -2, 80));
        Walkway(traversal, "02-03 Ascenso", 3, new Vector3(81, 0.6f, 80), new Vector3(79, 0.6f, 80), new Vector3(75, 2, 80));
        Walkway(traversal, "03-01 Atajo alto", 3, new Vector3(66, 2, 74), new Vector3(66, 2, 70), new Vector3(72, 2, 70), new Vector3(76, 0, 70),
            new Vector3(104, 0, 70), new Vector3(104, 0, 74));
        Walkway(traversal, "03-04 Bajada escalonada", 3, new Vector3(57.5f, 2, 80), new Vector3(55.5f, 2, 80), new Vector3(51.5f, 0, 80), new Vector3(49.5f, 0, 80),
            new Vector3(45.5f, -2, 80), new Vector3(43.5f, -2, 80), new Vector3(39.5f, -4, 80), new Vector3(37.5f, -4, 80), new Vector3(33.5f, -6, 80),
            new Vector3(31.5f, -6, 80), new Vector3(31.5f, -6, 78));
        Walkway(traversal, "04-05 Paso", 3, new Vector3(31.5f, -6, 34.5f), new Vector3(31.5f, -6, 28), new Vector3(36, -6, 28));
        Walkway(traversal, "05-06 Rampa", 3, new Vector3(60, -6, 28), new Vector3(66, -6, 28), new Vector3(66, -6, 24), new Vector3(66, -8, 20),
            new Vector3(66, -8, 18), new Vector3(66, -10, 14), new Vector3(66, -10, 5), new Vector3(64, -10, 5));
        Walkway(traversal, "06-07 Descenso", 3, new Vector3(38, -10, 5), new Vector3(36, -10, 5), new Vector3(32, -12, 5), new Vector3(30, -12, 5),
            new Vector3(26, -14, 5), new Vector3(24, -14, 5));
        Walkway(traversal, "07-08 Apoyos inferiores", 3, new Vector3(9, -14, 8), new Vector3(9, -14, 14), new Vector3(34, -14, 14),
            new Vector3(34, -14, 26), new Vector3(28, -14, 26));
        // The safe corridor of 06 ends on the 07-08 approach, before the horn gate (guide 2.2).
        Walkway(traversal, "06-08 Corredor seguro", 3, new Vector3(40, -10, 11), new Vector3(40, -10, 13), new Vector3(40, -14, 21),
            new Vector3(40, -14, 22), new Vector3(34, -14, 22));
        Kit("A05", traversal, "A05 Corredor 06 llega a 08", new Vector3(37.5f, -14, 22), 90, new Vector3(5, 5, 0), Anchor.Bottom);
        // Roots and stone along the high shortcut (guide zone 03: «raíces y piedra, apoyos continuos»).
        foreach (float x in new[] { 80f, 88f, 96f })
            Kit("N02a", _decor, "N02a Raíces del atajo", new Vector3(x, 0, 68.2f), 90, new Vector3(4, 0, 0), Anchor.Bottom, fitLargest: true);

        Kit("N01", atmosphere, "N01 Árbol seco central", new Vector3(62, -9, 52), 20, new Vector3(0, 9, 0), Anchor.Bottom);
        Atmosphere(atmosphere);
        Cavern(atmosphere);
        Player(world);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        // The plaza portal loads it by path (WorldTravel), which needs it in the build list.
        if (!EditorBuildSettings.scenes.Any(entry => entry.path == ScenePath))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) }).ToArray();
    }

    // ---------------------------------------------------------------- rooms (guide section 4)

    private static void Umbral(Room r)
    {
        var t = r.Root;
        Floor(t, "T02 Suelo", -6, 6, 0, 10, 0, _top);
        Wall(t, "T06 Pared posterior izq", -6, -1.6f, 10, 10.6f, 6);
        Wall(t, "T06 Pared posterior der", 1.6f, 6, 10, 10.6f, 6);
        Wall(t, "A06 Murete der", 6, 6.6f, 0, 10, 1);
        Wall(t, "A06 Murete izq", -6.6f, -6, 0, 4.6f, 1);
        Wall(t, "A06 Murete izq fondo", -6.6f, -6, 7.4f, 10, 1);
        Portal(t, "A04 Portal de llegada y retorno", new Vector3(0, 0, 0.7f), 0, true, null);
        Gate(t, "A02 Puerta del atajo", new Vector3(-5, 0, 6), 90, MIProgress.Shortcut03, false);
        Crystal(t, new Vector3(-3, 0, 2)); Crystal(t, new Vector3(3, 0, 7));
        Offering(t, "ofrenda_01", "Fragmento tallado", "C03d", new Vector3(4.6f, 0, 8.8f), 30,
            "Esquirla de piedra con un trazo en espiral, caída junto al muro del umbral.");
        Decor(r, new[] { (3.5f, 7.5f) }, new (float, float)[0]);
        Kit("N02b", _decor, "N02b Raíces del umbral", t.TransformPoint(new Vector3(5.2f, 0, 3)), r.Yaw + 70, new Vector3(2.4f, 0, 0), Anchor.Bottom, fitLargest: true);
    }

    private static void Santuario(Room r)
    {
        var t = r.Root;
        Floor(t, "T02 Terraza principal", -7, 7, 0, 7, 0, _top);
        Floor(t, "Suelo de recuperación", -7, 7, 7, 13, -1, _top, 1);
        Floor(t, "T01 Apoyo de salida (doble salto)", -1, 1, 7, 9, 1.8f, _trialTop, 2.8f);
        Floor(t, "Plataforma de llegada", -3, 3, 9, 13, 2.6f, _trialTop, 3.6f);
        Find(t, "Altar semilla", new Vector3(-3, 0, 4), 90, "O04d", MIProgress.Seed, MIFind.Kind.Seed, "Semilla", "O03", 0.3f,
            "Semilla dorada del santuario de raíces. Pieza de fantasía del juego: al confirmarla podrás dar un segundo salto antes de tocar el suelo.",
            "Doble salto obtenido", "Pulsa Espacio de nuevo en el aire. Prueba el apoyo alto del fondo.");
        Mark(t, "C04 Custodio de raíces (personaje pendiente)", new Vector3(3, 0, 4));
        Rest(t, 1, new Vector3(3, 0.01f, 1.8f));
        Kit("N04", t, "N04 Vegetación A", new Vector3(-4.3f, 0, 4.7f), 30, new Vector3(0, 0.8f, 0), Anchor.Bottom);
        Kit("N04", t, "N04 Vegetación B", new Vector3(-4.2f, 0, 3.1f), 160, new Vector3(0, 0.7f, 0), Anchor.Bottom);
        Kit("N04", t, "N04 Vegetación C", new Vector3(-1.8f, 0, 5.6f), 250, new Vector3(0, 0.6f, 0), Anchor.Bottom);
        Crystal(t, new Vector3(-5.5f, 0, 1), warm: true);
        Offering(t, "ofrenda_02", "Disco agrietado", "C03c", new Vector3(-5.6f, -1, 11.5f), 0,
            "Disco de piedra partido, en el suelo bajo el ensayo. Premia a quien explora la caída segura.");
        Decor(r, new[] { (0f, 0f) }, new (float, float)[0]);
    }

    private static void Brazaletes(Room r)
    {
        var t = r.Root;
        Floor(t, "Zona segura y lanzamiento", -6, 6, 0, 10, 0, _top);
        Floor(t, "Plataforma de recepción", -2, 2, 11, 15, 0, _trialTop);
        Floor(t, "Plataforma del centinela", -6, 6, 15, 18, 0, _top);
        Fall(t, "Pozo del ensayo de impulso", new Vector3(0, -4, 12.5f), new Vector3(14, 1, 5), Mark(t, "Ancla 03", new Vector3(0, 0, 8)), 10);
        Floor(t, "Fondo del pozo de ensayo", -6, 6, 10, 15, -4.5f, _depth, 1);
        Find(t, "Altar brazaletes 1", new Vector3(-3, 0, 5), 90, "O04b", MIProgress.Bracelets1, MIFind.Kind.Bracelets, "Brazaletes", null, 0,
            "Par de brazaletes de impulso. Pieza de fantasía del juego: concentran el empuje del primer impulso.",
            "Impulso nivel 1", "Pulsa Q para impulsarte hacia donde te mueves. Ensáyalo sobre el hueco.");
        // No shortcut lever (2026-10-07 playtest: its door opened onto nothing); the gate stays shut.
        Gate(t, "A02 Acceso del atajo", new Vector3(-5, 0, 9), 90, MIProgress.Shortcut03, false);
        Kit("A05", t, "A05 Boca del atajo", new Vector3(-8.6f, 0, 9), -90, new Vector3(5, 5, 0), Anchor.Bottom);
        Mark(t, "C01 Centinela (personaje pendiente)", new Vector3(0, 0, 16));
        Offering(t, "ofrenda_03", "Medallón calado", "A03", new Vector3(5, 0, 16.6f), 0,
            "Medallón circular con calados, olvidado en el borde de la plataforma del centinela.", 0.45f);
        Decor(r, new[] { (7f, 11f) }, new (float, float)[0]);
        Crystal(t, new Vector3(5.2f, 0, 2.5f));
    }

    private static void Centinelas(Room r)
    {
        var t = r.Root;
        Floor(t, "Módulos 1-2", -6, 6, 0, 22, 0, _top);
        Floor(t, "Módulo final", -9, 9, 22, 44, 0, _top);
        foreach (var (x, z) in new[] { (-2.5f, 15f), (2.5f, 17f) })
        {
            Block(t, "T05 Pilar", new Vector3(x, 2, z), new Vector3(2, 4, 2), _stone);
            Kit("T05", t, "T05 Pilar (modelo)", new Vector3(x, 0, z), 0, new Vector3(2.1f, 4, 2.1f), Anchor.Bottom);
        }
        Mark(t, "C01 Centinela A (personaje pendiente)", new Vector3(-2, 0, 4));
        Mark(t, "C01 Centinela B (personaje pendiente)", new Vector3(2, 0, 8));
        Find(t, "Altar brazaletes 2", new Vector3(-4, 0, 10.5f), 90, "O04b", MIProgress.Bracelets2, MIFind.Kind.Bracelets, "Brazaletes del patio", null, 0,
            "Segundo par de brazaletes. Pieza de fantasía del juego: permiten enlazar un impulso con otro.",
            "Impulso nivel 2", "Pulsa Q otra vez durante el impulso para encadenar dos.");
        Mark(t, "C02a Vigía (personaje pendiente)", new Vector3(0, 0, 19));
        Find(t, "Altar brazaletes 3", new Vector3(4, 0, 21.5f), -90, "O04b", MIProgress.Bracelets3, MIFind.Kind.Bracelets, "Brazaletes del centinela", null, 0,
            "Tercer par de brazaletes. Pieza de fantasía del juego: la tercera cadena quiebra defensas de piedra.",
            "Impulso nivel 3", "Una cadena de tres impulsos (Q Q Q) rompe la defensa del centinela.");
        // Chain practice lane: 16 m straight run with a gold edge, away from the arena.
        Block(t, "Carril de ensayo de cadena", new Vector3(0, 0.012f, 24), new Vector3(16, 0.02f, 1.2f), _metal, collider: false);
        Sentinel(t, new Vector3(0, 0, 35));
        Gate(t, "A01+A02 Salida de combate", new Vector3(0, 0, 43.5f), 0, MIProgress.Shield04, false);
        Wall(t, "Cierre final izq", -9, -1.6f, 43.4f, 44, 4);
        Wall(t, "Cierre final der", 1.6f, 9, 43.4f, 44, 4);
        Offering(t, "ofrenda_04", "Fragmento del muro", "C03d", new Vector3(-8.1f, 0, 27f), 140,
            "Trozo de la decoración del patio, caído en el rincón izquierdo del módulo final.");
        Crystal(t, new Vector3(-5.2f, 0, 1.5f)); Crystal(t, new Vector3(8, 0, 30)); Crystal(t, new Vector3(-8, 0, 40), warm: true);
        Decor(r, new[] { (0f, 0f) }, new (float, float)[0]);
    }

    private static void Pendulos(Room r)
    {
        var t = r.Root;
        Floor(t, "Terraza de preparación", -5, 5, 0, 4, 0, _top);
        Floor(t, "T01 Apoyo previo", -1, 1, 5, 7, 0, _trialTop);
        Floor(t, "Descanso central", -2, 2, 10, 14, 0, _top);
        Floor(t, "T01 Recepción", -1, 1, 17, 19, 0, _trialTop);
        Floor(t, "Terraza final", -5, 5, 20, 24, 0, _top);
        var a = Mark(t, "Ancla 05A", new Vector3(0, 0, 2));
        var b = Mark(t, "Ancla 05B", new Vector3(0, 0, 12));
        Pit(t, -5, 5, 4, 20, new[] { 5f, 8.5f, 15.5f, 19f });
        Fall(t, "Pozo 05 primer tramo", new Vector3(0, -2.5f, 7), new Vector3(12, 1, 6), a, 10);
        Fall(t, "Pozo 05 segundo tramo", new Vector3(0, -2.5f, 17), new Vector3(12, 1, 6), b, 10);
        Pendulum(t, "H02 Péndulo 1", new Vector3(0, 6, 8.5f), 0f, a);
        Pendulum(t, "H02 Péndulo 2", new Vector3(0, 6, 15.5f), 0.5f, b);
        Crystal(t, new Vector3(-4, 0, 1)); Crystal(t, new Vector3(4, 0, 22), warm: true);
        Decor(r, new[] { (4f, 20f) }, new[] { (4f, 20f) });
    }

    private static void Derrumbe(Room r)
    {
        var t = r.Root;
        Floor(t, "Preparación", -6, 6, 0, 4, 0, _top);
        Slab(t, "H03 Losa aislada", new Vector3(0, 0, 5.5f));
        Floor(t, "Refugio A", -5, -1, 5, 9, 0, _top);
        Slab(t, "H03 Losa 1", new Vector3(0, 0, 10)); Slab(t, "H03 Losa 2", new Vector3(0, 0, 13)); Slab(t, "H03 Losa 3", new Vector3(0, 0, 16));
        Floor(t, "Refugio B", 1, 5, 16, 20, 0, _top);
        Floor(t, "Tramo de piedras y salida", -6, 6, 20, 26, 0, _top);
        Pit(t, -6, 6, 4, 20, new[] { 5f, 11f, 13f, 15f, 17f });
        Fall(t, "Pozo 06 inicio", new Vector3(0, -2.5f, 6.5f), new Vector3(14, 1, 5), Mark(t, "Ancla 06 preparación", new Vector3(0, 0, 2)), 10);
        Fall(t, "Pozo 06 secuencia", new Vector3(0, -2.5f, 14.5f), new Vector3(14, 1, 11), Mark(t, "Ancla 06 refugio A", new Vector3(-3, 0, 7)), 10);
        Stalactite(t, "H04 Estalactita A", new Vector3(-1, 0, 20), new Vector3(0, 1, -1.5f), true);
        Stalactite(t, "H04 Estalactita B", new Vector3(1, 0, 23), new Vector3(0, 1, -1.5f), true);
        Lever(t, "H05 Palanca de retorno", new Vector3(4, 0, 24), -90, MIProgress.Corridor06, "Corredor seguro abierto",
            "Puedes volver al inicio o llegar a la antesala sin repetir el derrumbe.");
        Gate(t, "A02 Corredor seguro", new Vector3(5.4f, 0, 24), 90, MIProgress.Corridor06, false);
        Kit("A05", t, "A05 Entrada del corredor", new Vector3(8.6f, 0, 24), 90, new Vector3(5, 5, 0), Anchor.Bottom);
        Offering(t, "ofrenda_05", "Raíz petrificada", "N02c", new Vector3(4.4f, 0, 17), 210,
            "Raíz convertida en piedra al fondo del refugio B, lejos de las sombras de las estalactitas.", 0.8f);
        Crystal(t, new Vector3(-5, 0, 1.5f)); Crystal(t, new Vector3(-4.5f, 0, 7.5f), warm: true); Crystal(t, new Vector3(-5, 0, 25));
        Decor(r, new[] { (22f, 26f) }, new[] { (22f, 26f) });
    }

    private static void Cuerno(Room r)
    {
        var t = r.Root;
        Floor(t, "Suelo de combate y preparación", -7, 7, 0, 7, 0, _top);
        Floor(t, "T01 Apoyo A", -2, 0, 7.5f, 9.5f, 0, _trialTop);
        Floor(t, "T01 Apoyo B", 0, 2, 10.5f, 12.5f, 0, _trialTop);
        Floor(t, "Isla del cuerno", -3, 3, 12.5f, 17.5f, 0, _top);
        Pit(t, -5, 5, 7, 12.5f, new[] { 8f, 10f, 12f });
        Fall(t, "Pozo 07", new Vector3(0, -2.5f, 10), new Vector3(14, 1, 6), Mark(t, "Ancla 07", new Vector3(0, 0, 6)), 10);
        Mark(t, "C01 Centinela (personaje pendiente)", new Vector3(0, 0, 3.5f));
        Find(t, "Altar del cuerno", new Vector3(0, 0, 15.5f), 180, "O04c", MIProgress.Horn, MIFind.Kind.Horn, "Cuerno", "O02", 0.6f,
            "Cuerno curvo de la cámara inferior. Pieza de fantasía del juego: es la llave del soporte junto a la reja de la antesala; no se consume.",
            "Cuerno obtenido", "Llévalo al soporte junto a la reja de la antesala (zona 08).");
        Frame(t, "A01 Arco de fondo", new Vector3(0, 0, 17.2f), 0);
        Kit("N02d", t, "N02d Ruta de vuelta (decorativa)", new Vector3(-6.4f, 0, 5.5f), 90, new Vector3(0, 3, 0), Anchor.Bottom);
        Crystal(t, new Vector3(-6, 0, 1)); Crystal(t, new Vector3(2.4f, 0, 16.8f), warm: true);
        Decor(r, new[] { (12f, 18f) }, new[] { (12f, 18f) });
    }

    private static void Antesala(Room r)
    {
        var t = r.Root;
        Floor(t, "Suelo de la antesala", -7, 7, 0, 12, 0, _top);
        Rest(t, 7, new Vector3(-3, 0.01f, 3));
        HornSocket(t, new Vector3(-2.5f, 0, 8.5f));
        Gate(t, "A01+A02 Reja ritual", new Vector3(0, 0, 10.5f), 0, MIProgress.HornGate, false);
        // Low enough for the arena camera behind them to see the fight; a hidden limit keeps the edge.
        foreach (var (x0, x1) in new[] { (-7f, -1.6f), (1.6f, 7f) })
        {
            Wall(t, "Cierre (muro bajo)", x0, x1, 10.4f, 11, 2.2f);
            var limit = Block(t, "Cierre (límite)", new Vector3((x0 + x1) * .5f, 2.5f, 10.7f), new Vector3(x1 - x0, 5, .6f), _stone);
            limit.GetComponent<MeshRenderer>().enabled = false; limit.layer = 2;
        }
        Crystal(t, new Vector3(-3, 0, 10), warm: true); Crystal(t, new Vector3(3, 0, 10), warm: true);
        Mark(t, "C04 Custodio (personaje pendiente)", new Vector3(4, 0, 6));
        Kit("T03", t, "T03 Hueco cerrado", new Vector3(4, 0.02f, 3), 0, new Vector3(2, 0, 0), Anchor.Top, fitLargest: true);
        Kit("A03", t, "A03 Rejilla cerrada", new Vector3(4, 0.03f, 3), 0, new Vector3(1.6f, 0, 0), Anchor.Bottom, fitLargest: true);
        Offering(t, "ofrenda_06", "Sello de piedra", "H03b", new Vector3(6, 0, 1.2f), 25,
            "Bloque tallado como sello, en la esquina de la antesala junto a la rejilla cerrada.", 0.5f);
        Decor(r, new[] { (3f, 7f) }, new (float, float)[0]);
    }

    private static void Guardian(Room r)
    {
        var t = r.Root;
        Floor(t, "Arena", -9, 9, 0, 16, 0, _top);
        Wall(t, "T06 Límite izq", -9.6f, -9, 0, 16.6f, 6);
        Wall(t, "T06 Límite der", 9, 9.6f, 0, 16.6f, 6);
        Wall(t, "T06 Límite fondo", -9.6f, 9.6f, 16, 16.6f, 6);
        // The camera stands behind the entry: a low parapet keeps the arena in view, and a tall
        // invisible collider on Ignore Raycast keeps the player inside without pulling the camera in.
        foreach (var (x0, x1) in new[] { (-9.6f, -1.6f), (1.6f, 9.6f) })
        {
            Wall(t, "Frente (parapeto)", x0, x1, 0.4f, 1, 1.1f);
            var limit = Block(t, "Frente (límite)", new Vector3((x0 + x1) * .5f, 3, .7f), new Vector3(x1 - x0, 6, .6f), _stone);
            limit.GetComponent<MeshRenderer>().enabled = false; limit.layer = 2;
        }
        var arena = Gate(t, "A01+A02 Cierre de arena", new Vector3(0, 0, 0.7f), 0, "cierre_arena", true);
        Floor(t, "T01 Apoyo lateral izq", -7, -5, 7, 9, 0.8f, _trialTop, 0.8f);
        Floor(t, "T01 Apoyo lateral der", 5, 7, 7, 9, 0.8f, _trialTop, 0.8f);
        Portal(t, "A04 Portal de victoria", new Vector3(7, 0, 13), -90, true, MIProgress.Guardian);
        Idol(t, arena);
        foreach (var (x, z) in new[] { (-8.4f, 15.4f), (8.4f, 15.4f), (-8.4f, 1.8f), (8.4f, 1.8f) })
            Crystal(t, new Vector3(x, 0, z), warm: z > 10);
    }

    // ---------------------------------------------------------------- assemblies

    private static Room NewRoom(Transform parent, int index, string name, Vector3 entry, float yaw, float width, float depth, Vector3? spawn = null, float cameraTurn = 0, Vector2? framing = null)
    {
        var root = new GameObject("Room_" + name.Replace(" ", "_")).transform;
        root.SetParent(parent, false);
        root.position = entry; root.rotation = Quaternion.Euler(0, yaw, 0);
        var volume = root.gameObject.AddComponent<BoxCollider>();
        volume.isTrigger = true; volume.center = new Vector3(0, 2, depth * 0.5f); volume.size = new Vector3(width + 2, 14, depth + 2);
        var zone = root.gameObject.AddComponent<MICameraZone>();
        var so = new SerializedObject(zone);
        so.FindProperty("roomIndex").intValue = index; so.FindProperty("yaw").floatValue = yaw + cameraTurn;
        so.FindProperty("distance").floatValue = framing?.x ?? 0; so.FindProperty("pitch").floatValue = framing?.y ?? 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        var room = new Room { Index = index, Name = name, Root = root, Yaw = yaw, Width = width, Depth = depth };
        room.Spawn = Mark(root, "Spawn " + name.Substring(0, 2), spawn ?? new Vector3(0, 0, 1.5f));
        // Dust in suspension through the room volume, lit by its crystals (guide 7.2).
        MIParticles.Motes(root, new Vector3(0, 2.5f, depth * .5f), new Vector3(width, 4, depth), new Color(.75f, .88f, 1f, .35f), width * depth * .05f);
        // A cold fill light over the middle of the room keeps the floor readable.
        var fill = AddLight(root, "Luz de relleno", new Vector3(0, 7, depth * .5f), new Color(.45f, .62f, .85f), Mathf.Max(width, depth) * 1.1f, 1.1f);
        fill.shadows = LightShadows.None;
        Rooms.Add(room);
        return room;
    }

    // Altar with an independent pickup (guide 3.2): O04a base, a support on top, the find floating
    // above it with its warm halo and sparks; MIFind turns it during the inspection.
    private static void Find(Transform room, string name, Vector3 position, float yaw, string support, string findId, MIFind.Kind kind,
        string displayName, string item, float itemSize, string description, string rewardTitle, string rewardText)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Kit("O04a", root, "O04a Base", Vector3.zero, 0, new Vector3(1.5f, 0.9f, 1f), Anchor.Bottom, solid: true);
        Kit(support, root, support + " Soporte", new Vector3(0, 0.9f, 0), 0, new Vector3(support == "O04d" ? 0.9f : 1.2f, 0, 0), Anchor.Bottom);
        var pickup = new GameObject("Hallazgo (pieza independiente)").transform; pickup.SetParent(root, false);
        pickup.localPosition = new Vector3(0, 1.3f, 0);
        if (support == "O04b")
        {
            // One reward made of two meshes, 0.2 m apart.
            Kit("O01a", pickup, "O01a Brazalete izquierdo", new Vector3(-0.2f, -0.08f, 0), 0, new Vector3(0.24f, 0, 0), Anchor.Center, fitLargest: true);
            Kit("O01b", pickup, "O01b Brazalete derecho", new Vector3(0.2f, -0.08f, 0), 0, new Vector3(0.24f, 0, 0), Anchor.Center, fitLargest: true);
        }
        else if (item != null)
            Kit(item, pickup, item + " Hallazgo", Vector3.zero, 0, new Vector3(itemSize, 0, 0), Anchor.Center, fitLargest: true);
        pickup.gameObject.AddComponent<MIFloat>();
        var halo = AddLight(root, "Halo del hallazgo", new Vector3(0, 2.2f, 0), new Color(1f, 0.78f, 0.42f), 6, 2.4f);
        halo.gameObject.AddComponent<MIGlow>();
        var sparks = MIParticles.Sparks(root, new Vector3(0, 1.3f, 0), new Color(1f, .8f, .45f, .8f));
        var find = root.gameObject.AddComponent<MIFind>();
        var so = new SerializedObject(find);
        so.FindProperty("findId").stringValue = findId; so.FindProperty("kind").enumValueIndex = (int)kind;
        so.FindProperty("displayName").stringValue = displayName; so.FindProperty("item").objectReferenceValue = pickup;
        so.FindProperty("halo").objectReferenceValue = halo; so.FindProperty("sparks").objectReferenceValue = sparks;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("rewardTitle").stringValue = rewardTitle; so.FindProperty("rewardText").stringValue = rewardText;
        so.FindProperty("range").floatValue = 2.2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Optional offering for the cultural archive: a small piece on a low pedestal, a cold sparkle.
    private static void Offering(Transform room, string id, string displayName, string model, Vector3 position, float yaw, string where, float size = 0.4f)
    {
        var root = new GameObject("Ofrenda · " + displayName).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Kit("T05", root, "T05 Peana", Vector3.zero, 0, new Vector3(0.5f, 0.55f, 0.5f), Anchor.Bottom, solid: true);
        var pickup = new GameObject("Ofrenda (pieza independiente)").transform; pickup.SetParent(root, false);
        pickup.localPosition = new Vector3(0, 0.55f + size * .5f + .08f, 0);
        Kit(model, pickup, model + " Ofrenda", Vector3.zero, 0, new Vector3(size, 0, 0), Anchor.Center, fitLargest: true);
        pickup.gameObject.AddComponent<MIFloat>();
        var halo = AddLight(root, "Halo de la ofrenda", new Vector3(0, 1.4f, 0), new Color(.55f, .85f, 1f), 3.5f, 1.4f);
        halo.gameObject.AddComponent<MIGlow>();
        var sparks = MIParticles.Sparks(root, pickup.localPosition, new Color(.6f, .9f, 1f, .7f));
        var find = root.gameObject.AddComponent<MIFind>();
        var so = new SerializedObject(find);
        so.FindProperty("findId").stringValue = id; so.FindProperty("kind").enumValueIndex = (int)MIFind.Kind.Offering;
        so.FindProperty("displayName").stringValue = displayName; so.FindProperty("item").objectReferenceValue = pickup;
        so.FindProperty("halo").objectReferenceValue = halo; so.FindProperty("sparks").objectReferenceValue = sparks;
        so.FindProperty("description").stringValue = where + "\n\nPieza de fantasía creada para el juego; no es una reconstrucción arqueológica.";
        so.FindProperty("range").floatValue = 1.8f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // H05: the Tripo lever as base and a separate handle on a pivot that MILever pulls.
    private static void Lever(Transform room, string name, Vector3 position, float yaw, string flag, string doneTitle, string doneText)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Kit("H05", root, "H05 Base", Vector3.zero, 0, new Vector3(0, 1.0f, 0), Anchor.Bottom, solid: true);
        var pivot = new GameObject("Mango (pivote)").transform; pivot.SetParent(root, false); pivot.localPosition = new Vector3(0, 1.0f, 0);
        Block(pivot, "Mango", new Vector3(0, 0.45f, 0), new Vector3(0.12f, 0.9f, 0.12f), _metal, collider: false);
        Block(pivot, "Remate", new Vector3(0, 0.92f, 0), new Vector3(0.24f, 0.18f, 0.24f), _metal, collider: false);
        AddLight(root, "Luz de la palanca", new Vector3(0, 2.0f, 0.6f), new Color(1f, .8f, .5f), 3.5f, 1.2f);
        var lever = root.gameObject.AddComponent<MILever>();
        var so = new SerializedObject(lever);
        so.FindProperty("flagId").stringValue = flag; so.FindProperty("handle").objectReferenceValue = pivot;
        so.FindProperty("displayName").stringValue = "Palanca"; so.FindProperty("doneTitle").stringValue = doneTitle;
        so.FindProperty("doneText").stringValue = doneText; so.FindProperty("range").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // O04e v2 rest disc: E heals and fixes the checkpoint; the spawn stands beside it.
    private static void Rest(Transform room, int index, Vector3 position)
    {
        var root = new GameObject("O04e Descanso · checkpoint " + (index + 1).ToString("00")).transform; root.SetParent(room, false);
        root.localPosition = position;
        Kit("O04e", root, "O04e Disco", Vector3.zero, 0, new Vector3(1.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        var spawn = Mark(root, "Spawn del descanso", new Vector3(1.4f, 0, 0));
        var glow = AddLight(root, "Luz del descanso", new Vector3(0, 1.4f, 0), new Color(.45f, .9f, .8f), 4.5f, 1.4f);
        glow.gameObject.AddComponent<MIGlow>();
        MIParticles.Sparks(root, new Vector3(0, .4f, 0), new Color(.5f, .95f, .85f, .55f));
        var rest = root.gameObject.AddComponent<MIRest>();
        var so = new SerializedObject(rest);
        so.FindProperty("room").intValue = index; so.FindProperty("spawn").objectReferenceValue = spawn;
        so.FindProperty("glow").objectReferenceValue = glow; so.FindProperty("displayName").stringValue = "Descanso";
        so.FindProperty("range").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Horn socket of 08: empty support until the horn is placed; a copy of O02 appears on it.
    private static void HornSocket(Transform room, Vector3 position)
    {
        var root = new GameObject("Receptor del cuerno").transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, 180, 0);
        Kit("O04a", root, "O04a Base", Vector3.zero, 0, new Vector3(1.5f, 0.9f, 1f), Anchor.Bottom, solid: true);
        Kit("O04c", root, "O04c Soporte", new Vector3(0, 0.9f, 0), 0, new Vector3(1.2f, 0, 0), Anchor.Bottom);
        var horn = Kit("O02", root, "O02 Cuerno colocado", new Vector3(0, 1.3f, 0), 0, new Vector3(0.6f, 0, 0), Anchor.Center, fitLargest: true);
        var glow = AddLight(root, "Luz del receptor", new Vector3(0, 2.2f, 0), new Color(1f, .75f, .4f), 5, .6f);
        var socket = root.gameObject.AddComponent<MIHornSocket>();
        var so = new SerializedObject(socket);
        so.FindProperty("placedHorn").objectReferenceValue = horn; so.FindProperty("glow").objectReferenceValue = glow;
        so.FindProperty("displayName").stringValue = "Soporte del cuerno"; so.FindProperty("range").floatValue = 2.2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // A01 frame (stone jambs and lintel, the A01a model export is broken) with an A02 leaf on a
    // hinge 1.1 m to the left of the passage centre.
    private static MIGate Gate(Transform room, string name, Vector3 position, float yaw, string flag, bool startOpen)
    {
        var root = Frame(room, name, position, yaw);
        var hinge = new GameObject("Bisagra").transform; hinge.SetParent(root, false); hinge.localPosition = new Vector3(-1.1f, 0, 0);
        // The Tripo leaf is modelled in the YZ plane: a quarter turn puts it across the passage.
        var leaf = Kit("A02", hinge, "A02 Hoja", new Vector3(1.1f, 0, 0), 0, new Vector3(2.2f, 3f, 0), Anchor.Bottom, tilt: Quaternion.Euler(0, 90, 0));
        var box = leaf.AddComponent<BoxCollider>(); box.center = new Vector3(0, 1.5f, 0); box.size = new Vector3(2.2f, 3f, 0.25f);
        // Barred leaf: it stops the player but the camera sees through it, so it is not an obstruction.
        foreach (var t in leaf.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2;
        var gate = root.gameObject.AddComponent<MIGate>();
        var so = new SerializedObject(gate);
        so.FindProperty("flagId").stringValue = flag; so.FindProperty("hinge").objectReferenceValue = hinge;
        so.FindProperty("startOpen").boolValue = startOpen;
        so.ApplyModifiedPropertiesWithoutUndo();
        return gate;
    }

    private static Transform Frame(Transform room, string name, Vector3 position, float yaw)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Block(root, "Jamba izq", new Vector3(-1.45f, 1.85f, 0), new Vector3(0.5f, 3.7f, 0.6f), _frame);
        Block(root, "Jamba der", new Vector3(1.45f, 1.85f, 0), new Vector3(0.5f, 3.7f, 0.6f), _frame);
        Block(root, "Dintel", new Vector3(0, 3.5f, 0), new Vector3(3.4f, 0.5f, 0.6f), _frame);
        // Thin frames stop the player but do not pull the room camera onto the player's back.
        foreach (Transform part in root) part.gameObject.layer = 2;
        return root;
    }

    private static void Portal(Transform room, string name, Vector3 position, float yaw, bool active, string requiredFlag)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Kit("A04", root, "A04 Marco", Vector3.zero, 0, new Vector3(4.2f, 0, 0), Anchor.Bottom);
        // Effect plane and interaction volume are separate from the frame (guide A04).
        var veil = GameObject.CreatePrimitive(PrimitiveType.Quad);
        veil.name = "Plano de efecto"; veil.transform.SetParent(root, false);
        veil.transform.localPosition = new Vector3(0, 1.75f, 0); veil.transform.localScale = new Vector3(2.5f, 3.3f, 1);
        UnityEngine.Object.DestroyImmediate(veil.GetComponent<Collider>());
        veil.GetComponent<Renderer>().sharedMaterial = _veil;
        var glow = AddLight(root, "Luz del portal", new Vector3(0, 1.8f, 0.8f), new Color(.4f, .7f, 1f), 7, 2.2f);
        MIParticles.Sparks(root, new Vector3(0, 1.6f, 0.2f), new Color(.5f, .8f, 1f, .6f));
        var portal = root.gameObject.AddComponent<MIPortal>();
        var so = new SerializedObject(portal);
        so.FindProperty("active").boolValue = active; so.FindProperty("effect").objectReferenceValue = veil;
        so.FindProperty("requiredFlag").stringValue = requiredFlag ?? ""; so.FindProperty("glow").objectReferenceValue = glow;
        so.FindProperty("range").floatValue = 1.8f; // the arrival spawn (2.3 m away) stays out of range
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Pendulum(Transform room, string name, Vector3 pivot, float phase, Transform anchor)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false); root.localPosition = pivot;
        Block(root, "Soporte de techo", new Vector3(0, 0.3f, 0), new Vector3(1.2f, 0.6f, 1.2f), _stone, collider: false);
        var swing = new GameObject("Eje (oscila en Z)").transform; swing.SetParent(root, false);
        var pendulum = swing.gameObject.AddComponent<MIPendulum>();
        var so = new SerializedObject(pendulum); so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
        Kit("H02", swing, "H02 Péndulo", Vector3.zero, 0, new Vector3(0, 5.4f, 0), Anchor.Top);
        // Hit volume around the 1.2 x 1.8 x 0.6 weight, moving with it.
        var hit = new GameObject("Volumen de golpe", typeof(BoxCollider), typeof(Rigidbody)).transform;
        hit.SetParent(swing, false); hit.localPosition = new Vector3(0, -4.6f, 0);
        var box = hit.GetComponent<BoxCollider>(); box.isTrigger = true; box.size = new Vector3(1.4f, 1.8f, 0.9f);
        hit.GetComponent<Rigidbody>().isKinematic = true;
        SetAnchor(hit.gameObject.AddComponent<MIFallZone>(), anchor, 15);
    }

    // H03: root at the top surface; solid collider and H03a visual are separate children.
    private static void Slab(Transform room, string name, Vector3 center)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false); root.localPosition = center;
        var solid = Block(root, "Superficie sólida", new Vector3(0, -0.25f, 0), new Vector3(2, 0.5f, 2), _stone, topMaterial: _trialTop);
        var visualRoot = new GameObject("Visual").transform; visualRoot.SetParent(root, false);
        Kit("H03a", visualRoot, name + " (modelo)", new Vector3(0, 0.02f, 0), 0, new Vector3(2, 0, 2), Anchor.Top);
        solid.GetComponent<MeshRenderer>().enabled = false;
        solid.transform.SetParent(root, true);
        var slab = root.gameObject.AddComponent<MISlab>();
        var so = new SerializedObject(slab);
        so.FindProperty("solid").objectReferenceValue = solid.GetComponent<Collider>(); so.FindProperty("visual").objectReferenceValue = visualRoot;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // H04: root on the floor where it will land; the rock waits 3 m up; a red shadow announces it.
    private static MIStalactite Stalactite(Transform parent, string name, Vector3 floor, Vector3 triggerCenter, bool automatic, float height = 3f)
    {
        var root = new GameObject(name).transform; root.SetParent(parent, false); root.localPosition = floor;
        var rock = Kit("H04", root, "H04 Roca", new Vector3(0, height, 0), 0, new Vector3(0, 2, 0), Anchor.Bottom);
        var shadow = new GameObject("Sombra de aviso").transform; shadow.SetParent(root, false); shadow.localPosition = new Vector3(0, 0.02f, 0);
        Disc(shadow, "Disco", 1.1f, _warning);
        var stalactite = root.gameObject.AddComponent<MIStalactite>();
        var so = new SerializedObject(stalactite);
        so.FindProperty("rock").objectReferenceValue = rock.transform; so.FindProperty("shadow").objectReferenceValue = shadow;
        so.FindProperty("triggerCenter").vector3Value = triggerCenter; so.FindProperty("automatic").boolValue = automatic;
        so.ApplyModifiedPropertiesWithoutUndo();
        return stalactite;
    }

    // Provisional C03a: stone body, a visible shield in front, a crystal core and a ground pulse.
    private static void Sentinel(Transform room, Vector3 position)
    {
        var root = new GameObject("Centinela de escudo (provisional)").transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, 180, 0);
        var body = new GameObject("Cuerpo").transform; body.SetParent(root, false);
        Kit("T08d", body, "T08d Cuerpo de piedra", Vector3.zero, 0, new Vector3(0, 2.4f, 0), Anchor.Bottom);
        var shield = Kit("C03c", root, "C03c Escudo", new Vector3(0, 1.25f, 0.95f), 0, new Vector3(1.5f, 0, 0), Anchor.Center, fitLargest: true);
        var core = Kit("N05", body, "N05 Núcleo", new Vector3(0, 2.5f, 0.25f), 0, new Vector3(0.55f, 0, 0), Anchor.Center, fitLargest: true);
        var light = AddLight(core.transform, "Luz del núcleo", new Vector3(0, 0, 0.4f), new Color(.45f, .8f, 1f), 6, 2.2f);
        var ring = new GameObject("Pulso").transform; ring.SetParent(root, false); ring.localPosition = new Vector3(0, 0.03f, 0);
        Disc(ring, "Disco", 1f, _warning);
        var box = root.gameObject.AddComponent<BoxCollider>(); box.isTrigger = true; box.center = new Vector3(0, 1.4f, 0.2f); box.size = new Vector3(2.4f, 2.8f, 2.6f);
        var rb = root.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true;
        Block(root, "Volumen sólido", new Vector3(0, 1.2f, 0), new Vector3(1.4f, 2.4f, 1.4f), _stone).GetComponent<MeshRenderer>().enabled = false;
        var sentinel = root.gameObject.AddComponent<MIShieldSentinel>();
        var so = new SerializedObject(sentinel);
        so.FindProperty("shield").objectReferenceValue = shield; so.FindProperty("body").objectReferenceValue = body;
        so.FindProperty("core").objectReferenceValue = light; so.FindProperty("pulseRing").objectReferenceValue = ring;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Provisional C05: a 4.2 m idol at the back of the arena, its crystal core facing the entry.
    private static void Idol(Transform room, MIGate arena)
    {
        var root = new GameObject("Guardián del fondo (provisional)").transform; root.SetParent(room, false);
        var body = new GameObject("Cuerpo").transform; body.SetParent(root, false); body.localPosition = new Vector3(0, 0, 11.5f);
        body.localRotation = Quaternion.Euler(0, 180, 0);
        Kit("T08d", body, "T08d Cuerpo del ídolo", Vector3.zero, 0, new Vector3(0, 4.2f, 0), Anchor.Bottom);
        Kit("T05", body, "T05 Hombro izq", new Vector3(-1.6f, 2.4f, 0.2f), 0, new Vector3(0.9f, 1.8f, 0.9f), Anchor.Bottom);
        Kit("T05", body, "T05 Hombro der", new Vector3(1.6f, 2.4f, 0.2f), 0, new Vector3(0.9f, 1.8f, 0.9f), Anchor.Bottom);
        Block(body, "Volumen sólido", new Vector3(0, 2.1f, 0), new Vector3(3, 4.2f, 2.2f), _stone).GetComponent<MeshRenderer>().enabled = false;
        var core = Kit("N05", root, "N05 Núcleo", new Vector3(0, 2.4f, 10.2f), 0, new Vector3(0.95f, 0, 0), Anchor.Center, fitLargest: true);
        var light = AddLight(core.transform, "Luz del núcleo", new Vector3(0, 0, -0.6f), new Color(.4f, .7f, 1f), 9, 2f);
        var hit = root.gameObject.AddComponent<BoxCollider>(); hit.isTrigger = true; hit.center = new Vector3(0, 2.2f, 10.0f); hit.size = new Vector3(2.2f, 2.6f, 2.2f);
        hit.enabled = false;
        var rb = root.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true;
        Transform Sweep(string name, float x)
        {
            var marker = new GameObject(name).transform; marker.SetParent(root, false); marker.localPosition = new Vector3(0, 0.03f, 11);
            Block(marker, "Zona", new Vector3(x * 4.5f, 0, -4.5f), new Vector3(9, 0.02f, 9), _warning, collider: false);
            return marker;
        }
        var left = Sweep("Barrido izquierdo", -1); var right = Sweep("Barrido derecho", 1);
        var waveObject = new GameObject("Onda"); waveObject.transform.SetParent(root, false);
        var wave = waveObject.AddComponent<LineRenderer>(); wave.sharedMaterial = _wave; wave.widthMultiplier = 0.45f; wave.loop = false;
        wave.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var stones = new[] { Stalactite(room, "H04 Piedra del guardián A", new Vector3(-4, 0, 7), Vector3.zero, false, 5f),
                             Stalactite(room, "H04 Piedra del guardián B", new Vector3(4, 0, 9), Vector3.zero, false, 5f) };
        var guardian = root.gameObject.AddComponent<MIGuardian>();
        var so = new SerializedObject(guardian);
        so.FindProperty("body").objectReferenceValue = body; so.FindProperty("core").objectReferenceValue = core.transform;
        so.FindProperty("coreLight").objectReferenceValue = light; so.FindProperty("coreHit").objectReferenceValue = hit;
        so.FindProperty("arenaGate").objectReferenceValue = arena;
        so.FindProperty("sweepLeft").objectReferenceValue = left; so.FindProperty("sweepRight").objectReferenceValue = right;
        so.FindProperty("wave").objectReferenceValue = wave;
        var list = so.FindProperty("stones"); list.arraySize = stones.Length;
        for (int i = 0; i < stones.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = stones[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Pit floor 3 m down with H01 spike modules (2 x 2 m) in the given rows.
    private static void Pit(Transform room, float x0, float x1, float z0, float z1, float[] rows)
    {
        Floor(room, "Fondo del pozo", x0, x1, z0, z1, -3, _depth, 1);
        foreach (float z in rows)
            for (float x = x0 + 1; x <= x1 - 1 + 0.01f; x += 2)
                Kit("H01", room, "H01 Pinchos", new Vector3(x, -3, z), (int)(x * 7 + z * 3) % 4 * 90, new Vector3(2, 1.2f, 2), Anchor.Bottom);
        var glow = AddLight(room, "Brillo del pozo", new Vector3((x0 + x1) * .5f, -1.6f, (z0 + z1) * .5f), new Color(.9f, .25f, .15f), Mathf.Max(x1 - x0, z1 - z0) * .7f, 0.8f);
        glow.shadows = LightShadows.None;
    }

    private static void Fall(Transform room, string name, Vector3 center, Vector3 size, Transform anchor, float damage)
    {
        var zone = new GameObject(name, typeof(BoxCollider)).transform;
        zone.SetParent(room, false); zone.localPosition = center;
        var box = zone.GetComponent<BoxCollider>(); box.isTrigger = true; box.size = size;
        SetAnchor(zone.gameObject.AddComponent<MIFallZone>(), anchor, damage);
    }
    private static void SetAnchor(MIFallZone zone, Transform anchor, float damage)
    {
        var so = new SerializedObject(zone); so.FindProperty("anchor").objectReferenceValue = anchor;
        so.FindProperty("damage").floatValue = damage; so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Crystal(Transform room, Vector3 position, bool warm = false)
    {
        var c = Kit("N05", room, "N05 Piedra de luz", position, (int)(position.x * 40) % 360, new Vector3(0.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        var light = AddLight(c.transform, "Luz", new Vector3(0, 0.9f, 0), warm ? new Color(1f, 0.72f, 0.4f) : new Color(0.45f, 0.8f, 1f), 7, 1.6f);
        light.gameObject.AddComponent<MIGlow>();
    }

    // Rocks, stalagmites and roots along the side edges, outside the walkable floor (guide 4:
    // «T08 rocas y N02 raíces, separación mínima de 1 m del carril»); openings are skipped.
    private static void Decor(Room room, (float, float)[] skipLeft, (float, float)[] skipRight)
    {
        string[] rocks = { "T08a", "T08b", "T08d", "T08a" };
        int i = 0;
        foreach (int side in new[] { -1, 1 })
        {
            var skips = side < 0 ? skipLeft : skipRight;
            for (float z = 1.5f; z < room.Depth - 0.5f; z += 3.2f, i++)
            {
                if (skips.Any(s => z > s.Item1 - 1 && z < s.Item2 + 1)) continue;
                string id = rocks[i % rocks.Length];
                float size = 1.4f + (i * 37 % 10) * .12f;
                var local = new Vector3(side * (room.Width * .5f + 0.9f + (i % 3) * .3f), -0.6f, z);
                Kit(id, _decor, id + " " + room.Name.Substring(0, 2), room.Root.TransformPoint(local), room.Yaw + i * 53, new Vector3(0, size + 1.2f, 0), Anchor.Bottom);
                if (i % 3 == 0)
                    MIParticles.Drips(room.Root, local + new Vector3(-side * .4f, 5f, 0), new Vector3(.8f, .1f, .8f));
            }
        }
        // Hanging stalactites high over the back corners: depth without hiding the lanes.
        foreach (int side in new[] { -1, 1 })
            Kit("H04", _decor, "H04 Decorativa " + room.Name.Substring(0, 2), room.Root.TransformPoint(new Vector3(side * room.Width * .5f, 8.5f, room.Depth - 1)), room.Yaw, new Vector3(0, 2.6f, 0), Anchor.Top);
    }

    // ---------------------------------------------------------------- player, light, cavern

    private static void Player(Transform world)
    {
        var spawn = Rooms[0].Spawn;
        var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = "Player";
        playerObject.layer = 2; // Ignore Raycast: kept out of the camera obstruction test.
        playerObject.transform.position = spawn.position + Vector3.up * 1.05f;
        playerObject.transform.rotation = Rooms[0].Root.rotation;
        UnityEngine.Object.DestroyImmediate(playerObject.GetComponent<CapsuleCollider>());
        playerObject.GetComponent<Renderer>().sharedMaterial = _marker;
        var character = playerObject.AddComponent<CharacterController>();
        character.height = 2; character.radius = 0.45f; character.stepOffset = 0.35f; character.slopeLimit = 45;
        var player = playerObject.AddComponent<PlayerController>();
        playerObject.AddComponent<Health>();
        // A soft lamp travels with the player so the path ahead always reads in the dark.
        AddLight(playerObject.transform, "Luz del jugador", new Vector3(0, 2.4f, 0.4f), new Color(1f, .86f, .7f), 6.5f, 0.9f).shadows = LightShadows.None;

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ExplorationOrbitCamera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.nearClipPlane = 0.1f; camera.farClipPlane = 180; camera.fieldOfView = 60;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.035f, 0.05f, 0.075f);
        var orbit = cameraObject.GetComponent<ExplorationOrbitCamera>();
        var so = new SerializedObject(orbit); so.FindProperty("target").objectReferenceValue = playerObject.transform; so.FindProperty("distance").floatValue = 8;
        so.ApplyModifiedPropertiesWithoutUndo();
        cameraObject.transform.SetPositionAndRotation(playerObject.transform.position + Rooms[0].Root.rotation * new Vector3(0, 5, -7),
            Quaternion.Euler(30, Rooms[0].Yaw, 0));
        so = new SerializedObject(player);
        so.FindProperty("enableSprint").boolValue = true;
        so.FindProperty("enableExplorationDash").boolValue = true; // Q dash, Shift runs (guide 3.1)
        so.FindProperty("movementReference").objectReferenceValue = cameraObject.transform;
        so.ApplyModifiedPropertiesWithoutUndo();
        Visual(playerObject);

        var runtime = new GameObject("Runtime", typeof(MundoInferiorBlockout));
        runtime.transform.SetParent(world, false);
        so = new SerializedObject(runtime.GetComponent<MundoInferiorBlockout>());
        so.FindProperty("player").objectReferenceValue = player;
        so.FindProperty("orbitCamera").objectReferenceValue = orbit;
        so.FindProperty("showHelp").boolValue = false;
        var spawns = so.FindProperty("roomSpawns"); var names = so.FindProperty("roomNames");
        spawns.arraySize = Rooms.Count; names.arraySize = Rooms.Count;
        for (int i = 0; i < Rooms.Count; i++)
        {
            spawns.GetArrayElementAtIndex(i).objectReferenceValue = Rooms[i].Spawn;
            names.GetArrayElementAtIndex(i).stringValue = Rooms[i].Name;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Same fitting as NemequenePlayerSetup: model height = capsule, Animator origin on its bottom.
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
        if (b.size.y > 0.01f) visual.transform.localScale = Vector3.one * (cc.height * 0.98f / b.size.y);
        float bottom = player.transform.TransformPoint(cc.center).y - cc.height * 0.5f - cc.skinWidth;
        visual.transform.position = new Vector3(visual.transform.position.x, bottom, visual.transform.position.z);
    }

    // Blue-grey cavern: dim cold key light, trilight ambient and deep blue mist; warm accents
    // come from altars, rests and the guardian's core (guide 7.2).
    private static void Atmosphere(Transform parent)
    {
        var sun = new GameObject("Luz de caverna", typeof(Light)).GetComponent<Light>();
        sun.transform.SetParent(parent, false);
        sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(62, 25, 0);
        sun.color = new Color(0.55f, 0.68f, 0.92f); sun.intensity = 0.55f; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.65f;
        RenderSettings.skybox = null;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.24f, 0.33f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.15f, 0.19f, 0.25f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.06f, 0.08f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.035f, 0.05f, 0.075f); RenderSettings.fogStartDistance = 22; RenderSettings.fogEndDistance = 95;
    }

    // Far cavern shell around the whole map, never near a route: walls and a dark abyss floor.
    private static void Cavern(Transform parent)
    {
        var shell = Group(parent, "Caverna lejana");
        Block(shell, "Fondo del abismo", new Vector3(56, -42, 40), new Vector3(220, 2, 200), _depth, collider: false);
        var walls = new[] { (new Vector3(-22, -18, 40), 90f), (new Vector3(134, -18, 40), -90f), (new Vector3(56, -18, -22), 0f), (new Vector3(56, -18, 102), 180f) };
        foreach (var (position, yaw) in walls)
            for (int k = -2; k <= 2; k++)
            {
                var offset = Quaternion.Euler(0, yaw, 0) * new Vector3(k * 26, 0, 0);
                Kit(k % 2 == 0 ? "T06b" : "T06c", shell, "T06 Pared de caverna", position + offset, yaw + k * 17, new Vector3(28, 34, 0), Anchor.Bottom);
            }
    }

    // ---------------------------------------------------------------- graybox helpers

    private static Transform Group(Transform parent, string name) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }

    private static Transform Mark(Transform parent, string name, Vector3 localPosition)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = localPosition; return t;
    }

    private static Light AddLight(Transform parent, string name, Vector3 localPosition, Color color, float range, float intensity)
    {
        var light = new GameObject(name, typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(parent, false); light.transform.localPosition = localPosition;
        light.type = LightType.Point; light.color = color; light.range = range; light.intensity = intensity;
        return light;
    }

    // Walkable box between x0..x1 / z0..z1 (room space) with its top at `top`.
    private static GameObject Floor(Transform parent, string name, float x0, float x1, float z0, float z1, float top, Material material, float thickness = 2)
        => Block(parent, name, new Vector3((x0 + x1) * 0.5f, top - thickness * 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, thickness, z1 - z0),
            material == _depth ? _depth : _stone, topMaterial: material);

    private static GameObject Wall(Transform parent, string name, float x0, float x1, float z0, float z1, float height)
        => Block(parent, name, new Vector3((x0 + x1) * 0.5f, height * 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, height, z1 - z0), _stone);

    // Box with UVs in metres (2 m per texture tile) so stone and slabs keep their scale on any
    // size; the top face can use its own material (walkable surface).
    private static GameObject Block(Transform parent, string name, Vector3 localCenter, Vector3 size, Material material, bool collider = true, Material topMaterial = null)
    {
        var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.isStatic = true;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localCenter;
        obj.GetComponent<MeshFilter>().sharedMesh = BoxMesh(size);
        obj.GetComponent<MeshRenderer>().sharedMaterials = new[] { topMaterial ?? material, material };
        if (collider) obj.AddComponent<BoxCollider>().size = size;
        return obj;
    }

    private static Mesh BoxMesh(Vector3 size)
    {
        string key = size.x.ToString("0.###") + "x" + size.y.ToString("0.###") + "x" + size.z.ToString("0.###");
        if (Meshes.TryGetValue(key, out var cached)) return cached;
        var h = size * 0.5f;
        var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>();
        var top = new List<int>(); var sides = new List<int>();
        void Face(Vector3 n, Vector3 u, Vector3 v, float du, float dv, List<int> tris)
        {
            int i = vertices.Count;
            Vector3 c = Vector3.Scale(n, h);
            Vector3 hu = u * (Vector3.Scale(u, h).magnitude), hv = v * (Vector3.Scale(v, h).magnitude);
            vertices.Add(c - hu - hv); vertices.Add(c - hu + hv); vertices.Add(c + hu + hv); vertices.Add(c + hu - hv);
            for (int k = 0; k < 4; k++) normals.Add(n);
            const float tile = 0.5f; // 2 m per texture repeat
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(0, dv * tile)); uvs.Add(new Vector2(du * tile, dv * tile)); uvs.Add(new Vector2(du * tile, 0));
            tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z, top);
        Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z, sides);
        Face(Vector3.forward, Vector3.left, Vector3.up, size.x, size.y, sides);
        Face(Vector3.back, Vector3.right, Vector3.up, size.x, size.y, sides);
        Face(Vector3.right, Vector3.forward, Vector3.up, size.z, size.y, sides);
        Face(Vector3.left, Vector3.back, Vector3.up, size.z, size.y, sides);
        var mesh = new Mesh { name = "MI_Bloque_" + key };
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2; mesh.SetTriangles(top, 0); mesh.SetTriangles(sides, 1);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        Meshes[key] = mesh;
        return mesh;
    }

    // Flat disc (warning shadows, pulse rings) of the given radius.
    private static void Disc(Transform parent, string name, float radius, Material material)
    {
        var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.transform.SetParent(parent, false);
        string key = "disc" + radius;
        if (!Meshes.TryGetValue(key, out var mesh))
        {
            const int segments = 40;
            var v = new List<Vector3> { Vector3.zero }; var tris = new List<int>();
            for (int i = 0; i <= segments; i++) { float a = i * Mathf.PI * 2 / segments; v.Add(new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius); }
            for (int i = 1; i <= segments; i++) tris.AddRange(new[] { 0, i + 1, i });
            mesh = new Mesh { name = "MI_Disco" }; mesh.SetVertices(v); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            Meshes[key] = mesh;
        }
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // Walkway through the points (world space): flat runs, ramps where the height changes, square
    // pads at the corners so turns have no gaps. A pad next to a ramp would stand above the slope
    // as a step the controller cannot climb, so corners must join flat runs.
    private static void Walkway(Transform parent, string name, float width, params Vector3[] points)
    {
        var root = Group(parent, name);
        for (int i = 0; i < points.Length; i++)
        {
            Mark(root, "Punto " + i, points[i]);
            if (i > 0 && i < points.Length - 1)
            {
                Vector3 inDir = points[i] - points[i - 1], outDir = points[i + 1] - points[i];
                if (Vector2.Angle(new Vector2(inDir.x, inDir.z), new Vector2(outDir.x, outDir.z)) > 1f)
                {
                    if (Mathf.Abs(inDir.y) > 0.01f || Mathf.Abs(outDir.y) > 0.01f) Debug.LogWarning("MI_BLOCKOUT: esquina junto a rampa en " + name + " punto " + i);
                    Block(root, "Rellano " + i, points[i] - Vector3.up * 0.5f, new Vector3(width, 1, width), _stone, topMaterial: _top);
                }
            }
            if (i == 0) continue;
            Vector3 a = points[i - 1], b = points[i], d = b - a;
            if (new Vector2(d.x, d.z).magnitude < 0.01f) continue;
            var rotation = Quaternion.LookRotation(d);
            var segment = Block(root, "Tramo " + i, Vector3.zero, new Vector3(width, 0.5f, d.magnitude + 0.2f), _stone, topMaterial: _top);
            segment.transform.rotation = rotation;
            segment.transform.position = (a + b) * 0.5f - rotation * Vector3.up * 0.25f;
        }
    }

    private static Material Textured(string name, string texture, Color tint, float smoothness)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.shader = Shader.Find("Universal Render Pipeline/Lit");
        string albedo = TextureFolder + "/" + texture + "_albedo.png", normal = TextureFolder + "/" + texture + "_normal.png";
        var normalImporter = AssetImporter.GetAtPath(normal) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap) { normalImporter.textureType = TextureImporterType.NormalMap; normalImporter.SaveAndReimport(); }
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
        material.SetColor("_BaseColor", tint);
        var bump = AssetDatabase.LoadAssetAtPath<Texture2D>(normal);
        if (bump != null) { material.SetTexture("_BumpMap", bump); material.SetFloat("_BumpScale", 1f); material.EnableKeyword("_NORMALMAP"); }
        material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Metallic", 0);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material Flat(string name, Color color, Color? emission = null, float transparent = 0, float smoothness = 0.12f, float metallic = 0)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", transparent > 0 ? new Color(color.r, color.g, color.b, transparent) : color);
        material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Metallic", metallic);
        if (emission.HasValue) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission.Value); material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
        if (transparent > 0)
        {
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    // Saved additive particle material with a soft dot texture (scene particles keep it).
    private static Material ParticleMaterial()
    {
        string texturePath = TextureFolder + "/MI_Punto.png";
        if (!File.Exists(texturePath))
        {
            var dot = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                    float a = Mathf.Clamp01(1 - d); dot.SetPixel(x, y, new Color(1, 1, 1, a * a));
                }
            Directory.CreateDirectory(TextureFolder);
            File.WriteAllBytes(texturePath, dot.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(dot);
            AssetDatabase.ImportAsset(texturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath); importer.alphaIsTransparency = true; importer.SaveAndReimport();
        }
        Directory.CreateDirectory("Assets/_Game/Resources");
        string path = "Assets/_Game/Resources/MIParticulas.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader;
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 2);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_ZWrite", 0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 3000;
        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- kit models

    // Places the decimated kit model <ID>_Optimizado.fbx; falls back to a marker box (and a
    // warning) when the optimization has not produced it yet, so the level still builds.
    private static GameObject Kit(string id, Transform parent, string name, Vector3 localPosition, float yaw, Vector3 size, Anchor anchor,
        bool fitLargest = false, Quaternion? tilt = null, bool solid = false)
    {
        if (_kit == null)
            _kit = Directory.Exists(KitRoot)
                ? Directory.GetFiles(KitRoot, "*_Optimizado.fbx", SearchOption.AllDirectories)
                    .ToDictionary(f => Path.GetFileName(f).Replace("_Optimizado.fbx", ""), f => f.Replace('\\', '/'))
                : new Dictionary<string, string>();
        if (!_kit.TryGetValue(id, out string fbx))
        {
            Debug.LogWarning("MI_BLOCKOUT: falta " + id + "_Optimizado.fbx; se usa un marcador.");
            var s = new Vector3(size.x > 0 ? size.x : 1, size.y > 0 ? size.y : 1, size.z > 0 ? size.z : (size.x > 0 ? size.x : 1));
            float y = anchor == Anchor.Bottom ? s.y * 0.5f : anchor == Anchor.Top ? -s.y * 0.5f : 0;
            var box = Block(parent, name + " [falta " + id + "]", localPosition + Vector3.up * y, s, _marker, collider: solid);
            box.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            return box;
        }
        var model = Place(AssetDatabase.LoadAssetAtPath<GameObject>(fbx), KitMaterial(id, Path.GetDirectoryName(fbx).Replace('\\', '/')),
            parent, name, localPosition, yaw, size, anchor, fitLargest, tilt);
        if (solid)
        {
            var b = LocalBounds(model.transform, model);
            var box = model.AddComponent<BoxCollider>(); box.center = b.center; box.size = b.size;
        }
        return model;
    }

    // Same contract as PlazaModelSetup.Place: container at localPosition, model scaled to size in
    // parent space (0 on an axis = keep proportion) and anchored by its bounds.
    private static GameObject Place(GameObject prefab, Material material, Transform parent, string name, Vector3 localPosition, float yaw, Vector3 size,
        Anchor anchor, bool fitLargest = false, Quaternion? tilt = null)
    {
        var container = new GameObject(name).transform;
        container.SetParent(parent, false);
        container.localPosition = localPosition; container.localRotation = Quaternion.Euler(0, yaw, 0);
        var scaler = new GameObject("Scale").transform; scaler.SetParent(container, false);
        var pivot = new GameObject("Orientation").transform; pivot.SetParent(scaler, false); pivot.localRotation = tilt ?? Quaternion.identity;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pivot);
        instance.transform.localPosition = Vector3.zero;
        if (material != null)
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true)) r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
        var b = LocalBounds(container, instance);
        Vector3 ratio;
        if (fitLargest) ratio = Vector3.one * (size.x / Mathf.Max(b.size.x, b.size.y, b.size.z));
        else
        {
            var given = new List<float>();
            if (size.x > 0) given.Add(size.x / b.size.x); if (size.y > 0) given.Add(size.y / b.size.y); if (size.z > 0) given.Add(size.z / b.size.z);
            float uniform = given.Average();
            ratio = new Vector3(size.x > 0 ? size.x / b.size.x : uniform, size.y > 0 ? size.y / b.size.y : uniform, size.z > 0 ? size.z / b.size.z : uniform);
        }
        scaler.localScale = ratio;
        b = LocalBounds(container, instance);
        float y = anchor == Anchor.Bottom ? b.min.y : anchor == Anchor.Top ? b.max.y : b.center.y;
        scaler.localPosition = -new Vector3(b.center.x, y, b.center.z);
        foreach (var t in container.GetComponentsInChildren<Transform>(true)) { t.gameObject.layer = parent.gameObject.layer; t.gameObject.isStatic = false; }
        return container.gameObject;
    }

    // URP Lit from the Tripo textures next to the original model (base colour, normal, and a
    // metallic/smoothness map composed from the metallic and roughness maps).
    private static Material KitMaterial(string id, string folder)
    {
        if (Materials.TryGetValue(id, out var cached)) return cached;
        string path = KitMaterialFolder + "/MI_" + id + ".mat";
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
            if (importer.textureType != TextureImporterType.NormalMap) { importer.textureType = TextureImporterType.NormalMap; importer.SaveAndReimport(); }
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal)); material.EnableKeyword("_NORMALMAP");
        }
        string metallic = find("_metallic"), roughness = find("_roughness");
        if (metallic != null && roughness != null)
        {
            string packed = KitMaterialFolder + "/MI_" + id + "_MetallicSmoothness.png";
            if (!File.Exists(packed)) Pack(metallic, roughness, packed);
            AssetDatabase.ImportAsset(packed);
            var importer = (TextureImporter)AssetImporter.GetAtPath(packed);
            if (importer.sRGBTexture || importer.maxTextureSize != 1024) { importer.sRGBTexture = false; importer.maxTextureSize = 1024; importer.SaveAndReimport(); }
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(packed));
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Metallic", 1); material.SetFloat("_Smoothness", 1); material.SetFloat("_SmoothnessTextureChannel", 0);
        }
        else { material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", 0.3f); }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        Materials[id] = material;
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
                float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                byte m = (byte)(metallic.GetPixelBilinear(u, v).r * 255);
                byte s = (byte)((1 - roughness.GetPixelBilinear(u, v).r) * 255);
                pixels[y * w + x] = new Color32(m, m, m, s);
            }
        result.SetPixels32(pixels); result.Apply();
        File.WriteAllBytes(output, result.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(metallic); UnityEngine.Object.DestroyImmediate(roughness); UnityEngine.Object.DestroyImmediate(result);
    }

    private static Bounds LocalBounds(Transform space, GameObject go)
    {
        bool first = true; var b = new Bounds();
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            Bounds local; Matrix4x4 toWorld;
            if (r is SkinnedMeshRenderer skin && skin.sharedMesh != null) { local = skin.sharedMesh.bounds; toWorld = skin.transform.localToWorldMatrix; }
            else { var f = r.GetComponent<MeshFilter>(); if (f == null || f.sharedMesh == null) continue; local = f.sharedMesh.bounds; toWorld = r.transform.localToWorldMatrix; }
            for (int i = 0; i < 8; i++)
            {
                var corner = local.center + Vector3.Scale(local.extents, new Vector3((i & 1) != 0 ? 1 : -1, (i & 2) != 0 ? 1 : -1, (i & 4) != 0 ? 1 : -1));
                var p = space.InverseTransformPoint(toWorld.MultiplyPoint3x4(corner));
                if (first) { b = new Bounds(p, Vector3.zero); first = false; } else b.Encapsulate(p);
            }
        }
        return b;
    }
}

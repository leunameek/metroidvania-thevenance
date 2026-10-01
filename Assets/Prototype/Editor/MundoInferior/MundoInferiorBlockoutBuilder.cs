using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the Mundo Inferior blockout scene (Specs/Worlds/Guia-completa-mundo-inferior, stages 1-3):
// nine room roots with their local origin at the centre of the entry edge, graybox floors with
// real colliders at the guide's heights, physical connectors (ramps of 2 m per 4 m), pits with
// fall volumes, and the functional instances at their proposed local positions. Key objects use
// the decimated Tripo kit (Assets/Models/Mundo_Inferior/*/<ID>_Optimizado.fbx); characters that
// have no model yet are the hub's training bot. The scene is regenerated from scratch each run.
[InitializeOnLoad]
public static class MundoInferiorBlockoutBuilder
{
    public const string ScenePath = WorldTravel.LowerWorldScene;
    private const string KitRoot = "Assets/Models/Mundo_Inferior";
    private const string MaterialFolder = "Assets/Worlds/MundoInferior/Materials";
    private const string KitMaterialFolder = MaterialFolder + "/Kit";
    private const string BotFolder = "Assets/Models/Guardian+de+entrenamiento";
    private const string BotController = BotFolder + "/Guardian_Entrenamiento.controller";
    private const string BotMaterial = "Assets/Art/Environments/PlazaNunez/Materials/Props/Guardian_de_entrenamiento.mat";
    private const string PlayerVisual = "Assets/Models/Nemequene/Nemequene_Player_Visual.prefab";
    private const string AutoRunKey = "MundoInferior.Blockout.v1";

    private enum Anchor { Bottom, Center, Top }

    private sealed class Room
    {
        public int Index; public string Name; public Transform Root; public float Yaw; public Transform Spawn;
    }

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    private static Dictionary<string, string> _kit;
    private static Material _floor, _cliff, _path, _trial, _hazard, _frame, _veil, _marker;
    private static readonly List<Room> Rooms = new List<Room>();

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

    [MenuItem("Nemequene/Mundo Inferior/Construir bloqueo")]
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
        Materials.Clear(); Rooms.Clear(); _kit = null;
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        Directory.CreateDirectory(KitMaterialFolder);
        AssetDatabase.Refresh();
        _floor = Flat("MI_Suelo", new Color(0.29f, 0.40f, 0.42f));
        _cliff = Flat("MI_Roca", new Color(0.19f, 0.21f, 0.27f));
        _path = Flat("MI_Conector", new Color(0.36f, 0.39f, 0.43f));
        _trial = Flat("MI_Prueba", new Color(0.56f, 0.47f, 0.27f));
        _hazard = Flat("MI_Peligro", new Color(0.50f, 0.13f, 0.11f));
        _frame = Flat("MI_Marco", new Color(0.33f, 0.33f, 0.36f));
        _marker = Flat("MI_Marca", new Color(0.86f, 0.70f, 0.32f));
        _veil = Flat("MI_Velo_Portal", new Color(0.35f, 0.75f, 1f), emission: new Color(0.25f, 0.6f, 1f) * 1.6f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var world = new GameObject("MI_WorldRoot").transform;
        var rooms = Group(world, "Rooms");
        var traversal = Group(world, "Traversal");
        var atmosphere = Group(world, "Atmosphere");

        // Global heights of the guide (1.2): 01 at 0; 02 -2; 03 +2; 04-05 -6; 06 -10; 07-09 -14.
        // Rooms are turned so their local +Z (entry -> back) follows the route; the fixed camera
        // turns with them through the room volumes.
        Umbral(NewRoom(rooms, 0, "01 Umbral", new Vector3(110, 0, 80), -90, 12, 10, new Vector3(0, 0, 3)));
        Santuario(NewRoom(rooms, 1, "02 Santuario de raíces", new Vector3(94, -2, 80), -90, 14, 13));
        Brazaletes(NewRoom(rooms, 2, "03 Galería de brazaletes", new Vector3(75, 2, 80), -90, 12, 18));
        Centinelas(NewRoom(rooms, 3, "04 Patio de centinelas", new Vector3(31.5f, -6, 78), 180, 18, 44));
        Pendulos(NewRoom(rooms, 4, "05 Paso de péndulos", new Vector3(36, -6, 28), 90, 10, 24));
        Derrumbe(NewRoom(rooms, 5, "06 Galería del derrumbe", new Vector3(64, -10, 5), -90, 12, 26));
        Cuerno(NewRoom(rooms, 6, "07 Cámara del cuerno", new Vector3(24, -14, 5), -90, 14, 18));
        Antesala(NewRoom(rooms, 7, "08 Antesala", new Vector3(28, -14, 26), -90, 14, 12, new Vector3(0, 0, 3)));
        Guardian(NewRoom(rooms, 8, "09 Cámara del guardián", new Vector3(16, -14, 26), -90, 18, 16));

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
        Label(traversal, "Corredor seguro de 06 → antesala", new Vector3(37.5f, -10.5f, 22), Quaternion.Euler(0, 90, 0));

        Kit("N01", atmosphere, "N01 Árbol seco central", new Vector3(62, -9, 52), 20, new Vector3(0, 9, 0), Anchor.Bottom);
        Atmosphere(atmosphere);
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
        Floor(t, "T02 Suelo", -6, 6, 0, 10, 0, _floor);
        Wall(t, "T06 Pared posterior izq", -6, -1.6f, 10, 10.6f, 6);
        Wall(t, "T06 Pared posterior der", 1.6f, 6, 10, 10.6f, 6);
        Wall(t, "A06 Murete der", 6, 6.6f, 0, 10, 1);
        Wall(t, "A06 Murete izq", -6.6f, -6, 0, 4.6f, 1);
        Wall(t, "A06 Murete izq fondo", -6.6f, -6, 7.4f, 10, 1);
        Portal(t, "A04 Portal de llegada y retorno", new Vector3(0, 0, 0.7f), 0, true, "A04 · volver a Plaza Núñez (E)");
        Gate(t, "A02 Puerta del atajo", new Vector3(-5, 0, 6), 90, "atajo_03_01", false);
        Crystal(t, new Vector3(-3, 0, 2)); Crystal(t, new Vector3(3, 0, 7));
        Label(t, "Salida a 02 →", new Vector3(0, 2.2f, 9.5f));
    }

    private static void Santuario(Room r)
    {
        var t = r.Root;
        Floor(t, "T02 Terraza principal", -7, 7, 0, 7, 0, _floor);
        Floor(t, "Suelo de recuperación", -7, 7, 7, 13, -1, _cliff, 1);
        Floor(t, "T01 Apoyo de salida (doble salto)", -1, 1, 7, 9, 1.8f, _trial, 2.8f);
        Floor(t, "Plataforma de llegada", -3, 3, 9, 13, 2.6f, _trial, 3.6f);
        Altar(t, "Altar semilla", new Vector3(-3, 0, 4), 90, "O04d", "O03", 0.3f, "O03 Semilla · doble salto");
        Bot(t, "C04 Custodio de raíces", new Vector3(3, 0, 4), 180, 1.8f);
        Kit("O04e", t, "O04e Descanso", new Vector3(3, 0.01f, 1.8f), 0, new Vector3(1.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        Label(t, "Descanso · checkpoint 02", new Vector3(3, 1.2f, 1.8f));
        Kit("N04", t, "N04 Vegetación A", new Vector3(-4.3f, 0, 4.7f), 30, new Vector3(0, 0.8f, 0), Anchor.Bottom);
        Kit("N04", t, "N04 Vegetación B", new Vector3(-4.2f, 0, 3.1f), 160, new Vector3(0, 0.7f, 0), Anchor.Bottom);
        Crystal(t, new Vector3(-5.5f, 0, 1));
        Label(t, "Ensayo de doble salto ↑", new Vector3(0, 3.6f, 8));
    }

    private static void Brazaletes(Room r)
    {
        var t = r.Root;
        Floor(t, "Zona segura y lanzamiento", -6, 6, 0, 10, 0, _floor);
        Floor(t, "Plataforma de recepción", -2, 2, 11, 15, 0, _trial);
        Floor(t, "Plataforma del centinela", -6, 6, 15, 18, 0, _floor);
        Fall(t, "Pozo del ensayo de impulso", new Vector3(0, -4, 12.5f), new Vector3(14, 1, 5), Mark(t, "Ancla 03", new Vector3(0, 0, 8)));
        Altar(t, "Altar brazaletes 1", new Vector3(-3, 0, 5), 90, "O04b", null, 0, "O01 Brazaletes · impulso nivel 1");
        Kit("H05", t, "H05 Palanca del atajo", new Vector3(-4, 0, 8), 90, new Vector3(0, 1.4f, 0), Anchor.Bottom, solid: true);
        Label(t, "H05 · abre atajo 03 → 01 (E, pendiente)", new Vector3(-4, 2, 8));
        Gate(t, "A02 Acceso del atajo", new Vector3(-5, 0, 9), 90, "atajo_03_01", false);
        Kit("A05", t, "A05 Boca del atajo", new Vector3(-8.6f, 0, 9), -90, new Vector3(5, 5, 0), Anchor.Bottom);
        Bot(t, "C01 Centinela", new Vector3(0, 0, 16), 180, 1.7f);
        Label(t, "Ensayo de impulso (Q)", new Vector3(0, 1.8f, 10.5f));
    }

    private static void Centinelas(Room r)
    {
        var t = r.Root;
        Floor(t, "Módulos 1-2", -6, 6, 0, 22, 0, _floor);
        Floor(t, "Módulo final", -9, 9, 22, 44, 0, _floor);
        foreach (var (x, z) in new[] { (-2.5f, 15f), (2.5f, 17f) })
        {
            Block(t, "T05 Pilar", new Vector3(x, 2, z), new Vector3(2, 4, 2), _cliff);
            Kit("T05", t, "T05 Pilar (modelo)", new Vector3(x, 0, z), 0, new Vector3(2.1f, 4, 2.1f), Anchor.Bottom);
        }
        Bot(t, "C01 Centinela A", new Vector3(-2, 0, 4), 180, 1.7f);
        Bot(t, "C01 Centinela B", new Vector3(2, 0, 8), 180, 1.7f);
        Altar(t, "Altar brazaletes 2", new Vector3(-4, 0, 10.5f), 90, "O04b", null, 0, "O01 · impulso nivel 2");
        Bot(t, "C02a Vigía (arco C02b pendiente)", new Vector3(0, 0, 19), 180, 1.8f);
        Altar(t, "Altar brazaletes 3", new Vector3(4, 0, 21.5f), -90, "O04b", null, 0, "O01 · impulso nivel 3");
        Block(t, "Carril de ensayo de cadena", new Vector3(0, 0.01f, 24), new Vector3(16, 0.02f, 1.2f), _trial, collider: false);
        Label(t, "Carril de cadena Q-Q-Q", new Vector3(-6, 1, 24));
        Block(t, "Arena del escudo (Z 28-43)", new Vector3(0, 0.01f, 35.5f), new Vector3(17, 0.02f, 15), _frame, collider: false);
        var shield = Bot(t, "C03a Guardián de escudo", new Vector3(0, 0, 35), 180, 2.1f);
        Kit("C03c", shield.transform, "C03c Escudo (intacto C03b pendiente)", new Vector3(0, 1.1f, 0.7f), 0, new Vector3(1.3f, 0, 0), Anchor.Center, fitLargest: true);
        Gate(t, "A01+A02 Salida de combate", new Vector3(0, 0, 43.5f), 0, "salida_04", false);
        Wall(t, "Cierre final izq", -9, -1.6f, 43.4f, 44, 4);
        Wall(t, "Cierre final der", 1.6f, 9, 43.4f, 44, 4);
    }

    private static void Pendulos(Room r)
    {
        var t = r.Root;
        Floor(t, "Terraza de preparación", -5, 5, 0, 4, 0, _floor);
        Floor(t, "T01 Apoyo previo", -1, 1, 5, 7, 0, _trial);
        Floor(t, "Descanso central", -2, 2, 10, 14, 0, _floor);
        Floor(t, "T01 Recepción", -1, 1, 17, 19, 0, _trial);
        Floor(t, "Terraza final", -5, 5, 20, 24, 0, _floor);
        var a = Mark(t, "Ancla 05A", new Vector3(0, 0, 2));
        var b = Mark(t, "Ancla 05B", new Vector3(0, 0, 12));
        Pit(t, -5, 5, 4, 20, new[] { 5f, 8.5f, 15.5f, 19f });
        Fall(t, "Pozo 05 primer tramo", new Vector3(0, -2.5f, 7), new Vector3(12, 1, 6), a);
        Fall(t, "Pozo 05 segundo tramo", new Vector3(0, -2.5f, 17), new Vector3(12, 1, 6), b);
        Pendulum(t, "H02 Péndulo 1", new Vector3(0, 6, 8.5f), 0f, a);
        Pendulum(t, "H02 Péndulo 2", new Vector3(0, 6, 15.5f), 0.5f, b);
    }

    private static void Derrumbe(Room r)
    {
        var t = r.Root;
        Floor(t, "Preparación", -6, 6, 0, 4, 0, _floor);
        Slab(t, "H03 Losa aislada", new Vector3(0, 0, 5.5f));
        Floor(t, "Refugio A", -5, -1, 5, 9, 0, _floor);
        Slab(t, "H03 Losa 1", new Vector3(0, 0, 10)); Slab(t, "H03 Losa 2", new Vector3(0, 0, 13)); Slab(t, "H03 Losa 3", new Vector3(0, 0, 16));
        Floor(t, "Refugio B", 1, 5, 16, 20, 0, _floor);
        Floor(t, "Tramo de piedras y salida", -6, 6, 20, 26, 0, _floor);
        Pit(t, -6, 6, 4, 20, new[] { 5f, 11f, 13f, 15f, 17f });
        Fall(t, "Pozo 06 inicio", new Vector3(0, -2.5f, 6.5f), new Vector3(14, 1, 5), Mark(t, "Ancla 06 preparación", new Vector3(0, 0, 2)));
        Fall(t, "Pozo 06 secuencia", new Vector3(0, -2.5f, 14.5f), new Vector3(14, 1, 11), Mark(t, "Ancla 06 refugio A", new Vector3(-3, 0, 7)));
        foreach (var (x, z) in new[] { (-1f, 20f), (1f, 23f) })
        {
            Kit("H04", t, "H04 Estalactita que cae", new Vector3(x, 5, z), 0, new Vector3(0, 2, 0), Anchor.Top);
            Block(t, "Sombra de aviso H04", new Vector3(x, 0.015f, z), new Vector3(1.6f, 0.02f, 1.6f), _hazard, collider: false);
        }
        Kit("H05", t, "H05 Palanca de retorno", new Vector3(4, 0, 24), -90, new Vector3(0, 1.4f, 0), Anchor.Bottom, solid: true);
        Gate(t, "A02 Corredor seguro", new Vector3(5.4f, 0, 24), 90, "corredor_06", false);
        Kit("A05", t, "A05 Entrada del corredor", new Vector3(8.6f, 0, 24), 90, new Vector3(5, 5, 0), Anchor.Bottom);
    }

    private static void Cuerno(Room r)
    {
        var t = r.Root;
        Floor(t, "Suelo de combate y preparación", -7, 7, 0, 7, 0, _floor);
        Floor(t, "T01 Apoyo A", -2, 0, 7.5f, 9.5f, 0, _trial);
        Floor(t, "T01 Apoyo B", 0, 2, 10.5f, 12.5f, 0, _trial);
        Floor(t, "Isla del cuerno", -3, 3, 12.5f, 17.5f, 0, _floor);
        Pit(t, -5, 5, 7, 12.5f, new[] { 8f, 10f, 12f });
        Fall(t, "Pozo 07", new Vector3(0, -2.5f, 10), new Vector3(14, 1, 6), Mark(t, "Ancla 07", new Vector3(0, 0, 6)));
        Bot(t, "C01 Centinela", new Vector3(0, 0, 3.5f), 180, 1.7f);
        Altar(t, "Altar del cuerno", new Vector3(0, 0, 15.5f), 180, "O04c", "O02", 0.6f, "O02 Cuerno · abre la reja de 08");
        Frame(t, "A01a Arco de fondo (modelo dañado)", new Vector3(0, 0, 17.2f), 0);
        Kit("N02d", t, "N02d Ruta de vuelta (decorativa)", new Vector3(-6.4f, 0, 5.5f), 90, new Vector3(0, 3, 0), Anchor.Bottom);
        Label(t, "Salida hacia 08 →", new Vector3(3.2f, 1.8f, 15), Quaternion.Euler(0, 90, 0));
    }

    private static void Antesala(Room r)
    {
        var t = r.Root;
        Floor(t, "Suelo de la antesala", -7, 7, 0, 12, 0, _floor);
        Kit("O04e", t, "O04e Descanso", new Vector3(-3, 0.01f, 3), 0, new Vector3(1.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        Label(t, "Descanso · checkpoint 08", new Vector3(-3, 1.2f, 3));
        Altar(t, "Receptor del cuerno", new Vector3(-2.5f, 0, 8.5f), 180, "O04c", null, 0, "Receptor · E con cuerno (pendiente)");
        Gate(t, "A01+A02 Reja ritual", new Vector3(0, 0, 10.5f), 0, "reja_cuerno", false);
        Wall(t, "Cierre izq", -7, -1.6f, 10.4f, 11, 5);
        Wall(t, "Cierre der", 1.6f, 7, 10.4f, 11, 5);
        Crystal(t, new Vector3(-3, 0, 10), warm: true); Crystal(t, new Vector3(3, 0, 10), warm: true);
        Bot(t, "C04 Custodio (opcional)", new Vector3(4, 0, 6), 180, 1.8f);
        Kit("T03", t, "T03 Hueco cerrado", new Vector3(4, 0.02f, 3), 0, new Vector3(2, 0, 0), Anchor.Top, fitLargest: true);
        Kit("A03", t, "A03 Rejilla cerrada", new Vector3(4, 0.03f, 3), 0, new Vector3(1.6f, 0, 0), Anchor.Bottom, fitLargest: true);
    }

    private static void Guardian(Room r)
    {
        var t = r.Root;
        Floor(t, "Arena", -9, 9, 0, 16, 0, _floor);
        Wall(t, "T06 Límite izq", -9.6f, -9, 0, 16.6f, 6);
        Wall(t, "T06 Límite der", 9, 9.6f, 0, 16.6f, 6);
        Wall(t, "T06 Límite fondo", -9.6f, 9.6f, 16, 16.6f, 6);
        Wall(t, "Frente izq", -9.6f, -1.6f, 0.4f, 1, 6);
        Wall(t, "Frente der", 1.6f, 9.6f, 0.4f, 1, 6);
        Gate(t, "A01+A02 Cierre de arena", new Vector3(0, 0, 0.7f), 0, "cierre_arena", true);
        Block(t, "Trigger de inicio (pendiente)", new Vector3(0, 0.01f, 3), new Vector3(4, 0.02f, 2), _marker, collider: false);
        Floor(t, "T01 Apoyo lateral izq", -7, -5, 7, 9, 0.8f, _trial, 0.8f);
        Floor(t, "T01 Apoyo lateral der", 5, 7, 7, 9, 0.8f, _trial, 0.8f);
        Bot(t, "C05 Guardián del fondo", new Vector3(0, 0, 11), 180, 4.2f);
        Portal(t, "A04 Portal de victoria", new Vector3(7, 0, 13), -90, false, "A04 · regreso tras la victoria (inactivo)");
        foreach (var (x, z) in new[] { (-4f, 7f), (4f, 9f) })
            Kit("H04", t, "H04 Ataque del jefe", new Vector3(x, 6, z), 0, new Vector3(0, 2, 0), Anchor.Top);
    }

    // ---------------------------------------------------------------- assemblies

    private static Room NewRoom(Transform parent, int index, string name, Vector3 entry, float yaw, float width, float depth, Vector3? spawn = null)
    {
        var root = new GameObject("Room_" + name.Replace(" ", "_")).transform;
        root.SetParent(parent, false);
        root.position = entry; root.rotation = Quaternion.Euler(0, yaw, 0);
        var volume = root.gameObject.AddComponent<BoxCollider>();
        volume.isTrigger = true; volume.center = new Vector3(0, 2, depth * 0.5f); volume.size = new Vector3(width + 2, 14, depth + 2);
        var zone = root.gameObject.AddComponent<MICameraZone>();
        var so = new SerializedObject(zone);
        so.FindProperty("roomIndex").intValue = index; so.FindProperty("yaw").floatValue = yaw;
        so.ApplyModifiedPropertiesWithoutUndo();
        var room = new Room { Index = index, Name = name, Root = root, Yaw = yaw };
        room.Spawn = Mark(root, "Spawn " + name.Substring(0, 2), spawn ?? new Vector3(0, 0, 1.5f));
        Label(root, name.ToUpperInvariant(), new Vector3(0, 3.2f, 0.4f), size: 0.26f);
        Rooms.Add(room);
        return room;
    }

    // O04a in front of the lane with its support in the top socket and the pickup above it.
    private static void Altar(Transform room, string name, Vector3 position, float yaw, string support, string item, float itemSize, string label)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Kit("O04a", root, "O04a Base", Vector3.zero, 0, new Vector3(1.5f, 0.9f, 1f), Anchor.Bottom, solid: true);
        Kit(support, root, support + " Soporte", new Vector3(0, 0.9f, 0), 0, new Vector3(support == "O04d" ? 0.9f : 1.2f, 0, 0), Anchor.Bottom);
        if (support == "O04b")
        {
            // One reward made of two meshes, 0.2 m apart.
            Kit("O01a", root, "O01a Brazalete izquierdo", new Vector3(-0.2f, 1.22f, 0), 0, new Vector3(0.24f, 0, 0), Anchor.Center, fitLargest: true);
            Kit("O01b", root, "O01b Brazalete derecho", new Vector3(0.2f, 1.22f, 0), 0, new Vector3(0.24f, 0, 0), Anchor.Center, fitLargest: true);
        }
        else if (item != null)
            Kit(item, root, item + " Hallazgo", new Vector3(0, 1.3f, 0), 0, new Vector3(itemSize, 0, 0), Anchor.Center, fitLargest: true);
        var light = new GameObject("Luz del hallazgo", typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(root, false); light.transform.localPosition = new Vector3(0, 2.2f, 0);
        light.type = LightType.Point; light.color = new Color(1f, 0.78f, 0.42f); light.range = 6; light.intensity = 2.2f;
        Label(root, label, new Vector3(0, 2.1f, 0), Quaternion.Euler(0, -yaw, 0));
    }

    // A01 frame (graybox jambs and lintel, the A01a model export is broken) with an A02 leaf on a
    // hinge 1.1 m to the left of the passage centre.
    private static void Gate(Transform room, string name, Vector3 position, float yaw, string flag, bool startOpen)
    {
        var root = Frame(room, name, position, yaw);
        var hinge = new GameObject("Bisagra").transform; hinge.SetParent(root, false); hinge.localPosition = new Vector3(-1.1f, 0, 0);
        // The Tripo leaf is modelled in the YZ plane: a quarter turn puts it across the passage.
        var leaf = Kit("A02", hinge, "A02 Hoja", new Vector3(1.1f, 0, 0), 0, new Vector3(2.2f, 3f, 0), Anchor.Bottom, tilt: Quaternion.Euler(0, 90, 0));
        var box = leaf.AddComponent<BoxCollider>(); box.center = new Vector3(0, 1.5f, 0); box.size = new Vector3(2.2f, 3f, 0.25f);
        var gate = root.gameObject.AddComponent<MIGate>();
        var so = new SerializedObject(gate);
        so.FindProperty("flagId").stringValue = flag; so.FindProperty("hinge").objectReferenceValue = hinge;
        so.FindProperty("startOpen").boolValue = startOpen;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform Frame(Transform room, string name, Vector3 position, float yaw)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false);
        root.localPosition = position; root.localRotation = Quaternion.Euler(0, yaw, 0);
        Block(root, "Jamba izq", new Vector3(-1.45f, 1.85f, 0), new Vector3(0.5f, 3.7f, 0.6f), _frame);
        Block(root, "Jamba der", new Vector3(1.45f, 1.85f, 0), new Vector3(0.5f, 3.7f, 0.6f), _frame);
        Block(root, "Dintel", new Vector3(0, 3.5f, 0), new Vector3(3.4f, 0.5f, 0.6f), _frame);
        return root;
    }

    private static void Portal(Transform room, string name, Vector3 position, float yaw, bool active, string label)
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
        var portal = root.gameObject.AddComponent<MIPortal>();
        var so = new SerializedObject(portal);
        so.FindProperty("active").boolValue = active; so.FindProperty("effect").objectReferenceValue = veil;
        so.FindProperty("range").floatValue = 1.8f; // the arrival spawn (2.3 m away) stays out of range
        so.ApplyModifiedPropertiesWithoutUndo();
        Label(root, label, new Vector3(0, 4.6f, 0), Quaternion.Euler(0, -yaw, 0));
    }

    private static void Pendulum(Transform room, string name, Vector3 pivot, float phase, Transform anchor)
    {
        var root = new GameObject(name).transform; root.SetParent(room, false); root.localPosition = pivot;
        Block(root, "Soporte de techo", new Vector3(0, 0.3f, 0), new Vector3(1.2f, 0.6f, 1.2f), _frame, collider: false);
        var swing = new GameObject("Eje (oscila en Z)").transform; swing.SetParent(root, false);
        var pendulum = swing.gameObject.AddComponent<MIPendulum>();
        var so = new SerializedObject(pendulum); so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
        Kit("H02", swing, "H02 Péndulo", Vector3.zero, 0, new Vector3(0, 5.4f, 0), Anchor.Top);
        // Hit volume around the 1.2 x 1.8 x 0.6 weight, moving with it.
        var hit = new GameObject("Volumen de golpe", typeof(BoxCollider), typeof(Rigidbody)).transform;
        hit.SetParent(swing, false); hit.localPosition = new Vector3(0, -4.6f, 0);
        var box = hit.GetComponent<BoxCollider>(); box.isTrigger = true; box.size = new Vector3(1.4f, 1.8f, 0.9f);
        hit.GetComponent<Rigidbody>().isKinematic = true;
        SetAnchor(hit.gameObject.AddComponent<MIFallZone>(), anchor);
    }

    // H03: graybox slab with its collider (collapse comes in stage 7) under the H03a model.
    private static void Slab(Transform room, string name, Vector3 center)
    {
        Block(room, name, center + new Vector3(0, -0.25f, 0), new Vector3(2, 0.5f, 2), _trial);
        Kit("H03a", room, name + " (modelo)", center + new Vector3(0, 0.02f, 0), 0, new Vector3(2, 0, 2), Anchor.Top);
    }

    // Pit floor 3 m down with H01 spike modules (2 x 2 m) in the given rows.
    private static void Pit(Transform room, float x0, float x1, float z0, float z1, float[] rows)
    {
        Floor(room, "Fondo del pozo", x0, x1, z0, z1, -3, _hazard, 1);
        foreach (float z in rows)
            for (float x = x0 + 1; x <= x1 - 1 + 0.01f; x += 2)
                Kit("H01", room, "H01 Pinchos", new Vector3(x, -3, z), (int)(x * 7 + z * 3) % 4 * 90, new Vector3(2, 1.2f, 2), Anchor.Bottom);
    }

    private static void Fall(Transform room, string name, Vector3 center, Vector3 size, Transform anchor)
    {
        var zone = new GameObject(name, typeof(BoxCollider)).transform;
        zone.SetParent(room, false); zone.localPosition = center;
        var box = zone.GetComponent<BoxCollider>(); box.isTrigger = true; box.size = size;
        SetAnchor(zone.gameObject.AddComponent<MIFallZone>(), anchor);
    }
    private static void SetAnchor(MIFallZone zone, Transform anchor)
    {
        var so = new SerializedObject(zone); so.FindProperty("anchor").objectReferenceValue = anchor; so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Crystal(Transform room, Vector3 position, bool warm = false)
    {
        var c = Kit("N05", room, "N05 Piedra de luz", position, (int)(position.x * 40) % 360, new Vector3(0.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        var light = new GameObject("Luz", typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(c.transform, false); light.transform.localPosition = new Vector3(0, 0.9f, 0);
        light.type = LightType.Point; light.range = 7; light.intensity = 1.6f;
        light.color = warm ? new Color(1f, 0.72f, 0.4f) : new Color(0.45f, 0.8f, 1f);
    }

    // Training bot of Plaza Núñez standing in for a character that has no model yet.
    private static GameObject Bot(Transform room, string name, Vector3 position, float yaw, float height)
    {
        var folder = BotFolder;
        string fbx = Directory.Exists(folder) ? Directory.GetFiles(folder, "tripo_convert*.fbx").FirstOrDefault() : null;
        GameObject model;
        if (fbx == null)
        {
            model = Block(room, name, position + Vector3.up * height * 0.5f, new Vector3(0.8f, height, 0.8f), _marker);
            model.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        }
        else
        {
            model = Place(AssetDatabase.LoadAssetAtPath<GameObject>(fbx.Replace('\\', '/')), AssetDatabase.LoadAssetAtPath<Material>(BotMaterial),
                room, name, position, yaw, new Vector3(0, height, 0), Anchor.Bottom);
            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null) animator = model.GetComponentsInChildren<Transform>().First(t => t.parent != null && t.parent.name == "Orientation").gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(BotController);
            animator.applyRootMotion = false;
            var box = model.AddComponent<BoxCollider>();
            box.center = new Vector3(0, height * 0.5f, 0); box.size = new Vector3(height * 0.35f, height, height * 0.35f);
        }
        Label(room, name + " · bot", position + new Vector3(0, height + 0.5f, 0));
        return model;
    }

    // ---------------------------------------------------------------- player, light

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

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ExplorationOrbitCamera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.nearClipPlane = 0.1f; camera.farClipPlane = 160; camera.fieldOfView = 60;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.05f, 0.08f, 0.12f);
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

    // Blue-grey cavern: dim cold key light, trilight ambient and mist; warm accents come from altars.
    private static void Atmosphere(Transform parent)
    {
        var sun = new GameObject("Luz de caverna", typeof(Light)).GetComponent<Light>();
        sun.transform.SetParent(parent, false);
        sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(58, 25, 0);
        sun.color = new Color(0.62f, 0.74f, 0.95f); sun.intensity = 0.75f; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.6f;
        RenderSettings.skybox = null;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.30f, 0.40f, 0.52f);
        RenderSettings.ambientEquatorColor = new Color(0.20f, 0.25f, 0.31f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.11f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.07f, 0.11f, 0.16f); RenderSettings.fogStartDistance = 28; RenderSettings.fogEndDistance = 110;
    }

    // ---------------------------------------------------------------- graybox helpers

    private static Transform Group(Transform parent, string name) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }

    private static Transform Mark(Transform parent, string name, Vector3 localPosition)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = localPosition; return t;
    }

    // Walkable box between x0..x1 / z0..z1 (room space) with its top at `top`.
    private static GameObject Floor(Transform parent, string name, float x0, float x1, float z0, float z1, float top, Material material, float thickness = 2)
        => Block(parent, name, new Vector3((x0 + x1) * 0.5f, top - thickness * 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, thickness, z1 - z0), material);

    private static GameObject Wall(Transform parent, string name, float x0, float x1, float z0, float z1, float height)
        => Block(parent, name, new Vector3((x0 + x1) * 0.5f, height * 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, height, z1 - z0), _cliff);

    private static GameObject Block(Transform parent, string name, Vector3 localCenter, Vector3 size, Material material, bool collider = true)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name; obj.isStatic = true;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localCenter; obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }

    // World-space walkway through the points: flat runs, ramps where the height changes, square
    // pads at the corners so turns have no gaps. A pad next to a ramp would stand above the
    // slope as a step the controller cannot climb, so corners must join flat runs.
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
                    Block(root, "Rellano " + i, points[i] - Vector3.up * 0.5f, new Vector3(width, 1, width), _path);
                }
            }
            if (i == 0) continue;
            Vector3 a = points[i - 1], b = points[i], d = b - a;
            if (new Vector2(d.x, d.z).magnitude < 0.01f) continue;
            var rotation = Quaternion.LookRotation(d);
            var segment = Block(root, "Tramo " + i, Vector3.zero, new Vector3(width, 0.5f, d.magnitude + 0.2f), _path);
            segment.transform.rotation = rotation;
            segment.transform.position = (a + b) * 0.5f - rotation * Vector3.up * 0.25f;
        }
    }

    private static void Label(Transform parent, string text, Vector3 position, Quaternion? rotation = null, float size = 0.16f)
    {
        // TextMeshPro is depth tested: labels of other rooms do not show through the rock.
        var obj = new GameObject("LABEL_" + text, typeof(TMPro.TextMeshPro));
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        // Readable from the room camera, which looks along the room's +Z.
        obj.transform.localRotation = rotation ?? Quaternion.identity;
        var tmp = obj.GetComponent<TMPro.TextMeshPro>();
        tmp.text = text; tmp.fontSize = size * 20; tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(10, 1.5f); tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        tmp.color = new Color(0.95f, 0.88f, 0.70f);
    }

    private static Material Flat(string name, Color color, Color? emission = null)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.12f);
        if (emission.HasValue) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission.Value); }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- kit models

    // Places the decimated kit model <ID>_Optimizado.fbx; falls back to a marker box (and a
    // warning) when the optimization has not produced it yet, so the blockout still builds.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Replaces the Plaza Núñez graybox props with the Tripo models in Assets/Models.
// The original primitives are only hidden (renderer off): their colliders and every object the
// gameplay code references (artifacts, guardian, portal veils, warning ring) stay in place.
// Runs once automatically after import; re-run from Nemequene > Plaza Núñez > Integrar modelos 3D.
[InitializeOnLoad]
public static class PlazaModelSetup
{
    private const string ScenePath = "Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity";
    private const string Models = "Assets/Models/";
    private const string MaterialFolder = "Assets/Art/Environments/PlazaNunez/Materials/Props";
    private const string GuardianController = Models + "Guardian+de+entrenamiento/Guardian_Entrenamiento.controller";
    private const string Prefix = "MODEL_";
    private const string AutoRunKey = "Plaza.ModelSetup.v3";
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    // Shells seen from inside (the cavern) render both faces.
    private static readonly HashSet<string> DoubleSided = new HashSet<string> { "Umbral+mundo+inferior" };
    private const string SkyMaterial = "Assets/Art/Environments/PlazaNunez/AndeanSky.mat";

    private enum Anchor { Bottom, Center, Top }

    static PlazaModelSetup()
    {
        if (Application.isBatchMode || EditorPrefs.GetBool(Key)) return;
        // If the scripts reload while Play is active (or about to start), wait until the editor is
        // back in Edit mode; otherwise the one-time integration would be skipped for good.
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

    [MenuItem("Nemequene/Plaza Núñez/Integrar modelos 3D")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var open = Enumerable.Range(0, EditorSceneManager.sceneCount).Select(i => EditorSceneManager.GetSceneAt(i).path)
            .Where(p => !string.IsNullOrEmpty(p)).ToArray();
        try
        {
            Apply();
            Debug.Log("PLAZA_MODELS_OK: modelos 3D integrados en " + ScenePath);
        }
        catch (Exception e) { Debug.LogError("PLAZA_MODELS_FAILED: " + e); }
        finally
        {
            if (!Application.isBatchMode && open.Length > 0 && open[0] != ScenePath)
            {
                EditorSceneManager.OpenScene(open[0], OpenSceneMode.Single);
                for (int i = 1; i < open.Length; i++) EditorSceneManager.OpenScene(open[i], OpenSceneMode.Additive);
            }
        }
    }

    private static void Apply()
    {
        Materials.Clear();
        AssetDatabase.ImportAsset(Models + "Guardian+de+entrenamiento", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "PLAZA_NUNEZ_Lobby")?.transform;
        if (root == null) throw new InvalidOperationException("No se encontró PLAZA_NUNEZ_Lobby en la escena del hub.");
        foreach (var old in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith(Prefix)).ToArray())
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        Ground(root);
        Thresholds(root);
        Sky(root);
        Stations(root);
        Arena(root);
        Fountain(root);
        Architecture(root);
        Gardens(root);
        Portals(root);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- props

    private static void Ground(Transform root)
    {
        var ground = Find(root, "01_Pavement");
        Hide(ground, "Stone paving", "Central promenade", "Promenade brass edge", "East west walkway", "Arrival inlay",
            "West boundary", "East boundary", "South boundary", "North boundary");
        // The slabs have irregular, broken edges: the foundation below becomes a dark mortar bed.
        var foundation = Find(ground, "Plaza foundation").GetComponent<Renderer>();
        foundation.enabled = true; foundation.sharedMaterial = Grout();
        // 10 x 10 slabs of 4 m, 15 cm thick, top at 3 cm; quarter turns break the repetition.
        for (int x = 0; x < 10; x++)
            for (int z = 0; z < 10; z++)
                // Slabs overlap by 0.5 m over their broken edges; staggered heights avoid z-fighting.
                Place("Pavimento", ground, "Pavimento_" + x + "_" + z, new Vector3(-18 + x * 4, 0.03f + ((x + 2 * z) % 3) * 0.006f, -18 + z * 4), ((x * 7 + z * 3) % 4) * 90,
                    new Vector3(4.5f, 0.15f, 4.5f), Anchor.Top);
        Place("Marca+de+llegada", ground, "Marca", new Vector3(0, 0.025f, -13), 0, new Vector3(5f, 0.1f, 5f), Anchor.Bottom);
        // Perimeter: 12 carved wall modules per side (3.5 x 2 m) facing the plaza. The north side
        // leaves the archive entrance free. The old boundary colliders stay as the real limit.
        for (int k = 0; k < 12; k++)
        {
            float t = -19.25f + k * 3.5f;
            var size = new Vector3(3.5f, 2f, 0);
            Place("Muros", ground, "Muro_S_" + k, new Vector3(t, 0, -21), 180, size, Anchor.Bottom);
            Place("Muros", ground, "Muro_O_" + k, new Vector3(-21, 0, t), -90, size, Anchor.Bottom);
            Place("Muros", ground, "Muro_E_" + k, new Vector3(21, 0, t), 90, size, Anchor.Bottom);
            if (Mathf.Abs(t) > 5f) Place("Muros", ground, "Muro_N_" + k, new Vector3(t, 0, 21), 0, size, Anchor.Bottom);
        }
    }

    private static Material Grout()
    {
        Directory.CreateDirectory(MaterialFolder);
        string path = MaterialFolder + "/Mortero.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", new Color(0.36f, 0.32f, 0.27f)); material.SetFloat("_Smoothness", 0.08f); material.SetFloat("_Metallic", 0);
        EditorUtility.SetDirty(material); return material;
    }

    private static void Thresholds(Transform root)
    {
        var group = Find(root, "07_PortalsAndWorldThresholds");
        // Floors, parapets and end walls keep their colliders (the real walkable area, 23 x 24 m,
        // top at y 0); only the graybox renderers go. Each model's walkable surface is measured
        // with ray casts and aligned to that floor.
        var lower = Find(group, "MundoInferior_Umbral");
        Hide(lower);
        // The cavern is a half dome: its open side is turned to face the fixed camera (south) and the
        // player arrives just inside the mouth, looking at the crystals and the altar.
        var cave = Place("Umbral+mundo+inferior", lower, "Umbral", new Vector3(0, 0, 6), 180, Vector3.one * 26f, Anchor.Bottom, fitLargest: true);
        AlignToSurface(cave, lower, new Vector3(0, 0, 2));
        foreach (var (position, tint) in new[] { (new Vector3(-4.5f, 4.5f, 5), new Color(0.55f, 0.35f, 1f)), (new Vector3(4.5f, 4.5f, 5), new Color(0.35f, 0.75f, 1f)), (new Vector3(0, 6, 1), new Color(0.7f, 0.45f, 1f)) })
        {
            var light = new GameObject("Luz de cristal", typeof(Light)).GetComponent<Light>();
            light.transform.SetParent(cave.transform, false); light.transform.position = lower.TransformPoint(position);
            light.type = LightType.Point; light.color = tint; light.range = 20; light.intensity = 7f;
        }
        var upper = Find(group, "MundoSuperior_Umbral");
        Hide(upper);
        // Large enough for its circular base to cover the walkable area.
        var sky = Place("Umbral+mundo+superior", upper, "Umbral", new Vector3(0, 0, 1), 0, Vector3.one * 30f, Anchor.Bottom, fitLargest: true);
        AlignToSurface(sky, upper, Vector3.zero);
    }

    // Moves the model vertically so the surface under localPoint (parent space) sits at the
    // parent's floor (y 0). The lowest upward-facing hit is the floor: roofs and floating rocks
    // are above it, and a single-surface mesh has no hidden faces below.
    private static void AlignToSurface(GameObject model, Transform parent, Vector3 localPoint)
    {
        var colliders = model.GetComponentsInChildren<MeshFilter>().Select(f => f.gameObject.AddComponent<MeshCollider>()).ToList();
        Physics.SyncTransforms();
        try
        {
            var b = WorldBounds(model);
            var origin = parent.TransformPoint(localPoint); origin.y = b.max.y + 1;
            var hits = Physics.RaycastAll(origin, Vector3.down, b.size.y + 2).Where(h => colliders.Contains(h.collider as MeshCollider)).ToArray();
            if (hits.Length == 0) { Debug.LogWarning("PLAZA_MODELS: sin superficie bajo " + parent.name); return; }
            float surface = hits.Min(h => h.point.y);
            model.transform.position += Vector3.up * (parent.position.y - surface);
        }
        finally { foreach (var c in colliders) UnityEngine.Object.DestroyImmediate(c); }
    }

    // Procedural Andean sky (Nemequene/Andean Sky) replaces the placeholder ridge spheres. Sunny
    // midday look: clear light-blue sky, bright white-gold sun, blue haze instead of grey fog.
    private static void Sky(Transform root)
    {
        Hide(Find(root, "02_Architecture"), "Andean ridge");
        var shader = Shader.Find("Nemequene/Andean Sky");
        if (shader == null) throw new InvalidOperationException("Falta el shader Nemequene/Andean Sky.");
        var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterial);
        if (sky == null) { sky = new Material(shader); AssetDatabase.CreateAsset(sky, SkyMaterial); }
        sky.shader = shader;
        var haze = new Color(0.74f, 0.85f, 0.93f);
        sky.SetColor("_ZenithColor", new Color(0.20f, 0.52f, 0.90f));
        sky.SetColor("_SkyColor", new Color(0.46f, 0.74f, 0.98f));
        sky.SetColor("_HorizonColor", new Color(0.84f, 0.93f, 1.00f));
        sky.SetColor("_FogColor", haze);
        sky.SetColor("_SunColor", new Color(1f, 0.97f, 0.86f));
        sky.SetColor("_CloudColor", Color.white);
        sky.SetColor("_FarMountain", new Color(0.36f, 0.45f, 0.62f));
        sky.SetColor("_MidMountain", new Color(0.26f, 0.48f, 0.30f));
        sky.SetColor("_NearMountain", new Color(0.22f, 0.42f, 0.24f));
        sky.SetColor("_Rock", new Color(0.50f, 0.43f, 0.37f));
        sky.SetColor("_Snow", new Color(0.97f, 0.98f, 1f));
        sky.SetFloat("_CloudCover", 0.38f); sky.SetFloat("_MountainScale", 1.7f); sky.SetFloat("_Haze", 0.18f); sky.SetFloat("_SunSize", 0.9992f);
        var sun = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional);
        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(52, -35, 0);
            sun.color = new Color(1f, 0.96f, 0.88f); sun.intensity = 1.9f; sun.shadowStrength = 0.75f;
            sky.SetVector("_SunDirection", (-sun.transform.forward).normalized);
        }
        EditorUtility.SetDirty(sky);
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.76f, 0.92f);
        RenderSettings.ambientEquatorColor = new Color(0.62f, 0.62f, 0.56f);
        RenderSettings.ambientGroundColor = new Color(0.32f, 0.30f, 0.25f);
        RenderSettings.fogColor = haze; RenderSettings.fogStartDistance = 45; RenderSettings.fogEndDistance = 140;
        foreach (var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (camera.CompareTag("MainCamera")) { camera.clearFlags = CameraClearFlags.Skybox; camera.backgroundColor = haze; camera.farClipPlane = Mathf.Max(camera.farClipPlane, 160); }
        DynamicGI.UpdateEnvironment();
    }

    private static void Stations(Transform root)
    {
        var group = Find(root, "05_HandTutorialStations");
        string[] artifacts = { "Jarron", "Disco+del+alba", "Guardián+de+Jade" };
        for (int i = 0; i < 3; i++)
        {
            var station = Find(group, "Station_0" + (i + 1));
            Hide(station, "Station mosaic", "Brass inlay", "Pedestal base", "Carved pedestal", "Gold table edge");
            Place("Pedestal", station, "Pedestal", Vector3.zero, 0, new Vector3(0, 1.36f, 0), Anchor.Bottom);
            // Artifacts turn around their own centre during inspection: centred, ~1.35 m across.
            var artifact = Find(station, "Artifact_0" + (i + 1));
            Hide(artifact);
            var model = Place(artifacts[i], artifact, "Artifact", Vector3.zero, 0, Vector3.one * (i == 1 ? 1.5f : 1.35f), Anchor.Center, fitLargest: true,
                // Disc lies flat in the file; the jade figure faces +Z. Both are turned to face the inspection camera (-Z).
                tilt: i == 1 ? Quaternion.Euler(-90, 0, 0) : i == 2 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity);
            model.isStatic = false;
        }
    }

    private static void Arena(Transform root)
    {
        var arena = Find(root, "06_TrainingCircle");
        Hide(arena, "Arena floor", "Arena inner mosaic", "Arena radial inlay");
        // The warning ring ("Defense signal") stays visible on top: the duel colours it.
        // Flattened to a 14 cm plinth: the player duels on it and the ring (y 0.16) stays on top.
        Place("Base_entrenamiento", arena, "Base", new Vector3(0, 0.005f, 0), 0, new Vector3(9.4f, 0.14f, 9.4f), Anchor.Bottom);
        var guardian = Find(arena, "Training guardian");
        Hide(guardian);
        var model = Place("Guardian+de+entrenamiento", guardian, "Guardian", Vector3.zero, 0, new Vector3(0, 2.9f, 0), Anchor.Bottom);
        var animator = model.GetComponentInChildren<Animator>();
        if (animator == null) animator = model.GetComponentsInChildren<Transform>().First(t => t.parent != null && t.parent.name == "Scale").gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = BuildGuardianController();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (animator.GetComponent<GuardianAnimator>() == null) animator.gameObject.AddComponent<GuardianAnimator>();
    }

    private static void Fountain(Transform root)
    {
        var fountain = Find(root, "04_FountainOfMemory");
        Hide(fountain);
        Place("Fuente", fountain, "Fuente", Vector3.zero, 0, new Vector3(6.2f, 0, 0), Anchor.Bottom);
    }

    private static void Architecture(Transform root)
    {
        var district = Find(root, "02_Architecture");
        // Three facade modules per side, 7.5 m tall, facing the plaza (the side cloisters' old
        // graybox windows faced outwards). The 42 m facade colliders are unchanged.
        foreach (var (name, flip) in new[] { ("North museum", false), ("West cloister", true), ("East cloister", true) })
        {
            var facade = Find(district, name);
            Hide(facade);
            for (int k = -1; k <= 1; k++)
                Place("Muro", facade, "Fachada_" + (k + 2), new Vector3(k * 14f, 0, flip ? -1f : 1f), flip ? 180 : 0,
                    new Vector3(14.1f, 7.5f, 0), Anchor.Bottom);
        }
        var entrance = Find(district, "Northern archive entrance");
        Hide(entrance);
        Place("Entrada+del+archivo", entrance, "Entrada", new Vector3(0, -0.08f, -0.1f), 0, new Vector3(6.5f, 0, 0), Anchor.Bottom);
        Hide(district, "Terrace stair 0", "Terrace stair 1", "Terrace stair 2", "Terrace stair 3");
        Place("Escaleras", district, "Escaleras", new Vector3(0, 0, 12.3f), 180, new Vector3(7f, 0.96f, 2.4f), Anchor.Bottom);
    }

    private static void Gardens(Transform root)
    {
        var gardens = Find(root, "03_GardensAndFurniture");
        var lamps = Children(gardens, "Lamp foot");
        var curbs = Children(gardens, "Garden curb");
        var benches = Children(gardens, "Cedar bench");
        Hide(gardens, "Lamp foot", "Lamp post", "Lantern cap", "Lantern", "Garden curb", "Garden soil", "Laurel trunk", "Laurel crown", "Shrub");
        for (int i = 0; i < lamps.Length; i++)
            Place("Farol", gardens, "Farol_" + (i + 1), Flat(lamps[i].localPosition), 0, new Vector3(0, 3.9f, 0), Anchor.Bottom);
        foreach (var bench in benches)
        {
            Hide(bench);
            Place("Banca", bench, "Banca", Vector3.zero, 0, new Vector3(3.2f, 0, 0), Anchor.Bottom);
        }
        for (int i = 0; i < curbs.Length; i++)
        {
            Vector3 p = Flat(curbs[i].localPosition);
            Place("Jardinera", gardens, "Jardinera_" + (i + 1), p, i * 37, new Vector3(4.6f, 0, 0), Anchor.Bottom);
            Place("Arbol", gardens, "Arbol_" + (i + 1), p + Vector3.up * 0.55f, i * 71, new Vector3(0, 6f, 0), Anchor.Bottom);
            // Bushes flank the planter along the free side (the north garden sits beside the terrace stairs).
            bool north = p.z > 8;
            Vector3[] offsets = north ? new[] { new Vector3(3.3f, 0, 0), new Vector3(0, 0, -2.9f) } : new[] { new Vector3(-3.3f, 0, 0), new Vector3(3.3f, 0, 0) };
            for (int b = 0; b < offsets.Length; b++)
                Place("Arbustos", gardens, "Arbustos_" + (i + 1) + "_" + (b + 1), p + offsets[b], b * 120 + i * 45, new Vector3(1.8f, 0, 0), Anchor.Bottom, fitLargest: true);
        }
    }

    private static void Portals(Transform root)
    {
        var group = Find(root, "07_PortalsAndWorldThresholds");
        foreach (var portal in group.GetComponentsInChildren<PlazaPortal>(true))
        {
            string key = portal.world < 0 ? "Portal_Underworld" : portal.world > 0 ? "Heaven+Portal" : "Portal1";
            var t = portal.transform;
            Hide(t, "Gate mosaic", "Gate pillar", "Pillar inlay", "Gate footing", "Stone archivolt", "Orbiting gold halo", "Halo jewel", "Inner light");
            var model = Place(key, t, "Portal", Vector3.zero, 0, new Vector3(portal.world == 0 ? 6f : 6.4f, 0, 0), Anchor.Bottom);
            // Veils deleted by hand in the scene are respected: the portal just has no surface.
            if (portal.veil != null) FitVeil(portal, model, portal.world == 0);
        }
    }

    // The veil keeps showing locked/open through its colour (PlazaPortal): it is fitted into the
    // arch opening found by ray casts, or laid on the platform for the ring-shaped return portals.
    private static void FitVeil(PlazaPortal portal, GameObject model, bool platform)
    {
        var colliders = model.GetComponentsInChildren<MeshFilter>().Select(f => f.gameObject.AddComponent<MeshCollider>()).ToList();
        Physics.SyncTransforms();
        try
        {
            var b = WorldBounds(model);
            var t = portal.transform;
            Func<Vector3, Vector3, float, float> cast = (from, dir, max) =>
            {
                var hits = Physics.RaycastAll(from, dir, max).Where(h => colliders.Contains(h.collider as MeshCollider)).OrderBy(h => h.distance).ToArray();
                return hits.Length > 0 ? hits[0].distance : -1;
            };
            float baseTop;
            if (platform)
            {
                float d = cast(new Vector3(b.center.x, b.max.y + 1, b.center.z), Vector3.down, b.size.y + 2);
                baseTop = d < 0 ? b.min.y : b.max.y + 1 - d;
                Sink(model, portal, baseTop, 0.18f, out float shift);
                var veilPlate = portal.veil.transform;
                veilPlate.position = new Vector3(b.center.x, baseTop + shift + 0.03f, b.center.z);
                veilPlate.localScale = new Vector3(b.size.x * 0.42f, 0.04f, b.size.x * 0.42f);
                return;
            }
            // Gate plane: depths where a ray across the portal meets the pillars.
            float y = b.min.y + b.size.y * 0.5f; var depths = new List<float>();
            for (float z = b.min.z; z <= b.max.z; z += 0.05f)
                if (cast(new Vector3(b.min.x - 1, y, z), Vector3.right, b.size.x + 2) >= 0) depths.Add(z);
            float gateZ = depths.Count > 0 ? depths[depths.Count / 2] : b.center.z;
            float down = cast(new Vector3(b.center.x, y, gateZ), Vector3.down, b.size.y);
            baseTop = down < 0 ? b.min.y : y - down;
            float up = cast(new Vector3(b.center.x, baseTop + 0.2f, gateZ), Vector3.up, b.size.y);
            float top = up < 0 ? b.max.y : baseTop + 0.2f + up;
            float mid = baseTop + (top - baseTop) * 0.4f;
            float left = cast(new Vector3(b.center.x, mid, gateZ), Vector3.left, b.size.x);
            float right = cast(new Vector3(b.center.x, mid, gateZ), Vector3.right, b.size.x);
            float width = (left < 0 ? b.size.x * 0.25f : left) + (right < 0 ? b.size.x * 0.25f : right);
            Sink(model, portal, baseTop, 0.18f, out float s);
            var veil = portal.veil.transform;
            veil.position = new Vector3(b.center.x + ((right < 0 ? 0 : right) - (left < 0 ? 0 : left)) * 0.5f, (baseTop + top) * 0.5f + s, gateZ);
            veil.rotation = t.rotation;
            veil.localScale = new Vector3(width * 0.86f, (top - baseTop) * 0.9f, 0.08f);
        }
        finally { foreach (var c in colliders) UnityEngine.Object.DestroyImmediate(c); }
    }

    // The walkable plinth of the tall portal bases is sunk into the pavement, leaving a low step.
    private static void Sink(GameObject model, PlazaPortal portal, float baseTopWorld, float keep, out float shift)
    {
        shift = portal.transform.position.y + keep - baseTopWorld;
        model.transform.position += Vector3.up * shift;
    }

    // ---------------------------------------------------------------- guardian animation

    private static AnimatorController BuildGuardianController()
    {
        AssetDatabase.DeleteAsset(GuardianController);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(GuardianController);
        foreach (string p in new[] { "Hit", "Attack", "Die", "Reset" }) controller.AddParameter(p, AnimatorControllerParameterType.Trigger);
        var sm = controller.layers[0].stateMachine;
        var idle = sm.AddState("Idle", new Vector3(300, 0));
        // Nemequene's Humanoid idle retargets onto the guardian (no idle clip was supplied).
        idle.motion = Clip(Models + "Nemequene/Animations/Orc Idle.fbx");
        sm.defaultState = idle;
        var hit = sm.AddState("Hit", new Vector3(300, 120)); hit.motion = Clip(Models + "Guardian+de+entrenamiento/Standing Block React Large.fbx");
        var attack = sm.AddState("Attack", new Vector3(560, 60)); attack.motion = Clip(Models + "Guardian+de+entrenamiento/Standing 2H Magic Attack 01.fbx");
        var death = sm.AddState("Death", new Vector3(560, -80)); death.motion = Clip(Models + "Guardian+de+entrenamiento/Death From Right.fbx");
        Transition(sm.AddAnyStateTransition(hit), "Hit", 0.08f);
        Transition(sm.AddAnyStateTransition(attack), "Attack", 0.12f);
        Transition(sm.AddAnyStateTransition(death), "Die", 0.1f);
        Transition(sm.AddAnyStateTransition(idle), "Reset", 0.2f);
        foreach (var state in new[] { hit, attack })
        {
            var back = state.AddTransition(idle); back.hasExitTime = true; back.exitTime = 0.92f; back.duration = 0.2f; back.hasFixedDuration = true;
        }
        EditorUtility.SetDirty(controller);
        return controller;
    }
    private static void Transition(AnimatorStateTransition t, string trigger, float duration)
    {
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t.duration = duration; t.hasFixedDuration = true; t.hasExitTime = false; t.canTransitionToSelf = false;
    }
    private static AnimationClip Clip(string path)
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        if (clip == null) throw new InvalidOperationException("No hay clip de animación en " + path);
        return clip;
    }

    // ---------------------------------------------------------------- placement helpers

    // Instantiates a model under parent/MODEL_name, scales it to the requested size in parent
    // space (0 on an axis = keep proportion) and anchors its bounds at localPosition.
    private static GameObject Place(string key, Transform parent, string name, Vector3 localPosition, float yaw, Vector3 size, Anchor anchor,
        bool fitLargest = false, Quaternion? tilt = null)
    {
        var container = new GameObject(Prefix + name).transform;
        container.SetParent(parent, false);
        container.localPosition = localPosition; container.localRotation = Quaternion.Euler(0, yaw, 0);
        var scaler = new GameObject("Scale").transform; scaler.SetParent(container, false);
        var pivot = new GameObject("Orientation").transform; pivot.SetParent(scaler, false); pivot.localRotation = tilt ?? Quaternion.identity;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(LoadModel(key), pivot);
        instance.transform.localPosition = Vector3.zero;
        var material = MaterialFor(key);
        foreach (var r in instance.GetComponentsInChildren<Renderer>(true)) r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();

        var b = LocalBounds(container, instance);
        Vector3 ratio;
        if (fitLargest) { float m = Mathf.Max(b.size.x, b.size.y, b.size.z); float s = size.x / m; ratio = Vector3.one * s; }
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
        foreach (var t in container.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = parent.gameObject.layer;
        return container.gameObject;
    }

    private static GameObject LoadModel(string key)
    {
        string folder = Models + key;
        string path = Directory.GetFiles(folder, "*_Optimizado.fbx").FirstOrDefault()
            ?? Directory.GetFiles(folder, "tripo_convert*.fbx").FirstOrDefault();
        if (path == null) throw new InvalidOperationException("No hay FBX en " + folder);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
    }

    // URP Lit from the Tripo textures: base colour, normal, and a metallic/smoothness map
    // composed from the metallic and roughness maps (URP reads smoothness from alpha).
    private static Material MaterialFor(string key)
    {
        if (Materials.TryGetValue(key, out var cached)) return cached;
        Directory.CreateDirectory(MaterialFolder);
        string name = key.Replace("+", "_").Replace("á", "a");
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.shader = Shader.Find("Universal Render Pipeline/Lit");
        string textures = Directory.GetDirectories(Models + key, "*.fbm").FirstOrDefault()?.Replace('\\', '/');
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
            string packed = MaterialFolder + "/" + name + "_MetallicSmoothness.png";
            if (!File.Exists(packed)) Pack(metallic, roughness, packed);
            AssetDatabase.ImportAsset(packed);
            var importer = (TextureImporter)AssetImporter.GetAtPath(packed);
            if (importer.sRGBTexture || importer.maxTextureSize != 2048) { importer.sRGBTexture = false; importer.maxTextureSize = 2048; importer.SaveAndReimport(); }
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(packed));
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Metallic", 1); material.SetFloat("_Smoothness", 1);
            material.SetFloat("_SmoothnessTextureChannel", 0);
        }
        else { material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", 0.3f); }
        material.enableInstancing = true;
        if (DoubleSided.Contains(key)) { material.SetFloat("_Cull", 0); material.doubleSidedGI = true; }
        EditorUtility.SetDirty(material);
        Materials[key] = material;
        return material;
    }
    private static void Pack(string metallicPath, string roughnessPath, string output)
    {
        var metallic = new Texture2D(2, 2); metallic.LoadImage(File.ReadAllBytes(metallicPath));
        var roughness = new Texture2D(2, 2); roughness.LoadImage(File.ReadAllBytes(roughnessPath));
        int w = Mathf.Min(metallic.width, 2048), h = Mathf.Min(metallic.height, 2048);
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

    private static Bounds WorldBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds); return b;
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

    private static Transform Find(Transform parent, string name)
    {
        var t = parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
        if (t == null) throw new InvalidOperationException("No se encontró '" + name + "' en " + parent.name);
        return t;
    }
    private static Transform[] Children(Transform parent, string name) => parent.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
    private static Vector3 Flat(Vector3 p) => new Vector3(p.x, 0, p.z);

    // Turns the graybox renderers off (all of them, or only those named); colliders stay.
    private static void Hide(Transform root, params string[] names)
    {
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentsInParent<Transform>(true).Any(t => t.name.StartsWith(Prefix))) continue;
            if (names.Length == 0 || names.Contains(r.gameObject.name)) r.enabled = false;
        }
    }
}

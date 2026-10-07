using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Builds the rigged cast of the campaign from Assets/_Game/Art/Characters (2026-10-06):
//  - Bacata_Humanoid.controller: locomotion plus the acting states (Talk, Point, Kneel, Attack,
//    Block, Death...) made of Nemequene's Mixamo clips, shared by every humanoid through
//    Humanoid retargeting; a "calm" override swaps the combat idle for Breathing Idle.
//  - Serpiente / Jaguar / Guacamaya controllers from their *_Animado.fbx takes.
//  - The same acting states added to Nemequene_Player.controller (they return to locomotion).
//  - One prefab per character in Assets/_Game/Resources/Characters/<Key>.prefab: root at the
//    feet facing +Z, the model scaled to its height, URP material from the .fbm textures,
//    Animator and CharacterActions. The scenes already load these keys (StoryProps.Figure,
//    BacataDirector, MSDuelEncounter, CharacterModels).
// Runs once after compiling (EditorPrefs key) and from Nemequene > Personajes > Construir personajes.
[InitializeOnLoad]
public static class CharacterLibrarySetup
{
    private const string ArtRoot = "Assets/_Game/Art/Characters/";
    private const string Shared = ArtRoot + "_Compartido/";
    private const string Clips = ArtRoot + "Nemequene/Animations/";
    private const string Output = "Assets/_Game/Resources/Characters/";
    private const string PlayerController = ArtRoot + "Nemequene/Nemequene_Player.controller";
    private const string PlayerPrefab = ArtRoot + "Nemequene/Nemequene_Player_Visual.prefab";
    private const string AutoRunKey = "Bacata.CharacterLibrary.v1";

    private enum Kind { Mixamo, TripoHuman, Creature }
    private sealed class Cast
    {
        public string Key, Folder, Model; public Kind Kind; public float Height; public bool Calm; public float Yaw;
        public Cast(string key, string folder, Kind kind, float height, bool calm, string model = null, float yaw = 0)
        { Key = key; Folder = folder; Kind = kind; Height = height; Calm = calm; Model = model; Yaw = yaw; }
    }

    // Key = Resources name the game loads. Heights in metres (creatures: standing height).
    private static readonly Cast[] Characters =
    {
        new Cast("Nemequene", "Nemequene", Kind.Mixamo, 1.78f, true, "tripo_convert_d15b6933-3dfe-4830-bee3-862d8d530ca7.fbx"),
        new Cast("Nemequeneniño", "Nemeneque+Niño", Kind.Mixamo, 1.25f, true),
        new Cast("Tisquesusa", "Tisquesusa", Kind.TripoHuman, 1.74f, true),
        new Cast("Bachue", "Bachué", Kind.Mixamo, 1.7f, true),
        new Cast("Furachogua", "Furachogua", Kind.Mixamo, 1.75f, true),
        new Cast("Invasor", "Invasor+de+plata+y+oro", Kind.Mixamo, 1.85f, false),
        new Cast("Quimue", "Quimue", Kind.Mixamo, 1.95f, false),
        new Cast("CustodiodeRaices", "Custodio+de+raices", Kind.Mixamo, 2.1f, true),
        new Cast("MujerCondor", "Mujer+condor", Kind.TripoHuman, 2.3f, false, "Mujer_condor_Optimizado.fbx"),
        new Cast("MujerAguila", "Mujer+águila", Kind.Mixamo, 2.2f, false),
        new Cast("HombreCaiman", "Hombre+caiman", Kind.Mixamo, 2.0f, false),
        new Cast("HombreCaimanEscudo", "Hombre+caiman+++escudo", Kind.Mixamo, 2.0f, false),
        new Cast("HombreMurcielago", "Hombre+murciélago", Kind.Mixamo, 1.9f, false),
        new Cast("JefeLagartoMurcielago", "Jefe+Lagarto-murciélago", Kind.Mixamo, 4.2f, false),
        new Cast("Serpiente", "Serpiente+Bicéfala", Kind.Creature, 6.0f, false, "Animations/Serpiente_Animado.fbx"),
        new Cast("Jaguar", "Jaguar+(Transformación)", Kind.Creature, 1.25f, false, "Animations/Jaguar_Animado.fbx"),
        new Cast("Guacamaya", "Guacamaya+(Transformación)", Kind.Creature, 1.1f, false, "Animations/Guacamaya_Animado.fbx"),
    };

    // Acting states of the humanoids: state, Nemequene clip, loop, held (stays until Rest).
    private static readonly (string state, string clip, bool loop, bool hold)[] Actions =
    {
        ("Talk", "Talking", true, true), ("Talk2", "Talking (1)", true, true), ("Point", "Pointing", false, false),
        ("Reach", "Reaching Out", false, false), ("Pickup", "Picking Up Object", false, false),
        ("Lever", "Pulling Lever", false, false), ("Button", "Button Pushing", false, false), ("Open", "Opening", false, false),
        ("Pray", "Praying", true, true), ("Sit", "Sitting Idle", true, true), ("Kneel", "Kneeling Down", false, true),
        ("Crouch", "Standing To Crouch", false, true), ("Stand", "Crouch To Standing", false, false),
        ("Attack", "Stable Sword Inward Slash", false, false), ("Combo", "Standing Melee Combo Attack Ver. 3", false, false),
        ("Spin", "Standing Melee Attack 360 Low", false, false), ("Block", "Sword And Shield Block", false, false),
        ("PowerUp", "Sword And Shield Power Up", false, false), ("DodgeLeft", "Standing Dodge Left", false, false),
        ("DodgeRight", "Dodging Right", false, false), ("HitLeft", "Standing React Large From Left", false, false),
        ("HitRight", "Standing React Large From Right", false, false), ("HitGut", "Standing React Large Gut", false, false),
        ("Death", "Dying", false, true), ("DeathBack", "Dying Backwards", false, true), ("Land", "Landing", false, false),
    };

    // Creature states: state = take name; one-shots return to `next` (default Idle).
    private static readonly Dictionary<string, (string state, string next)[]> CreatureStates = new Dictionary<string, (string, string)[]>
    {
        { "Serpiente", new[] { ("Idle", null), ("AtaqueA", "Idle"), ("SacudidaA", "Idle"), ("AtaqueB", "Idle"), ("PulsoB", "Idle"),
            ("GolpeA", "Idle"), ("GolpeB", "Idle"), ("Emerger", "Idle"), ("Liberada", "Reposo"), ("Reposo", null) } },
        { "Jaguar", new[] { ("Idle", null), ("Caminar", null), ("Correr", null), ("Embestida", "Idle"), ("Rugido", "Idle"), ("Golpe", "Idle") } },
        { "Guacamaya", new[] { ("Idle", null), ("Despegue", "Vuelo"), ("Vuelo", null), ("Planeo", null), ("Aterrizaje", "Idle") } },
    };

    static CharacterLibrarySetup()
    {
        string key = AutoRunKey + ":" + Application.dataPath;
        if (EditorPrefs.GetBool(key) || Application.isBatchMode) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(key)) return;
            EditorPrefs.SetBool(key, true);
            Run();
        };
    }

    [MenuItem("Nemequene/Personajes/Construir personajes")]
    public static void Run()
    {
        var report = new List<string>();
        try
        {
            Directory.CreateDirectory(Shared); Directory.CreateDirectory(Output);
            ReimportCast();
            var humanoid = BuildHumanoidController();
            var calm = BuildCalmOverride(humanoid);
            AddPlayerActions();
            foreach (var c in Characters)
            {
                try { report.Add(BuildCharacter(c, humanoid, calm)); }
                catch (Exception e) { report.Add(c.Key + ": ERROR " + e.Message); Debug.LogWarning("CHARACTER_FAILED " + c.Key + ": " + e); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("CHARACTERS_OK\n" + string.Join("\n", report));
        }
        catch (Exception e)
        {
            Debug.LogError("CHARACTERS_FAILED: " + e);
        }
    }

    // Batch entry for the scratch validation copy.
    public static void RunBatch()
    {
        Run();
        EditorApplication.Exit(0);
    }

    private static void ReimportCast()
    {
        foreach (var c in Characters)
        {
            string folder = ArtRoot + c.Folder;
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            foreach (string guid in AssetDatabase.FindAssets("", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (c.Key == "Nemequene") continue; // has its own importer and setup
                var importer = AssetImporter.GetAtPath(path);
                if (importer is ModelImporter model)
                {
                    // Re-run CharacterImportSettings only where the stored settings disagree.
                    bool creature = c.Kind == Kind.Creature, generic = creature || c.Kind == Kind.TripoHuman;
                    var want = generic ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.Human;
                    bool clipsOk = !path.EndsWith("_Animado.fbx") || model.clipAnimations.Length > 0;
                    if (model.animationType != want || model.materialImportMode != ModelImporterMaterialImportMode.None || !clipsOk)
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }
                else if (importer is TextureImporter texture && texture.maxTextureSize != 2048)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
        }
    }

    // ------------------------------------------------------------------ controllers

    private static AnimationClip Clip(string file)
    {
        return AssetDatabase.LoadAllAssetsAtPath(Clips + file + ".fbx").OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    private static AnimatorController BuildHumanoidController()
    {
        string path = Shared + "Bacata_Humanoid.controller";
        AssetDatabase.DeleteAsset(path);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var sm = controller.layers[0].stateMachine;
        var locomotion = sm.AddState("Locomotion", new Vector3(300, 0));
        var tree = new BlendTree { name = "Locomotion", blendType = BlendTreeType.Simple1D, blendParameter = "Speed",
            useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(Clip("Orc Idle"), 0f);
        tree.AddChild(Clip("Walking"), 1.5f);
        tree.AddChild(Clip("Running"), 5f);
        locomotion.motion = tree;
        sm.defaultState = locomotion;
        AddActions(sm, locomotion, holdPoses: true);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    // Calm characters (Bachué, Tisquesusa, the Custodio...) rest in Breathing Idle.
    private static AnimatorOverrideController BuildCalmOverride(AnimatorController controller)
    {
        string path = Shared + "Bacata_Humanoid_Sereno.overrideController";
        AssetDatabase.DeleteAsset(path);
        var calm = new AnimatorOverrideController(controller) { name = "Bacata_Humanoid_Sereno" };
        var idle = Clip("Orc Idle"); var breathing = Clip("Breathing Idle");
        if (idle != null && breathing != null) calm[idle] = breathing;
        AssetDatabase.CreateAsset(calm, path);
        return calm;
    }

    private static void AddActions(AnimatorStateMachine sm, AnimatorState rest, bool holdPoses)
    {
        int row = 0;
        foreach (var (state, file, loop, hold) in Actions)
        {
            if (sm.states.Any(s => s.state.name == state)) continue;
            var clip = Clip(file);
            if (clip == null) { Debug.LogWarning("CHARACTERS: falta el clip " + file); continue; }
            var s = sm.AddState(state, new Vector3(650 + 230 * (row % 3), -300 + 60 * (row / 3)));
            s.motion = clip;
            row++;
            // Held poses wait for CharacterActions.Rest; everything else returns to rest by itself.
            if (hold && holdPoses) continue;
            if (loop && holdPoses) continue;
            var t = s.AddTransition(rest);
            t.hasExitTime = true; t.exitTime = loop ? 1f : .9f; t.duration = .2f; t.hasFixedDuration = true;
        }
    }

    // The player keeps moving: every acting state returns to locomotion when its clip ends.
    private static void AddPlayerActions()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerController);
        if (controller == null) { Debug.LogWarning("CHARACTERS: falta " + PlayerController); return; }
        var sm = controller.layers[0].stateMachine;
        var locomotion = sm.states.First(s => s.state.name == "Locomotion").state;
        AddActions(sm, locomotion, holdPoses: false);
        EditorUtility.SetDirty(controller);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        if (prefab != null && prefab.GetComponent<CharacterActions>() == null)
        {
            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefab);
            var actions = contents.AddComponent<CharacterActions>();
            actions.restState = "Locomotion";
            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefab);
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static AnimatorController BuildCreatureController(Cast c, string modelPath)
    {
        string path = ArtRoot + c.Folder + "/" + c.Key + ".controller";
        AssetDatabase.DeleteAsset(path);
        var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
            .Where(a => !a.name.StartsWith("__preview__")).ToDictionary(a => a.name);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm = controller.layers[0].stateMachine;
        var states = new Dictionary<string, AnimatorState>();
        int i = 0;
        foreach (var (state, _) in CreatureStates[c.Key])
        {
            if (!clips.TryGetValue(state, out var clip)) { Debug.LogWarning("CHARACTERS: " + c.Key + " sin toma " + state); continue; }
            var s = sm.AddState(state, new Vector3(300 + 220 * (i % 3), 120 * (i / 3)));
            s.motion = clip; states[state] = s; i++;
        }
        if (states.TryGetValue("Idle", out var idle)) sm.defaultState = idle;
        foreach (var (state, next) in CreatureStates[c.Key])
        {
            if (next == null || !states.ContainsKey(state) || !states.ContainsKey(next)) continue;
            var t = states[state].AddTransition(states[next]);
            t.hasExitTime = true; t.exitTime = .95f; t.duration = .2f; t.hasFixedDuration = true;
        }
        EditorUtility.SetDirty(controller);
        return controller;
    }

    // ------------------------------------------------------------------ prefabs

    private static string BuildCharacter(Cast c, AnimatorController humanoid, AnimatorOverrideController calm)
    {
        string folder = ArtRoot + c.Folder;
        if (!AssetDatabase.IsValidFolder(folder)) return c.Key + ": falta la carpeta " + folder;
        string modelPath = c.Model != null ? folder + "/" + c.Model
            : AssetDatabase.FindAssets("t:Model", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => Path.GetDirectoryName(p).Replace('\\', '/') == folder && Path.GetFileName(p).StartsWith("tripo_convert"));
        var model = modelPath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) : null;
        if (model == null) return c.Key + ": falta el modelo en " + folder;

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
        RuntimeAnimatorController controller = c.Calm ? (RuntimeAnimatorController)calm : humanoid;
        string rest = "Locomotion";
        if (c.Kind == Kind.TripoHuman) avatar = BuildTripoAvatar(c, model);
        if (c.Kind == Kind.Creature) { controller = BuildCreatureController(c, modelPath); rest = "Idle"; }
        if (c.Kind != Kind.Creature && (avatar == null || !avatar.isValid || !avatar.isHuman))
            return c.Key + ": ERROR avatar Humanoid no válido (" + modelPath + ")";
        var material = BuildMaterial(c, folder);

        var root = new GameObject(c.Key);
        try
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            instance.name = "Modelo";
            instance.transform.localRotation = Quaternion.Euler(0, c.Yaw, 0);
            var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
            {
                if (material != null) r.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, r.sharedMaterials.Length)).ToArray();
                if (r is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;
            }
            // Height and feet: the bounds of the rest pose, scaled to the character's height,
            // bottom on the root, centred on it.
            var b = Bounds(root, instance);
            if (b.size.y > .001f) instance.transform.localScale = Vector3.one * (c.Height / b.size.y);
            b = Bounds(root, instance);
            instance.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
            var actions = root.AddComponent<CharacterActions>();
            actions.restState = rest;
            var so = new SerializedObject(actions);
            so.FindProperty("animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Output + c.Key + ".prefab");
            return c.Key + ": ok (" + Path.GetFileName(modelPath) + ", " + c.Height + " m)";
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static Bounds Bounds(GameObject root, GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
        Bounds b = default; bool first = true;
        foreach (var r in renderers)
        {
            var local = r is SkinnedMeshRenderer s && s.sharedMesh != null ? s.sharedMesh.bounds : r.localBounds;
            // Corners of the mesh bounds in the root's space.
            var m = root.transform.worldToLocalMatrix * r.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                var corner = local.center + Vector3.Scale(local.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var p = m.MultiplyPoint3x4(corner);
                if (first) { b = new Bounds(p, Vector3.zero); first = false; } else b.Encapsulate(p);
            }
        }
        return b;
    }

    // URP Lit from the textures Tripo left in <folder>/*.fbm: base colour and normal map.
    private static Material BuildMaterial(Cast c, string folder)
    {
        var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        string Pick(Func<string, bool> match) => textures.FirstOrDefault(p => match(Path.GetFileNameWithoutExtension(p).ToLowerInvariant()));
        string baseMap = Pick(n => n.Contains("basecolor")) ?? Pick(n => n == "color") ?? Pick(n => n.StartsWith("tripo_rgb"))
            ?? Pick(n => !n.Contains("normal") && !n.Contains("metallic") && !n.Contains("rough") && !n.EndsWith("_rm"));
        string normal = Pick(n => n.Contains("normal"));
        string path = folder + "/" + c.Key + "_URP.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader;
        material.SetColor("_BaseColor", Color.white);
        material.SetTexture("_BaseMap", baseMap != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(baseMap) : null);
        var bump = normal != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(normal) : null;
        material.SetTexture("_BumpMap", bump);
        if (bump != null) material.EnableKeyword("_NORMALMAP"); else material.DisableKeyword("_NORMALMAP");
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", .25f);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Tripo humanoid rig (Root > Hip > Pelvis/Waist...): explicit Humanoid map, skeleton from the model.
    private static readonly (string human, string bone)[] TripoMap =
    {
        ("Hips", "Hip"), ("Spine", "Waist"), ("Chest", "Spine01"), ("UpperChest", "Spine02"), ("Neck", "NeckTwist01"), ("Head", "Head"),
        ("LeftUpperLeg", "L_Thigh"), ("LeftLowerLeg", "L_Calf"), ("LeftFoot", "L_Foot"), ("LeftToes", "L_ToeBase"),
        ("RightUpperLeg", "R_Thigh"), ("RightLowerLeg", "R_Calf"), ("RightFoot", "R_Foot"), ("RightToes", "R_ToeBase"),
        ("LeftShoulder", "L_Clavicle"), ("LeftUpperArm", "L_Upperarm"), ("LeftLowerArm", "L_Forearm"), ("LeftHand", "L_Hand"),
        ("RightShoulder", "R_Clavicle"), ("RightUpperArm", "R_Upperarm"), ("RightLowerArm", "R_Forearm"), ("RightHand", "R_Hand"),
    };

    private static Avatar BuildTripoAvatar(Cast c, GameObject model)
    {
        var instance = UnityEngine.Object.Instantiate(model);
        instance.name = model.name;
        try
        {
            var all = instance.GetComponentsInChildren<Transform>(true);
            Transform Find(string name) => all.FirstOrDefault(t => t.name == name || t.name.EndsWith(":" + name));
            var human = new List<HumanBone>();
            foreach (var (h, bone) in TripoMap)
            {
                var t = Find(bone);
                if (t == null) { Debug.LogWarning("CHARACTERS: " + c.Key + " sin hueso " + bone); continue; }
                human.Add(new HumanBone { humanName = h, boneName = t.name, limit = new HumanLimit { useDefaultValues = true } });
            }
            var skeleton = all.Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray();
            var description = new HumanDescription
            {
                human = human.ToArray(), skeleton = skeleton,
                upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f,
                armStretch = .05f, legStretch = .05f, feetSpacing = 0, hasTranslationDoF = false,
            };
            var avatar = AvatarBuilder.BuildHumanAvatar(instance, description);
            avatar.name = c.Key + "_Avatar";
            string path = ArtRoot + c.Folder + "/" + c.Key + "_Avatar.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(avatar, path);
            if (!avatar.isValid || !avatar.isHuman) Debug.LogWarning("CHARACTERS: avatar de " + c.Key + " no válido");
            return avatar;
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }
}

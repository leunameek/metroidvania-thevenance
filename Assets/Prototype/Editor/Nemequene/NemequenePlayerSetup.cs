using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the animated Nemequene player visual (material, Animator Controller, prefab)
// from Assets/Models/Nemequene and puts it on the PlayerController in the demo scenes.
// Runs once automatically after the scripts compile; re-run from the Tools menu.
[InitializeOnLoad]
public static class NemequenePlayerSetup
{
    private const string Folder = NemequeneImportSettings.ModelFolder;
    private const string AnimFolder = NemequeneImportSettings.AnimationFolder;
    private const string ModelPath = Folder + "/tripo_convert_d15b6933-3dfe-4830-bee3-862d8d530ca7.fbx";
    private const string TextureFolder = Folder + "/tripo_convert_d15b6933-3dfe-4830-bee3-862d8d530ca7.fbm";
    private const string MaterialPath = Folder + "/Nemequene_Player_URP.mat";
    private const string ControllerPath = Folder + "/Nemequene_Player.controller";
    private const string PrefabPath = Folder + "/Nemequene_Player_Visual.prefab";
    private const string VisualName = "Nemequene_Visual";
    private const string AutoRunKey = "Nemequene.PlayerSetup.v1";

    private static readonly string[] Scenes =
    {
        "Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity",
        "Assets/Prototype/Scenes/Movement.unity",
    };

    static NemequenePlayerSetup()
    {
        string key = AutoRunKey + ":" + Application.dataPath;
        if (EditorPrefs.GetBool(key)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(key)) return;
            EditorPrefs.SetBool(key, true);
            Run();
        };
    }

    [MenuItem("Tools/Nemequene/Setup animated player character")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        try
        {
            ReimportFolder();
            var material = BuildMaterial();
            var controller = BuildController();
            var prefab = BuildPrefab(controller, material);
            var openScenes = Enumerable.Range(0, EditorSceneManager.sceneCount)
                .Select(i => EditorSceneManager.GetSceneAt(i).path).Where(p => !string.IsNullOrEmpty(p)).ToArray();
            foreach (string scenePath in Scenes) ApplyToScene(scenePath, prefab);
            if (openScenes.Length > 0)
            {
                EditorSceneManager.OpenScene(openScenes[0], OpenSceneMode.Single);
                for (int i = 1; i < openScenes.Length; i++) EditorSceneManager.OpenScene(openScenes[i], OpenSceneMode.Additive);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("NEMEQUENE_PLAYER_OK: personaje animado aplicado a " + string.Join(", ", Scenes));
        }
        catch (Exception e)
        {
            Debug.LogError("NEMEQUENE_PLAYER_FAILED: " + e);
        }
    }

    private static void ReimportFolder()
    {
        // Makes sure NemequeneImportSettings is applied even if Unity imported the FBX first.
        foreach (string guid in AssetDatabase.FindAssets("", new[] { Folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is ModelImporter || AssetImporter.GetAtPath(path) is TextureImporter)
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
        if (importer == null) throw new InvalidOperationException("Missing " + ModelPath);
        var avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isValid || !avatar.isHuman)
            throw new InvalidOperationException("El avatar Humanoid de Nemequene no es válido; revisa el Rig del FBX.");
    }

    private static Material BuildMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        material.shader = shader;
        material.SetColor("_BaseColor", Color.white);
        material.SetTexture("_BaseMap", FindTexture("_basecolor"));
        var normal = FindTexture("_normal");
        material.SetTexture("_BumpMap", normal);
        if (normal != null) material.EnableKeyword("_NORMALMAP");
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", 0.3f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D FindTexture(string suffix)
    {
        return AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => System.IO.Path.GetFileNameWithoutExtension(p).ToLowerInvariant().EndsWith(suffix))
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
            .FirstOrDefault();
    }

    private static AnimationClip Clip(string fileName)
    {
        string path = AnimFolder + "/" + fileName + ".fbx";
        var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        if (clip == null) throw new InvalidOperationException("No animation clip in " + path);
        return clip;
    }

    private static AnimatorController BuildController()
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
        controller.AddParameter("Dashing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("OnLadder", AnimatorControllerParameterType.Bool);
        controller.AddParameter("ClimbSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("AirJump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("HardLand", AnimatorControllerParameterType.Trigger);
        var parameters = controller.parameters;
        parameters.First(p => p.name == "Grounded").defaultBool = true;
        parameters.First(p => p.name == "ClimbSpeed").defaultFloat = 1f;
        controller.parameters = parameters;

        var sm = controller.layers[0].stateMachine;

        var locomotion = sm.AddState("Locomotion", new Vector3(300, 0));
        var tree = new BlendTree
        {
            name = "Locomotion",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed",
            useAutomaticThresholds = false,
            hideFlags = HideFlags.HideInHierarchy
        };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(Clip("Orc Idle"), 0f);
        tree.AddChild(Clip("Walking"), 2.5f);
        tree.AddChild(Clip("Running"), 6f);
        locomotion.motion = tree;
        sm.defaultState = locomotion;

        var jump = sm.AddState("Jump", new Vector3(550, -120));
        jump.motion = Clip("Jumping Up");
        jump.speed = 1.3f;
        var airJump = sm.AddState("AirJump", new Vector3(800, -120));
        airJump.motion = Clip("Front Flip");
        airJump.speed = 1.4f;
        var fall = sm.AddState("Fall", new Vector3(650, 60));
        fall.motion = Clip("Falling Idle");
        var land = sm.AddState("HardLand", new Vector3(400, 200));
        land.motion = Clip("Hard Landing");
        land.speed = 1.5f;
        var dash = sm.AddState("Dash", new Vector3(50, 200));
        dash.motion = Clip("Running");
        dash.speed = 1.8f;
        var climb = sm.AddState("Climb", new Vector3(50, -150));
        climb.motion = Clip("Climbing Ladder");
        climb.speedParameterActive = true;
        climb.speedParameter = "ClimbSpeed";

        // Any State: ladder and dash win over everything, then the two jump triggers.
        var t = sm.AddAnyStateTransition(climb); Setup(t, 0.15f);
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, "OnLadder");
        t = sm.AddAnyStateTransition(dash); Setup(t, 0.05f);
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, "Dashing");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "OnLadder");
        t = sm.AddAnyStateTransition(airJump); Setup(t, 0.08f);
        t.canTransitionToSelf = true;
        t.AddCondition(AnimatorConditionMode.If, 0, "AirJump");
        t = sm.AddAnyStateTransition(jump); Setup(t, 0.08f);
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, "Jump");

        t = locomotion.AddTransition(fall); Setup(t, 0.2f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        t = jump.AddTransition(locomotion); Setup(t, 0.15f);
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        t = jump.AddTransition(fall); Setup(t, 0.3f);
        t.AddCondition(AnimatorConditionMode.Less, -1f, "VerticalVelocity");

        t = airJump.AddTransition(locomotion); Setup(t, 0.15f);
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        t = airJump.AddTransition(fall); Setup(t, 0.25f, 0.85f);

        t = fall.AddTransition(land); Setup(t, 0.05f);
        t.AddCondition(AnimatorConditionMode.If, 0, "HardLand");
        t = fall.AddTransition(locomotion); Setup(t, 0.12f);
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");

        t = land.AddTransition(locomotion); Setup(t, 0.25f, 0.6f);
        t = land.AddTransition(locomotion); Setup(t, 0.2f);
        t.AddCondition(AnimatorConditionMode.Greater, 1f, "Speed");
        t = land.AddTransition(fall); Setup(t, 0.2f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        t = dash.AddTransition(locomotion); Setup(t, 0.12f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Dashing");
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        t = dash.AddTransition(fall); Setup(t, 0.15f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Dashing");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        t = climb.AddTransition(locomotion); Setup(t, 0.2f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "OnLadder");
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        t = climb.AddTransition(fall); Setup(t, 0.2f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "OnLadder");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void Setup(AnimatorStateTransition t, float duration, float exitTime = -1f)
    {
        t.duration = duration;
        t.hasFixedDuration = true;
        t.hasExitTime = exitTime >= 0f;
        if (t.hasExitTime) t.exitTime = exitTime;
        t.interruptionSource = TransitionInterruptionSource.Destination;
    }

    private static GameObject BuildPrefab(AnimatorController controller, Material material)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        try
        {
            instance.name = VisualName;
            var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().First();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (instance.GetComponent<PlayerAnimator>() == null) instance.AddComponent<PlayerAnimator>();
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("El modelo no tiene renderers.");
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, renderer.sharedMaterials.Length)).ToArray();
                if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;
            }
            return PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static void ApplyToScene(string scenePath, GameObject prefab)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null) return;
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player == null)
        {
            Debug.LogWarning("Sin PlayerController en " + scenePath);
            return;
        }

        // Remove the previous visual (old idle-only Nemequene) and hide the placeholder capsule.
        foreach (Transform child in player.transform.Cast<Transform>().ToArray())
            if (child.name == VisualName) UnityEngine.Object.DestroyImmediate(child.gameObject);
        var capsule = player.GetComponent<MeshRenderer>();
        if (capsule != null) capsule.enabled = false;

        var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
        visual.name = VisualName;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;
        foreach (Transform t in visual.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = player.gameObject.layer;

        // Fit the model to the CharacterController capsule: same height, and the Animator
        // origin (where Humanoid clips put the feet) on the capsule bottom.
        var cc = player.GetComponent<CharacterController>();
        float capsuleHeight = cc.height * player.transform.lossyScale.y;
        var bounds = WorldBounds(visual);
        if (bounds.size.y > 0.01f)
        {
            float scale = capsuleHeight * 0.98f / bounds.size.y / player.transform.lossyScale.y;
            visual.transform.localScale = Vector3.one * scale;
        }
        float bottom = player.transform.TransformPoint(cc.center).y - capsuleHeight * 0.5f - cc.skinWidth;
        var origin = visual.transform.position;
        visual.transform.position = new Vector3(origin.x, bottom, origin.z);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Bounds WorldBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        var bounds = renderers[0].bounds;
        foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
        return bounds;
    }
}

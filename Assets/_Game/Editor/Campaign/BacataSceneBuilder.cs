using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Creates Assets/_Game/Scenes/Bacata.unity (prologue and epilogue of the campaign) and lists it in
// Build Settings. The scene holds only a camera, the sun and the BacataDirector, which builds the
// four places at runtime; character prefabs are assigned here when they exist. Re-running it
// refreshes the prefab references without touching anything else.
public static class BacataSceneBuilder
{
    public const string ScenePath = "Assets/_Game/Scenes/Bacata.unity";
    private const string NemequenePrefab = "Assets/_Game/Art/Characters/Nemequene/Nemequene_Player_Visual.prefab";

    [MenuItem("Nemequene/Campaña/Crear o actualizar escena de Bacatá")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) return;
        var scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var director = Object.FindFirstObjectByType<BacataDirector>();
        if (director == null) director = new GameObject("BacataDirector").AddComponent<BacataDirector>();
        var so = new SerializedObject(director);
        Assign(so, "nemequenePrefab", NemequenePrefab);
        Assign(so, "tisquesusaPrefab", "Assets/_Game/Art/Characters/Tisquesusa/Tisquesusa.prefab");
        Assign(so, "saguanmachicaPrefab", "Assets/_Game/Art/Characters/Saguanmachica/Saguanmachica.prefab");
        Assign(so, "bachuePrefab", "Assets/_Game/Art/Characters/Bachue/Bachue.prefab");
        Assign(so, "furachoguaPrefab", "Assets/_Game/Art/Characters/Furachogua/Furachogua.prefab");
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.type == LightType.Directional) so.FindProperty("sun").objectReferenceValue = light;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuild(ScenePath);
        Debug.Log("BACATA_SCENE_OK " + ScenePath);
    }

    private static void Assign(SerializedObject so, string field, string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null) so.FindProperty(field).objectReferenceValue = prefab;
    }

    private static void AddToBuild(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == path) return;
        // Right after the title, before the plaza: the order a new game follows.
        int index = Mathf.Min(1, scenes.Count);
        scenes.Insert(index, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // Batch entry: Unity -batchmode -executeMethod BacataSceneBuilder.BuildBatch -quit
    public static void BuildBatch() => Build();
}

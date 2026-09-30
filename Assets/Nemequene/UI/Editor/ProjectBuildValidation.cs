using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nemequene.UI.Editor
{
    // Run on every build so assembly moves or broken scene references cannot ship silently.
    public sealed class ProjectBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) { Validate(); }

        [MenuItem("Tools/Nemequene/Validate project structure")]
        public static void Validate()
        {
            var results = new List<string>();
            CheckAssembly(typeof(PlayerAbilityModel), "Prototype.Model", "Prototype.Runtime", "Nemequene.UI.Runtime");
            CheckAssembly(typeof(PlayerController), "Prototype.Runtime", "Nemequene.UI.Runtime");
            CheckAssembly(typeof(UIManager), "Nemequene.UI.Runtime");
            results.Add("PASS: UI -> gameplay -> models; no reverse project dependencies.");

            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            Require(scenes.Length > 0 && scenes[0] == TitleMenuController.ScenePath, "Main menu must be the first build scene.");
            Require(scenes.Distinct().Count() == scenes.Length, "Duplicate scenes in Build Settings.");
            Require(scenes.Contains(TitleMenuController.TutorialPath), "Gameplay scene missing from Build Settings.");
            Require(scenes.Any(s => Path.GetFileNameWithoutExtension(s) == "Hand Landmark Detection"), "Additive hand-tracking scene missing from Build Settings.");
            foreach (string path in scenes)
            {
                Require(AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null, "Build scene is missing: " + path);
                var scene = EditorSceneManager.OpenPreviewScene(path);
                try
                {
                    foreach (var root in scene.GetRootGameObjects()) CheckComponents(root, path);
                }
                finally { EditorSceneManager.ClosePreviewScene(scene); }
                results.Add("PASS scene: " + path);
            }

            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prototype", "Assets/Nemequene" });
            foreach (string guid in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CheckComponents(AssetDatabase.LoadAssetAtPath<GameObject>(path), path);
            }
            results.Add("PASS: " + prefabs.Length + " project prefabs contain no missing scripts.");
            var theme = Resources.Load<UITheme>("Nemequene/Theme");
            Require(theme != null && theme.bodyFont != null && theme.titleFont != null, "UI theme or fonts are missing.");
            Require(Resources.Load<GameObject>("Nemequene/UI_Root")?.GetComponent<UIManager>() != null, "UI root prefab is missing its controller.");
            results.Add("PASS: UI root, theme and fonts resolve after assembly migration.");
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/architecture-structure.txt", results);
            Debug.Log("NEMEQUENE_STRUCTURE_PASS: " + scenes.Length + " scenes, " + prefabs.Length + " prefabs, 3 runtime assemblies.");
        }

        private static void CheckAssembly(Type type, string expected, params string[] forbidden)
        {
            var assembly = type.Assembly;
            Require(assembly.GetName().Name == expected, type.Name + " must belong to " + expected);
            foreach (var reference in assembly.GetReferencedAssemblies())
                Require(!forbidden.Contains(reference.Name), expected + " cannot depend on " + reference.Name);
        }

        private static void CheckComponents(GameObject root, string path)
        {
            Require(root != null, "Prefab could not be loaded: " + path);
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0,
                    "Missing script: " + path + " / " + transform.name);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new BuildFailedException(message);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// The models of Bacatá and of the story pieces (Art/Environments/Bacatá, Tripo, 2026-10-07) as
// ready prefabs the runtime loads by name:
//  - Resources/Bacata/<Key>: the village, the hill, the refuge and the lagoon (BacataDirector).
//  - Resources/Props/<Key>: coca, masks, urns, yopo, poporo, staff, bow and arrow (StoryProps).
// Each prefab: root at the base centre (the masks at their centre), the model scaled to its real
// size, a URP material from the textures beside it, no colliders (they are scenery of cinematics).
// Runs once after compiling (EditorPrefs key) and from Nemequene > Campaña > Modelos de Bacatá.
[InitializeOnLoad]
public static class BacataModelSetup
{
    private const string ArtRoot = "Assets/_Game/Art/Environments/Bacatá/";
    private const string AutoRunKey = "Bacata.Models.v1";
    private enum Fit { Height, Largest, Footprint }

    // folder, prefab key, Resources folder, size in metres, how it is measured, centred pivot.
    private static readonly (string folder, string key, string target, float size, Fit fit, bool centred)[] Models =
    {
        ("bohio1", "Bohio1", "Bacata", 5.2f, Fit.Footprint, false),
        ("Bohio2", "Bohio2", "Bacata", 4.6f, Fit.Footprint, false),
        ("Casa+zipa", "CasaZipa", "Bacata", 11f, Fit.Footprint, false),
        ("Empalizada", "Empalizada", "Bacata", 3f, Fit.Footprint, false),
        ("Piedravertical1", "PiedraVertical1", "Bacata", 1.6f, Fit.Height, false),
        ("Piedravertical2", "PiedraVertical2", "Bacata", 1.3f, Fit.Height, false),
        ("Piedravertical3", "PiedraVertical3", "Bacata", 1.9f, Fit.Height, false),
        ("Piedraofrenda", "PiedraOfrenda", "Bacata", 1.4f, Fit.Footprint, false),
        ("Interior+refugio", "InteriorRefugio", "Bacata", 9.5f, Fit.Footprint, false),
        ("Fogon", "Fogon", "Bacata", 1.1f, Fit.Footprint, false),
        ("Estera", "Estera", "Bacata", 2.4f, Fit.Footprint, false),
        ("juncos", "Juncos", "Bacata", 1.6f, Fit.Height, false),
        ("Frailejon", "Frailejon", "Bacata", 1.5f, Fit.Height, false),
        ("Rocaorilla1", "RocaOrilla1", "Bacata", 2.2f, Fit.Largest, false),
        ("Rocaorilla2", "RocaOrilla2", "Bacata", 1.6f, Fit.Largest, false),
        ("Rocaorilla3", "RocaOrilla3", "Bacata", 3f, Fit.Largest, false),
        ("hojacoca", "Coca", "Props", .32f, Fit.Largest, false),
        ("Mascarachia", "MascaraChia", "Props", .5f, Fit.Largest, true),
        ("Mascarasue", "MascaraSue", "Props", .5f, Fit.Largest, true),
        ("Urnavacia", "UrnaVacia", "Props", .9f, Fit.Height, false),
        ("Urnamemoria", "UrnaMemoria", "Props", .9f, Fit.Height, false),
        ("recipienteyopo", "Yopo", "Props", .36f, Fit.Largest, false),
        ("poporo", "Poporo", "Props", .3f, Fit.Largest, false),
        ("Bastónbicéfalo", "Baston", "Props", 1.7f, Fit.Largest, false),
        ("Arcoinvasor", "Arco", "Props", 1.4f, Fit.Largest, false),
        ("flechainvasor", "Flecha", "Props", .8f, Fit.Largest, false),
    };

    static BacataModelSetup()
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

    public static void RunBatch() { Run(); EditorApplication.Exit(0); }

    [MenuItem("Nemequene/Campaña/Modelos de Bacatá")]
    public static void Run()
    {
        var report = new System.Collections.Generic.List<string>();
        foreach (var m in Models)
        {
            try { report.Add(Build(m.folder, m.key, m.target, m.size, m.fit, m.centred)); }
            catch (Exception e) { report.Add(m.key + ": ERROR " + e.Message); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("BACATA_MODELS_OK\n" + string.Join("\n", report));
    }

    private static string Build(string folder, string key, string target, float size, Fit fit, bool centred)
    {
        string dir = ArtRoot + folder;
        if (!AssetDatabase.IsValidFolder(dir)) return key + ": falta " + dir;
        string modelPath = AssetDatabase.FindAssets("t:Model", new[] { dir }).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        var model = modelPath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) : null;
        if (model == null) return key + ": falta el modelo en " + dir;
        var material = Material(dir, key);

        var root = new GameObject(key);
        try
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            instance.name = "Modelo";
            foreach (var c in instance.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
            foreach (var a in instance.GetComponentsInChildren<Animator>(true)) UnityEngine.Object.DestroyImmediate(a);
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
                if (material != null) r.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, r.sharedMaterials.Length)).ToArray();
            // Scale to the real size, then sit the base (or the centre) on the root.
            var b = Bounds(instance);
            float measure = fit == Fit.Height ? b.size.y : fit == Fit.Footprint ? Mathf.Max(b.size.x, b.size.z) : Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (measure > 1e-4f) instance.transform.localScale *= size / measure;
            b = Bounds(instance);
            Vector3 anchor = centred ? b.center : new Vector3(b.center.x, b.min.y, b.center.z);
            instance.transform.position -= anchor;
            // A room with a floor (the refuge): the walkable floor, not the foundation, at the root.
            if (key == "InteriorRefugio") instance.transform.position -= Vector3.up * FloorHeight(instance);
            string outDir = "Assets/_Game/Resources/" + target;
            Directory.CreateDirectory(outDir);
            PrefabUtility.SaveAsPrefabAsset(root, outDir + "/" + key + ".prefab");
            b = Bounds(instance);
            return key + ": " + b.size.x.ToString("0.00") + " x " + b.size.y.ToString("0.00") + " x " + b.size.z.ToString("0.00") + " m";
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static Material Material(string dir, string key)
    {
        var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { dir }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        string Pick(Func<string, bool> match) => textures.FirstOrDefault(p => match(Path.GetFileNameWithoutExtension(p).ToLowerInvariant()));
        string baseMap = Pick(n => n.Contains("basecolor") || n.Contains("diffuse") || n.Contains("albedo")) ?? Pick(n => n.StartsWith("tripo_rgb"))
            ?? Pick(n => !n.Contains("normal") && !n.Contains("metal") && !n.Contains("rough") && !n.EndsWith("_rm"));
        string normal = Pick(n => n.Contains("normal"));
        string path = dir + "/" + key + "_URP.mat";
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader;
        material.SetColor("_BaseColor", Color.white);
        material.SetTexture("_BaseMap", baseMap != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(baseMap) : null);
        var bump = normal != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(normal) : null;
        if (bump != null)
        {
            var importer = AssetImporter.GetAtPath(normal) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap) { importer.textureType = TextureImporterType.NormalMap; importer.SaveAndReimport(); }
            material.SetTexture("_BumpMap", bump); material.EnableKeyword("_NORMALMAP");
        }
        else { material.SetTexture("_BumpMap", null); material.DisableKeyword("_NORMALMAP"); }
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", .2f);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Height of the floor under the middle of a model, read from its mesh (temporary colliders).
    private static float FloorHeight(GameObject go)
    {
        var added = new System.Collections.Generic.List<MeshCollider>();
        foreach (var f in go.GetComponentsInChildren<MeshFilter>(true))
            if (f.sharedMesh != null) { var c = f.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = f.sharedMesh; added.Add(c); }
        Physics.SyncTransforms();
        var b = Bounds(go);
        float floor = 0;
        var scene = go.scene.IsValid() ? go.scene.GetPhysicsScene() : Physics.defaultPhysicsScene;
        if (scene.Raycast(new Vector3(b.center.x, b.min.y + 1.6f, b.center.z), Vector3.down, out var hit, 3f))
            floor = Mathf.Max(0, hit.point.y - b.min.y);
        foreach (var c in added) UnityEngine.Object.DestroyImmediate(c);
        return floor;
    }

    private static Bounds Bounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
}

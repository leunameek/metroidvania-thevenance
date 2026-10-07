using UnityEngine;

// The rigged cast lives in Resources/Characters/<Key> (built by CharacterLibrarySetup): root at
// the feet, facing +Z, with its Animator and CharacterActions. Scenes made before the models
// existed keep their provisional stand-ins and swap them at runtime with these helpers.
public static class CharacterModels
{
    public static bool Exists(string key) => Resources.Load<GameObject>("Characters/" + key) != null;

    // Instance of the character under parent (local pose), or null when the prefab is missing.
    public static CharacterActions Spawn(string key, Transform parent, Vector3 localPosition, Quaternion localRotation, float scale = 1f)
    {
        var prefab = Resources.Load<GameObject>("Characters/" + key);
        if (prefab == null) return null;
        var go = Object.Instantiate(prefab, parent, false);
        go.name = key;
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.transform.localScale = Vector3.one * scale;
        foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = parent != null ? parent.gameObject.layer : 0;
        return CharacterActions.Of(go.transform);
    }

    // Hides the renderers of a stand-in (its colliders and logic stay), except under `keep`.
    public static void Hide(Transform root, params Transform[] keep)
    {
        if (root == null) return;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            bool kept = false;
            foreach (var k in keep) if (k != null && r.transform.IsChildOf(k)) kept = true;
            if (!kept) r.enabled = false;
        }
    }
}

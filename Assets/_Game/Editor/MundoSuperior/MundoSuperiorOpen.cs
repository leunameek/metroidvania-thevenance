using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

// Opens the Mundo Superior level in the editor: from the menu, or once on the next domain reload
// when a request file exists (written by tools working on the level):
//   Library/MundoSuperior.update  -> apply new clouds and models to the saved scene, then open it
//   Library/MundoSuperior.open    -> just open it
[InitializeOnLoad]
public static class MundoSuperiorOpen
{
    private const string OpenRequest = "Library/MundoSuperior.open";
    private const string UpdateRequest = "Library/MundoSuperior.update";

    static MundoSuperiorOpen()
    {
        if (!File.Exists(OpenRequest) && !File.Exists(UpdateRequest)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            bool update = File.Exists(UpdateRequest);
            if (File.Exists(UpdateRequest)) File.Delete(UpdateRequest);
            if (File.Exists(OpenRequest)) File.Delete(OpenRequest);
            if (update) MundoSuperiorBuilder.UpdateScene(); // leaves the level open
            else Open();
        };
    }

    [MenuItem("Nemequene/Mundo Superior/Abrir nivel")]
    public static void Open()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(MundoSuperiorBuilder.ScenePath, OpenSceneMode.Single);
    }
}

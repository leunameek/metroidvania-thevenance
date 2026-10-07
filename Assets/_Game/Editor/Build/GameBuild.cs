using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// The Windows build of the game: the enabled scenes of the Build Settings into
// Builds/Windows/ElAsedioDeBacata.exe (Builds/ is not versioned). From the menu
// Nemequene > Compilar > Windows, or in batch mode:
//   Unity.exe -batchmode -quit -projectPath <proyecto> -executeMethod GameBuild.Windows -logFile build.txt
// The log ends with BUILD_OK or BUILD_FAILED and the report's size, time and errors.
public static class GameBuild
{
    public const string Output = "Builds/Windows/ElAsedioDeBacata.exe";

    [MenuItem("Nemequene/Compilar/Windows")]
    public static void Windows()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled && File.Exists(s.path)).Select(s => s.path).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        var options = new BuildPlayerOptions
        {
            scenes = scenes, locationPathName = Output, target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone, options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        foreach (var step in report.steps)
            foreach (var message in step.messages)
                if (message.type == LogType.Error || message.type == LogType.Exception)
                    Debug.Log("BUILD_ERROR " + step.name + ": " + message.content);
        bool ok = summary.result == BuildResult.Succeeded;
        Debug.Log((ok ? "BUILD_OK " : "BUILD_FAILED ") + summary.result + " " + Path.GetFullPath(Output)
            + " size " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB time " + summary.totalTime
            + " errors " + summary.totalErrors + " warnings " + summary.totalWarnings + " scenes " + scenes.Length);
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }
}

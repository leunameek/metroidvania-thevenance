using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class PlazaValidation
{
    private const string Pending = "Nemequene.Plaza.Validation";
    private static Keyboard _keyboard;
    private static Mouse _mouse;
    private static Key[] _keys = Array.Empty<Key>();
    private static MouseState _mouseState;
    private static int _checks, _errors;

    static PlazaValidation()
    {
        if (SessionState.GetBool(Pending, false)) EditorApplication.update += WaitForPlay;
    }
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Validation must run in batchmode.");
        SessionState.SetBool(Pending, true);
        EditorApplication.update -= WaitForPlay; EditorApplication.update += WaitForPlay;
        EditorSceneManager.OpenScene(Week08SceneBuilder.ScenePath);
        EditorApplication.EnterPlaymode();
    }
    private static void WaitForPlay()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        EditorApplication.update -= WaitForPlay;
        Application.logMessageReceived += Log;
        var driver = new GameObject("TEST_ONLY_PlazaValidation").AddComponent<Week08SmokeDriver>();
        Object.DontDestroyOnLoad(driver);
        driver.Run(Exercise(), Finish);
    }
    private static void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) _errors++;
    }
    private static void Finish(int code)
    {
        SessionState.SetBool(Pending, false);
        InputSystem.onBeforeUpdate -= Input;
        Application.logMessageReceived -= Log;
        Debug.Log("PLAZA_VALIDATION_RESULT checks=" + _checks + " runtimeErrors=" + _errors);
        if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
        if (_mouse != null) InputSystem.RemoveDevice(_mouse);
        EditorApplication.Exit(code != 0 || _errors != 0 ? 1 : 0);
    }
    private static void Input()
    {
        if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(_keyboard, new KeyboardState(_keys));
        InputSystem.QueueStateEvent(_mouse, _mouseState);
    }
    private static void Keys(params Key[] keys) { _keys = keys; }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("PLAZA_CHECK_FAIL: " + message);
        _checks++; Debug.Log("PLAZA_CHECK_PASS: " + message);
    }
    private static IEnumerator Exercise()
    {
        Time.captureDeltaTime = 1f / 60f;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        _keyboard = InputSystem.AddDevice<Keyboard>(); _mouse = InputSystem.AddDevice<Mouse>();
        InputSystem.onBeforeUpdate += Input;
        yield return 1.0;
        var demo = Object.FindFirstObjectByType<TechnicalDemoController>();
        var player = demo.Player;
        Check(demo.enabled && player != null, "lobby systems active");
        Check(Object.FindObjectsByType<AnalyzableObject>(FindObjectsSortMode.None).Length == 3, "three practice objects");
        Check(Object.FindObjectsByType<PlazaPortal>(FindObjectsSortMode.None).Length == 4, "two outbound portals and two return portals");
        Check(!demo.Hands.Requested, "camera is opt-in");
        Check(!demo.PortalsUnlocked && demo.Objectives.AnalyzedCount == 0, "portals start sealed");
        Check(player.IsGrounded, "player stands on plaza pavement");
        Screenshot("01-lobby");
        Vector3 start = player.transform.position;
        Keys(Key.W); yield return 0.45; Keys();
        float walk = Vector3.Distance(start, player.transform.position);
        Check(walk > 1.5f, "walking moves player");
        player.Teleport(start); yield return 0.2;
        Keys(Key.W, Key.LeftShift); yield return 0.45; Keys();
        Check(Vector3.Distance(start, player.transform.position) > walk * 1.3f, "sprint is faster than walk");
        player.Teleport(start); yield return 0.2;
        Keys(Key.Q); yield return 0.05; Keys();
        Check(player.IsDashing, "Q starts unlocked lobby dash");
        yield return 0.4;
        Check(!player.IsDashing, "dash finishes");
        player.Teleport(start); yield return 0.2;
        Keys(Key.Space); yield return 0.18; Keys();
        Check(player.transform.position.y > start.y + 0.3f, "jump leaves ground");
        yield return 1.0;
        Check(player.IsGrounded, "jump lands");

        var portal = Object.FindObjectsByType<PlazaPortal>(FindObjectsSortMode.None).First(x => x.world == -1);
        demo.Travel(portal); yield return 0.1;
        Check(demo.World == 0 && demo.State == TechnicalDemoState.Exploration, "locked portal refuses travel");
        var items = Object.FindObjectsByType<AnalyzableObject>(FindObjectsSortMode.None).OrderBy(x => x.Data.objectId).ToArray();
        int completed = 0;
        foreach (var item in items)
        {
            player.Teleport(item.transform.position + new Vector3(0, -0.95f, -2.3f)); yield return 0.4;
            Check(demo.Nearby == item, "proximity prompt for " + item.Data.displayName);
            Keys(Key.E); yield return 0.1; Keys(); yield return 0.5;
            Check(demo.State == TechnicalDemoState.Analyzing && demo.Selected == item, "E enters inspection");
            Check(demo.Objectives.AnalyzedCount == completed, "opening alone does not grant progress");
            Vector3 locked = player.transform.position;
            Keys(Key.W); yield return 0.2; Keys();
            Check(Vector3.Distance(locked, player.transform.position) < 0.01f, "analysis locks player movement");
            if (!demo.MouseMode) { Keys(Key.M); yield return 0.1; Keys(); }
            _mouseState = new MouseState { buttons = 1, delta = new Vector2(12, 12) };
            yield return 0.6;
            _mouseState = new MouseState();
            yield return 0.8;
            Check(demo.Lesson.Complete, "real mouse adapter completes measured rotation and freeze");
            Check(!demo.Lesson.UsedHands, "mouse test is not reported as camera evidence");
            Check(demo.Objectives.AnalyzedCount == ++completed, "station counted once");
            if (completed == 3) Screenshot("02-object-tutorial");
            Keys(Key.E); yield return 0.2; Keys();
            Check(demo.State == TechnicalDemoState.Exploration, "E exits completed lesson");
            yield return 0.1;
        }
        Check(!demo.PortalsUnlocked, "objects alone cannot unlock portals");
        player.Teleport(demo.Combat.EntryPosition); yield return 0.3;
        Check(demo.NearCombat, "arena entry is accessible");
        Keys(Key.E); yield return 0.2; Keys();
        Check(demo.State == TechnicalDemoState.Combat, "E begins guided fight");
        Check(demo.Combat.Model.Phase == PlazaCombatPhase.Attack && demo.Combat.Model.Hits == 0, "entry E does not also attack");
        yield return 0.15;
        Screenshot("03-combat");
        Keys(Key.E); yield return 0.15; Keys();
        Check(demo.Combat.Model.Hits == 1, "E attacks in player turn");
        yield return 1.9;
        Check(demo.Combat.Model.Phase == PlazaCombatPhase.React, "enemy telegraph opens reaction window");
        Keys(Key.F); yield return 0.1; Keys();
        Check(demo.Combat.Model.Mistakes == 1, "wrong defense gives feedback");
        yield return 4.7;
        Check(demo.Combat.Model.Phase == PlazaCombatPhase.React, "missed defense repeats");
        Keys(Key.Space); yield return 0.1; Keys();
        Check(demo.Combat.Model.LastDefenseSucceeded, "space dodges direct attack");
        yield return 1.3;
        Keys(Key.E); yield return 0.1; Keys(); yield return 1.9;
        Check(demo.Combat.Model.Expected == PlazaDefense.Guard, "second enemy lesson requires guard");
        Keys(Key.F); yield return 0.1; Keys(); yield return 1.3;
        // Two blows to read without the answer on screen: front (guard), then sweep (dodge).
        Keys(Key.E); yield return 0.1; Keys(); yield return 1.9;
        Check(!demo.Combat.Model.ShowsAnswer && demo.Combat.Model.Expected == PlazaDefense.Guard, "third blow is read, not told");
        Keys(Key.F); yield return 0.1; Keys(); yield return 1.3;
        Keys(Key.E); yield return 0.1; Keys(); yield return 1.9;
        Check(demo.Combat.Model.Expected == PlazaDefense.Dodge, "fourth blow is a sweep");
        Keys(Key.Space); yield return 0.1; Keys(); yield return 1.3;
        Keys(Key.E); yield return 0.1; Keys();
        Check(demo.Combat.Completed && demo.Combat.Model.Phase == PlazaCombatPhase.Won, "five strikes plus two guided and two read defenses win training");
        Check(demo.PortalsUnlocked, "all requirements unlock both portals");
        yield return 0.5;
        Keys(Key.E); yield return 0.2; Keys();
        Check(demo.State == TechnicalDemoState.Exploration && !player.InputLocked, "victory restores exploration");

        foreach (int world in new[] { -1, 1 })
        {
            portal = Object.FindObjectsByType<PlazaPortal>(FindObjectsSortMode.None).First(x => x.world == world);
            player.Teleport(portal.transform.position + new Vector3(0, 1.1f, -1.8f)); yield return 0.3;
            Check(demo.NearbyPortal == portal, "portal proximity selects correct destination");
            Keys(Key.E); yield return 0.15; Keys();
            float deadline = Time.realtimeSinceStartup + 5;
            while (demo.State == TechnicalDemoState.Transition && Time.realtimeSinceStartup < deadline) yield return 0.1;
            yield return 0.2;
            Check(demo.World == world && Vector3.Distance(player.transform.position, portal.destination.position) < 0.4f, "portal reaches world " + world + " state=" + demo.State + " actual=" + player.transform.position);
            Check(demo.State == TechnicalDemoState.Exploration && demo.Fade == 0 && !player.InputLocked, "transition restores input and clears fade");
            Screenshot(world < 0 ? "04-mundo-inferior" : "05-mundo-superior");
            player.Teleport(portal.destination.position + Vector3.down * 30); yield return 0.4;
            Check(Vector3.Distance(player.transform.position, portal.destination.position) < 0.4f, "world fall returns to local safe spawn");
            var back = Object.FindObjectsByType<PlazaPortal>(FindObjectsSortMode.None).Where(x => x.world == 0).OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).First();
            player.Teleport(back.transform.position + new Vector3(0, 1.1f, 1.8f)); yield return 0.3;
            Keys(Key.E); yield return 0.15; Keys();
            deadline = Time.realtimeSinceStartup + 5;
            while (demo.State == TechnicalDemoState.Transition && Time.realtimeSinceStartup < deadline) yield return 0.1;
            Check(demo.World == 0 && demo.PortalsUnlocked, "return portal preserves tutorial progress");
        }
        demo.Combat.Begin(); yield return 0.2; Keys(Key.Escape); yield return 0.2; Keys();
        Check(demo.State == TechnicalDemoState.Exploration && !player.InputLocked, "Esc cancels replay safely");
        Keys(Key.H); yield return 0.15; Keys();
        Check(demo.HelpOpen && player.InputLocked, "help pauses player input");
        yield return 0.15;
        Screenshot("06-help");
        Keys(Key.H); yield return 0.15; Keys();
        Keys(Key.V); yield return 0.15; Keys();
        Check(demo.Audio.Muted, "mute control works");
        Keys(Key.R); yield return 1.2; Keys();
        demo = Object.FindFirstObjectByType<TechnicalDemoController>();
        Check(demo.Objectives.AnalyzedCount == 0 && !demo.Combat.Completed && !demo.PortalsUnlocked, "restart resets full tutorial");
        Check(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(x => x.enabled) == 1, "exactly one active audio listener");
        ValidateNativeModelAndAudio();
        Check(_errors == 0, "no runtime errors during complete playthrough");
        Debug.Log("PLAZA_PLAYTHROUGH_PASS");
    }
    private static void ValidateNativeModelAndAudio()
    {
        var settings = AssetDatabase.LoadAssetAtPath<Mediapipe.Unity.Sample.AppSettings>("Assets/_Game/Data/PlazaNunez/PlazaMediaPipeSettings.asset");
        Check(settings.assetLoaderType == Mediapipe.Unity.Sample.AppSettings.AssetLoaderType.StreamingAssets, "camera bootstrap uses bundled model in Editor and builds");
        var options = new Mediapipe.Tasks.Vision.HandLandmarker.HandLandmarkerOptions(
            new Mediapipe.Tasks.Core.BaseOptions(modelAssetPath: Application.streamingAssetsPath + "/hand_landmarker.bytes"),
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE, numHands: 2);
        var blank = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        blank.SetPixels32(new Color32[128 * 128]); blank.Apply();
        using (var task = Mediapipe.Tasks.Vision.HandLandmarker.HandLandmarker.CreateFromOptions(options))
        using (var input = new Mediapipe.Image(blank))
        {
            var result = task.Detect(input);
            Check(result.handLandmarks == null || result.handLandmarks.Count == 0, "native MediaPipe CPU loads model and processes blank frame without a webcam");
        }
        Object.Destroy(blank);
        var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Game/Audio/PlazaNunez" });
        Check(clips.Length == 16, "all sixteen original sound assets are imported");
        foreach (var guid in clips)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
            Check(clip != null && clip.length > 0.1f && clip.samples > 1000, "valid audio: " + clip.name);
        }
    }
    private static void Screenshot(string name)
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        Directory.CreateDirectory("Specs/Week08/Evidence/PlazaNunez");
        Camera camera = Camera.main;
        var canvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).First(x => x.name == "UI_PlazaNunez");
        var target = new RenderTexture(1440, 900, 24);
        var prior = RenderTexture.active;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.enabled = false;
        canvas.scaleFactor = 1;
        camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 0.5f;
        Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
        var png = new Texture2D(1440, 900, TextureFormat.RGB24, false);
        png.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0); png.Apply();
        File.WriteAllBytes("Specs/Week08/Evidence/PlazaNunez/" + name + ".png", png.EncodeToPNG());
        camera.targetTexture = null; RenderTexture.active = prior;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        scaler.enabled = true;
        Object.Destroy(png); target.Release(); Object.Destroy(target);
    }
}

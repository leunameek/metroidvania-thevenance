using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit batch-only smoke test. Never enters Play or exits an interactive editor automatically.
[InitializeOnLoad]
public static class Week08SmokeValidation
{
    private const string Pending = "Nemequene.Week08.SmokePending";
    private static Keyboard _keyboard;
    private static Key[] _heldKeys = Array.Empty<Key>();

    static Week08SmokeValidation()
    {
        if (SessionState.GetBool(Pending, false)) EditorApplication.update += Tick;
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Usar una copia temporal en batchmode.");
        SessionState.SetBool(Pending, true);
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorSceneManager.OpenScene(Week08SceneBuilder.ScenePath);
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        EditorApplication.update -= Tick;
        var driver = new GameObject("TEST_ONLY_Week08Smoke").AddComponent<Week08SmokeDriver>();
        UnityEngine.Object.DontDestroyOnLoad(driver.gameObject);
        driver.Run(Exercise(), Finish);
    }

    private static void Finish(int code)
    {
        SessionState.SetBool(Pending, false);
        EditorApplication.update -= Tick;
        InputSystem.onBeforeUpdate -= InjectKeys;
        if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
        EditorApplication.Exit(code);
    }

    private static void Keys(params Key[] keys)
    {
        _heldKeys = keys;
    }

    private static void InjectKeys()
    {
        if (_keyboard != null && InputState.currentUpdateType == InputUpdateType.Dynamic)
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(_heldKeys));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("WEEK08_SMOKE_FAIL: " + message);
        Debug.Log("WEEK08_CHECK: " + message);
    }

    private static IEnumerator Exercise()
    {
        Time.captureDeltaTime = 1f / 60f;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        _keyboard = InputSystem.AddDevice<Keyboard>();
        InputSystem.onBeforeUpdate += InjectKeys;
        yield return 1.0;
        var demo = UnityEngine.Object.FindFirstObjectByType<TechnicalDemoController>();
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        Check(demo != null && demo.enabled && player != null, "sesión y controlador presentes");
        Check(UnityEngine.Object.FindObjectsByType<AnalyzableObject>(FindObjectsSortMode.None).Length == 3, "tres objetos");
        Check(UnityEngine.Object.FindFirstObjectByType<BossFightController>() == null
            && UnityEngine.Object.FindFirstObjectByType<HandTrackingBootstrapper>() == null
            && UnityEngine.Object.FindFirstObjectByType<VoiceCommandRecognizer>() == null,
            "demo separada del jefe y del hardware");
        Check(player.IsGrounded, "suelo detectado");

        Vector3 start = player.transform.position;
        Keys(Key.W);
        yield return 0.5;
        Keys();
        float walk = Vector3.Distance(start, player.transform.position);
        Check(walk > 1f, $"W mueve al jugador: distancia={walk}, tecla={_keyboard.wKey.isPressed}, actual={Keyboard.current == _keyboard}");
        player.Teleport(start);
        yield return 0.2;
        Keys(Key.W, Key.LeftShift);
        yield return 0.5;
        Keys();
        float run = Vector3.Distance(start, player.transform.position);
        Check(run > walk * 1.15f && !player.IsDashing, "Shift corre sin dash en la demo");

        player.Teleport(start);
        yield return 0.4;
        float beforeJump = player.transform.position.y;
        Keys(Key.Space);
        yield return 0.2;
        Keys();
        Check(player.transform.position.y > beforeJump + 0.3f, "salto despega del suelo");
        yield return 1.0;
        Check(player.IsGrounded, "salto vuelve al suelo");

        player.Teleport(new Vector3(12, 1.1f, -10));
        yield return 0.4;
        Keys(Key.W);
        yield return 0.7;
        Keys();
        Check(player.transform.position.z < -8.8f, "pared impide atravesarla");

        player.Teleport(new Vector3(0, 1.1f, 0.5f));
        yield return 0.3;
        Keys(Key.W);
        yield return 1.4;
        Keys();
        Check(player.transform.position.y > 2f, "ascenso por pendiente");

        var camera = Camera.main;
        Check(camera != null && Vector3.Distance(camera.transform.position, player.transform.position) < 12f,
            "cámara sigue al jugador");
        var mouse = InputSystem.AddDevice<Mouse>();
        Quaternion beforeOrbit = camera.transform.rotation;
        InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 2, delta = new Vector2(140, 0) });
        yield return 0.3;
        InputSystem.QueueStateEvent(mouse, new MouseState());
        Check(Quaternion.Angle(beforeOrbit, camera.transform.rotation) > 3f, "botón derecho rota la cámara");
        InputSystem.RemoveDevice(mouse);

        var items = UnityEngine.Object.FindObjectsByType<AnalyzableObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var item in items)
        {
            Vector3 position = item.transform.position + new Vector3(0, -0.5f, -2);
            player.Teleport(position);
            yield return 0.6;
            Check(demo.Nearby == item, "objeto señalado por proximidad: " + item.Data.objectId);
            Keys(Key.E);
            yield return 0.2;
            Keys();
            Check(demo.State == TechnicalDemoState.Analyzing, "E inicia análisis");
            Check(demo.Selected == item && demo.Objectives.AnalyzedCount == ++count, "registro de objeto único");
            Vector3 lockedPosition = player.transform.position;
            Keys(Key.W);
            yield return 0.2;
            Keys();
            Check(Vector3.Distance(lockedPosition, player.transform.position) < 0.01f, "análisis bloquea locomoción");
            yield return 0.1;
            Keys(Key.Escape);
            yield return 0.2;
            Keys();
            Check(demo.State == TechnicalDemoState.Exploration, "Esc restaura exploración");
        }
        Check(demo.Objectives.IsComplete, "objetivo se completa con tres objetos");
        player.Teleport(new Vector3(0, -12, 0));
        yield return 0.3;
        Check(player.transform.position.y > 0, "caída devuelve al punto de aparición");
        Keys(Key.R);
        yield return 1.0;
        Keys();
        demo = UnityEngine.Object.FindFirstObjectByType<TechnicalDemoController>();
        Check(demo != null && demo.Objectives.AnalyzedCount == 0, "R recarga y reinicia objetivos");
    }
}

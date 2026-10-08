using System;
using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A story piece of the plaza held in the hands (the urns of H12): the camera comes close, the piece
// turns with the hands as in the first prototype (HandInspection), the mouse or A/D and W/S, and
// what it hides shows only after it has been turned and tilted enough to look inside. «tomar» /
// E / a held fist uses it; «salir» / Esc puts it back. Built at runtime, one at a time.
public sealed class PlazaPieceInspection : MonoBehaviour
{
    private const float LookNeeded = 110f;   // degrees of turning before the inside is seen

    public static PlazaPieceInspection Active { get; private set; }

    private TechnicalDemoController _demo;
    private Transform _piece;
    private Func<string> _revealed;
    private string _title, _body, _useLabel;
    private Action _use;
    private Vector3 _center, _cameraPosition;
    private Quaternion _pieceRotation, _cameraRotation;
    private Vector3 _piecePosition;
    private ExplorationOrbitCamera _orbit;
    private Camera _camera;
    private Renderer[] _hidden;
    private float _blend, _looked, _size = 1, _sensitivity = 1;
    private int _openFrame;

    private GameObject _canvas;
    private TMP_Text _text, _guide;
    private Image _progress;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Active = null;

    public static bool Seen => Active != null && Active._looked >= LookNeeded;

    // revealed: what the player learns once the inside is in view. use: the action of «tomar».
    public static void Open(TechnicalDemoController demo, Transform piece, string title, string body,
        Func<string> revealed, string useLabel, Action use)
    {
        if (Active != null || demo == null || piece == null || Camera.main == null) return;
        var host = new GameObject("InspeccionPieza").AddComponent<PlazaPieceInspection>();
        host._demo = demo; host._piece = piece; host._title = title; host._body = body;
        host._revealed = revealed; host._useLabel = useLabel; host._use = use;
        host.Begin();
    }

    private void Begin()
    {
        Active = this; _openFrame = Time.frameCount;
        _camera = Camera.main;
        _orbit = _camera.GetComponent<ExplorationOrbitCamera>();
        if (_orbit != null) _orbit.enabled = false;
        _cameraPosition = _camera.transform.position; _cameraRotation = _camera.transform.rotation;
        _piecePosition = _piece.position; _pieceRotation = _piece.rotation;
        var bounds = Bounds(_piece);
        _center = bounds.center; _size = Mathf.Max(.6f, bounds.extents.magnitude);
        _demo.Player.SetInputLocked(true);
        var visible = new List<Renderer>();
        foreach (var r in _demo.Player.GetComponentsInChildren<Renderer>()) if (r.enabled) { visible.Add(r); r.enabled = false; }
        _hidden = visible.ToArray();
        _sensitivity = NaturalInputPrefs.Load().handSensitivity;
        HandInspection.Reset(_demo.Hands != null ? _demo.Hands.Tracker : null);
        _demo.Hands?.Tracker?.Gestures.ClearPending();
        _demo.Audio?.Play(PlazaSound.Inspect);
        BuildView();
    }

    private void BuildView()
    {
        var canvas = UIKit.ScreenCanvas(transform, "PieceCanvas", 1050);
        _canvas = canvas.gameObject;
        var panel = UIKit.Place(UIKit.HudPanel(canvas, "Panel"), new Vector2(1, .5f), new Vector2(-90, 0), new Vector2(620, 620));
        var title = UIKit.Label(panel, _title, 40, UIPalette.GoldLight, true);
        title.alignment = TextAlignmentOptions.TopLeft;
        title.rectTransform.offsetMin = new Vector2(48, 0); title.rectTransform.offsetMax = new Vector2(-48, -40);
        _text = UIKit.Label(panel, "", 24, UIPalette.Ivory);
        _text.alignment = TextAlignmentOptions.TopLeft; _text.textWrappingMode = TextWrappingModes.Normal;
        _text.rectTransform.offsetMin = new Vector2(48, 210); _text.rectTransform.offsetMax = new Vector2(-48, -112);
        var rail = UIKit.Rect("Look", panel); rail.anchorMin = new Vector2(.08f, 0); rail.anchorMax = new Vector2(.92f, 0);
        rail.pivot = new Vector2(.5f, 0); rail.anchoredPosition = new Vector2(0, 186); rail.sizeDelta = new Vector2(0, 10);
        _progress = UIKit.Bar(rail, new Vector2(500, 10), UIPalette.GoldLight);
        _guide = UIKit.Label(panel, "", 20, UIPalette.Muted);
        _guide.alignment = TextAlignmentOptions.BottomLeft; _guide.textWrappingMode = TextWrappingModes.Normal;
        _guide.rectTransform.offsetMin = new Vector2(48, 40); _guide.rectTransform.offsetMax = new Vector2(-48, -440);
    }

    private void Update()
    {
        if (_piece == null || _demo == null) { Close(); return; }
        // Frame the piece from the player's side, the panel on the right of the screen.
        _blend = Mathf.MoveTowards(_blend, 1, Time.unscaledDeltaTime / .35f);
        Vector3 toCamera = _cameraPosition - _center; toCamera.y = 0;
        if (toCamera.sqrMagnitude < .01f) toCamera = Vector3.back;
        Vector3 side = Vector3.Cross(Vector3.up, toCamera.normalized);
        float size = _size;
        Vector3 eye = _center + toCamera.normalized * (1.3f + size * 1.4f) + Vector3.up * (.9f + size * .7f) - side * .6f * size;
        Quaternion look = Quaternion.LookRotation(_center - side * .6f * size - eye);
        float t = Mathf.SmoothStep(0, 1, _blend);
        _camera.transform.SetPositionAndRotation(Vector3.Lerp(_cameraPosition, eye, t), Quaternion.Slerp(_cameraRotation, look, t));

        float yaw = 0, pitch = 0;
        var mouse = Mouse.current; var keyboard = Keyboard.current;
        if (mouse != null && mouse.leftButton.isPressed) { Vector2 d = mouse.delta.ReadValue() * .3f; yaw -= d.x; pitch += d.y; }
        if (keyboard != null)
        {
            if (GameBindings.Held(GameAction.MoveLeft) || keyboard.leftArrowKey.isPressed) yaw += 120 * Time.unscaledDeltaTime;
            if (GameBindings.Held(GameAction.MoveRight) || keyboard.rightArrowKey.isPressed) yaw -= 120 * Time.unscaledDeltaTime;
            if (GameBindings.Held(GameAction.MoveForward) || keyboard.upArrowKey.isPressed) pitch += 90 * Time.unscaledDeltaTime;
            if (GameBindings.Held(GameAction.MoveBack) || keyboard.downArrowKey.isPressed) pitch -= 90 * Time.unscaledDeltaTime;
        }
        var hands = _demo.Hands;
        bool live = hands != null && hands.Live;
        if (live && !HandInspection.Read(hands.Tracker, _sensitivity, out float handYaw, out float handPitch))
        { yaw += handYaw; pitch += handPitch; }
        if (yaw != 0 || pitch != 0) Turn(yaw, pitch);
        Refresh(live);

        if (Time.frameCount <= _openFrame + 1) return; // the E that opened it does not choose
        if (keyboard != null && (GameBindings.Pressed(GameAction.Interact) || keyboard.enterKey.wasPressedThisFrame)) Confirm();
        else if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Back();
        else if (live && Seen && hands.Tracker.ConsumeGesture(HandGesture.Grab)) Confirm();
    }

    // Degrees around the piece's centre: yaw about the vertical, pitch toward the camera.
    public void Turn(float yaw, float pitch)
    {
        _piece.RotateAround(_center, Vector3.up, yaw);
        _piece.RotateAround(_center, _camera.transform.right, pitch);
        _looked = Mathf.Min(LookNeeded, _looked + Mathf.Abs(yaw) + Mathf.Abs(pitch) * 1.5f);
        if (Mathf.Abs(yaw) + Mathf.Abs(pitch) > .4f) _demo.Audio?.Play(PlazaSound.Rotate, .2f);
    }

    private void Refresh(bool live)
    {
        bool seen = _looked >= LookNeeded;
        _progress.fillAmount = _looked / LookNeeded;
        string text = _body + "\n\n" + (seen ? _revealed?.Invoke() ?? "" : "Gírala e inclínala para mirar dentro.");
        if (_text.text != text) _text.text = text;
        string words = VoicePrompt.Enabled
            ? (seen ? "Di «tomar» para " + _useLabel.ToLowerInvariant() + " o «salir» para devolverla." : "Di «salir» para devolverla.")
            : "";
        string keys = (seen ? GameBindings.Cap(GameAction.Interact) + " · " + _useLabel + "     " : "") + "Esc · Devolver     Ratón o " + GameBindings.Cap(GameAction.MoveLeft) + " / " + GameBindings.Cap(GameAction.MoveRight) + ", " + GameBindings.Cap(GameAction.MoveForward) + " / " + GameBindings.Cap(GameAction.MoveBack) + " · Girar";
        string gestures = live ? HandInspection.Guide + (seen ? " · puño sostenido: " + _useLabel.ToLowerInvariant() : "")
            : "Con la cámara (C) puedes girarla con las manos.";
        string guide = (words.Length > 0 ? words + "\n" : "") + gestures + "\n" + keys;
        if (_guide.text != guide) _guide.text = guide;
    }

    // «tomar», E or the held fist: only once the inside has been seen.
    public void Confirm()
    {
        if (!Seen) { _demo.SetStatus("Primero gírala e inclínala para mirar dentro."); return; }
        var use = _use;
        Close();
        use?.Invoke();
    }

    public void Back()
    {
        GameAudio.Play("Foley/examinar_cerrar", .8f);
        Close();
    }

    private void Close()
    {
        if (Active == this) Active = null;
        if (_piece != null) _piece.SetPositionAndRotation(_piecePosition, _pieceRotation);
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_orbit != null) { _orbit.enabled = true; _orbit.SnapAfterTeleport(); }
        if (_hidden != null) foreach (var r in _hidden) if (r != null) r.enabled = true;
        _hidden = null;
        if (_demo != null) _demo.Player.SetInputLocked(false);
        Destroy(gameObject);
    }

    private void OnDestroy() { if (Active == this) Active = null; }

    private static Bounds Bounds(Transform root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(root.position + Vector3.up * .5f, Vector3.one);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
}

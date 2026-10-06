using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Plays story lines (historia.json) in any scene: one request at a time, queued until every
// registered gate says the moment is safe (no inspection, pause or duel). While a request plays
// the player is locked, scene controllers treat it as busy (StoryPlayer.Active), and the camera
// frames the speaker when a StoryActor of that name is present. Advance: E, Space, Enter, click
// or the voice word «siguiente»/«next». Hold Esc 1 s to skip: skipping applies the same flags as
// finishing (guion C "Salto y reproducción").
public sealed class StoryPlayer : MonoBehaviour
{
    public const float RevealPerSecond = 48f, HoldToSkip = 1f;

    private sealed class Request
    {
        public string Key, Title;
        public StorySequence Sequence;
        public List<StoryLine> Lines;
        public bool Complete, Repeatable;
        public Action Done;
    }

    private static StoryPlayer _instance;
    private static readonly List<KeyValuePair<UnityEngine.Object, Func<bool>>> Gates = new List<KeyValuePair<UnityEngine.Object, Func<bool>>>();
    private readonly Queue<Request> _queue = new Queue<Request>();
    private Request _current;
    private int _index;
    private float _revealed, _skipHeld;
    private bool _voiceNext, _wasLocked;
    private StoryDialogueView _view;
    private PlayerController _player;
    private readonly List<VoiceCommandRecognizer> _voices = new List<VoiceCommandRecognizer>();
    // Camera framing
    private Camera _camera;
    private Vector3 _cameraPosition;
    private Quaternion _cameraRotation;
    private readonly List<Behaviour> _cameraDrivers = new List<Behaviour>();
    private Vector3 _shotPosition;
    private Quaternion _shotRotation;
    private bool _framing;

    public static bool Active => _instance != null && _instance._current != null;
    public static bool Pending => _instance != null && (_instance._current != null || _instance._queue.Count > 0);
    public static event Action<string> Finished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { _instance = null; Gates.Clear(); Finished = null; }

    // A scene system that must not be interrupted registers "ready" (dies with its owner).
    public static void AddGate(UnityEngine.Object owner, Func<bool> ready)
    {
        Gates.RemoveAll(g => g.Key == null || g.Key == owner);
        Gates.Add(new KeyValuePair<UnityEngine.Object, Func<bool>>(owner, ready));
    }
    private static bool GatesOpen()
    {
        Gates.RemoveAll(g => g.Key == null);
        foreach (var gate in Gates)
        {
            try { if (!gate.Value()) return false; }
            catch (Exception) { return false; }
        }
        return true;
    }

    // Plays the trigger's lines once per save. True when something was queued.
    public static bool Trigger(string key, Action done = null)
    {
        var trigger = StoryTriggers.Find(key);
        if (trigger == null || CampaignProgress.Has(trigger.SeenFlag)) return false;
        var sequence = CampaignProgress.Script.Get(trigger.Sequence);
        if (sequence == null) return false;
        var lines = new List<StoryLine>();
        if (trigger.Cues.Length == 0) lines.AddRange(sequence.lines);
        else foreach (var cue in trigger.Cues) lines.AddRange(sequence.Cue(cue));
        if (lines.Count == 0) return false;
        Ensure().Enqueue(new Request
        {
            Key = trigger.SeenFlag, Sequence = sequence, Lines = lines, Complete = trigger.Complete, Done = done,
            Title = trigger.Cues.Length == 0 ? sequence.title : null,
        });
        return true;
    }

    // A whole sequence (cinematic scenes), optionally with a title card.
    public static void PlaySequence(string sequenceId, Action done = null, string title = null)
    {
        var sequence = CampaignProgress.Script.Get(sequenceId);
        if (sequence == null) { done?.Invoke(); return; }
        Ensure().Enqueue(new Request
        {
            Key = StorySequence.SeenFlag(sequenceId), Sequence = sequence, Lines = new List<StoryLine>(sequence.lines),
            Complete = true, Done = done, Title = title,
        });
    }

    // Part of a sequence, for cinematics that act between lines (C19: the arrow after "No era Quimue").
    // complete = this part ends the sequence and applies its flags.
    public static void PlayLines(string sequenceId, int from, int count, bool complete, Action done = null, string title = null)
    {
        var sequence = CampaignProgress.Script.Get(sequenceId);
        if (sequence == null || from >= sequence.lines.Length) { done?.Invoke(); return; }
        count = Mathf.Min(count, sequence.lines.Length - from);
        Ensure().Enqueue(new Request
        {
            Key = sequenceId + ":" + from, Repeatable = !complete, Sequence = sequence, Complete = complete, Done = done, Title = title,
            Lines = new List<StoryLine>(sequence.lines).GetRange(from, count),
        });
    }

    // A contextual question to a guide (D01-D12): the player asks, the guide answers. Repeatable.
    public static void PlayHint(StoryHint hint, string guide, Action done = null)
    {
        if (hint == null) { done?.Invoke(); return; }
        Ensure().Enqueue(new Request
        {
            Key = "hint:" + hint.id, Repeatable = true, Done = done,
            Lines = new List<StoryLine>
            {
                new StoryLine { speaker = "Nemequene", text = hint.question, note = "" },
                new StoryLine { speaker = guide, text = hint.answer, note = "" },
            },
        });
    }

    // Same as pressing E / holding Esc (on-screen buttons, gestures and tests).
    public static void Next() { if (Active) _instance._voiceNext = true; }
    public static void SkipAll() { if (Active) _instance.End(); }

    // Raised for every key that a trigger listens to; flags arrive from CampaignProgress.Changed.
    private static void OnCampaignChanged(string id)
    {
        if (!string.IsNullOrEmpty(id)) Trigger(StoryTriggers.Flag(id));
    }

    private static StoryPlayer Ensure()
    {
        if (_instance != null) return _instance;
        _instance = new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
        return _instance;
    }
    // Scenes call this once so flag triggers play there (world finds, plaza unlocks).
    public static void Listen()
    {
        Ensure();
        CampaignProgress.Changed -= OnCampaignChanged;
        CampaignProgress.Changed += OnCampaignChanged;
    }

    private void Awake()
    {
        _view = new StoryDialogueView(transform);
    }
    private void OnDestroy()
    {
        if (_instance == this) { _instance = null; CampaignProgress.Changed -= OnCampaignChanged; }
        Unhook();
    }

    private void Enqueue(Request request)
    {
        foreach (var queued in _queue) if (queued.Key == request.Key) return;
        if (_current != null && _current.Key == request.Key) return;
        _queue.Enqueue(request);
    }

    private void Update()
    {
        if (_current == null)
        {
            if (_queue.Count > 0 && GatesOpen()) Begin(_queue.Dequeue());
            return;
        }
        var keyboard = Keyboard.current; var mouse = Mouse.current;
        UpdateCamera();
        // Hold Esc to skip the rest.
        if (keyboard != null && keyboard.escapeKey.isPressed) _skipHeld += Time.unscaledDeltaTime; else _skipHeld = 0;
        _view.SetSkip(_skipHeld / HoldToSkip);
        if (_skipHeld >= HoldToSkip) { End(); return; }

        var line = _current.Lines[_index];
        int length = line.text.Length;
        if (_revealed < length) { _revealed += Time.unscaledDeltaTime * RevealPerSecond; _view.Reveal(Mathf.Min(length, (int)_revealed)); }
        bool next = _voiceNext
            || keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
            || mouse != null && mouse.leftButton.wasPressedThisFrame;
        _voiceNext = false;
        if (!next) return;
        if (_revealed < length) { _revealed = length; _view.Reveal(length); return; }
        if (++_index >= _current.Lines.Count) End(); else ShowLine();
    }

    private void Begin(Request request)
    {
        _current = request; _index = 0; _skipHeld = 0;
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null)
        {
            _wasLocked = _player.InputLocked; _player.SetInputLocked(true);
            if (StoryActor.Find("Nemequene") == null) StoryActor.Ensure(_player.gameObject, "Nemequene", 1.55f);
        }
        Hook();
        _view.SetTitle(request.Title);
        _view.SetHint("E / Espacio · Siguiente     Mantén Esc · Saltar");
        _view.Show(true);
        ShowLine();
    }

    private void ShowLine()
    {
        var line = _current.Lines[_index];
        _revealed = 0;
        _view.SetLine(line.speaker, line.note, line.text);
        Frame(StoryActor.Find(line.speaker));
    }

    private void End()
    {
        var done = _current;
        _current = null;
        // Through CampaignProgress so the flags are saved and their own triggers fire.
        if (!done.Repeatable) CampaignProgress.Set(done.Key);
        if (done.Complete && done.Sequence != null) CampaignProgress.CompleteSequence(done.Sequence.id);
        _view.Show(false);
        RestoreCamera();
        Unhook();
        if (_player != null) _player.SetInputLocked(_wasLocked);
        Finished?.Invoke(done.Key);
        done.Done?.Invoke();
    }

    // ---------- Voice ----------
    private void Hook()
    {
        Unhook();
        foreach (var voice in FindObjectsByType<VoiceCommandRecognizer>(FindObjectsSortMode.None))
        {
            voice.CommandRecognized += OnVoice; _voices.Add(voice);
        }
    }
    private void Unhook()
    {
        foreach (var voice in _voices) if (voice != null) voice.CommandRecognized -= OnVoice;
        _voices.Clear();
    }
    private void OnVoice(VoiceCommand command, string phrase)
    {
        if (command == VoiceCommand.Confirm || command == VoiceCommand.Interact) _voiceNext = true;
    }

    // ---------- Camera ----------
    private void Frame(StoryActor actor)
    {
        if (actor == null) return;
        if (!_framing)
        {
            _camera = Camera.main;
            if (_camera == null) return;
            _cameraPosition = _camera.transform.position; _cameraRotation = _camera.transform.rotation;
            _cameraDrivers.Clear();
            foreach (var behaviour in _camera.GetComponents<Behaviour>())
            {
                if (behaviour == null || behaviour is Camera || behaviour is AudioListener || !behaviour.enabled) continue;
                if (!(behaviour is MonoBehaviour) || behaviour.GetType().Namespace == "UnityEngine.Rendering.Universal") continue;
                behaviour.enabled = false; _cameraDrivers.Add(behaviour);
            }
            _framing = true;
        }
        // Three-quarter close shot from the side the camera already was, at face height.
        Vector3 face = actor.Face;
        Vector3 side = _cameraPosition - face; side.y = 0;
        if (side.sqrMagnitude < .01f) side = actor.transform.forward;
        side = side.normalized;
        // First clear three-quarter angle: nobody (the player included) stands between.
        _shotPosition = face + Quaternion.AngleAxis(25f, Vector3.up) * side * 2.6f + Vector3.up * .25f;
        foreach (float angle in new[] { 25f, -25f, 60f, -60f, 100f, -100f, 150f, -150f })
        {
            Vector3 eye = face + Quaternion.AngleAxis(angle, Vector3.up) * side * 2.6f + Vector3.up * .25f;
            if (Clear(actor, face, eye)) { _shotPosition = eye; break; }
        }
        _shotRotation = Quaternion.LookRotation(face - _shotPosition);
    }
    private static bool Clear(StoryActor actor, Vector3 face, Vector3 eye)
    {
        Vector3 d = eye - face;
        foreach (var hit in Physics.SphereCastAll(face, .25f, d.normalized, d.magnitude, ~0, QueryTriggerInteraction.Ignore))
            if (!hit.transform.IsChildOf(actor.transform)) return false;
        return true;
    }
    private void UpdateCamera()
    {
        if (!_framing || _camera == null) return;
        float t = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 4f);
        _camera.transform.SetPositionAndRotation(Vector3.Lerp(_camera.transform.position, _shotPosition, t),
            Quaternion.Slerp(_camera.transform.rotation, _shotRotation, t));
    }
    private void RestoreCamera()
    {
        if (!_framing) return;
        _framing = false;
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        foreach (var behaviour in _cameraDrivers) if (behaviour != null) behaviour.enabled = true;
        _cameraDrivers.Clear();
    }
}

using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// What the hands and the voice can do in a world scene, next to the keys that do the same:
// the player sees which gesture and which word apply right now, and every action the camera or
// the microphone triggers goes through the same code as its key.
public enum NaturalContext { None, Interact, Inspect, Duel, Defend, Fight }

[RequireComponent(typeof(HandTrackingSession))]
public sealed class WorldNaturalInput : MonoBehaviour
{
    // A word is slower than a key: it stays valid this long after being heard.
    private const float VoiceLifetime = 1.2f;

    private HandTrackingSession _hands;
    private VoiceCommandRecognizer _voice;
    private NaturalInputHUD _view;
    private NaturalInputPrefs _prefs;
    private VoiceCommand _heard;
    private string _heardPhrase = "";
    private float _heardAt = -99, _nextPrefsCheck;
    private int _heardFrame = -1;
    private bool _voiceFailed;
    private string _voiceError;
    private NaturalContext _context, _shownContext;
    private string _contextAction = "";
    private float _hold;

    public static WorldNaturalInput Instance { get; private set; }
    public HandTrackingSession Hands => _hands;
    public bool HandsLive => _hands != null && _hands.Live;
    public bool VoiceOn => _prefs != null && _prefs.voiceEnabled && _voice != null && _voice.IsListening;
    public VoiceCommandRecognizer Voice => _voice;

    public static WorldNaturalInput Create(Transform parent)
    {
        var host = new GameObject("EntradaNatural");
        host.transform.SetParent(parent, false);
        host.AddComponent<HandGestureTracker>();
        host.AddComponent<HandTrackingSession>();
        return host.AddComponent<WorldNaturalInput>();
    }

    private void Awake()
    {
        Instance = this;
        _hands = GetComponent<HandTrackingSession>();
        _voice = gameObject.AddComponent<VoiceCommandRecognizer>();
        _voice.CommandRecognized += OnHeard;
        _voice.Unavailable += OnVoiceUnavailable;
        _prefs = NaturalInputPrefs.Load();
        if (string.IsNullOrEmpty(HandTrackingSession.LastCamera) && !string.IsNullOrEmpty(_prefs.camera)) HandTrackingSession.LastCamera = _prefs.camera;
        _view = new GameObject("EntradaNatural_HUD").AddComponent<NaturalInputHUD>();
        _view.transform.SetParent(transform, false);
        _view.Build();
    }

    private void OnDestroy()
    {
        if (_voice != null) { _voice.CommandRecognized -= OnHeard; _voice.Unavailable -= OnVoiceUnavailable; }
        if (Instance == this) Instance = null;
    }

    private void OnHeard(VoiceCommand command, string phrase)
    {
        _heard = command; _heardPhrase = phrase; _heardAt = Time.unscaledTime; _heardFrame = Time.frameCount;
    }

    private void OnVoiceUnavailable(string error)
    {
        _voiceFailed = true; _voiceError = error;
    }

    // ------------------------------------------------------------------ queries (one per action)

    // The owner of the current moment says what the gestures mean; read again every frame.
    public void SetContext(NaturalContext context, string action = "")
    {
        _context = context; _contextAction = action ?? "";
    }

    public bool ConsumeInteract(string action)
    {
        if (TakeVoice(VoiceCommand.Interact, action)) return true;
        return TakeGesture(HandGesture.PalmHold, "Palma abierta · " + action);
    }

    public bool ConsumeConfirm(string action)
    {
        if (TakeVoice(VoiceCommand.Confirm, action)) return true;
        return TakeGesture(HandGesture.Grab, "Puño sostenido · " + action);
    }

    public bool ConsumeBack(string action) => TakeVoice(VoiceCommand.Back, action);

    // Any recent word the caller accepts (duel verbs and targets), consumed once.
    public bool ConsumeVoice(System.Func<VoiceCommand, bool> accept, out VoiceCommand command, out string phrase)
    {
        command = _heard; phrase = _heardPhrase;
        if (Time.unscaledTime - _heardAt > VoiceLifetime || !accept(_heard)) return false;
        _heardAt = -99;
        _view.Flash("«" + _heardPhrase + "»");
        return true;
    }
    public bool ConsumeGesture(HandGesture gesture, string feedback) => TakeGesture(gesture, feedback);
    public bool GuardPoseHeld => HandsLive && _hands.Tracker.Gestures.GuardHeld;

    public bool ConsumeAttack(string action, bool gestures = true)
    {
        if (TakeVoice(VoiceCommand.Attack, action)) return true;
        return gestures && TakeGesture(HandGesture.Strike, "Puño · " + action);
    }

    // The guard is a held pose: two raised open palms count for as long as they are held.
    public bool ConsumeGuard(string action)
    {
        if (TakeVoice(VoiceCommand.Guard, action)) return true;
        if (!HandsLive || !_hands.Tracker.Gestures.GuardHeld) return false;
        _view.Flash("Dos palmas · " + action);
        return true;
    }

    public bool ConsumeDodge(string action, out int direction)
    {
        direction = 0;
        if (TakeVoice(VoiceCommand.Dodge, action)) return true;
        if (!TakeGesture(HandGesture.Swipe, "Barrido · " + action)) return false;
        direction = _hands.Tracker.Gestures.LastSwipeDirection;
        return true;
    }

    // Inspection as in the first prototype (HandInspection): left hand turns, right hand tilts,
    // two fists freeze. Degrees for this frame; zero without the camera.
    public bool ConsumeHandTurn(out float yaw, out float pitch)
    {
        yaw = pitch = 0;
        if (!HandsLive) { HandInspection.Reset(_hands != null ? _hands.Tracker : null); return false; }
        HandInspection.Read(_hands.Tracker, _prefs.handSensitivity, out yaw, out pitch);
        return yaw != 0 || pitch != 0;
    }

    // Open-hand movement as a mouse drag: x to the player's right, y up (pixels-like units).
    public Vector2 ConsumeRotate()
    {
        if (!HandsLive) return Vector2.zero;
        Vector2 d = _hands.Tracker.ConsumeAnyHandDelta();
        d.x = Mathf.Abs(d.x) < .0015f ? 0 : Mathf.Clamp(d.x, -.05f, .05f);
        d.y = Mathf.Abs(d.y) < .0015f ? 0 : Mathf.Clamp(d.y, -.05f, .05f);
        return new Vector2(-d.x, -d.y) * 900f * _prefs.handSensitivity;
    }

    // New turn, window or screen: gestures and words from before do not count.
    public void ClearPending()
    {
        // A word heard in this very frame belongs to the new moment: keep it.
        if (_heardFrame != Time.frameCount) _heardAt = -99;
        if (_hands != null && _hands.Tracker != null)
        {
            _hands.Tracker.Gestures.ClearPending();
            _hands.Tracker.ConsumeAnyHandDelta();
        }
    }

    public float ReactionScale => _prefs != null ? _prefs.reactionScale : 1;

    private bool TakeVoice(VoiceCommand command, string action)
    {
        if (_heard != command || Time.unscaledTime - _heardAt > VoiceLifetime) return false;
        _heardAt = -99;
        _view.Flash("«" + _heardPhrase + "» · " + action);
        return true;
    }

    private bool TakeGesture(HandGesture gesture, string feedback)
    {
        if (!HandsLive || !_hands.Tracker.ConsumeGesture(gesture)) return false;
        _view.Flash(feedback);
        return true;
    }

    // ------------------------------------------------------------------ devices and view

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && GameBindings.Pressed(GameAction.Hands) && Time.timeScale > 0) _hands.Toggle();
        if (Time.unscaledTime >= _nextPrefsCheck)
        {
            _nextPrefsCheck = Time.unscaledTime + 1;
            UpdateVoice();
        }
        _hands.Tracker.Gestures.SetDwellSeconds(_prefs.dwellSeconds);
    }

    private void UpdateVoice()
    {
        bool want = _prefs.voiceEnabled && !_voiceFailed && !Application.isBatchMode;
        if (want && Microphone.devices.Length == 0) { _voiceFailed = true; _voiceError = "sin micrófono"; want = false; }
        if (want && !_voice.IsListening)
        {
            _voice.MinimumConfidence = _prefs.confidence;
            _voice.StartListening();
        }
        else if (!want && _voice.IsListening) _voice.StopListening();
        VoicePrompt.Enabled = want && !_voiceFailed;
    }

    // Pause menu entry: the plaza setting and this one are the same stored value.
    public void SetVoiceEnabled(bool enabled)
    {
        _prefs.voiceEnabled = enabled; _voiceFailed = false;
        string json = PlayerPrefs.GetString(NaturalInputPrefs.StorageKey, "");
        // Keep every other plaza setting: only the voice flag changes in the stored entry.
        json = string.IsNullOrEmpty(json) ? "{\"version\":1,\"voiceDefaults\":1,\"voiceEnabled\":" + (enabled ? "true" : "false") + "}"
            : System.Text.RegularExpressions.Regex.Replace(json, "\"voiceEnabled\":(true|false)", "\"voiceEnabled\":" + (enabled ? "true" : "false"));
        // The choice is the player's from now on (see NaturalInputPrefs.Load).
        if (!json.Contains("\"voiceDefaults\"")) json = json.Insert(json.IndexOf('{') + 1, "\"voiceDefaults\":1,");
        PlayerPrefs.SetString(NaturalInputPrefs.StorageKey, json); PlayerPrefs.Save();
        UpdateVoice();
    }

    public bool VoiceEnabledSetting => _prefs.voiceEnabled;

    // Voice and camera can be switched from the world's pause, like the plaza's settings.
    public void AddPauseEntries(MIHud hud)
    {
        if (hud == null) return;
        hud.AddPauseButton(() => VoiceEnabledSetting ? "Voz: activada" : "Voz: desactivada", () => SetVoiceEnabled(!VoiceEnabledSetting));
        hud.AddPauseButton(() => _hands.Requested ? "Cámara de manos: activada" : "Cámara de manos: desactivada", () => _hands.Toggle());
    }

    // The words and gestures of the inspection, for the find panel next to its keys.
    public string InspectionGuide => Guide(NaturalContext.Inspect, "", HandsLive);

    private void LateUpdate()
    {
        var context = _context; string action = _contextAction;
        _context = NaturalContext.None; _contextAction = "";
        // With neither camera nor voice, the panel only appears where they could be used.
        bool inUse = _hands.Requested || _prefs.voiceEnabled;
        // In a duel the duel screen already names the keys: the plate shows only when hands or
        // voice are in use, and then clear of the action bar.
        bool duel = TurnDuelController.Running;
        bool visible = Time.timeScale > 0 && (inUse || (context != NaturalContext.None && !duel)) && !StoryPlayer.Active;
        _view.SetVisible(visible);
        _view.SetDuelLayout(duel);
        if (!visible) { _shownContext = context; return; }
        if (context != _shownContext)
        {
            // A turn, a window or an inspection starts or ends: stale gestures do not carry over.
            if (Turn(context) || Turn(_shownContext)) ClearPending();
            _shownContext = context;
        }
        _view.SetPanelVisible(context != NaturalContext.Inspect);
        var tracker = _hands.Tracker;
        bool live = HandsLive;
        int count = (tracker.LeftHandPresent ? 1 : 0) + (tracker.RightHandPresent ? 1 : 0);
        string hands = live ? (count == 0 ? "Manos · muéstralas a la cámara" : "Manos · " + count + (count == 1 ? " detectada" : " detectadas"))
            : _hands.Requested ? "Manos · iniciando cámara…" : "Manos · C activa la cámara";
        string voice = VoiceOn ? "Voz · escuchando" : _prefs.voiceEnabled && _voiceFailed ? "Voz · no disponible" + (string.IsNullOrEmpty(_voiceError) ? "" : " (" + Short(_voiceError) + ")")
            : "Voz · desactivada (Pausa)";
        _view.SetState(hands, live, voice, VoiceOn, Guide(context, action, live));
        _view.SetHands(tracker, live);
        float hold = !live ? 0 : context == NaturalContext.Interact ? tracker.Gestures.PalmHoldProgress
            : context == NaturalContext.Inspect ? tracker.Gestures.GrabProgress : 0;
        _hold = Mathf.MoveTowards(_hold, hold, Time.unscaledDeltaTime * 6);
        _view.SetHold(_hold);
    }

    private static bool Turn(NaturalContext c) => c == NaturalContext.Duel || c == NaturalContext.Defend || c == NaturalContext.Inspect;

    private static string Short(string error) => error.Length > 28 ? error.Substring(0, 28) + "…" : error;

    private string Guide(NaturalContext context, string action, bool hands)
    {
        bool voice = VoiceOn;
        if (!hands && !voice) return context == NaturalContext.None ? "" : "Activa la cámara (C) o la voz para usar gestos y palabras.";
        string h = "", v = "";
        switch (context)
        {
            case NaturalContext.Interact: h = "Palma abierta quieta: " + action.ToLowerInvariant(); v = "«examinar» o «usar»"; break;
            case NaturalContext.Inspect: h = HandInspection.Guide + " · puño sostenido: tomar"; v = "«tomar» para quedártela o encajarla · «salir» para devolverla"; break;
            case NaturalContext.Duel: h = "Cierra el puño: atacar"; v = "«atacar»"; break;
            case NaturalContext.Defend: h = "Dos palmas arriba: bloquear · barrido: esquivar"; v = "«bloquea» · «esquiva»"; break;
            case NaturalContext.Fight: h = "Cierra el puño: impulso"; v = "«impulso»"; break;
            default: return "";
        }
        // Voice first, then the gesture.
        return (voice ? "Di " + v : "") + (hands && voice ? "\n" : "") + (hands ? h : "");
    }
}

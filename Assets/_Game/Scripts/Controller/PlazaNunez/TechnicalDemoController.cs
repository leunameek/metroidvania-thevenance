using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum TechnicalDemoState { Exploration, Analyzing, Combat, Transition }

public sealed class TechnicalDemoController : MonoBehaviour
{
    [SerializeField] private TechnicalDemoConfig config;
    [SerializeField] private PlayerController player;
    [SerializeField] private ExplorationOrbitCamera orbitCamera;
    [SerializeField] private AnalyzableObject[] objects;
    [SerializeField] private PlazaCombatController combat;
    [SerializeField] private PlazaPortal[] portals;
    private readonly Dictionary<string, PlazaLessonModel> _lessons = new Dictionary<string, PlazaLessonModel>();
    private Vector3 _worldSpawn;
    private AnalyzableObject _nearby, _selected;
    private Quaternion _originalRotation;
    private InspectionModel _inspection;
    private PlazaHandSession _hands;
    private PlazaAudio _audio;
    private bool _restarting;
    private int _ignoreInteractionFrame = -1;
    private Renderer[] _hiddenPlayerRenderers;
    private Light _sun;
    private Color _skyAmbient, _equatorAmbient, _groundAmbient, _sunColor;
    private float _sunIntensity;
    public ExplorationObjectiveModel Objectives { get; private set; }
    private TechnicalDemoState _state;
    public TechnicalDemoState State { get => _state; private set { if (_state == value) return; _state = value; ViewChanged?.Invoke(); } }
    public event Action ViewChanged;
    public bool ManagedUI { get; set; }
    public AnalyzableObject[] Objects => objects;
    public PlazaPortal[] Portals => portals;
    private float _handSensitivity = 1;
    private bool _reducedMotion;
    public TechnicalDemoConfig Config => config;
    public AnalyzableObject Selected => _selected;
    public AnalyzableObject Nearby => _nearby;
    public PlayerController Player => player;
    public PlazaHandSession Hands => _hands;
    public PlazaAudio Audio => _audio;
    public PlazaCombatController Combat => combat;
    public PlazaLessonModel Lesson => _selected != null ? _lessons[_selected.Data.objectId] : null;
    public bool MouseMode { get; private set; }
    public bool PortalsUnlocked => Objectives != null && Objectives.IsComplete && combat != null && combat.Completed;
    public string[] CompletedObjectIds
    {
        get
        {
            var ids = new List<string>();
            foreach (var item in objects)
                if (item != null && item.Data != null && item.Completed) ids.Add(item.Data.objectId);
            return ids.ToArray();
        }
    }
    public PlazaPortal NearbyPortal { get; private set; }
    public PlazaStoryPoint NearbyStory { get; private set; }
    public bool HasInteraction => Nearby != null || NearbyStory != null || NearbyPortal != null || NearCombat;
    public PlazaCampaign Campaign { get; private set; }
    // The training circle belongs to the tutorial: once the story moves on it is no longer offered.
    public bool NearCombat => combat != null && CampaignProgress.Model.Chapter <= CampaignChapter.PlazaTutorial
        && Vector3.Distance(player.transform.position, combat.EntryPosition) < 3.2f;
    public float Fade { get; private set; }
    public int World { get; private set; }
    public bool HelpOpen { get; private set; }
    private string _status = "Bienvenido. Sigue las tres estaciones doradas y practica a tu ritmo.";
    public string Status { get => _status; private set { if (_status == value) return; _status = value; ViewChanged?.Invoke(); } }
    public Health PlayerHealth => player != null ? player.GetComponent<Health>() : null;

    private void Awake()
    {
        if (player == null || config == null)
        {
            Debug.LogError("Plaza Núñez: faltan Player o Config.", this); enabled = false; return;
        }
        _worldSpawn = player.transform.position;
        var ids = new List<string>();
        for (int i = 0; i < objects.Length; i++)
        {
            var item = objects[i];
            if (item == null || item.Data == null) continue;
            ids.Add(item.Data.objectId);
            _lessons[item.Data.objectId] = new PlazaLessonModel(i);
        }
        Objectives = new ExplorationObjectiveModel(ids);
        _inspection = new InspectionModel(0.4f, 0.3f, 300f, false, true);
        _hands = GetComponent<PlazaHandSession>();
        // Turning the camera on means using the hands; turning it off goes back to the mouse.
        if (_hands != null) _hands.RequestedChanged += SyncInputMode;
        _audio = GetComponent<PlazaAudio>();
        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.type == LightType.Directional) _sun = light;
        if (_sun != null) { _sunColor = _sun.color; _sunIntensity = _sun.intensity; }
        _skyAmbient = RenderSettings.ambientSkyColor;
        _equatorAmbient = RenderSettings.ambientEquatorColor;
        _groundAmbient = RenderSettings.ambientGroundColor;
        // Before the UI starts its session (sceneLoaded) so its first save keeps this progress.
        if (WorldTravel.ReturningFrom != 0) RestoreProgress(WorldTravel.CompletedObjectIds, WorldTravel.CombatCompleted);
        // The story is already past the tutorial (a later chapter loaded or jumped to): the
        // stations and the training show as done and the lower portal stays open.
        if (CampaignProgress.Has(CampaignFlags.MiUnlocked) && !PortalsUnlocked) RestoreProgress(ids.ToArray(), true);
    }

    // Back from a world scene: standing in front of its portal (the player is ready in Start).
    private void ArriveFromWorld(int world)
    {
        foreach (var portal in portals)
        {
            if (portal == null || portal.world != world) continue;
            Vector3 away = _worldSpawn - portal.transform.position; away.y = 0;
            away = away.sqrMagnitude > 0.01f ? away.normalized : -portal.transform.forward;
            // Teleport moves the capsule centre: 1 m above the feet on the 2 m plaza capsule.
            player.Teleport(portal.transform.position + away * 4.5f + Vector3.up * 1.1f);
            player.transform.rotation = Quaternion.LookRotation(away);
            orbitCamera.SnapAfterTeleport();
            break;
        }
        Status = "De vuelta en Plaza Núñez. Tu progreso se conserva.";
    }
    private void Start()
    {
        player.GrantDash(1);
        PlazaGardens.Dress(transform);
        // Story: lines play only while exploring with no menu open (the UI adds its own gate).
        StoryPlayer.Listen();
        StoryPlayer.AddGate(this, () => State == TechnicalDemoState.Exploration && !HelpOpen && !_restarting && PlazaPieceInspection.Active == null);
        Campaign = PlazaCampaign.Create(this);
        if (combat != null && combat.Guardian != null) StoryActor.Ensure(combat.Guardian.gameObject, "Guardián de entrenamiento", 1.9f);
        SyncCampaign();
        StoryPlayer.Trigger(StoryTriggers.PlazaArrival);
        if (WorldTravel.ReturningFrom != 0) ArriveFromWorld(WorldTravel.ReturningFrom);
        WorldTravel.ClearReturn();
    }

    private void Update()
    {
        if (_restarting) return;
        if (StoryPlayer.Active || TurnDuelController.Running || PlazaPieceInspection.Active != null) return;
        if (ManagedUI && HelpOpen) return;
        Keyboard k = Keyboard.current;
        if (k != null && GameBindings.Pressed(GameAction.Mute)) _audio.ToggleMute();
        if (k != null && GameBindings.Pressed(GameAction.Hands)) _hands.Toggle();
        if (k != null && GameBindings.Pressed(GameAction.InputMode)) ToggleInputMode();
        if (HelpOpen) return;
        if (State == TechnicalDemoState.Transition) return;
        if (!ManagedUI && k != null && GameBindings.Pressed(GameAction.Restart)) { Restart(); return; }
        if (player.transform.position.y < config.fallThreshold)
        {
            if (State == TechnicalDemoState.Combat) combat.Cancel();
            EndAnalysis();
            player.Teleport(_worldSpawn);
            orbitCamera.SnapAfterTeleport();
            Status = "Has vuelto a un lugar seguro. Tu progreso se conserva.";
        }
        if (State == TechnicalDemoState.Combat) return;
        if (State == TechnicalDemoState.Analyzing)
        {
            if (k != null && k.escapeKey.wasPressedThisFrame) { EndAnalysis(); return; }
            if (k != null && GameBindings.Pressed(GameAction.Interact))
            {
                if (Lesson.Complete) EndAnalysis();
                else Status = "Completa el gesto indicado. Esc permite salir y continuar después.";
                return;
            }
            // Lesson learnt: a held fist puts the piece back, like E.
            if (Lesson.Complete && !MouseMode && _hands.Live && _hands.Tracker.ConsumeGesture(HandGesture.Grab)) { EndAnalysis(); return; }
            UpdateAnalysis();
            return;
        }
        SyncCampaign();
        FindNearby();
        if (Time.frameCount > _ignoreInteractionFrame && k != null && GameBindings.Pressed(GameAction.Interact)) Interact();
    }

    // Being in the plaza means the prologue is behind; lessons and training feed the story.
    private bool _campaignSynced, _lessonsSynced, _trainingSynced;
    private void SyncCampaign()
    {
        bool lessons = Objectives.IsComplete, training = combat != null && combat.Completed;
        if (_campaignSynced && _lessonsSynced == lessons && _trainingSynced == training) return;
        _campaignSynced = true; _lessonsSynced = lessons; _trainingSynced = training;
        CampaignProgress.ImportPlaza(lessons, training);
    }

    private void SyncInputMode()
    {
        if (_hands.Requested == MouseMode) ToggleInputMode();
    }

    public void ToggleInputMode()
    {
        MouseMode = !MouseMode;
        _hands?.Tracker.ConsumeLeftHandDeltaX();
        _hands?.Tracker.ConsumeRightHandDeltaY();
        Status = MouseMode ? "Modo mouse: arrastra con botón izquierdo; suéltalo para congelar."
            : "Modo manos: C activa la cámara. Mano izquierda gira; mano derecha inclina.";
    }

    private void FindNearby()
    {
        AnalyzableObject nearest = null;
        float best = config.interactionDistance;
        Vector3 origin = player.transform.position + Vector3.up * 0.35f;
        foreach (var item in objects)
        {
            if (item == null || item.Data == null || !item.gameObject.activeInHierarchy) continue;
            Vector3 delta = item.transform.position - origin;
            float distance = delta.magnitude;
            if (distance >= best) continue;
            if (Physics.Raycast(origin, delta.normalized, out RaycastHit hit, distance,
                1, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<AnalyzableObject>() != item) continue;
            best = distance; nearest = item;
        }
        if (_nearby != nearest)
        {
            if (_nearby != null) _nearby.SetHighlighted(false);
            _nearby = nearest;
            if (_nearby != null) _nearby.SetHighlighted(true);
            ViewChanged?.Invoke();
        }
        NearbyStory = _nearby == null ? PlazaStoryPoint.Nearest(player.transform.position) : null;
        NearbyPortal = null;
        float portalDistance = 3f;
        foreach (var portal in portals)
        {
            if (portal == null) continue;
            float distance = Vector3.Distance(player.transform.position, portal.transform.position + Vector3.up);
            if (distance < portalDistance) { NearbyPortal = portal; portalDistance = distance; }
        }
    }

    public void ConfigurePresentation(float sensitivity, bool invert, float motion, float handSensitivity, bool reduced)
    {
        orbitCamera.ConfigurePresentation(sensitivity, invert, motion);
        _handSensitivity = handSensitivity; _reducedMotion = reduced;
    }

    public void Interact()
    {
        if (State != TechnicalDemoState.Exploration || StoryPlayer.Pending) return;
        if (_nearby != null)
        {
            // First time at a station Bachue names its lesson (H05), then the analysis opens.
            var item = _nearby;
            if (!StoryPlayer.Trigger(StoryTriggers.PlazaStation(System.Array.IndexOf(objects, item)), () => BeginAnalysis(item))) BeginAnalysis(item);
            return;
        }
        if (NearbyStory != null) { NearbyStory.use?.Invoke(); return; }
        if (NearbyPortal != null)
        {
            if (!NearbyPortal.Available)
            {
                Status = NearbyPortal.LockedReason;
                GameAudio.UI(UICue.Blocked);
            }
            else Travel(NearbyPortal);
            return;
        }
        if (NearCombat && !StoryPlayer.Trigger(StoryTriggers.PlazaTraining, () => combat.Begin())) combat.Begin();
    }

    public void BeginAnalysis(AnalyzableObject item)
    {
        if (State != TechnicalDemoState.Exploration || item == null || !_lessons.ContainsKey(item.Data.objectId)) return;
        _selected = item;
        var visible = new List<Renderer>();
        foreach (var renderer in player.GetComponentsInChildren<Renderer>())
            if (renderer.enabled) { visible.Add(renderer); renderer.enabled = false; }
        _hiddenPlayerRenderers = visible.ToArray();
        _originalRotation = item.transform.rotation;
        var camera = orbitCamera.transform;
        Vector3 focus = item.transform.position + Vector3.right * 0.65f;
        Vector3 eye = focus + new Vector3(0, 0.7f, -3.8f);
        _inspection.BeginInspect(item.transform.eulerAngles, camera.position, camera.rotation,
            eye, Quaternion.LookRotation(focus - eye));
        State = TechnicalDemoState.Analyzing;
        player.SetInputLocked(true);
        orbitCamera.enabled = false;
        _hands.Tracker.ConsumeLeftHandDeltaX();
        _hands.Tracker.ConsumeRightHandDeltaY();
        _hands.Tracker.Gestures.ClearPending();
        _audio.Play(PlazaSound.Inspect);
        Status = "Sigue la instrucción. Los puños cerrados detienen el giro.";
    }

    private void UpdateAnalysis()
    {
        if (_selected == null) { EndAnalysis(); return; }
        if (_inspection.CurrentState == InspectionModel.State.EnteringInspect)
        {
            var pose = _inspection.TickBlend(Time.deltaTime);
            orbitCamera.transform.SetPositionAndRotation(pose.position, pose.rotation);
            return;
        }
        float yaw = 0, pitch = 0;
        bool freeze = false, realHands = !MouseMode && _hands.Live;
        bool complete = Lesson.Complete;
        if (MouseMode)
        {
            Mouse mouse = Mouse.current;
            bool dragging = mouse != null && mouse.leftButton.isPressed;
            Vector2 delta = dragging ? mouse.delta.ReadValue() : Vector2.zero;
            yaw = delta.x * 0.3f; pitch = -delta.y * 0.3f;
            _selected.transform.rotation = _inspection.ApplyMouseRotation(delta);
            freeze = !dragging;
        }
        else
        {
            var tracker = _hands.Tracker;
            float x = tracker.ConsumeLeftHandDeltaX();
            float y = tracker.ConsumeRightHandDeltaY();
            if (realHands)
            {
                x = Mathf.Abs(x) < 0.0015f ? 0 : Mathf.Clamp(x, -0.04f, 0.04f);
                y = Mathf.Abs(y) < 0.0015f ? 0 : Mathf.Clamp(y, -0.04f, 0.04f);
                yaw = -x * 300f * _handSensitivity; pitch = y * 300f * _handSensitivity;
                _selected.transform.rotation = _inspection.ApplyHandRotation(y * _handSensitivity, x * _handSensitivity);
                freeze = tracker.LeftHandPresent && tracker.RightHandPresent && !tracker.LeftHandOpen && !tracker.RightHandOpen;
            }
        }
        Lesson.Move(yaw, pitch, realHands);
        Lesson.Freeze(freeze, Time.deltaTime);
        if (Mathf.Abs(yaw) + Mathf.Abs(pitch) > 0.35f) _audio.Play(PlazaSound.Rotate, 0.22f);
        if (!complete && Lesson.Complete)
        {
            // The fists that froze the piece must open before a held fist can close the lesson.
            _hands.Tracker.Gestures.ClearPending();
            Objectives.Analyze(_selected.Data.objectId);
            _selected.SetCompleted();
            _audio.Play(PlazaSound.Complete);
            Status = Objectives.IsComplete ? "Tres estaciones activadas. Ve al círculo de entrenamiento." : "Gesto aprendido. E para volver a la plaza.";
        }
    }

    public void EndAnalysis()
    {
        if (State != TechnicalDemoState.Analyzing) return;
        if (_selected != null) _selected.transform.rotation = _originalRotation;
        _selected = null;
        if (_hiddenPlayerRenderers != null)
            foreach (var renderer in _hiddenPlayerRenderers) if (renderer != null) renderer.enabled = true;
        _hiddenPlayerRenderers = null;
        _inspection.EndInspect();
        GameAudio.Play("Foley/examinar_cerrar", .8f);
        SetExploration();
    }
    public void SetCombat()
    {
        State = TechnicalDemoState.Combat;
        player.SetInputLocked(true);
        orbitCamera.enabled = false;
    }
    public void SetExploration()
    {
        State = TechnicalDemoState.Exploration;
        _ignoreInteractionFrame = Time.frameCount;
        player.SetInputLocked(false);
        orbitCamera.enabled = true;
        orbitCamera.SnapAfterTeleport();
    }
    public void SetStatus(string status) { Status = status; }
    public void RestoreProgress(string[] completedIds, bool combatCompleted)
    {
        if (completedIds != null)
            foreach (string id in completedIds)
                foreach (var item in objects)
                    if (item != null && item.Data != null && item.Data.objectId == id && _lessons.TryGetValue(id, out var lesson))
                    {
                        lesson.RestoreComplete();
                        item.SetCompleted();
                        Objectives.Analyze(id);
                    }
        if (combatCompleted) combat.RestoreCompleted();
        ViewChanged?.Invoke();
    }
    public void SetHelp(bool open)
    {
        HelpOpen = open;
        player.SetInputLocked(open || State != TechnicalDemoState.Exploration);
        orbitCamera.enabled = !open && State == TechnicalDemoState.Exploration;
    }

    public void Travel(PlazaPortal portal)
    {
        if (State != TechnicalDemoState.Exploration || portal == null || portal.destination == null || !portal.Available) return;
        StartCoroutine(Transit(portal));
    }
    private IEnumerator Transit(PlazaPortal portal)
    {
        State = TechnicalDemoState.Transition;
        player.SetInputLocked(true);
        _audio.Play(PlazaSound.Portal);
        float fadeOut = _reducedMotion ? .01f : .4f;
        for (float t = 0; t < fadeOut; t += Time.deltaTime) { Fade = t / fadeOut; yield return null; }
        Fade = 1;
        // Worlds with their own level scene leave the plaza instead of using the in-scene threshold.
        if (portal.world != 0 && WorldTravel.SceneFor(portal.world) != null)
        {
            WorldTravel.LeavePlaza(portal.world, CompletedObjectIds, combat != null && combat.Completed);
            yield break;
        }
        World = portal.world;
        ViewChanged?.Invoke();
        _worldSpawn = portal.destination.position;
        player.Teleport(_worldSpawn);
        player.transform.rotation = portal.destination.rotation;
        _audio.SetWorld(World);
        if (_sun != null)
        {
            _sun.intensity = World < 0 ? 0.3f : _sunIntensity;
            _sun.color = World < 0 ? new Color(0.42f, 0.52f, 0.85f) : _sunColor;
        }
        RenderSettings.ambientSkyColor = World < 0 ? new Color(0.18f, 0.23f, 0.34f) : _skyAmbient;
        RenderSettings.ambientEquatorColor = World < 0 ? new Color(0.15f, 0.13f, 0.24f) : _equatorAmbient;
        RenderSettings.ambientGroundColor = World < 0 ? new Color(0.08f, 0.07f, 0.14f) : _groundAmbient;
        RenderSettings.fogColor = World < 0 ? new Color(0.055f, 0.09f, 0.14f)
            : World > 0 ? new Color(0.66f, 0.78f, 0.84f) : new Color(0.64f, 0.69f, 0.67f);
        Camera.main.backgroundColor = RenderSettings.fogColor;
        orbitCamera.SnapAfterTeleport();
        Status = World == 0 ? "De vuelta en Plaza Núñez. Puedes repetir el entrenamiento o visitar el otro mundo."
            : "Has llegado a " + portal.destinationName + ". El portal de retorno te lleva a la plaza.";
        yield return null;
        float fadeIn = _reducedMotion ? .01f : .5f;
        for (float t = 0; t < fadeIn; t += Time.deltaTime) { Fade = 1 - t / fadeIn; yield return null; }
        Fade = 0;
        SetExploration();
    }
    public void Restart()
    {
        if (_restarting || State == TechnicalDemoState.Transition) return;
        _restarting = true;
        EndAnalysis();
        if (combat != null) combat.Cancel();
        SceneLoader.Load(gameObject.scene.path);
    }
    private void OnDestroy()
    {
        if (_hands != null) _hands.RequestedChanged -= SyncInputMode;
    }
    private void OnDisable()
    {
        EndAnalysis();
        if (_nearby != null) _nearby.SetHighlighted(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

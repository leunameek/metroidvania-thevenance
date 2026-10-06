using System.Collections;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Loads the installed MediaPipe scene on explicit camera activation, without its demo UI. The
// tracking scene is additive, so a level change unloads it: the choice to use the camera is
// kept in Wanted and the next scene's session (plaza or world) starts it again by itself.
[RequireComponent(typeof(HandGestureTracker))]
public class HandTrackingSession : MonoBehaviour
{
    [SerializeField] private GameObject bootstrapPrefab;
    private const string TrackingScene = "Hand Landmark Detection";
    private HandLandmarkerRunner _runner;
    private bool _busy, _ownsScene, _stopped, _cancelRequested;

    // The player's choice across scene changes (reset when the game starts).
    public static bool Wanted { get; private set; }
    public static string LastCamera { get; set; }
    // The plaza's bootstrap (bundled model, StreamingAssets loader) serves the world sessions too.
    private static GameObject _sharedBootstrap;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Wanted = false; LastCamera = null; _sharedBootstrap = null; }

    public bool Requested { get; private set; }
    public string Status { get; protected set; }
    public HandGestureTracker Tracker { get; private set; }
    public bool Live => Requested && Tracker != null && Tracker.IsConnected;
    public string PreferredCamera { get => LastCamera; set => LastCamera = value; }
    public event System.Action RequestedChanged;
    // How the current scene works without the camera (plaza: mouse; worlds: keyboard).
    protected virtual string Fallback => "puedes seguir con el teclado";
    protected virtual string ToggleKey => "C";

    private void SetRequested(bool value)
    {
        if (Requested == value) return;
        Requested = value; Wanted = value;
        RequestedChanged?.Invoke();
    }

    public void Deactivate()
    {
        _cancelRequested = true; SetRequested(false);
        if (_runner != null) _runner.Pause();
        if (Tracker != null) Tracker.enabled = false;
        Status = "Cámara apagada · " + Fallback;
    }
    public void SelectCamera(string name)
    {
        PreferredCamera = name;
        var source = ImageSourceProvider.ImageSource;
        if (source == null || source.sourceCandidateNames == null) return;
        int index = System.Array.IndexOf(source.sourceCandidateNames, name);
        if (index < 0) return;
        bool resume = Requested;
        if (_runner != null) _runner.Stop();
        source.SelectSource(index);
        if (resume && _runner != null) _runner.Play();
    }

    protected virtual void Awake()
    {
        if (bootstrapPrefab != null) _sharedBootstrap = bootstrapPrefab;
        else bootstrapPrefab = _sharedBootstrap;
        Tracker = GetComponent<HandGestureTracker>();
        Tracker.enabled = false; // it polls for the runner while enabled
        Status = "Cámara apagada · pulsa " + ToggleKey + " para activar las manos";
        SceneManager.sceneLoaded += OnLoaded;
    }
    protected virtual void Start()
    {
        // The camera was on in the previous scene: keep it on without asking again.
        if (Wanted && !Requested) StartCoroutine(EnableHands());
    }
    public void Toggle()
    {
        if (_busy) return;
        if (Requested)
        {
            SetRequested(false);
            if (_runner != null) _runner.Pause();
            Tracker.enabled = false;
            Status = "Cámara pausada · " + ToggleKey + " para reactivar";
            return;
        }
        StartCoroutine(EnableHands());
    }
    private IEnumerator EnableHands()
    {
        _busy = true;
        _cancelRequested = false;
        if (Application.isBatchMode)
        {
            Status = "Cámara disponible en una sesión interactiva";
            _busy = false;
            yield break;
        }
        if (WebCamTexture.devices.Length == 0)
        {
            Status = "No se encontró cámara · conecta una y pulsa " + ToggleKey + ", o " + Fallback;
            _busy = false;
            yield break;
        }
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (_cancelRequested) { _busy = false; yield break; }
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Status = "Permiso de cámara pendiente · " + Fallback;
            _busy = false;
            yield break;
        }
        SetRequested(true);
        Tracker.enabled = true;
        Status = "Iniciando cámara… coloca tus manos frente a ella";
        if (GameObject.Find("Bootstrap") == null && bootstrapPrefab != null)
        {
            var bootstrap = Instantiate(bootstrapPrefab);
            bootstrap.name = "Bootstrap";
            DontDestroyOnLoad(bootstrap);
        }
        if (_runner != null) { _runner.Play(); _stopped = false; }
        else if (!SceneManager.GetSceneByName(TrackingScene).isLoaded)
        {
            if (!Application.CanStreamedLevelBeLoaded(TrackingScene))
            {
                SetRequested(false);
                Status = "Falta la escena de manos en Build Settings · " + Fallback;
                _busy = false;
                yield break;
            }
            _ownsScene = true;
            yield return SceneManager.LoadSceneAsync(TrackingScene, LoadSceneMode.Additive);
        }
        else OnLoaded(SceneManager.GetSceneByName(TrackingScene), LoadSceneMode.Additive);
        if (_cancelRequested) { Deactivate(); _busy = false; yield break; }
        if (!string.IsNullOrEmpty(PreferredCamera)) SelectCamera(PreferredCamera);
        _busy = false;
    }
    private void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TrackingScene) return;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (var camera in root.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (var listener in root.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
            foreach (var light in root.GetComponentsInChildren<Light>(true)) light.enabled = false;
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true)) canvas.enabled = false;
            foreach (var events in root.GetComponentsInChildren<EventSystem>(true)) events.enabled = false;
            var runner = root.GetComponentInChildren<HandLandmarkerRunner>(true);
            if (runner != null) _runner = runner;
        }
    }
    protected virtual void Update()
    {
        if (!Requested || _busy) return;
        Status = Live ? "Cámara activa · " + HandText(true) + "  /  " + HandText(false)
            : "Esperando cámara… revisa permisos o " + Fallback;
    }
    private string HandText(bool right)
    {
        bool present = right ? Tracker.RightHandPresent : Tracker.LeftHandPresent;
        bool open = right ? Tracker.RightHandOpen : Tracker.LeftHandOpen;
        return (right ? "D: " : "I: ") + (!present ? "fuera de cuadro" : open ? "abierta" : "cerrada");
    }
    private void OnDisable()
    {
        if (_ownsScene && _runner != null && !_stopped)
        {
            _runner.StopAllCoroutines();
            _runner.Stop();
            _stopped = true;
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        // The native runner owns its task; stop it before its scene is unloaded on restart.
        if (_ownsScene && _runner != null)
        {
            if (!_stopped) _runner.Stop();
            if (SceneManager.GetSceneByName(TrackingScene).isLoaded)
                SceneManager.UnloadSceneAsync(TrackingScene);
        }
    }
}

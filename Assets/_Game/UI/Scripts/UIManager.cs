using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nemequene.UI
{
    [DefaultExecutionOrder(-200)]
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public static bool EnterGameplayOnLoad;
        public TechnicalDemoController Demo { get; private set; }
        public UITheme Theme { get; private set; }
        public SettingsManager Settings { get; private set; }
        public ScreenManager Screens { get; private set; }
        public UIFactory Factory { get; private set; }
        public RectTransform Root { get; private set; }
        public Canvas Canvas { get; private set; }
        public NotificationManager Notifications { get; private set; }
        public VoiceUIController Voice { get; private set; }
        public HandTrackingUIController Hands { get; private set; }
        public MapUIController Map { get; private set; }
        public PoporoUIController Poporo { get; private set; }
        public DialogueUIController Dialogue { get; private set; }
        public SubtitleController Subtitles { get; private set; }
        public LoadingScreenController Loading { get; private set; }
        public bool SessionStarted { get; private set; }
        public bool HasSession { get; private set; }
        public float PlaySeconds => _previousPlaySeconds + Mathf.Max(0, Time.time - _sessionStartedAt);
        public bool ModalOpen => _modal != null && _modal.activeSelf;
        public bool CanActivate => !_ready || Time.frameCount > _backFrame;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private AccessibilityManager _accessibility;
        private HUDController _hud;
        private CombatUIController _combat;
        private InventoryUIController _inventory;
        private SaveLoadUIController _saveLoad;
        private InspectionUIController _inspection;
        private MenuController _menu;
        private CalibrationController _calibration;
        private TutorialController _tutorial;
        private GameObject _modal;
        private TMP_Text _modalText, _modalDetail;
        private UIIconGraphic _modalIcon;
        private Button _confirm;
        private Action _confirmed;
        private GameObject _priorFocus;
        private CanvasGroup _screenLayer;
        private float _priorTimeScale = 1;
        private bool _paused, _ready;
        private int _backFrame = -1;
        private RenderPipelineAsset _originalQualityPipeline;
        private UniversalRenderPipelineAsset _pipeline;
        private AudioSource _sound;
        private float _lastSound;
        private AudioClip _click;
        private float _sessionStartedAt, _previousPlaySeconds, _nextAutoSave;
        private string _lastSavedProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Instance = null; EnterGameplayOnLoad = false; SceneManager.sceneLoaded -= SceneLoaded; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() { SceneManager.sceneLoaded += SceneLoaded; }
        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TechnicalDemoController demo = null;
            foreach (var root in scene.GetRootGameObjects())
            { demo = root.GetComponentInChildren<TechnicalDemoController>(); if (demo != null) break; }
            if (demo == null || !demo.enabled) return;
            var legacy = demo.GetComponent<TechnicalDemoHUD>(); if (legacy != null) legacy.enabled = false;
            var prefab = Resources.Load<GameObject>("Nemequene/UI_Root");
            var host = prefab != null ? Instantiate(prefab) : new GameObject("UI_Nemequene", typeof(RectTransform));
            host.name = "UI_Nemequene"; SceneManager.MoveGameObjectToScene(host, scene);
            var manager = host.GetComponent<UIManager>() ?? host.AddComponent<UIManager>(); manager.Initialize(demo);
        }
        public void Initialize(TechnicalDemoController demo)
        {
            // Story lines wait for the session to start and for menus, modals and dialogues to close.
            StoryPlayer.AddGate(this, () => SessionStarted && Screens != null && Screens.Current == UIScreen.None && !ModalOpen && !(Dialogue?.Active ?? false));
            Instance = this; Demo = demo; Demo.ManagedUI = true;
            Settings = new SettingsManager(); Screens = new ScreenManager();
            Theme = Resources.Load<UITheme>("Nemequene/Theme");
            if (Theme == null || Theme.bodyFont == null || Theme.titleFont == null)
            {
                Debug.LogError("Nemequene UI: run Tools/Nemequene/UI/Build assets before playing.");
                var legacy = demo.GetComponent<TechnicalDemoHUD>(); if (legacy != null) legacy.enabled = true;
                Demo.ManagedUI = false; Destroy(gameObject); return;
            }
            Factory = new UIFactory(Theme);
            Root = GetComponent<RectTransform>();
            Canvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay; Canvas.sortingOrder = 200;
            var scaler = gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                var events = new GameObject("UI_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false); events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            _sound = gameObject.AddComponent<AudioSource>(); _sound.playOnAwake = false; _sound.ignoreListenerPause = true;
            _click = Resources.Load<AudioClip>("Nemequene/UI_Select");
            _originalQualityPipeline = QualitySettings.renderPipeline;
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset source)
            { _pipeline = Instantiate(source); QualitySettings.renderPipeline = _pipeline; }
            _hud = new HUDController(this);
            _inspection = new InspectionUIController(this);
            _combat = new CombatUIController(this);
            var screens = Factory.Rect("Screens", Root, Vector2.zero, Vector2.one); _screenLayer = screens.gameObject.AddComponent<CanvasGroup>();
            _menu = new MenuController(this, screens);
            _saveLoad = new SaveLoadUIController(this, _menu);
            Map = new MapUIController(this, _menu);
            _inventory = new InventoryUIController(this, _menu);
            Poporo = new PoporoUIController(this, _menu);
            new SettingsUIController(this, _menu);
            Voice = new VoiceUIController(this); Hands = new HandTrackingUIController(this);
            _calibration = new CalibrationController(this, _menu);
            Loading = new LoadingScreenController(this, _menu);
            Notifications = new NotificationManager(this);
            _tutorial = new TutorialController(this);
            Subtitles = new SubtitleController(this);
            Dialogue = new DialogueUIController(this);
            BuildConfirmation();
            _accessibility = new AccessibilityManager(this);
            Screens.Changed += OnScreen; Settings.Apply(false);
            _ready = true;
            if (!Application.isBatchMode && Settings.Values.screenWidth >= 800 && Settings.Values.screenHeight >= 600)
                Screen.SetResolution(Settings.Values.screenWidth, Settings.Values.screenHeight, Settings.Values.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            EnterGameplayOnLoad = false; StartSession();
            if (GameSaveStore.LoadOnNextScene && GameSaveStore.TryRead(GameSaveStore.ActiveSlot, out var loaded))
            {
                Demo.RestoreProgress(loaded.completedObjectIds, loaded.combatCompleted);
                Map.RestoreVisitedMask(loaded.visitedWorlds);
                _previousPlaySeconds = loaded.playSeconds;
                _lastSavedProgress = ProgressSignature();
            }
            else if (WorldTravel.ReturningFrom != 0)
            {
                // Back from a world scene: the demo restored lessons and training already.
                Map.RestoreVisitedMask(WorldTravel.VisitedMask);
                _previousPlaySeconds = WorldTravel.PlaySeconds;
            }
            WorldTravel.Leaving += OnLeavingPlaza;
            GameSaveStore.ClearPendingLoad();
            _sessionStartedAt = Time.time;
            SaveCurrent();
            Demo.PlayerHealth.Died += OnDeath;
            _subscriptions.Add(_hud); _subscriptions.Add(_combat); _subscriptions.Add(_inspection);
            _subscriptions.Add(Map); _subscriptions.Add(Voice); _subscriptions.Add(Hands); _subscriptions.Add(Notifications);
            _subscriptions.Add(Poporo); _subscriptions.Add(_inventory); _subscriptions.Add(_saveLoad);
        }
        private void OnLeavingPlaza()
        {
            WorldTravel.VisitedMask = Map.VisitedMask | 1; // the lower world is about to be visited
            WorldTravel.PlaySeconds = PlaySeconds;
        }
        public void StartSession()
        {
            SessionStarted = true; HasSession = true;
            // Mouse unless the camera is already on (back from a world with the hands active).
            if (Demo.MouseMode == Demo.Hands.Requested) Demo.ToggleInputMode();
            Screens.Show(UIScreen.None, false); RefreshPause(); _hud.Refresh();
        }
        private string ProgressSignature() => string.Join("|", Demo.CompletedObjectIds) + ":" + Demo.Combat.Completed + ":" + Demo.World;
        public bool SaveCurrent()
        {
            if (!SessionStarted || GameSaveStore.ActiveSlot < 0 || Demo.State != TechnicalDemoState.Exploration) return false;
            GameSaveStore.Write(new GameSaveData
            {
                slot = GameSaveStore.ActiveSlot,
                completedObjectIds = Demo.CompletedObjectIds,
                combatCompleted = Demo.Combat.Completed,
                visitedWorlds = Map.VisitedMask,
                playSeconds = PlaySeconds
            });
            _lastSavedProgress = ProgressSignature();
            return true;
        }
        public void RepeatTutorial()
        {
            Settings.Values.tutorials = true; Settings.Apply();
            _tutorial.Reset(); Screens.Show(UIScreen.None, false);
        }
        public void ApplyGraphics(int aa, bool shadows)
        {
            if (_pipeline == null) return;
            QualitySettings.renderPipeline = _pipeline;
            _pipeline.msaaSampleCount = aa == 4 ? 4 : aa == 2 ? 2 : 1;
            _pipeline.shadowDistance = shadows ? 60 : 0;
        }
        private void OnScreen(UIScreen screen)
        {
            _backFrame = Time.frameCount; RefreshPause(); _hud.Refresh(); _menu.Refresh(screen);
            Settings.Flush();
            _calibration?.OnScreen(screen); Voice?.OnScreen(screen);
            _accessibility?.Apply();
        }
        public void RefreshPause()
        {
            bool pause = Screens.Current != UIScreen.None || ModalOpen || (Dialogue != null && Dialogue.Active);
            if (pause && !_paused) { _priorTimeScale = Time.timeScale; Time.timeScale = 0; Demo.SetHelp(true); }
            else if (!pause && _paused) { Time.timeScale = _priorTimeScale; Demo.SetHelp(false); }
            _paused = pause;
            if (pause) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
        private void Update()
        {
            if (!_ready) return;
            var k = Keyboard.current;
            // A story line owns Esc (hold to skip) and the keys while it plays.
            // The microphone keeps listening for «siguiente».
            if (StoryPlayer.Active) { _hud.Tick(); Voice.Tick(); return; }
            bool back = k != null && k.escapeKey.wasPressedThisFrame
                || Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
            if (back && _backFrame != Time.frameCount)
            {
                if (ModalOpen) CloseConfirmation();
                else if (Screens.Current == UIScreen.Loading) { }
                else if (Screens.Current != UIScreen.None) Screens.Back();
                // An urn in the hands takes Esc to put it back (PlazaPieceInspection).
                else if (Demo.State != TechnicalDemoState.Analyzing && PlazaPieceInspection.Active == null) Screens.Show(UIScreen.Pause);
            }
            if (k != null && !ModalOpen && !Dialogue.Active && Screens.Current == UIScreen.None && Demo.State == TechnicalDemoState.Exploration
                && PlazaPieceInspection.Active == null)
            {
                if (k.tabKey.wasPressedThisFrame) Screens.Show(UIScreen.Map);
                else if (k.hKey.wasPressedThisFrame) Screens.Show(UIScreen.Controls);
                else if (k.rKey.wasPressedThisFrame) Confirm("confirm.restart", () => Loading.Restart());
            }
            NavigateTab(k);
            _hud.Tick(); _inspection.Tick(); _combat.Tick(); Voice.Tick(); Hands.Tick(); _calibration.Tick();
            Notifications.Tick(); Dialogue.Tick(); Subtitles.Tick(); Loading.Tick();
            _tutorial.Tick();
            if (Time.unscaledTime >= _nextAutoSave)
            {
                _nextAutoSave = Time.unscaledTime + 2;
                if (GameSaveStore.ActiveSlot >= 0 && Demo.State == TechnicalDemoState.Exploration && ProgressSignature() != _lastSavedProgress)
                    SaveCurrent();
            }
        }
        private void NavigateTab(Keyboard k)
        {
            if (k == null || !k.tabKey.wasPressedThisFrame || Screens.Current == UIScreen.None && !ModalOpen && !Dialogue.Active || EventSystem.current == null || _backFrame == Time.frameCount) return;
            var controls = new List<Selectable>();
            foreach (var c in GetComponentsInChildren<Selectable>()) if (c.IsInteractable() && (!ModalOpen || c.transform.IsChildOf(_modal.transform))) controls.Add(c);
            if (controls.Count == 0) return;
            int i = controls.FindIndex(c => c.gameObject == EventSystem.current.currentSelectedGameObject);
            i = (i + (k.shiftKey.isPressed ? controls.Count - 1 : 1)) % controls.Count;
            EventSystem.current.SetSelectedGameObject(controls[i].gameObject);
        }
        private void BuildConfirmation()
        {
            _modal = Factory.Rect("UI_Modal_Confirmation", Root, Vector2.zero, Vector2.one).gameObject;
            var shade = _modal.AddComponent<Image>(); shade.color = new Color(.031f,.039f,.043f,.86f); shade.raycastTarget = true;
            // Screen 27 «Confirmación y errores»: framed modal, alert emblem, the question, what is
            // kept and what is lost, then the safe action first (it receives focus) and the
            // confirmation as a line below it.
            var panel = Factory.Panel("ConfirmationSheet", _modal.transform, new Vector2(.32f,.20f), new Vector2(.68f,.80f), true);
            var body = Factory.Column(panel, "Content", 14); body.Inset(80, 60, 80, 84);
            var layout = body.GetComponent<VerticalLayoutGroup>(); layout.childAlignment = TextAnchor.MiddleCenter;
            _modalIcon = Factory.Icon(body, UIIcon.Alert, 60, UIPalette.GoldLight);
            _modalText = Factory.Text(body, "", 40, true); _modalText.alignment = TextAlignmentOptions.Center;
            UIFactory.Tone(_modalText, UITone.Gold);
            _modalDetail = Factory.Text(body, "", 24); _modalDetail.alignment = TextAlignmentOptions.Center;
            var actions = Factory.Column(body, "Actions", 10); actions.gameObject.AddComponent<LayoutElement>().minHeight = 128;
            var cancel = Factory.Button(actions, UIStrings.Get("cancel"), CloseConfirmation, true);
            _confirm = Factory.Button(actions, UIStrings.Get("confirm"), () => { var action = _confirmed; CloseConfirmation(); action?.Invoke(); });
            foreach (var b in new[] { cancel, _confirm }) UIFactory.Center(b);
            Factory.Hint(_modal.transform, UIStrings.Get("footer.cancel"), Vector2.zero);
            _modal.SetActive(false);
        }
        public void Confirm(string key, Action action)
        {
            _confirmed = action; _priorFocus = EventSystem.current?.currentSelectedGameObject;
            string message = UIStrings.Get(key);
            int question = message.IndexOf('?');
            bool split = question > 0 && question < message.Length - 1;
            _modalText.text = split ? message.Substring(0, question + 1) : message;
            _modalDetail.text = split ? message.Substring(question + 1).Trim() : "";
            _modalDetail.gameObject.SetActive(split);
            _modal.SetActive(true); _screenLayer.interactable = false;
            var style = _confirm.GetComponent<TitleMenuButton>();
            style.danger = key == "save.deleteConfirm" || key == "save.replaceConfirm" || key == "save.loadConfirm"
                || key == "confirm.restart" || key == "confirm.replay" || key == "confirm.menu" || key == "confirm.quit";
            style.Refresh();
            _modalIcon.SetIcon(style.danger ? UIIcon.Alert : key.StartsWith("graphics") ? UIIcon.Settings : UIIcon.Info);
            RefreshPause(); Voice?.Suspend();
            _accessibility.Apply(); EventSystem.current?.SetSelectedGameObject(_modal.GetComponentInChildren<Button>().gameObject);
        }
        public void CloseConfirmation()
        {
            _modal.SetActive(false); _screenLayer.interactable = true; _confirmed = null; RefreshPause();
            if (_priorFocus != null && _priorFocus.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(_priorFocus);
            else Screens.SelectDefault();
        }
        public void Sound(PlazaSound cue)
        {
            if (_sound == null || _click == null || Time.unscaledTime - _lastSound < .12f) return;
            _lastSound = Time.unscaledTime; _sound.pitch = cue == PlazaSound.Inspect ? 1 : .9f;
            _sound.PlayOneShot(_click, Settings.Values.uiVolume);
        }
        public void ReturnToMenu()
        {
            SaveCurrent();
            Dialogue.Close();
            Demo.EndAnalysis(); Demo.Combat.Cancel(); SessionStarted = false;
            Voice.Suspend(); Hands.Stop(); Loading.ReturnToTitle();
        }
        private void OnDeath() { Screens.Show(UIScreen.Defeat); }
        public void Quit()
        {
            Settings.Apply(); Settings.Flush();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnDestroy()
        {
            if (!_ready) return;
            WorldTravel.Leaving -= OnLeavingPlaza;
            Screens.Changed -= OnScreen; _accessibility.Dispose();
            _calibration.Dispose(); Loading.Dispose(); Settings.Flush();
            foreach (var subscription in _subscriptions) subscription.Dispose();
            if (Demo != null && Demo.PlayerHealth != null) Demo.PlayerHealth.Died -= OnDeath;
            if (_paused) Time.timeScale = _priorTimeScale;
            if (_pipeline != null)
            {
                if (QualitySettings.renderPipeline == _pipeline) QualitySettings.renderPipeline = _originalQualityPipeline;
                Destroy(_pipeline);
            }
            if (Instance == this) Instance = null;
        }
    }
}

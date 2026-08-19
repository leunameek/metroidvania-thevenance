using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VoiceCommandRecognizer))]
public class BossFightController : MonoBehaviour
{
    private enum FightState { Inactive, Telegraph, PlayerReact, Resolve }

    [SerializeField] private PlayerController player;
    [SerializeField] private HandGestureTracker handTracker;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Timing")]
    [SerializeField] private float telegraphDuration = 1.2f;
    [SerializeField] private float reactDuration = 1.5f;
    [SerializeField] private float resolveDuration = 0.8f;

    [Header("Punishment")]
    [SerializeField] private float missDamage = 15f;

    [Header("Hand raise detection")]
    [Tooltip("Palm landmark Y below this (0 = top of frame, 1 = bottom) counts as raised.")]
    [SerializeField] private float raisedYThreshold = 0.4f;

    [Header("Telegraph alert")]
    [SerializeField] private Vector2 alertSize = new Vector2(28f, 90f);

    private VoiceCommandRecognizer _recognizer;
    private FightState _state;
    private float _stateTimeRemaining;
    private bool _dodgeRequested;
    private int _dodgeDirection;

    private GameObject _alertGo;
    private GameObject _turnLabelGo;
    private Text _turnLabelText;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (handTracker == null) handTracker = FindFirstObjectByType<HandGestureTracker>();
        if (cameraFollow == null) cameraFollow = FindFirstObjectByType<CameraFollow>();

        _recognizer = GetComponent<VoiceCommandRecognizer>();
        _recognizer.CommandRecognized += OnCommandRecognized;

        BuildAlertIcon();
        BuildTurnLabel();
    }

    private void OnDestroy()
    {
        if (_recognizer != null) _recognizer.CommandRecognized -= OnCommandRecognized;
    }

    public void StartFight()
    {
        if (_state != FightState.Inactive) return;

        if (cameraFollow != null) cameraFollow.EnterThirdPerson();
        if (player != null) player.SetInputLocked(true);
        _recognizer.StartListening();

        BeginTelegraph();
    }

    public void EndFight()
    {
        if (_state == FightState.Inactive) return;

        _state = FightState.Inactive;
        if (cameraFollow != null) cameraFollow.ExitThirdPerson();
        if (player != null) player.SetInputLocked(false);
        _recognizer.StopListening();
        if (_alertGo != null) _alertGo.SetActive(false);
        if (_turnLabelGo != null) _turnLabelGo.SetActive(false);
    }

    private void OnCommandRecognized(CombatCommand command, string phrase)
    {
        if (_state != FightState.PlayerReact || command != CombatCommand.Dodge) return;

        int direction = GetRaisedHandDirection();
        if (direction == 0) return;

        _dodgeRequested = true;
        _dodgeDirection = direction;
    }

    private int GetRaisedHandDirection()
    {
        if (handTracker == null) return 0;

        bool rightRaised = handTracker.RightHandPresent
            && handTracker.RightHandPoints.Count > 9
            && handTracker.RightHandPoints[9].y < raisedYThreshold;
        bool leftRaised = handTracker.LeftHandPresent
            && handTracker.LeftHandPoints.Count > 9
            && handTracker.LeftHandPoints[9].y < raisedYThreshold;


      if (rightRaised && !leftRaised) return -1;
        if (leftRaised && !rightRaised) return 1;
        return 0;
    }

    private void Update()
    {
        switch (_state)
        {
            case FightState.Telegraph:
                _stateTimeRemaining -= Time.deltaTime;
                if (_stateTimeRemaining <= 0f) BeginPlayerReact();
                break;

            case FightState.PlayerReact:
                if (_dodgeRequested)
                {
                    _dodgeRequested = false;
                    if (player != null) player.PerformDodge(_dodgeDirection);
                    BeginResolve();
                    break;
                }

                _stateTimeRemaining -= Time.deltaTime;
                if (_stateTimeRemaining <= 0f)
                {
                    Health health = player != null ? player.GetComponent<Health>() : null;
                    if (health != null) health.TakeDamage(missDamage);
                    BeginResolve();
                }
                break;

            case FightState.Resolve:
                _stateTimeRemaining -= Time.deltaTime;
                if (_stateTimeRemaining <= 0f) BeginTelegraph();
                break;
        }
    }

    private void BeginTelegraph()
    {
        _state = FightState.Telegraph;
        _stateTimeRemaining = telegraphDuration;
        if (_alertGo != null) _alertGo.SetActive(true);
        SetTurnLabel("Turno del Enemigo");
    }

    private void BeginPlayerReact()
    {
        _state = FightState.PlayerReact;
        _stateTimeRemaining = reactDuration;
        _dodgeRequested = false;
        if (_alertGo != null) _alertGo.SetActive(false);
        SetTurnLabel("Tu Turno");
    }

    private void BeginResolve()
    {
        _state = FightState.Resolve;
        _stateTimeRemaining = resolveDuration;
        if (_turnLabelGo != null) _turnLabelGo.SetActive(false);
    }

    private void SetTurnLabel(string text)
    {
        if (_turnLabelText == null) return;
        _turnLabelText.text = text;
        _turnLabelGo.SetActive(true);
    }

    private void BuildAlertIcon()
    {
        GameObject canvasGo = new GameObject("BossTelegraphAlertCanvas", typeof(RectTransform));
        _alertGo = canvasGo;
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        RectTransform anchorRect = canvasGo.GetComponent<RectTransform>();
        anchorRect.anchorMin = new Vector2(0.5f, 1f);
        anchorRect.anchorMax = new Vector2(0.5f, 1f);
        anchorRect.pivot = new Vector2(0.5f, 1f);
        anchorRect.anchoredPosition = new Vector2(0f, -40f);
        anchorRect.sizeDelta = new Vector2(60f, 120f);

        Color alertColor = new Color(1f, 0.15f, 0.1f);

        GameObject barGo = new GameObject("AlertBar", typeof(RectTransform));
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.SetParent(anchorRect, false);
        barRect.sizeDelta = new Vector2(alertSize.x, alertSize.y * 0.72f);
        barRect.anchoredPosition = new Vector2(0f, -alertSize.y * 0.14f);
        barGo.AddComponent<Image>().color = alertColor;

        GameObject dotGo = new GameObject("AlertDot", typeof(RectTransform));
        RectTransform dotRect = dotGo.GetComponent<RectTransform>();
        dotRect.SetParent(anchorRect, false);
        dotRect.sizeDelta = new Vector2(alertSize.x, alertSize.x);
        dotRect.anchoredPosition = new Vector2(0f, -alertSize.y + alertSize.x * 0.5f);
        dotGo.AddComponent<Image>().color = alertColor;

        canvasGo.SetActive(false);
    }

    private void BuildTurnLabel()
    {
        GameObject canvasGo = new GameObject("BossTurnLabelCanvas", typeof(RectTransform));
        _turnLabelGo = canvasGo;
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 955;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject labelGo = new GameObject("TurnLabel", typeof(RectTransform));
        labelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -10f);
        labelRect.sizeDelta = new Vector2(500f, 50f);

        _turnLabelText = labelGo.AddComponent<Text>();
        _turnLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _turnLabelText.fontSize = 30;
        _turnLabelText.fontStyle = FontStyle.Bold;
        _turnLabelText.alignment = TextAnchor.MiddleCenter;
        _turnLabelText.color = Color.white;

        Shadow shadow = labelGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        canvasGo.SetActive(false);
    }
}

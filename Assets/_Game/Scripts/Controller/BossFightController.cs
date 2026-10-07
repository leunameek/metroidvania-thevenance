using UnityEngine;

[RequireComponent(typeof(VoiceCommandRecognizer))]
public class BossFightController : MonoBehaviour
{
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

    private VoiceCommandRecognizer _recognizer;
    private BossFightModel _fight;
    private BossFightUI _ui;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (handTracker == null) handTracker = FindFirstObjectByType<HandGestureTracker>();
        if (cameraFollow == null) cameraFollow = FindFirstObjectByType<CameraFollow>();

        _recognizer = GetComponent<VoiceCommandRecognizer>();
        _recognizer.CommandRecognized += OnCommandRecognized;

        _ui = GetComponent<BossFightUI>();
        if (_ui == null) _ui = gameObject.AddComponent<BossFightUI>();

        _fight = new BossFightModel(telegraphDuration, reactDuration, resolveDuration, raisedYThreshold);
        _fight.EnteredTelegraph += HandleEnteredTelegraph;
        _fight.EnteredPlayerReact += HandleEnteredPlayerReact;
        _fight.EnteredResolve += _ui.HideTurnLabel;
        _fight.MissedReact += HandleMissedReact;
        _fight.DodgeResolved += HandleDodgeResolved;
    }

    private void OnDestroy()
    {
        if (_recognizer != null) _recognizer.CommandRecognized -= OnCommandRecognized;
    }

    public void StartFight()
    {
        if (!_fight.StartFight()) return;

        if (cameraFollow != null) cameraFollow.EnterThirdPerson();
        if (player != null) player.SetInputLocked(true);
        _recognizer.StartListening();
    }

    public void EndFight()
    {
        if (!_fight.EndFight()) return;

        if (cameraFollow != null) cameraFollow.ExitThirdPerson();
        if (player != null) player.SetInputLocked(false);
        _recognizer.StopListening();
        _ui.HideAlert();
        _ui.HideTurnLabel();
    }

    private void HandleEnteredTelegraph()
    {
        _ui.ShowAlert();
        _ui.SetTurnLabel("Turno del Enemigo");
    }

    private void HandleEnteredPlayerReact()
    {
        _ui.HideAlert();
        _ui.SetTurnLabel("Tu Turno");
    }

    private void HandleMissedReact()
    {
        Health health = player != null ? player.GetComponent<Health>() : null;
        if (health != null) health.TakeDamage(missDamage);
    }

    private void HandleDodgeResolved(int direction)
    {
        if (player != null) player.PerformDodge(direction);
    }

    private void OnCommandRecognized(VoiceCommand command, string phrase)
    {
        if (command != VoiceCommand.Dodge) return;
        _fight.RequestDodge(GetRaisedHandDirection());
    }

    private int GetRaisedHandDirection()
    {
        if (handTracker == null) return 0;

        bool rightPresent = handTracker.RightHandPresent && handTracker.RightHandPoints.Count > 9;
        float rightPalmY = rightPresent ? handTracker.RightHandPoints[9].y : 0f;
        bool leftPresent = handTracker.LeftHandPresent && handTracker.LeftHandPoints.Count > 9;
        float leftPalmY = leftPresent ? handTracker.LeftHandPoints[9].y : 0f;

        return _fight.GetRaisedHandDirection(rightPresent, rightPalmY, leftPresent, leftPalmY);
    }

    private void Update()
    {
        _fight.Tick(Time.deltaTime);
    }
}

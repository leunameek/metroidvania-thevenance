using System;

// Pure state machine + judging rules for the boss encounter: the
// Telegraph -> PlayerReact -> Resolve turn loop, dodge-window judging, and the
// raised-hand-direction geometry rule. No MonoBehaviour/UI dependency - BossFightController
// applies the actual camera/input/UI/damage side effects in response to these events,
// mirroring the event-driven pattern Health.cs already uses for HealthChanged/Died.
public class BossFightModel
{
    public enum FightState { Inactive, Telegraph, PlayerReact, Resolve }

    private readonly float _telegraphDuration;
    private readonly float _reactDuration;
    private readonly float _resolveDuration;
    private readonly float _raisedYThreshold;

    private float _stateTimeRemaining;
    private bool _dodgeRequested;
    private int _dodgeDirection;

    public FightState State { get; private set; } = FightState.Inactive;

    public event Action EnteredTelegraph;
    public event Action EnteredPlayerReact;
    public event Action EnteredResolve;
    public event Action MissedReact;
    public event Action<int> DodgeResolved;

    public BossFightModel(float telegraphDuration, float reactDuration, float resolveDuration, float raisedYThreshold)
    {
        _telegraphDuration = telegraphDuration;
        _reactDuration = reactDuration;
        _resolveDuration = resolveDuration;
        _raisedYThreshold = raisedYThreshold;
    }

    // Returns false if a fight is already running (matches the original's Inactive-only guard).
    public bool StartFight()
    {
        if (State != FightState.Inactive) return false;

        BeginTelegraph();
        return true;
    }

    // Returns false if there's no fight to end.
    public bool EndFight()
    {
        if (State == FightState.Inactive) return false;

        State = FightState.Inactive;
        return true;
    }

    public void RequestDodge(int direction)
    {
        if (State != FightState.PlayerReact || direction == 0) return;

        _dodgeRequested = true;
        _dodgeDirection = direction;
    }

    public int GetRaisedHandDirection(bool rightPresent, float rightPalmY, bool leftPresent, float leftPalmY)
    {
        bool rightRaised = rightPresent && rightPalmY < _raisedYThreshold;
        bool leftRaised = leftPresent && leftPalmY < _raisedYThreshold;

        if (rightRaised && !leftRaised) return -1;
        if (leftRaised && !rightRaised) return 1;
        return 0;
    }

    public void Tick(float deltaTime)
    {
        switch (State)
        {
            case FightState.Telegraph:
                _stateTimeRemaining -= deltaTime;
                if (_stateTimeRemaining <= 0f) BeginPlayerReact();
                break;

            case FightState.PlayerReact:
                if (_dodgeRequested)
                {
                    _dodgeRequested = false;
                    int direction = _dodgeDirection;
                    DodgeResolved?.Invoke(direction);
                    BeginResolve();
                    break;
                }

                _stateTimeRemaining -= deltaTime;
                if (_stateTimeRemaining <= 0f)
                {
                    MissedReact?.Invoke();
                    BeginResolve();
                }
                break;

            case FightState.Resolve:
                _stateTimeRemaining -= deltaTime;
                if (_stateTimeRemaining <= 0f) BeginTelegraph();
                break;
        }
    }

    private void BeginTelegraph()
    {
        State = FightState.Telegraph;
        _stateTimeRemaining = _telegraphDuration;
        EnteredTelegraph?.Invoke();
    }

    private void BeginPlayerReact()
    {
        State = FightState.PlayerReact;
        _stateTimeRemaining = _reactDuration;
        _dodgeRequested = false;
        EnteredPlayerReact?.Invoke();
    }

    private void BeginResolve()
    {
        State = FightState.Resolve;
        _stateTimeRemaining = _resolveDuration;
        EnteredResolve?.Invoke();
    }
}

using System;

// What the player is doing in the plaza at this moment: walking, analysing a piece, in a duel,
// or between scenes. Every other system asks this model (stories wait for Exploration, the
// orbit camera only turns while exploring) and the HUD listens to Changed.
public enum TechnicalDemoState { Exploration, Analyzing, Combat, Transition }

public sealed class GameState
{
    public TechnicalDemoState Current { get; private set; } = TechnicalDemoState.Exploration;
    public bool Exploring => Current == TechnicalDemoState.Exploration;

    public event Action<TechnicalDemoState> Changed;

    // True when the state actually changed.
    public bool Set(TechnicalDemoState state)
    {
        if (Current == state) return false;
        Current = state;
        Changed?.Invoke(state);
        return true;
    }
}

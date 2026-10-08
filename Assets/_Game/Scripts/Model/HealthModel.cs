using System;

// Vitality rules of anyone who can be hurt (player, enemies, guardians): damage with a short
// invulnerability after each hit, healing, death and revival. Pure C#: the Health component
// (Controller) hosts one per GameObject and the health bars (View) listen to its events.
public sealed class HealthModel
{
    public float Max { get; private set; }
    public float Current { get; private set; }
    public bool IsDead { get; private set; }
    // Seconds after a hit during which further damage is ignored.
    public float Invulnerability { get; set; }

    public event Action<float, float> Changed;
    public event Action Died;

    private float _lastHit = float.NegativeInfinity;

    public HealthModel(float max, float invulnerability = 0f)
    {
        Max = Math.Max(1f, max);
        Current = Max;
        Invulnerability = Math.Max(0f, invulnerability);
    }

    // now: the game clock in seconds. True when the hit landed.
    public bool TakeDamage(float amount, float now)
    {
        if (IsDead || amount <= 0f) return false;
        if (now - _lastHit < Invulnerability) return false;
        _lastHit = now;
        Current = Math.Max(0f, Current - amount);
        Changed?.Invoke(Current, Max);
        if (Current <= 0f)
        {
            IsDead = true;
            Died?.Invoke();
        }
        return true;
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Current = Math.Min(Max, Current + amount);
        Changed?.Invoke(Current, Max);
    }

    public void Revive()
    {
        IsDead = false;
        Current = Max;
        _lastHit = float.NegativeInfinity;
        Changed?.Invoke(Current, Max);
    }
}

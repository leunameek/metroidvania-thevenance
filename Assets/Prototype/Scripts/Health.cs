using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float, float> HealthChanged;
    public event Action Died;

    private float _lastHitTime = -999f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        if (Time.time - _lastHitTime < invulnerabilityDuration) return;

        _lastHitTime = Time.time;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            Died?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Revive()
    {
        IsDead = false;
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

using System;
using UnityEngine;

// Hosts the HealthModel of a GameObject (player, enemy, guardian): the inspector sets its numbers,
// hits from the scene (hurtboxes, projectiles, falls) come in through TakeDamage, and the rules
// and events live in the model, which the health bars (View) listen to.
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    private HealthModel _model;
    // Created on first use: another object's OnEnable may subscribe before this Awake.
    public HealthModel Model => _model ??= new HealthModel(maxHealth, invulnerabilityDuration);

    public float MaxHealth => Model.Max;
    public float CurrentHealth => Model.Current;
    public bool IsDead => Model.IsDead;

    public event Action<float, float> HealthChanged { add => Model.Changed += value; remove => Model.Changed -= value; }
    public event Action Died { add => Model.Died += value; remove => Model.Died -= value; }

    private void Awake() => _ = Model;

    public void TakeDamage(float amount)
    {
        Model.Invulnerability = invulnerabilityDuration; // the inspector (or a test) may change it at run time
        Model.TakeDamage(amount, Time.time);
    }

    public void Heal(float amount) => Model.Heal(amount);
    public void Revive() => Model.Revive();
}

using UnityEngine;

// Breathing light: crystals flicker softly, find halos pulse a little faster.
public sealed class MIGlow : MonoBehaviour
{
    [SerializeField] private float amount = .25f, period = 3.5f;
    private Light _light;
    private float _base, _phase;
    private void Awake() { _light = GetComponent<Light>(); if (_light != null) _base = _light.intensity; _phase = Random.value * 6f; }
    // Called when gameplay changes the resting brightness (a collected find dims its halo).
    public void SetBase(float intensity) => _base = intensity;
    private void Update()
    {
        if (_light == null) return;
        float wave = Mathf.Sin((Time.time + _phase) * Mathf.PI * 2f / period) * .6f + Mathf.Sin((Time.time + _phase) * 7.3f) * .4f;
        _light.intensity = _base * (1f + amount * wave);
    }
}

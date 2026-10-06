using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class PlayerHealthBarUI : MonoBehaviour
{
    private Health _health;
    private Image _fill;
    private TMP_Text _value;

    private void Awake()
    {
        _health = GetComponent<Health>();
        BuildUI();
    }

    private void OnEnable()
    {
        _health.HealthChanged += OnHealthChanged;
        OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
    }

    private void OnDisable()
    {
        _health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        _fill.fillAmount = max > 0f ? current / max : 0f;
        _value.text = current.ToString("0") + " / " + max.ToString("0");
    }

    private void BuildUI()
    {
        // Same vitality as the plaza HUD (screen 08): portrait ring and crimson bar, top left.
        RectTransform canvas = UIKit.ScreenCanvas(transform, "PlayerHealthBarCanvas", 900);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        _fill = UIKit.HealthFrame(canvas, "Vida", out _value);
    }
}

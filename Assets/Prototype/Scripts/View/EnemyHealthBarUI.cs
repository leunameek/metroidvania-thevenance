using Nemequene.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class EnemyHealthBarUI : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(120f, 14f);
    [SerializeField] private float worldScale = 0.01f;
    // Renamed from fillColor so the prefabs' old bright red is dropped for the ceremonial palette.
    [SerializeField] private Color barColor = new Color(0.64f, 0.2f, 0.16f);

    private Health _health;
    private ShieldEnemy _shieldEnemy;
    private Transform _barTransform;
    private Image _fill;
    private Transform _cameraTransform;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _shieldEnemy = GetComponent<ShieldEnemy>();
        BuildUI();
    }

    private void OnEnable()
    {
        _health.HealthChanged += OnHealthChanged;
        _health.Died += OnDied;
        UpdateVisibility();
    }

    private void OnDisable()
    {
        _health.HealthChanged -= OnHealthChanged;
        _health.Died -= OnDied;
    }

    private void OnDied()
    {
        Destroy(_barTransform.gameObject);
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            _cameraTransform = cam.transform;
        }

        _barTransform.position = transform.position + worldOffset;
        _barTransform.rotation = _cameraTransform.rotation;
    }

    private void OnHealthChanged(float current, float max)
    {
        _fill.fillAmount = max > 0f ? current / max : 0f;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool shielded = _shieldEnemy != null && _shieldEnemy.IsShielded;
        bool atFullHealth = _health.CurrentHealth >= _health.MaxHealth;
        _barTransform.gameObject.SetActive(!shielded && !atFullHealth && !_health.IsDead);
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("EnemyHealthBarCanvas", typeof(RectTransform));
        _barTransform = canvasGo.transform;

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = barSize;
        canvasGo.transform.localScale = Vector3.one * worldScale;

        _fill = UIKit.Bar(canvasRect, barSize, barColor);
        canvasGo.SetActive(false);
    }
}

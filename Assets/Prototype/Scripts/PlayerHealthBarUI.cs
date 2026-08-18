using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Vector2 barSize = new Vector2(280f, 24f);
    [SerializeField] private Vector2 margin = new Vector2(24f, 24f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.2f, 0.2f);

    private Health _health;
    private Image _fill;

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
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("PlayerHealthBarCanvas", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject anchorGo = new GameObject("Anchor", typeof(RectTransform));
        RectTransform anchorRect = anchorGo.GetComponent<RectTransform>();
        anchorRect.SetParent(canvasGo.transform, false);
        anchorRect.anchorMin = new Vector2(0f, 1f);
        anchorRect.anchorMax = new Vector2(0f, 1f);
        anchorRect.pivot = new Vector2(0f, 1f);
        anchorRect.anchoredPosition = new Vector2(margin.x, -margin.y);

        _fill = HealthBarBuilder.Build(anchorRect, barSize, backgroundColor, fillColor);
    }
}

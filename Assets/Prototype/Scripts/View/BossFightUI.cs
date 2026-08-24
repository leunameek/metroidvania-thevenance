using UnityEngine;
using UnityEngine.UI;

// Screen-space UI for the boss encounter: the telegraph alert icon and the turn-order label.
// Built entirely at runtime (same procedural-Canvas technique already used by
// HandOverlayUI/HealthBarBuilder). Presentation only - BossFightController decides when to
// show/hide these in response to BossFightModel's events.
public class BossFightUI : MonoBehaviour
{
    [SerializeField] private Vector2 alertSize = new Vector2(28f, 90f);

    private GameObject _alertGo;
    private GameObject _turnLabelGo;
    private Text _turnLabelText;

    private void Awake()
    {
        BuildAlertIcon();
        BuildTurnLabel();
    }

    public void ShowAlert()
    {
        if (_alertGo != null) _alertGo.SetActive(true);
    }

    public void HideAlert()
    {
        if (_alertGo != null) _alertGo.SetActive(false);
    }

    public void SetTurnLabel(string text)
    {
        if (_turnLabelText == null) return;
        _turnLabelText.text = text;
        _turnLabelGo.SetActive(true);
    }

    public void HideTurnLabel()
    {
        if (_turnLabelGo != null) _turnLabelGo.SetActive(false);
    }

    private void BuildAlertIcon()
    {
        GameObject canvasGo = new GameObject("BossTelegraphAlertCanvas", typeof(RectTransform));
        _alertGo = canvasGo;
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        RectTransform anchorRect = canvasGo.GetComponent<RectTransform>();
        anchorRect.anchorMin = new Vector2(0.5f, 1f);
        anchorRect.anchorMax = new Vector2(0.5f, 1f);
        anchorRect.pivot = new Vector2(0.5f, 1f);
        anchorRect.anchoredPosition = new Vector2(0f, -40f);
        anchorRect.sizeDelta = new Vector2(60f, 120f);

        Color alertColor = new Color(1f, 0.15f, 0.1f);

        GameObject barGo = new GameObject("AlertBar", typeof(RectTransform));
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.SetParent(anchorRect, false);
        barRect.sizeDelta = new Vector2(alertSize.x, alertSize.y * 0.72f);
        barRect.anchoredPosition = new Vector2(0f, -alertSize.y * 0.14f);
        barGo.AddComponent<Image>().color = alertColor;

        GameObject dotGo = new GameObject("AlertDot", typeof(RectTransform));
        RectTransform dotRect = dotGo.GetComponent<RectTransform>();
        dotRect.SetParent(anchorRect, false);
        dotRect.sizeDelta = new Vector2(alertSize.x, alertSize.x);
        dotRect.anchoredPosition = new Vector2(0f, -alertSize.y + alertSize.x * 0.5f);
        dotGo.AddComponent<Image>().color = alertColor;

        canvasGo.SetActive(false);
    }

    private void BuildTurnLabel()
    {
        GameObject canvasGo = new GameObject("BossTurnLabelCanvas", typeof(RectTransform));
        _turnLabelGo = canvasGo;
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 955;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject labelGo = new GameObject("TurnLabel", typeof(RectTransform));
        labelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -10f);
        labelRect.sizeDelta = new Vector2(500f, 50f);

        _turnLabelText = labelGo.AddComponent<Text>();
        _turnLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _turnLabelText.fontSize = 30;
        _turnLabelText.fontStyle = FontStyle.Bold;
        _turnLabelText.alignment = TextAnchor.MiddleCenter;
        _turnLabelText.color = Color.white;

        Shadow shadow = labelGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        canvasGo.SetActive(false);
    }
}

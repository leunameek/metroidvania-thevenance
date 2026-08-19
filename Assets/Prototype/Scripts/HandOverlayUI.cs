using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HandGestureTracker))]
public class HandOverlayUI : MonoBehaviour
{
    [SerializeField] private Vector2 viewportSize = new Vector2(260f, 200f);
    [SerializeField] private Vector2 viewportMargin = new Vector2(24f, 24f);
    [SerializeField] private Color leftHandColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color rightHandColor = new Color(1f, 0.55f, 0.15f);
    [SerializeField] private float dotSize = 8f;

    private HandGestureTracker _tracker;
    private GameObject _canvasRoot;
    private RectTransform _viewport;
    private RectTransform[] _leftDots;
    private RectTransform[] _rightDots;

    private void Awake()
    {
        _tracker = GetComponent<HandGestureTracker>();
        BuildUI();
    }

    public void SetVisible(bool visible)
    {
        if (_canvasRoot != null) _canvasRoot.SetActive(visible);
    }

    private void Update()
    {
        if (_canvasRoot == null || !_canvasRoot.activeSelf) return;

        UpdateHand(_tracker.RightHandPresent, _tracker.RightHandPoints, _rightDots);
        UpdateHand(_tracker.LeftHandPresent, _tracker.LeftHandPoints, _leftDots);
    }

    private void UpdateHand(bool present, IReadOnlyList<Vector2> points, RectTransform[] dots)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            bool show = present && points != null && i < points.Count;
            dots[i].gameObject.SetActive(show);
            if (!show) continue;

            Vector2 normalized = points[i];
            float x = normalized.x * _viewport.rect.width;
            float y = (1f - normalized.y) * _viewport.rect.height;
            dots[i].anchoredPosition = new Vector2(x, y);
        }
    }

    private void BuildUI()
    {
        _canvasRoot = new GameObject("HandOverlayCanvas", typeof(RectTransform));
        _canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = _canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        _canvasRoot.AddComponent<GraphicRaycaster>();

        GameObject panelGo = new GameObject("Viewport", typeof(RectTransform));
        panelGo.transform.SetParent(_canvasRoot.transform, false);
        _viewport = panelGo.GetComponent<RectTransform>();
        _viewport.anchorMin = new Vector2(1f, 0f);
        _viewport.anchorMax = new Vector2(1f, 0f);
        _viewport.pivot = new Vector2(1f, 0f);
        _viewport.sizeDelta = viewportSize;
        _viewport.anchoredPosition = -viewportMargin;

        Image background = panelGo.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.35f);

        _leftDots = CreateDots(21, leftHandColor, "LeftDot");
        _rightDots = CreateDots(21, rightHandColor, "RightDot");
    }

    private RectTransform[] CreateDots(int count, Color color, string namePrefix)
    {
        RectTransform[] dots = new RectTransform[count];
        for (int i = 0; i < count; i++)
        {
            GameObject dotGo = new GameObject(namePrefix + i, typeof(RectTransform));
            dotGo.transform.SetParent(_viewport, false);

            RectTransform rect = dotGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(dotSize, dotSize);

            Image image = dotGo.AddComponent<Image>();
            image.color = color;

            dots[i] = rect;
            dotGo.SetActive(false);
        }

        return dots;
    }
}

using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hands and voice in the world scenes, on the Bacatá HUD kit: a light plate in the lower right
// (above the key hint) with the camera and voice state and the gesture of the moment, a small
// view of the tracked hands, a gold bar under the interaction ribbon while a hold fills, and a
// short ribbon naming what a gesture or a word just did ("Puño · Impulso").
public sealed class NaturalInputHUD : MonoBehaviour
{
    private GameObject _root, _panel, _flash, _hold, _viewport;
    private TMP_Text _handsLine, _voiceLine, _guide, _flashLabel;
    private UIIconGraphic _handsIcon, _voiceIcon;
    private Image _holdFill;
    private RectTransform _viewportRect;
    private RectTransform[] _leftDots, _rightDots;
    private float _flashUntil;
    private bool _panelVisible = true;

    public void Build()
    {
        var canvas = UIKit.ScreenCanvas(transform, "EntradaNatural", 960);
        _root = canvas.gameObject;

        _panel = UIKit.Place(UIKit.HudPanel(canvas, "Estado"), new Vector2(1, 0), new Vector2(-64, 100), new Vector2(470, 176)).gameObject;
        _handsIcon = Row(_panel.transform, UIIcon.Hand, .77f, out _handsLine);
        _voiceIcon = Row(_panel.transform, UIIcon.Voice, .56f, out _voiceLine);
        _guide = UIKit.Label(_panel.transform, "", 18, UIPalette.Muted);
        _guide.alignment = TextAlignmentOptions.TopLeft;
        _guide.rectTransform.anchorMin = Vector2.zero; _guide.rectTransform.anchorMax = new Vector2(1, .43f);
        _guide.rectTransform.offsetMin = new Vector2(30, 14); _guide.rectTransform.offsetMax = new Vector2(-24, -4);

        // Tracked hands, mirrored like the player sees themselves: jade left, gold right.
        _viewport = UIKit.Place(UIKit.HudPanel(canvas, "Manos"), new Vector2(1, 0), new Vector2(-64, 288), new Vector2(230, 150)).gameObject;
        _viewportRect = UIKit.Rect("Puntos", _viewport.transform);
        _viewportRect.offsetMin = new Vector2(14, 12); _viewportRect.offsetMax = new Vector2(-14, -12);
        _leftDots = Dots("I", UIPalette.Jade);
        _rightDots = Dots("D", UIPalette.GoldLight);

        _flash = UIKit.Place(UIKit.Ribbon(canvas, "Gesto", true), new Vector2(.5f, 0), new Vector2(0, 372), new Vector2(640, 64)).gameObject;
        _flashLabel = UIKit.Label(_flash.transform, "", 24, UIPalette.Ivory);
        _flashLabel.fontStyle = FontStyles.Bold; _flashLabel.textWrappingMode = TextWrappingModes.NoWrap;
        _flashLabel.rectTransform.offsetMin = new Vector2(80, 4); _flashLabel.rectTransform.offsetMax = new Vector2(-80, -4);
        _flash.SetActive(false);

        _hold = UIKit.Place(UIKit.Rect("Sosten", canvas), new Vector2(.5f, 0), new Vector2(0, 138), new Vector2(440, 24)).gameObject;
        _holdFill = UIKit.Bar((RectTransform)_hold.transform, new Vector2(420, 18), UIPalette.GoldLight);
        _hold.SetActive(false);
    }

    private static UIIconGraphic Row(Transform panel, UIIcon icon, float y, out TMP_Text label)
    {
        var glyph = UIKit.Icon(panel, icon, UIPalette.GoldLight);
        glyph.rectTransform.anchorMin = glyph.rectTransform.anchorMax = new Vector2(0, y);
        glyph.rectTransform.sizeDelta = new Vector2(30, 30); glyph.rectTransform.anchoredPosition = new Vector2(42, 0);
        label = UIKit.Label(panel, "", 20, UIPalette.Ivory);
        label.alignment = TextAlignmentOptions.MidlineLeft; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.enableAutoSizing = true; label.fontSizeMax = 20; label.fontSizeMin = 14;
        label.rectTransform.anchorMin = new Vector2(0, y - .1f); label.rectTransform.anchorMax = new Vector2(1, y + .1f);
        label.rectTransform.offsetMin = new Vector2(70, 0); label.rectTransform.offsetMax = new Vector2(-22, 0);
        return glyph;
    }

    private RectTransform[] Dots(string prefix, Color color)
    {
        var dots = new RectTransform[21];
        for (int i = 0; i < dots.Length; i++)
        {
            var dot = UIKit.Rect(prefix + i, _viewportRect);
            dot.anchorMin = dot.anchorMax = Vector2.zero; dot.sizeDelta = new Vector2(i == 0 || i == 9 ? 9 : 6, i == 0 || i == 9 ? 9 : 6);
            var image = dot.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
            dot.gameObject.SetActive(false); dots[i] = dot;
        }
        return dots;
    }

    public void SetVisible(bool visible) { if (_root != null && _root.activeSelf != visible) _root.SetActive(visible); }

    public void SetState(string hands, bool handsOn, string voice, bool voiceOn, string guide)
    {
        if (_handsLine.text != hands) _handsLine.text = hands;
        if (_voiceLine.text != voice) _voiceLine.text = voice;
        _handsIcon.color = handsOn ? UIPalette.GoldLight : UIPalette.Disabled;
        _voiceIcon.color = voiceOn ? UIPalette.Jade : UIPalette.Disabled;
        guide ??= "";
        if (_guide.text != guide) _guide.text = guide;
    }

    // The find panel takes the lower right during an inspection; its guide moves into that panel.
    public void SetPanelVisible(bool visible)
    {
        _panelVisible = visible;
        if (_panel.activeSelf != visible) _panel.SetActive(visible);
    }

    public void SetHold(float progress)
    {
        bool show = progress > .02f;
        if (_hold.activeSelf != show) _hold.SetActive(show);
        if (show) _holdFill.fillAmount = Mathf.Clamp01(progress);
    }

    public void Flash(string text)
    {
        _flashLabel.text = text;
        _flash.SetActive(true);
        _flashUntil = Time.unscaledTime + 1.3f;
    }

    public void SetHands(HandGestureTracker tracker, bool live)
    {
        live &= _panelVisible;
        if (_viewport.activeSelf != live) _viewport.SetActive(live);
        if (!live) return;
        Place(tracker.LeftHandPresent, tracker.LeftHandPoints, _leftDots);
        Place(tracker.RightHandPresent, tracker.RightHandPoints, _rightDots);
    }

    private void Place(bool present, IReadOnlyList<Vector2> points, RectTransform[] dots)
    {
        Vector2 size = _viewportRect.rect.size;
        for (int i = 0; i < dots.Length; i++)
        {
            bool show = present && points != null && i < points.Count;
            if (dots[i].gameObject.activeSelf != show) dots[i].gameObject.SetActive(show);
            if (show) dots[i].anchoredPosition = new Vector2((1f - points[i].x) * size.x, (1f - points[i].y) * size.y);
        }
    }

    private void Update()
    {
        if (_flash != null && _flash.activeSelf && Time.unscaledTime >= _flashUntil) _flash.SetActive(false);
    }
}

using System;
using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Interface of the Mundo Inferior with the Bacatá kit (same composition as the plaza):
// vitality upper left, zone, objective and counters upper right, notifications under them,
// lesson-style inspection panel on the right, pause column, guardian bar, defeat and fades.
public sealed class MIHud : MonoBehaviour
{
    private TMP_Text _zone, _objective, _counters, _toastTitle, _toastText, _inspectName, _inspectKind, _inspectBody, _pauseZone, _pauseStats;
    private TMP_Text _bossName, _healthValue, _deathBody;
    private Image _healthFill, _bossFill, _fade;
    private UIIconGraphic _toastIcon;
    private GameObject _toast, _inspect, _pause, _boss, _death, _hints, _header;
    private CanvasGroup _toastGroup;
    private Button _resume;
    private RectTransform _pauseList;
    private TMP_Text _inspectKeys;
    private readonly System.Collections.Generic.List<(TMP_Text, Func<string>)> _pauseLabels = new System.Collections.Generic.List<(TMP_Text, Func<string>)>();
    private float _toastUntil, _healthShown = 1, _healthTarget = 1, _fadeTarget, _fadeValue;
    private Health _health;
    private readonly System.Collections.Generic.Queue<(string, string, UIIcon, Color)> _queue = new System.Collections.Generic.Queue<(string, string, UIIcon, Color)>();

    public bool PauseOpen => _pause != null && _pause.activeSelf;

    public void Build(Health health, Action resume, Action toPlaza, Action toMenu)
    {
        _health = health;
        if (EventSystem.current == null)
        {
            var events = new GameObject("MI_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        var canvas = UIKit.ScreenCanvas(transform, "MundoInferiorHUD", 905);
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        // Soft top veil so the header reads over bright crystals and fog.
        var veil = UIKit.Rect("TopVeil", canvas); veil.anchorMin = new Vector2(0, .74f);
        var veilImage = veil.gameObject.AddComponent<RawImage>(); veilImage.texture = Gradient(); veilImage.raycastTarget = false;

        _healthFill = UIKit.HealthFrame(canvas, "Vida", out _healthValue);
        _healthValue.color = UIPalette.Ivory;
        if (_health != null) { _health.HealthChanged += OnHealth; OnHealth(_health.CurrentHealth, _health.MaxHealth); }

        // Zone, objective and counters on a smoked backdrop (they float over clouds and crystals).
        _header = UIKit.Scrim(canvas, "HeaderScrim", Vector2.one, Vector2.one, .74f).gameObject;
        UIKit.Place((RectTransform)_header.transform, Vector2.one, new Vector2(-18f, -18f), new Vector2(820f, 196f));
        _zone = UIKit.Shadow(UIKit.Label(canvas, "Mundo inferior", 42f, UIPalette.GoldText, true));
        UIKit.Place(_zone.rectTransform, Vector2.one, new Vector2(-76f, -40f), new Vector2(720f, 60f)); _zone.alignment = TextAlignmentOptions.BottomRight;
        _zone.enableAutoSizing = true; _zone.fontSizeMax = 42; _zone.fontSizeMin = 28; _zone.textWrappingMode = TextWrappingModes.NoWrap;
        _objective = UIKit.Shadow(UIKit.Label(canvas, "", 24f, UIPalette.Ivory));
        UIKit.Place(_objective.rectTransform, Vector2.one, new Vector2(-76f, -102f), new Vector2(680f, 60f)); _objective.alignment = TextAlignmentOptions.TopRight;
        _objective.enableAutoSizing = true; _objective.fontSizeMax = 24; _objective.fontSizeMin = 18;
        _counters = UIKit.Shadow(UIKit.Label(canvas, "", 20f, UIPalette.Ivory));
        UIKit.Place(_counters.rectTransform, Vector2.one, new Vector2(-76f, -166f), new Vector2(720f, 30f)); _counters.alignment = TextAlignmentOptions.TopRight;
        _counters.enableAutoSizing = true; _counters.fontSizeMax = 20; _counters.fontSizeMin = 16; _counters.textWrappingMode = TextWrappingModes.NoWrap;

        // Notification plate (INPUT_notification): icon, name and text, never colour alone.
        // The plate grows with its text (SizeToast), so a long line never spills out of it.
        _toast = UIKit.Place(UIKit.HudPanel(canvas, "Toast"), Vector2.one, new Vector2(-64f, -222f), new Vector2(580f, 112f)).gameObject;
        _toastGroup = _toast.AddComponent<CanvasGroup>();
        _toastIcon = UIKit.Icon(_toast.transform, UIIcon.Info, UIPalette.GoldLight);
        UIKit.Place(_toastIcon.rectTransform, new Vector2(0, 1), new Vector2(28, -24), new Vector2(44, 44));
        _toastIcon.rectTransform.pivot = new Vector2(0, 1);
        _toastTitle = UIKit.Label(_toast.transform, "", 22, UIPalette.GoldText, true); _toastTitle.alignment = TextAlignmentOptions.TopLeft;
        Inset(_toastTitle.rectTransform, 92, 24, 18, 0, 30);
        _toastText = UIKit.Label(_toast.transform, "", 20, UIPalette.Ivory); _toastText.alignment = TextAlignmentOptions.TopLeft;
        Inset(_toastText.rectTransform, 92, 24, 50, 0, 60);
        _toast.SetActive(false);

        // Inspection of a find (screens 10-12): the piece keeps the left, the panel the right.
        _inspect = UIKit.Rect("Inspection", canvas).gameObject;
        var panel = UIKit.Rect("Panel", _inspect.transform); panel.anchorMin = new Vector2(.60f, .11f); panel.anchorMax = new Vector2(.94f, .76f);
        var bg = panel.gameObject.AddComponent<Image>(); bg.color = UIPalette.Stone; bg.raycastTarget = false;
        if (UIBacata.Available) UIBacata.Frame(panel.gameObject, .6f, true, true);
        _inspectName = UIKit.Label(panel, "", 44, UIPalette.GoldText, true); _inspectName.alignment = TextAlignmentOptions.TopLeft;
        Inset(_inspectName.rectTransform, 60, 60, 84, -1, 64);
        _inspectKind = UIKit.Label(panel, "", 20, UIPalette.Jade); _inspectKind.alignment = TextAlignmentOptions.TopLeft; _inspectKind.characterSpacing = 2;
        _inspectKind.fontStyle = FontStyles.UpperCase; Inset(_inspectKind.rectTransform, 60, 60, 150, -1, 30);
        _inspectBody = UIKit.Label(panel, "", 24, UIPalette.Ivory); _inspectBody.alignment = TextAlignmentOptions.TopLeft;
        _inspectBody.rectTransform.offsetMin = new Vector2(60, 232); _inspectBody.rectTransform.offsetMax = new Vector2(-60, -190);
        _inspectBody.enableAutoSizing = true; _inspectBody.fontSizeMax = 24; _inspectBody.fontSizeMin = 17;
        // What to do with the piece: a rule above it, one line per way of answering.
        var keyRule = UIKit.Icon(panel, UIIcon.Divider, UIPalette.Gold);
        keyRule.rectTransform.anchorMin = new Vector2(0, 0); keyRule.rectTransform.anchorMax = new Vector2(1, 0);
        keyRule.rectTransform.offsetMin = new Vector2(60, 214); keyRule.rectTransform.offsetMax = new Vector2(-60, 226);
        var keys = _inspectKeys = UIKit.Label(panel, InspectionKeys, 19, UIPalette.Muted);
        keys.alignment = TextAlignmentOptions.TopLeft; keys.rectTransform.anchorMin = Vector2.zero; keys.rectTransform.anchorMax = new Vector2(1, 0);
        keys.rectTransform.offsetMin = new Vector2(60, 40); keys.rectTransform.offsetMax = new Vector2(-60, 206);
        keys.textWrappingMode = TextWrappingModes.Normal; keys.enableAutoSizing = true; keys.fontSizeMax = 20; keys.fontSizeMin = 16;
        keys.lineSpacing = 6;
        _inspect.SetActive(false);

        // Guardian bar: name in the serif and a crimson bar at the top centre.
        // Guardian bar: name in the serif and a crimson bar at the bottom centre, on a backdrop, so
        // it never meets the zone and objective of the upper right.
        _boss = UIKit.Rect("GuardianBar", canvas).gameObject;
        var bossRect = UIKit.Place((RectTransform)_boss.transform, new Vector2(.5f, 0), new Vector2(0, 244), new Vector2(760, 96));
        UIKit.Scrim(bossRect, "Scrim", Vector2.zero, Vector2.one, .6f).rectTransform.offsetMin = new Vector2(-60, -14);
        _bossName = UIKit.Shadow(UIKit.Label(_boss.transform, "", 30, UIPalette.GoldText, true));
        _bossName.rectTransform.anchorMin = new Vector2(0, .5f);
        _bossName.enableAutoSizing = true; _bossName.fontSizeMax = 30; _bossName.fontSizeMin = 20; _bossName.textWrappingMode = TextWrappingModes.NoWrap;
        _bossFill = UIKit.Bar((RectTransform)_boss.transform, new Vector2(640, 26), UIPalette.Crimson);
        var rail = (RectTransform)_bossFill.transform.parent.parent; rail.anchorMin = rail.anchorMax = new Vector2(.5f, .26f);
        _boss.SetActive(false);

        // Pause (screen 19): one framed column over the dimmed cavern.
        _pause = UIKit.Rect("Pause", canvas).gameObject;
        var shade = _pause.AddComponent<Image>(); shade.color = new Color(.031f, .039f, .043f, .82f);
        var column = UIKit.Rect("Column", _pause.transform); column.anchorMin = new Vector2(.355f, .10f); column.anchorMax = new Vector2(.645f, .88f);
        var columnBg = column.gameObject.AddComponent<Image>(); columnBg.color = UIPalette.Stone;
        if (UIBacata.Available) UIBacata.Frame(column.gameObject, .6f, true, true);
        var title = UIKit.Label(column, "Pausa", 60, UIPalette.Danger, true); title.rectTransform.anchorMin = new Vector2(.08f, .82f); title.rectTransform.anchorMax = new Vector2(.92f, .92f);
        _pauseZone = UIKit.Label(column, "", 22, UIPalette.Muted); _pauseZone.rectTransform.anchorMin = new Vector2(.08f, .76f); _pauseZone.rectTransform.anchorMax = new Vector2(.92f, .82f);
        var rule = UIKit.Icon(column, UIIcon.Divider, UIPalette.Gold); rule.rectTransform.anchorMin = new Vector2(.16f, .735f); rule.rectTransform.anchorMax = new Vector2(.84f, .765f);
        var list = UIKit.Rect("Actions", column); list.anchorMin = new Vector2(.08f, .30f); list.anchorMax = new Vector2(.92f, .72f);
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 6; layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
        _pauseList = list;
        _resume = UIKitButton.Create(list, "Continuar", resume, true);
        UIKitButton.Create(list, "Volver a Plaza Núñez", toPlaza);
        UIKitButton.Create(list, "Volver al menú principal", toMenu);
        _pauseStats = UIKit.Label(column, "", 20, UIPalette.Muted); _pauseStats.rectTransform.anchorMin = new Vector2(.08f, .07f); _pauseStats.rectTransform.anchorMax = new Vector2(.92f, .27f);
        UIKit.Hint(_pause.transform, "Esc", "Continuar", Vector2.zero);
        UIKit.Hint(_pause.transform, "Enter", "Seleccionar", Vector2.right);
        _pause.SetActive(false);

        // Defeat (screen 28): no frame, crimson title, what is kept.
        _death = UIKit.Rect("Defeat", canvas).gameObject;
        var deathShade = _death.AddComponent<Image>(); deathShade.color = new Color(.07f, .02f, .03f, .86f); deathShade.raycastTarget = false;
        var deathTitle = UIKit.Label(_death.transform, "Has caído", 72, UIPalette.Danger, true);
        deathTitle.rectTransform.anchorMin = new Vector2(.2f, .52f); deathTitle.rectTransform.anchorMax = new Vector2(.8f, .66f);
        _deathBody = UIKit.Label(_death.transform, "", 24, UIPalette.Muted);
        _deathBody.rectTransform.anchorMin = new Vector2(.25f, .40f); _deathBody.rectTransform.anchorMax = new Vector2(.75f, .52f);
        _death.SetActive(false);

        _hints = UIKit.Rect("Hints", canvas).gameObject;
        UIKit.Hint(_hints.transform, "Esc", "Pausa", Vector2.zero);
        UIKit.Hint(_hints.transform, VoicePrompt.Cap("impulso", "Q"), "Impulso", Vector2.right);

        var fade = UIKit.Rect("Fade", canvas); _fade = fade.gameObject.AddComponent<Image>();
        _fade.color = new Color(.031f, .039f, .043f, 0); _fade.raycastTarget = false;
    }

    private static void Inset(RectTransform rect, float left, float right, float top, float bottom, float height)
    {
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1);
        rect.offsetMin = new Vector2(left, -top - height); rect.offsetMax = new Vector2(-right, -top);
    }

    private static Texture2D Gradient()
    {
        var texture = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 64; y++) texture.SetPixel(0, y, new Color(.031f, .039f, .043f, Mathf.SmoothStep(0, .7f, y / 63f)));
        texture.Apply();
        return texture;
    }

    private void OnHealth(float value, float max)
    {
        _healthTarget = max > 0 ? value / max : 0;
        _healthValue.text = value.ToString("0") + " / " + max.ToString("0");
    }

    public void SetZone(string zone) { _zone.text = zone; _pauseZone.text = zone; }
    public void SetObjective(string objective)
    {
        // A new objective is heard as well as read (not the first one of the scene).
        if (!string.IsNullOrEmpty(_objective.text) && objective != _objective.text) GameAudio.UI(UICue.Objective);
        _objective.text = objective;
    }
    public void SetCounters(string counters) { _counters.text = counters; _pauseStats.text = counters.Replace("   ·   ", "\n"); }
    public void SetHintsVisible(bool visible) { if (_hints.activeSelf != visible) _hints.SetActive(visible); }
    // Zone, objective and counters step aside while the turn duel screen uses the top band.
    public void SetHeaderVisible(bool visible)
    {
        foreach (var t in new[] { _zone, _objective, _counters })
            if (t != null && t.gameObject.activeSelf != visible) t.gameObject.SetActive(visible);
        if (_header != null && _header.activeSelf != visible) _header.SetActive(visible);
    }

    public void Notify(string title, string text, UIIcon icon, Color color)
    {
        _queue.Enqueue((title, text, icon, color));
    }

    public void ShowInspection(string name, string kind, string body)
    {
        _inspectName.text = name; _inspectKind.text = kind; _inspectBody.text = body;
        _inspect.SetActive(true);
    }
    public void HideInspection() { _inspect.SetActive(false); _fitStatus = null; _guide = null; }

    private const string InspectionKeys = "E · Tomar     Esc · Devolver al altar\nRatón o A / D · Girar";
    private const string FitKeys = "E · Encajar     Esc · Devolver al altar\nRatón, A / D y W / S · Girar";
    private string _fitStatus, _guide;
    // Hands or voice on: the words and gestures come first, the keys after them. A piece to fit
    // in its table puts its status (turning, seated, not yet) above them, in gold.
    public void SetInspectionGuide(string guide) { _guide = guide; ComposeKeys(); }
    public void SetInspectionFit(string status) { _fitStatus = status; ComposeKeys(); }
    private void ComposeKeys()
    {
        if (_inspectKeys == null) return;
        string keys = _fitStatus != null ? FitKeys : InspectionKeys;
        string text = (string.IsNullOrEmpty(_fitStatus) ? "" : "<color=#E8C77A>" + _fitStatus + "</color>\n")
            + (string.IsNullOrEmpty(_guide) ? "" : _guide + "\n") + keys;
        _inspectKeys.richText = true;
        if (_inspectKeys.text != text) _inspectKeys.text = text;
    }

    // Extra pause entry right under «Continuar»; its label is read again each time the pause opens.
    public void AddPauseButton(Func<string> label, Action action)
    {
        if (_pauseList == null) return;

        var button = UIKitButton.Create(_pauseList, label(), () => { action(); Relabel(); });
        button.transform.SetSiblingIndex(1 + _pauseLabels.Count);
        _pauseLabels.Add((button.GetComponentInChildren<TMP_Text>(), label));
    }

    private void Relabel()
    {
        foreach (var (text, label) in _pauseLabels)
        {
            string value = label();
            if (text != null && text.text != value) text.text = value;
        }
    }

    public void ShowPause(bool open)
    {
        _pause.SetActive(open);
        if (open) Relabel();
        if (open && EventSystem.current != null) { EventSystem.current.SetSelectedGameObject(null); EventSystem.current.SetSelectedGameObject(_resume.gameObject); }
    }

    public void SetBoss(string name, float fraction, bool visible)
    {
        if (_boss.activeSelf != visible) _boss.SetActive(visible);
        _bossName.text = name; _bossFill.fillAmount = Mathf.Clamp01(fraction);
    }

    public void ShowDeath(bool visible, string body = "")
    {
        _death.SetActive(visible); _deathBody.text = body;
    }

    public void SetFade(float target) => _fadeTarget = target;

    private void Update()
    {
        if (_pause.activeSelf && _pauseLabels.Count > 0) Relabel(); // the camera starts asynchronously
        _healthShown = Mathf.MoveTowards(_healthShown, _healthTarget, Time.unscaledDeltaTime * 1.4f);
        if (_healthFill != null)
        {
            _healthFill.fillAmount = _healthShown;
            // In a turn duel the same vitality frame sits at the bottom left (TurnDuelHUD).
            var vitality = _healthFill.transform.parent.parent.gameObject;
            if (vitality.activeSelf == TurnDuelController.Running) vitality.SetActive(!TurnDuelController.Running);
        }
        _fadeValue = Mathf.MoveTowards(_fadeValue, _fadeTarget, Time.unscaledDeltaTime * 3f);
        _fade.color = new Color(.031f, .039f, .043f, _fadeValue);
        // Notices wait while the inspection, the pause or a duel holds the screen.
        bool held = _inspect.activeSelf || _pause.activeSelf || _death.activeSelf || TurnDuelController.Running;
        if (held) { if (_toast.activeSelf) _toast.SetActive(false); _toastUntil = 0; }
        else if (Time.unscaledTime >= _toastUntil)
        {
            if (_queue.Count > 0)
            {
                var (title, text, icon, color) = _queue.Dequeue();
                _toastTitle.text = title; _toastText.text = text; _toastIcon.SetIcon(icon); _toastIcon.color = color;
                _toast.SetActive(true); SizeToast(); _toastUntil = Time.unscaledTime + 4.5f;
            }
            else if (_toast.activeSelf) _toast.SetActive(false);
        }
        if (_toast.activeSelf) _toastGroup.alpha = Mathf.Clamp01((_toastUntil - Time.unscaledTime) / .3f);
    }

    // Height of the notice plate from its text: title line, then the body as it wraps.
    private void SizeToast()
    {
        var plate = (RectTransform)_toast.transform;
        float width = plate.sizeDelta.x - 92 - 24;
        float body = string.IsNullOrEmpty(_toastText.text) ? 0 : _toastText.GetPreferredValues(_toastText.text, width, 0).y;
        float height = Mathf.Max(96, 50 + body + 22);
        plate.sizeDelta = new Vector2(plate.sizeDelta.x, height);
        _toastText.rectTransform.offsetMin = new Vector2(92, -50 - body - 4);
    }

    private void OnDestroy() { if (_health != null) _health.HealthChanged -= OnHealth; }
}

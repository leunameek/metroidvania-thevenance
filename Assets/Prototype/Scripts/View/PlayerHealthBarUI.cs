using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Vector2 barSize = new Vector2(280f, 24f);
    // Renamed from margin/fillColor so the old serialized 24 px margin and bright red are dropped.
    [SerializeField] private Vector2 screenMargin = new Vector2(96f, 44f);
    // Ceremonial red: ivory figures on it keep at least 5.4:1.
    [SerializeField] private Color barColor = new Color(0.64f, 0.2f, 0.16f);

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
        // Same vitality plate as the plaza HUD: top-left inside the 5 % / 4 % safe margins.
        RectTransform canvas = UIKit.ScreenCanvas(transform, "PlayerHealthBarCanvas", 900);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        RectTransform panel = UIKit.Place(UIKit.HudPanel(canvas, "Vitality"), new Vector2(0f, 1f), new Vector2(screenMargin.x, -screenMargin.y),
            new Vector2(barSize.x + 140f, 96f));
        UIIconGraphic heart = UIKit.Icon(panel, UIIcon.Heart, UITheme.Hex("C8352C"));
        UIKit.Place(heart.rectTransform, new Vector2(0f, .5f), new Vector2(24f, 0f), new Vector2(44f, 44f));
        TMP_Text caption = UIKit.Label(panel, "Vida", 20f, UIPalette.GoldText, true);
        caption.alignment = TextAlignmentOptions.MidlineLeft;
        caption.rectTransform.offsetMin = new Vector2(88f, 50f); caption.rectTransform.offsetMax = new Vector2(-20f, -10f);
        _fill = UIKit.Bar(panel, barSize, barColor);
        RectTransform rail = (RectTransform)_fill.transform.parent.parent;
        UIKit.Place(rail, Vector2.zero, new Vector2(88f, 16f), new Vector2(barSize.x + 32f, 34f));
        _value = UIKit.Label(rail, "", 20f, UIPalette.Ivory);
        _value.alignment = TextAlignmentOptions.MidlineRight; _value.margin = new Vector4(0f, 0f, 12f, 0f);
    }
}

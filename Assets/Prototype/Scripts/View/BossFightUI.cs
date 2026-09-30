using Nemequene.UI;
using TMPro;
using UnityEngine;

// Screen-space UI for the boss encounter: the telegraph alert icon and the turn-order label.
// Built entirely at runtime with UIKit, the prototype mirror of the Nemequene UI factory. Presentation only - BossFightController decides when to
// show/hide these in response to BossFightModel's events.
public class BossFightUI : MonoBehaviour
{
    [SerializeField] private float alertTop = 150f;

    private GameObject _alertGo;
    private GameObject _turnLabelGo;
    private TMP_Text _turnLabelText;

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
        // Sun medallion with "!" and a written cue below the turn plate (the old icon anchored the
        // root canvas rect, which Unity ignores, so it rendered at the screen centre).
        _alertGo = UIKit.Alert(transform, "BossTelegraphAlertCanvas", 950, "¡Prepárate!", alertTop);
    }

    private void BuildTurnLabel()
    {
        RectTransform canvas = UIKit.ScreenCanvas(transform, "BossTurnLabelCanvas", 955);
        _turnLabelGo = canvas.gameObject;
        RectTransform plate = UIKit.Place(UIKit.HudPanel(canvas, "TurnPlate"), new Vector2(.5f, 1f), new Vector2(0f, -44f), new Vector2(560f, 84f));
        _turnLabelText = UIKit.Label(plate, "", 30f, UIPalette.GoldText, true);
        _turnLabelText.rectTransform.offsetMin = new Vector2(24f, 8f); _turnLabelText.rectTransform.offsetMax = new Vector2(-24f, -8f);
        _turnLabelGo.SetActive(false);
    }
}

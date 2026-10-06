using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Flight budget of the wings (guide 4.4 and 17): a gold bar at the lower centre, above the
// interaction ribbon, shown while flying or recharging; it turns crimson under 25 % and names
// the key, so it never depends on colour alone.
public sealed class MSFlightMeter : MonoBehaviour
{
    private MSWings _wings;
    private GameObject _root;
    private Image _fill;
    private TMP_Text _label;

    public void Build(MSWings wings)
    {
        _wings = wings;
        var canvas = UIKit.ScreenCanvas(transform, "MS_Vuelo", 904);
        _root = UIKit.Rect("Vuelo", canvas).gameObject;
        var rect = (RectTransform)_root.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.sizeDelta = new Vector2(520, 70); rect.anchoredPosition = new Vector2(0, 290);
        _label = UIKit.Shadow(UIKit.Label(rect, "Alas · F", 22, UIPalette.GoldText, true));
        _label.rectTransform.anchorMin = new Vector2(0, .55f); _label.alignment = TextAlignmentOptions.Bottom;
        _fill = UIKit.Bar(rect, new Vector2(420, 20), UIPalette.Gold);
        var rail = (RectTransform)_fill.transform.parent.parent; rail.anchorMin = rail.anchorMax = new Vector2(.5f, .25f);
        _root.SetActive(false);
    }

    private void Update()
    {
        if (_wings == null || _root == null) return;
        float value = _wings.Remaining01;
        bool show = _wings.Owned && (_wings.Flying || value < .999f);
        if (_root.activeSelf != show) _root.SetActive(show);
        if (!show) return;
        _fill.fillAmount = value;
        bool low = value <= .25f;
        _fill.color = low ? UIPalette.Crimson : UIPalette.Gold;
        string text = _wings.Flying ? (low ? "Alas · poco vuelo · F cierra" : "Alas · Espacio / Ctrl suben o bajan · F cierra") : "Alas · se recargan al aterrizar";
        if (_label.text != text) _label.text = text;
    }
}

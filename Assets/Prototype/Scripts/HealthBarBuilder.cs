using UnityEngine;
using UnityEngine.UI;

public static class HealthBarBuilder
{
    private static Sprite _fillSprite;

    public static Image Build(RectTransform parent, Vector2 size, Color backgroundColor, Color fillColor)
    {
        GameObject backgroundGo = new GameObject("HealthBarBackground", typeof(RectTransform));
        RectTransform backgroundRect = backgroundGo.GetComponent<RectTransform>();
        backgroundRect.SetParent(parent, false);
        backgroundRect.sizeDelta = size;

        Image background = backgroundGo.AddComponent<Image>();
        background.color = backgroundColor;

        GameObject fillGo = new GameObject("HealthBarFill", typeof(RectTransform));
        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.SetParent(backgroundRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        if (_fillSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            _fillSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }

        Image fill = fillGo.AddComponent<Image>();
        fill.sprite = _fillSprite;
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;

        return fill;
    }
}

using Nemequene.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Story dialogue of the Bacatá kit for every scene (plaza and both worlds): smoked plate low on
// the screen, speaker on the crimson ribbon, the line revealed letter by letter, and the two
// controls written on screen (advance; hold to skip) with the hold progress as a thin bar.
public sealed class StoryDialogueView
{
    private readonly GameObject _root;
    private readonly TMP_Text _speaker, _line, _hint, _title;
    private readonly RectTransform _ribbon;
    private readonly GameObject _titlePlate;
    private readonly Image _skip;

    public StoryDialogueView(Transform parent)
    {
        var canvas = UIKit.ScreenCanvas(parent, "StoryDialogueCanvas", 1300);
        _root = canvas.gameObject;
        var panel = UIKit.Place(UIKit.HudPanel(canvas, "StoryPanel"), new Vector2(.5f, 0), new Vector2(0, 56), new Vector2(1320, 250));
        // A deeper plate than the HUD's: the line is read over any scene behind it.
        var deep = UIKit.Rect("Depth", panel); deep.SetAsFirstSibling(); deep.offsetMin = new Vector2(10, 10); deep.offsetMax = new Vector2(-10, -10);
        var depth = deep.gameObject.AddComponent<Image>(); var tone = UIPalette.Deep; tone.a = .9f; depth.color = tone; depth.raycastTarget = false;
        _ribbon = UIKit.Place(UIKit.Ribbon(panel, "Speaker"), new Vector2(0, 1), new Vector2(64, 26), new Vector2(420, 60));
        _speaker = UIKit.Label(_ribbon, "", 26, UIPalette.GoldLight, true);
        // One line always inside the plate (2026-10-07 playtest: «Guardián de entrenamiento» spilled
        // out): the plate widens with the name up to 640 px, and a longer one shrinks to fit.
        _speaker.margin = new Vector4(44, 4, 44, 4);
        _speaker.textWrappingMode = TextWrappingModes.NoWrap;
        _speaker.enableAutoSizing = true; _speaker.fontSizeMax = 26; _speaker.fontSizeMin = 16;
        _line = UIKit.Label(panel, "", 30, UIPalette.Ivory);
        _line.alignment = TextAlignmentOptions.TopLeft;
        _line.enableAutoSizing = true; _line.fontSizeMax = 30; _line.fontSizeMin = 21;
        _line.rectTransform.anchorMin = new Vector2(.05f, .30f); _line.rectTransform.anchorMax = new Vector2(.95f, .80f);
        _hint = UIKit.Label(panel, "", 20, UIPalette.Ivory);
        _hint.alignment = TextAlignmentOptions.BottomRight;
        _hint.rectTransform.anchorMin = new Vector2(.05f, .06f); _hint.rectTransform.anchorMax = new Vector2(.95f, .24f);
        var bar = UIKit.Rect("SkipTrack", panel);
        bar.anchorMin = new Vector2(.05f, .04f); bar.anchorMax = new Vector2(.30f, .07f);
        _skip = UIKit.Bar(bar, new Vector2(320, 10), UIPalette.GoldLight);
        _skip.fillAmount = 0;
        // Chapter/place card shown at the start of a scene ("Plaza Núñez").
        var title = UIKit.Place(UIKit.HudPanel(canvas, "StoryTitle"), new Vector2(.5f, 1), new Vector2(0, -90), new Vector2(760, 96));
        _titlePlate = title.gameObject;
        _title = UIKit.Label(title, "", 34, UIPalette.GoldLight, true);
        _root.SetActive(false);
    }

    public void Show(bool visible) { if (_root.activeSelf != visible) _root.SetActive(visible); }
    public void SetTitle(string title)
    {
        _titlePlate.SetActive(!string.IsNullOrEmpty(title));
        _title.text = title ?? "";
    }
    public void SetLine(string speaker, string note, string text)
    {
        _speaker.text = string.IsNullOrEmpty(note) ? speaker : speaker + " · " + note;
        _speaker.enableAutoSizing = false; _speaker.fontSize = 26;
        float width = _speaker.GetPreferredValues(_speaker.text, 10000, 60).x + 88 + 24;
        _speaker.enableAutoSizing = true;
        _ribbon.sizeDelta = new Vector2(Mathf.Clamp(width, 420, 640), _ribbon.sizeDelta.y);
        _line.text = text;
        _line.maxVisibleCharacters = 0;
    }
    public void Reveal(int characters) => _line.maxVisibleCharacters = characters;
    public void SetHint(string hint) => _hint.text = hint;
    public void SetSkip(float progress) => _skip.fillAmount = Mathf.Clamp01(progress);
}

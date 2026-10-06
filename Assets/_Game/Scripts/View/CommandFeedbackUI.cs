using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(VoiceCommandRecognizer))]
public class CommandFeedbackUI : MonoBehaviour
{
    private static readonly Dictionary<CombatCommand, string> EffectDescriptions = new Dictionary<CombatCommand, string>
    {
        { CombatCommand.Dodge, "Esquivar según la mano alzada" },
        { CombatCommand.Attack, "Atacar" },
        { CombatCommand.Guard, "Bloquear" }
    };

    [SerializeField] private float displayDuration = 1.5f;

    private VoiceCommandRecognizer _recognizer;
    private GameObject _canvasRoot;
    private TMP_Text _label;
    private Coroutine _hideRoutine;

    private void Awake()
    {
        _recognizer = GetComponent<VoiceCommandRecognizer>();
        _recognizer.CommandRecognized += OnCommandRecognized;
        BuildUI();
    }

    private void OnDestroy()
    {
        if (_recognizer != null) _recognizer.CommandRecognized -= OnCommandRecognized;
    }

    private void OnCommandRecognized(CombatCommand command, string phrase)
    {
        string effect = EffectDescriptions.TryGetValue(command, out string description) ? description : command.ToString();
        _label.text = phrase.ToUpperInvariant() + " · " + effect;
        _canvasRoot.SetActive(true);

        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        _canvasRoot.SetActive(false);
    }

    private void BuildUI()
    {
        // HUD plate above the interaction band, "FRASE · efecto" as in the plaza's key prompts.
        RectTransform canvas = UIKit.ScreenCanvas(transform, "CommandFeedbackCanvas", 960);
        _canvasRoot = canvas.gameObject;
        RectTransform plate = UIKit.Place(UIKit.HudPanel(canvas, "CommandPlate"), new Vector2(.5f, 0f), new Vector2(0f, 180f), new Vector2(900f, 76f));
        _label = UIKit.Label(plate, "", 28f, UIPalette.Ivory);
        _label.rectTransform.offsetMin = new Vector2(28f, 8f); _label.rectTransform.offsetMax = new Vector2(-28f, -8f);
        _canvasRoot.SetActive(false);
    }
}

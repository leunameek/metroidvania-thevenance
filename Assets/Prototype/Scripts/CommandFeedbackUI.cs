using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VoiceCommandRecognizer))]
public class CommandFeedbackUI : MonoBehaviour
{
    private static readonly Dictionary<CombatCommand, string> EffectDescriptions = new Dictionary<CombatCommand, string>
    {
        { CombatCommand.Dodge, "Esquivar (segun la mano alzada)" }
    };

    [SerializeField] private float displayDuration = 1.5f;

    private VoiceCommandRecognizer _recognizer;
    private GameObject _canvasRoot;
    private Text _label;
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
        _label.text = $"{phrase} -> {effect}";
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
        _canvasRoot = new GameObject("CommandFeedbackCanvas", typeof(RectTransform));
        _canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 960;

        CanvasScaler scaler = _canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject labelGo = new GameObject("CommandLabel", typeof(RectTransform));
        labelGo.transform.SetParent(_canvasRoot.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 140f);
        labelRect.sizeDelta = new Vector2(900f, 60f);

        _label = labelGo.AddComponent<Text>();
        _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize = 32;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color = Color.white;

        Shadow shadow = labelGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        _canvasRoot.SetActive(false);
    }
}

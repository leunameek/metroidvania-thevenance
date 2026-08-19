using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public enum CombatCommand
{
    Dodge
}

public class VoiceCommandRecognizer : MonoBehaviour
{
    private static readonly Dictionary<string, CombatCommand> Phrases = new Dictionary<string, CombatCommand>
    {
        { "esquiva", CombatCommand.Dodge },
        { "dodge", CombatCommand.Dodge }
    };

    public event Action<CombatCommand, string> CommandRecognized;

    public bool IsListening => _recognizer != null && _recognizer.IsRunning;

    private KeywordRecognizer _recognizer;

    public void StartListening()
    {
        if (_recognizer != null) return;

        string[] keywords = new string[Phrases.Count];
        Phrases.Keys.CopyTo(keywords, 0);

        _recognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
        _recognizer.OnPhraseRecognized += OnPhraseRecognized;
        _recognizer.Start();
        Debug.Log($"[VoiceCommandRecognizer] Listening for: {string.Join(", ", keywords)} (running: {_recognizer.IsRunning})");
    }

    public void StopListening()
    {
        if (_recognizer == null) return;

        _recognizer.OnPhraseRecognized -= OnPhraseRecognized;
        _recognizer.Stop();
        _recognizer.Dispose();
        _recognizer = null;
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"[VoiceCommandRecognizer] Heard '{args.text}' (confidence: {args.confidence})");
        if (Phrases.TryGetValue(args.text, out CombatCommand command))
            CommandRecognized?.Invoke(command, args.text);
    }

    private void OnDestroy()
    {
        StopListening();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public enum CombatCommand
{
    Dodge, Attack, Guard
}

public class VoiceCommandRecognizer : MonoBehaviour
{
    private static readonly Dictionary<string, CombatCommand> Phrases = new Dictionary<string, CombatCommand>
    {
        { "esquiva", CombatCommand.Dodge },
        { "dodge", CombatCommand.Dodge }
        , { "esquivar", CombatCommand.Dodge }
        , { "atacar", CombatCommand.Attack }
        , { "bloquear", CombatCommand.Guard }
    };

    public event Action<CombatCommand, string> CommandRecognized;
    public event Action<string> Unavailable;
    public int MinimumConfidence { get; set; } = 0;
    public string LastError { get; private set; }

    public bool IsListening => _recognizer != null && _recognizer.IsRunning;

    private KeywordRecognizer _recognizer;

    public void StartListening()
    {
        if (_recognizer != null) return;
        LastError = null;
        try
        {

        string[] keywords = new string[Phrases.Count];
        Phrases.Keys.CopyTo(keywords, 0);

        _recognizer = new KeywordRecognizer(keywords, MinimumConfidence >= 2 ? ConfidenceLevel.High : MinimumConfidence == 1 ? ConfidenceLevel.Medium : ConfidenceLevel.Low);
        _recognizer.OnPhraseRecognized += OnPhraseRecognized;
        _recognizer.Start();
        Debug.Log($"[VoiceCommandRecognizer] Listening for: {string.Join(", ", keywords)} (running: {_recognizer.IsRunning})");
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            StopListening();
            Unavailable?.Invoke(LastError);
        }
    }

    public void StopListening()
    {
        if (_recognizer == null) return;

        _recognizer.OnPhraseRecognized -= OnPhraseRecognized;
        if (_recognizer.IsRunning) _recognizer.Stop();
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
    private void OnDisable() { StopListening(); }
}

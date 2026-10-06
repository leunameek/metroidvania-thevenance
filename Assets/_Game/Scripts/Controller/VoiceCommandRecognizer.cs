using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

// Combat verbs plus the words that drive objects: approach and examine, take or confirm, leave.
public enum VoiceCommand
{
    Dodge, Attack, Guard, Interact, Confirm, Back
}

public class VoiceCommandRecognizer : MonoBehaviour
{
    private static readonly Dictionary<string, VoiceCommand> Phrases = new Dictionary<string, VoiceCommand>
    {
        { "esquiva", VoiceCommand.Dodge }, { "esquivar", VoiceCommand.Dodge }, { "dodge", VoiceCommand.Dodge },
        { "atacar", VoiceCommand.Attack }, { "ataca", VoiceCommand.Attack }, { "golpe", VoiceCommand.Attack },
        { "impulso", VoiceCommand.Attack },
        { "bloquear", VoiceCommand.Guard }, { "bloquea", VoiceCommand.Guard }, { "escudo", VoiceCommand.Guard },
        { "examinar", VoiceCommand.Interact }, { "examina", VoiceCommand.Interact }, { "usar", VoiceCommand.Interact },
        { "activar", VoiceCommand.Interact }, { "entrar", VoiceCommand.Interact }, { "enfrentar", VoiceCommand.Interact },
        { "tomar", VoiceCommand.Confirm }, { "recoger", VoiceCommand.Confirm }, { "confirmar", VoiceCommand.Confirm },
        { "salir", VoiceCommand.Back }, { "volver", VoiceCommand.Back }, { "cerrar", VoiceCommand.Back }
    };

    public static IEnumerable<string> PhrasesFor(VoiceCommand command)
    {
        foreach (var pair in Phrases) if (pair.Value == command) yield return pair.Key;
    }

    public event Action<VoiceCommand, string> CommandRecognized;
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
            if (!PhraseRecognitionSystem.isSupported) throw new InvalidOperationException("Speech recognition is not supported on this system.");
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
        if (Phrases.TryGetValue(args.text, out VoiceCommand command))
            CommandRecognized?.Invoke(command, args.text);
    }

    // Lets tests and the keyboard-free validation drive the same path as a heard phrase.
    public void Simulate(string phrase)
    {
        if (Phrases.TryGetValue(phrase, out VoiceCommand command)) CommandRecognized?.Invoke(command, phrase);
    }

    private void OnDestroy()
    {
        StopListening();
    }
    private void OnDisable() { StopListening(); }
}

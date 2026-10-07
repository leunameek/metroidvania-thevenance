using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

// Listens for the words of VoiceVocabulary (Spanish and English) through Windows speech.
public class VoiceCommandRecognizer : MonoBehaviour
{
    private static Dictionary<string, VoiceCommand> Phrases => VoiceVocabulary.Phrases;

    public static IEnumerable<string> PhrasesFor(VoiceCommand command) => VoiceVocabulary.PhrasesFor(command);

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
        if (VoiceVocabulary.TryParse(phrase, out VoiceCommand command)) CommandRecognized?.Invoke(command, phrase);
    }

    private void OnDestroy()
    {
        StopListening();
    }
    private void OnDisable() { StopListening(); }
}

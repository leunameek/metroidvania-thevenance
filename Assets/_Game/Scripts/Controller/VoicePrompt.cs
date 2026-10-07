using UnityEngine;

// What the on-screen help names first: with the voice on, the word to say («examinar»); with
// it off or failing, the key. The plaza UI (VoiceUIController) and the worlds
// (WorldNaturalInput) keep Enabled up to date; until then the stored setting decides.
public static class VoicePrompt
{
    private static bool? _enabled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _enabled = null;

    public static bool Enabled
    {
        get { if (_enabled == null) _enabled = NaturalInputPrefs.Load().voiceEnabled; return _enabled.Value; }
        set => _enabled = value;
    }

    public static string Word(string word) => "«" + word + "»";

    // Key cap content: the word when the voice listens, the key otherwise.
    public static string Cap(string word, string key) => Enabled ? Word(word) : key;

    // The interaction word a prompt starts with («Hablar con Bachué» → hablar), or «usar».
    public static string InteractWord(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            string first = prompt.Trim().Split(' ')[0].ToLowerInvariant();
            if (VoiceVocabulary.TryParse(first, out var command) && command == VoiceCommand.Interact) return first;
            if (first == "viajar" || first == "regresar" || first == "cruzar") return "entrar";
            if (first == "responder") return "hablar";
        }
        return "usar";
    }
}

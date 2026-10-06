using System;
using TMPro;
using UnityEngine;

namespace Nemequene.UI
{
    // Shows the key of an "Acción · Tecla" label inside a carved key cap (reference: interaction
    // prompt and combat actions). The label keeps the action; the cap keeps the key.
    public sealed class UIKeyHint : MonoBehaviour
    {
        private static readonly string[] Named = { "Esc", "Enter", "Espacio", "Tab", "Ctrl", "Shift", "Supr" };
        public TMP_Text label, capText;
        public GameObject cap;
        private string _shown;
        private static bool IsKey(string candidate)
        {
            candidate = candidate.Trim();
            return candidate.Length == 1 && char.IsLetterOrDigit(candidate[0]) || Array.IndexOf(Named, candidate) >= 0;
        }
        // Accepts both "E · Examinar" (HUD prompts) and "Atacar · E" (buttons).
        public static bool Split(string value, out string action, out string key)
        {
            action = value; key = null;
            if (string.IsNullOrEmpty(value) || value.IndexOf('\n') >= 0) return false;
            int first = value.IndexOf(" · ", StringComparison.Ordinal);
            if (first > 0 && IsKey(value.Substring(0, first)))
            {
                string head = value.Substring(0, first).Trim();
                key = head.Length == 1 ? head.ToUpperInvariant() : head;
                action = value.Substring(first + 3).TrimStart(); return true;
            }
            int last = value.LastIndexOf(" · ", StringComparison.Ordinal);
            if (last <= 0 || !IsKey(value.Substring(last + 3))) return false;
            string candidate = value.Substring(last + 3).Trim();
            action = value.Substring(0, last).TrimEnd();
            key = candidate.Length == 1 ? candidate.ToUpperInvariant() : candidate;
            return true;
        }
        private void LateUpdate()
        {
            if (label == null || cap == null) return;
            string text = label.text;
            if (text == _shown) return;
            if (Split(text, out var action, out var key))
            {
                label.text = action; capText.text = key;
                if (!cap.activeSelf) cap.SetActive(true);
            }
            else if (cap.activeSelf) cap.SetActive(false);
            _shown = label.text;
        }
    }
}

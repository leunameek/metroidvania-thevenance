using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nemequene.UI
{
    public static class UIStrings
    {
        [Serializable] public sealed class Entry { public string key, value; }
        [Serializable] public sealed class Catalogue { public Entry[] entries; }
        private static Dictionary<string, string> _strings;
        public static string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return "";
            if (_strings == null)
            {
                _strings = new Dictionary<string, string>();
                var asset = Resources.Load<TextAsset>("Nemequene/es");
                if (asset != null)
                    foreach (var entry in JsonUtility.FromJson<Catalogue>(asset.text).entries) _strings[entry.key] = entry.value;
            }
            string value = _strings.TryGetValue(key, out var translated) ? translated : "[" + key + "]";
            return Keys(args.Length == 0 ? value : string.Format(value, args));
        }
        // "[[Interact]]" is the key the player gave that action on the Controles page; "[[Move]]"
        // the four movement keys ("WASD" by default).
        public static string Keys(string value)
        {
            int start = value.IndexOf("[[", StringComparison.Ordinal);
            while (start >= 0)
            {
                int end = value.IndexOf("]]", start + 2, StringComparison.Ordinal);
                if (end < 0) break;
                string name = value.Substring(start + 2, end - start - 2), key = null;
                if (name == "Move")
                {
                    string w = GameBindings.Cap(GameAction.MoveForward), a = GameBindings.Cap(GameAction.MoveLeft),
                        s = GameBindings.Cap(GameAction.MoveBack), d = GameBindings.Cap(GameAction.MoveRight);
                    key = w.Length == 1 && a.Length == 1 && s.Length == 1 && d.Length == 1 ? w + a + s + d : w + " / " + a + " / " + s + " / " + d;
                }
                else if (Enum.TryParse(name, out GameAction action)) key = GameBindings.Cap(action);
                if (key == null) { start = value.IndexOf("[[", end, StringComparison.Ordinal); continue; }
                value = value.Substring(0, start) + key + value.Substring(end + 2);
                start = value.IndexOf("[[", start + key.Length, StringComparison.Ordinal);
            }
            return value;
        }
    }
}

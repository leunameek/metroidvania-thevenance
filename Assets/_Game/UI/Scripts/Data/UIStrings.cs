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
            return args.Length == 0 ? value : string.Format(value, args);
        }
    }
}

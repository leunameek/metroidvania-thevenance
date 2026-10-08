using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // «Créditos» as a centred roll: team, audio, QA, tools, third-party assets and licences,
    // cultural note, special thanks and the legal line. The content lives in
    // Resources/Nemequene/credits.json, so names and roles are edited without touching code; the
    // entries still «Por confirmar» read in a muted tone. The roll scrolls by itself (unless
    // reduced motion is on) and stops as soon as the player scrolls or presses a key.
    public sealed class CreditsPage
    {
        [Serializable] private sealed class Entry { public string role; public string[] names; }
        [Serializable] private sealed class Section { public string title, text; public Entry[] entries; }
        [Serializable] private sealed class Data { public string game, tagline, placeholder; public Section[] sections; public string[] legal; }

        private readonly UIFactory _f;
        private readonly SettingsManager _settings;
        private ScrollRect _scroll;
        private float _startAt;
        private bool _auto;

        public CreditsPage(UIFactory factory, SettingsManager settings) { _f = factory; _settings = settings; }

        public void Build(RectTransform group)
        {
            var ticker = group.gameObject.AddComponent<UIPageTicker>();
            ticker.shown = () => { _scroll = group.GetComponentInParent<ScrollRect>(true); _startAt = Time.unscaledTime + 2.5f; _auto = !_settings.Values.reducedMotion; };
            ticker.tick = Roll;
            var asset = Resources.Load<TextAsset>("Nemequene/credits");
            Data data = null;
            try { if (asset != null) data = JsonUtility.FromJson<Data>(asset.text); } catch (Exception) { data = null; }
            if (data == null) { _f.Text(group, UIStrings.Get("credits.body"), 24); return; }
            string placeholder = string.IsNullOrEmpty(data.placeholder) ? "Por confirmar" : data.placeholder;

            Spacer(group, 40);
            var title = _f.Display(_f.Text(group, data.game, 54, true), UITone.Gold); title.alignment = TextAlignmentOptions.Center;
            title.enableAutoSizing = false;
            Line(group, data.tagline, 24, UITone.Muted, FontStyles.Italic);
            foreach (var section in data.sections ?? new Section[0])
            {
                Spacer(group, 26); _f.Rule(group);
                var heading = _f.Caption(group, section.title, 32); heading.alignment = TextAlignmentOptions.Center;
                Spacer(group, 6);
                foreach (var entry in section.entries ?? new Entry[0])
                {
                    if (!string.IsNullOrEmpty(entry.role)) Line(group, entry.role.ToUpperInvariant(), 18, UITone.Gold, FontStyles.Normal, 2);
                    foreach (var name in entry.names ?? new string[0])
                    {
                        bool pending = name.StartsWith(placeholder, StringComparison.OrdinalIgnoreCase);
                        Line(group, name, 25, pending ? UITone.Muted : UITone.Default, pending ? FontStyles.Italic : FontStyles.Normal);
                    }
                    Spacer(group, 12);
                }
                if (!string.IsNullOrEmpty(section.text)) Line(group, section.text, 22, UITone.Muted, FontStyles.Normal);
            }
            Spacer(group, 30); _f.Rule(group);
            foreach (var line in data.legal ?? new string[0])
            {
                bool pending = line.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase) >= 0;
                Line(group, line, 19, UITone.Muted, pending ? FontStyles.Italic : FontStyles.Normal);
            }
            Spacer(group, 60);
        }

        private TMP_Text Line(Transform parent, string value, float size, UITone tone, FontStyles style, float spacing = 0)
        {
            var text = UIFactory.Tone(_f.Text(parent, value, size), tone);
            text.alignment = TextAlignmentOptions.Center; text.fontStyle = style; text.characterSpacing = spacing;
            text.margin = new Vector4(60, 0, 60, 0);
            return text;
        }

        private static void Spacer(Transform parent, float height)
        {
            var space = new GameObject("Espacio", typeof(RectTransform), typeof(LayoutElement));
            space.transform.SetParent(parent, false);
            space.GetComponent<LayoutElement>().minHeight = height;
        }

        private void Roll()
        {
            if (!_auto || _scroll == null || Time.unscaledTime < _startAt) return;
            var keyboard = Keyboard.current; var mouse = Mouse.current;
            bool touched = keyboard != null && keyboard.anyKey.wasPressedThisFrame
                || mouse != null && (mouse.scroll.ReadValue().sqrMagnitude > 0 || mouse.leftButton.wasPressedThisFrame);
            if (touched) { _auto = false; return; }
            float range = _scroll.content.rect.height - _scroll.viewport.rect.height;
            if (range <= 1) return;
            _scroll.verticalNormalizedPosition = Mathf.Max(0, _scroll.verticalNormalizedPosition - 38f * Time.unscaledDeltaTime / range);
        }
    }
}

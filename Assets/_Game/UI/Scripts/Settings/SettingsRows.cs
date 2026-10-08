using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Rows shared by the settings pages of the title screen and of the pause: the same factory
    // controls, plus an explanation for the description panel and values that may change while
    // the page is open (devices, resolutions, presets that turn into «Personalizado»).
    public class SettingsRows
    {
        protected readonly UIFactory F;
        protected readonly SettingsManager Settings;
        protected UISettings V => Settings.Values;
        protected Action<string> Describe;

        public SettingsRows(UIFactory factory, SettingsManager settings) { F = factory; Settings = settings; }

        protected void Explain(Component control, string descriptionKey)
        {
            if (string.IsNullOrEmpty(descriptionKey)) return;
            var described = control.gameObject.AddComponent<UIDescribed>();
            described.text = UIStrings.Get(descriptionKey); described.sink = text => Describe?.Invoke(text);
        }

        protected void Commit() { Settings.Apply(); }

        public TMP_Text Section(Transform parent, string key)
        {
            var caption = F.Caption(parent, UIStrings.Get(key), 26);
            caption.alignment = TextAlignmentOptions.BottomLeft;
            caption.gameObject.AddComponent<LayoutElement>().minHeight = 52;
            return caption;
        }

        public TMP_Text Note(Transform parent, string text, float size = 20)
        {
            var note = UIFactory.Tone(F.Text(parent, text, size), UITone.Muted);
            var layout = note.gameObject.AddComponent<LayoutElement>(); layout.minHeight = 28;
            // Same inset as the labels of the rows.
            note.margin = new Vector4(76, 0, 40, 0);
            return note;
        }

        public Button Toggle(Transform parent, string key, string descriptionKey, Func<bool> get, Action<bool> set)
        {
            var button = F.Toggle(parent, key, get, v => { set(v); Commit(); }, Settings);
            Explain(button, descriptionKey); return button;
        }

        // A value that cycles with Enter or a click. The list itself may be rebuilt (devices).
        public Button Choice(Transform parent, string key, string descriptionKey, Func<string[]> values, Func<int> get, Action<int> set)
        {
            TMP_Text value = null;
            Action refresh = () =>
            {
                var options = values();
                value.text = options.Length == 0 ? "—" : options[Mathf.Clamp(get(), 0, options.Length - 1)];
            };
            var button = F.Button(parent, UIStrings.Get(key), () =>
            {
                int count = values().Length; if (count == 0) return;
                set((get() + 1) % count); Commit(); refresh();
            });
            value = F.Value(button, ""); refresh();
            button.gameObject.AddComponent<UIValueBinding>().Bind(Settings, refresh);
            Explain(button, descriptionKey); return button;
        }

        public Button Choice(Transform parent, string key, string descriptionKey, string[] values, Func<int> get, Action<int> set)
            => Choice(parent, key, descriptionKey, () => values, get, set);

        public Slider Slider(Transform parent, string key, string descriptionKey, float min, float max, Func<float> get, Action<float> set, Func<float, string> format = null)
        {
            format ??= v => UIFactory.Format(v, min, max);
            var slider = F.Slider(parent, key, min, max, get(), v => { set(v); Commit(); });
            slider.onValueChanged.AddListener(v => UIFactory.SetValue(slider.transform.parent, format(v)));
            UIFactory.SetValue(slider.transform.parent, format(get()));
            slider.gameObject.AddComponent<UIValueBinding>().Bind(Settings, () =>
            { slider.SetValueWithoutNotify(get()); UIFactory.SetValue(slider.transform.parent, format(get())); });
            Explain(slider, descriptionKey); return slider;
        }

        // A row that names an option the engine cannot offer, so the player knows why.
        public Button Unavailable(Transform parent, string key, string descriptionKey, string value)
        {
            var button = F.Button(parent, UIStrings.Get(key), null);
            F.Value(button, value);
            button.interactable = false; button.GetComponent<TitleMenuButton>().Refresh();
            // Still reachable with the pointer for its explanation.
            Explain(button, descriptionKey); return button;
        }

        public Button Act(Transform parent, string key, Action action, string descriptionKey = null, bool primary = false)
        {
            var button = F.Button(parent, UIStrings.Get(key), () => action?.Invoke(), primary);
            Explain(button, descriptionKey); return button;
        }

        public static void SetEnabled(Selectable control, bool enabled)
        {
            if (control == null) return;
            control.interactable = enabled;
            var style = control.GetComponent<TitleMenuButton>(); if (style != null) style.Refresh();
        }
    }
}

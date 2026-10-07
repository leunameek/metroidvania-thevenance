using System;
using UnityEngine;

namespace Nemequene.UI
{
    public sealed class SettingsUIController
    {
        private readonly UIManager _ui;
        private UISettings S => _ui.Settings.Values;
        public SettingsUIController(UIManager ui, MenuController menu)
        {
            _ui = ui; var f = ui.Factory;
            var body = menu.Page(UIScreen.Settings, UIStrings.Get("settings"));
            var tabs = menu.SectionNavigation(UIScreen.Settings);
            var content = f.Column(body, "SettingsContent", 16);
            var groups = new RectTransform[7];
            var tabStyles = new TitleMenuButton[7];
            string[] names = { "settings.gameplay", "settings.audio", "settings.graphics", "controls", "voice", "hands", "accessibility" };
            Action<int> select = index =>
            {
                for (int j = 0; j < groups.Length; j++)
                { groups[j].gameObject.SetActive(index == j); tabStyles[j].tabSelected = index == j; tabStyles[j].Refresh(); }
                body.GetComponentInParent<UnityEngine.UI.ScrollRect>(true).verticalNormalizedPosition = 1;
            };
            for (int i = 0; i < groups.Length; i++)
            {
                int index = i; groups[i] = f.Column(content, names[i]);
                var tab = UIFactory.Tab(f.Button(tabs, UIStrings.Get(names[i]), () => select(index)));
                tabStyles[i] = tab.GetComponent<TitleMenuButton>();
            }
            tabStyles[6].transform.SetAsFirstSibling();
            Toggle(groups[0], "settings.objectives", () => S.showObjectives, v => S.showObjectives = v);
            Toggle(groups[0], "settings.tutorials", () => S.tutorials, v => S.tutorials = v);
            Slider(groups[0], "settings.notices", 3, 15, () => S.noticeSeconds, v => S.noticeSeconds = v);
            f.Button(groups[0], UIStrings.Get("restart"), () => ui.Confirm("confirm.restart", () => ui.Loading.Restart()));
            menu.Link(groups[0], "credits", UIScreen.Credits);
            Slider(groups[1], "audio.master", 0, 1, () => S.master, v => S.master = v);
            Slider(groups[1], "audio.music", 0, 1, () => S.music, v => S.music = v);
            Slider(groups[1], "audio.effects", 0, 1, () => S.effects, v => S.effects = v);
            Slider(groups[1], "audio.ambience", 0, 1, () => S.ambience, v => S.ambience = v);
            Slider(groups[1], "audio.voices", 0, 1, () => S.voices, v => S.voices = v);
            Slider(groups[1], "audio.ui", 0, 1, () => S.uiVolume, v => S.uiVolume = v);
            int[] widths = {1280,1920,2560,1920}; int[] heights = {720,1080,1440,1200}; int resolution = 1;
            f.Choice(groups[2], "graphics.resolution", new[] {"1280 × 720", "1920 × 1080", "2560 × 1440", "1920 × 1200"}, () => resolution, v => resolution = v);
            bool fullscreen = Screen.fullScreen;
            f.Toggle(groups[2], "graphics.fullscreen", () => fullscreen, v => fullscreen = v);
            f.Button(groups[2], UIStrings.Get("graphics.apply"), () => _ui.Loading.PreviewResolution(widths[resolution], heights[resolution], fullscreen));
            f.Choice(groups[2], "graphics.quality", QualitySettings.names, () => Mathf.Max(0, S.quality), v => { S.quality = v; Apply(); });
            Toggle(groups[2], "graphics.vsync", () => S.vSync, v => S.vSync = v);
            Toggle(groups[2], "graphics.shadows", () => S.shadows, v => S.shadows = v);
            f.Choice(groups[2], "graphics.aa", new[] { "1×", "2×", "4×" }, () => S.aa == 4 ? 2 : S.aa == 2 ? 1 : 0, v => { S.aa = v == 2 ? 4 : v == 1 ? 2 : 1; Apply(); });
            int[] fps = {30,60,120,-1}; f.Choice(groups[2], "graphics.fps", new[] {"30", "60", "120", UIStrings.Get("unlimited")}, () => Math.Max(0, Array.IndexOf(fps, S.frameLimit)), v => { S.frameLimit = fps[v]; Apply(); });
            f.Text(groups[2], UIStrings.Get("graphics.limitations"), 20);
            f.Text(groups[3], UIStrings.Get("controls.body"));
            f.Button(groups[3], UIStrings.Get("tutorial.repeat"), ui.RepeatTutorial);
            Toggle(groups[4], "voice.enable", () => S.voiceEnabled, v => S.voiceEnabled = v);
            Toggle(groups[4], "voice.ptt", () => S.pushToTalk, v => S.pushToTalk = v);
            Toggle(groups[4], "voice.toggle", () => S.toggleTalk, v => S.toggleTalk = v);
            f.Choice(groups[4], "voice.confidence", new[] {UIStrings.Get("low"), UIStrings.Get("medium"), UIStrings.Get("high")}, () => S.confidence, v => { S.confidence = v; Apply(); });
            Slider(groups[4], "voice.gain", .5f, 4, () => S.inputGain, v => S.inputGain = v);
            Slider(groups[4], "access.reaction", 1, 3, () => S.reactionScale, v => S.reactionScale = v);
            menu.Link(groups[4], "voice.calibrate", UIScreen.VoiceCalibration);
            Slider(groups[5], "hands.sensitivity", .5f, 2, () => S.handSensitivity, v => S.handSensitivity = v);
            Slider(groups[5], "hands.smoothing", .02f, .4f, () => S.handSmoothing, v => S.handSmoothing = v);
            Slider(groups[5], "hands.dwell", .5f, 3, () => S.dwellSeconds, v => S.dwellSeconds = v);
            Slider(groups[5], "hands.deadzone", 0, .05f, () => S.handDeadZone, v => S.handDeadZone = v);
            Toggle(groups[5], "hands.left", () => S.leftHand, v => S.leftHand = v);
            Toggle(groups[5], "hands.cursor", () => S.handCursor, v => S.handCursor = v);
            Toggle(groups[5], "hands.guide", () => S.handGuide, v => S.handGuide = v);
            menu.Link(groups[5], "hands.calibrate", UIScreen.HandCalibration);
            var access = groups[6];
            f.Choice(access, "access.text", new[] {"100 %", "125 %", "150 %"}, () => Mathf.RoundToInt((S.textScale - 1) * 4), v => { S.textScale = 1 + v * .25f; Apply(); });
            Toggle(access, "access.readableFont", () => S.readableFont, v => S.readableFont = v);
            Toggle(access, "access.contrast", () => S.highContrast, v => S.highContrast = v);
            Toggle(access, "access.motion", () => S.reducedMotion, v => S.reducedMotion = v);
            Slider(access, "access.camera", 0, 1, () => S.cameraMotion, v => S.cameraMotion = v);
            Slider(access, "access.flash", 0, 1, () => S.flashIntensity, v => S.flashIntensity = v);
            Toggle(access, "access.subtitles", () => S.subtitles, v => S.subtitles = v);
            Slider(access, "access.subtitleSize", 1, 1.5f, () => S.subtitleScale, v => S.subtitleScale = v);
            Slider(access, "access.subtitleOpacity", .35f, 1, () => S.subtitleOpacity, v => S.subtitleOpacity = v);
            Toggle(access, "access.speakers", () => S.speakerNames, v => S.speakerNames = v);
            Toggle(access, "access.captions", () => S.soundCaptions, v => S.soundCaptions = v);
            Slider(access, "access.reaction", 1, 3, () => S.reactionScale, v => S.reactionScale = v);
            Toggle(access, "settings.objectives", () => S.showObjectives, v => S.showObjectives = v);
            Slider(access, "hands.dwell", .5f, 3, () => S.dwellSeconds, v => S.dwellSeconds = v);
            f.Button(access, UIStrings.Get("access.reset"), () => ui.Confirm("confirm.access", ui.Settings.ResetAccessibility));
            ui.Screens.Register(UIScreen.Accessibility, body.GetComponentInParent<TitleMenuBackdrop>(true).gameObject);
            ui.Screens.Changed += screen => { if (screen == UIScreen.Accessibility) select(6); };
            select(6);
        }
        private void Apply() { _ui.Settings.Apply(); }
        private void Toggle(Transform p, string key, Func<bool> get, Action<bool> set) { _ui.Factory.Toggle(p, key, get, v => { set(v); Apply(); }); }
        private void Slider(Transform p, string key, float min, float max, Func<float> get, Action<float> set)
        {
            var slider = _ui.Factory.Slider(p, key, min, max, get(), v => { set(v); Apply(); });
            slider.gameObject.AddComponent<UIValueBinding>().Bind(_ui.Settings, () =>
            { slider.SetValueWithoutNotify(get()); UIFactory.SetValue(slider.transform.parent, UIFactory.Format(get(), min, max)); });
        }
    }
}

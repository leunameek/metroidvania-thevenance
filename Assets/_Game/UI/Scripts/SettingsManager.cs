using System;
using UnityEngine;

namespace Nemequene.UI
{
    [Serializable]
    public sealed class UISettings
    {
        public int version = 1;
        public bool configured, highContrast, reducedMotion, readableFont, invertY, voiceEnabled = true, pushToTalk;
        public bool showObjectives = true, tutorials = true, subtitles = true, speakerNames = true, soundCaptions = true;
        public bool handCursor = true, handGuide = true, leftHand, toggleTalk;
        public bool dialogueAuto, dialogueInstant = true;
        public float dialogueSpeed = 30;
        public float menuMusic = .3f;
        // 2026-10-06 audio revision: one music slider for the whole game and one for voices
        // (creatures and characters); menuMusic stays only to migrate older saves.
        public float music = .6f, voices = .8f;
        public float textScale = 1, subtitleScale = 1, subtitleOpacity = .9f, master = .8f, effects = .7f, ambience = .35f, uiVolume = .3f;
        public float cameraSensitivity = 1, cameraMotion = 1, flashIntensity = .3f, noticeSeconds = 6;
        public float reactionScale = 1, handSensitivity = 1, handSmoothing = .12f, dwellSeconds = 1, handDeadZone = .015f, inputGain = 1;
        public int confidence = 1, quality = -1, frameLimit = 60, aa = 2;
        // 1 once the voice-first defaults (voice on, always listening) were applied.
        public int voiceDefaults = 1;
        public int screenWidth, screenHeight;
        public bool fullscreen;
        public bool vSync = true, shadows = true;
        public string microphone = "", camera = "";
        // Graphics page (2026-10-07). GraphicsRuntime applies every field in every scene; the
        // defaults are the «Alto» preset. displayMode: 0 exclusive full screen, 1 borderless, 2 window.
        public int displayMode = 1;
        public int refreshNumerator, refreshDenominator = 1; // 0 = the monitor's own rate
        public float renderScale = 1;
        public int upscaler; // 0 bilinear, 1 FSR 1.0, 2 STP
        public int shadowQuality = 3, reflections = 1, textureQuality = 2, anisotropic = 3, viewDistance = 2;
        public int antialiasing = 2; // 0 off, 1 FXAA, 2 SMAA, 3 TAA, 4-6 MSAA 2x/4x/8x
        public bool ambientOcclusion = true, bloom = true, motionBlur, depthOfField, filmGrain, lowLatency, hdr;
        public float paperWhite = 200, brightness, gamma;
        public void Clamp()
        {
            textScale = Mathf.Clamp(textScale, 1, 1.5f); subtitleScale = Mathf.Clamp(subtitleScale, 1, 1.5f);
            master = Mathf.Clamp01(master); music = Mathf.Clamp01(music); voices = Mathf.Clamp01(voices); effects = Mathf.Clamp01(effects); ambience = Mathf.Clamp01(ambience); uiVolume = Mathf.Clamp01(uiVolume);
            subtitleOpacity = Mathf.Clamp(subtitleOpacity, .35f, 1); reactionScale = Mathf.Clamp(reactionScale, 1, 3);
            cameraSensitivity = Mathf.Clamp(cameraSensitivity, .25f, 2); cameraMotion = Mathf.Clamp01(cameraMotion);
            flashIntensity = Mathf.Clamp01(flashIntensity); handSensitivity = Mathf.Clamp(handSensitivity, .5f, 2);
            handSmoothing = Mathf.Clamp(handSmoothing, .02f, .4f); dwellSeconds = Mathf.Clamp(dwellSeconds, .5f, 3);
            handDeadZone = Mathf.Clamp(handDeadZone, 0, .05f); inputGain = Mathf.Clamp(inputGain, .5f, 4);
            noticeSeconds = Mathf.Clamp(noticeSeconds, 3, 15); confidence = Mathf.Clamp(confidence, 0, 2);
            dialogueSpeed = Mathf.Clamp(dialogueSpeed, 15, 60);
            menuMusic = Mathf.Clamp01(menuMusic);
            displayMode = Mathf.Clamp(displayMode, 0, 2); renderScale = Mathf.Clamp(renderScale, .5f, 1); upscaler = Mathf.Clamp(upscaler, 0, 2);
            shadowQuality = Mathf.Clamp(shadowQuality, 0, 4); reflections = Mathf.Clamp(reflections, 0, 2);
            textureQuality = Mathf.Clamp(textureQuality, 0, 2); anisotropic = Mathf.Clamp(anisotropic, 0, 4);
            viewDistance = Mathf.Clamp(viewDistance, 0, 3); antialiasing = Mathf.Clamp(antialiasing, 0, 6);
            paperWhite = Mathf.Clamp(paperWhite, 80, 400); brightness = Mathf.Clamp(brightness, -.5f, .5f); gamma = Mathf.Clamp(gamma, -.5f, .5f);
            if (frameLimit == 0 || frameLimit < -1) frameLimit = -1;
            if (refreshDenominator == 0) refreshDenominator = 1;
        }
    }

    public sealed class SettingsManager
    {
        public const string StorageKey = "Nemequene.UI.Settings.v1";
        public UISettings Values { get; private set; }
        public event Action Changed;
        public SettingsManager()
        {
            string json=PlayerPrefs.GetString(StorageKey, "");
            try { Values = JsonUtility.FromJson<UISettings>(json); }
            catch (Exception) { Values = null; }
            if (Values == null || Values.version != 1) Values = new UISettings();
            else if (!json.Contains("\"menuMusic\"")) Values.menuMusic=.3f;
            if (Values != null && !string.IsNullOrEmpty(json) && !json.Contains("\"music\"")) Values.music = Mathf.Clamp01(Values.menuMusic * 2f);
            if (Values != null && !string.IsNullOrEmpty(json) && !json.Contains("\"voices\"")) Values.voices = .8f;
            // 2026-10-06: the game is voice-first. Older settings switch the voice on once and
            // listen without holding Ctrl; the player can change both again in Ajustes.
            if (!string.IsNullOrEmpty(json) && !json.Contains("\"voiceDefaults\""))
            {
                Values.voiceEnabled = true; Values.pushToTalk = false; Values.voiceDefaults = 1;
                PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(Values));
            }
            // Settings saved before the graphics page keep their window choice, MSAA and shadows.
            if (!string.IsNullOrEmpty(json) && !json.Contains("\"displayMode\""))
            {
                Values.displayMode = Values.fullscreen || Values.screenWidth == 0 ? 1 : 2;
                Values.antialiasing = Values.aa == 4 ? 5 : Values.aa == 2 ? 4 : 0;
                Values.shadowQuality = Values.shadows ? 3 : 0;
            }
            Values.Clamp();
        }
        public static FullScreenMode ScreenMode(int displayMode) =>
            displayMode == 0 ? FullScreenMode.ExclusiveFullScreen : displayMode == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        public void Apply(bool save = true)
        {
            Values.Clamp();
            AudioListener.volume = Values.master;
            GameAudio.SetMix(new AudioMixSettings
            {
                master = Values.master, effects = Values.effects, ambience = Values.ambience, uiVolume = Values.uiVolume,
                music = Values.music, voices = Values.voices, menuMusic = Values.menuMusic, soundCaptions = Values.soundCaptions,
            });
            GraphicsRuntime.Apply(Values);
            if (save) PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(Values));
            Changed?.Invoke();
        }
        public void Flush() { PlayerPrefs.Save(); }
        public void ResetAccessibility()
        {
            var d = new UISettings();
            Values.textScale = d.textScale; Values.highContrast = false; Values.reducedMotion = false; Values.readableFont = false;
            Values.subtitleScale = 1; Values.subtitleOpacity = .9f; Values.subtitles = Values.speakerNames = Values.soundCaptions = true;
            Values.reactionScale = 1; Values.cameraMotion = 1; Values.flashIntensity = .3f; Values.dwellSeconds = 1;
            Values.handSensitivity = 1; Values.toggleTalk = false; Apply();
        }
    }
}

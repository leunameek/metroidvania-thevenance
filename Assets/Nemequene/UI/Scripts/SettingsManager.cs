using System;
using UnityEngine;

namespace Nemequene.UI
{
    [Serializable]
    public sealed class UISettings
    {
        public int version = 1;
        public bool configured, highContrast, reducedMotion, readableFont, invertY, voiceEnabled, pushToTalk = true;
        public bool showObjectives = true, tutorials = true, subtitles = true, speakerNames = true, soundCaptions = true;
        public bool handCursor = true, handGuide = true, leftHand, toggleTalk;
        public bool dialogueAuto, dialogueInstant = true;
        public float dialogueSpeed = 30;
        public float menuMusic = .3f;
        public float textScale = 1, subtitleScale = 1, subtitleOpacity = .9f, master = .8f, effects = .7f, ambience = .35f, uiVolume = .3f;
        public float cameraSensitivity = 1, cameraMotion = 1, flashIntensity = .3f, noticeSeconds = 6;
        public float reactionScale = 1, handSensitivity = 1, handSmoothing = .12f, dwellSeconds = 1, handDeadZone = .015f, inputGain = 1;
        public int confidence = 1, quality = -1, frameLimit = 60, aa = 2;
        public int screenWidth, screenHeight;
        public bool fullscreen;
        public bool vSync = true, shadows = true;
        public string microphone = "", camera = "";
        public void Clamp()
        {
            textScale = Mathf.Clamp(textScale, 1, 1.5f); subtitleScale = Mathf.Clamp(subtitleScale, 1, 1.5f);
            master = Mathf.Clamp01(master); effects = Mathf.Clamp01(effects); ambience = Mathf.Clamp01(ambience); uiVolume = Mathf.Clamp01(uiVolume);
            subtitleOpacity = Mathf.Clamp(subtitleOpacity, .35f, 1); reactionScale = Mathf.Clamp(reactionScale, 1, 3);
            cameraSensitivity = Mathf.Clamp(cameraSensitivity, .25f, 2); cameraMotion = Mathf.Clamp01(cameraMotion);
            flashIntensity = Mathf.Clamp01(flashIntensity); handSensitivity = Mathf.Clamp(handSensitivity, .5f, 2);
            handSmoothing = Mathf.Clamp(handSmoothing, .02f, .4f); dwellSeconds = Mathf.Clamp(dwellSeconds, .5f, 3);
            handDeadZone = Mathf.Clamp(handDeadZone, 0, .05f); inputGain = Mathf.Clamp(inputGain, .5f, 4);
            noticeSeconds = Mathf.Clamp(noticeSeconds, 3, 15); confidence = Mathf.Clamp(confidence, 0, 2);
            dialogueSpeed = Mathf.Clamp(dialogueSpeed, 15, 60);
            menuMusic = Mathf.Clamp01(menuMusic);
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
            Values.Clamp();
        }
        public void Apply(bool save = true)
        {
            Values.Clamp();
            AudioListener.volume = Values.master;
            if (Values.quality >= 0) QualitySettings.SetQualityLevel(Mathf.Clamp(Values.quality, 0, QualitySettings.names.Length - 1));
            QualitySettings.vSyncCount = Values.vSync ? 1 : 0;
            Application.targetFrameRate = Values.frameLimit;
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

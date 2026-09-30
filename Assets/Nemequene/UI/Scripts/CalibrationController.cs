using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample;

namespace Nemequene.UI
{
    public sealed class CalibrationController
    {
        private readonly UIManager _ui;
        private readonly TMP_Text _micDevice, _micStatus, _noise, _handStatus, _cameraDevice, _step;
        private readonly Image _meter, _handProgress;
        private readonly RawImage _preview;
        private readonly GameObject _previewRoot;
        private readonly float[] _samples = new float[256];
        private AudioClip _recording;
        private string _recordingDevice;
        private float _noiseUntil, _noiseSum, _next;
        private int _noiseCount, _calibrationStep;
        private bool _showPreview, _ownedCamera;
        private Vector2 _handStart;
        public CalibrationController(UIManager ui, MenuController menu)
        {
            _ui = ui; var f = ui.Factory;
            var voice = menu.Page(UIScreen.VoiceCalibration, UIStrings.Get("voice.calibrate"));
            f.Text(voice, UIStrings.Get("voice.explain"), 22);
            _micDevice = f.Text(voice, "", 22);
            f.Button(voice, UIStrings.Get("device.next"), CycleMicrophone);
            f.Button(voice, UIStrings.Get("voice.measure"), StartMeter, true);
            var meterHost = f.Rect("InputMeter", voice, Vector2.zero, Vector2.one);
            meterHost.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            _meter = f.Bar(meterHost, "Meter", Vector2.zero, Vector2.one);
            _noise = f.Text(voice, "", 20); _micStatus = f.Text(voice, "", 24);
            f.Text(voice, UIStrings.Get("voice.say"), 32, true);
            f.Button(voice, UIStrings.Get("voice.test"), () => ui.Voice.Test());
            f.Toggle(voice, "voice.enable", () => ui.Settings.Values.voiceEnabled, v => { ui.Settings.Values.voiceEnabled = v; ui.Settings.Apply(); });
            f.Toggle(voice, "voice.ptt", () => ui.Settings.Values.pushToTalk, v => { ui.Settings.Values.pushToTalk = v; ui.Settings.Apply(); });
            f.Button(voice, UIStrings.Get("voice.next"), () => ui.Screens.Show(UIScreen.HandCalibration), true);
            f.Button(voice, UIStrings.Get("first.fallback"), menu.FinishSetup);
            var hands = menu.Page(UIScreen.HandCalibration, UIStrings.Get("hands.calibrate"));
            _cameraDevice = f.Text(hands, "", 22); f.Button(hands, UIStrings.Get("device.next"), CycleCamera);
            _handStatus = f.Text(hands, "", 22); _step = f.Text(hands, "", 28, true);
            f.Button(hands, UIStrings.Get("hands.start"), StartHands, true);
            f.Toggle(hands, "hands.preview", () => _showPreview, v => _showPreview = v);
            f.Toggle(hands, "hands.left", () => ui.Settings.Values.leftHand, v => { ui.Settings.Values.leftHand = v; ui.Settings.Apply(); });
            f.Slider(hands, "hands.sensitivity", .5f, 2, ui.Settings.Values.handSensitivity, v => { ui.Settings.Values.handSensitivity = v; ui.Settings.Apply(); });
            f.Slider(hands, "hands.dwell", .5f, 3, ui.Settings.Values.dwellSeconds, v => { ui.Settings.Values.dwellSeconds = v; ui.Settings.Apply(); });
            var hp = f.Rect("HandProgress", hands, Vector2.zero, Vector2.one); hp.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;
            _handProgress = f.Bar(hp, "Progress", Vector2.zero, Vector2.one);
            f.Button(hands, UIStrings.Get("calibration.finish"), () => { StopDevices(); if (!ui.Settings.Values.configured) menu.FinishSetup(); else ui.Screens.Back(); }, true);
            f.Button(hands, UIStrings.Get("hands.fallback"), () => { if (!ui.Demo.MouseMode) ui.Demo.ToggleInputMode(); StopDevices(); if (!ui.Settings.Values.configured) menu.FinishSetup(); else ui.Screens.Back(); });
            _previewRoot = f.Panel("UI_Camera_CalibrationOnly", ui.Root, new Vector2(.69f,.34f), new Vector2(.95f,.73f)).gameObject;
            var preview = f.Rect("Preview", _previewRoot.transform, new Vector2(.03f,.2f), new Vector2(.97f,.95f));
            _preview = preview.gameObject.AddComponent<RawImage>(); _preview.raycastTarget = false;
            f.Label(_previewRoot.transform, UIStrings.Get("hands.light"), new Vector2(.04f,.02f), new Vector2(.96f,.19f), 20);
            _previewRoot.SetActive(false); RefreshDevices();
            ui.Hands.Changed += HandChanged;
        }
        private void RefreshDevices()
        {
            _micDevice.text = UIStrings.Get("voice.device", string.IsNullOrEmpty(_ui.Settings.Values.microphone) ? UIStrings.Get("device.default") : _ui.Settings.Values.microphone);
            _cameraDevice.text = UIStrings.Get("hands.device", string.IsNullOrEmpty(_ui.Settings.Values.camera) ? UIStrings.Get("device.default") : _ui.Settings.Values.camera);
        }
        private void CycleMicrophone()
        {
            StopMeter(); var names = Microphone.devices;
            if (names.Length == 0) { _noise.text = UIStrings.Get("voice.state.NoSignal"); return; }
            int index = Array.IndexOf(names, _ui.Settings.Values.microphone);
            _ui.Settings.Values.microphone = names[(index + 1) % names.Length]; _ui.Settings.Apply(); RefreshDevices();
        }
        private void CycleCamera()
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0) { _handStatus.text = UIStrings.Get("hands.missing"); return; }
            int index = -1;
            for (int i = 0; i < devices.Length; i++) if (devices[i].name == _ui.Settings.Values.camera) index = i;
            string name = devices[(index + 1) % devices.Length].name;
            _ui.Settings.Values.camera = name; _ui.Demo.Hands.SelectCamera(name); _ui.Settings.Apply(); RefreshDevices();
        }
        private void StartMeter()
        {
            StopMeter();
            if (Application.isBatchMode || Microphone.devices.Length == 0) { _noise.text = UIStrings.Get("voice.state.NoSignal"); return; }
            _recordingDevice = string.IsNullOrEmpty(_ui.Settings.Values.microphone) ? null : _ui.Settings.Values.microphone;
            try
            {
                _recording = Microphone.Start(_recordingDevice, true, 1, 16000);
                if (_recording == null) { _noise.text = UIStrings.Get("voice.state.Unavailable"); return; }
                _noiseUntil = Time.unscaledTime + 3; _noiseCount = 0; _noiseSum = 0;
                _noise.text = UIStrings.Get("voice.quiet");
            }
            catch (Exception) { _noise.text = UIStrings.Get("voice.state.PermissionDenied"); }
        }
        private void StopMeter()
        {
            if (_recording != null) { Microphone.End(_recordingDevice); UnityEngine.Object.Destroy(_recording); _recording = null; }
        }
        private void StartHands()
        {
            _calibrationStep = 0; _ui.Demo.Hands.PreferredCamera = _ui.Settings.Values.camera;
            if (!_ui.Demo.Hands.Requested) { _ui.Demo.Hands.Toggle(); _ownedCamera = true; }
            if (_ui.Demo.MouseMode) _ui.Demo.ToggleInputMode();
        }
        private void HandChanged()
        {
            if (_ui.Screens.Current != UIScreen.HandCalibration) return;
            if (_calibrationStep == 0 && _ui.Hands.State == HandState.Detected) { _calibrationStep = 1; _handStart = _ui.Hands.Position; }
            if (_calibrationStep == 2 && _ui.Hands.State == HandState.Confirmed) _calibrationStep = 3;
        }
        public void OnScreen(UIScreen screen)
        {
            if (screen != UIScreen.VoiceCalibration) StopMeter();
            if (screen != UIScreen.HandCalibration)
            { if (_ownedCamera) { _ui.Hands.Stop(); _ownedCamera = false; } _previewRoot.SetActive(false); }
            RefreshDevices();
        }
        public void Tick()
        {
            var screen = _ui.Screens.Current;
            if (screen != UIScreen.VoiceCalibration && screen != UIScreen.HandCalibration) return;
            if (Time.unscaledTime < _next) return; _next = Time.unscaledTime + .1f;
            if (screen == UIScreen.VoiceCalibration)
            {
                _micStatus.text = _ui.Voice.StatusText;
                if (_recording != null)
                {
                    if (!Microphone.IsRecording(_recordingDevice)) { StopMeter(); _noise.text = UIStrings.Get("voice.disconnected"); return; }
                    int position = Microphone.GetPosition(_recordingDevice);
                    if (position >= _samples.Length && _recording.GetData(_samples, position - _samples.Length))
                    {
                        float sum = 0; foreach (float value in _samples) sum += value * value;
                        float level = Mathf.Sqrt(sum / _samples.Length); UIFactory.Fill(_meter, level * 8 * _ui.Settings.Values.inputGain);
                        if (Time.unscaledTime < _noiseUntil) { _noiseSum += level; _noiseCount++; }
                        else _noise.text = UIStrings.Get(_noiseCount > 0 && _noiseSum / _noiseCount > .06f ? "voice.noisy" : "voice.measured");
                    }
                }
            }
            else
            {
                if (_calibrationStep == 1 && Vector2.Distance(_ui.Hands.Position, _handStart) > .12f) _calibrationStep = 2;
                _step.text = UIStrings.Get("hands.step." + _calibrationStep);
                _handStatus.text = _ui.Hands.StatusText + "\n" + _ui.Demo.Hands.Status;
                UIFactory.Fill(_handProgress, _calibrationStep / 3f);
                var source = ImageSourceProvider.ImageSource;
                bool preview = _showPreview && _ui.Demo.Hands.Requested && source != null && source.isPrepared;
                _previewRoot.SetActive(preview);
                if (preview)
                {
                    _preview.texture = source.GetCurrentTexture();
                    _preview.uvRect = new Rect(source.isFrontFacing ? 1 : 0, source.isVerticallyFlipped ? 1 : 0, source.isFrontFacing ? -1 : 1, source.isVerticallyFlipped ? -1 : 1);
                }
            }
        }
        public void StopDevices() { StopMeter(); _ui.Voice.Suspend(); if (_ownedCamera) _ui.Hands.Stop(); _ownedCamera = false; _previewRoot.SetActive(false); }
        public void Dispose() { StopDevices(); _ui.Hands.Changed -= HandChanged; }
    }
}

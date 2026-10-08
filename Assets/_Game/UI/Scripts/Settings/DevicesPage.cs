using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // «Cámara y micrófono» in Accesibilidad: which camera the hand tracking opens and which
    // microphone the level meter and the calibration listen to, with a live preview and meter to
    // check them on this PC. Windows speech recognition always listens through the system's
    // default input, so the page says so and opens the Windows sound settings.
    public sealed class DevicesPage : SettingsRows
    {
        private readonly Action<string> _cameraChanged;
        private readonly Func<bool> _cameraBusy;
        private string[] _cameras = new string[0], _microphones = new string[0];
        private RawImage _preview;
        private AspectRatioFitter _previewAspect;
        private GameObject _previewBox;
        private WebCamTexture _webcam;
        private AudioClip _recording;
        private string _recordingDevice;
        private readonly float[] _samples = new float[512];
        private Image _level;
        private TMP_Text _cameraState, _microphoneState;
        private bool _previewOn, _meterOn;
        private float _shownLevel;

        // cameraChanged: switches a running hand-tracking session; cameraBusy: that session owns the camera.
        public DevicesPage(UIFactory factory, SettingsManager settings, Action<string> cameraChanged = null, Func<bool> cameraBusy = null)
            : base(factory, settings) { _cameraChanged = cameraChanged; _cameraBusy = cameraBusy; }

        public void Build(RectTransform group)
        {
            var host = F.Column(group, "Dispositivos", 16);
            var ticker = host.gameObject.AddComponent<UIPageTicker>();
            ticker.shown = Rescan; ticker.hidden = () => { StopPreview(); StopMeter(); }; ticker.tick = Tick;
            Section(host, "devices.section");
            Note(host, UIStrings.Get("devices.intro"));
            Choice(host, "devices.camera", "devices.camera.info", CameraLabels, CameraIndex, SelectCamera);
            Toggle(host, "devices.preview", "devices.preview.info", () => _previewOn, v => { _previewOn = v; RefreshPreview(); });
            var box = F.Rect("VistaPrevia", host, Vector2.zero, Vector2.one);
            box.gameObject.AddComponent<LayoutElement>().preferredHeight = 260;
            var frame = F.Rect("Imagen", box, Vector2.zero, Vector2.one); frame.offsetMin = new Vector2(76, 0); frame.offsetMax = new Vector2(-40, 0);
            var black = frame.gameObject.AddComponent<Image>(); black.color = Color.black; black.raycastTarget = false;
            var picture = F.Rect("Camara", frame, Vector2.zero, Vector2.one);
            _preview = picture.gameObject.AddComponent<RawImage>(); _preview.raycastTarget = false;
            _previewAspect = picture.gameObject.AddComponent<AspectRatioFitter>();
            _previewAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; _previewAspect.aspectRatio = 16f / 9f;
            _previewBox = box.gameObject; _previewBox.SetActive(false);
            _cameraState = Note(host, "");

            Choice(host, "devices.microphone", "devices.microphone.info", MicrophoneLabels, MicrophoneIndex, SelectMicrophone);
            Toggle(host, "devices.meter", "devices.meter.info", () => _meterOn, v => { _meterOn = v; if (v) StartMeter(); else StopMeter(); });
            var track = F.Rect("Nivel", host, Vector2.zero, Vector2.one);
            track.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;
            var rail = F.Rect("Barra", track, Vector2.zero, Vector2.one); rail.offsetMin = new Vector2(76, 0); rail.offsetMax = new Vector2(-40, 0);
            _level = F.Bar(rail, "NivelEntrada", Vector2.zero, Vector2.one, UIPalette.Jade); UIFactory.Fill(_level, 0);
            _microphoneState = Note(host, "");
            Note(host, UIStrings.Get("devices.windowsVoice"));
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                Act(host, "devices.openSound", () => Application.OpenURL("ms-settings:sound"), "devices.openSound.info");
            Act(host, "devices.rescan", Rescan, "devices.rescan.info");
        }

        private void Rescan()
        {
            try { var list = WebCamTexture.devices; _cameras = new string[list.Length]; for (int i = 0; i < list.Length; i++) _cameras[i] = list[i].name; }
            catch (Exception) { _cameras = new string[0]; }
            try { _microphones = Microphone.devices; } catch (Exception) { _microphones = new string[0]; }
            Settings.Apply(false); // refreshes the two choices
            _cameraState.text = _cameras.Length == 0 ? UIStrings.Get("devices.noCamera") : UIStrings.Get("devices.cameraCount", _cameras.Length);
            _microphoneState.text = _microphones.Length == 0 ? UIStrings.Get("devices.noMicrophone") : UIStrings.Get("devices.microphoneCount", _microphones.Length);
            if (_previewOn) RefreshPreview();
            if (_meterOn) StartMeter();
        }

        // «Predeterminada» first; a saved device that is not connected now stays listed as such.
        private string[] Labels(string[] devices, string saved)
        {
            bool missing = !string.IsNullOrEmpty(saved) && Array.IndexOf(devices, saved) < 0;
            var labels = new string[devices.Length + 1 + (missing ? 1 : 0)];
            labels[0] = UIStrings.Get("devices.default");
            for (int i = 0; i < devices.Length; i++) labels[i + 1] = devices[i];
            if (missing) labels[labels.Length - 1] = UIStrings.Get("devices.disconnected", saved);
            return labels;
        }
        private static int Index(string[] devices, string saved)
        {
            if (string.IsNullOrEmpty(saved)) return 0;
            int index = Array.IndexOf(devices, saved);
            return index >= 0 ? index + 1 : devices.Length + 1;
        }
        private string[] CameraLabels() => Labels(_cameras, V.camera);
        private int CameraIndex() => Index(_cameras, V.camera);
        private string[] MicrophoneLabels() => Labels(_microphones, V.microphone);
        private int MicrophoneIndex() => Index(_microphones, V.microphone);

        private void SelectCamera(int index)
        {
            // The «not connected» entry cycles back to the default.
            V.camera = index >= 1 && index <= _cameras.Length ? _cameras[index - 1] : "";
            HandTrackingSession.LastCamera = string.IsNullOrEmpty(V.camera) ? null : V.camera;
            _cameraChanged?.Invoke(V.camera);
            if (_previewOn) RefreshPreview();
        }

        private void SelectMicrophone(int index)
        {
            V.microphone = index >= 1 && index <= _microphones.Length ? _microphones[index - 1] : "";
            if (_meterOn) StartMeter();
        }

        private void RefreshPreview()
        {
            StopPreview();
            _previewBox.SetActive(_previewOn);
            if (!_previewOn) return;
            if (_cameras.Length == 0) { _cameraState.text = UIStrings.Get("devices.noCamera"); return; }
            if (_cameraBusy != null && _cameraBusy()) { _cameraState.text = UIStrings.Get("devices.cameraBusy"); return; }
            if (Application.isBatchMode) return;
            try
            {
                string device = string.IsNullOrEmpty(V.camera) || Array.IndexOf(_cameras, V.camera) < 0 ? _cameras[0] : V.camera;
                _webcam = new WebCamTexture(device, 640, 360, 30);
                _webcam.Play();
                _preview.texture = _webcam;
                _cameraState.text = UIStrings.Get("devices.previewing", device);
            }
            catch (Exception) { _cameraState.text = UIStrings.Get("devices.cameraError"); StopPreview(); }
        }

        private void StopPreview()
        {
            if (_webcam != null) { if (_webcam.isPlaying) _webcam.Stop(); UnityEngine.Object.Destroy(_webcam); _webcam = null; }
            if (_preview != null) _preview.texture = null;
        }

        private void StartMeter()
        {
            StopMeter();
            if (!_meterOn || Application.isBatchMode) return;
            if (_microphones.Length == 0) { _microphoneState.text = UIStrings.Get("devices.noMicrophone"); return; }
            _recordingDevice = string.IsNullOrEmpty(V.microphone) || Array.IndexOf(_microphones, V.microphone) < 0 ? null : V.microphone;
            try
            {
                _recording = Microphone.Start(_recordingDevice, true, 1, 16000);
                _microphoneState.text = _recording == null ? UIStrings.Get("devices.microphoneError") : UIStrings.Get("devices.speak");
            }
            catch (Exception) { _recording = null; _microphoneState.text = UIStrings.Get("devices.microphoneError"); }
        }

        private void StopMeter()
        {
            if (_recording != null) { Microphone.End(_recordingDevice); UnityEngine.Object.Destroy(_recording); _recording = null; }
            _shownLevel = 0; if (_level != null) UIFactory.Fill(_level, 0);
        }

        private void Tick()
        {
            if (_webcam != null && _webcam.width > 16) _previewAspect.aspectRatio = (float)_webcam.width / _webcam.height;
            if (_recording == null) return;
            int position = Microphone.GetPosition(_recordingDevice);
            float level = 0;
            if (position >= _samples.Length)
            {
                _recording.GetData(_samples, position - _samples.Length);
                float sum = 0; foreach (var s in _samples) sum += s * s;
                // RMS on a perceptual scale: -50 dBFS empty, 0 dBFS full.
                float db = 20 * Mathf.Log10(Mathf.Sqrt(sum / _samples.Length) + 1e-6f);
                level = Mathf.InverseLerp(-50, 0, db);
            }
            _shownLevel = Mathf.Max(level, Mathf.MoveTowards(_shownLevel, 0, Time.unscaledDeltaTime * 1.5f));
            UIFactory.Fill(_level, _shownLevel);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nemequene.UI
{
    public enum VoiceState { Inactive, Preparing, Listening, Processing, Recognized, Unrecognized, NoSignal, PermissionDenied, Unavailable }
    public sealed class VoiceUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly VoiceCommandRecognizer _recognizer;
        private float _until, _nextDeviceCheck;
        private bool _calibrating, _failed, _talkToggle, _disposed;
        private int _lastAcceptedFrame = -1;
        public VoiceState State { get; private set; }
        public string LastPhrase { get; private set; } = "";
        public bool Listening => _recognizer != null && _recognizer.IsListening;
        public bool HasError => State == VoiceState.Unavailable || State == VoiceState.PermissionDenied || State == VoiceState.NoSignal;
        public string StatusText => UIStrings.Get("voice.state." + State) + (State == VoiceState.Recognized ? " · " + LastPhrase.ToUpperInvariant() : "");
        public event Action Changed;
        public VoiceUIController(UIManager ui)
        {
            _ui = ui; _recognizer = ui.gameObject.AddComponent<VoiceCommandRecognizer>();
            _recognizer.CommandRecognized += Recognized; _recognizer.Unavailable += Unavailable;
            ui.Settings.Changed += SettingsChanged;
        }
        private void SettingsChanged() { Suspend(); }
        public void OnScreen(UIScreen screen) { if (screen != UIScreen.VoiceCalibration) { _calibrating = false; Suspend(); } }
        public void Test()
        {
            _failed = false; _calibrating = true; _until = Time.unscaledTime + 8 * _ui.Settings.Values.reactionScale;
            Begin();
        }
        private void Set(VoiceState state) { if (State == state) return; State = state; Changed?.Invoke(); }
        private void Begin()
        {
            if (Listening || _failed) return;
            if (Application.isBatchMode) { _failed = true; Set(VoiceState.Unavailable); return; }
            if (Microphone.devices.Length == 0) { _failed = true; Set(VoiceState.NoSignal); return; }
            _recognizer.MinimumConfidence = _ui.Settings.Values.confidence;
            _recognizer.StartListening();
            if (Listening) Set(VoiceState.Listening);
        }
        private void Unavailable(string error)
        {
            _failed = true;
            Set(error != null && (error.IndexOf("denied", StringComparison.OrdinalIgnoreCase) >= 0 || error.IndexOf("access", StringComparison.OrdinalIgnoreCase) >= 0)
                ? VoiceState.PermissionDenied : VoiceState.Unavailable);
        }
        private void Recognized(VoiceCommand command, string phrase)
        {
            if (_disposed || !Listening || _ui.ModalOpen || Time.frameCount == _lastAcceptedFrame) return;
            LastPhrase = phrase; Set(VoiceState.Processing);
            if (_calibrating)
            {
                _calibrating = false; _recognizer.StopListening();
                Set(command == VoiceCommand.Attack ? VoiceState.Recognized : VoiceState.Unrecognized);
                _until = Time.unscaledTime + 3; return;
            }
            var d = _ui.Demo;
            if (d.HelpOpen) { Set(VoiceState.Inactive); return; }
            if (d.State != TechnicalDemoState.Combat) { Explore(d, command); return; }
            var phase = d.Combat.Model.Phase;
            if (phase == PlazaCombatPhase.Won && command == VoiceCommand.Back) { Accept(); d.Combat.Cancel(); return; }
            bool allowed = command == VoiceCommand.Attack ? phase == PlazaCombatPhase.Attack : phase == PlazaCombatPhase.React;
            if (!allowed) { Set(VoiceState.Unrecognized); _until = Time.unscaledTime + 2; return; }
            if (command != VoiceCommand.Attack && command != VoiceCommand.Dodge && command != VoiceCommand.Guard) { Set(VoiceState.Unrecognized); _until = Time.unscaledTime + 2; return; }
            Accept();
            if (command == VoiceCommand.Attack) d.Combat.Attack();
            else d.Combat.Defend(command == VoiceCommand.Dodge ? PlazaDefense.Dodge : PlazaDefense.Guard);
        }
        // Outside the duel the words drive objects: «examinar» what is near (a station, a portal,
        // the training circle), «salir» or «tomar» in the inspection.
        private void Explore(TechnicalDemoController d, VoiceCommand command)
        {
            bool done = false;
            if (d.State == TechnicalDemoState.Exploration && command == VoiceCommand.Interact && HasTarget(d)) { Accept(); d.Interact(); done = true; }
            else if (d.State == TechnicalDemoState.Analyzing && command == VoiceCommand.Back) { Accept(); d.EndAnalysis(); done = true; }
            else if (d.State == TechnicalDemoState.Analyzing && command == VoiceCommand.Confirm && d.Lesson != null && d.Lesson.Complete) { Accept(); d.EndAnalysis(); done = true; }
            if (!done) { Set(VoiceState.Unrecognized); _until = Time.unscaledTime + 2; }
        }
        private static bool HasTarget(TechnicalDemoController d) => d.Nearby != null || d.NearbyPortal != null || d.NearCombat;
        private void Accept()
        {
            _lastAcceptedFrame = Time.frameCount;
            _recognizer.StopListening(); Set(VoiceState.Recognized); _until = Time.unscaledTime + 2;
            _ui.Sound(PlazaSound.Inspect);
        }
        public void Tick()
        {
            if (_disposed) return;
            if (Time.unscaledTime >= _nextDeviceCheck)
            {
                _nextDeviceCheck = Time.unscaledTime + 1;
                if (Listening && Microphone.devices.Length == 0) { Suspend(); _failed = true; Set(VoiceState.NoSignal); }
            }
            if (_ui.ModalOpen) { if (Listening) Suspend(); return; }
            if (_calibrating)
            {
                if (Time.unscaledTime >= _until) { _calibrating = false; _recognizer.StopListening(); Set(VoiceState.Unrecognized); }
                return;
            }
            var d = _ui.Demo; var s = _ui.Settings.Values;
            var phase = d.Combat.Model.Phase;
            bool free = !d.HelpOpen && _ui.Screens.Current == UIScreen.None;
            bool turn = free && (d.State == TechnicalDemoState.Combat
                    && (phase == PlazaCombatPhase.Attack || phase == PlazaCombatPhase.React || phase == PlazaCombatPhase.Won)
                || d.State == TechnicalDemoState.Exploration && HasTarget(d)
                || d.State == TechnicalDemoState.Analyzing);
            var k = Keyboard.current;
            if (k != null && k.leftCtrlKey.wasPressedThisFrame && turn) _talkToggle = !_talkToggle;
            bool talk = !s.pushToTalk || (s.toggleTalk ? _talkToggle : k != null && k.leftCtrlKey.isPressed);
            if (!turn || !s.voiceEnabled || !talk)
            {
                if (Listening) _recognizer.StopListening();
                if (!HasError && Time.unscaledTime >= _until) Set(turn && s.voiceEnabled ? VoiceState.Preparing : VoiceState.Inactive);
                return;
            }
            if (Time.unscaledTime >= _until || (State != VoiceState.Recognized && State != VoiceState.Unrecognized)) Begin();
        }
        public void Suspend()
        {
            if (_recognizer != null) _recognizer.StopListening(); _calibrating = false; _talkToggle = false; _failed = false;
            Set(VoiceState.Inactive);
        }
        public void Dispose()
        {
            _disposed = true; Suspend(); _ui.Settings.Changed -= SettingsChanged;
            if (_recognizer != null) { _recognizer.CommandRecognized -= Recognized; _recognizer.Unavailable -= Unavailable; }
        }
    }
}

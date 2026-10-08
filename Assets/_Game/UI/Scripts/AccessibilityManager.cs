using UnityEngine;

namespace Nemequene.UI
{
    public sealed class AccessibilityManager
    {
        private readonly UIManager _ui;
        public AccessibilityManager(UIManager ui) { _ui = ui; ui.Settings.Changed += Apply; }
        public void Apply()
        {
            foreach (var binding in _ui.GetComponentsInChildren<UIStyleBinding>(true)) binding.Apply(_ui.Theme, _ui.Settings.Values);
            foreach (var button in _ui.GetComponentsInChildren<TitleMenuButton>(true)) button.Refresh();
            foreach (var shade in _ui.GetComponentsInChildren<TitleMenuBackdrop>(true)) shade.SetContrast(_ui.Settings.Values.highContrast);
            var s = _ui.Settings.Values;
            _ui.Demo.Audio.SetVolumes(s.effects, s.ambience);
            _ui.Demo.ConfigurePresentation(s.cameraSensitivity, s.invertY, s.cameraMotion, s.handSensitivity, s.reducedMotion);
            _ui.Demo.Combat.Model.SetReactionScale(s.reactionScale);
            _ui.Demo.Combat.FlashIntensity = s.flashIntensity;
        }
        public void Dispose() { _ui.Settings.Changed -= Apply; }
    }
}

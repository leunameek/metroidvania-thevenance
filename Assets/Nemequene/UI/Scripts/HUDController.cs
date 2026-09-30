using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class HUDController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly Health _healthSource;
        private readonly GameObject _root, _healthPanel, _objectivePanel, _promptPanel, _zonePanel, _devicesPanel;
        private readonly TMP_Text _health, _objective, _zone, _prompt, _devices;
        private readonly Image _healthFill, _fade;
        private float _healthTarget = 1, _healthShown = 1, _next, _zoneUntil, _objectiveUntil;
        private int _world = int.MinValue;
        private string _previousObjective;
        private bool _dirty = true;
        public HUDController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            // Layout on the 1920 x 1080 reference: 5 % side and 4 % vertical safe margins, and no two
            // exploration surfaces share screen space (reference block 2, HUD).
            _root = f.Rect("UI_HUD_Exploration", ui.Root, Vector2.zero, Vector2.one).gameObject;
            var health = f.HudPanel("Vitality", _root.transform, new Vector2(.05f,.865f), new Vector2(.27f,.955f));
            _healthPanel = health.gameObject;
            var heart = f.Icon(health, UIIcon.Heart, new Vector2(0,.5f), new Vector2(0,.5f), UITheme.Hex("C8352C"));
            heart.rectTransform.sizeDelta = new Vector2(44,44); heart.rectTransform.anchoredPosition = new Vector2(46,0);
            Fit(f.Caption(health, UIStrings.Get("hud.health"), 20)).rectTransform.SetAnchors(new Vector2(.24f,.56f), new Vector2(.94f,.9f));
            _healthFill = f.Bar(health, "Life", new Vector2(.24f,.16f), new Vector2(.94f,.52f), UITheme.Hex("A3322A"));
            // Ivory on the red fill is 5.4:1; on the empty rail it is higher.
            _health = f.Label(_healthFill.transform.parent.parent, "", Vector2.zero, Vector2.one, 20);
            _health.alignment = TextAlignmentOptions.MidlineRight; _health.margin = new Vector4(0,0,12,0); Fit(_health);
            _objectivePanel = f.HudPanel("Objective", _root.transform, new Vector2(.05f,.80f), new Vector2(.34f,.955f)).gameObject;
            var mark = f.Icon(_objectivePanel.transform, UIIcon.Diamond, new Vector2(0,1), new Vector2(0,1), f.Theme.gold);
            mark.rectTransform.sizeDelta = new Vector2(30,30); mark.rectTransform.anchoredPosition = new Vector2(44,-40);
            f.Caption(_objectivePanel.transform, UIStrings.Get("hud.objectiveTitle"), 20).rectTransform.SetAnchors(new Vector2(.16f,.62f), new Vector2(.94f,.9f));
            _objective = f.Label(_objectivePanel.transform, "", new Vector2(.16f,.1f), new Vector2(.94f,.62f), 22);
            _zonePanel = f.HudPanel("ZoneBanner", _root.transform, new Vector2(.33f,.845f), new Vector2(.67f,.955f)).gameObject;
            _zone = f.Heading(_zonePanel.transform, "", new Vector2(.04f,.32f), new Vector2(.96f,.94f), 44);
            f.Divider(_zonePanel.transform, new Vector2(.22f,.1f), new Vector2(.78f,.3f));
            _promptPanel = f.HudPanel("UI_Indicator_Interaction", _root.transform, new Vector2(.33f,.05f), new Vector2(.67f,.15f)).gameObject;
            var promptRow = f.Row(_promptPanel.transform, "PromptRow", 20);
            promptRow.offsetMin = new Vector2(32,12); promptRow.offsetMax = new Vector2(-32,-12);
            var promptLayout = promptRow.GetComponent<HorizontalLayoutGroup>(); promptLayout.childForceExpandWidth = false;
            _prompt = f.Text(promptRow, "", 24); _prompt.alignment = TextAlignmentOptions.MidlineLeft;
            _prompt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            f.KeyHint(_prompt, promptRow, true);
            _devicesPanel = f.HudPanel("DeviceStatus", _root.transform, new Vector2(.74f,.895f), new Vector2(.95f,.955f)).gameObject;
            _devices = f.Label(_devicesPanel.transform, "", new Vector2(.06f,.1f), new Vector2(.94f,.9f), 18); _devices.alignment = TextAlignmentOptions.Center;
            var fade = f.Rect("PortalFade", _root.transform, Vector2.zero, Vector2.one);
            _fade = fade.gameObject.AddComponent<Image>(); _fade.raycastTarget = false; _fade.color = Color.clear;
            ui.Demo.ViewChanged += MarkDirty; ui.Demo.Objectives.Changed += MarkDirty;
            _healthSource=ui.Demo.PlayerHealth;
            _healthSource.HealthChanged += Health; ui.Screens.Changed += Screen; ui.Settings.Changed += MarkDirty;
            Health(_healthSource.CurrentHealth, _healthSource.MaxHealth);
        }
        // Fixed HUD boxes shrink their text at 150 % instead of wrapping or spilling.
        private static TMP_Text Fit(TMP_Text text)
        {
            text.textWrappingMode = TextWrappingModes.NoWrap; text.enableAutoSizing = true;
            text.fontSizeMax = text.fontSize; text.fontSizeMin = 14; return text;
        }
        private void MarkDirty() { _dirty = true; }
        private void Screen(UIScreen s) { MarkDirty(); }
        public void Refresh() { _dirty = true; _root.SetActive(_ui.Screens.Current == UIScreen.None); }
        private void Health(float value, float max)
        {
            _health.text = value.ToString("0") + " / " + max.ToString("0"); _healthTarget = max > 0 ? value / max : 0;
        }
        public void Tick()
        {
            _root.SetActive(_ui.Screens.Current == UIScreen.None && !_ui.ModalOpen && !(_ui.Dialogue?.Active ?? false));
            if (!_root.activeSelf) return;
            _healthShown = _ui.Settings.Values.reducedMotion ? _healthTarget : Mathf.MoveTowards(_healthShown, _healthTarget, Time.unscaledDeltaTime * 1.4f);
            UIFactory.Fill(_healthFill, _healthShown);
            _fade.color = new Color(.086f,.075f,.059f,_ui.Demo.Fade);
            if (Time.unscaledTime < _next && !_dirty) return;
            _next = Time.unscaledTime + .15f; _dirty = false;
            var d = _ui.Demo; bool exploration = d.State == TechnicalDemoState.Exploration;
            _ui.Map?.Discover();
            _healthPanel.SetActive(d.State == TechnicalDemoState.Combat || exploration && _healthTarget < 1);
            string objective = d.PortalsUnlocked ? UIStrings.Get("hud.portals") : d.Objectives.IsComplete ? UIStrings.Get("hud.duel")
                : UIStrings.Get("hud.objective", d.Objectives.AnalyzedCount, d.Objectives.RequiredCount);
            if (exploration && objective != _previousObjective)
            { _previousObjective = objective; _objective.text = objective; _objectiveUntil = Time.unscaledTime + 8; }
            if (d.World != _world)
            {
                _world = d.World; _zoneUntil = Time.unscaledTime + 3;
                if (d.State == TechnicalDemoState.Transition) _ui.Subtitles?.Show("", UIStrings.Get("caption.portal"), 4, true);
                _zone.text = UIStrings.Get(d.World < 0 ? "world.lower" : d.World > 0 ? "world.upper" : "world.plaza");
            }
            _zonePanel.SetActive(exploration && Time.unscaledTime < _zoneUntil);
            _objectivePanel.SetActive(exploration && d.World == 0 && _ui.Settings.Values.showObjectives && !_healthPanel.activeSelf
                && Time.unscaledTime >= _zoneUntil && Time.unscaledTime < _objectiveUntil);
            string prompt = d.Nearby != null ? UIStrings.Get("hud.inspect", d.Nearby.Data.displayName)
                : d.NearbyPortal != null ? UIStrings.Get(d.NearbyPortal.Available ? "hud.travel" : "hud.blocked", d.NearbyPortal.destinationName)
                : d.NearCombat ? UIStrings.Get("hud.startCombat") : "";
            _promptPanel.SetActive(exploration && prompt.Length > 0 && !(_ui.Subtitles?.Active ?? false)); if (_prompt.text != prompt) _prompt.text = prompt;
            _devices.text = d.Hands.Requested ? UIStrings.Get("hud.cameraActive") : "";
            _devicesPanel.SetActive(_devices.text.Length > 0);
        }
        public void Dispose()
        {
            if (_ui.Demo != null)
            { _ui.Demo.ViewChanged -= MarkDirty; _ui.Demo.Objectives.Changed -= MarkDirty; }
            // Scene unload can destroy the player before the UI. Keep the original event source.
            if (_healthSource != null) _healthSource.HealthChanged -= Health;
            _ui.Screens.Changed -= Screen; _ui.Settings.Changed -= MarkDirty;
        }
    }
}

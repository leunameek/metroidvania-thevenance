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
        private readonly GameObject _root, _healthPanel, _objectivePanel, _promptPanel, _zonePanel, _devicesPanel, _hints;
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
            var veil = _root.AddComponent<TitleMenuBackdrop>(); veil.raycastTarget = false; veil.fromTop = true; veil.readingEdge = .22f; veil.strength = .55f;
            // Screen 08: persistent information near the edges, the centre free for the scene.
            // Upper left: portrait ring and health (Marco_HUD). Upper right: zone and objective.
            // Lower centre: the contextual interaction on the crimson ribbon. Corners: key hints.
            var health = f.Rect("Vitality", _root.transform, new Vector2(0, 1), new Vector2(0, 1));
            health.pivot = new Vector2(0, 1); health.anchoredPosition = new Vector2(64, -40); health.sizeDelta = new Vector2(452, 138);
            _healthPanel = health.gameObject;
            var ring = f.Rect("Portrait", health, new Vector2(.151f, .5f), new Vector2(.151f, .5f)); ring.sizeDelta = new Vector2(104, 104);
            var portrait = ring.gameObject.AddComponent<RawImage>(); portrait.texture = UIBacata.Art("Retrato_HUD"); portrait.raycastTarget = false;
            var frame = health.gameObject.AddComponent<Image>(); frame.sprite = UIBacata.Get("Frames/Frame_HUD"); frame.raycastTarget = false;
            frame.preserveAspect = true; if (frame.sprite == null) frame.color = Color.clear;
            var channel = f.Rect("Inner", health, new Vector2(.336f, .35f), new Vector2(.904f, .60f));
            var fill = f.Rect("Fill", channel, Vector2.zero, Vector2.one);
            _healthFill = fill.gameObject.AddComponent<Image>(); _healthFill.sprite = UIBacata.Get("Controls/Bar_Fill");
            _healthFill.type = Image.Type.Sliced; _healthFill.color = UIPalette.Crimson; _healthFill.raycastTarget = false;
            var name = UIFactory.Shadow(Fit(f.Label(health, UIStrings.Get("hud.health"), new Vector2(.34f, .64f), new Vector2(.70f, .98f), 22)));
            name.alignment = TextAlignmentOptions.BottomLeft; name.fontStyle = FontStyles.Bold;
            _health = UIFactory.Shadow(Fit(UIFactory.Tone(f.Label(health, "", new Vector2(.62f, .64f), new Vector2(.90f, .98f), 20), UITone.Muted)));
            _health.alignment = TextAlignmentOptions.BottomRight;
            _zonePanel = f.Rect("Zone", _root.transform, new Vector2(.50f, .885f), new Vector2(.96f, .955f)).gameObject;
            _zone = UIFactory.Shadow(f.Heading(_zonePanel.transform, "", Vector2.zero, Vector2.one, 42));
            _zone.alignment = TextAlignmentOptions.BottomRight;
            _objectivePanel = f.Rect("Objective", _root.transform, new Vector2(.50f, .835f), new Vector2(.96f, .885f)).gameObject;
            _objective = UIFactory.Shadow(f.Label(_objectivePanel.transform, "", Vector2.zero, Vector2.one, 24));
            _objective.alignment = TextAlignmentOptions.TopRight;
            _promptPanel = f.Rect("UI_Indicator_Interaction", _root.transform, new Vector2(.32f, .17f), new Vector2(.68f, .235f)).gameObject;
            UIBacata.Skin(_promptPanel.transform, "Controls/Ribbon");
            var promptRow = f.Row(_promptPanel.transform, "PromptRow", 14);
            promptRow.offsetMin = new Vector2(80, 4); promptRow.offsetMax = new Vector2(-80, -4);
            var promptLayout = promptRow.GetComponent<HorizontalLayoutGroup>(); promptLayout.childForceExpandWidth = false;
            _prompt = f.Text(promptRow, "", 26); _prompt.alignment = TextAlignmentOptions.Midline; _prompt.fontStyle = FontStyles.Bold;
            _prompt.textWrappingMode = TextWrappingModes.NoWrap;
            f.KeyHint(_prompt, promptRow, true);
            _devicesPanel = f.Rect("DeviceStatus", _root.transform, new Vector2(.50f, .80f), new Vector2(.96f, .835f)).gameObject;
            _devices = UIFactory.Shadow(Fit(UIFactory.Tone(f.Label(_devicesPanel.transform, "", Vector2.zero, Vector2.one, 20), UITone.Success)));
            _devices.alignment = TextAlignmentOptions.TopRight;
            _hints = f.Rect("Hints", _root.transform, Vector2.zero, Vector2.one).gameObject;
            f.Hint(_hints.transform, UIStrings.Get("footer.pause"), Vector2.zero);
            f.Hint(_hints.transform, UIStrings.Get("footer.journal"), Vector2.right);
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
            _root.SetActive(_ui.Screens.Current == UIScreen.None && !_ui.ModalOpen && !(_ui.Dialogue?.Active ?? false) && !StoryPlayer.Active && !TurnDuelController.Running);
            if (!_root.activeSelf) return;
            _healthShown = _ui.Settings.Values.reducedMotion ? _healthTarget : Mathf.MoveTowards(_healthShown, _healthTarget, Time.unscaledDeltaTime * 1.4f);
            UIFactory.Fill(_healthFill, _healthShown);
            _fade.color = new Color(.031f,.039f,.043f,_ui.Demo.Fade);
            if (Time.unscaledTime < _next && !_dirty) return;
            _next = Time.unscaledTime + .15f; _dirty = false;
            var d = _ui.Demo; bool exploration = d.State == TechnicalDemoState.Exploration;
            _ui.Map?.Discover();
            _healthPanel.SetActive(d.State == TechnicalDemoState.Combat || exploration && _healthTarget < 1);
            // After the tutorial the story names the next step (CampaignModel.Objective).
            var campaign = CampaignProgress.Model;
            string objective = campaign.Chapter > CampaignChapter.PlazaTutorial ? campaign.Objective.Text
                : d.PortalsUnlocked ? UIStrings.Get("hud.portals") : d.Objectives.IsComplete ? UIStrings.Get("hud.duel")
                : UIStrings.Get("hud.objective", d.Objectives.AnalyzedCount, d.Objectives.RequiredCount);
            if (exploration && objective != _previousObjective)
            { _previousObjective = objective; _objective.text = objective; _objectiveUntil = Time.unscaledTime + 8; }
            if (d.World != _world)
            {
                _world = d.World; _zoneUntil = Time.unscaledTime + 3;
                if (d.State == TechnicalDemoState.Transition) _ui.Subtitles?.Show("", UIStrings.Get("caption.portal"), 4, true);
                _zone.text = UIStrings.Get(d.World < 0 ? "world.lower" : d.World > 0 ? "world.upper" : "world.plaza");
            }
            // Zone and objective share the upper-right corner, so both can stay on screen; only
            // one main objective occupies the header.
            _zonePanel.SetActive(exploration);
            _hints.SetActive(exploration);
            _objectivePanel.SetActive(exploration && d.World == 0 && _ui.Settings.Values.showObjectives);
            string prompt = d.Nearby != null ? UIStrings.Get("hud.inspect", d.Nearby.Data.displayName)
                : d.NearbyStory != null ? "E · " + d.NearbyStory.Prompt
                : d.NearbyPortal != null ? UIStrings.Get(d.NearbyPortal.Available ? "hud.travel" : "hud.blocked", d.NearbyPortal.destinationName, d.NearbyPortal.LockedReason)
                : d.NearCombat ? UIStrings.Get("hud.startCombat") : "";
            _promptPanel.SetActive(exploration && prompt.Length > 0 && !(_ui.Subtitles?.Active ?? false)); if (_prompt.text != prompt) _prompt.text = prompt;
            // Camera, the open-palm hint while something can be used, and the microphone.
            string devices = d.Hands.Requested ? UIStrings.Get("hud.cameraActive") : "";
            if (d.Hands.Live && exploration && prompt.Length > 0 && !d.MouseMode) devices += " · " + UIStrings.Get("hud.handsHold");
            if (_ui.Voice != null && _ui.Voice.Listening) devices += (devices.Length > 0 ? " · " : "") + UIStrings.Get("hud.voiceListening");
            _devices.text = devices;
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

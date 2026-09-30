using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class CombatUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly GameObject _root;
        private readonly TMP_Text _turn, _body, _enemy, _timer, _voice;
        private readonly Image _progress, _guardian;
        private readonly RectTransform _commands;
        private readonly Button _attack, _dodge, _block, _return;
        private float _next;
        public CombatUIController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            _root = f.Rect("UI_HUD_Combat", ui.Root, Vector2.zero, Vector2.one).gameObject;
            // Turn banner with the guardian's resistance drawn like the reference boss bar:
            // three carved segments, one per hit, with the written count inside.
            var title = f.HudPanel("Turn", _root.transform, new Vector2(.33f,.80f), new Vector2(.67f,.955f));
            _turn = f.Heading(title, "", new Vector2(.05f,.50f), new Vector2(.95f,.92f), 36);
            Fit(f.Caption(title, UIStrings.Get("combat.guardian"), 20)).rectTransform.SetAnchors(new Vector2(.06f,.10f), new Vector2(.30f,.44f));
            _guardian = f.Bar(title, "GuardianResistance", new Vector2(.31f,.15f), new Vector2(.94f,.40f), UITheme.Hex("A3322A"));
            var inner = _guardian.transform.parent;
            for (int i = 1; i < 3; i++)
            {
                var notch = f.Rect("Segment", inner, new Vector2(i / 3f, 0), new Vector2(i / 3f, 1));
                notch.sizeDelta = new Vector2(3, 0); notch.gameObject.AddComponent<Image>().color = UIPalette.Charcoal;
            }
            _enemy = f.Label(inner.parent, "", Vector2.zero, Vector2.one, 20);
            _enemy.alignment = TextAlignmentOptions.MidlineRight; _enemy.margin = new Vector4(0,0,12,0); Fit(_enemy);
            var commands = f.Panel("Commands", _root.transform, new Vector2(.66f,.12f), new Vector2(.95f,.68f), true);
            _commands = commands;
            // Pixel insets keep reading text clear of the carved corners and the top crest.
            var body = f.Scroll(commands, Vector2.zero, Vector2.one);
            ((RectTransform)body.parent.parent).Inset(60, 88, 60, 76);
            _body = f.Text(body, "", 24); _timer = UIFactory.Tone(f.Text(body, "", 22), UITone.Gold);
            _attack = Action(f, body, "combat.attack", UIIcon.Sword, () => ui.Demo.Combat.Attack(), true);
            _dodge = Action(f, body, "combat.dodge", UIIcon.Dodge, () => ui.Demo.Combat.Defend(PlazaDefense.Dodge));
            _block = Action(f, body, "combat.block", UIIcon.Shield, () => ui.Demo.Combat.Defend(PlazaDefense.Guard));
            _return = f.Button(body, UIStrings.Get("combat.return"), () => ui.Demo.Combat.Cancel());
            _voice = UIFactory.Tone(f.Text(body, "", 20), UITone.Muted);
            _progress = f.Bar(commands, "ReactionWindow", Vector2.zero, Vector2.one);
            ((RectTransform)_progress.transform.parent.parent).Band(60, 16, 64);
            ui.Demo.Combat.Model.Changed += Refresh; ui.Demo.ViewChanged += Refresh; ui.Screens.Changed += OnScreen;
            Refresh();
        }
        private static Button Action(UIFactory f, Transform parent, string key, UIIcon icon, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            var button = f.Button(parent, UIStrings.Get(key), action, primary);
            var glyph = f.Icon(button.transform, icon, 30, primary ? UIPalette.OnGold : UIPalette.GoldLight);
            glyph.transform.SetSiblingIndex(1);
            return button;
        }
        // Fixed HUD boxes shrink their text at 150 % instead of wrapping or spilling.
        private static TMP_Text Fit(TMP_Text text)
        {
            text.textWrappingMode = TextWrappingModes.NoWrap; text.enableAutoSizing = true;
            text.fontSizeMax = text.fontSize; text.fontSizeMin = 14; return text;
        }
        private void OnScreen(UIScreen s) { Refresh(); }
        private void Refresh()
        {
            var model = _ui.Demo.Combat.Model;
            _root.SetActive(_ui.Demo.State == TechnicalDemoState.Combat && _ui.Screens.Current == UIScreen.None);
            if (!_root.activeSelf) return;
            _turn.text = UIStrings.Get("combat.phase." + model.Phase);
            int resistance = Mathf.Clamp(3 - model.Hits, 0, 3);
            _enemy.text = resistance + " / 3"; UIFactory.Fill(_guardian, resistance / 3f);
            _body.text = UIStrings.Get("combat.body." + model.Phase);
            if (model.Phase == PlazaCombatPhase.Telegraph || model.Phase == PlazaCombatPhase.React)
                _body.text += "\n\n" + UIStrings.Get(model.Expected == PlazaDefense.Dodge ? "combat.dodge" : "combat.block");
            if (model.Phase == PlazaCombatPhase.Feedback) _body.text = UIStrings.Get(model.LastDefenseSucceeded ? "combat.success" : "combat.retry");
            _attack.gameObject.SetActive(model.Phase == PlazaCombatPhase.Attack);
            _dodge.gameObject.SetActive(model.Phase == PlazaCombatPhase.React);
            _block.gameObject.SetActive(model.Phase == PlazaCombatPhase.React);
            _return.gameObject.SetActive(model.Phase == PlazaCombatPhase.Won);
            _progress.transform.parent.parent.gameObject.SetActive(model.Phase == PlazaCombatPhase.React);
            if (model.Phase == PlazaCombatPhase.React) _body.text = UIStrings.Get(model.Expected == PlazaDefense.Dodge ? "combat.promptDodge" : "combat.promptBlock");
            if (model.Phase == PlazaCombatPhase.Won) _body.text = UIStrings.Get(_ui.Demo.Objectives.IsComplete ? "hud.portals" : "combat.body.Won");
            var focus = model.Phase == PlazaCombatPhase.Attack ? _attack : model.Phase == PlazaCombatPhase.Won ? _return
                : model.Phase == PlazaCombatPhase.React ? model.Expected == PlazaDefense.Dodge ? _dodge : _block : null;
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(focus != null ? focus.gameObject : null);
        }
        public void Tick()
        {
            if (!_root.activeSelf || Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + .05f;
            var m = _ui.Demo.Combat.Model;
            _commands.anchorMin = new Vector2(.64f, .68f - (m.Phase == PlazaCombatPhase.React ? .36f : .27f) * _ui.Settings.Values.textScale
                - (_ui.Settings.Values.voiceEnabled ? .10f : 0));
            _timer.gameObject.SetActive(m.Phase == PlazaCombatPhase.React);
            _voice.text = _ui.Settings.Values.voiceEnabled && _ui.Voice != null ? _ui.Voice.StatusText : "";
            _voice.gameObject.SetActive(_voice.text.Length > 0 && _ui.Voice.State != VoiceState.Inactive);
            _timer.text = m.Phase == PlazaCombatPhase.React ? UIStrings.Get("combat.seconds", m.Remaining.ToString("0.0")) : "";
            UIFactory.Fill(_progress, m.Phase == PlazaCombatPhase.React ? m.Remaining / m.ReactionSeconds : m.Phase == PlazaCombatPhase.Telegraph ? m.Remaining / m.TelegraphSeconds : 1);
        }
        public void Dispose()
        {
            if (_ui.Demo != null) { _ui.Demo.ViewChanged -= Refresh; _ui.Demo.Combat.Model.Changed -= Refresh; }
            _ui.Screens.Changed -= OnScreen;
        }
    }
}

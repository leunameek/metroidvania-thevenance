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
        private readonly TMP_Text _turn, _body, _enemy, _timer, _voice, _hands;
        private readonly Image _progress, _guardian;
        private readonly RectTransform _commands;
        private readonly Button _attack, _dodge, _block, _return;
        private float _next;
        public CombatUIController(UIManager ui)
        {
            _ui = ui; var f = ui.Factory;
            _root = f.Rect("UI_HUD_Combat", ui.Root, Vector2.zero, Vector2.one).gameObject;
            // Screens 14 and 15: the turn on a short crimson ribbon at the top, the guardian's
            // name and resistance in the upper right, the actions in one row near the bottom
            // edge, with instruction and reaction time together in the same panel.
            var banner = f.Rect("Turn", _root.transform, new Vector2(.35f, .80f), new Vector2(.65f, .865f));
            UIBacata.Skin(banner, "Controls/Ribbon_Focus");
            _turn = f.Label(banner, "", new Vector2(.12f, 0), new Vector2(.88f, 1), 28);
            _turn.alignment = TextAlignmentOptions.Center; _turn.fontStyle = FontStyles.Bold | FontStyles.UpperCase; Fit(_turn);
            var enemy = f.Rect("Guardian", _root.transform, new Vector2(.62f, .86f), new Vector2(.96f, .955f));
            var enemyName = UIFactory.Shadow(Fit(f.Label(enemy, UIStrings.Get("combat.guardian"), new Vector2(0, .55f), new Vector2(1, 1), 22)));
            enemyName.alignment = TextAlignmentOptions.BottomRight;
            _guardian = f.Bar(enemy, "GuardianResistance", new Vector2(.12f, .22f), new Vector2(1, .52f), UIPalette.Crimson);
            var inner = _guardian.transform.parent;
            for (int i = 1; i < 3; i++)
            {
                var notch = f.Rect("Segment", inner, new Vector2(i / 3f, 0), new Vector2(i / 3f, 1));
                notch.sizeDelta = new Vector2(3, 0); notch.gameObject.AddComponent<Image>().color = UIPalette.Charcoal;
            }
            _enemy = UIFactory.Shadow(UIFactory.Tone(f.Label(enemy, "", new Vector2(.12f, -.12f), new Vector2(1, .22f), 18), UITone.Muted));
            _enemy.alignment = TextAlignmentOptions.TopRight; Fit(_enemy);
            var commands = f.Panel("Commands", _root.transform, new Vector2(.24f, .04f), new Vector2(.76f, .31f), true, true, false);
            _commands = commands;
            // Pixel insets keep reading text clear of the rim and the emblem.
            var body = f.Scroll(commands, Vector2.zero, Vector2.one);
            ((RectTransform)body.parent.parent).Inset(56, 66, 56, 60);
            body.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            _body = f.Text(body, "", 24); _body.alignment = TextAlignmentOptions.Center;
            _timer = UIFactory.Tone(f.Text(body, "", 22), UITone.Gold); _timer.alignment = TextAlignmentOptions.Center;
            var actions = f.Row(body, "Actions", 16); actions.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            _attack = Action(f, actions, "combat.attack", UIIcon.Sword, () => ui.Demo.Combat.Attack(), true);
            _dodge = Action(f, actions, "combat.dodge", UIIcon.Dodge, () => ui.Demo.Combat.Defend(PlazaDefense.Dodge));
            _block = Action(f, actions, "combat.block", UIIcon.Shield, () => ui.Demo.Combat.Defend(PlazaDefense.Guard));
            _return = f.Button(actions, UIStrings.Get("combat.return"), () => ui.Demo.Combat.Cancel(), true);
            UIFactory.CenterAll(actions);
            _voice = UIFactory.Tone(f.Text(body, "", 20), UITone.Success); _voice.alignment = TextAlignmentOptions.Center;
            _hands = UIFactory.Tone(f.Text(body, "", 20), UITone.Gold); _hands.alignment = TextAlignmentOptions.Center;
            _progress = f.Bar(commands, "ReactionWindow", Vector2.zero, Vector2.one, UIPalette.GoldLight);
            ((RectTransform)_progress.transform.parent.parent).Band(36, 14, 72);
            f.Hint(_root.transform, UIStrings.Get("footer.pause"), Vector2.zero);
            ui.Demo.Combat.Model.Changed += Refresh; ui.Demo.ViewChanged += Refresh; ui.Screens.Changed += OnScreen;
            Refresh();
        }
        private static Button Action(UIFactory f, Transform parent, string key, UIIcon icon, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            var button = f.Button(parent, UIStrings.Get(key), action, primary);
            var glyph = f.Icon(button.transform, icon, 30, primary ? UIPalette.Ivory : UIPalette.GoldLight);
            glyph.transform.SetSiblingIndex(1);
            var h = button.GetComponent<HorizontalLayoutGroup>(); h.padding.left = 56; h.padding.right = 44; h.spacing = 10;
            UIFactory.Fit(button.GetComponentInChildren<TMP_Text>());
            return button;
        }
        // Fixed HUD boxes shrink their text at 150 % instead of wrapping or spilling.
        private static TMP_Text Fit(TMP_Text text)
        {
            text.textWrappingMode = TextWrappingModes.NoWrap; text.enableAutoSizing = true;
            text.fontSizeMax = text.fontSize; text.fontSizeMin = 14; return text;
        }
        private void OnScreen(UIScreen s) { Refresh(); }
        private static void Label(Button button, string key, string word, string keyName)
        {
            // With the voice on, the button says its word once, in the middle («Atacar»), and the line
            // below says the voice is listening (2026-10-07 playtest: «Atacar» and «ATACAR» side by
            // side). With the voice off, UIKeyHint splits "Atacar · E" into the label and its key cap.
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = VoicePrompt.Enabled ? "«" + char.ToUpperInvariant(word[0]) + word.Substring(1) + "»"
                : UIStrings.Get(key, VoicePrompt.Cap(word, keyName));
        }
        private void Refresh()
        {
            var model = _ui.Demo.Combat.Model;
            _root.SetActive(_ui.Demo.State == TechnicalDemoState.Combat && _ui.Screens.Current == UIScreen.None);
            if (!_root.activeSelf) return;
            _turn.text = UIStrings.Get("combat.phase." + model.Phase);
            int resistance = Mathf.Clamp(3 - model.Hits, 0, 3);
            _enemy.text = resistance + " / 3"; UIFactory.Fill(_guardian, resistance / 3f);
            // Voice first: each action names its word («atacar»), or its key with the voice off.
            Label(_attack, "combat.attack", "atacar", "E"); Label(_dodge, "combat.dodge", "esquivar", "Espacio");
            Label(_block, "combat.block", "bloquear", "F"); Label(_return, "combat.return", "volver", "E");
            _body.text = UIStrings.Get("combat.body." + model.Phase);
            if (model.Phase == PlazaCombatPhase.Telegraph || model.Phase == PlazaCombatPhase.React)
                _body.text += "\n\n" + (model.Expected == PlazaDefense.Dodge ? UIStrings.Get("combat.dodge", VoicePrompt.Cap("esquivar", "Espacio"))
                    : UIStrings.Get("combat.block", VoicePrompt.Cap("bloquear", "F")));
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
            _commands.anchorMax = new Vector2(.76f, .31f + (_ui.Settings.Values.textScale - 1) * .22f
                + (_ui.Settings.Values.voiceEnabled ? .04f : 0) + (_ui.Demo.Hands.Live ? .08f : 0));
            _timer.gameObject.SetActive(m.Phase == PlazaCombatPhase.React);
            _voice.text = _ui.Settings.Values.voiceEnabled && _ui.Voice != null ? _ui.Voice.StatusText : "";
            _voice.gameObject.SetActive(_voice.text.Length > 0 && _ui.Voice.State != VoiceState.Inactive);
            // Camera on: the gesture of this turn, or the one just recognised for a moment.
            var combat = _ui.Demo.Combat;
            string hands = !_ui.Demo.Hands.Live ? "" : Time.unscaledTime - combat.LastGestureTime < 1.5f ? combat.LastGesture
                : UIStrings.Get("combat.hands." + m.Phase);
            if (hands.StartsWith("[")) hands = "";
            if (_hands.text != hands) _hands.text = hands;
            _hands.gameObject.SetActive(hands.Length > 0);
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

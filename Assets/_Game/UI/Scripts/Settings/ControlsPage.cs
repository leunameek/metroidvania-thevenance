using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // «Controles»: every keyboard action with two keys. Enter (or a click) on a key waits for the
    // next key; Esc cancels and Supr leaves the slot empty. A key taken by another action of the
    // same moment of play (movement, combat, system) is swapped with it; across moments it is
    // shared, and the row says so. The choice is GameBindings', read by the plaza and both worlds.
    public sealed class ControlsPage : SettingsRows
    {
        private readonly Action<string, Action> _confirm;
        private readonly List<(GameAction action, int slot, Button button)> _keys = new List<(GameAction, int, Button)>();
        private TMP_Text _status;
        private Button _waiting;
        private GameAction _waitAction;
        private int _waitSlot, _waitFrame, _releaseFrame = -1;
        private bool _navigation = true;

        // confirm: the menu's own confirmation (title modal or pause modal).
        public ControlsPage(UIFactory factory, SettingsManager settings, Action<string, Action> confirm) : base(factory, settings)
        { _confirm = confirm; }

        public void Build(RectTransform group)
        {
            var ticker = group.gameObject.AddComponent<UIPageTicker>();
            ticker.tick = Tick; ticker.shown = Refresh;
            ticker.hidden = () => { Stop(false); if (_releaseFrame >= 0) Release(); };
            _status = UIFactory.Tone(F.Text(group, UIStrings.Get("keys.intro"), 22), UITone.Muted);
            _status.margin = new Vector4(76, 0, 40, 0);
            _status.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            var header = F.Row(group, "Encabezado", 18); header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
            var action = UIFactory.Tone(F.Text(header, UIStrings.Get("keys.action"), 20), UITone.Muted); action.margin = new Vector4(76, 0, 0, 0);
            action.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var first = UIFactory.Tone(F.Text(header, UIStrings.Get("keys.primary"), 20), UITone.Muted); first.alignment = TextAlignmentOptions.Center;
            first.gameObject.AddComponent<LayoutElement>().preferredWidth = 230;
            var second = UIFactory.Tone(F.Text(header, UIStrings.Get("keys.secondary"), 20), UITone.Muted); second.alignment = TextAlignmentOptions.Center;
            second.gameObject.AddComponent<LayoutElement>().preferredWidth = 230;

            BindingGroup? current = null;
            for (int i = 0; i < GameBindings.Count; i++)
            {
                var a = (GameAction)i;
                if (current != GameBindings.Group(a)) { current = GameBindings.Group(a); Section(group, "keys.group." + current); }
                Row(group, a);
            }
            Note(group, UIStrings.Get("keys.fixed"));
            Act(group, "keys.reset", () => _confirm("keys.resetConfirm", () => { GameBindings.ResetDefaults(); Refresh(); _status.text = UIStrings.Get("keys.resetDone"); }));
            GameBindings.Changed += Refresh;
            group.gameObject.AddComponent<UIDisposeHook>().disposed = () => GameBindings.Changed -= Refresh;
        }

        private void Row(Transform parent, GameAction action)
        {
            var row = F.Row(parent, "Tecla_" + action, 12);
            var layout = row.GetComponent<HorizontalLayoutGroup>(); layout.childForceExpandWidth = false;
            row.gameObject.AddComponent<LayoutElement>().minHeight = 56;
            var label = F.Text(row, GameBindings.Name(action), 24); label.alignment = TextAlignmentOptions.MidlineLeft;
            label.margin = new Vector4(76, 0, 0, 0); label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            for (int slot = 0; slot < GameBindings.Slots; slot++)
            {
                int s = slot; Button button = null;
                button = F.Button(row, "", () => Begin(action, s, button));
                UIFactory.Center(button);
                var h = button.GetComponent<HorizontalLayoutGroup>(); h.padding.left = h.padding.right = 20;
                var size = button.GetComponent<LayoutElement>(); size.preferredWidth = size.minWidth = 230;
                var text = button.GetComponentInChildren<TMP_Text>(); UIFactory.Tone(text, UITone.GoldLight);
                // The key cap of the factory button would split «Ctrl» from the label: keep the plain text.
                var hint = text.GetComponent<UIKeyHint>(); if (hint != null) UnityEngine.Object.Destroy(hint);
                var described = button.gameObject.AddComponent<UIDescribed>();
                described.text = GameBindings.Name(action); described.sink = _ => { if (_waiting == null) _status.text = Shared(action); };
                _keys.Add((action, s, button));
            }
        }

        private void Refresh()
        {
            foreach (var (action, slot, button) in _keys)
            {
                var key = GameBindings.Get(action, slot);
                var text = button.GetComponentInChildren<TMP_Text>();
                text.text = button == _waiting ? UIStrings.Get("keys.press") : key == Key.None ? "—" : GameBindings.KeyLabel(key);
            }
        }

        private string Shared(GameAction action)
        {
            var names = new List<string>();
            for (int slot = 0; slot < GameBindings.Slots; slot++)
                foreach (var other in GameBindings.SharedWith(action, GameBindings.Get(action, slot)))
                    if (!names.Contains(GameBindings.Name(other))) names.Add(GameBindings.Name(other));
            return names.Count == 0 ? UIStrings.Get("keys.intro") : UIStrings.Get("keys.shared", GameBindings.Name(action), string.Join(", ", names));
        }

        private void Begin(GameAction action, int slot, Button button)
        {
            if (_waiting != null) return;
            _waiting = button; _waitAction = action; _waitSlot = slot; _waitFrame = Time.frameCount;
            GameBindings.Listening = true;
            // Arrows, Enter and Tab must reach the binding, not move the menu focus.
            if (EventSystem.current != null) { _navigation = EventSystem.current.sendNavigationEvents; EventSystem.current.sendNavigationEvents = false; }
            _status.text = UIStrings.Get("keys.waiting", GameBindings.Name(action));
            Refresh();
        }

        private void Stop(bool keepFocus)
        {
            if (_waiting == null) return;
            var button = _waiting; _waiting = null;
            // Released next frame, so the key that ended the wait does not also act in the menu.
            _releaseFrame = Time.frameCount + 1;
            if (!keepFocus) Release();
            Refresh();
            if (keepFocus && button != null) EventSystem.current?.SetSelectedGameObject(button.gameObject);
        }

        private void Release()
        {
            GameBindings.Listening = false; _releaseFrame = -1;
            if (EventSystem.current != null) EventSystem.current.sendNavigationEvents = _navigation;
        }

        private void Tick()
        {
            if (_releaseFrame >= 0 && Time.frameCount > _releaseFrame) Release();
            if (_waiting == null || Time.frameCount <= _waitFrame) return;
            var keyboard = Keyboard.current; if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { _status.text = UIStrings.Get("keys.cancelled"); Stop(true); return; }
            if (keyboard.deleteKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
            {
                GameBindings.Set(_waitAction, _waitSlot, Key.None);
                _status.text = UIStrings.Get("keys.cleared", GameBindings.Name(_waitAction)); Stop(true); return;
            }
            foreach (var control in keyboard.allKeys)
            {
                if (control == null || !control.wasPressedThisFrame) continue;
                var key = control.keyCode;
                if (Array.IndexOf(GameBindings.Reserved, key) >= 0) { _status.text = UIStrings.Get("keys.reserved", GameBindings.KeyLabel(key)); return; }
                var swapped = GameBindings.Set(_waitAction, _waitSlot, key);
                string message = UIStrings.Get("keys.assigned", GameBindings.KeyLabel(key), GameBindings.Name(_waitAction));
                if (swapped.HasValue) message += " " + UIStrings.Get("keys.swapped", GameBindings.Name(swapped.Value), GameBindings.Label(swapped.Value));
                var shared = GameBindings.SharedWith(_waitAction, key);
                if (shared.Count > 0) message += " " + UIStrings.Get("keys.alsoUsed", GameBindings.Name(shared[0]));
                _status.text = message; Stop(true); return;
            }
        }
    }
}

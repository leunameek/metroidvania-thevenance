using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum UIScreen { None, Start, FirstRun, MainMenu, Settings, Accessibility, VoiceCalibration, HandCalibration, Pause, SaveLoad, Map, Objectives, Inventory, Poporo, Archive, Controls, Credits, Defeat, Complete, Loading }
    public sealed class ScreenManager
    {
        private readonly Dictionary<UIScreen, GameObject> _screens = new Dictionary<UIScreen, GameObject>();
        private readonly Stack<UIScreen> _history = new Stack<UIScreen>();
        private readonly Dictionary<UIScreen, GameObject> _focus = new Dictionary<UIScreen, GameObject>();
        public UIScreen Current { get; private set; }
        public event Action<UIScreen> Changed;
        public IEnumerable<GameObject> Screens => _screens.Values;
        public void Register(UIScreen id, GameObject panel) { _screens.Add(id, panel); panel.SetActive(false); }
        public void Show(UIScreen id, bool remember = true)
        {
            Navigate(id, remember, false);
        }
        public void Back() { Navigate(_history.Count > 0 ? _history.Pop() : UIScreen.None, false, true); }
        public void Replace(UIScreen id) { Navigate(id, false, true); }
        private void Navigate(UIScreen id, bool remember, bool preserveHistory)
        {
            if (id == Current) return;
            if (Current != UIScreen.None && EventSystem.current != null) _focus[Current] = EventSystem.current.currentSelectedGameObject;
            if (remember) _history.Push(Current); else if (!preserveHistory) _history.Clear();
            if (_screens.TryGetValue(Current, out var previous)) previous.SetActive(false);
            Current = id;
            if (_screens.TryGetValue(id, out var next)) next.SetActive(true);
            Changed?.Invoke(id); SelectDefault();
        }
        public void SelectDefault()
        {
            if (EventSystem.current == null) return;
            if (!_screens.TryGetValue(Current, out var panel))
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if (selected != null && !selected.activeInHierarchy) EventSystem.current.SetSelectedGameObject(null);
                return;
            }
            EventSystem.current.SetSelectedGameObject(null);
            if (_focus.TryGetValue(Current, out var focus) && focus != null && focus.activeInHierarchy
                && focus.TryGetComponent<Selectable>(out var selectable) && selectable.IsInteractable())
                EventSystem.current.SetSelectedGameObject(focus);
            else foreach (var control in panel.GetComponentsInChildren<Selectable>())
                if (control.IsInteractable()) { EventSystem.current.SetSelectedGameObject(control.gameObject); break; }
        }
    }
}

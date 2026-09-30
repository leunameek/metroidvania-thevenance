using Nemequene.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ScreenFocusTests
{
    [Test]
    public void FocusWithoutSelectableFallsBackToAnInteractiveControl()
    {
        var previous = EventSystem.current;
        var root = new GameObject("Navigation focus test");
        try
        {
            var events = root.AddComponent<EventSystem>();
            EventSystem.current = events;
            var pause = new GameObject("Pause");
            var map = new GameObject("Map");
            pause.transform.SetParent(root.transform);
            map.transform.SetParent(root.transform);
            var focus = new GameObject("Non-selectable focus");
            focus.transform.SetParent(pause.transform);
            var button = new GameObject("Button", typeof(RectTransform), typeof(Button));
            button.transform.SetParent(pause.transform);
            var screens = new ScreenManager();
            screens.Register(UIScreen.Pause, pause);
            screens.Register(UIScreen.Map, map);
            screens.Show(UIScreen.Pause);
            events.SetSelectedGameObject(focus);
            screens.Show(UIScreen.Map);
            Assert.DoesNotThrow(() => screens.Back());
            Assert.That(events.currentSelectedGameObject, Is.EqualTo(button));
        }
        finally
        {
            Object.DestroyImmediate(root);
            if (previous != null) EventSystem.current = previous;
        }
    }
}

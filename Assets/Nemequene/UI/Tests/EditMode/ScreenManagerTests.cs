using NUnit.Framework;
using UnityEngine;

namespace Nemequene.UI.Tests
{
    public sealed class ScreenManagerTests
    {
        private GameObject _root;
        private ScreenManager _screens;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Navigation test");
            _screens = new ScreenManager();
            foreach (var id in new[] { UIScreen.Pause, UIScreen.Map, UIScreen.Objectives, UIScreen.Settings })
            {
                var panel = new GameObject(id.ToString());
                panel.transform.SetParent(_root.transform);
                _screens.Register(id, panel);
            }
        }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_root); }

        [Test]
        public void SelectingCurrentJournalTabRepeatedlyDoesNotAddBackSteps()
        {
            _screens.Show(UIScreen.Pause);
            _screens.Show(UIScreen.Map);
            for (int i = 0; i < 8; i++) _screens.Replace(UIScreen.Map);
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.Pause));
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.None));
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.None));
        }

        [Test]
        public void ChangingJournalTabsPreservesThePauseReturnPoint()
        {
            _screens.Show(UIScreen.Pause);
            _screens.Show(UIScreen.Map);
            _screens.Replace(UIScreen.Objectives);
            Assert.That(_root.transform.Find("Map").gameObject.activeSelf, Is.False);
            Assert.That(_root.transform.Find("Objectives").gameObject.activeSelf, Is.True);
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.Pause));
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.None));
        }

        [Test]
        public void StartingAnotherFlowClearsPreviousNavigation()
        {
            _screens.Show(UIScreen.Pause);
            _screens.Show(UIScreen.Map);
            _screens.Show(UIScreen.Settings, false);
            _screens.Back();
            Assert.That(_screens.Current, Is.EqualTo(UIScreen.None));
        }

    }
}

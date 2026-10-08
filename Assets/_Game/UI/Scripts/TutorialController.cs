using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class TutorialController
    {
        private readonly UIManager _ui;
        private readonly GameObject _card;
        private readonly TMP_Text _instruction;
        private int _step = -1;
        private Vector3 _start;
        private float _next;
        private bool _dismissed;
        public TutorialController(UIManager ui)
        {
            _ui = ui;
            var f = ui.Factory;
            // Guide panel of screens 08 and 23: a wide framed band near the bottom edge, title in
            // the serif, one instruction, and the acknowledgement on the right.
            var card = f.Panel("UI_TutorialCard", ui.Root, new Vector2(.20f,.03f), new Vector2(.80f,.20f), true, true, false);
            _card = card.gameObject;
            var column = f.Column(card, "Content", 6); column.Inset(60, 26, 380, 44);
            column.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            var title = f.Caption(column, UIStrings.Get("tutorial.title"), 30);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            _instruction = f.Text(column, "", 24);
            _instruction.alignment = TextAlignmentOptions.MidlineLeft;
            var actions = f.Column(card, "Acknowledge"); actions.anchorMin = actions.anchorMax = actions.pivot = new Vector2(1, .5f);
            actions.anchoredPosition = new Vector2(-44, -4); actions.sizeDelta = new Vector2(320, 60);
            var close = f.Button(actions, UIStrings.Get("tutorial.ok"), () => { _dismissed = true; _card.SetActive(false); }, true);
            UIFactory.Center(close);
            _card.SetActive(false);
        }
        public void Reset() { _step = -1; _next = 0; _dismissed = false; _card.SetActive(false); }
        public void Tick()
        {
            if (_card.activeSelf && (!_ui.Settings.Values.tutorials || !_ui.SessionStarted || _ui.Screens.Current != UIScreen.None
                || _ui.Demo.State != TechnicalDemoState.Exploration))
                _card.SetActive(false);
            if (!_ui.Settings.Values.tutorials || !_ui.SessionStarted || _ui.Screens.Current != UIScreen.None || Time.unscaledTime < _next) return;
            // Only the first visit teaches walking: back from a world, the plaza says nothing.
            if (CampaignProgress.Model.Chapter > CampaignChapter.PlazaTutorial) { if (_card.activeSelf) _card.SetActive(false); return; }
            _next = Time.unscaledTime + .2f;
            var d = _ui.Demo;
            if (_step == -1) { _start = d.Player.transform.position; Advance(0); }
            // Interaction prompts, inspection instructions and objective changes own the next steps.
            else if (_step == 0)
            {
                if (Vector3.Distance(_start,d.Player.transform.position) > 2) { _step = 1; _card.SetActive(false); }
                else if (!_dismissed && !_card.activeSelf && d.State == TechnicalDemoState.Exploration) _card.SetActive(true);
            }
        }
        private void Advance(int step)
        {
            _step = step; _instruction.text = UIStrings.Get("tutorial.step." + step);
            _card.SetActive(!_dismissed);
        }
    }
}

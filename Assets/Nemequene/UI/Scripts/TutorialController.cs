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
            // Reference block 3 «Panel de tutorial»: parchment card, sun medallion and carved title.
            // It sits on the right, below the toast column and clear of the inspection panel.
            var card = f.Parchment("UI_TutorialCard", ui.Root, new Vector2(.68f,.40f), new Vector2(.95f,.73f));
            _card = card.gameObject;
            var column = f.Column(card, "Content", 12); column.Inset(56, 44, 56, 44);
            column.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            var header = f.Row(column, "Header", 16); header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
            f.Icon(header, UIIcon.Sun, 52, UIPalette.GoldDeep);
            var title = f.Caption(header, UIStrings.Get("tutorial.title"), 28, UITone.Ink);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            var rule = f.Divider(column, Vector2.zero, Vector2.one); rule.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            rule.GetComponent<UIIconGraphic>().color = UIPalette.GoldDeep;
            _instruction = UIFactory.Tone(f.Text(column, "", 24), UITone.Ink);
            _instruction.alignment = TextAlignmentOptions.Center;
            var close = f.Button(column, UIStrings.Get("tutorial.ok"), () => { _dismissed = true; _card.SetActive(false); }, true);
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

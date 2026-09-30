using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class MenuController
    {
        private readonly UIManager _ui;
        private readonly Transform _parent;
        private readonly Dictionary<UIScreen, RectTransform> _bodies = new Dictionary<UIScreen, RectTransform>();
        private readonly Dictionary<UIScreen, TMP_Text> _titles = new Dictionary<UIScreen, TMP_Text>();
        private TMP_Text _objectives, _statTime, _statStations, _statZones, _statTraining;
        private Button _finish;
        public MenuController(UIManager ui, Transform parent)
        {
            _ui = ui; _parent = parent;
            // Application entry lives in the isolated Nemequene_MainMenu scene.
            var pause = Page(UIScreen.Pause, UIStrings.Get("pause"), false);
            ui.Factory.Button(pause, UIStrings.Get("resume"), () => ui.Screens.Show(UIScreen.None, false), true);
            Link(pause, "journal", UIScreen.Map);
            Link(pause, "inventory", UIScreen.Inventory);
            ui.Factory.Button(pause, UIStrings.Get("save.game"), () =>
            {
                if (ui.SaveCurrent()) ui.Notifications.Post(UIStrings.Get("save.saved"));
                else ui.Notifications.Post(UIStrings.Get("save.unavailable"));
            });
            Link(pause, "save.title", UIScreen.SaveLoad);
            Link(pause, "settings", UIScreen.Settings);
            ui.Factory.Button(pause, UIStrings.Get("mainMenu"), () => ui.Confirm("confirm.menu", ui.ReturnToMenu));
            ui.Factory.Button(pause, UIStrings.Get("quit"), () => ui.Confirm("confirm.quit", ui.Quit));
            var objectives = JournalPage(UIScreen.Objectives);
            _objectives = ui.Factory.Text(objectives, "");
            _finish = ui.Factory.Button(objectives, UIStrings.Get("finish"), () => ui.Screens.Show(UIScreen.Complete));
            var controls = Page(UIScreen.Controls, UIStrings.Get("controls"));
            ui.Factory.Text(controls, UIStrings.Get("controls.body"));
            ui.Factory.Button(controls, UIStrings.Get("tutorial.repeat"), ui.RepeatTutorial);
            Link(controls, "voice.calibrate", UIScreen.VoiceCalibration);
            Link(controls, "hands.calibrate", UIScreen.HandCalibration);
            var credits = Page(UIScreen.Credits, UIStrings.Get("credits"));
            ui.Factory.Text(credits, UIStrings.Get("credits.body"));
            UIFactory.CenterAll(pause);
            BuildDefeat();
            BuildComplete();
        }
        // Reference block 4 «Pantalla de muerte»: ceremonial red stone, mask glyph, dominant title,
        // one line of voice and the recovery actions, centred.
        private void BuildDefeat()
        {
            var f = _ui.Factory;
            var defeat = Page(UIScreen.Defeat, UIStrings.Get("defeat"), false);
            var panel = Centre(UIScreen.Defeat, .31f, .69f);
            UIFactory.Tint(panel, UITheme.Hex("FF9E94"));
            panel.parent.GetComponent<TitleMenuBackdrop>().tint = UITheme.Hex("2A0C0A");
            UIFactory.Tone(_titles[UIScreen.Defeat], UITone.Danger).GetComponent<UIStyleBinding>().baseSize = 60;
            var mask = f.Icon(defeat, UIIcon.Mask, 112, UITheme.Hex("C8352C")); mask.transform.SetAsFirstSibling();
            var quote = UIFactory.Tone(f.Text(defeat, UIStrings.Get("defeat.quote"), 26), UITone.Gold);
            quote.alignment = TextAlignmentOptions.Center; quote.fontStyle = FontStyles.Italic;
            var body = UIFactory.Tone(f.Text(defeat, UIStrings.Get("defeat.body"), 22), UITone.Muted); body.alignment = TextAlignmentOptions.Center;
            f.Rule(defeat);
            _ui.Factory.Button(defeat, UIStrings.Get("restart"), () => _ui.Confirm("confirm.restart", () => _ui.Loading.Restart()), true);
            Link(defeat, "settings", UIScreen.Settings);
            _ui.Factory.Button(defeat, UIStrings.Get("mainMenu"), () => _ui.Confirm("confirm.menu", _ui.ReturnToMenu));
            UIFactory.CenterAll(defeat);
        }
        // Reference block 4 «Nivel completado»: sun glyph, title and a stat table built only from
        // values the slice actually records.
        private void BuildComplete()
        {
            var f = _ui.Factory;
            var complete = Page(UIScreen.Complete, UIStrings.Get("complete"));
            Centre(UIScreen.Complete, .28f, .72f);
            var sun = f.Icon(complete, UIIcon.Radiant, 104, f.Theme.gold); sun.transform.SetAsFirstSibling();
            var table = f.Column(complete, "Stats", 8);
            _statTime = StatRow(table, UIIcon.Hourglass, "complete.time");
            _statStations = StatRow(table, UIIcon.Diamond, "complete.stations");
            _statZones = StatRow(table, UIIcon.Portal, "complete.zones");
            _statTraining = StatRow(table, UIIcon.Shield, "complete.training");
            f.Rule(complete);
            f.Button(complete, UIStrings.Get("mainMenu"), _ui.ReturnToMenu, true);
            f.Button(complete, UIStrings.Get("replay"), () => _ui.Confirm("confirm.replay", () => _ui.Loading.RestartFresh()));
            Link(complete, "credits", UIScreen.Credits);
            UIFactory.CenterAll(complete);
        }
        private TMP_Text StatRow(Transform parent, UIIcon icon, string key)
        {
            var f = _ui.Factory;
            var row = f.Row(parent, "Stat_" + key, 16);
            row.gameObject.AddComponent<Image>().color = new Color(.055f,.047f,.035f,.62f);
            var layout = row.GetComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(20,20,8,8);
            layout.childForceExpandWidth = false; layout.childAlignment = TextAnchor.MiddleLeft;
            row.gameObject.AddComponent<LayoutElement>().minHeight = 52;
            f.Icon(row, icon, 28, f.Theme.gold);
            var label = f.Text(row, UIStrings.Get(key), 22); label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var value = UIFactory.Tone(f.Text(row, "", 22), UITone.GoldLight); value.alignment = TextAlignmentOptions.MidlineRight;
            return value;
        }
        // Re-seat a page's reading surface in the centre of the screen (results and defeat).
        private RectTransform Centre(UIScreen id, float left, float right)
        {
            var panel = (RectTransform)_bodies[id].parent.parent.parent;
            panel.anchorMin = new Vector2(left, .06f); panel.anchorMax = new Vector2(right, .94f);
            panel.parent.GetComponent<TitleMenuBackdrop>().even = true;
            return panel;
        }
        public void FinishSetup()
        {
            _ui.Settings.Values.configured = true;
            if (!_ui.Demo.MouseMode) _ui.Demo.ToggleInputMode();
            _ui.Voice.Suspend(); _ui.Hands.Stop(); _ui.Settings.Apply(); _ui.Screens.Back();
        }
        public RectTransform Page(UIScreen id, string title, bool back = true)
        {
            var f = _ui.Factory;
            var screen = f.Rect("UI_Screen_" + id, _parent, Vector2.zero, Vector2.one);
            var shade = screen.gameObject.AddComponent<TitleMenuBackdrop>(); shade.raycastTarget = true;
            shade.readingEdge = id == UIScreen.Pause ? .40f : .62f;
            var panel = f.Panel("ReadingSurface", screen, new Vector2(.075f,.04f), new Vector2(.64f,.96f));
            if (id == UIScreen.Pause) panel.anchorMax = new Vector2(.48f,.96f);
            if (id == UIScreen.Inventory || id == UIScreen.SaveLoad) panel.anchorMax = new Vector2(.94f,.96f);
            panel.gameObject.AddComponent<UIPanelTransition>();
            // Carved title under the stone crest, then the ornamental divider of the reference headers.
            _titles[id] = f.Heading(panel, title, new Vector2(.08f,.84f), new Vector2(.92f,.935f), 48);
            f.Divider(panel, new Vector2(.28f,.81f), new Vector2(.72f,.835f)).name = "HeaderRule";
            var body = f.Scroll(panel, new Vector2(.055f,.13f), new Vector2(.945f,.79f));
            if (back)
            {
                // Above the carved bottom border, never on its corner brackets.
                var row = f.Column(panel, "BackRow"); row.anchorMin = row.anchorMax = new Vector2(.5f,0);
                row.pivot = new Vector2(.5f,0); row.anchoredPosition = new Vector2(0,64); row.sizeDelta = new Vector2(360,60);
                f.Button(row, UIStrings.Get("back"), () => _ui.Screens.Back());
            }
            _bodies[id] = body; _ui.Screens.Register(id, screen.gameObject); return body;
        }
        public RectTransform SectionNavigation(UIScreen id)
        {
            var body = _bodies[id]; var panel = body.parent.parent.parent as RectTransform;
            panel.anchorMax = new Vector2(.93f,.96f);
            var scroll = (RectTransform)body.parent.parent;
            // Both columns stay inside the stone frame (5 % inner margin), 3 % gutter between them.
            scroll.anchorMin = new Vector2(.33f,.15f); scroll.anchorMax = new Vector2(.955f,.80f);
            var nav = _ui.Factory.Scroll(panel, new Vector2(.045f,.15f), new Vector2(.30f,.80f));
            nav.name = "Sections";
            var back = panel.Find("BackRow") as RectTransform;
            if (back != null)
            {
                back.anchorMin = new Vector2(.045f,0); back.anchorMax = new Vector2(.30f,0); back.pivot = new Vector2(.5f,0);
                back.offsetMin = new Vector2(0,64); back.offsetMax = new Vector2(0,124);
            }
            // The stone itself is the reading surface (ivory on it: 12.5:1, 6.1:1 on the lightest speck).
            return nav;
        }
        public RectTransform JournalPage(UIScreen id)
        {
            var body = Page(id, UIStrings.Get("journal"));
            var nav = SectionNavigation(id);
            foreach (var entry in new[] { UIScreen.Map, UIScreen.Objectives, UIScreen.Archive })
            {
                var target = entry;
                var button = _ui.Factory.Button(nav, UIStrings.Get(entry == UIScreen.Map ? "map" : entry == UIScreen.Objectives ? "objectives" : "archive"), () => _ui.Screens.Replace(target));
                var style = button.GetComponent<TitleMenuButton>(); style.tabSelected = entry == id; style.Refresh();
            }
            return body;
        }
        public void Link(Transform parent, string key, UIScreen screen) { _ui.Factory.Button(parent, UIStrings.Get(key), () => _ui.Screens.Show(screen)); }
        public void Refresh(UIScreen screen)
        {
            if (screen == UIScreen.Objectives)
            {
                _objectives.text = UIStrings.Get("objectives.body", _ui.Demo.Objectives.AnalyzedCount, _ui.Demo.Objectives.RequiredCount,
                    UIStrings.Get(_ui.Demo.Combat.Completed ? "done" : "pending"), UIStrings.Get(_ui.Demo.PortalsUnlocked ? "unlocked" : "locked"));
                _finish.gameObject.SetActive(_ui.Demo.PortalsUnlocked);
            }
            if (screen == UIScreen.Complete)
            {
                var played = System.TimeSpan.FromSeconds(_ui.PlaySeconds);
                _statTime.text = played.TotalHours >= 1 ? played.ToString(@"h\:mm\:ss") : played.ToString(@"mm\:ss");
                _statStations.text = _ui.Demo.Objectives.AnalyzedCount + " / " + _ui.Demo.Objectives.RequiredCount;
                _statZones.text = _ui.Map.VisitedWorlds.ToString();
                _statTraining.text = UIStrings.Get(_ui.Demo.Combat.Completed ? "done" : "pending");
            }
        }
    }
}

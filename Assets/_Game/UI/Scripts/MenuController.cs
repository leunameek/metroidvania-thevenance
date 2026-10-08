using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Composition of «Interfaces de El Asedio de Bacatá»:
    //  Sheet  — title and subtitle in the upper-left safe area, optional tab row, framed panel
    //           below, «Esc Volver» and «Enter Seleccionar» in the lower corners (screens 20–26).
    //  Column — the pause: one framed column over the dimmed scene (screen 19).
    //  Modal  — centred framed panel with emblem, title and actions (16, 27, 29).
    //  Bare   — no frame, the illustration or scene carries the page (07 loading, 28 defeat).
    public enum PageKind { Sheet, Column, Modal, Bare }

    public sealed class MenuController
    {
        private readonly UIManager _ui;
        private readonly Transform _parent;
        private readonly Dictionary<UIScreen, RectTransform> _bodies = new Dictionary<UIScreen, RectTransform>();
        private readonly Dictionary<UIScreen, TMP_Text> _titles = new Dictionary<UIScreen, TMP_Text>();
        private readonly Dictionary<UIScreen, TMP_Text> _subtitles = new Dictionary<UIScreen, TMP_Text>();
        private TMP_Text _objectives, _statTime, _statStations, _statZones, _statTraining, _pauseZone;
        private Button _finish;
        public MenuController(UIManager ui, Transform parent)
        {
            _ui = ui; _parent = parent;
            // Application entry lives in the isolated Nemequene_MainMenu scene.
            var pause = Page(UIScreen.Pause, UIStrings.Get("pause"), false, PageKind.Column);
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
            Link(pause, "accessibility", UIScreen.Accessibility);
            ui.Factory.Button(pause, UIStrings.Get("mainMenu"), () => ui.Confirm("confirm.menu", ui.ReturnToMenu));
            ui.Factory.Button(pause, UIStrings.Get("quit"), () => ui.Confirm("confirm.quit", ui.Quit));
            UIFactory.CenterAll(pause);
            var objectives = JournalPage(UIScreen.Objectives);
            _objectives = ui.Factory.Text(objectives, "");
            _finish = ui.Factory.Button(objectives, UIStrings.Get("finish"), () => ui.Screens.Show(UIScreen.Complete), true);
            var controls = Page(UIScreen.Controls, UIStrings.Get("controls"));
            BuildControls(controls);
            var credits = Page(UIScreen.Credits, UIStrings.Get("credits"));
            new CreditsPage(ui.Factory, ui.Settings).Build(credits);
            BuildDefeat();
            BuildComplete();
        }
        // Controls and help (H): the rebindable keys first, then the voice and hands help and the
        // three actions. The key list scrolls; the help text follows the keys the player chose.
        private void BuildControls(RectTransform body)
        {
            var f = _ui.Factory;
            var actions = f.Row(body, "Actions", 18);
            actions.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            f.Button(actions, UIStrings.Get("tutorial.repeat"), _ui.RepeatTutorial);
            Link(actions, "voice.calibrate", UIScreen.VoiceCalibration);
            Link(actions, "hands.calibrate", UIScreen.HandCalibration);
            new ControlsPage(f, _ui.Settings, _ui.Confirm).Build(f.Column(body, "Teclas", 16));
            var help = f.Text(body, "", 21); help.alignment = TextAlignmentOptions.TopLeft;
            help.margin = new Vector4(76, 12, 40, 0);
            System.Action refresh = () => help.text = UIStrings.Get("controls.body");
            refresh(); GameBindings.Changed += refresh;
            help.gameObject.AddComponent<UIDisposeHook>().disposed = () => GameBindings.Changed -= refresh;
        }

        // Screen 28 «Derrota y reintento»: no frame, a crimson title over the dimmed scene, the
        // reassurance that approved lessons are kept, Reintentar first.
        private void BuildDefeat()
        {
            var f = _ui.Factory;
            var defeat = Page(UIScreen.Defeat, UIStrings.Get("defeat"), false, PageKind.Bare);
            var title = UIFactory.Tone(_titles[UIScreen.Defeat], UITone.Danger);
            title.GetComponent<UIStyleBinding>().baseSize = 72; title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.SetAnchors(new Vector2(.2f, .56f), new Vector2(.8f, .70f));
            var scroll = (RectTransform)defeat.parent.parent; scroll.SetAnchors(new Vector2(.34f, .14f), new Vector2(.66f, .55f));
            defeat.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            var quote = UIFactory.Tone(f.Text(defeat, UIStrings.Get("defeat.quote"), 26), UITone.Gold);
            quote.alignment = TextAlignmentOptions.Center; quote.fontStyle = FontStyles.Italic;
            var body = UIFactory.Tone(f.Text(defeat, UIStrings.Get("defeat.body"), 24), UITone.Muted); body.alignment = TextAlignmentOptions.Center;
            f.Button(defeat, UIStrings.Get("restart"), () => _ui.Confirm("confirm.restart", () => _ui.Loading.Restart()), true);
            Link(defeat, "settings", UIScreen.Settings);
            f.Button(defeat, UIStrings.Get("mainMenu"), () => _ui.Confirm("confirm.menu", _ui.ReturnToMenu));
            UIFactory.CenterAll(defeat);
            _parentScreen(UIScreen.Defeat).GetComponent<TitleMenuBackdrop>().tint = UITheme.Hex("12070A");
        }
        // Screen 29 «Fin del recorrido disponible»: check emblem, title and a summary built only
        // from values the slice actually records.
        private void BuildComplete()
        {
            var f = _ui.Factory;
            var complete = Page(UIScreen.Complete, UIStrings.Get("complete"), true, PageKind.Modal);
            var mark = f.Icon(complete, UIIcon.Check, 64, f.Theme.paleGold); mark.transform.SetAsFirstSibling();
            var table = f.Column(complete, "Stats", 4);
            _statTime = StatRow(table, UIIcon.Rotate, "complete.time");
            _statStations = StatRow(table, UIIcon.Objective, "complete.stations");
            _statZones = StatRow(table, UIIcon.Portal, "complete.zones");
            _statTraining = StatRow(table, UIIcon.Shield, "complete.training");
            f.Button(complete, UIStrings.Get("mainMenu"), _ui.ReturnToMenu, true);
            f.Button(complete, UIStrings.Get("replay"), () => _ui.Confirm("confirm.replay", () => _ui.Loading.RestartFresh()));
            Link(complete, "credits", UIScreen.Credits);
            UIFactory.CenterAll(complete);
        }
        private TMP_Text StatRow(Transform parent, UIIcon icon, string key)
        {
            var f = _ui.Factory;
            var row = f.Row(parent, "Stat_" + key, 16);
            var layout = row.GetComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(12, 12, 6, 6);
            layout.childForceExpandWidth = false; layout.childAlignment = TextAnchor.MiddleLeft;
            row.gameObject.AddComponent<LayoutElement>().minHeight = 48;
            f.Icon(row, icon, 28, f.Theme.gold);
            var label = f.Text(row, UIStrings.Get(key), 24); label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var value = UIFactory.Tone(f.Text(row, "", 24), UITone.GoldLight); value.alignment = TextAlignmentOptions.MidlineRight;
            f.Rule(parent);
            return value;
        }
        private RectTransform _parentScreen(UIScreen id) => (RectTransform)_bodies[id].GetComponentInParent<TitleMenuBackdrop>(true).transform;
        public void FinishSetup()
        {
            _ui.Settings.Values.configured = true;
            if (!_ui.Demo.MouseMode) _ui.Demo.ToggleInputMode();
            _ui.Voice.Suspend(); _ui.Hands.Stop(); _ui.Settings.Apply(); _ui.Screens.Back();
        }
        public RectTransform Page(UIScreen id, string title, bool back = true, PageKind kind = PageKind.Sheet)
        {
            var f = _ui.Factory;
            var screen = f.Rect("UI_Screen_" + id, _parent, Vector2.zero, Vector2.one);
            var shade = screen.gameObject.AddComponent<TitleMenuBackdrop>(); shade.raycastTarget = true;
            // Pause keeps the scene visible; reading pages veil it further (document, screen 19).
            shade.even = true; shade.strength = kind == PageKind.Column ? .72f : kind == PageKind.Modal ? .80f : .93f;
            RectTransform panel, body;
            if (kind == PageKind.Sheet)
            {
                _titles[id] = f.Heading(screen, title, new Vector2(.04f, .865f), new Vector2(.70f, .95f), 56);
                _titles[id].alignment = TextAlignmentOptions.BottomLeft;
                _subtitles[id] = Subtitle(screen, "subtitle." + id, new Vector2(.04f, .825f), new Vector2(.70f, .865f), TextAlignmentOptions.TopLeft);
                bool wide = id == UIScreen.Inventory || id == UIScreen.SaveLoad || id == UIScreen.Controls;
                // Calibration keeps the right side for the camera preview (screens 05 and 06).
                bool split = id == UIScreen.VoiceCalibration || id == UIScreen.HandCalibration;
                panel = f.Panel("ReadingSurface", screen, new Vector2(split ? .06f : wide ? .08f : .18f, .10f), new Vector2(split ? .60f : wide ? .92f : .82f, .79f));
                body = f.Scroll(panel, Vector2.zero, Vector2.one);
                ((RectTransform)body.parent.parent).Inset(64, 52, 64, 72);
            }
            else if (kind == PageKind.Column)
            {
                panel = f.Panel("ReadingSurface", screen, new Vector2(.355f, .06f), new Vector2(.645f, .90f));
                _titles[id] = f.Heading(panel, title, new Vector2(.08f, .83f), new Vector2(.92f, .92f), 60, UITone.Danger);
                _subtitles[id] = _pauseZone = Subtitle(panel, "", new Vector2(.08f, .78f), new Vector2(.92f, .83f), TextAlignmentOptions.Center);
                f.Divider(panel, new Vector2(.16f, .75f), new Vector2(.84f, .78f)).name = "HeaderRule";
                body = f.Scroll(panel, new Vector2(.06f, .04f), new Vector2(.94f, .745f));
                body.GetComponent<VerticalLayoutGroup>().spacing = 2;
            }
            else if (kind == PageKind.Modal)
            {
                panel = f.Panel("ReadingSurface", screen, new Vector2(.31f, .08f), new Vector2(.69f, .92f));
                _titles[id] = f.Heading(panel, title, new Vector2(.08f, .80f), new Vector2(.92f, .92f), 48);
                f.Divider(panel, new Vector2(.22f, .77f), new Vector2(.78f, .80f)).name = "HeaderRule";
                body = f.Scroll(panel, new Vector2(.08f, .06f), new Vector2(.92f, .76f));
            }
            else
            {
                panel = f.Rect("ReadingSurface", screen, new Vector2(.04f, .06f), new Vector2(.96f, .94f));
                _titles[id] = f.Heading(screen, title, new Vector2(.05f, .26f), new Vector2(.80f, .36f), 60);
                _titles[id].alignment = TextAlignmentOptions.BottomLeft;
                body = f.Scroll(panel, new Vector2(.01f, .02f), new Vector2(.62f, .24f));
            }
            panel.gameObject.AddComponent<UIPanelTransition>();
            if (back)
            {
                f.FooterButton(screen, UIStrings.Get("footer.back"), () => _ui.Screens.Back(), Vector2.zero).transform.parent.name = "BackRow";
                f.Hint(screen, UIStrings.Get("footer.select"), Vector2.right);
            }
            else if (kind == PageKind.Column)
            {
                f.Hint(screen, UIStrings.Get("footer.resume"), Vector2.zero);
                f.Hint(screen, UIStrings.Get("footer.select"), Vector2.right);
            }
            _bodies[id] = body; _ui.Screens.Register(id, screen.gameObject); return body;
        }
        private TMP_Text Subtitle(Transform parent, string key, Vector2 min, Vector2 max, TextAlignmentOptions alignment)
        {
            string value = key.Length == 0 ? "" : UIStrings.Get(key);
            if (value.StartsWith("[")) value = "";
            var text = UIFactory.Fit(UIFactory.Tone(_ui.Factory.Label(parent, value, min, max, 22), UITone.Muted));
            text.alignment = alignment; return text;
        }
        public void SetSubtitle(UIScreen id, string value) { if (_subtitles.TryGetValue(id, out var text)) text.text = value; }
        public TMP_Text Title(UIScreen id) => _titles[id];
        // Tab row under the title (screens 20 and 24): the active tab wears the crimson ribbon.
        public RectTransform SectionNavigation(UIScreen id)
        {
            var body = _bodies[id]; var panel = (RectTransform)body.parent.parent.parent;
            var screen = (RectTransform)panel.parent;
            panel.anchorMin = new Vector2(.08f, .10f); panel.anchorMax = new Vector2(.92f, .745f);
            var nav = _ui.Factory.Row(screen, "Sections", 12); nav.name = "Sections";
            nav.anchorMin = new Vector2(.04f, .755f); nav.anchorMax = new Vector2(.96f, .815f); nav.offsetMin = nav.offsetMax = Vector2.zero;
            return nav;
        }
        public RectTransform JournalPage(UIScreen id)
        {
            var body = Page(id, UIStrings.Get("journal.title"));
            var nav = SectionNavigation(id);
            foreach (var entry in new[] { UIScreen.Objectives, UIScreen.Map, UIScreen.Archive })
            {
                var target = entry;
                var button = _ui.Factory.Button(nav, UIStrings.Get(entry == UIScreen.Map ? "map" : entry == UIScreen.Objectives ? "objectives" : "archive"), () => _ui.Screens.Replace(target));
                UIFactory.Tab(button);
                var style = button.GetComponent<TitleMenuButton>(); style.tabSelected = entry == id; style.Refresh();
            }
            return body;
        }
        public void Link(Transform parent, string key, UIScreen screen) { _ui.Factory.Button(parent, UIStrings.Get(key), () => _ui.Screens.Show(screen)); }
        public void Refresh(UIScreen screen)
        {
            if (screen == UIScreen.Pause && _pauseZone != null)
                _pauseZone.text = UIStrings.Get(_ui.Demo.World < 0 ? "world.lower" : _ui.Demo.World > 0 ? "world.upper" : "world.plaza");
            if (screen == UIScreen.Objectives)
            {
                var story = CampaignProgress.Objective;
                _objectives.text = UIStrings.Get("objectives.story", story.Title.ToUpperInvariant(), story.Text) + "\n\n" + UIStrings.Get("objectives.body", _ui.Demo.Objectives.AnalyzedCount, _ui.Demo.Objectives.RequiredCount,
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

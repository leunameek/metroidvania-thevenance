using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class SaveLoadUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly TMP_Text[] _labels = new TMP_Text[GameSaveStore.SlotCount];
        private readonly Button[] _load = new Button[GameSaveStore.SlotCount];
        private readonly Button[] _delete = new Button[GameSaveStore.SlotCount];

        public SaveLoadUIController(UIManager ui, MenuController menu)
        {
            _ui = ui;
            var body = menu.Page(UIScreen.SaveLoad, UIStrings.Get("save.title"));
            ui.Factory.Text(body, UIStrings.Get("save.checkpointNote"), 22);
            for (int i = 0; i < GameSaveStore.SlotCount; i++)
            {
                int slot = i;
                var card = ui.Factory.Column(body, "SaveSlot_" + (i+1), 8);
                card.gameObject.AddComponent<Image>().color=ui.Theme.panel;
                ui.Factory.Frame(card,false);
                card.GetComponent<VerticalLayoutGroup>().padding=new RectOffset(28,28,24,24);
                var element = card.gameObject.AddComponent<LayoutElement>(); element.preferredHeight = 216;
                _labels[i] = ui.Factory.Text(card, "", 22);
                var actions = ui.Factory.Rect("Actions", card, Vector2.zero, Vector2.one);
                actions.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
                var row = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
                row.spacing = 8; row.childControlWidth = row.childControlHeight = true;
                row.childForceExpandWidth = row.childForceExpandHeight = true;
                ui.Factory.Button(actions, UIStrings.Get("save.game"), () => Save(slot));
                _load[i] = ui.Factory.Button(actions, UIStrings.Get("save.load"), () => Load(slot));
                _delete[i] = ui.Factory.Button(actions, UIStrings.Get("save.delete"), () => Delete(slot));
                // Destructive action in ceremonial red, as in the reference button set.
                _delete[i].GetComponent<TitleMenuButton>().danger = true;
                UIFactory.CenterAll(actions);
            }
            ui.Screens.Changed += OnScreen;
            Refresh();
        }
        private void OnScreen(UIScreen screen) { if (screen == UIScreen.SaveLoad) Refresh(); }
        private void Refresh()
        {
            for (int i = 0; i < GameSaveStore.SlotCount; i++)
            {
                bool valid = GameSaveStore.TryRead(i, out var data);
                _labels[i].text = valid
                    ? UIStrings.Get("save.slot", i+1, data.checkpoint, data.CompletionPercent,
                        TimeSpan.FromSeconds(data.playSeconds).ToString(@"hh\:mm"),
                        DateTime.Parse(data.savedAtUtc).ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
                    : GameSaveStore.IsOccupied(i) ? UIStrings.Get("save.corrupt", i+1) : UIStrings.Get("save.empty", i+1);
                _load[i].interactable = valid;
                _delete[i].interactable = GameSaveStore.IsOccupied(i);
                _load[i].GetComponent<TitleMenuButton>().Refresh();
                _delete[i].GetComponent<TitleMenuButton>().Refresh();
            }
        }
        private void Save(int slot)
        {
            void Write()
            {
                GameSaveStore.Begin(slot, false);
                if (_ui.SaveCurrent()) _ui.Notifications.Post(UIStrings.Get("save.saved"));
                Refresh();
            }
            if (GameSaveStore.IsOccupied(slot)) _ui.Confirm("save.replaceConfirm", Write);
            else Write();
        }
        private void Load(int slot)
        {
            if (!GameSaveStore.TryRead(slot, out _)) return;
            _ui.Confirm("save.loadConfirm", () =>
            {
                GameSaveStore.Begin(slot, true);
                _ui.Loading.Restart();
            });
        }
        private void Delete(int slot)
        {
            if (!GameSaveStore.IsOccupied(slot)) return;
            _ui.Confirm("save.deleteConfirm", () => { GameSaveStore.Delete(slot); Refresh(); });
        }
        public void Dispose() { _ui.Screens.Changed -= OnScreen; }
    }
}

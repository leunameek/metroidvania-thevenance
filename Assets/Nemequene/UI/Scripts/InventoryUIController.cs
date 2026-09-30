using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Completed inspections produce actual collected records in the current slice.
    public sealed class InventoryUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly Button[] _slots;
        private readonly UIArtifactGlyph[] _glyphs;
        private readonly UIArtifactGlyph _preview;
        private readonly TMP_Text _details;
        private int _selected = -1;

        public InventoryUIController(UIManager ui, MenuController menu)
        {
            _ui = ui;
            var body = menu.Page(UIScreen.Inventory, UIStrings.Get("inventory"));
            ui.Factory.Text(body, UIStrings.Get("inventory.intro"), 22);
            var workspace = ui.Factory.Rect("InventoryWorkspace", body, Vector2.zero, Vector2.one);
            workspace.gameObject.AddComponent<LayoutElement>().preferredHeight = 500;
            var columns = workspace.gameObject.AddComponent<HorizontalLayoutGroup>();
            columns.spacing = 24; columns.childControlWidth = columns.childControlHeight = true;
            columns.childForceExpandWidth = false; columns.childForceExpandHeight = true;
            var grid = ui.Factory.HudPanel("ArtifactSlots", workspace, Vector2.zero, Vector2.one);
            var gridSize = grid.gameObject.AddComponent<LayoutElement>(); gridSize.preferredWidth = 3 * 210 + 2 * 16 + 48; gridSize.flexibleWidth = 0;
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(210,128); layout.spacing = new Vector2(16,16);
            layout.padding = new RectOffset(24,24,24,24); layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 3;
            _slots = new Button[ui.Demo.Objects.Length];
            _glyphs = new UIArtifactGlyph[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
            {
                int index = i;
                _slots[i] = ui.Factory.Button(grid, "", () => Select(index));
                ConfigureSlot(ui, _slots[i], i, out _glyphs[i]);
            }
            for (int i = _slots.Length; i < 9; i++)
            {
                var locked = ui.Factory.Button(grid, UIStrings.Get("inventory.locked"), () => { });
                ConfigureSlot(ui, locked, i, out var glyph); glyph.SetState(i,false,false);
                locked.interactable = false; locked.GetComponent<TitleMenuButton>().Refresh();
            }
            var detail = ui.Factory.HudPanel("ArtifactDetail", workspace, Vector2.zero, Vector2.one);
            detail.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var icon = ui.Factory.Rect("ArtifactEmblem", detail, new Vector2(.32f,.60f), new Vector2(.68f,.88f));
            _preview = icon.gameObject.AddComponent<UIArtifactGlyph>(); _preview.raycastTarget = false;
            _details = ui.Factory.Label(detail, "", new Vector2(.08f,.10f), new Vector2(.92f,.60f), 24);
            ui.Screens.Changed += OnScreen;
            ui.Demo.Objectives.Changed += Refresh;
            Refresh();
        }

        private void OnScreen(UIScreen screen) { if (screen == UIScreen.Inventory) Refresh(); }
        private static void ConfigureSlot(UIManager ui, Button button, int index, out UIArtifactGlyph glyph)
        {
            button.GetComponent<HorizontalLayoutGroup>().enabled = false;
            var label = button.GetComponentInChildren<TMP_Text>();
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = new Vector2(.06f,.04f);
            label.rectTransform.anchorMax = new Vector2(.94f,.28f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            label.GetComponent<UIStyleBinding>().baseSize = 18;
            label.fontSize = 18;
            var mark = ui.Factory.Rect("ArtifactGlyph",button.transform,new Vector2(.22f,.34f),new Vector2(.78f,.88f));
            glyph = mark.gameObject.AddComponent<UIArtifactGlyph>(); glyph.design = index; glyph.raycastTarget = false;
        }
        private void Select(int index) { _selected = index; Refresh(); }
        private void Refresh()
        {
            int first = -1;
            for (int i = 0; i < _slots.Length; i++)
                if (_ui.Demo.Objects[i] != null && _ui.Demo.Objects[i].Completed) { first = i; break; }
            if (_selected < 0 || _selected >= _slots.Length ||
                _ui.Demo.Objects[_selected] == null || !_ui.Demo.Objects[_selected].Completed) _selected = first;
            for (int i = 0; i < _slots.Length; i++)
            {
                var item = _ui.Demo.Objects[i];
                bool obtained = item != null && item.Completed;
                _slots[i].interactable = obtained;
                _slots[i].GetComponentInChildren<TMP_Text>().text = obtained
                    ? item.Data.displayName + "  ×1" : UIStrings.Get("inventory.unknown");
                _glyphs[i].onGold = obtained && _selected == i;
                _glyphs[i].SetState(i,obtained,obtained && _selected == i);
                var style = _slots[i].GetComponent<TitleMenuButton>();
                style.tabSelected = obtained && _selected == i; style.Refresh();
            }
            _preview.gameObject.SetActive(_selected >= 0);
            if (_selected < 0) _details.text = UIStrings.Get("inventory.empty");
            else
            {
                var item = _ui.Demo.Objects[_selected];
                _preview.SetState(_selected,true,true);
                _details.text = item.Data.displayName + "\n\n" + InspectionUIController.CulturalDescription(item.Data)
                    + "\n\n" + UIStrings.Get("inventory.use");
            }
        }
        public void Dispose()
        {
            _ui.Screens.Changed -= OnScreen;
            if (_ui.Demo != null) _ui.Demo.Objectives.Changed -= Refresh;
        }
    }
}

using System;
using TMPro;

namespace Nemequene.UI
{
    public sealed class PoporoUIController : IDisposable
    {
        private readonly TMP_Text _content;
        private IPoporoInventory _inventory;
        public PoporoUIController(UIManager ui, MenuController menu)
        { _content = ui.Factory.Text(menu.Page(UIScreen.Poporo, UIStrings.Get("poporo")), UIStrings.Get("poporo.empty")); }
        public void Bind(IPoporoInventory inventory)
        {
            if (_inventory != null) _inventory.Changed -= Refresh;
            _inventory = inventory; if (_inventory != null) _inventory.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            string text = "";
            if (_inventory != null) foreach (var resource in _inventory.Resources)
                text += UIStrings.Get(resource.nameKey) + " · " + _inventory.Quantity(resource.id) + "\n" + UIStrings.Get(resource.descriptionKey)
                    + "\n" + UIStrings.Get(_inventory.IsAvailable(resource.id) ? "available" : "locked") + "\n\n";
            _content.text = text.Length == 0 ? UIStrings.Get("poporo.empty") : text;
        }
        public void Dispose() { if (_inventory != null) _inventory.Changed -= Refresh; }
    }
}

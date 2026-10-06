using UnityEngine;

namespace Nemequene.UI
{
    [CreateAssetMenu(menuName = "Nemequene/UI/Poporo resource")]
    public sealed class PoporoResource : ScriptableObject
    {
        public string id, nameKey, descriptionKey;
        public Sprite icon;
    }
    public interface IPoporoInventory
    {
        event System.Action Changed;
        System.Collections.Generic.IEnumerable<PoporoResource> Resources { get; }
        int Quantity(string id);
        bool IsAvailable(string id);
    }
}

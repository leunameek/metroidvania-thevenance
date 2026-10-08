using System;
using UnityEngine;

namespace Nemequene.UI
{
    // Unsubscribes a page built in code from static events when its menu is destroyed.
    public sealed class UIDisposeHook : MonoBehaviour
    {
        public Action disposed;
        private void OnDestroy() => disposed?.Invoke();
    }
}

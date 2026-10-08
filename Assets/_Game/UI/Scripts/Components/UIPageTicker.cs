using System;
using UnityEngine;

namespace Nemequene.UI
{
    // Per-frame work of a settings section built in code (display countdown, key capture, device
    // meters). It lives on the section, so it only runs while that tab is visible.
    public sealed class UIPageTicker : MonoBehaviour
    {
        public Action tick, shown, hidden;
        private void OnEnable() => shown?.Invoke();
        private void OnDisable() => hidden?.Invoke();
        private void Update() => tick?.Invoke();
    }
}

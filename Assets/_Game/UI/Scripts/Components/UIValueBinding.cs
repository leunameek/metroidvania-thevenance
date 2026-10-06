using System;
using UnityEngine;
namespace Nemequene.UI
{
    public sealed class UIValueBinding : MonoBehaviour
    {
        private SettingsManager _settings;
        private Action _refresh;
        public void Bind(SettingsManager settings, Action refresh)
        { _settings = settings; _refresh = refresh; _settings.Changed += _refresh; }
        private void OnDestroy() { if (_settings != null) _settings.Changed -= _refresh; }
    }
}

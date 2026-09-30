using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public sealed class LoadingScreenController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly TMP_Text _status;
        private readonly Image _progress;
        private readonly Button _return;
        private bool _loading, _preview;
        private int _width, _height;
        private FullScreenMode _mode;
        private float _deadline;
        public LoadingScreenController(UIManager ui, MenuController menu)
        {
            _ui = ui; var body = menu.Page(UIScreen.Loading, UIStrings.Get("world.plaza"), false);
            // Reference block 4 «Pantalla de carga»: status in carved capitals, gold progress rail, tip.
            _status = ui.Factory.Caption(body, UIStrings.Get("loading"), 28);
            var host = ui.Factory.Rect("Progress", body, Vector2.zero, Vector2.one); host.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
            _progress = ui.Factory.Bar(host, "SceneProgress", Vector2.zero, Vector2.one);
            ui.Factory.Caption(body, UIStrings.Get("loading.tipTitle"), 20);
            UIFactory.Tone(ui.Factory.Text(body, UIStrings.Get("loading.tip"), 22), UITone.Muted);
            _return = ui.Factory.Button(body, UIStrings.Get("mainMenu"), () => { if (!_loading) ui.ReturnToMenu(); });
            _return.gameObject.SetActive(false);
        }
        public void Restart() { RestartInternal(true); }
        public void RestartFresh() { RestartInternal(false); }
        private void RestartInternal(bool fromCheckpoint)
        {
            if (_loading || _ui.Demo.State == TechnicalDemoState.Transition) return;
            if (fromCheckpoint && !GameSaveStore.LoadOnNextScene && GameSaveStore.TryRead(GameSaveStore.ActiveSlot, out _))
                GameSaveStore.Begin(GameSaveStore.ActiveSlot, true);
            if (!fromCheckpoint && GameSaveStore.ActiveSlot >= 0) GameSaveStore.Begin(GameSaveStore.ActiveSlot, false);
            string path = _ui.Demo.gameObject.scene.path;
            if (!Application.CanStreamedLevelBeLoaded(path)) { _ui.Screens.Show(UIScreen.Loading, false); ShowError(); return; }
            _ui.StartCoroutine(Load(path));
        }
        public void ReturnToTitle()
        {
            if (_loading) return;
            if (!Application.CanStreamedLevelBeLoaded(TitleMenuController.ScenePath))
            { _ui.Screens.Show(UIScreen.Loading,false); ShowError(); return; }
            _ui.StartCoroutine(Load(TitleMenuController.ScenePath,false));
        }
        private IEnumerator Load(string path, bool gameplay = true)
        {
            _loading = true; _ui.Voice.Suspend(); _ui.Hands.Stop();
            _return.gameObject.SetActive(false);
            _ui.Screens.Show(UIScreen.Loading, false);
            yield return null;
            AsyncOperation operation = null;
            try { operation = SceneManager.LoadSceneAsync(path); }
            catch (Exception) { _status.text = UIStrings.Get("loading.error"); }
            if (operation == null) { ShowError(); yield break; }
            UIManager.EnterGameplayOnLoad = gameplay;
            while (!operation.isDone)
            { UIFactory.Fill(_progress, operation.progress); _status.text = UIStrings.Get(gameplay ? "loading" : "title.returning"); yield return null; }
        }
        private void ShowError()
        {
            GameSaveStore.ClearPendingLoad();
            _loading = false; _status.text = UIStrings.Get("loading.error"); _return.gameObject.SetActive(true);
            _ui.Screens.SelectDefault();
        }
        public void PreviewResolution(int width, int height, bool full)
        {
            if (_preview) return;
            _width = Screen.width; _height = Screen.height; _mode = Screen.fullScreenMode;
            Screen.SetResolution(width, height, full ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            _preview = true; _deadline = Time.unscaledTime + 15;
            _ui.Confirm("graphics.keep", () =>
            {
                _preview = false; _ui.Settings.Values.screenWidth = width; _ui.Settings.Values.screenHeight = height;
                _ui.Settings.Values.fullscreen = full; _ui.Settings.Apply(); _ui.Settings.Flush();
            });
        }
        public void Tick()
        {
            if (!_preview) return;
            if (Time.unscaledTime >= _deadline || !_ui.ModalOpen)
            { Screen.SetResolution(_width, _height, _mode); _preview = false; if (_ui.ModalOpen) _ui.CloseConfirmation(); }
        }
        public void Dispose()
        {
            if (!_preview) return;
            Screen.SetResolution(_width, _height, _mode);
            _preview = false;
        }
    }
}

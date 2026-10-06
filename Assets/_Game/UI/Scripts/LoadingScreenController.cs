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
            _ui = ui; var f = ui.Factory;
            var body = menu.Page(UIScreen.Loading, UIStrings.Get("world.plaza"), false, PageKind.Bare);
            // Screen 07 «Carga»: the destination's own illustration, its name in the monumental
            // serif, a short tip and a thin progress rail. No percentage: progress is the real
            // scene operation only.
            var screen = body.GetComponentInParent<TitleMenuBackdrop>(true).transform;
            var texture = UIBacata.Art("Plaza_Nunez");
            if (texture != null)
            {
                var art = f.Rect("Illustration", screen, Vector2.zero, Vector2.one); art.SetAsFirstSibling();
                var raw = art.gameObject.AddComponent<RawImage>(); raw.texture = texture; raw.raycastTarget = false;
                var fit = art.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = (float)texture.width / texture.height;
                var veil = f.Rect("Veil", screen, Vector2.zero, Vector2.one); veil.SetSiblingIndex(1);
                var shade = veil.gameObject.AddComponent<TitleMenuBackdrop>(); shade.raycastTarget = false; shade.readingEdge = .42f;
                screen.GetComponent<TitleMenuBackdrop>().strength = 0;
            }
            menu.Title(UIScreen.Loading).GetComponent<UIStyleBinding>().baseSize = 64;
            UIFactory.Tone(f.Text(body, UIStrings.Get("loading.tip"), 24), UITone.Muted);
            var host = f.Rect("Progress", body, Vector2.zero, Vector2.one); host.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            _progress = f.Bar(host, "SceneProgress", Vector2.zero, Vector2.one);
            _status = UIFactory.Tone(f.Text(body, UIStrings.Get("loading"), 22), UITone.GoldLight);
            _status.alignment = TextAlignmentOptions.MidlineRight;
            _return = f.Button(body, UIStrings.Get("mainMenu"), () => { if (!_loading) ui.ReturnToMenu(); });
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

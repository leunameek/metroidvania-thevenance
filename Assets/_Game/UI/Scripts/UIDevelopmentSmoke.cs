#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // Opt-in diagnostic for the development Windows build. Never runs in ordinary play.
    public sealed class UIDevelopmentSmoke : MonoBehaviour
    {
        private int _errors;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Array.Exists(Environment.GetCommandLineArgs(), arg => arg == "--nemequene-ui-smoke")) return;
            var host=new GameObject("UI_DevelopmentSmoke"); DontDestroyOnLoad(host); host.AddComponent<UIDevelopmentSmoke>();
        }
        private IEnumerator Start()
        {
            Application.logMessageReceived += Log;
            yield return new WaitForSecondsRealtime(1);
            var title=TitleMenuController.Instance;
            if(title==null||title.CurrentView!=TitleView.Home||UIManager.Instance!=null||title.Music.clip==null){Finish(false);yield break;}
            string evidence = Path.GetFullPath(Path.Combine(Application.dataPath,"../Evidence")); Directory.CreateDirectory(evidence);
            Capture(title.Canvas, Path.Combine(evidence,"windows-main-menu.png"));
            title.OpenSettings();yield return new WaitForSecondsRealtime(.2f);
            bool settings=title.CurrentView==TitleView.Settings;
            title.ShowHome();yield return null;title.NewGame();
            float deadline=Time.unscaledTime+25;
            while(UIManager.Instance==null&&Time.unscaledTime<deadline)yield return null;
            var ui = UIManager.Instance;
            if (ui == null || ui.Canvas == null || ui.Theme.bodyFont == null || ui.Theme.titleFont == null) { Finish(false); yield break; }
            if (ui.Demo.Hands.Requested || ui.Voice.Listening) { Finish(false); yield break; }
            yield return new WaitForSecondsRealtime(.5f);
            bool running = ui.Screens.Current == UIScreen.None && !ui.Demo.HelpOpen && Time.timeScale > 0;
            ui.Screens.Show(UIScreen.Pause); yield return new WaitForSecondsRealtime(.1f);
            bool paused = ui.Demo.HelpOpen && Time.timeScale == 0;
            ui.ReturnToMenu();deadline=Time.unscaledTime+25;
            while(TitleMenuController.Instance==null&&Time.unscaledTime<deadline)yield return null;
            bool returned=TitleMenuController.Instance!=null&&UIManager.Instance==null;
            bool passed=settings&&running&&paused&&returned&&_errors==0;
            File.WriteAllText(Path.Combine(evidence,"windows-smoke.txt"),"Isolated title scene, music, settings, direct new game and return\nConfiguration: "+settings+"\nExploration: "+running+"\nPause: "+paused+"\nReturn to title: "+returned+"\nErrors: "+_errors+"\nPassed: "+passed);
            Finish(passed);
        }
        private static void Capture(Canvas canvas, string path)
        {
            // Batch players have no presented swap chain. Render the actual camera and Canvas explicitly.
            var camera = Camera.main; var scaler = canvas.GetComponent<CanvasScaler>();
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            var target = new RenderTexture(1920,1080,24);
            var texture = new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try
            {
                scaler.enabled = false; canvas.scaleFactor = 1;
                camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera; canvas.planeDistance = .3f;
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0,0,1920,1080),0,0); texture.Apply();
                File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive; camera.targetTexture = previousTarget;
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; scaler.enabled = true;
                Destroy(texture); target.Release(); Destroy(target);
            }
        }
        private void Log(string message,string trace,LogType type)
        { if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert) _errors++; }
        private void Finish(bool passed)
        {
            Application.logMessageReceived -= Log;
            Debug.Log(passed ? "NEMEQUENE_WINDOWS_SMOKE_PASS" : "NEMEQUENE_WINDOWS_SMOKE_FAIL");
            Application.Quit(passed?0:1);
        }
    }
}
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Nemequene.UI.Editor
{
    [InitializeOnLoad]
    public static class UIValidation
    {
        private const string Pending = "Nemequene.UI.Validation.Pending";
        private const string Evidence = "EssentialUIEvidence";
        private static int _checks, _errors, _editorErrors;
        private static Keyboard _keyboard;
        private static Mouse _mouse;
        private static Key[] _keys = Array.Empty<Key>();
        private static MouseState _mouseState;
        private static readonly List<string> Results = new List<string>();
        static UIValidation() { if (SessionState.GetBool(Pending,false)) EditorApplication.update += Wait; }
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run validation in a separate batch project.");
            UIAssetBuilder.Build(); TitleMenuSceneBuilder.Build();
            SessionState.SetString("Nemequene.UI.TestSettings", PlayerPrefs.GetString(SettingsManager.StorageKey, ""));
            PlayerPrefs.DeleteKey(SettingsManager.StorageKey);
            EditorSceneManager.OpenScene("Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity");
            SessionState.SetBool(Pending, true); EditorApplication.update += Wait;
            EditorApplication.EnterPlaymode();
        }
        private static void Wait()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            EditorApplication.update -= Wait;
            Application.logMessageReceived += Log;
            var driver = new GameObject("UI_ValidationDriver").AddComponent<UIValidationDriver>();
            Object.DontDestroyOnLoad(driver); driver.Run(Exercise(), Finish);
        }
        private static void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            // Report editor search-index failures separately; never hide them as successful runtime checks.
            if (trace != null && trace.Contains("UnityEditor.Search.SearchDatabase")) { _editorErrors++; return; }
            _errors++;
        }
        private static void Check(bool condition, string label)
        {
            if (!condition) throw new Exception("UI_CHECK_FAIL: " + label);
            _checks++; Results.Add(label); Debug.Log("UI_CHECK_PASS: " + label);
        }
        private static void Input()
        {
            if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(_keys)); InputSystem.QueueStateEvent(_mouse, _mouseState);
        }
        private static IEnumerator Press(Key key)
        { _keys = new[]{key}; yield return new WaitForSecondsRealtime(.08f); _keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.12f); }
        private static IEnumerator Exercise()
        {
            yield return new WaitForSecondsRealtime(.5f);
            _keyboard = InputSystem.AddDevice<Keyboard>(); _mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onBeforeUpdate += Input;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            var ui = UIManager.Instance; Check(ui != null && ui.Theme != null, "UI root and fonts loaded");
            string currentSettings=PlayerPrefs.GetString(SettingsManager.StorageKey,"");
            PlayerPrefs.SetString(SettingsManager.StorageKey,"{\"version\":1,\"textScale\":1.25,\"master\":0.6}");
            var migrated=new SettingsManager().Values;
            Check(Mathf.Approximately(migrated.menuMusic,.3f)&&Mathf.Approximately(migrated.textScale,1.25f)&&Mathf.Approximately(migrated.master,.6f),"older preferences preserve accessibility and receive the menu music default");
            if(currentSettings.Length==0)PlayerPrefs.DeleteKey(SettingsManager.StorageKey);else PlayerPrefs.SetString(SettingsManager.StorageKey,currentSettings);
            Check(ui.Screens.Current == UIScreen.None && Time.timeScale == 1, "tutorial scene starts directly in gameplay");
            Check(!ui.Demo.Hands.Requested && !ui.Voice.Listening, "devices opt in");
            ui.Settings.Values.tutorials = false;
            yield return new WaitForSecondsRealtime(8.5f);
            Check(!ui.transform.Find("UI_HUD_Exploration/Vitality").gameObject.activeSelf, "full health is hidden outside combat");
            Check(ui.transform.Find("UI_HUD_Exploration/Objective").gameObject.activeSelf, "one main objective stays in the upper-right header");
            Check(!ui.GetComponentsInChildren<TMP_Text>().Any(t => t.text.Contains("WASD")), "exploration has no permanent control legend");
            Check(!Object.FindObjectsByType<PlazaWorldLabel>(FindObjectsSortMode.None).Any(t=>t.GetComponent<Renderer>().enabled), "floating world labels are hidden in managed UI");
            foreach (var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1920,1200),new Vector2Int(2560,1440)})
                Capture(ui,"quiet-"+size.x+"x"+size.y,size.x,size.y);
            ui.Demo.PlayerHealth.TakeDamage(10); yield return new WaitForSecondsRealtime(.2f);
            Check(ui.transform.Find("UI_HUD_Exploration/Vitality").gameObject.activeSelf, "damage makes health visible");
            ui.Demo.PlayerHealth.Revive();
            ui.Settings.Values.configured = true; ui.Settings.Apply(); ui.Screens.Show(UIScreen.Pause,false);
            Check(Time.timeScale==0&&ui.Demo.HelpOpen,"pause is separate from application title");
            Check(ui.GetComponentsInChildren<Button>().Length == 8, "pause exposes journal, inventory and save controls");
            Capture(ui,"pause",1920,1080);
            ui.Settings.Values.textScale=1.5f; ui.Settings.Apply();
            Capture(ui,"pause-150-720p",1280,720);
            ui.Settings.Values.textScale=1; ui.Settings.Apply();
            ui.Screens.Show(UIScreen.Map); ui.Screens.Replace(UIScreen.Objectives); ui.Screens.Replace(UIScreen.Archive); ui.Screens.Back();
            Check(ui.Screens.Current == UIScreen.Pause, "journal tabs preserve return to pause");
            foreach (UIScreen screen in Enum.GetValues(typeof(UIScreen)))
            {
                if (screen == UIScreen.None || screen == UIScreen.Start || screen == UIScreen.FirstRun || screen == UIScreen.MainMenu || screen == UIScreen.Loading || screen==UIScreen.Pause) continue;
                ui.Screens.Show(screen); yield return null;
                Check(ui.Screens.Current == screen && EventSystem.current.currentSelectedGameObject != null, "screen and keyboard focus: " + screen);
                Capture(ui,"menu-"+screen,1920,1080);
                ui.Screens.Back(); Check(ui.Screens.Current == UIScreen.Pause, "back returns from " + screen);
            }
            ui.Screens.Show(UIScreen.Accessibility); ui.Settings.Values.textScale = 1.5f; ui.Settings.Values.highContrast = true; ui.Settings.Values.reducedMotion = true; ui.Settings.Apply();
            yield return null; Capture(ui,"accessibility-150-720p",1280,720); Capture(ui,"accessibility-150-1610",1920,1200);
            Check(new SettingsManager().Values.textScale == 1.5f, "settings survive reload");
            var selected = EventSystem.current.currentSelectedGameObject; yield return Press(Key.Tab);
            Check(EventSystem.current.currentSelectedGameObject != selected, "Tab advances focus");
            ui.Settings.Values.textScale=1; ui.Settings.Values.highContrast=false; ui.Settings.Values.reducedMotion=false; ui.Settings.Apply();
            ui.Settings.Values.reactionScale=2; ui.Settings.Apply();
            Check(Mathf.Approximately(ui.Demo.Combat.Model.ReactionSeconds,4.8f),"accessibility changes real combat timing");
            ui.Settings.Values.reactionScale=1; ui.Settings.Apply();
            ui.StartSession(); yield return new WaitForSecondsRealtime(.3f);
            Check(Time.timeScale == 1 && !ui.Demo.HelpOpen && !ui.Demo.Player.InputLocked,"session resumes world");
            var start = ui.Demo.Player.transform.position; _keys = new[]{Key.W}; yield return new WaitForSecondsRealtime(.3f); _keys=Array.Empty<Key>();
            Check(Vector3.Distance(start,ui.Demo.Player.transform.position)> .5f,"movement preserved");
            ui.RepeatTutorial(); yield return new WaitForSecondsRealtime(.3f);
            Check(ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==UIStrings.Get("tutorial.step.0")),"repeat tutorial returns to its first instruction");
            var dialogue = ScriptableObject.CreateInstance<DialogueData>();
            dialogue.lines = new[]{new DialogueData.Line{speakerKey="controls",textKey="controls.body"}};
            ui.Settings.Values.textScale=1.5f; ui.Settings.Values.dialogueInstant=false; ui.Settings.Apply();
            ui.Dialogue.Show(dialogue); yield return new WaitForSecondsRealtime(.15f);
            Check(ui.Dialogue.Active && ui.Demo.HelpOpen,"dialogue pauses gameplay");
            var dialogueText=ui.transform.Find("UI_Dialogue").GetComponentsInChildren<TMP_Text>().First(t=>t.overflowMode==TextOverflowModes.Page);
            Check(dialogueText.maxVisibleCharacters < dialogueText.textInfo.characterCount,"dialogue reveals text at configured speed");
            ui.Dialogue.Next(); yield return null;
            Check(dialogueText.maxVisibleCharacters>=dialogueText.textInfo.pageInfo[dialogueText.pageToDisplay-1].lastCharacterIndex,"advance first reveals the current page");
            Capture(ui,"dialogue-validation-fixture",1920,1080);
            ui.Dialogue.Close(); Check(!ui.Demo.HelpOpen && Time.timeScale==1,"dialogue restores gameplay");
            ui.Settings.Values.dialogueInstant=true; ui.Settings.Values.dialogueAuto=true; ui.Settings.Apply();
            dialogue.lines[0].textKey="done"; ui.Dialogue.Show(dialogue);
            yield return new WaitForSecondsRealtime(2.3f);
            Check(!ui.Dialogue.Active,"automatic dialogue advances after reading time");
            Object.Destroy(dialogue);
            ui.Subtitles.Show("",string.Join(" ",Enumerable.Repeat(UIStrings.Get("controls.body"),4)),2);
            var subtitleText=ui.transform.Find("UI_Subtitle").GetComponentInChildren<TMP_Text>();
            Check(subtitleText.textInfo.pageCount>1,"long subtitles paginate");
            ui.Settings.Values.subtitleScale=1.25f; ui.Settings.Apply(); yield return null;
            Check(Mathf.Approximately(subtitleText.fontSize,35),"subtitle size remains independent from general text size");
            Check(Enumerable.Range(0,subtitleText.textInfo.pageCount).All(p=>
                subtitleText.textInfo.characterInfo[subtitleText.textInfo.pageInfo[p].lastCharacterIndex].lineNumber-
                subtitleText.textInfo.characterInfo[subtitleText.textInfo.pageInfo[p].firstCharacterIndex].lineNumber+1<=3),"subtitle pages contain at most three lines");
            ui.Subtitles.Show("",UIStrings.Get("done"),2);
            ui.Settings.Values.textScale=1; ui.Settings.Values.subtitleScale=1; ui.Settings.Values.dialogueAuto=false; ui.Settings.Apply();
            ui.Screens.Show(UIScreen.Pause); var paused = ui.Demo.Player.transform.position;
            _keys = new[]{Key.W,Key.E}; yield return new WaitForSecondsRealtime(.2f); _keys=Array.Empty<Key>();
            Check(Vector3.Distance(paused,ui.Demo.Player.transform.position)<.01f,"pause locks movement");
            ui.Confirm("confirm.restart",()=>throw new Exception("Cancel dispatched destructive action")); ui.CloseConfirmation();
            Check(ui.Demo.HelpOpen && Time.timeScale==0,"confirmation cancellation keeps parent paused");
            ui.Screens.Show(UIScreen.None,false); yield return null;
            Capture(ui,"exploration",1920,1080);
            foreach(var item in ui.Demo.Objects)
            {
                ui.Demo.BeginAnalysis(item); yield return new WaitForSecondsRealtime(.6f);
                Capture(ui,"inspection-active-"+item.Data.objectId,1920,1080);
                ui.Settings.Values.textScale=1.5f; ui.Settings.Apply(); yield return new WaitForSecondsRealtime(.15f);
                Capture(ui,"inspection-active-150-"+item.Data.objectId,1280,720);
                ui.Settings.Values.textScale=1; ui.Settings.Apply();
                _mouseState = new MouseState{buttons=1,delta=new Vector2(14,14)}; yield return new WaitForSecondsRealtime(.65f);
                _mouseState=new MouseState(); yield return new WaitForSecondsRealtime(.8f);
                Check(ui.Demo.Lesson.Complete,"inspection completes with real mouse adapter: "+item.Data.name);
                Capture(ui,"inspection-"+item.Data.objectId,1920,1080);
                ui.Demo.EndAnalysis(); yield return new WaitForSecondsRealtime(.1f);
            }
            Check(ui.Demo.Objectives.AnalyzedCount==3,"three discoveries integrated");
            ui.Screens.Show(UIScreen.Map); yield return null;
            Check(ui.Demo.Objects.All(item=>ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains(item.Data.displayName))),"map includes every completed discovery");
            Capture(ui,"map",1920,1080); ui.Screens.Back();
            ui.Screens.Show(UIScreen.Archive); yield return null; Capture(ui,"archive",1920,1080); ui.Screens.Back();
            ui.Demo.Combat.Begin(); yield return new WaitForSecondsRealtime(.1f); Capture(ui,"combat",1920,1080);
            Check(ui.transform.Find("UI_HUD_Combat").GetComponentsInChildren<Button>().Length==1,"attack turn only shows attack");
            ui.Screens.Show(UIScreen.Pause); ui.Screens.Back();
            Check(EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.transform.IsChildOf(ui.transform.Find("UI_HUD_Combat")), "resume restores combat keyboard focus");
            ui.Demo.Combat.Attack(); yield return new WaitForSecondsRealtime(.2f);
            ui.Screens.Show(UIScreen.Pause); float remaining=ui.Demo.Combat.Model.Remaining;
            yield return new WaitForSecondsRealtime(.3f);
            Check(Mathf.Approximately(remaining,ui.Demo.Combat.Model.Remaining),"pause freezes reaction timer");
            ui.Screens.Back();
            for(int i=0;i<2;i++)
            {
                float deadline=Time.unscaledTime+5;
                while(ui.Demo.Combat.Model.Phase!=PlazaCombatPhase.React && Time.unscaledTime<deadline) yield return null;
                Check(ui.Demo.Combat.Model.Phase==PlazaCombatPhase.React,"reaction opens");
                Check(ui.transform.Find("UI_HUD_Combat").GetComponentsInChildren<Button>().Length==2,"reaction preserves both defense choices");
                ui.Settings.Values.textScale=1.5f; ui.Settings.Apply(); yield return new WaitForSecondsRealtime(.15f);
                Capture(ui,"reaction-150-"+i,1280,720);
                ui.Settings.Values.textScale=1; ui.Settings.Apply();
                ui.Demo.Combat.Defend(ui.Demo.Combat.Model.Expected);
                yield return new WaitForSecondsRealtime(1.3f); ui.Demo.Combat.Attack();
            }
            Check(ui.Demo.Combat.Completed && ui.Demo.PortalsUnlocked,"combat and objects unlock existing portals");
            ui.Demo.Combat.Cancel();
            foreach(int world in new[]{-1,1})
            {
                var portal=ui.Demo.Portals.First(p=>p.world==world); ui.Demo.Travel(portal);
                yield return new WaitForSecondsRealtime(1.1f);
                Check(ui.Demo.World==world && ui.Demo.State==TechnicalDemoState.Exploration,"travel to world "+world);
                ui.Map.Discover(); Capture(ui,"world-"+world,1920,1080);
                var back=ui.Demo.Portals.Where(p=>p.world==0).OrderBy(p=>Vector3.Distance(p.transform.position,ui.Demo.Player.transform.position)).First();
                ui.Demo.Travel(back); yield return new WaitForSecondsRealtime(1.1f);
            }
            ui.Screens.Show(UIScreen.Complete); yield return null; Capture(ui,"complete",1920,1080);
            ui.Screens.Show(UIScreen.VoiceCalibration); ui.Voice.Test();
            yield return new WaitForSecondsRealtime(.2f);
            Check(ui.Voice.HasError,"batch has honest unavailable device feedback");
            Capture(ui,"voice-unavailable",1920,1080);
            ui.Screens.Show(UIScreen.Pause,false); ui.Loading.Restart();
            yield return new WaitForSecondsRealtime(2);
            ui=UIManager.Instance;
            Check(ui!=null && ui.SessionStarted && ui.Demo.Objectives.AnalyzedCount==0,"async restart resets slice and resumes gameplay");
            Check(Object.FindObjectsByType<UIManager>(FindObjectsSortMode.None).Length==1,"no duplicate UI after scene reload");
            ui.ReturnToMenu(); yield return new WaitForSecondsRealtime(2);
            Check(TitleMenuController.Instance!=null&&UIManager.Instance==null,"return unloads gameplay and opens the isolated menu");
            Check(_errors==0,"no runtime errors");
        }
        public static void Capture(UIManager ui,string name,int width,int height)
        { CaptureCanvas(ui.Canvas, Evidence, name, width, height); }
        public static void CaptureCanvas(Canvas canvas,string directory,string name,int width,int height,Action validate=null)
        {
            Directory.CreateDirectory(directory);
            var world=Camera.main; var scaler=canvas.GetComponent<CanvasScaler>();
            var target=new RenderTexture(width,height,24); var worldTarget=new RenderTexture(width,height,24);
            var previousActive=RenderTexture.active;
            var previousTarget=world.targetTexture; var previousMask=world.cullingMask;
            var previousMode=canvas.renderMode; var previousCamera=canvas.worldCamera;
            var previousDistance=canvas.planeDistance; var previousScale=canvas.scaleFactor;
            var previousScaler=scaler.enabled;
            var nodes=canvas.GetComponentsInChildren<Transform>(true);
            var layers=nodes.Select(node=>node.gameObject.layer).ToArray();
            var cameraObject=new GameObject("CaptureUI",typeof(Camera)); var camera=cameraObject.GetComponent<Camera>();
            camera.allowMSAA=false; camera.allowHDR=false; camera.cullingMask=1<<31;
            camera.nearClipPlane=.1f; camera.farClipPlane=10; camera.targetTexture=target;
            var backdrop=new GameObject("CaptureWorld",typeof(RectTransform),typeof(RawImage));
            backdrop.transform.SetParent(canvas.transform,false); backdrop.transform.SetAsFirstSibling(); backdrop.layer=31;
            var backdropRect=(RectTransform)backdrop.transform;
            backdropRect.anchorMin=Vector2.zero; backdropRect.anchorMax=Vector2.one;
            backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
            backdrop.GetComponent<RawImage>().texture=worldTarget; backdrop.GetComponent<RawImage>().raycastTarget=false;
            // Overlay UI needs its own depth pass: world geometry must never obscure the capture.
            try
            {
                foreach(var node in nodes) node.gameObject.layer=31;
                scaler.enabled=false; canvas.scaleFactor=Mathf.Sqrt((width/1920f)*(height/1080f));
                canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
                world.cullingMask=previousMask & ~(1<<31); world.targetTexture=worldTarget;
                // Capture the world first, then render that frame behind the actual UI on its own camera.
                // A single overlay pass avoids both geometry occlusion and URP base-camera color clears.
                Canvas.ForceUpdateCanvases(); validate?.Invoke(); world.Render(); camera.Render(); RenderTexture.active=target;
                var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,width,height),0,0); texture.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG()); Object.Destroy(texture);
            }
            finally
            {
                RenderTexture.active=previousActive; world.targetTexture=previousTarget; world.cullingMask=previousMask;
                backdrop.SetActive(false); Object.Destroy(backdrop);
                canvas.renderMode=previousMode; canvas.worldCamera=previousCamera; canvas.planeDistance=previousDistance;
                canvas.scaleFactor=previousScale; scaler.enabled=previousScaler;
                for(int i=0;i<nodes.Length;i++) nodes[i].gameObject.layer=layers[i];
                camera.targetTexture=null; Object.Destroy(cameraObject); target.Release(); Object.Destroy(target);
                worldTarget.Release(); Object.Destroy(worldTarget);
            }
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Pending,false); InputSystem.onBeforeUpdate-=Input; Application.logMessageReceived-=Log;
            if(_keyboard!=null) InputSystem.RemoveDevice(_keyboard); if(_mouse!=null) InputSystem.RemoveDevice(_mouse);
            string previous=SessionState.GetString("Nemequene.UI.TestSettings","");
            if(previous.Length==0) PlayerPrefs.DeleteKey(SettingsManager.StorageKey); else PlayerPrefs.SetString(SettingsManager.StorageKey,previous); PlayerPrefs.Save();
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence+"/validation.txt","Checks: "+_checks+"\nRuntime errors: "+_errors+"\nEditor search diagnostics: "+_editorErrors+"\nExit: "+code+"\n"+string.Join("\n",Results));
            Debug.Log("NEMEQUENE_UI_VALIDATION checks="+_checks+" errors="+_errors+" code="+code);
            EditorApplication.Exit(code==0&&_errors==0?0:1);
        }
    }
}

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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Nemequene.UI.Editor
{
    [InitializeOnLoad]
    public static class TitleMenuValidation
    {
        private const string Pending="Nemequene.Title.Validation.Pending";
        private static Keyboard _keyboard;
        private static Mouse _mouse;
        private static Key[] _keys=Array.Empty<Key>();
        private static MouseState _mouseState;
        private static readonly List<string> Checks=new List<string>();
        private static int _errors, _editorErrors;
        private const string Evidence="TitleMenuEvidence";
        static TitleMenuValidation() {if(SessionState.GetBool(Pending,false))EditorApplication.update+=Wait;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Use an isolated batch project.");
            UIAssetBuilder.Build();TitleMenuSceneBuilder.Build();
            SessionState.SetString("Nemequene.Title.PreviousSettings",PlayerPrefs.GetString(SettingsManager.StorageKey,""));
            PlayerPrefs.DeleteKey(SettingsManager.StorageKey);
            for(int i=0;i<GameSaveStore.SlotCount;i++)
            {
                string key="Bacata.Save.v1."+i;
                SessionState.SetString("Nemequene.Title.PreviousSave."+i,PlayerPrefs.GetString(key,""));
                PlayerPrefs.DeleteKey(key);
            }
            EditorSceneManager.OpenScene(TitleMenuController.ScenePath);
            SessionState.SetBool(Pending,true);EditorApplication.update+=Wait;EditorApplication.EnterPlaymode();
        }
        private static void Wait()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            EditorApplication.update-=Wait;Application.logMessageReceived+=Log;
            var driver=new GameObject("TitleValidation").AddComponent<UIValidationDriver>();
            Object.DontDestroyOnLoad(driver);driver.Run(Exercise(),Finish);
        }
        private static void Log(string message,string trace,LogType type)
        {
            if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
            if(trace!=null&&trace.Contains("UnityEditor.Search.SearchDatabase")){_editorErrors++;return;}
            _errors++;
        }
        private static void Check(bool value,string name)
        {if(!value)throw new Exception("TITLE_CHECK_FAIL: "+name);Checks.Add(name);Debug.Log("TITLE_CHECK_PASS: "+name);}
        private static void QueueInput()
        {
            if(InputState.currentUpdateType!=InputUpdateType.Dynamic)return;
            InputSystem.QueueStateEvent(_keyboard,new KeyboardState(_keys));InputSystem.QueueStateEvent(_mouse,_mouseState);
        }
        private static IEnumerator Press(Key key)
        {_keys=new[]{key};yield return new WaitForSecondsRealtime(.1f);_keys=Array.Empty<Key>();yield return new WaitForSecondsRealtime(.2f);}
        private static Button FindButton(TitleMenuController title,string text)
        {return title.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text==text);}
        private static IEnumerator Exercise()
        {
            yield return new WaitForSecondsRealtime(.7f);
            _keyboard=InputSystem.AddDevice<Keyboard>();_mouse=InputSystem.AddDevice<Mouse>();InputSystem.onBeforeUpdate+=QueueInput;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            var title=TitleMenuController.Instance;
            Check(title!=null&&title.CurrentView==TitleView.Home,"application opens directly on the isolated title");
            Check(Object.FindObjectsByType<TechnicalDemoController>(FindObjectsSortMode.None).Length==0&&UIManager.Instance==null,"tutorial and its devices are not loaded behind the menu");
            var buttons=title.transform.Find("Title_Home/PrimaryActions").GetComponentsInChildren<Button>();
            Check(buttons.Length==7&&buttons.Select(b=>b.GetComponentInChildren<TMP_Text>().text).SequenceEqual(new[]{"Continuar","Nueva partida","Cargar partida","Configuración","Accesibilidad","Créditos","Salir"}),"seven actions of screen 01 in the documented order");
            Check(!buttons[0].interactable,"continue is unavailable without a save");
            Check(EventSystem.current.currentSelectedGameObject==buttons[1].gameObject,"new game has initial keyboard focus without a save");
            Check(title.Music.clip!=null&&title.Music.loop&&title.Music.volume>0,"independent menu music loops with gentle startup");
            CheckContrast(buttons[1],"initial focused action has readable contrast");
            var painting=title.transform.Find("Menu_Artwork").GetComponent<RawImage>();
            Check(painting.texture!=null&&painting.texture.name=="Portada_Bacata","the project's own cover fills the title screen");
            var emblem=title.transform.Find("Title_Home/Title_EmblemFrame").GetComponentInChildren<RawImage>();
            Check(emblem.texture!=null&&!emblem.raycastTarget&&!painting.raycastTarget,"separate title art and cover leave menu input unobstructed");
            Check(buttons.All(b=>b.transform.Find("Skin")!=null&&b.transform.Find("Skin").GetComponent<Image>().type==Image.Type.Sliced),"all actions use the painted line and ribbon");
            Check(buttons.All(b=>b.GetComponentInChildren<TMP_Text>().alignment==TextAlignmentOptions.Center),"actions retain live centered text");
            foreach(var size in new[]{new Vector2Int(1536,1024),new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1440),new Vector2Int(1920,1200)})
            {
                Capture(title.Canvas,"title-"+size.x+"x"+size.y,size.x,size.y);
            }
            yield return Press(Key.DownArrow);
            Check(EventSystem.current.currentSelectedGameObject!=buttons[1].gameObject,"arrow key moves away from new game");
            CheckContrast(EventSystem.current.currentSelectedGameObject.GetComponent<Button>(),"keyboard selected action has readable contrast");
            Check(buttons[1].transform.Find("Skin").GetComponent<Image>().sprite!=EventSystem.current.currentSelectedGameObject.transform.Find("Skin").GetComponent<Image>().sprite,"only the focused action wears the crimson ribbon");
            EventSystem.current.SetSelectedGameObject(FindButton(title,"Configuración").gameObject);
            yield return Press(Key.Enter);
            Check(title.CurrentView==TitleView.Settings,"Enter opens configuration without starting the tutorial");
            Check(title.transform.Find("Title_Settings").GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Accesibilidad"),"accessibility is the first configuration section");
            title.Settings.Values.textScale=1.5f;title.Settings.Values.highContrast=true;title.Settings.Values.reducedMotion=true;title.Settings.Apply();
            yield return null;Capture(title.Canvas,"settings-150-720p",1280,720);Capture(title.Canvas,"settings-150-1610",1920,1200);
            title.ShowHome();yield return null;
            Capture(title.Canvas,"title-150-720p",1280,720);
            CheckContrast(FindButton(title,"Nueva partida"),"high contrast focus remains readable at 150 percent");
            Capture(title.Canvas,"title-150-1610",1920,1200);
            Check(new SettingsManager().Values.highContrast&&new SettingsManager().Values.textScale==1.5f,"accessibility settings persist");
            title.Settings.Values.textScale=1;title.Settings.Values.highContrast=false;title.Settings.Values.reducedMotion=false;title.Settings.Apply();
            yield return Press(Key.M);yield return new WaitForSecondsRealtime(.8f);
            Check(title.Settings.Values.menuMusic==0&&title.Music.volume==0,"M mutes menu music");
            yield return Press(Key.M);yield return new WaitForSecondsRealtime(.8f);
            Check(title.Settings.Values.menuMusic>0&&title.Music.volume>0,"M restores menu music");
            yield return Press(Key.Tab);Check(EventSystem.current.currentSelectedGameObject!=FindButton(title,"Nueva partida").gameObject,"Tab advances through available actions");
            title.OpenSettings();yield return Press(Key.Escape);
            Check(title.CurrentView==TitleView.Home&&EventSystem.current.currentSelectedGameObject==FindButton(title,"Configuración").gameObject,"Escape returns and restores configuration focus");
            FindButton(title,"Salir").onClick.Invoke();
            Check(title.CurrentView==TitleView.Quit&&EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text==UIStrings.Get("cancel"),"exit requires confirmation with cancel selected");
            Capture(title.Canvas,"quit-confirmation",1920,1080);yield return Press(Key.Escape);
            Check(title.CurrentView==TitleView.Home&&EventSystem.current.currentSelectedGameObject==FindButton(title,"Salir").gameObject,"cancel exit restores the prior action");
            title.OpenSettings();title.SelectSection(2);yield return null;
            // Gráficos: a new screen mode is tried in place, with Mantener / Revertir and a 15 s countdown.
            FindButton(title,UIStrings.Get("gfx.mode")).onClick.Invoke();yield return null;
            FindButton(title,UIStrings.Get("gfx.applyDisplay")).onClick.Invoke();yield return null;
            var trial=title.transform.Find("Title_Settings").GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="ConfirmarPantalla");
            Check(trial.gameObject.activeInHierarchy,"screen mode change offers reversible confirmation");
            FindButton(title,UIStrings.Get("gfx.revertDisplay")).onClick.Invoke();yield return null;
            Check(!trial.gameObject.activeInHierarchy&&title.CurrentView==TitleView.Settings,"reverting the display change returns to configuration");
            title.ShowHome();yield return null;
            // Click the real button with an Input System mouse, rather than invoking NewGame directly.
            Canvas.ForceUpdateCanvases();var play=FindButton(title,"Nueva partida");
            Vector3 point=play.transform.TransformPoint(((RectTransform)play.transform).rect.center);
            _mouseState=new MouseState{position=RectTransformUtility.WorldToScreenPoint(null,point)};
            yield return new WaitForSecondsRealtime(.1f);_mouseState.buttons=1;yield return new WaitForSecondsRealtime(.1f);_mouseState.buttons=0;
            Check(title.CurrentView==TitleView.Saves,"new game opens save slots");
            title.transform.Find("Title_Saves/SaveStone/SaveSlots/Slot_1").GetComponentInChildren<Button>().onClick.Invoke();
            float deadline=Time.unscaledTime+25;
            while(UIManager.Instance==null&&Time.unscaledTime<deadline)yield return null;
            var ui=UIManager.Instance;
            Check(ui!=null&&ui.SessionStarted&&ui.Screens.Current==UIScreen.None,"selected save slot opens the playable tutorial");
            Check(GameSaveStore.TryRead(0,out _),"new game creates a real save slot");
            Check(TitleMenuController.Instance==null,"title scene and menu music unload on new game");
            Check(!ui.Demo.Hands.Requested&&!ui.Voice.Listening,"new game requires neither camera nor microphone");
            Check(ui.Settings.Values.menuMusic>0,"menu settings reach the game");
            ui.Screens.Show(UIScreen.Pause);ui.ReturnToMenu();deadline=Time.unscaledTime+25;
            while(TitleMenuController.Instance==null&&Time.unscaledTime<deadline)yield return null;
            yield return new WaitForSecondsRealtime(.8f);title=TitleMenuController.Instance;
            Check(title!=null&&title.CurrentView==TitleView.Home&&UIManager.Instance==null,"pause returns to the independent title and unloads the tutorial");
            Check(Object.FindObjectsByType<TitleMenuController>(FindObjectsSortMode.None).Length==1&&title.Music.isPlaying,"return creates one menu and one playing music source");
            Check(_errors==0,"no runtime errors in the complete title flow");
        }
        private static void CheckTextBounds(TitleMenuController title,string label)
        {
            var home=title.transform.Find("Title_Home");
            var emblem=home.Find("Title_EmblemFrame").GetComponentInChildren<RawImage>();
            var corners=new Vector3[4];emblem.rectTransform.GetWorldCorners(corners);
            var root=(RectTransform)title.Canvas.transform;
            var logoMin=root.InverseTransformPoint(corners[0]);var logoMax=root.InverseTransformPoint(corners[2]);
            var actions=home.Find("PrimaryActions").GetComponent<RectTransform>();actions.GetWorldCorners(corners);
            float actionTop=root.InverseTransformPoint(corners[2]).y;
            float actionRight=root.InverseTransformPoint(corners[2]).x;
            Check(actionRight<root.rect.center.x,label+": actions stay in the left field, clear of the cover's figures");
            Check(root.rect.Contains(new Vector2(logoMin.x,logoMin.y))&&root.rect.Contains(new Vector2(logoMax.x,logoMax.y))
                &&logoMin.y>actionTop,label+": emblem fits above all actions");
            foreach(var text in home.GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                if(text.textInfo.characterCount==0)continue;
                var bounds=text.textBounds;var rect=text.rectTransform.rect;
                Check(bounds.size.x<=rect.width+3&&bounds.size.y<=rect.height+3,label+": "+text.text);
            }
        }
        private static void CheckContrast(Button button,string label)
        {
            var skin=button.transform.Find("Skin");
            var image=skin!=null&&skin.GetComponent<Image>().enabled?skin.GetComponent<Image>():button.GetComponent<Image>();
            var text=button.GetComponentInChildren<TMP_Text>();
            Color background=image.color*image.canvasRenderer.GetColor();Color foreground=text.color;
            float b=Luminance(background),f=Luminance(foreground);
            if(image.sprite!=null)
            {
                var source=new Texture2D(2,2);
                try
                {
                    source.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(image.sprite)));var rect=image.sprite.rect;
                    b=0;
                    // Measure the brightest part of the real stone behind the live label, excluding the ornaments.
                    for(int y=(int)(rect.y+rect.height*.25f);y<rect.y+rect.height*.75f;y+=8)
                        for(int x=(int)(rect.x+rect.width*.25f);x<rect.x+rect.width*.75f;x+=8)
                            b=Mathf.Max(b,Luminance(source.GetPixel(x,y)*background));
                }
                finally {Object.Destroy(source);}
            }
            float ratio=(Mathf.Max(b,f)+.05f)/(Mathf.Min(b,f)+.05f);
            Check(ratio>=4.5f,label+" ("+ratio.ToString("0.00")+":1)");
        }
        private static float Luminance(Color color)
        {
            Func<float,float> linear=v=>v<=.04045f?v/12.92f:Mathf.Pow((v+.055f)/1.055f,2.4f);
            return .2126f*linear(color.r)+.7152f*linear(color.g)+.0722f*linear(color.b);
        }
        public static void Capture(Canvas canvas,string name,int width,int height)
        { UIValidation.CaptureCanvas(canvas,Evidence,name,width,height,name.StartsWith("title-")?()=>CheckTextBounds(TitleMenuController.Instance,name):null); }
        private static void Finish(int code)
        {
            SessionState.SetBool(Pending,false);InputSystem.onBeforeUpdate-=QueueInput;Application.logMessageReceived-=Log;
            if(_keyboard!=null)InputSystem.RemoveDevice(_keyboard);if(_mouse!=null)InputSystem.RemoveDevice(_mouse);
            string previous=SessionState.GetString("Nemequene.Title.PreviousSettings","");
            if(previous.Length==0)PlayerPrefs.DeleteKey(SettingsManager.StorageKey);else PlayerPrefs.SetString(SettingsManager.StorageKey,previous);PlayerPrefs.Save();
            for(int i=0;i<GameSaveStore.SlotCount;i++)
            {
                string key="Bacata.Save.v1."+i;
                string saved=SessionState.GetString("Nemequene.Title.PreviousSave."+i,"");
                if(saved.Length==0)PlayerPrefs.DeleteKey(key);else PlayerPrefs.SetString(key,saved);
            }
            PlayerPrefs.Save();
            Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/validation.txt","Checks: "+Checks.Count+"\nRuntime errors: "+_errors+"\nEditor search diagnostics: "+_editorErrors+"\nExit: "+code+"\n"+string.Join("\n",Checks));
            Debug.Log("NEMEQUENE_TITLE_VALIDATION checks="+Checks.Count+" errors="+_errors+" code="+code);
            EditorApplication.Exit(code==0&&_errors==0?0:1);
        }
    }
}

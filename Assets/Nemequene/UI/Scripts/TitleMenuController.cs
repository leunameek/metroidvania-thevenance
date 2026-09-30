using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum TitleView { Home, Settings, Saves, Quit, Loading }

    // The title scene owns no player, tutorial, camera capture or voice recognizer.
    public sealed class TitleMenuController : MonoBehaviour
    {
        public const string ScenePath="Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity";
        public const string TutorialPath="Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity";
        public static TitleMenuController Instance { get; private set; }
        public Canvas Canvas { get; private set; }
        public SettingsManager Settings { get; private set; }
        public TitleView CurrentView { get; private set; }
        public AudioSource Music => _music;
        public bool IsLoading { get; private set; }
        private UITheme _theme;
        private UIFactory _factory;
        private GameObject _home, _settings, _saves, _quit, _loading;
        private RectTransform _root;
        private RectTransform _primaryActions;
        private Button _newGame, _continue, _configuration, _cancelQuit, _confirmQuit, _back, _loadingBack;
        private readonly Button[] _slotButtons = new Button[GameSaveStore.SlotCount];
        private readonly Button[] _deleteButtons = new Button[GameSaveStore.SlotCount];
        private bool _newSaveMode;
        private TMP_Text _loadStatus, _resolutionLabel;
        private TMP_Text _continueHint;
        private Image _progress;
        private TitleMenuBackdrop _shade;
        private TitleMenuAtmosphere _atmosphere;
        private readonly List<RectTransform> _groups=new List<RectTransform>();
        private readonly List<TitleMenuButton> _tabs=new List<TitleMenuButton>();
        private AudioSource _music, _cues;
        private AudioClip _cue;
        private CanvasGroup _homeFade;
        private float _entered, _lastCue, _muteRestore=.3f;
        private int _changedFrame=-1, _section;
        private bool _resolutionPreview;
        private float _resolutionUntil;
        private int _oldWidth, _oldHeight, _requestedWidth=1920, _requestedHeight=1080;
        private FullScreenMode _oldMode;
        private bool _requestedFullscreen;
        private Action _confirmAction;
        private TMP_Text _confirmText;
        private TitleView _beforeConfirm;
        private GameObject _beforeConfirmFocus;

        private void Awake()
        {
            Instance=this; UIManager.EnterGameplayOnLoad=false; Time.timeScale=1;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
            _theme=Resources.Load<UITheme>("Nemequene/Theme");
            if (_theme==null || _theme.bodyFont==null || _theme.titleFont==null)
            { Debug.LogError("Nemequene title: UI theme or fonts are missing."); enabled=false; return; }
            Settings=new SettingsManager(); _factory=new UIFactory(_theme);
            _root=GetComponent<RectTransform>();
            Canvas=GetComponent<Canvas>(); Canvas.renderMode=RenderMode.ScreenSpaceOverlay; Canvas.sortingOrder=100;
            var scaler=GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
            if (EventSystem.current==null)
            {
                var events=new GameObject("Title_EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform,false); events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            BuildBackdrop(); BuildHome(); BuildSettings(); BuildSaves(); BuildConfirmation(); BuildLoading();
            _music=gameObject.AddComponent<AudioSource>(); _music.playOnAwake=false; _music.loop=true; _music.volume=0;
            _music.clip=Resources.Load<AudioClip>("Nemequene/Menu_Bruma");
            if (_music.clip!=null) _music.Play();
            _cues=gameObject.AddComponent<AudioSource>(); _cues.playOnAwake=false;
            _cue=Resources.Load<AudioClip>("Nemequene/UI_Select");
            Settings.Changed+=ApplyPresentation; Settings.Apply(false);
            if (!Application.isBatchMode && Settings.Values.screenWidth>=800 && Settings.Values.screenHeight>=600)
                Screen.SetResolution(Settings.Values.screenWidth,Settings.Values.screenHeight,Settings.Values.fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
            _entered=Time.unscaledTime; ShowHome();
        }
        private void BuildBackdrop()
        {
            var image=_factory.Rect("Menu_Artwork",_root,Vector2.zero,Vector2.one);
            var raw=image.gameObject.AddComponent<RawImage>(); raw.texture=Resources.Load<Texture2D>("Nemequene/Menu_BacataRuins"); raw.raycastTarget=false;
            _atmosphere=image.gameObject.AddComponent<TitleMenuAtmosphere>();
            // Fit the complete reference composition to the viewport: keep both foreground edges and the sun.
            _shade=_factory.Rect("ReadingShade",_root,Vector2.zero,Vector2.one).gameObject.AddComponent<TitleMenuBackdrop>(); _shade.raycastTarget=false;
            _shade.readingEdge=.31f;
        }
        private void BuildHome()
        {
            _home=_factory.Rect("Title_Home",_root,Vector2.zero,Vector2.one).gameObject;
            _homeFade=_home.AddComponent<CanvasGroup>();
            var emblem=Resources.Load<Texture2D>("Nemequene/Title_BacataMountains");
            if(emblem!=null)
            {
                // Title on the right, over the sky; the stone actions stay on the left.
                var frame=_factory.Rect("Title_EmblemFrame",_home.transform,new Vector2(.50f,.55f),new Vector2(.95f,.99f));
                var art=_factory.Rect("El asedio de Bacatá",frame,Vector2.zero,Vector2.one);
                var image=art.gameObject.AddComponent<RawImage>(); image.texture=emblem; image.raycastTarget=false;
                var aspect=art.gameObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio=(float)emblem.width/emblem.height;
            }
            else _factory.Label(_home.transform,UIStrings.Get("game.title"),new Vector2(.50f,.55f),new Vector2(.95f,.93f),64,true);
            var actions=_factory.Column(_home.transform,"PrimaryActions",8); _primaryActions=actions;
            actions.anchorMin=new Vector2(.065f,.09f); actions.anchorMax=new Vector2(.455f,.57f);
            actions.GetComponent<VerticalLayoutGroup>().childForceExpandHeight=true;
            _newGame=Button(actions,"newGame",NewGame,true,30);
            _continue=Button(actions,"save.continue",ContinueGame,true,30);
            var load=Button(actions,"save.load",()=>OpenSaves(false),true,30);
            _configuration=Button(actions,"settings",OpenSettings,true,30);
            var credits=Button(actions,"credits",()=>{OpenSettings();SelectSection(4);},true,30);
            var exit=Button(actions,"title.exit",RequestQuit,true,30);
            exit.GetComponent<TitleMenuButton>().danger=true;
            var skins=Resources.LoadAll<Sprite>("Nemequene/Menu_StoneButton");
            var stone=skins.Length>0?skins[0]:null;
            foreach(var button in new[]{_newGame,_continue,load,_configuration,credits,exit})
            {
                var layout=button.GetComponent<LayoutElement>();layout.minHeight=64;layout.preferredHeight=78;layout.flexibleHeight=1;
                button.GetComponent<TitleMenuButton>().UseStoneSkin(stone,_theme.titleFont,true);
            }
            _continueHint=_factory.Label(_home.transform,UIStrings.Get("continue.unavailable"),new Vector2(.065f,.035f),new Vector2(.455f,.08f),20);
            _continueHint.alignment=TextAlignmentOptions.Center;
        }
        private void BuildSaves()
        {
            _saves=_factory.Rect("Title_Saves",_root,Vector2.zero,Vector2.one).gameObject;
            var sheet=_factory.Panel("SaveStone",_saves.transform,new Vector2(.14f,.08f),new Vector2(.86f,.92f),true);
            _factory.Heading(sheet,UIStrings.Get("save.title"),new Vector2(.10f,.84f),new Vector2(.90f,.935f),48);
            _factory.Divider(sheet,new Vector2(.30f,.81f),new Vector2(.70f,.835f));
            var list=_factory.Column(sheet,"SaveSlots",16); list.anchorMin=new Vector2(.09f,.18f); list.anchorMax=new Vector2(.91f,.80f);
            for(int i=0;i<GameSaveStore.SlotCount;i++)
            {
                int slot=i;
                var row=_factory.Rect("Slot_"+(i+1),list,Vector2.zero,Vector2.one);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight=156;
                row.gameObject.AddComponent<Image>().color=_theme.panel;
                _factory.Frame(row,false);
                var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing=16;
                layout.padding=new RectOffset(24,24,24,24);
                layout.childControlWidth=layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;
                _slotButtons[i]=Button(row,"save.empty",()=>SelectSlot(slot),false,24);
                _deleteButtons[i]=Button(row,"save.delete",()=>Confirm("save.deleteConfirm",()=>{GameSaveStore.Delete(slot);RefreshSaves();}),false,22);
                _deleteButtons[i].GetComponent<LayoutElement>().preferredWidth=180;
            }
            var back=_factory.Column(sheet,"Back"); back.anchorMin=new Vector2(.09f,.055f);back.anchorMax=new Vector2(.35f,.14f);
            Button(back,"title.back",ShowHome);
        }
        private void RefreshSaves()
        {
            for(int i=0;i<GameSaveStore.SlotCount;i++)
            {
                bool valid=GameSaveStore.TryRead(i,out var data);
                string text=valid ? UIStrings.Get("save.slot",i+1,data.checkpoint,data.CompletionPercent,
                    TimeSpan.FromSeconds(data.playSeconds).ToString(@"hh\:mm"),DateTime.Parse(data.savedAtUtc).ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
                    : GameSaveStore.IsOccupied(i) ? UIStrings.Get("save.corrupt",i+1) : UIStrings.Get("save.empty",i+1);
                _slotButtons[i].GetComponentInChildren<TMP_Text>().text=text;
                _slotButtons[i].interactable=_newSaveMode || valid;
                _slotButtons[i].GetComponent<TitleMenuButton>().Refresh();
                _deleteButtons[i].gameObject.SetActive(GameSaveStore.IsOccupied(i));
            }
            if(CurrentView==TitleView.Saves && EventSystem.current!=null
                && EventSystem.current.currentSelectedGameObject!=null
                && !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            {
                GameObject next=null;
                foreach(var button in _slotButtons) if(button.interactable) {next=button.gameObject;break;}
                if(next==null) next=_saves.transform.Find("Back").GetComponentInChildren<Button>().gameObject;
                EventSystem.current.SetSelectedGameObject(next);
            }
        }
        private void OpenSaves(bool create)
        {
            _newSaveMode=create; RefreshSaves();
            GameObject focus=null;
            foreach(var button in _slotButtons) if(button.interactable) {focus=button.gameObject;break;}
            SetView(TitleView.Saves,focus);
        }
        private void SelectSlot(int slot)
        {
            if(_newSaveMode)
            {
                if(GameSaveStore.IsOccupied(slot)) Confirm("save.replaceConfirm",()=>StartGame(slot,false));
                else StartGame(slot,false);
            }
            else if(GameSaveStore.TryRead(slot,out _)) StartGame(slot,true);
        }
        private void ContinueGame()
        {
            int slot=GameSaveStore.LatestSlot();
            if(slot>=0) StartGame(slot,true);
        }
        private void BuildSettings()
        {
            _settings=_factory.Rect("Title_Settings",_root,Vector2.zero,Vector2.one).gameObject;
            var sheet=_factory.Panel("SettingsStone",_settings.transform,new Vector2(.06f,.06f),new Vector2(.94f,.94f),true);
            _factory.Heading(sheet,UIStrings.Get("settings"),new Vector2(.08f,.84f),new Vector2(.92f,.935f),48);
            _factory.Divider(sheet,new Vector2(.30f,.81f),new Vector2(.70f,.835f));
            var nav=_factory.Column(sheet,"Sections",16); nav.anchorMin=new Vector2(.075f,.28f); nav.anchorMax=new Vector2(.29f,.79f);
            var body=_factory.Scroll(sheet,new Vector2(.34f,.19f),new Vector2(.93f,.79f));
            string[] sections={"accessibility","settings.audio","settings.graphics","controls","credits"};
            for(int i=0;i<sections.Length;i++)
            {
                int index=i; var group=_factory.Column(body,"Settings_"+i,16); _groups.Add(group);
                _tabs.Add(Button(nav,sections[i],()=>SelectSection(index)).GetComponent<TitleMenuButton>());
                _factory.Text(group,UIStrings.Get(sections[i]),32,true);
            }
            var access=_groups[0];
            Choice(access,"access.text",new[]{"100 %","125 %","150 %"},()=>Mathf.RoundToInt((Settings.Values.textScale-1)*4),v=>Settings.Values.textScale=1+v*.25f);
            Toggle(access,"access.readableFont",()=>Settings.Values.readableFont,v=>Settings.Values.readableFont=v);
            Toggle(access,"access.contrast",()=>Settings.Values.highContrast,v=>Settings.Values.highContrast=v);
            Toggle(access,"access.motion",()=>Settings.Values.reducedMotion,v=>Settings.Values.reducedMotion=v);
            Toggle(access,"access.subtitles",()=>Settings.Values.subtitles,v=>Settings.Values.subtitles=v);
            Choice(access,"access.subtitleSize",new[]{"100 %","125 %","150 %"},()=>Mathf.RoundToInt((Settings.Values.subtitleScale-1)*4),v=>Settings.Values.subtitleScale=1+v*.25f);
            Slider(access,"access.subtitleOpacity",.35f,1,()=>Settings.Values.subtitleOpacity,v=>Settings.Values.subtitleOpacity=v);
            Toggle(access,"access.speakers",()=>Settings.Values.speakerNames,v=>Settings.Values.speakerNames=v);
            Toggle(access,"access.captions",()=>Settings.Values.soundCaptions,v=>Settings.Values.soundCaptions=v);
            Slider(access,"access.reaction",1,3,()=>Settings.Values.reactionScale,v=>Settings.Values.reactionScale=v);
            Slider(access,"access.camera",0,1,()=>Settings.Values.cameraMotion,v=>Settings.Values.cameraMotion=v);
            Slider(access,"access.flash",0,1,()=>Settings.Values.flashIntensity,v=>Settings.Values.flashIntensity=v);
            Button(access,"access.reset",()=>Confirm("confirm.access",Settings.ResetAccessibility));
            var audio=_groups[1];
            Slider(audio,"audio.master",0,1,()=>Settings.Values.master,v=>Settings.Values.master=v);
            Slider(audio,"title.music",0,1,()=>Settings.Values.menuMusic,v=>Settings.Values.menuMusic=v);
            Slider(audio,"audio.ui",0,1,()=>Settings.Values.uiVolume,v=>Settings.Values.uiVolume=v);
            Slider(audio,"audio.effects",0,1,()=>Settings.Values.effects,v=>Settings.Values.effects=v);
            Slider(audio,"audio.ambience",0,1,()=>Settings.Values.ambience,v=>Settings.Values.ambience=v);
            var graphics=_groups[2];
            int[] widths={1280,1920,2560,1920}; int[] heights={720,1080,1440,1200};
            int resolution=1;
            for(int i=0;i<widths.Length;i++) if(widths[i]==Settings.Values.screenWidth&&heights[i]==Settings.Values.screenHeight) resolution=i;
            _requestedWidth=widths[resolution]; _requestedHeight=heights[resolution]; _requestedFullscreen=Settings.Values.fullscreen;
            Choice(graphics,"graphics.resolution",new[]{"1280 × 720","1920 × 1080","2560 × 1440","1920 × 1200"},()=>resolution,v=>{resolution=v;_requestedWidth=widths[v];_requestedHeight=heights[v];});
            Toggle(graphics,"graphics.fullscreen",()=>_requestedFullscreen,v=>_requestedFullscreen=v);
            Button(graphics,"graphics.apply",PreviewResolution);
            _resolutionLabel=_factory.Text(graphics,UIStrings.Get("title.resolutionNote"),22);
            Toggle(graphics,"graphics.vsync",()=>Settings.Values.vSync,v=>Settings.Values.vSync=v);
            Choice(graphics,"graphics.quality",QualitySettings.names,()=>Settings.Values.quality<0?QualitySettings.GetQualityLevel():Settings.Values.quality,v=>Settings.Values.quality=v);
            int[] fps={30,60,120,-1}; Choice(graphics,"graphics.fps",new[]{"30","60","120",UIStrings.Get("unlimited")},()=>Mathf.Max(0,Array.IndexOf(fps,Settings.Values.frameLimit)),v=>Settings.Values.frameLimit=fps[v]);
            var controls=_groups[3];
            _factory.Text(controls,UIStrings.Get("title.controlsIntro"),24);
            _factory.Text(controls,UIStrings.Get("controls.body"),24);
            Toggle(controls,"settings.tutorials",()=>Settings.Values.tutorials,v=>Settings.Values.tutorials=v);
            _factory.Text(_groups[4],UIStrings.Get("credits.body"),24);
            _factory.Text(_groups[4],UIStrings.Get("title.credits"),22);
            var backRow=_factory.Column(sheet,"Back"); backRow.anchorMin=new Vector2(.075f,.065f); backRow.anchorMax=new Vector2(.29f,.16f);
            _back=Button(backRow,"title.back",BackToHome);
            SelectSection(0);
        }
        private void BuildConfirmation()
        {
            _quit=_factory.Rect("Title_Confirmation",_root,Vector2.zero,Vector2.one).gameObject;
            var sheet=_factory.Panel("ConfirmationStone",_quit.transform,new Vector2(.25f,.22f),new Vector2(.75f,.78f),true);
            var content=_factory.Column(sheet,"ConfirmationContent",24);
            content.anchorMin=new Vector2(.10f,.12f); content.anchorMax=new Vector2(.90f,.86f);
            _confirmText=_factory.Text(content,"",40,true);
            _cancelQuit=Button(content,"cancel",CancelConfirmation);
            _confirmQuit=Button(content,"confirm",()=>{var action=_confirmAction; _confirmAction=null; CloseConfirmation(false); action?.Invoke();});
        }
        private void BuildLoading()
        {
            _loading=_factory.Rect("Title_Loading",_root,Vector2.zero,Vector2.one).gameObject;
            var sheet=_factory.Panel("LoadingStone",_loading.transform,new Vector2(.24f,.25f),new Vector2(.76f,.75f),true);
            var body=_factory.Column(sheet,"LoadingContent",24); body.anchorMin=new Vector2(.10f,.12f); body.anchorMax=new Vector2(.90f,.86f);
            _factory.Text(body,UIStrings.Get("world.plaza"),48,true);
            _loadStatus=_factory.Text(body,UIStrings.Get("title.loading"),28);
            var track=_factory.Rect("Progress",body,Vector2.zero,Vector2.one); track.gameObject.AddComponent<LayoutElement>().preferredHeight=16;
            _progress=_factory.Bar(track,"ActualLoadProgress",Vector2.zero,Vector2.one);
            _loadingBack=Button(body,"title.back",()=>{if(!IsLoading)ShowHome();});
            _loadingBack.gameObject.SetActive(false);
        }
        private Button Button(Transform parent,string key,Action action,bool main=false,float size=24)
        {
            var button=_factory.Button(parent,UIStrings.Get(key),()=>{if(Time.frameCount<=_changedFrame)return; PlayCue(true);action?.Invoke();});
            var label=button.GetComponentInChildren<TMP_Text>(); label.GetComponent<UIStyleBinding>().baseSize=size; label.fontSize=size;
            button.GetComponent<TitleMenuButton>().Initialize(main); return button;
        }
        private void Toggle(Transform parent,string key,Func<bool> get,Action<bool> set)
        {
            Button button=null;
            Action refresh=()=>button.GetComponentInChildren<TMP_Text>().text=UIStrings.Get(key)+"   "+UIStrings.Get(get()?"on":"off");
            button=Button(parent,key,()=>{set(!get()); Settings.Apply();});
            button.gameObject.AddComponent<UIValueBinding>().Bind(Settings,refresh);
        }
        private void Choice(Transform parent,string key,string[] values,Func<int> get,Action<int> set)
        {
            Button button=null;
            Action refresh=()=>button.GetComponentInChildren<TMP_Text>().text=UIStrings.Get(key)+"   "+values[Mathf.Clamp(get(),0,values.Length-1)];
            button=Button(parent,key,()=>{set((get()+1)%values.Length); Settings.Apply();});
            button.gameObject.AddComponent<UIValueBinding>().Bind(Settings,refresh);
        }
        private void Slider(Transform parent,string key,float min,float max,Func<float> get,Action<float> set)
        {
            var slider=_factory.Slider(parent,key,min,max,get(),v=>{set(v);Settings.Apply();});
            var label=slider.transform.parent.GetComponentInChildren<TMP_Text>();
            slider.gameObject.AddComponent<UIValueBinding>().Bind(Settings,()=>{slider.SetValueWithoutNotify(get());label.text=UIStrings.Get(key)+"   "+get().ToString("0.00");});
        }
        private static void ConnectVertical(Button[] buttons)
        {
            for(int i=0;i<buttons.Length;i++)
            { var nav=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=buttons[(i+buttons.Length-1)%buttons.Length],selectOnDown=buttons[(i+1)%buttons.Length]}; buttons[i].navigation=nav; }
        }
        public void SelectSection(int index)
        {
            _section=index;
            for(int i=0;i<_groups.Count;i++) { _groups[i].gameObject.SetActive(i==index);_tabs[i].tabSelected=i==index;_tabs[i].Refresh(); }
            if(_groups[index].GetComponentInParent<ScrollRect>() is ScrollRect scroll) scroll.verticalNormalizedPosition=1;
            Settings.Flush();
        }
        private void SetView(TitleView view,GameObject focus)
        {
            _changedFrame=Time.frameCount; CurrentView=view;
            _home.SetActive(view==TitleView.Home); _settings.SetActive(view==TitleView.Settings);
            _saves.SetActive(view==TitleView.Saves);
            _shade.gameObject.SetActive(view!=TitleView.Home);
            _quit.SetActive(view==TitleView.Quit); _loading.SetActive(view==TitleView.Loading);
            Canvas.ForceUpdateCanvases(); EventSystem.current?.SetSelectedGameObject(null); EventSystem.current?.SetSelectedGameObject(focus);
        }
        public void ShowHome()
        {
            Settings.Flush();
            _continue.interactable=GameSaveStore.LatestSlot()>=0;
            _continue.GetComponent<TitleMenuButton>().Refresh();
            _continueHint.gameObject.SetActive(!_continue.interactable);
            SetView(TitleView.Home,_continue.interactable?_continue.gameObject:_newGame.gameObject);
        }
        private void BackToHome() { ShowHome();EventSystem.current?.SetSelectedGameObject(_configuration.gameObject); }
        public void OpenSettings() { SetView(TitleView.Settings,_tabs[0].gameObject);SelectSection(0); }
        private void Confirm(string key,Action action)
        {
            _beforeConfirm=CurrentView;_beforeConfirmFocus=EventSystem.current?.currentSelectedGameObject;
            _confirmAction=action;_confirmText.text=UIStrings.Get(key);
            var style=_confirmQuit.GetComponent<TitleMenuButton>();
            style.danger=key=="title.quitConfirm"||key=="save.deleteConfirm"||key=="save.replaceConfirm";
            style.Refresh();
            SetView(TitleView.Quit,_cancelQuit.gameObject);
        }
        public void RequestQuit() { Confirm("title.quitConfirm",Quit); }
        public void CancelConfirmation() { _confirmAction=null;CloseConfirmation(true); }
        private void CloseConfirmation(bool cancelled)
        {
            if(cancelled&&_resolutionPreview) RestoreResolution();
            SetView(_beforeConfirm,_beforeConfirmFocus!=null?_beforeConfirmFocus:(_beforeConfirm==TitleView.Settings?_tabs[_section].gameObject:_newGame.gameObject));
        }
        private void ApplyPresentation()
        {
            foreach(var binding in GetComponentsInChildren<UIStyleBinding>(true)) binding.Apply(_theme,Settings.Values);
            foreach(var button in GetComponentsInChildren<TitleMenuButton>(true)) button.Refresh();
            _shade.SetContrast(Settings.Values.highContrast);
            // Larger text widens the stone menu while the landscape remains visible.
            float right=Mathf.Lerp(.455f,.54f,(Settings.Values.textScale-1)*2);
            _primaryActions.anchorMin=new Vector2(.065f,.09f);
            _primaryActions.anchorMax=new Vector2(right,.57f);
            if(Settings.Values.reducedMotion) _homeFade.alpha=1;
        }
        public void PlayCue(bool activation)
        {
            if(_cues==null||_cue==null||Time.unscaledTime-_lastCue<.10f)return;
            _lastCue=Time.unscaledTime;_cues.pitch=activation ? .86f : .70f;
            _cues.PlayOneShot(_cue,Settings.Values.uiVolume*(activation ? .7f : .28f));
        }
        public void NewGame()
        {
            OpenSaves(true);
        }
        private void StartGame(int slot,bool load)
        {
            if(IsLoading)return;
            if(!Application.CanStreamedLevelBeLoaded(TutorialPath)) {ShowLoadError();return;}
            GameSaveStore.Begin(slot,load);
            _loadingBack.gameObject.SetActive(false);
            Settings.Values.configured=true;Settings.Apply();Settings.Flush();
            SetView(TitleView.Loading,null);
            StartCoroutine(LoadTutorial());
        }
        private IEnumerator LoadTutorial()
        {
            IsLoading=true; yield return null;
            AsyncOperation operation=null;
            try {operation=SceneManager.LoadSceneAsync(TutorialPath,LoadSceneMode.Single);}
            catch(Exception exception) {Debug.LogWarning("Could not load tutorial: "+exception.Message);}
            if(operation==null) {ShowLoadError();yield break;}
            operation.allowSceneActivation=false;
            while(operation.progress<.9f) {UIFactory.Fill(_progress,operation.progress);yield return null;}
            float end=Time.unscaledTime+.35f; float level=_music.volume;
            while(Time.unscaledTime<end) {_music.volume=level*Mathf.Clamp01((end-Time.unscaledTime)/.35f);yield return null;}
            _music.volume=0;
            UIManager.EnterGameplayOnLoad=true;operation.allowSceneActivation=true;
        }
        private void ShowLoadError()
        {
            GameSaveStore.ClearPendingLoad();
            IsLoading=false; _loadStatus.text=UIStrings.Get("loading.error"); _loadingBack.gameObject.SetActive(true);
            SetView(TitleView.Loading,_loadingBack.gameObject);
        }
        private void PreviewResolution()
        {
            _oldWidth=Screen.width;_oldHeight=Screen.height;_oldMode=Screen.fullScreenMode;
            Screen.SetResolution(_requestedWidth,_requestedHeight,_requestedFullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
            _resolutionPreview=true;_resolutionUntil=Time.unscaledTime+15;
            Confirm("graphics.keep",()=>{_resolutionPreview=false;Settings.Values.screenWidth=_requestedWidth;Settings.Values.screenHeight=_requestedHeight;Settings.Values.fullscreen=_requestedFullscreen;Settings.Apply();Settings.Flush();});
        }
        private void RestoreResolution() {Screen.SetResolution(_oldWidth,_oldHeight,_oldMode);_resolutionPreview=false;}
        private void Update()
        {
            if(Settings==null)return;
            var keyboard=Keyboard.current;
            if(!IsLoading&&Time.frameCount>_changedFrame)
            {
                bool back=(keyboard!=null&&keyboard.escapeKey.wasPressedThisFrame)||(Gamepad.current!=null&&Gamepad.current.buttonEast.wasPressedThisFrame);
                if(back)
                {
                    if(CurrentView==TitleView.Quit)CancelConfirmation();
                    else if(CurrentView==TitleView.Settings)BackToHome();
                    else if(CurrentView==TitleView.Saves)ShowHome();
                    else if(CurrentView==TitleView.Home)RequestQuit();
                }
                if(keyboard!=null&&keyboard.mKey.wasPressedThisFrame&&CurrentView==TitleView.Home)
                {if(Settings.Values.menuMusic>.001f){_muteRestore=Settings.Values.menuMusic;Settings.Values.menuMusic=0;}else Settings.Values.menuMusic=_muteRestore;Settings.Apply();}
                if(keyboard!=null&&keyboard.tabKey.wasPressedThisFrame)NavigateTab(keyboard.shiftKey.isPressed);
            }
            if(_resolutionPreview&&Time.unscaledTime>=_resolutionUntil)CancelConfirmation();
            if(_resolutionPreview)_confirmText.text=UIStrings.Get("graphics.keep")+"\n"+UIStrings.Get("title.revert",Mathf.CeilToInt(_resolutionUntil-Time.unscaledTime));
            if(_music!=null)_music.volume=Mathf.MoveTowards(_music.volume,IsLoading?0:Settings.Values.menuMusic,Time.unscaledDeltaTime*.45f);
            _homeFade.alpha=Settings.Values.reducedMotion?1:Mathf.Clamp01((Time.unscaledTime-_entered)/.45f);
        }
        private void NavigateTab(bool backwards)
        {
            var controls=new List<Selectable>();
            foreach(var control in GetComponentsInChildren<Selectable>())if(control.IsInteractable())controls.Add(control);
            if(controls.Count==0)return;
            int index=controls.FindIndex(c=>c.gameObject==EventSystem.current.currentSelectedGameObject);
            EventSystem.current.SetSelectedGameObject(controls[(index+(backwards?controls.Count-1:1))%controls.Count].gameObject);
        }
        private void Quit()
        {
            Settings.Flush();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void OnDestroy()
        {
            if(Settings!=null){Settings.Changed-=ApplyPresentation;Settings.Flush();}
            if(_music!=null)_music.Stop();
            if(_resolutionPreview)RestoreResolution();
            if(Instance==this)Instance=null;
        }
    }
}

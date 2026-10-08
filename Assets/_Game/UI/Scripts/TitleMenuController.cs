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
        public const string ScenePath="Assets/_Game/Scenes/MainMenu.unity";
        public const string TutorialPath="Assets/_Game/Scenes/PlazaNunez.unity";
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
        private TMP_Text _loadStatus, _savesTitle, _savesSubtitle;
        private TMP_Text _continueHint;
        private Image _progress;
        private TitleMenuBackdrop _shade;
        private TitleMenuAtmosphere _atmosphere;
        private readonly List<RectTransform> _groups=new List<RectTransform>();
        private readonly List<TitleMenuButton> _tabs=new List<TitleMenuButton>();
        private AudioSource _music;
        private CanvasGroup _homeFade;
        private float _entered, _lastCue, _muteRestore=.3f;
        private int _changedFrame=-1, _section;
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
            // The stored screen mode and size were applied at startup (GraphicsRuntime).
            Settings.Changed+=ApplyPresentation; Settings.Apply(false);
            _entered=Time.unscaledTime; ShowHome();
        }
        private void BuildBackdrop()
        {
            var image=_factory.Rect("Menu_Artwork",_root,Vector2.zero,Vector2.one);
            var raw=image.gameObject.AddComponent<RawImage>(); raw.raycastTarget=false;
            var cover=UIBacata.Art("Portada_Bacata");
            if(cover!=null)
            {
                // Screen 01: the project's own cover, the protagonist to the right and a near-black
                // field on the left for the actions. Envelope keeps it full-bleed at any aspect.
                raw.texture=cover;
                var fit=image.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio=(float)cover.width/cover.height;
            }
            else
            {
                raw.texture=Resources.Load<Texture2D>("Nemequene/Menu_BacataRuins");
                _atmosphere=image.gameObject.AddComponent<TitleMenuAtmosphere>();
            }
            _shade=_factory.Rect("ReadingShade",_root,Vector2.zero,Vector2.one).gameObject.AddComponent<TitleMenuBackdrop>(); _shade.raycastTarget=false;
            _shade.even=true; _shade.strength=.92f;
        }
        private void BuildHome()
        {
            _home=_factory.Rect("Title_Home",_root,Vector2.zero,Vector2.one).gameObject;
            _homeFade=_home.AddComponent<CanvasGroup>();
            var emblem=UIBacata.Art("Titulo_Bacata");
            if(emblem!=null)
            {
                // Painted title in the upper left, above the column of actions.
                var frame=_factory.Rect("Title_EmblemFrame",_home.transform,new Vector2(.045f,.70f),new Vector2(.315f,.95f));
                var art=_factory.Rect("El asedio de Bacatá",frame,Vector2.zero,Vector2.one);
                var image=art.gameObject.AddComponent<RawImage>(); image.texture=emblem; image.raycastTarget=false;
                var aspect=art.gameObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio=(float)emblem.width/emblem.height;
            }
            else _factory.Heading(_home.transform,UIStrings.Get("game.title"),new Vector2(.045f,.72f),new Vector2(.40f,.93f),64);
            // Actions as text over a soft gold line; the focused one wears the crimson ribbon.
            var actions=_factory.Column(_home.transform,"PrimaryActions",4); _primaryActions=actions;
            actions.anchorMin=new Vector2(.06f,.17f); actions.anchorMax=new Vector2(.29f,.67f);
            actions.GetComponent<VerticalLayoutGroup>().childForceExpandHeight=true;
            _continue=Button(actions,"save.continue",ContinueGame,false,28);
            _newGame=Button(actions,"newGame",NewGame,false,28);
            var load=Button(actions,"save.load",()=>OpenSaves(false),false,28);
            _configuration=Button(actions,"settings",OpenSettings,false,28);
            var access=Button(actions,"accessibility",()=>{OpenSettings();SelectSection(0);},false,28);
            var credits=Button(actions,"credits",()=>{OpenSettings();SelectSection(4);},false,28);
            var exit=Button(actions,"title.exit",RequestQuit,false,28);
            exit.GetComponent<TitleMenuButton>().danger=true;
            foreach(var button in new[]{_continue,_newGame,load,_configuration,access,credits,exit})
            {
                var layout=button.GetComponent<LayoutElement>();layout.minHeight=52;layout.preferredHeight=64;layout.flexibleHeight=1;
                button.GetComponent<TitleMenuButton>().UseMenuStyle();
            }
            _continueHint=UIFactory.Tone(_factory.Label(_home.transform,UIStrings.Get("continue.unavailable"),new Vector2(.06f,.11f),new Vector2(.29f,.16f),20),UITone.Muted);
            _continueHint.alignment=TextAlignmentOptions.Center;
            var version=UIFactory.Tone(_factory.Label(_home.transform,UIStrings.Get("title.version",Application.version),Vector2.zero,Vector2.zero,20),UITone.Muted);
            version.rectTransform.pivot=Vector2.zero; version.rectTransform.anchoredPosition=new Vector2(76,46); version.rectTransform.sizeDelta=new Vector2(600,40);
            version.alignment=TextAlignmentOptions.MidlineLeft;
            _factory.Hint(_home.transform,UIStrings.Get("footer.select"),Vector2.right);
        }
        // Upper-left title and subtitle shared by every full-screen menu page.
        private void Header(Transform parent,string title,string subtitle,out TMP_Text heading,out TMP_Text caption)
        {
            heading=_factory.Heading(parent,title,new Vector2(.04f,.865f),new Vector2(.70f,.95f),56); heading.alignment=TextAlignmentOptions.BottomLeft;
            caption=UIFactory.Fit(UIFactory.Tone(_factory.Label(parent,subtitle,new Vector2(.04f,.825f),new Vector2(.70f,.865f),22),UITone.Muted));
            caption.alignment=TextAlignmentOptions.TopLeft;
        }
        // Screens 02 and 03: slot list in a framed panel, the input mode explained on the right.
        private void BuildSaves()
        {
            _saves=_factory.Rect("Title_Saves",_root,Vector2.zero,Vector2.one).gameObject;
            Header(_saves.transform,UIStrings.Get("save.title"),"",out _savesTitle,out _savesSubtitle);
            var sheet=_factory.Panel("SaveStone",_saves.transform,new Vector2(.06f,.12f),new Vector2(.64f,.79f),true);
            var list=_factory.Column(sheet,"SaveSlots",18); list.Inset(64,56,64,80);
            for(int i=0;i<GameSaveStore.SlotCount;i++)
            {
                int slot=i;
                var row=_factory.Rect("Slot_"+(i+1),list,Vector2.zero,Vector2.one);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight=120;
                var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing=16;
                layout.padding=new RectOffset(0,0,8,8);
                layout.childControlWidth=layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;
                _slotButtons[i]=Button(row,"save.empty",()=>SelectSlot(slot),false,24);
                // Two lines (place, then progress and date): a framed tile the height of the row
                // instead of a one-line ribbon they spilled out of (2026-10-07 audit).
                _slotButtons[i].GetComponent<TitleMenuButton>().slot=true;
                _deleteButtons[i]=Button(row,"save.delete",()=>Confirm("save.deleteConfirm",()=>{GameSaveStore.Delete(slot);RefreshSaves();}),false,22);
                _deleteButtons[i].GetComponent<LayoutElement>().preferredWidth=220;
                _deleteButtons[i].GetComponent<LayoutElement>().flexibleWidth=0;
                _deleteButtons[i].GetComponent<TitleMenuButton>().danger=true;
                UIFactory.Center(_deleteButtons[i]);
                var deletePad=_deleteButtons[i].GetComponent<HorizontalLayoutGroup>(); deletePad.padding.left=deletePad.padding.right=28;
            }
            var side=_factory.Column(_saves.transform,"InputMode",12); side.anchorMin=new Vector2(.68f,.30f); side.anchorMax=new Vector2(.94f,.74f);
            side.GetComponent<VerticalLayoutGroup>().childAlignment=TextAnchor.UpperLeft;
            _factory.Caption(side,UIStrings.Get("title.inputMode"),28).alignment=TextAlignmentOptions.MidlineLeft;
            UIFactory.Tone(_factory.Text(side,UIStrings.Get("title.controlsIntro"),22),UITone.Muted);
            var back=_factory.FooterButton(_saves.transform,UIStrings.Get("footer.back"),ShowHome,Vector2.zero);
            back.transform.parent.name="Back";
            _factory.Hint(_saves.transform,UIStrings.Get("footer.select"),Vector2.right);
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
            _savesTitle.text=UIStrings.Get(create?"newGame":"save.load");
            _savesSubtitle.text=UIStrings.Get(create?"subtitle.NewGame":"subtitle.Saves");
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
            // Screen 24: title and subtitle, a row of tabs, the options in one framed list.
            Header(_settings.transform,UIStrings.Get("settings"),UIStrings.Get("subtitle.Settings"),out _,out _);
            var nav=_factory.Row(_settings.transform,"Sections",12); nav.anchorMin=new Vector2(.04f,.755f); nav.anchorMax=new Vector2(.96f,.815f);
            nav.offsetMin=nav.offsetMax=Vector2.zero;
            var sheet=_factory.Panel("SettingsStone",_settings.transform,new Vector2(.12f,.10f),new Vector2(.88f,.745f),true);
            var body=_factory.Scroll(sheet,Vector2.zero,Vector2.one); ((RectTransform)body.parent.parent).Inset(72,52,72,76);
            string[] sections={"accessibility","settings.audio","settings.graphics","controls","credits"};
            for(int i=0;i<sections.Length;i++)
            {
                int index=i; var group=_factory.Column(body,"Settings_"+i,16); _groups.Add(group);
                var tab=UIFactory.Tab(Button(nav,sections[i],()=>SelectSection(index)));
                _tabs.Add(tab.GetComponent<TitleMenuButton>());
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
            Toggle(access,"access.combatAnswers",()=>Settings.Values.combatAnswers,v=>Settings.Values.combatAnswers=v);
            Slider(access,"access.camera",0,1,()=>Settings.Values.cameraMotion,v=>Settings.Values.cameraMotion=v);
            Slider(access,"access.flash",0,1,()=>Settings.Values.flashIntensity,v=>Settings.Values.flashIntensity=v);
            new DevicesPage(_factory,Settings).Build(access);
            Button(access,"access.reset",()=>Confirm("confirm.access",Settings.ResetAccessibility));
            var audio=_groups[1];
            Slider(audio,"audio.master",0,1,()=>Settings.Values.master,v=>Settings.Values.master=v);
            Slider(audio,"audio.music",0,1,()=>Settings.Values.music,v=>Settings.Values.music=v);
            Slider(audio,"audio.effects",0,1,()=>Settings.Values.effects,v=>Settings.Values.effects=v);
            Slider(audio,"audio.ambience",0,1,()=>Settings.Values.ambience,v=>Settings.Values.ambience=v);
            Slider(audio,"audio.voices",0,1,()=>Settings.Values.voices,v=>Settings.Values.voices=v);
            Slider(audio,"audio.ui",0,1,()=>Settings.Values.uiVolume,v=>Settings.Values.uiVolume=v);
            new GraphicsPage(_factory,Settings).Build(_groups[2]);
            var controls=_groups[3];
            Toggle(controls,"settings.tutorials",()=>Settings.Values.tutorials,v=>Settings.Values.tutorials=v);
            new ControlsPage(_factory,Settings,Confirm).Build(controls);
            new CreditsPage(_factory,Settings).Build(_groups[4]);
            _back=_factory.FooterButton(_settings.transform,UIStrings.Get("footer.back"),BackToHome,Vector2.zero);
            _factory.Hint(_settings.transform,UIStrings.Get("footer.change"),Vector2.right);
            SelectSection(0);
        }
        // Screen 27: framed modal with the alert emblem; the safe action first and focused.
        private void BuildConfirmation()
        {
            _quit=_factory.Rect("Title_Confirmation",_root,Vector2.zero,Vector2.one).gameObject;
            var sheet=_factory.Panel("ConfirmationStone",_quit.transform,new Vector2(.32f,.22f),new Vector2(.68f,.78f),true);
            var content=_factory.Column(sheet,"ConfirmationContent",18); content.Inset(80,60,80,84);
            content.GetComponent<VerticalLayoutGroup>().childAlignment=TextAnchor.MiddleCenter;
            _factory.Icon(content,UIIcon.Alert,60,_theme.paleGold);
            _confirmText=UIFactory.Tone(_factory.Text(content,"",38,true),UITone.Gold); _confirmText.alignment=TextAlignmentOptions.Center;
            _cancelQuit=Button(content,"cancel",CancelConfirmation,true);
            _confirmQuit=Button(content,"confirm",()=>{var action=_confirmAction; _confirmAction=null; CloseConfirmation(false); action?.Invoke();});
            UIFactory.Center(_cancelQuit); UIFactory.Center(_confirmQuit);
            _factory.Hint(_quit.transform,UIStrings.Get("footer.cancel"),Vector2.zero);
        }
        // Screen 07: the plaza illustration, its name in the monumental serif, real progress only.
        private void BuildLoading()
        {
            _loading=_factory.Rect("Title_Loading",_root,Vector2.zero,Vector2.one).gameObject;
            var texture=UIBacata.Art("Plaza_Nunez");
            if(texture!=null)
            {
                var art=_factory.Rect("Illustration",_loading.transform,Vector2.zero,Vector2.one);
                var raw=art.gameObject.AddComponent<RawImage>(); raw.texture=texture; raw.raycastTarget=false;
                var fit=art.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio=(float)texture.width/texture.height;
                var veil=_factory.Rect("Veil",_loading.transform,Vector2.zero,Vector2.one).gameObject.AddComponent<TitleMenuBackdrop>();
                veil.raycastTarget=false; veil.readingEdge=.42f;
            }
            var title=_factory.Heading(_loading.transform,UIStrings.Get("world.plaza"),new Vector2(.05f,.26f),new Vector2(.80f,.36f),64);
            title.alignment=TextAlignmentOptions.BottomLeft;
            var body=_factory.Column(_loading.transform,"LoadingContent",14); body.anchorMin=new Vector2(.05f,.08f); body.anchorMax=new Vector2(.62f,.25f);
            UIFactory.Tone(_factory.Text(body,UIStrings.Get("loading.tip"),24),UITone.Muted);
            var track=_factory.Rect("Progress",body,Vector2.zero,Vector2.one); track.gameObject.AddComponent<LayoutElement>().preferredHeight=18;
            _progress=_factory.Bar(track,"ActualLoadProgress",Vector2.zero,Vector2.one);
            _loadStatus=UIFactory.Tone(_factory.Text(body,UIStrings.Get("title.loading"),22),UITone.GoldLight);
            _loadStatus.alignment=TextAlignmentOptions.MidlineRight;
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
            _factory.Toggle(parent,key,get,v=>{set(v);Settings.Apply();},Settings);
        }
        private void Choice(Transform parent,string key,string[] values,Func<int> get,Action<int> set)
        {
            _factory.Choice(parent,key,values,get,v=>{set(v);Settings.Apply();},Settings);
        }
        private void Slider(Transform parent,string key,float min,float max,Func<float> get,Action<float> set)
        {
            var slider=_factory.Slider(parent,key,min,max,get(),v=>{set(v);Settings.Apply();});
            slider.gameObject.AddComponent<UIValueBinding>().Bind(Settings,()=>{slider.SetValueWithoutNotify(get());UIFactory.SetValue(slider.transform.parent,UIFactory.Format(get(),min,max));});
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
            _shade.gameObject.SetActive(view!=TitleView.Home&&view!=TitleView.Loading);
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
            SetView(_beforeConfirm,_beforeConfirmFocus!=null?_beforeConfirmFocus:(_beforeConfirm==TitleView.Settings?_tabs[_section].gameObject:_newGame.gameObject));
        }
        private void ApplyPresentation()
        {
            foreach(var binding in GetComponentsInChildren<UIStyleBinding>(true)) binding.Apply(_theme,Settings.Values);
            foreach(var button in GetComponentsInChildren<TitleMenuButton>(true)) button.Refresh();
            _shade.SetContrast(Settings.Values.highContrast);
            // Larger text widens the action column while the cover remains visible.
            float right=Mathf.Lerp(.29f,.38f,(Settings.Values.textScale-1)*2);
            _primaryActions.anchorMin=new Vector2(.06f,.17f);
            _primaryActions.anchorMax=new Vector2(right,.67f);
            if(Settings.Values.reducedMotion) _homeFade.alpha=1;
        }
        // Interface cues come from the shared bank (wood and clay, see Specs/Audio).
        public void PlayCue(bool activation)
        {
            if(Time.unscaledTime-_lastCue<.10f)return;
            _lastCue=Time.unscaledTime;
            GameAudio.UI(activation?UICue.Confirm:UICue.Focus);
        }
        public void NewGame()
        {
            OpenSaves(true);
        }
        private void StartGame(int slot,bool load)
        {
            if(IsLoading)return;
            if(!Application.CanStreamedLevelBeLoaded(TutorialPath)) {ShowLoadError();return;}
            // A new game in a slot also starts both worlds and the story over (their own keys per slot).
            if(!load) { MIProgress.Erase(slot); MSProgress.Erase(slot); CampaignProgress.Erase(slot); }
            GameSaveStore.Begin(slot,load);
            _loadingBack.gameObject.SetActive(false);
            Settings.Values.configured=true;Settings.Apply();Settings.Flush();
            SetView(TitleView.Loading,null);
            // The story decides the first scene: the prologue in Bacatá until the plaza is reached.
            StartCoroutine(LoadTutorial(CampaignScenes.EntryScene()));
        }
        private IEnumerator LoadTutorial(string path)
        {
            IsLoading=true; yield return null;
            AsyncOperation operation=null;
            try {operation=SceneManager.LoadSceneAsync(path,LoadSceneMode.Single);}
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
        private void Update()
        {
            if(Settings==null)return;
            var keyboard=Keyboard.current;
            if(!IsLoading&&Time.frameCount>_changedFrame)
            {
                // While the Controles page waits for a new key, Esc only cancels that wait.
            bool back=!GameBindings.Listening&&((keyboard!=null&&keyboard.escapeKey.wasPressedThisFrame)||(Gamepad.current!=null&&Gamepad.current.buttonEast.wasPressedThisFrame));
                if(back)
                {
                    if(CurrentView==TitleView.Quit)CancelConfirmation();
                    else if(CurrentView==TitleView.Settings)BackToHome();
                    else if(CurrentView==TitleView.Saves)ShowHome();
                    else if(CurrentView==TitleView.Home)RequestQuit();
                }
                if(keyboard!=null&&!GameBindings.Listening&&keyboard.mKey.wasPressedThisFrame&&CurrentView==TitleView.Home)
                {if(Settings.Values.music>.001f){_muteRestore=Settings.Values.music;Settings.Values.music=0;}else Settings.Values.music=_muteRestore;Settings.Apply();}
                if(keyboard!=null&&!GameBindings.Listening&&keyboard.tabKey.wasPressedThisFrame)NavigateTab(keyboard.shiftKey.isPressed);
            }
            // The music slider sets the menu piece like the rest of the game's music (about -26 dB RMS).
            if(_music!=null)_music.volume=Mathf.MoveTowards(_music.volume,IsLoading?0:Settings.Values.music,Time.unscaledDeltaTime*.45f);
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
            if(Instance==this)Instance=null;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Nemequene.UI.Editor
{
    public static class UIAssetBuilder
    {
        private const string Root = "Assets/_Game/UI/";
        [MenuItem("Tools/Nemequene/UI/Build assets")]
        public static void Build()
        {
            string[] dirs = {"Resources/Nemequene", "Prefabs", "Themes", "Art", "Audio", "Icons", "Materials", "Scenes", "Animations", "ScriptableObjects", "Documentation"};
            foreach (string d in dirs) Directory.CreateDirectory(Root + d);
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset") == null)
            {
                string package = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories).FirstOrDefault();
                if (package == null) throw new InvalidOperationException("TMP essentials package missing.");
                AssetDatabase.ImportPackage(package, false);
            }
            AssetDatabase.Refresh();
            var body = Font("NotoSans-Regular"); var title = Font("NotoSerif-Regular");
            const string path = Root + "Resources/Nemequene/Theme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(path);
            if (theme == null) { theme = ScriptableObject.CreateInstance<UITheme>(); AssetDatabase.CreateAsset(theme, path); }
            theme.bodyFont = body; theme.titleFont = title;
            theme.displayFont = UIDisplayFontSetup.Create(title);
            theme.stonePanel = Sprite("StonePanel-v2");
            theme.parchmentPanel = Sprite("ParchmentPanel-v2");
            theme.stoneButton = Sprite("Menu_StoneButton");
            EditorUtility.SetDirty(theme);
            var root = new GameObject("UI_Nemequene", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIManager));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
            PrefabUtility.SaveAsPrefabAsset(root, Root + "Resources/Nemequene/UI_Root.prefab"); UnityEngine.Object.DestroyImmediate(root);
            var f = new UIFactory(theme);
            Save(f.Button(null, UIStrings.Get("begin"), null, true).gameObject, "UI_Button_Primary");
            Save(f.Button(null, UIStrings.Get("back"), null).gameObject, "UI_Button_Secondary");
            var destructive = f.Button(null, UIStrings.Get("quit"), null);
            destructive.GetComponent<TitleMenuButton>().danger = true;
            destructive.GetComponent<TitleMenuButton>().Refresh();
            Save(destructive.gameObject, "UI_Button_Destructive");
            var panel = f.Panel("UI_Panel", null, Vector2.zero, Vector2.one, true); Save(panel.gameObject, "UI_Panel");
            var counter = f.Text(null, "0 / 0", 24); Save(counter.gameObject, "UI_ResourceCounter");
            BuildComponentLibrary(f);
            string cue = "Assets/_Game/Audio/PlazaNunez";
            if (Directory.Exists(cue))
            {
                var clip = AssetDatabase.FindAssets("t:AudioClip", new[]{cue}).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.ToLowerInvariant().Contains("inspect"));
                if (clip != null && !File.Exists(Root + "Resources/Nemequene/UI_Select.wav")) AssetDatabase.CopyAsset(clip, Root + "Resources/Nemequene/UI_Select.wav");
            }
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("NEMEQUENE_UI_ASSETS_READY");
        }
        private static void Save(GameObject obj, string name)
        {
            obj.name = name; PrefabUtility.SaveAsPrefabAsset(obj, Root + "Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(obj);
        }
        private static void BuildComponentLibrary(UIFactory f)
        {
            string[] buttons={"UI_Button_Icon","UI_Button_Shortcut","UI_Tab","UI_Selector","UI_CombatOption"};
            foreach(string name in buttons)
            {
                var button=f.Button(null,UIStrings.Get("component.action"),null);
                var state=button.gameObject.AddComponent<UIStatePresenter>();
                state.Configure(f.Theme,button.GetComponentInChildren<TMP_Text>(),button.GetComponent<Image>(),"component.action");
                Save(button.gameObject,name);
            }
            foreach(string name in new[]{"UI_Checkbox","UI_Switch"})
            {
                var row=f.Rect(name,null,Vector2.zero,Vector2.one); row.sizeDelta=new Vector2(520,64);
                var background=row.gameObject.AddComponent<Image>(); background.color=f.Theme.panel;
                var mark=f.Panel("Check",row,new Vector2(.03f,.3f),new Vector2(.08f,.7f)); mark.GetComponent<Image>().color=f.Theme.gold;
                var label=f.Label(row,UIStrings.Get("component.option"),new Vector2(.12f,.1f),new Vector2(.96f,.9f));
                var toggle=row.gameObject.AddComponent<Toggle>(); toggle.targetGraphic=background; toggle.graphic=mark.GetComponent<Image>();
                Save(row.gameObject,name);
            }
            var slider=f.Slider(null,"component.value",0,1,.5f,v=>{}); Save(slider.transform.parent.gameObject,"UI_Slider");
            string[] panels={"UI_CalibrationField","UI_TooltipPanel","UI_Modal_Confirmation","UI_Notification","UI_ObjectiveIndicator","UI_InteractionIndicator",
                "UI_Indicator_Microphone","UI_Indicator_HandTracking","UI_HandCursor","UI_RecognitionIndicator","UI_CulturalObjectCard","UI_ArchiveEntry","UI_MapEntry","UI_Subtitle","UI_Dialogue"};
            foreach(string name in panels)
            {
                var panel=f.Panel(name,null,Vector2.zero,Vector2.one); panel.sizeDelta=new Vector2(560,144);
                var label=f.Label(panel,UIStrings.Get("component.status"),new Vector2(.05f,.12f),new Vector2(.95f,.88f));
                panel.gameObject.AddComponent<UIStatePresenter>().Configure(f.Theme,label,panel.GetComponent<Image>(),"component.status");
                Save(panel.gameObject,name);
            }
            foreach(string name in new[]{"UI_HealthBar","UI_ProgressBar"})
            {
                var bar=f.Bar(null,name,Vector2.zero,Vector2.one); var root=bar.transform.parent.GetComponent<RectTransform>();
                root.sizeDelta=new Vector2(400,16); UIFactory.Fill(bar,.6f); Save(root.gameObject,name);
            }
        }
        private static TMP_FontAsset Font(string name)
        {
            string path = Root + "Fonts/" + name + " SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path); if (existing != null) return existing;
            var font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/" + name + ".ttf");
            if (font == null) throw new InvalidOperationException("Missing font " + name);
            var asset = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            asset.name = name + " SDF";
            string characters = new string(Enumerable.Range(32,224).Select(n => (char)n).ToArray()) + "…—–‘’“”→";
            asset.TryAddCharacters(characters, out string missing);
            AssetDatabase.CreateAsset(asset, path);
            foreach (var atlas in asset.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, asset);
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset); return asset;
        }
        private static Sprite Sprite(string name)
        {
            string path = Root + "Resources/Nemequene/" + name + ".png";
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        }
        public static void BuildBatch() { Build(); }
        public static void WindowsBuild()
        {
            Build(); TitleMenuSceneBuilder.Build(); Directory.CreateDirectory("Builds/Nemequene");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray(),
                locationPathName = "Builds/Nemequene/Nemequene.exe", target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            Debug.Log("NEMEQUENE_WINDOWS_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors);
            if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}

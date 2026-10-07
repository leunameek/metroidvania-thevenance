using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nemequene.UI.Editor
{
    public static class TitleMenuSceneBuilder
    {
        [MenuItem("Tools/Nemequene/UI/Build isolated main menu")]
        public static void Build()
        {
            string path=TitleMenuController.ScenePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ImportArt();
            if (!File.Exists(path))
            {
                var previous=SceneManager.GetActiveScene();
                bool replaceUntitled=string.IsNullOrEmpty(previous.path);
                if(replaceUntitled&&!Application.isBatchMode)
                    throw new System.InvalidOperationException("Save the current scene before building the title menu.");
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,replaceUntitled?NewSceneMode.Single:NewSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                var camera=new GameObject("Menu_Camera",typeof(Camera),typeof(AudioListener)); camera.tag="MainCamera";
                camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor=UITheme.Hex("111815");
                camera.GetComponent<Camera>().orthographic=true; camera.GetComponent<Camera>().nearClipPlane=.1f;
                var root=new GameObject("Nemequene_MainMenu",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(TitleMenuController));
                root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                EditorSceneManager.SaveScene(scene,path);
                if(!replaceUntitled)
                {
                    EditorSceneManager.CloseScene(scene,true);
                    if(previous.IsValid()) SceneManager.SetActiveScene(previous);
                }
            }
            var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=path).ToList();
            scenes.Insert(0,new EditorBuildSettingsScene(path,true)); EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("NEMEQUENE_TITLE_SCENE_READY");
        }
        private static void ImportArt()
        {
            AssetDatabase.Refresh();
            const string art="Assets/_Game/UI/Resources/Nemequene/Menu_BacataValley.png";
            if(AssetImporter.GetAtPath(art) is TextureImporter texture)
            {
                texture.textureType=TextureImporterType.Default;texture.mipmapEnabled=false;texture.isReadable=false;
                texture.maxTextureSize=4096;texture.textureCompression=TextureImporterCompression.CompressedHQ;texture.SaveAndReimport();
            }
            const string emblem="Assets/_Game/UI/Resources/Nemequene/Title_BacataMountains.png";
            if(AssetImporter.GetAtPath(emblem) is TextureImporter logo)
            {
                logo.textureType=TextureImporterType.Default;logo.mipmapEnabled=false;logo.isReadable=false;
                logo.alphaSource=TextureImporterAlphaSource.FromInput;logo.alphaIsTransparency=true;
                logo.npotScale=TextureImporterNPOTScale.None;logo.wrapMode=TextureWrapMode.Clamp;
                logo.maxTextureSize=2048;logo.textureCompression=TextureImporterCompression.Uncompressed;logo.SaveAndReimport();
            }
            const string plate="Assets/_Game/UI/Resources/Nemequene/Menu_StoneButton.png";
            if(AssetImporter.GetAtPath(plate) is TextureImporter skin)
            {
                skin.textureType=TextureImporterType.Sprite;skin.spriteImportMode=SpriteImportMode.Multiple;
                skin.mipmapEnabled=false;skin.isReadable=false;skin.alphaIsTransparency=true;
                skin.alphaSource=TextureImporterAlphaSource.FromInput;skin.npotScale=TextureImporterNPOTScale.None;
                skin.wrapMode=TextureWrapMode.Clamp;skin.maxTextureSize=4096;skin.textureCompression=TextureImporterCompression.Uncompressed;
                var bounds=AlphaBounds(plate);
                // Define the sprite's region inside the original PNG; leave source pixels untouched.
#pragma warning disable 618
                skin.spritesheet=new[]{new SpriteMetaData{name="Menu_StoneButton",rect=bounds,pivot=new Vector2(.5f,.5f),
                    alignment=(int)SpriteAlignment.Center,border=new Vector4(bounds.width*.18f,bounds.height*.14f,bounds.width*.18f,bounds.height*.14f)}};
#pragma warning restore 618
                skin.SaveAndReimport();
            }
            const string music="Assets/_Game/UI/Resources/Nemequene/Menu_Bruma.wav";
            if(AssetImporter.GetAtPath(music) is AudioImporter audio)
            {
                var sample=audio.defaultSampleSettings;sample.loadType=AudioClipLoadType.Streaming;
                sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.quality=.7f;
                audio.defaultSampleSettings=sample;audio.loadInBackground=true;audio.SaveAndReimport();
            }
        }
        private static Rect AlphaBounds(string path)
        {
            var source=new Texture2D(2,2);
            try
            {
                source.LoadImage(File.ReadAllBytes(path));var pixels=source.GetPixels32();
                int left=source.width,right=0,bottom=source.height,top=0;
                for(int y=0;y<source.height;y++)for(int x=0;x<source.width;x++)
                    if(pixels[y*source.width+x].a>24){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
                return new Rect(Mathf.Max(0,left-2),Mathf.Max(0,bottom-2),Mathf.Min(source.width-left+2,right-left+5),Mathf.Min(source.height-bottom+2,top-bottom+5));
            }
            finally {Object.DestroyImmediate(source);}
        }
    }
}

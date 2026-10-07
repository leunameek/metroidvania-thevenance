using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Nemequene.UI.Editor
{
    // Builds the TMP asset for Cinzel (SIL OFL, Fonts/OFL-Cinzel.txt), the carved display face of
    // the reference headers, and assigns it to Theme.displayFont. Runs once after import; the menu
    // item rebuilds it. Missing glyphs fall back to Noto Serif.
    [InitializeOnLoad]
    public static class UIDisplayFontSetup
    {
        private const string Source = "Assets/_Game/UI/Fonts/Cinzel.ttf";
        private const string Target = "Assets/_Game/UI/Fonts/Cinzel SDF.asset";
        private const string ThemePath = "Assets/_Game/UI/Resources/Nemequene/Theme.asset";
        private const string Characters = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
            "¡¿«»·ÁÉÍÓÚÜÑáéíóúüñ–—…‘’“”";
        static UIDisplayFontSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
                var theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemePath);
                if (theme != null && theme.displayFont == null && AssetDatabase.LoadAssetAtPath<Font>(Source) != null) Assign(theme);
            };
        }
        public static void Ensure()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemePath);
            if (theme != null && theme.displayFont == null) Assign(theme);
        }
        [MenuItem("Tools/Nemequene/UI/Build display font (Cinzel)")]
        public static void Rebuild()
        {
            AssetDatabase.DeleteAsset(Target);
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemePath);
            if (theme != null) Assign(theme);
        }
        public static TMP_FontAsset Create(TMP_FontAsset fallback)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Target);
            if (existing != null) return existing;
            var font = AssetDatabase.LoadAssetAtPath<Font>(Source);
            if (font == null) { Debug.LogError("NEMEQUENE_DISPLAY_FONT_FAILED: missing " + Source); return null; }
            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (asset == null) { Debug.LogError("NEMEQUENE_DISPLAY_FONT_FAILED: TMP could not read " + Source); return null; }
            asset.name = "Cinzel SDF";
            AssetDatabase.CreateAsset(asset, Target);
            asset.atlasTextures[0].name = "Cinzel SDF Atlas"; AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = "Cinzel SDF Material"; AssetDatabase.AddObjectToAsset(asset.material, asset);
            if (fallback != null) asset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
            asset.TryAddCharacters(Characters, out string missing);
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
            Debug.Log("NEMEQUENE_DISPLAY_FONT_OK: " + Target + (string.IsNullOrEmpty(missing) ? "" : " · fallback for: " + missing));
            return asset;
        }
        private static void Assign(UITheme theme)
        {
            var asset = Create(theme.titleFont);
            if (asset == null) return;
            theme.displayFont = asset; EditorUtility.SetDirty(theme); AssetDatabase.SaveAssets();
        }
    }
}

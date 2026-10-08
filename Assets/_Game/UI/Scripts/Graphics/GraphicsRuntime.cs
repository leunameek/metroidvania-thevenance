using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Nemequene.UI
{
    // Applies the Gráficos page to the running game, in every scene (menu, Bacatá, plaza and both
    // worlds), including cameras the directors create at runtime:
    //  · a clone of the URP asset (render scale, upscaler, MSAA, shadow distance and maps), so the
    //    project asset is never edited;
    //  · QualitySettings (V-Sync, frame cap, queued frames, texture mip limit, anisotropy, LOD bias);
    //  · a global override volume above the scenes' own (bloom, motion blur, depth of field, film
    //    grain, brightness and gamma, HDR paper white);
    //  · each base camera (post-processing on, antialiasing mode, draw distance).
    // In the editor everything it touched goes back when play mode stops.
    [DefaultExecutionOrder(-300)]
    public sealed class GraphicsRuntime : MonoBehaviour
    {
        public static readonly float[] ViewFactors = { .45f, .7f, 1f, 1f };
        public static readonly float[] LodBias = { .5f, 1f, 1.5f, 2.5f };
        public static readonly int[] AnisoLevels = { 1, 2, 4, 8, 16 };

        private static GraphicsRuntime _instance;
        private static UISettings _values;
        private static bool _displayApplied;

        private UniversalRenderPipelineAsset _clone;
        private RenderPipelineAsset _originalQuality;
        private readonly Dictionary<ScriptableRendererFeature, bool> _features = new Dictionary<ScriptableRendererFeature, bool>();
        private readonly Dictionary<int, float> _farClip = new Dictionary<int, float>();
        private Volume _volume;
        private Bloom _bloom; private MotionBlur _motionBlur; private DepthOfField _depth; private FilmGrain _grain;
        private LiftGammaGain _liftGammaGain; private Tonemapping _tonemapping;
        private float _nextScan, _sceneReflection = 1;
        private int _vSync, _maxQueued, _mipLimit; private float _lod; private AnisotropicFiltering _aniso;
        private bool _realtimeProbes;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { _instance = null; _values = null; _displayApplied = false; }

        // Before the first scene: the stored choice is on screen before any menu is built.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var settings = new SettingsManager();
            Apply(settings.Values);
            if (!_displayApplied && !Application.isBatchMode && !Application.isEditor)
            {
                _displayApplied = true;
                ApplyDisplay(settings.Values.screenWidth, settings.Values.screenHeight, settings.Values.displayMode,
                    settings.Values.refreshNumerator, settings.Values.refreshDenominator);
            }
        }

        public static void Apply(UISettings values)
        {
            if (!Application.isPlaying || values == null) return;
            _values = values;
            if (_instance == null)
            {
                var host = new GameObject("Graficos_Ajustes");
                DontDestroyOnLoad(host); host.hideFlags = HideFlags.HideInHierarchy;
                _instance = host.AddComponent<GraphicsRuntime>();
            }
            _instance.Refresh();
        }

        // Screen mode, size and refresh rate (after the player confirms on the Gráficos page).
        public static void ApplyDisplay(int width, int height, int mode, int refreshNumerator, int refreshDenominator)
        {
            if (Application.isBatchMode) return;
            if (width < 640 || height < 480) { width = Screen.currentResolution.width; height = Screen.currentResolution.height; }
            var screenMode = SettingsManager.ScreenMode(mode);
            if (refreshNumerator > 0)
                Screen.SetResolution(width, height, screenMode, new RefreshRate { numerator = (uint)refreshNumerator, denominator = (uint)Mathf.Max(1, refreshDenominator) });
            else Screen.SetResolution(width, height, screenMode);
        }

        public static bool HdrAvailable => HDROutputSettings.main != null && HDROutputSettings.main.available;
        public static bool HdrActive => HdrAvailable && HDROutputSettings.main.active;
        public static bool StpSupported => SystemInfo.supportsComputeShaders;

        private void Awake()
        {
            _vSync = QualitySettings.vSyncCount; _maxQueued = QualitySettings.maxQueuedFrames;
            _mipLimit = QualitySettings.globalTextureMipmapLimit; _lod = QualitySettings.lodBias;
            _aniso = QualitySettings.anisotropicFiltering; _realtimeProbes = QualitySettings.realtimeReflectionProbes;
            _originalQuality = QualitySettings.renderPipeline;
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset source)
            {
                _clone = Instantiate(source); _clone.name = source.name + " (Ajustes)";
                QualitySettings.renderPipeline = _clone;
                foreach (var data in _clone.rendererDataList)
                    if (data != null)
                        foreach (var feature in data.rendererFeatures)
                            if (feature is ScreenSpaceAmbientOcclusion && !_features.ContainsKey(feature)) _features[feature] = feature.isActive;
            }
            BuildVolume();
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.quitting += Restore;
            CaptureSceneReflection();
        }

        private void BuildVolume()
        {
            var go = new GameObject("Graficos_Volumen"); go.transform.SetParent(transform, false);
            _volume = go.AddComponent<Volume>(); _volume.isGlobal = true; _volume.priority = 1000;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>(); profile.name = "Ajustes graficos";
            _volume.sharedProfile = profile;
            _bloom = profile.Add<Bloom>(); _motionBlur = profile.Add<MotionBlur>(); _depth = profile.Add<DepthOfField>();
            _grain = profile.Add<FilmGrain>(); _liftGammaGain = profile.Add<LiftGammaGain>(); _tonemapping = profile.Add<Tonemapping>();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) { _farClip.Clear(); CaptureSceneReflection(); }
            Refresh();
        }

        private void CaptureSceneReflection() => _sceneReflection = RenderSettings.reflectionIntensity;

        private void Refresh()
        {
            var v = _values; if (v == null) return;
            if (_clone != null)
            {
                if (QualitySettings.renderPipeline != _clone) QualitySettings.renderPipeline = _clone;
                _clone.renderScale = v.renderScale;
                _clone.upscalingFilter = v.upscaler == 1 ? UpscalingFilterSelection.FSR
                    : v.upscaler == 2 && StpSupported ? UpscalingFilterSelection.STP : UpscalingFilterSelection.Auto;
                _clone.msaaSampleCount = v.antialiasing == 4 ? 2 : v.antialiasing == 5 ? 4 : v.antialiasing == 6 ? 8 : 1;
                int q = v.shadowQuality;
                _clone.shadowDistance = new[] { 0f, 20f, 40f, 60f, 100f }[q];
                _clone.mainLightShadowmapResolution = new[] { 256, 1024, 2048, 2048, 4096 }[q];
                _clone.additionalLightsShadowmapResolution = new[] { 256, 512, 1024, 2048, 4096 }[q];
                _clone.shadowCascadeCount = new[] { 1, 1, 2, 4, 4 }[q];
            }
            foreach (var pair in _features) if (pair.Key != null) pair.Key.SetActive(pair.Value && v.ambientOcclusion);

            QualitySettings.vSyncCount = v.vSync ? 1 : 0;
            Application.targetFrameRate = v.frameLimit;
            // Same idea as the vendors' latency switches: the CPU may run at most one frame ahead.
            QualitySettings.maxQueuedFrames = v.lowLatency ? 1 : 2;
            QualitySettings.globalTextureMipmapLimit = 2 - v.textureQuality;
            if (v.anisotropic == 0) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            else
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                Texture.SetGlobalAnisotropicFilteringLimits(AnisoLevels[v.anisotropic], AnisoLevels[v.anisotropic]);
            }
            QualitySettings.lodBias = LodBias[v.viewDistance];
            QualitySettings.realtimeReflectionProbes = v.reflections == 2;
            RenderSettings.reflectionIntensity = v.reflections == 0 ? 0 : _sceneReflection;

            // A disabled effect is overridden to nothing; an enabled bloom keeps each scene's own.
            _bloom.active = !v.bloom; _bloom.intensity.Override(0);
            _motionBlur.active = true;
            _motionBlur.intensity.Override(v.motionBlur ? .35f : 0);
            _motionBlur.quality.Override(MotionBlurQuality.Medium);
            _depth.active = true;
            _depth.mode.Override(v.depthOfField ? DepthOfFieldMode.Gaussian : DepthOfFieldMode.Off);
            _depth.gaussianStart.Override(18); _depth.gaussianEnd.Override(70); _depth.gaussianMaxRadius.Override(1);
            _grain.active = true;
            _grain.type.Override(FilmGrainLookup.Medium1); _grain.intensity.Override(v.filmGrain ? .3f : 0); _grain.response.Override(.8f);
            bool tone = Mathf.Abs(v.brightness) > .001f || Mathf.Abs(v.gamma) > .001f;
            _liftGammaGain.active = tone;
            _liftGammaGain.gamma.Override(new Vector4(1, 1, 1, v.gamma));
            _liftGammaGain.gain.Override(new Vector4(1, 1, 1, v.brightness));
            if (HdrAvailable && HDROutputSettings.main.active != v.hdr) HDROutputSettings.main.RequestHDRModeChange(v.hdr);
            _tonemapping.active = HdrActive;
            _tonemapping.detectPaperWhite.Override(false); _tonemapping.paperWhite.Override(v.paperWhite);
            ApplyCameras();
        }

        private void Update()
        {
            // Directors and cutscenes create cameras at any time: look for new ones twice a second.
            if (Time.unscaledTime < _nextScan) return;
            _nextScan = Time.unscaledTime + .5f;
            ApplyCameras();
        }

        private void ApplyCameras()
        {
            var v = _values; if (v == null) return;
            foreach (var camera in Camera.allCameras)
            {
                if (camera == null || camera.targetTexture != null || camera.cameraType != CameraType.Game) continue;
                if (!camera.TryGetComponent(out UniversalAdditionalCameraData data)) data = camera.GetUniversalAdditionalCameraData();
                if (data == null || data.renderType != CameraRenderType.Base) continue;
                data.renderPostProcessing = true;
                data.antialiasing = v.antialiasing == 1 ? AntialiasingMode.FastApproximateAntialiasing
                    : v.antialiasing == 2 ? AntialiasingMode.SubpixelMorphologicalAntiAliasing
                    : v.antialiasing == 3 ? AntialiasingMode.TemporalAntiAliasing : AntialiasingMode.None;
                data.antialiasingQuality = AntialiasingQuality.High;
                camera.allowMSAA = v.antialiasing >= 4;
                int id = camera.GetInstanceID();
                if (!_farClip.TryGetValue(id, out float far)) _farClip[id] = far = camera.farClipPlane;
                // Never closer than 60 m (or the camera's own range, if shorter): rooms stay whole.
                camera.farClipPlane = Mathf.Max(far * ViewFactors[v.viewDistance], Mathf.Min(far, 60f));
            }
        }

        private void Restore()
        {
            foreach (var pair in _features) if (pair.Key != null) pair.Key.SetActive(pair.Value);
            QualitySettings.vSyncCount = _vSync; QualitySettings.maxQueuedFrames = _maxQueued;
            QualitySettings.globalTextureMipmapLimit = _mipLimit; QualitySettings.lodBias = _lod;
            QualitySettings.anisotropicFiltering = _aniso; QualitySettings.realtimeReflectionProbes = _realtimeProbes;
            if (QualitySettings.renderPipeline == _clone) QualitySettings.renderPipeline = _originalQuality;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.quitting -= Restore;
            Restore();
            if (_clone != null) Destroy(_clone);
            if (_volume != null && _volume.sharedProfile != null) Destroy(_volume.sharedProfile);
            if (_instance == this) _instance = null;
        }
    }
}

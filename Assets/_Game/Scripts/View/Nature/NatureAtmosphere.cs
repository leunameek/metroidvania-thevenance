using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Sky, light and grade of an open place built at runtime: the procedural Andean sky (green cerros
// and far snowy peaks around the horizon), trilight ambient matched to it, haze in the sky's own
// colour and a soft post-process grade (tonemapping, warmth, bloom on the sun, vignette).
public static class NatureAtmosphere
{
    public struct Look
    {
        public Color Zenith, Sky, Horizon, Fog, Sun, Cloud, Far, Mid, Near, Rock;
        public float CloudCover, MountainScale, Haze, FogDensity, SunIntensity;
        public Color AmbientSky, AmbientEquator, AmbientGround;
        public float Exposure, Contrast, Saturation, Bloom, Vignette;
        public Color Filter;
    }

    // Morning over the savanna of Bacatá.
    public static readonly Look Morning = new Look
    {
        Zenith = new Color(.22f, .50f, .82f), Sky = new Color(.48f, .72f, .93f), Horizon = new Color(.86f, .91f, .93f),
        Fog = new Color(.74f, .82f, .86f), Sun = new Color(1f, .93f, .80f), Cloud = new Color(1f, .98f, .95f),
        Far = new Color(.42f, .52f, .66f), Mid = new Color(.30f, .45f, .36f), Near = new Color(.26f, .40f, .24f), Rock = new Color(.50f, .45f, .40f),
        CloudCover = .42f, MountainScale = 1.1f, Haze = .32f, FogDensity = .0045f, SunIntensity = 1.35f,
        AmbientSky = new Color(.58f, .68f, .82f), AmbientEquator = new Color(.60f, .60f, .52f), AmbientGround = new Color(.30f, .28f, .20f),
        Exposure = .05f, Contrast = 10, Saturation = 12, Bloom = .35f, Vignette = .2f, Filter = new Color(1f, .98f, .94f),
    };

    // The village burning under the invasion: low ochre smoke over everything.
    public static readonly Look Smoke = new Look
    {
        Zenith = new Color(.30f, .27f, .27f), Sky = new Color(.48f, .38f, .32f), Horizon = new Color(.62f, .44f, .32f),
        Fog = new Color(.36f, .27f, .23f), Sun = new Color(1f, .58f, .36f), Cloud = new Color(.36f, .30f, .28f),
        Far = new Color(.38f, .30f, .28f), Mid = new Color(.30f, .25f, .22f), Near = new Color(.25f, .21f, .18f), Rock = new Color(.36f, .30f, .26f),
        CloudCover = .85f, MountainScale = 1.1f, Haze = .7f, FogDensity = .028f, SunIntensity = .75f,
        AmbientSky = new Color(.42f, .32f, .28f), AmbientEquator = new Color(.38f, .27f, .22f), AmbientGround = new Color(.18f, .13f, .11f),
        Exposure = 0, Contrast = 14, Saturation = -8, Bloom = .5f, Vignette = .32f, Filter = new Color(1f, .9f, .82f),
    };

    // Clear, cold air of the páramo around the lagoon of Iguaque.
    public static readonly Look Paramo = new Look
    {
        Zenith = new Color(.20f, .44f, .76f), Sky = new Color(.46f, .66f, .88f), Horizon = new Color(.84f, .89f, .92f),
        Fog = new Color(.70f, .78f, .84f), Sun = new Color(1f, .95f, .86f), Cloud = new Color(1f, 1f, 1f),
        Far = new Color(.40f, .48f, .62f), Mid = new Color(.36f, .44f, .36f), Near = new Color(.32f, .40f, .28f), Rock = new Color(.48f, .44f, .40f),
        CloudCover = .5f, MountainScale = 1.5f, Haze = .28f, FogDensity = .0042f, SunIntensity = 1.3f,
        AmbientSky = new Color(.56f, .66f, .82f), AmbientEquator = new Color(.56f, .58f, .54f), AmbientGround = new Color(.28f, .27f, .22f),
        Exposure = .05f, Contrast = 12, Saturation = 8, Bloom = .35f, Vignette = .22f, Filter = new Color(.98f, 1f, 1.02f),
    };

    private static Material _sky;
    private static Volume _volume;

    public static void Apply(Look look, Light sun, Camera camera)
    {
        var shader = Shader.Find("Nemequene/Andean Sky");
        if (shader != null)
        {
            if (_sky == null) _sky = new Material(shader) { name = "Cielo andino" };
            _sky.SetColor("_ZenithColor", look.Zenith); _sky.SetColor("_SkyColor", look.Sky); _sky.SetColor("_HorizonColor", look.Horizon);
            _sky.SetColor("_FogColor", look.Fog); _sky.SetColor("_SunColor", look.Sun); _sky.SetColor("_CloudColor", look.Cloud);
            _sky.SetColor("_FarMountain", look.Far); _sky.SetColor("_MidMountain", look.Mid); _sky.SetColor("_NearMountain", look.Near);
            _sky.SetColor("_Rock", look.Rock); _sky.SetColor("_Snow", new Color(.96f, .97f, 1f));
            _sky.SetFloat("_CloudCover", look.CloudCover); _sky.SetFloat("_MountainScale", look.MountainScale);
            _sky.SetFloat("_Haze", look.Haze); _sky.SetFloat("_SunSize", .9992f);
            if (sun != null) _sky.SetVector("_SunDirection", (-sun.transform.forward).normalized);
            RenderSettings.skybox = _sky;
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = look.Fog;
        RenderSettings.fogDensity = look.FogDensity;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = look.AmbientSky;
        RenderSettings.ambientEquatorColor = look.AmbientEquator;
        RenderSettings.ambientGroundColor = look.AmbientGround;
        if (sun != null)
        {
            sun.color = look.Sun; sun.intensity = look.SunIntensity;
            sun.shadows = LightShadows.Soft; sun.shadowStrength = .8f;
        }
        if (camera != null)
        {
            camera.clearFlags = shader != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            camera.backgroundColor = look.Fog;
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 600f);
            var data = camera.GetUniversalAdditionalCameraData();
            if (data != null) data.renderPostProcessing = true;
        }
        Grade(look);
    }

    private static void Grade(Look look)
    {
        if (_volume == null)
        {
            var go = new GameObject("Grado de color");
            _volume = go.AddComponent<Volume>();
            _volume.isGlobal = true; _volume.priority = 5;
            _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }
        var profile = _volume.profile;
        var tone = Get<Tonemapping>(profile); tone.mode.Override(TonemappingMode.Neutral);
        var color = Get<ColorAdjustments>(profile);
        color.postExposure.Override(look.Exposure); color.contrast.Override(look.Contrast);
        color.saturation.Override(look.Saturation); color.colorFilter.Override(look.Filter);
        var bloom = Get<Bloom>(profile);
        bloom.threshold.Override(1.05f); bloom.intensity.Override(look.Bloom); bloom.scatter.Override(.65f);
        var vignette = Get<Vignette>(profile);
        vignette.intensity.Override(look.Vignette); vignette.smoothness.Override(.45f);
    }

    private static T Get<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (!profile.TryGet<T>(out var c)) c = profile.Add<T>(true);
        c.active = true;
        return c;
    }
}

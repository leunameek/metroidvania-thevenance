namespace Nemequene.UI
{
    // The four quality presets and the three priorities of the Gráficos page. Neither is stored:
    // the page shows the one the current values match, or «Personalizado».
    public static class GraphicsPresets
    {
        public const int Low = 0, Medium = 1, High = 2, Ultra = 3, Custom = 4;
        private static readonly int[] Shadows = { 1, 2, 3, 4 }, Reflections = { 0, 1, 1, 2 }, Textures = { 0, 1, 2, 2 };
        private static readonly int[] Aniso = { 0, 1, 3, 4 }, Distance = { 0, 1, 2, 3 }, Antialiasing = { 1, 1, 2, 5 };

        public static void ApplyPreset(UISettings v, int preset)
        {
            v.shadowQuality = Shadows[preset]; v.ambientOcclusion = preset >= High; v.reflections = Reflections[preset];
            v.textureQuality = Textures[preset]; v.anisotropic = Aniso[preset]; v.viewDistance = Distance[preset];
            v.antialiasing = Antialiasing[preset]; v.bloom = preset >= Medium;
        }

        public static int DetectPreset(UISettings v)
        {
            for (int p = Low; p <= Ultra; p++)
                if (v.shadowQuality == Shadows[p] && v.ambientOcclusion == p >= High && v.reflections == Reflections[p]
                    && v.textureQuality == Textures[p] && v.anisotropic == Aniso[p] && v.viewDistance == Distance[p]
                    && v.antialiasing == Antialiasing[p] && v.bloom == p >= Medium) return p;
            return Custom;
        }

        // Performance favours frames (Medio, FSR at 59 %, no V-Sync, short CPU queue); quality
        // favours the image (Ultra at native size, V-Sync); balanced sits between them.
        private static readonly int[] PriorityPreset = { Medium, High, Ultra }, PriorityUpscaler = { 1, 1, 0 };
        private static readonly float[] PriorityScale = { .59f, .77f, 1f };
        private static readonly bool[] PriorityVSync = { false, true, true }, PriorityLatency = { true, false, false };

        public static void ApplyPriority(UISettings v, int priority)
        {
            ApplyPreset(v, PriorityPreset[priority]);
            v.upscaler = PriorityUpscaler[priority]; v.renderScale = PriorityScale[priority];
            v.vSync = PriorityVSync[priority]; v.lowLatency = PriorityLatency[priority]; v.frameLimit = -1;
        }

        public static int DetectPriority(UISettings v)
        {
            for (int p = 0; p < 3; p++)
                if (DetectPreset(v) == PriorityPreset[p] && v.upscaler == PriorityUpscaler[p] && System.Math.Abs(v.renderScale - PriorityScale[p]) < .006f
                    && v.vSync == PriorityVSync[p] && v.lowLatency == PriorityLatency[p] && v.frameLimit == -1) return p;
            return 3;
        }

        // «Restablecer gráficos»: the Alto preset at native scale; the screen mode is kept.
        public static void Reset(UISettings v)
        {
            var d = new UISettings();
            CopyGraphics(d, v);
        }

        // Every field of the page except the screen mode, size and refresh rate (those revert on their own).
        public static void CopyGraphics(UISettings from, UISettings to)
        {
            to.renderScale = from.renderScale; to.upscaler = from.upscaler; to.vSync = from.vSync; to.frameLimit = from.frameLimit;
            to.shadowQuality = from.shadowQuality; to.ambientOcclusion = from.ambientOcclusion; to.reflections = from.reflections;
            to.textureQuality = from.textureQuality; to.anisotropic = from.anisotropic; to.viewDistance = from.viewDistance;
            to.antialiasing = from.antialiasing; to.bloom = from.bloom; to.motionBlur = from.motionBlur; to.depthOfField = from.depthOfField;
            to.filmGrain = from.filmGrain; to.lowLatency = from.lowLatency; to.hdr = from.hdr; to.paperWhite = from.paperWhite;
            to.brightness = from.brightness; to.gamma = from.gamma;
        }
    }
}

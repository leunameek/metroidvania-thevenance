using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nemequene.UI
{
    // «Gráficos»: screen and resolution, presets and priority, lighting and effects, upscaling and
    // latency, brightness and gamma. Every row changes the running game through GraphicsRuntime;
    // the screen mode, size and refresh rate wait for «Aplicar» and revert by themselves after 15 s
    // unless the player keeps them.
    public sealed class GraphicsPage : SettingsRows
    {
        public static readonly int[] FrameCaps = { 30, 60, 120, 144, 165, 240, -1 };
        public static readonly float[] UpscalerScales = { .77f, .67f, .59f, .5f };
        private const float TestSeconds = 15;

        private RectTransform _group, _descriptionPanel, _scrollRect;
        private TMP_Text _description, _countdown, _displayState;
        private GameObject _confirmRow;
        private Button _keep, _apply;
        private readonly Graphic[] _patches = new Graphic[3];
        private List<Vector2Int> _sizes = new List<Vector2Int>();
        private List<RefreshRate> _rates = new List<RefreshRate>();
        private int _mode, _size, _rate;
        private bool _testing, _descriptionShown;
        private float _until;
        private int _oldWidth, _oldHeight, _oldMode, _oldNumerator, _oldDenominator;
        private string _snapshot;
        private float _scrollBottom;

        public GraphicsPage(UIFactory factory, SettingsManager settings) : base(factory, settings) { Describe = ShowDescription; }

        public void Build(RectTransform group)
        {
            _group = group;
            var ticker = group.gameObject.AddComponent<UIPageTicker>();
            ticker.shown = Shown; ticker.hidden = Hidden; ticker.tick = Tick;
            ReadDisplay();

            Section(group, "gfx.section.display");
            Choice(group, "gfx.mode", "gfx.mode.info",
                new[] { UIStrings.Get("gfx.mode.exclusive"), UIStrings.Get("gfx.mode.borderless"), UIStrings.Get("gfx.mode.window") },
                () => _mode, v => { _mode = v; PendingChanged(); });
            Choice(group, "gfx.resolution", "gfx.resolution.info", SizeLabels, () => _size, v => { _size = v; _rates = Rates(_sizes[_size]); _rate = 0; PendingChanged(); });
            Choice(group, "gfx.refresh", "gfx.refresh.info", RateLabels, () => _rate, v => { _rate = v; PendingChanged(); });
            _apply = Act(group, "gfx.applyDisplay", StartTest, "gfx.applyDisplay.info", true);
            _displayState = Note(group, "");
            _confirmRow = BuildConfirmRow(group);
            Slider(group, "gfx.renderScale", "gfx.renderScale.info", .5f, 1, () => V.renderScale, v => V.renderScale = Mathf.Round(v * 100) / 100f);
            Toggle(group, "gfx.vsync", "gfx.vsync.info", () => V.vSync, v => V.vSync = v);
            Choice(group, "gfx.fps", "gfx.fps.info", FrameCapLabels(), () => Mathf.Max(0, Array.IndexOf(FrameCaps, V.frameLimit)), v => V.frameLimit = FrameCaps[v]);

            Section(group, "gfx.section.quality");
            Choice(group, "gfx.preset", "gfx.preset.info",
                new[] { UIStrings.Get("gfx.low"), UIStrings.Get("gfx.medium"), UIStrings.Get("gfx.high"), UIStrings.Get("gfx.ultra"), UIStrings.Get("gfx.custom") },
                () => GraphicsPresets.DetectPreset(V), v => GraphicsPresets.ApplyPreset(V, v >= 4 ? 0 : v));
            Choice(group, "gfx.priority", "gfx.priority.info",
                new[] { UIStrings.Get("gfx.priority.performance"), UIStrings.Get("gfx.priority.balanced"), UIStrings.Get("gfx.priority.quality"), UIStrings.Get("gfx.custom") },
                () => GraphicsPresets.DetectPriority(V), v => GraphicsPresets.ApplyPriority(V, v >= 3 ? 0 : v));

            Section(group, "gfx.section.lighting");
            Choice(group, "gfx.shadows", "gfx.shadows.info",
                new[] { UIStrings.Get("off"), UIStrings.Get("gfx.low"), UIStrings.Get("gfx.medium"), UIStrings.Get("gfx.high"), UIStrings.Get("gfx.ultra") },
                () => V.shadowQuality, v => V.shadowQuality = v);
            Toggle(group, "gfx.ao", "gfx.ao.info", () => V.ambientOcclusion, v => V.ambientOcclusion = v);
            Choice(group, "gfx.reflections", "gfx.reflections.info",
                new[] { UIStrings.Get("off"), UIStrings.Get("gfx.reflections.standard"), UIStrings.Get("gfx.reflections.realtime") },
                () => V.reflections, v => V.reflections = v);
            Choice(group, "gfx.textures", "gfx.textures.info",
                new[] { UIStrings.Get("gfx.low"), UIStrings.Get("gfx.medium"), UIStrings.Get("gfx.high") },
                () => V.textureQuality, v => V.textureQuality = v);
            Choice(group, "gfx.aniso", "gfx.aniso.info", new[] { UIStrings.Get("off"), "2×", "4×", "8×", "16×" }, () => V.anisotropic, v => V.anisotropic = v);
            Choice(group, "gfx.distance", "gfx.distance.info",
                new[] { UIStrings.Get("gfx.distance.near"), UIStrings.Get("gfx.medium"), UIStrings.Get("gfx.distance.far"), UIStrings.Get("gfx.distance.max") },
                () => V.viewDistance, v => V.viewDistance = v);
            Choice(group, "gfx.aa", "gfx.aa.info", new[] { UIStrings.Get("off"), "FXAA", "SMAA", "TAA", "MSAA 2×", "MSAA 4×", "MSAA 8×" },
                () => V.antialiasing, v => V.antialiasing = v);
            Toggle(group, "gfx.bloom", "gfx.bloom.info", () => V.bloom, v => V.bloom = v);
            Toggle(group, "gfx.motionBlur", "gfx.motionBlur.info", () => V.motionBlur, v => V.motionBlur = v);
            Toggle(group, "gfx.dof", "gfx.dof.info", () => V.depthOfField, v => V.depthOfField = v);
            Toggle(group, "gfx.grain", "gfx.grain.info", () => V.filmGrain, v => V.filmGrain = v);

            Section(group, "gfx.section.scaling");
            var upscalers = GraphicsRuntime.StpSupported
                ? new[] { UIStrings.Get("gfx.upscaler.none"), "AMD FSR 1.0", "Unity STP" }
                : new[] { UIStrings.Get("gfx.upscaler.none"), "AMD FSR 1.0" };
            Choice(group, "gfx.upscaler", "gfx.upscaler.info", upscalers, () => Mathf.Min(V.upscaler, upscalers.Length - 1), v => V.upscaler = v);
            Choice(group, "gfx.upscalerMode", "gfx.upscalerMode.info",
                new[] { UIStrings.Get("gfx.upscalerMode.ultra"), UIStrings.Get("gfx.upscalerMode.quality"), UIStrings.Get("gfx.upscalerMode.balanced"),
                    UIStrings.Get("gfx.upscalerMode.performance"), UIStrings.Get("gfx.custom") },
                DetectUpscalerMode, v => V.renderScale = UpscalerScales[v >= 4 ? 0 : v]);
            Unavailable(group, "gfx.dlss", "gfx.dlss.info", UIStrings.Get("gfx.unavailable"));
            Unavailable(group, "gfx.frameGen", "gfx.frameGen.info", UIStrings.Get("gfx.unavailable"));
            Toggle(group, "gfx.latency", "gfx.latency.info", () => V.lowLatency, v => V.lowLatency = v);
            if (GraphicsRuntime.HdrAvailable)
            {
                Toggle(group, "gfx.hdr", "gfx.hdr.info", () => V.hdr, v => V.hdr = v);
                Slider(group, "gfx.paperWhite", "gfx.paperWhite.info", 80, 400, () => V.paperWhite, v => V.paperWhite = Mathf.Round(v / 10) * 10, v => Mathf.RoundToInt(v) + " nits");
            }
            else Unavailable(group, "gfx.hdr", "gfx.hdr.info", UIStrings.Get("gfx.hdr.missing"));

            Section(group, "gfx.section.calibration");
            BuildCalibration(group);
            Slider(group, "gfx.brightness", "gfx.brightness.info", -.5f, .5f, () => V.brightness, v => { V.brightness = Mathf.Round(v * 100) / 100f; PaintPatches(); }, Signed);
            Slider(group, "gfx.gamma", "gfx.gamma.info", -.5f, .5f, () => V.gamma, v => { V.gamma = Mathf.Round(v * 100) / 100f; PaintPatches(); }, Signed);

            Section(group, "gfx.section.changes");
            Act(group, "gfx.revert", Revert, "gfx.revert.info");
            Act(group, "gfx.reset", () => { GraphicsPresets.Reset(V); Commit(); PaintPatches(); }, "gfx.reset.info");
            PendingChanged();
        }

        private static string Signed(float v) { int p = Mathf.RoundToInt(v * 100); return (p > 0 ? "+" : "") + p + " %"; }

        private int DetectUpscalerMode()
        {
            for (int i = 0; i < UpscalerScales.Length; i++) if (Mathf.Abs(V.renderScale - UpscalerScales[i]) < .006f) return i;
            return 4;
        }

        private static string[] FrameCapLabels()
        {
            var labels = new string[FrameCaps.Length];
            for (int i = 0; i < FrameCaps.Length; i++) labels[i] = FrameCaps[i] < 0 ? UIStrings.Get("unlimited") : FrameCaps[i] + " FPS";
            return labels;
        }

        // ---- Screen, size and refresh rate ------------------------------------------------------

        private static Vector2Int Native()
        {
            int w = Display.main.systemWidth, h = Display.main.systemHeight;
            if (w < 800 || h < 600) { w = Screen.currentResolution.width; h = Screen.currentResolution.height; }
            if (w < 800 || h < 600) { w = 1920; h = 1080; }
            return new Vector2Int(w, h);
        }

        // The monitor's native size and every common size below it (at least eight), plus the
        // sizes the driver reports.
        public static List<Vector2Int> Sizes()
        {
            var native = Native();
            int[,] common = { {1024,768}, {1280,720}, {1280,800}, {1280,1024}, {1366,768}, {1440,900}, {1600,900}, {1680,1050},
                {1920,1080}, {1920,1200}, {2560,1080}, {2560,1440}, {3440,1440}, {3840,2160} };
            var sizes = new List<Vector2Int>();
            void Add(int w, int h) { var s = new Vector2Int(w, h); if (!sizes.Contains(s)) sizes.Add(s); }
            for (int i = 0; i < common.GetLength(0); i++) if (common[i, 0] <= native.x && common[i, 1] <= native.y) Add(common[i, 0], common[i, 1]);
            foreach (var r in Screen.resolutions) if (r.width >= 800 && r.height >= 600 && r.width <= native.x && r.height <= native.y) Add(r.width, r.height);
            Add(native.x, native.y);
            if (sizes.Count < 8) for (int i = 0; i < common.GetLength(0); i++) Add(common[i, 0], common[i, 1]);
            sizes.Sort((a, b) => a.x * a.y != b.x * b.y ? (a.x * a.y).CompareTo(b.x * b.y) : a.x.CompareTo(b.x));
            return sizes;
        }

        private string[] SizeLabels()
        {
            var native = Native(); var labels = new string[_sizes.Count];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = _sizes[i].x + " × " + _sizes[i].y + (_sizes[i] == native ? " " + UIStrings.Get("gfx.native") : "");
            return labels;
        }

        // «Monitor» first (the rate the system already uses), then each rate the driver lists for that size.
        private static List<RefreshRate> Rates(Vector2Int size)
        {
            var rates = new List<RefreshRate> { new RefreshRate { numerator = 0, denominator = 1 } };
            void Add(RefreshRate rate)
            {
                if (rate.numerator == 0) return;
                foreach (var r in rates) if (r.numerator != 0 && Mathf.Abs((float)(r.value - rate.value)) < .05f) return;
                rates.Add(rate);
            }
            foreach (var r in Screen.resolutions) if (r.width == size.x && r.height == size.y) Add(r.refreshRateRatio);
            if (rates.Count == 1) { foreach (var r in Screen.resolutions) Add(r.refreshRateRatio); Add(Screen.currentResolution.refreshRateRatio); }
            rates.Sort((a, b) => a.numerator == 0 && b.numerator == 0 ? 0 : a.numerator == 0 ? -1 : b.numerator == 0 ? 1 : a.value.CompareTo(b.value));
            return rates;
        }

        private string[] RateLabels()
        {
            var labels = new string[_rates.Count];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = _rates[i].numerator == 0 ? UIStrings.Get("gfx.refresh.monitor") : Math.Round(_rates[i].value, 2).ToString("0.##") + " Hz";
            return labels;
        }

        private void ReadDisplay()
        {
            _sizes = Sizes(); _mode = V.displayMode;
            var wanted = V.screenWidth >= 640 ? new Vector2Int(V.screenWidth, V.screenHeight) : new Vector2Int(Screen.width, Screen.height);
            _size = _sizes.IndexOf(wanted);
            if (_size < 0) { _sizes.Add(wanted); _size = _sizes.Count - 1; }
            _rates = Rates(_sizes[_size]); _rate = 0;
            for (int i = 1; i < _rates.Count; i++)
                if (_rates[i].numerator == V.refreshNumerator && _rates[i].denominator == V.refreshDenominator) _rate = i;
        }

        private bool PendingDiffers()
        {
            var size = _sizes[_size]; var rate = _rates[Mathf.Clamp(_rate, 0, _rates.Count - 1)];
            int width = V.screenWidth >= 640 ? V.screenWidth : Screen.width, height = V.screenHeight >= 480 ? V.screenHeight : Screen.height;
            return _mode != V.displayMode || size.x != width || size.y != height || (int)rate.numerator != V.refreshNumerator
                || (rate.numerator != 0 && (int)rate.denominator != V.refreshDenominator);
        }

        private void PendingChanged()
        {
            if (_displayState == null) return;
            bool differs = PendingDiffers();
            string current = Screen.width + " × " + Screen.height + " · " + UIStrings.Get(Screen.fullScreenMode == FullScreenMode.Windowed ? "gfx.mode.window"
                : Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? "gfx.mode.exclusive" : "gfx.mode.borderless");
            _displayState.text = UIStrings.Get(differs ? "gfx.display.pending" : "gfx.display.current", current)
                + (_mode == 2 && _rate != 0 ? "\n" + UIStrings.Get("gfx.refresh.windowed") : "");
            SetEnabled(_apply, differs && !_testing);
        }

        private GameObject BuildConfirmRow(Transform parent)
        {
            var column = F.Column(parent, "ConfirmarPantalla", 8);
            _countdown = UIFactory.Tone(F.Text(column, "", 24), UITone.GoldLight);
            _countdown.alignment = TextAlignmentOptions.Center;
            var row = F.Row(column, "Acciones", 18); row.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            _keep = Act(row, "gfx.keep", KeepDisplay, null, true);
            Act(row, "gfx.revertDisplay", () => RevertDisplay(false));
            UIFactory.CenterAll(row);
            column.gameObject.SetActive(false);
            return column.gameObject;
        }

        private void StartTest()
        {
            if (_testing || !PendingDiffers()) return;
            _oldWidth = V.screenWidth >= 640 ? V.screenWidth : Screen.width; _oldHeight = V.screenHeight >= 480 ? V.screenHeight : Screen.height;
            _oldMode = V.displayMode; _oldNumerator = V.refreshNumerator; _oldDenominator = V.refreshDenominator;
            var size = _sizes[_size]; var rate = _rates[_rate];
            GraphicsRuntime.ApplyDisplay(size.x, size.y, _mode, (int)rate.numerator, (int)rate.denominator);
            _testing = true; _until = Time.unscaledTime + TestSeconds;
            _confirmRow.SetActive(true); PendingChanged();
            EventSystem.current?.SetSelectedGameObject(_keep.gameObject);
        }

        private void KeepDisplay()
        {
            if (!_testing) return;
            var size = _sizes[_size]; var rate = _rates[_rate];
            V.screenWidth = size.x; V.screenHeight = size.y; V.displayMode = _mode; V.fullscreen = _mode != 2;
            V.refreshNumerator = (int)rate.numerator; V.refreshDenominator = (int)Mathf.Max(1, rate.denominator);
            _testing = false; _confirmRow.SetActive(false);
            Commit(); Settings.Flush(); PendingChanged();
            EventSystem.current?.SetSelectedGameObject(_apply.gameObject);
        }

        private void RevertDisplay(bool leaving)
        {
            if (!_testing) return;
            _testing = false;
            GraphicsRuntime.ApplyDisplay(_oldWidth, _oldHeight, _oldMode, _oldNumerator, _oldDenominator);
            if (leaving) return;
            _confirmRow.SetActive(false); ReadDisplay(); Commit(); PendingChanged();
            EventSystem.current?.SetSelectedGameObject(_apply.gameObject);
        }

        // ---- Brightness and gamma ---------------------------------------------------------------

        // Three emblems on black at 2, 5 and 10 % grey, painted as the current brightness and gamma
        // would show them: the first should be barely visible, the third clearly.
        private void BuildCalibration(Transform parent)
        {
            var box = F.Rect("Calibracion", parent, Vector2.zero, Vector2.one);
            box.gameObject.AddComponent<LayoutElement>().minHeight = 190;
            var black = F.Rect("Negro", box, new Vector2(0, .22f), Vector2.one);
            black.offsetMin = new Vector2(76, 0); black.offsetMax = new Vector2(-40, 0);
            var image = black.gameObject.AddComponent<Image>(); image.color = Color.black; image.raycastTarget = false;
            for (int i = 0; i < 3; i++)
            {
                float x = .2f + i * .3f;
                var icon = F.Icon(black, UIIcon.Objective, new Vector2(x - .07f, .12f), new Vector2(x + .07f, .88f), Color.black);
                icon.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                _patches[i] = icon;
                icon.name = "Emblema_" + (i == 0 ? 2 : i == 1 ? 5 : 10);
            }
            var hint = UIFactory.Tone(F.Label(box, UIStrings.Get("gfx.calibration.hint"), Vector2.zero, new Vector2(1, .2f), 20), UITone.Muted);
            hint.alignment = TextAlignmentOptions.Center;
            PaintPatches();
        }

        private void PaintPatches()
        {
            float[] levels = { .02f, .05f, .10f };
            for (int i = 0; i < _patches.Length; i++)
            {
                if (_patches[i] == null) continue;
                float shown = Mathf.Clamp01(Mathf.Pow(levels[i], 1f / (1f + V.gamma)) * (1f + V.brightness));
                _patches[i].color = new Color(shown, shown, shown, 1);
            }
        }

        // ---- Description panel, snapshot, countdown ---------------------------------------------

        private void Shown()
        {
            _snapshot = JsonUtility.ToJson(V);
            ReadDisplay(); PendingChanged(); PaintPatches();
            var scroll = _group.GetComponentInParent<ScrollRect>(true);
            if (scroll == null || _descriptionShown) return;
            _scrollRect = (RectTransform)scroll.transform;
            if (_descriptionPanel == null)
            {
                _descriptionPanel = F.HudPanel("Descripcion", _scrollRect.parent, Vector2.zero, new Vector2(1, 0));
                _description = F.Text(_descriptionPanel, "", 21);
                _description.rectTransform.Inset(28, 12, 28, 12);
                _description.alignment = TextAlignmentOptions.MidlineLeft;
                _description.enableAutoSizing = true; _description.fontSizeMax = 21; _description.fontSizeMin = 14;
            }
            _scrollBottom = _scrollRect.offsetMin.y;
            _descriptionPanel.offsetMin = new Vector2(_scrollRect.offsetMin.x, _scrollBottom);
            _descriptionPanel.offsetMax = new Vector2(_scrollRect.offsetMax.x, _scrollBottom + 118);
            _scrollRect.offsetMin = new Vector2(_scrollRect.offsetMin.x, _scrollBottom + 132);
            _descriptionPanel.gameObject.SetActive(true); _descriptionShown = true;
            ShowDescription(UIStrings.Get("gfx.description.idle"));
        }

        private void Hidden()
        {
            // Leaving the page while a new screen mode is on trial goes back to the one that worked.
            RevertDisplay(true);
            if (_confirmRow != null) _confirmRow.SetActive(false);
            if (!_descriptionShown) return;
            _descriptionShown = false;
            if (_scrollRect != null) _scrollRect.offsetMin = new Vector2(_scrollRect.offsetMin.x, _scrollBottom);
            if (_descriptionPanel != null) _descriptionPanel.gameObject.SetActive(false);
        }

        private void ShowDescription(string text) { if (_description != null) _description.text = text; }

        private void Revert()
        {
            if (string.IsNullOrEmpty(_snapshot)) return;
            // Only the graphics fields go back; audio and accessibility keep what they have now.
            GraphicsPresets.CopyGraphics(JsonUtility.FromJson<UISettings>(_snapshot), V);
            Commit(); PaintPatches();
        }

        private void Tick()
        {
            if (!_testing) return;
            float left = _until - Time.unscaledTime;
            _countdown.text = UIStrings.Get("gfx.keepQuestion", Mathf.CeilToInt(Mathf.Max(0, left)));
            if (left <= 0) RevertDisplay(false);
        }
    }
}

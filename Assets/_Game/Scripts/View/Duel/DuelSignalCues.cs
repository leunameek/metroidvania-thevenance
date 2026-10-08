using UnityEngine;

// The signal language in the world (DuelSignal), the same in EVERY fight — turn duels, the plaza
// training and the real-time creatures — and on top of each stage's own effects. Body, ground,
// light and sound say what is coming; the HUD does not (2026-10-07 playtest: the first version
// was too subtle, so every channel is now loud and lasts the whole warning):
//  - Front: the enemy leans back; a lane of orange chevrons runs on the ground from it to
//    Nemequene, dust streams along it, a warm light grows in front of the enemy; low drum.
//  - Sweep: the enemy winds to one side; a band of pale chevrons crosses Nemequene from that side,
//    air streaks flow across him, a cold light on that side; rising whistle.
//  - Above: the enemy bows; a dark shadow with a hard rim grows under Nemequene, debris pours from
//    above, a violet light overhead; creak.
//  - Glint: when the answer opens, a golden flare (light flash and sparks) on the enemy and a
//    chime: the blow can also be parried.
// Every sound also raises its caption, so the signal reaches players who play without sound.
public sealed class DuelSignalCues
{
    private readonly Transform _player, _enemy;
    private CombatSignalFx _fx;
    private bool _sweepRight;

    // Side the last sweep comes from (+1 right of Nemequene, -1 left), for bodies that wind to it.
    public float SweepSide => _sweepRight ? 1f : -1f;

    public DuelSignalCues(Transform player, Transform enemy)
    {
        _player = player; _enemy = enemy;
    }

    public void Warn(DuelMove move)
    {
        if (move == null) { Clear(); return; }
        Warn(move.Signal);
    }

    // The warning starts: everything builds from here until the blow (Progress 1).
    public void Warn(DuelSignal signal)
    {
        Clear();
        if (_player == null) return;
        if (signal == DuelSignal.Sweep) _sweepRight = !_sweepRight;
        if (_fx == null) _fx = CombatSignalFx.Create();
        _fx.Begin(signal, _player, _enemy, SweepSide);
        switch (signal)
        {
            case DuelSignal.Front: Sound("Combate/senal_frente", _fx.EnemyFloor + Vector3.up, "Tambor grave"); break;
            case DuelSignal.Sweep: Sound("Combate/senal_barrido", _fx.SideOrigin + Vector3.up, "Silbido de lado"); break;
            default: Sound("Combate/senal_arriba", _fx.PlayerFloor + Vector3.up * 4f, "Crujido arriba"); break;
        }
    }

    // 0 at the warning, 1 when the blow lands.
    public void Progress(float k) { if (_fx != null) _fx.Progress = Mathf.Clamp01(k); }

    // The answer opens: the signal sounds again, higher, and a parriable blow flares gold.
    public void Open(DuelMove move) => Open(move != null && move.Glint, move != null);
    public void Open(bool glint, bool repeat = true)
    {
        if (_fx == null || !_fx.Active) return;
        if (repeat)
        {
            string id = _fx.Signal == DuelSignal.Front ? "Combate/senal_frente" : _fx.Signal == DuelSignal.Sweep ? "Combate/senal_barrido" : "Combate/senal_arriba";
            GameAudio.PlayAt(id, _fx.PlayerFloor + Vector3.up, .8f, AudioChannel.Effects, 30f, 1.15f);
        }
        if (!glint) return;
        _fx.Flare();
        GameAudio.PlayAt("Combate/senal_destello", _fx.EnemyChest, 1f);
        GameAudio.Caption("Destello dorado");
    }

    public void Clear() { if (_fx != null) _fx.End(); }

    public void Dispose()
    {
        if (_fx != null) Object.Destroy(_fx.gameObject);
        _fx = null;
    }

    private static void Sound(string id, Vector3 at, string caption)
    {
        GameAudio.PlayAt(id, at, 1f, AudioChannel.Effects, 40f);
        GameAudio.Caption(caption);
    }
}

// The visible part of a signal: ground shape, streaming particles, light and the enemy's lean.
// One per fight, reused for every warning.
public sealed class CombatSignalFx : MonoBehaviour
{
    private static readonly Color FrontColor = new Color(1f, .45f, .12f);
    private static readonly Color SweepColor = new Color(.62f, .9f, 1f);
    private static readonly Color AboveColor = new Color(.62f, .38f, 1f);
    private static readonly Color Dust = new Color(.85f, .68f, .45f, .9f);
    private static readonly Color Air = new Color(.9f, .97f, 1f, .75f);
    private static readonly Color Debris = new Color(.55f, .5f, .45f, 1f);
    private static readonly Color Gold = new Color(1f, .8f, .3f, 1f);

    private static Material _laneMaterial;
    private static Texture2D _chevrons;

    private Transform _player, _enemy;
    private Renderer _lane;
    private Transform _laneRoot;
    private TelegraphMark _shadow, _rim;
    private ParticleSystem _stream;
    private Light _light, _flare;
    private SignalPose _pose;
    private float _side, _scroll, _flareTime;

    public DuelSignal Signal { get; private set; }
    public bool Active { get; private set; }
    public float Progress { get; set; }
    public Vector3 PlayerFloor { get; private set; }
    public Vector3 EnemyFloor { get; private set; }
    public Vector3 SideOrigin { get; private set; }
    public Vector3 EnemyChest => _enemy != null ? _enemy.position + Vector3.up * 1.4f : PlayerFloor + Forward * 3f + Vector3.up * 1.4f;
    private Vector3 Forward
    {
        get
        {
            Vector3 f = _enemy != null ? _enemy.position - PlayerFloor : (_player != null ? _player.forward : Vector3.forward);
            f.y = 0;
            return f.sqrMagnitude > .01f ? f.normalized : Vector3.forward;
        }
    }

    public static CombatSignalFx Create()
    {
        var fx = new GameObject("Senal_Combate").AddComponent<CombatSignalFx>();
        fx._laneRoot = new GameObject("Carril").transform;
        fx._laneRoot.SetParent(fx.transform, false);
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());
        quad.name = "Chevrones";
        quad.transform.SetParent(fx._laneRoot, false);
        quad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        fx._lane = quad.GetComponent<Renderer>();
        fx._lane.material = new Material(LaneMaterial);
        fx._lane.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fx._lane.receiveShadows = false;
        fx._shadow = TelegraphMark.Create(fx.transform, "Sombra");
        fx._rim = TelegraphMark.Create(fx.transform, "Borde");
        fx._light = new GameObject("Luz_Senal").AddComponent<Light>();
        fx._light.transform.SetParent(fx.transform, false);
        fx._light.type = LightType.Point; fx._light.shadows = LightShadows.None; fx._light.enabled = false;
        fx._flare = new GameObject("Destello").AddComponent<Light>();
        fx._flare.transform.SetParent(fx.transform, false);
        fx._flare.type = LightType.Point; fx._flare.color = Gold; fx._flare.range = 9f; fx._flare.shadows = LightShadows.None; fx._flare.enabled = false;
        fx.End();
        return fx;
    }

    public void Begin(DuelSignal signal, Transform player, Transform enemy, float side)
    {
        _player = player; _enemy = enemy; _side = side; Signal = signal;
        Active = true; Progress = 0; _scroll = 0;
        PlayerFloor = TelegraphMark.Floor(player.position, player.position.y);
        EnemyFloor = enemy != null ? TelegraphMark.Floor(enemy.position, PlayerFloor.y) : PlayerFloor + Forward * 3f;
        Vector3 right = Vector3.Cross(Vector3.up, Forward);
        SideOrigin = PlayerFloor + right * side * 3.2f;
        Color color = signal == DuelSignal.Front ? FrontColor : signal == DuelSignal.Sweep ? SweepColor : AboveColor;

        // Ground: a lane toward Nemequene, a band across him, or a shadow on him.
        if (signal == DuelSignal.Above)
        {
            _lane.gameObject.SetActive(false);
            _shadow.Show(PlayerFloor, 2.3f, new Color(.03f, .02f, .05f, .9f));
            _rim.Show(PlayerFloor, 2.3f, color);
        }
        else
        {
            Vector3 from, to; float width;
            if (signal == DuelSignal.Front) { from = EnemyFloor; to = PlayerFloor; width = 1.9f; }
            else { from = SideOrigin; to = PlayerFloor - right * side * 3.2f; width = 2.6f; }
            Vector3 dir = to - from; dir.y = 0;
            float length = Mathf.Max(1.5f, dir.magnitude);
            _laneRoot.position = (from + to) * .5f + Vector3.up * .04f;
            _laneRoot.rotation = Quaternion.LookRotation(dir.sqrMagnitude > .01f ? dir.normalized : Vector3.forward);
            _lane.transform.localScale = new Vector3(width, length, 1);
            _lane.material.color = color;
            _lane.material.SetTextureScale("_BaseMap", new Vector2(1, length / 1.4f));
            _lane.material.mainTextureScale = new Vector2(1, length / 1.4f);
            _lane.gameObject.SetActive(true);
        }

        // Particles that keep coming for the whole warning.
        if (_stream != null) Destroy(_stream.gameObject);
        _stream = Stream(signal, right);

        // Light: in front of the enemy, on the side of the sweep, or above Nemequene.
        _light.color = color; _light.range = signal == DuelSignal.Above ? 7f : 6f; _light.intensity = 0; _light.enabled = true;
        _light.transform.position = signal == DuelSignal.Front ? EnemyFloor + Forward * -1.2f + Vector3.up * 1.2f
            : signal == DuelSignal.Sweep ? SideOrigin + Vector3.up * 1.3f : PlayerFloor + Vector3.up * 3.5f;

        // The body: leans back for a blow from the front, winds to the side of a sweep, bows when
        // something will fall.
        if (_enemy != null)
        {
            _pose = _enemy.GetComponent<SignalPose>() ?? _enemy.gameObject.AddComponent<SignalPose>();
            _pose.Euler = signal == DuelSignal.Front ? new Vector3(-16, 0, 0)
                : signal == DuelSignal.Sweep ? new Vector3(0, -38 * side, 8 * side) : new Vector3(14, 0, 0);
            _pose.Weight = 0;
        }
    }

    private ParticleSystem Stream(DuelSignal signal, Vector3 right)
    {
        var go = new GameObject("Corriente");
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = MIParticles.Material;
        var main = ps.main; main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        var shape = ps.shape;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        Vector3 velocity;
        switch (signal)
        {
            case DuelSignal.Front:
                go.transform.position = EnemyFloor + Vector3.up * .25f;
                go.transform.rotation = Quaternion.LookRotation(Forward * -1f);
                shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(1.4f, .3f, .4f);
                velocity = -Forward * (Vector3.Distance(EnemyFloor, PlayerFloor) / .9f);
                main.startColor = Dust; main.startSize = new ParticleSystem.MinMaxCurve(.25f, .6f);
                main.startLifetime = .9f; emission.rateOverTime = 70; main.gravityModifier = -.05f;
                break;
            case DuelSignal.Sweep:
                go.transform.position = SideOrigin + Vector3.up * 1f;
                shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(.4f, 1.6f, 1.6f);
                velocity = -right * _side * 9f;
                main.startColor = Air; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .16f);
                main.startLifetime = .7f; emission.rateOverTime = 90;
                renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.velocityScale = .12f; renderer.lengthScale = 2f;
                break;
            default:
                go.transform.position = PlayerFloor + Vector3.up * 6f;
                shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 2f;
                go.transform.rotation = Quaternion.Euler(90, 0, 0);
                velocity = Vector3.down * 2f;
                main.startColor = Debris; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .22f);
                main.startLifetime = 1.6f; main.gravityModifier = 1.2f; emission.rateOverTime = 60;
                break;
        }
        main.startSpeed = 0;
        var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
        vel.x = velocity.x; vel.y = velocity.y; vel.z = velocity.z;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .15f), new GradientAlphaKey(1, .7f), new GradientAlphaKey(0, 1) });
        fade.color = g;
        ps.Play();
        return ps;
    }

    // A golden flare on the enemy: the blow can be parried.
    public void Flare()
    {
        _flare.transform.position = EnemyChest;
        _flare.enabled = true; _flareTime = .55f;
        MIParticles.Burst(EnemyChest, Gold, 90, 4f, .2f, -.1f);
        MIParticles.Burst(EnemyChest, new Color(1f, .97f, .8f, 1f), 30, 1.2f, .45f, 0f);
    }

    public void End()
    {
        Active = false;
        if (_lane != null) _lane.gameObject.SetActive(false);
        if (_shadow != null) _shadow.Hide();
        if (_rim != null) _rim.Hide();
        if (_stream != null) { _stream.Stop(true, ParticleSystemStopBehavior.StopEmitting); Destroy(_stream.gameObject, 2f); _stream = null; }
        if (_light != null) _light.enabled = false;
        if (_pose != null) _pose.Weight = 0;
    }

    private void Update()
    {
        if (_flareTime > 0)
        {
            _flareTime -= Time.deltaTime;
            _flare.intensity = Mathf.Max(0, _flareTime / .55f) * 14f;
            if (_flareTime <= 0) _flare.enabled = false;
        }
        if (!Active) return;
        float k = Progress;
        float pulse = .75f + .25f * Mathf.Sin(Time.time * (10f + 10f * k));
        // Chevrons flow toward Nemequene (front) or across him (sweep), faster as the blow nears.
        _scroll -= Time.deltaTime * Mathf.Lerp(1.2f, 3.5f, k);
        if (_lane.gameObject.activeSelf)
        {
            _lane.material.SetTextureOffset("_BaseMap", new Vector2(0, _scroll));
            _lane.material.mainTextureOffset = new Vector2(0, _scroll);
            var c = _lane.material.color; c.a = Mathf.Lerp(.55f, 1f, k) * pulse; _lane.material.color = c;
        }
        if (_shadow.Visible) { _shadow.SetProgress(k); _rim.SetProgress(Mathf.Max(.6f, k)); }
        _light.intensity = Mathf.Lerp(2f, 7f, k) * pulse;
        if (_pose != null) _pose.Weight = Mathf.SmoothStep(0, 1, Mathf.Min(1f, k * 2.2f)) + Mathf.Sin(Time.time * 18f) * .04f * k;
    }

    private void OnDestroy()
    {
        if (_pose != null) _pose.Weight = 0;
    }

    private static Material LaneMaterial
    {
        get
        {
            if (_laneMaterial != null) return _laneMaterial;
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            var m = new Material(shader) { name = "Senal_Chevrones" };
            m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.SetFloat("_Cull", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3011;
            m.mainTexture = Chevrons();
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", m.mainTexture);
            return _laneMaterial = m;
        }
    }

    // Chevrons pointing along +v (toward where the blow goes), soft at the lane's edges.
    private static Texture2D Chevrons()
    {
        if (_chevrons != null) return _chevrons;
        const int n = 64;
        _chevrons = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, name = "Senal_Chevrones" };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + .5f) / n, v = (y + .5f) / n;
                float f = Mathf.Repeat(v + Mathf.Abs(u - .5f) * .9f, 1f);
                float stripe = Mathf.Clamp01(1f - Mathf.Abs(f - .3f) / .16f);
                float edge = 1f - Mathf.Pow(Mathf.Abs(u - .5f) * 2f, 4f);
                float a = Mathf.Max(stripe, .18f) * edge;
                _chevrons.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(a)));
            }
        _chevrons.Apply();
        return _chevrons;
    }
}

// Adds a lean on top of whatever moves the body (animator, stage code): it detects when someone
// else wrote the rotation this frame and applies the offset to that, so it never accumulates.
[DefaultExecutionOrder(10000)]
public sealed class SignalPose : MonoBehaviour
{
    public Vector3 Euler;
    public float Weight;
    private Quaternion _base, _written;
    private bool _has;

    private void LateUpdate()
    {
        var current = transform.localRotation;
        if (!_has || Quaternion.Angle(current, _written) > .01f) _base = current;
        _has = true;
        _written = Weight > .001f ? _base * Quaternion.Euler(Euler * Weight) : _base;
        transform.localRotation = _written;
    }
}

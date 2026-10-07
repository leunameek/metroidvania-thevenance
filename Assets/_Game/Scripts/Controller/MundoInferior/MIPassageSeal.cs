using System.Collections;
using System.Collections.Generic;
using Nemequene.UI;
using UnityEngine;

// A veil of dark bonds across the way on (2026-10-06 playtest: every fight is required). It holds
// while any creature of its room still stands and dissolves when the last one yields; a creature
// beaten in the save leaves it open from the start.
public sealed class MIPassageSeal : MonoBehaviour
{
    private static readonly Color Bond = new Color(.55f, .35f, .85f);
    private readonly List<MIDashEnemy> _guards = new List<MIDashEnemy>();
    private BoxCollider _wall;
    private Renderer _veil;
    private ParticleSystem _motes;
    private float _width, _nextNotice;
    private bool _open;
    private string _who;

    // room: the room root; local: centre of the passage on the floor; width across it.
    public static MIPassageSeal Create(Transform room, Vector3 local, float width, float height, IEnumerable<MIDashEnemy> guards, string who)
    {
        var go = new GameObject("Lazo que cierra el paso");
        go.transform.SetParent(room, false);
        go.transform.localPosition = local; go.transform.localRotation = Quaternion.identity;
        var seal = go.AddComponent<MIPassageSeal>();
        seal._width = width; seal._who = who;
        seal._guards.AddRange(guards);
        seal._wall = go.AddComponent<BoxCollider>();
        seal._wall.center = new Vector3(0, height * .5f, 0); seal._wall.size = new Vector3(width, height, .5f);
        go.layer = 2; // Ignore Raycast: the camera and the floor probes see through it
        seal.BuildVeil(height);
        return seal;
    }

    private void Start()
    {
        MIDashEnemy.YieldedEvent += OnYielded;
        if (AllYielded()) Open(true);
    }

    private void OnDestroy() { MIDashEnemy.YieldedEvent -= OnYielded; }

    private bool AllYielded()
    {
        foreach (var g in _guards) if (g != null && !g.Yielded) return false;
        return true;
    }

    private void OnYielded(MIDashEnemy e) { if (!_open && _guards.Contains(e) && AllYielded()) Open(false); }

    private void Update()
    {
        if (_open || _veil == null) return;
        var m = _veil.material; var c = Bond; c.a = .38f + .12f * Mathf.Sin(Time.time * 2.3f); m.color = c;
        var mi = MundoInferiorBlockout.Instance; var player = mi != null ? FindPlayer() : null;
        if (player == null || Time.time < _nextNotice) return;
        Vector3 local = transform.InverseTransformPoint(player.position);
        if (Mathf.Abs(local.z) < 1.6f && Mathf.Abs(local.x) < _width * .5f + .5f)
        {
            _nextNotice = Time.time + 6f;
            mi.Hud?.Notify("El paso está cerrado", "Un lazo oscuro cierra el camino mientras " + _who + " siga en pie.", UIIcon.Guardian, UIPalette.Muted);
        }
    }

    private PlayerController _player;
    private Transform FindPlayer()
    {
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
        return _player != null ? _player.transform : null;
    }

    private void Open(bool instant)
    {
        _open = true;
        if (_wall != null) _wall.enabled = false;
        if (instant) { gameObject.SetActive(false); return; }
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        MIAudio.PlayAt("guardian_cae", transform.position + Vector3.up, .6f, 1.3f);
        MIParticles.Burst(transform.position + Vector3.up * 1.5f, new Color(Bond.r, Bond.g, Bond.b, .9f), 90, 3f, .12f, -.3f);
        if (_motes != null) _motes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        var m = _veil != null ? _veil.material : null;
        for (float t = 0; t < 1.2f; t += Time.deltaTime)
        {
            if (m != null) { var c = Bond; c.a = .45f * (1 - t / 1.2f); m.color = c; }
            yield return null;
        }
        MundoInferiorBlockout.Instance?.Hud?.Notify("El paso se abre", "El lazo se deshace: puedes seguir.", UIIcon.Check, UIPalette.Jade);
        gameObject.SetActive(false);
    }

    private void BuildVeil(float height)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());
        quad.name = "Velo";
        quad.layer = 2;
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = new Vector3(0, height * .5f, 0);
        quad.transform.localScale = new Vector3(_width, height, 1);
        _veil = quad.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        var m = new Material(shader) { name = "Velo_Lazo" };
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetFloat("_Cull", 0);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = 3000;
        var tex = VeilTexture();
        m.mainTexture = tex; if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        _veil.material = m;
        _veil.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _motes = MIParticles.Motes(transform, new Vector3(0, height * .5f, 0), new Vector3(_width, height, .4f), new Color(.7f, .5f, 1f, .7f), _width * height * 2.5f);
    }

    // Vertical strands, denser near the floor, fading at the top and the sides.
    private static Texture2D _texture;
    private static Texture2D VeilTexture()
    {
        if (_texture != null) return _texture;
        const int w = 128, h = 128;
        _texture = new Texture2D(w, h, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "Velo_Lazo" };
        var rng = new System.Random(7);
        var strands = new float[w];
        for (int i = 0; i < 18; i++)
        {
            int x0 = rng.Next(w); float a = .5f + (float)rng.NextDouble() * .5f;
            for (int dx = -3; dx <= 3; dx++) { int x = (x0 + dx + w) % w; strands[x] = Mathf.Max(strands[x], a * (1 - Mathf.Abs(dx) / 4f)); }
        }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float v = y / (float)(h - 1), u = x / (float)(w - 1);
                float side = Mathf.Clamp01(Mathf.Min(u, 1 - u) / .08f);
                float a = (.35f + .65f * strands[x]) * Mathf.Lerp(1f, .15f, v) * side;
                _texture.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        _texture.Apply();
        return _texture;
    }
}

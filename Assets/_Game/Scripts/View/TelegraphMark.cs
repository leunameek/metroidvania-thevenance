using UnityEngine;

// The floor mark of an announced blow: a translucent ring lying on the ground with a fill that grows
// toward the edge as the blow approaches, so the reach and the moment read at a glance. It is drawn
// flat on the floor and transparent, so the characters standing in it stay visible.
public sealed class TelegraphMark : MonoBehaviour
{
    private static Material _ringMaterial, _fillMaterial;
    private static Texture2D _ring, _disc;
    private Transform _fill;
    private Renderer _ringRenderer, _fillRenderer;
    private Color _color = new Color(.95f, .3f, .2f);
    private float _radius = 1;

    public static TelegraphMark Create(Transform parent, string name = "Aviso")
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var mark = go.AddComponent<TelegraphMark>();
        mark._ringRenderer = Quad(go.transform, "Anillo", RingMaterial);
        mark._fillRenderer = Quad(go.transform, "Relleno", FillMaterial);
        mark._fill = mark._fillRenderer.transform;
        go.SetActive(false);
        return mark;
    }

    // Shows the mark centred on a floor point (`at` is already on the ground).
    public void Show(Vector3 at, float radius, Color? color = null)
    {
        _radius = radius; _color = color ?? new Color(.95f, .3f, .2f);
        transform.position = at + Vector3.up * .03f;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _ringRenderer.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        SetProgress(0);
        gameObject.SetActive(true);
    }

    // 0 when announced, 1 when the blow lands: the fill reaches the ring and brightens.
    public void SetProgress(float k)
    {
        k = Mathf.Clamp01(k);
        float r = Mathf.Lerp(.15f, 1f, k) * _radius;
        _fill.localScale = new Vector3(r * 2, r * 2, 1);
        float pulse = .5f + .5f * Mathf.Sin(Time.time * 14f);
        var ring = _color; ring.a = Mathf.Lerp(.55f, .95f, k) * (.85f + .15f * pulse);
        var fill = _color; fill.a = Mathf.Lerp(.18f, .5f, k);
        _ringRenderer.material.color = ring;
        _fillRenderer.material.color = fill;
    }

    // Only the outer ring, fully opaque (a marker above a body part, not a reach on the floor).
    public void RingOnly(float alpha = 1f)
    {
        _fill.gameObject.SetActive(false);
        var ring = _color; ring.a = alpha; _ringRenderer.material.color = ring;
    }

    // Grows the whole mark (the scream ring that widens toward its reach).
    public void SetRadius(float radius)
    {
        _radius = radius;
        _ringRenderer.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
    }

    public void Hide() { if (this != null) gameObject.SetActive(false); }
    public bool Visible => this != null && gameObject.activeSelf;

    private static Renderer Quad(Transform parent, string name, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(go.GetComponent<Collider>());
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localRotation = Quaternion.Euler(90, 0, 0);
        var renderer = go.GetComponent<Renderer>();
        renderer.material = new Material(material);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
    }

    private static Material RingMaterial => _ringMaterial != null ? _ringMaterial : _ringMaterial = Transparent("Aviso_Anillo", RingTexture(), 3010);
    private static Material FillMaterial => _fillMaterial != null ? _fillMaterial : _fillMaterial = Transparent("Aviso_Relleno", DiscTexture(), 3009);

    private static Material Transparent(string name, Texture texture, int queue)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        var m = new Material(shader) { name = name };
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.SetFloat("_Cull", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = queue;
        m.mainTexture = texture;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texture);
        return m;
    }

    // A crisp ring with a soft inner glow.
    private static Texture2D RingTexture()
    {
        if (_ring != null) return _ring;
        const int n = 128;
        _ring = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "Aviso_Anillo" };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - .5f, n / 2f - .5f)) / (n / 2f);
                float edge = Mathf.Clamp01(1 - Mathf.Abs(d - .93f) / .06f);
                float glow = d < .93f ? Mathf.Pow(d / .93f, 6) * .45f : 0;
                _ring.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(Mathf.Max(edge, glow))));
            }
        _ring.Apply();
        return _ring;
    }

    // A filled disc with a soft rim.
    private static Texture2D DiscTexture()
    {
        if (_disc != null) return _disc;
        const int n = 64;
        _disc = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "Aviso_Disco" };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - .5f, n / 2f - .5f)) / (n / 2f);
                _disc.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1 - d) / .08f)));
            }
        _disc.Apply();
        return _disc;
    }

    // Floor point under `at`, skipping the characters (the player's capsule, creatures) so the mark
    // never lands on a head.
    public static Vector3 Floor(Vector3 at, float fallbackY)
    {
        var hits = Physics.RaycastAll(at + Vector3.up * 1.5f, Vector3.down, 12f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue; Vector3 point = new Vector3(at.x, fallbackY, at.z);
        foreach (var h in hits)
        {
            if (h.distance >= best || IsCharacter(h.collider)) continue;
            best = h.distance; point = h.point;
        }
        return point;
    }

    private static bool IsCharacter(Collider c) =>
        c is CharacterController || c.GetComponentInParent<PlayerController>() != null || c.GetComponentInParent<CharacterActions>() != null
        || c.GetComponentInParent<MIDashEnemy>() != null;
}

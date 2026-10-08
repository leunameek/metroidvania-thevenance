using System;
using System.Collections.Generic;
using UnityEngine;

// Marks something the player should go to (2026-10-07 playtest: players did not see the tutorial
// stations, nor the coca and the masks floating in their niches). While its condition holds the
// thing glows gold, breathing (an additive rim drawn over its own meshes), a soft light pulses on
// it and, for the main steps, a column of light with rising motes stands over it, seen from afar.
// It fades out when the condition stops holding (examined, taken, done). Nothing is added to the
// object's hierarchy (the fitting sketch and the framing read it): the glow copies live under one
// scene object and follow the object's meshes every frame.
public sealed class Beacon : MonoBehaviour
{
    public static readonly Color Gold = new Color(1f, .76f, .38f);
    private const float BeamHeight = 7f, BeamRadius = .32f;

    private Transform _shape;
    private Func<bool> _on;
    private Color _color = Gold;
    private bool _beam;
    private float _level;
    private Light _light;
    private readonly List<(MeshFilter filter, Renderer renderer, MeshRenderer copy)> _parts = new List<(MeshFilter, Renderer, MeshRenderer)>();
    private MeshRenderer _beamView;
    private MaterialPropertyBlock _properties, _beamProperties;
    private static Transform _views;
    private static Material _glow, _column;
    private static Mesh _cylinder;

    // shape: the thing that glows (its meshes); on: while it should; beam: the column over it.
    public static Beacon Attach(GameObject host, Transform shape, Func<bool> on, bool beam = true, Color? color = null)
    {
        if (host == null || shape == null || on == null) return null;
        var beacon = host.AddComponent<Beacon>();
        beacon._shape = shape; beacon._on = on; beacon._beam = beam;
        if (color.HasValue) beacon._color = color.Value;
        return beacon;
    }

    private void Collect()
    {
        if (_shape == null) return;
        foreach (var filter in _shape.GetComponentsInChildren<MeshFilter>(true))
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || filter.sharedMesh == null || _parts.Exists(p => p.filter == filter)) continue;
            var copy = View("Brillo " + filter.name, filter.sharedMesh, _glow);
            _parts.Add((filter, renderer, copy));
        }
    }

    private static MeshRenderer View(string name, Mesh mesh, Material material)
    {
        if (_views == null) _views = new GameObject("Brillos guía").transform;
        var go = new GameObject(name);
        go.transform.SetParent(_views, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        var materials = new Material[Mathf.Max(1, mesh.subMeshCount)];
        for (int i = 0; i < materials.Length; i++) materials[i] = material;
        r.sharedMaterials = materials;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
        r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off; r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        go.SetActive(false);
        return r;
    }

    private void OnDestroy()
    {
        foreach (var p in _parts) if (p.copy != null) Destroy(p.copy.gameObject);
        if (_beamView != null) Destroy(_beamView.gameObject);
    }

    private void Show(bool on)
    {
        foreach (var p in _parts) if (p.copy != null && p.copy.gameObject.activeSelf != on) p.copy.gameObject.SetActive(on);
        if (_beamView != null && _beamView.gameObject.activeSelf != on) _beamView.gameObject.SetActive(on);
    }

    private void LateUpdate()
    {
        bool on;
        try { on = _shape != null && _shape.gameObject.activeInHierarchy && _on(); }
        catch (Exception) { on = false; }
        _level = Mathf.MoveTowards(_level, on ? 1f : 0f, Time.unscaledDeltaTime * (on ? 1.2f : 2.5f));
        if (_light != null) _light.enabled = _level > 0;
        if (_level <= 0 || _shape == null) { Show(false); return; }
        if (!Materials()) return;
        if (_parts.Count == 0 || Time.frameCount % 120 == 0) Collect();
        if (_properties == null) { _properties = new MaterialPropertyBlock(); _beamProperties = new MaterialPropertyBlock(); }

        float t = Time.time;
        float breath = .5f + .5f * Mathf.Sin(t * 2.6f);
        float level = Mathf.SmoothStep(0, 1, _level);
        bool any = false; Bounds bounds = default;
        _properties.SetColor("_Color", _color);
        _properties.SetFloat("_Intensity", level * (.55f + .75f * breath));
        foreach (var (filter, renderer, copy) in _parts)
        {
            if (copy == null) continue;
            bool shown = filter != null && renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy;
            if (copy.gameObject.activeSelf != shown) copy.gameObject.SetActive(shown);
            if (!shown) continue;
            var m = renderer.localToWorldMatrix;
            copy.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
            copy.transform.localScale = m.lossyScale;
            copy.SetPropertyBlock(_properties);
            if (!any) { bounds = renderer.bounds; any = true; } else bounds.Encapsulate(renderer.bounds);
        }
        // A rigged body (the training guardian) has no plain meshes to glow: the column still stands on it.
        if (!any)
            foreach (var renderer in _shape.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;
                if (!any) { bounds = renderer.bounds; any = true; } else bounds.Encapsulate(renderer.bounds);
            }
        if (!any) bounds = new Bounds(_shape.position + Vector3.up * .5f, Vector3.one);

        if (_beam)
        {
            if (_beamView == null) _beamView = View("Columna guía", Cylinder(), _column);
            if (!_beamView.gameObject.activeSelf) _beamView.gameObject.SetActive(true);
            _beamProperties.SetColor("_Color", _color);
            _beamProperties.SetFloat("_Intensity", level * (.8f + .2f * breath));
            _beamProperties.SetFloat("_Height", BeamHeight);
            _beamView.SetPropertyBlock(_beamProperties);
            float radius = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * .9f, BeamRadius * .7f, BeamRadius * 2.2f);
            _beamView.transform.position = new Vector3(bounds.center.x, bounds.min.y - .05f, bounds.center.z);
            _beamView.transform.localScale = new Vector3(radius, BeamHeight, radius);
        }

        if (_light == null)
        {
            var go = new GameObject("Luz guía");
            go.transform.SetParent(transform, false);
            _light = go.AddComponent<Light>();
            _light.type = LightType.Point; _light.shadows = LightShadows.None; _light.color = _color;
            _light.range = 3f;
        }
        _light.transform.position = bounds.center + Vector3.up * (bounds.extents.y + .3f);
        _light.intensity = level * (.7f + 1.1f * breath);
    }

    private static bool Materials()
    {
        if (_glow == null)
        {
            var shader = Resources.Load<Shader>("Effects/BeaconGlow");
            if (shader == null) return false;
            _glow = new Material(shader) { name = "Brillo_Guia", enableInstancing = false };
        }
        if (_column == null)
        {
            var shader = Resources.Load<Shader>("Effects/BeaconBeam");
            if (shader == null) return false;
            _column = new Material(shader) { name = "Columna_Guia" };
        }
        return true;
    }

    // An open cylinder, radius 1, from y 0 to 1: uv.x around, uv.y up.
    private static Mesh Cylinder()
    {
        if (_cylinder != null) return _cylinder;
        const int sides = 28;
        var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
        for (int i = 0; i <= sides; i++)
        {
            float a = i * Mathf.PI * 2f / sides;
            var n = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            vertices.Add(n); normals.Add(n); uvs.Add(new Vector2((float)i / sides, 0));
            vertices.Add(n + Vector3.up); normals.Add(n); uvs.Add(new Vector2((float)i / sides, 1));
            if (i == sides) continue;
            int b = i * 2;
            triangles.AddRange(new[] { b, b + 1, b + 2, b + 1, b + 3, b + 2 });
        }
        _cylinder = new Mesh { name = "Columna guía" };
        _cylinder.SetVertices(vertices); _cylinder.SetNormals(normals); _cylinder.SetUVs(0, uvs); _cylinder.SetTriangles(triangles, 0);
        _cylinder.bounds = new Bounds(new Vector3(0, .5f, 0), new Vector3(2, 1, 2));
        return _cylinder;
    }
}

using UnityEngine;

// A state the player reads from the enemy instead of the HUD (2026-10-07 playtest): a pulsing
// light and a slow aura of motes in one colour around a body part. Quimue glows gold while the sun
// is the active origin and silver-blue for the moon; the serpent's exposed head glows gold.
public sealed class DuelGlow : MonoBehaviour
{
    private Light _light;
    private ParticleSystem _aura;
    private Transform _follow;
    private Vector3 _offset;
    private Color _color;
    private float _strength, _shown;
    private TelegraphMark _halo;
    private float _haloHeight, _haloRadius;
    private Transform _haloCentre;

    public static DuelGlow Create(string name, Transform follow, Vector3 offset, float range = 5f)
    {
        var glow = new GameObject(name).AddComponent<DuelGlow>();
        glow._follow = follow; glow._offset = offset;
        glow._light = glow.gameObject.AddComponent<Light>();
        glow._light.type = LightType.Point; glow._light.range = range; glow._light.shadows = LightShadows.None; glow._light.intensity = 0;
        var go = new GameObject("Aura");
        go.transform.SetParent(glow.transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = MIParticles.Material;
        var main = ps.main; main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.8f); main.startSpeed = new ParticleSystem.MinMaxCurve(.2f, .6f);
        main.startSize = new ParticleSystem.MinMaxCurve(.12f, .3f); main.gravityModifier = -.08f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .7f;
        var emission = ps.emission; emission.rateOverTime = 0;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .25f), new GradientAlphaKey(0, 1) });
        fade.color = g;
        ps.Play();
        glow._aura = ps;
        return glow;
    }

    // A golden ring floating above the body part, turned to the camera, readable from the high duel camera even
    // over a bright sky (2026-10-08 playtest: the light alone did not show which head was open).
    // centre: the ring leans outward from it (to the side of its own head) so two heads that
    // stand close never share one ring and it stays clear of the HUD above.
    public DuelGlow WithHalo(float height, float radius, Transform centre = null)
    {
        _haloCentre = centre;
        _haloHeight = height;
        _halo = TelegraphMark.Create(transform, "Halo");
        _haloRadius = radius;
        _halo.Show(transform.position + Vector3.up * height, radius, _color.a > 0 ? _color : new Color(1f, .8f, .3f));
        return this;
    }

    // strength 0 hides it; 1 is the full reading glow.
    public void Set(Color color, float strength)
    {
        _color = color; _strength = Mathf.Clamp01(strength);
        _light.color = color;
        if (_halo != null) _halo.Show(transform.position + Vector3.up * _haloHeight, _haloRadius, color);
        var main = _aura.main; main.startColor = new Color(color.r, color.g, color.b, .85f);
        var emission = _aura.emission; emission.rateOverTime = 45f * _strength;
    }

    // A moment brighter (the origin just announced itself).
    public void Flash() => _shown = Mathf.Max(_shown, 1.6f);

    private void LateUpdate()
    {
        if (_follow == null) { Destroy(gameObject); return; }
        transform.position = _follow.position + _offset;
        _shown = Mathf.MoveTowards(_shown, _strength, Time.deltaTime * 1.5f);
        float pulse = .8f + .2f * Mathf.Sin(Time.time * 3.2f);
        _light.intensity = _shown * 9f * pulse;
        _light.enabled = _shown > .01f;
        if (_halo != null)
        {
            // Upright toward the camera (a flat ring was seen edge-on from the duel camera).
            var cam = Camera.main;
            Vector3 lift = Vector3.up * (_haloHeight + .08f * Mathf.Sin(Time.time * 2.4f));
            if (cam != null && _haloCentre != null)
            {
                float side = Mathf.Sign(Vector3.Dot(transform.position - _haloCentre.position, cam.transform.right));
                // Around the head itself, nudged a little outward: it never climbs under the HUD.
                lift = cam.transform.right * side * _haloHeight * .25f;
            }
            _halo.transform.position = transform.position + lift;
            if (cam != null) _halo.transform.rotation = cam.transform.rotation * Quaternion.Euler(-90, 0, 0);
            _halo.RingOnly(.75f + .25f * pulse); // a crisp ring, not a filled disc (it read as another glowing orb)
            _halo.transform.localScale = Vector3.one * (.9f + .15f * pulse);
            _halo.gameObject.SetActive(_shown > .05f);
        }
    }
}

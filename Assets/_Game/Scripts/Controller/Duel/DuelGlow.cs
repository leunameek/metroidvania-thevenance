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

    // strength 0 hides it; 1 is the full reading glow.
    public void Set(Color color, float strength)
    {
        _color = color; _strength = Mathf.Clamp01(strength);
        _light.color = color;
        var main = _aura.main; main.startColor = new Color(color.r, color.g, color.b, .85f);
        var emission = _aura.emission; emission.rateOverTime = 28f * _strength;
    }

    // A moment brighter (the origin just announced itself).
    public void Flash() => _shown = Mathf.Max(_shown, 1.6f);

    private void LateUpdate()
    {
        if (_follow == null) { Destroy(gameObject); return; }
        transform.position = _follow.position + _offset;
        _shown = Mathf.MoveTowards(_shown, _strength, Time.deltaTime * 1.5f);
        float pulse = .8f + .2f * Mathf.Sin(Time.time * 3.2f);
        _light.intensity = _shown * 6f * pulse;
        _light.enabled = _shown > .01f;
    }
}

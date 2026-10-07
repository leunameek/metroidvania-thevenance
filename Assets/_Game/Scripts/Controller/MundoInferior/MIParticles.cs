using UnityEngine;

// Built-in particle presets shared by the scene builder (ambient) and runtime events (bursts).
public static class MIParticles
{
    // Saved asset assigned by the scene builder, so particles in the scene keep their material.
    public static Material SharedMaterial;
    private static Material _material;
    public static Material Material
    {
        get
        {
            if (SharedMaterial != null) return SharedMaterial;
            if (SharedMaterial == null && Application.isPlaying) SharedMaterial = Resources.Load<Material>("MIParticulas");
            if (SharedMaterial != null) return SharedMaterial;
            if (_material != null) return _material;
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            _material = new Material(shader) { name = "MI_Particulas" };
            _material.SetFloat("_Surface", 1); _material.SetFloat("_Blend", 2); // transparent, additive
            _material.SetOverrideTag("RenderType", "Transparent");
            _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _material.SetInt("_ZWrite", 0);
            _material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _material.renderQueue = 3000;
            _material.mainTexture = SoftDot();
            return _material;
        }
    }
    private static Texture2D _dot;
    private static Texture2D SoftDot()
    {
        if (_dot != null) return _dot;
        _dot = new Texture2D(32, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "MI_Punto" };
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f;
                float a = Mathf.Clamp01(1 - d); a *= a;
                _dot.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        _dot.Apply();
        return _dot;
    }

    private static ParticleSystem Create(Transform parent, string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false); go.transform.localPosition = localPosition;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var renderer = go.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = Material;
        return ps;
    }

    // Slow motes of dust drifting through a room volume.
    public static ParticleSystem Motes(Transform parent, Vector3 center, Vector3 size, Color color, float rate = 10)
    {
        var ps = Create(parent, "Polvo en suspensión", center);
        var main = ps.main; main.loop = true; main.startLifetime = new ParticleSystem.MinMaxCurve(6, 10); main.startSpeed = 0.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(.03f, .08f); main.startColor = color; main.maxParticles = 300; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = true;
        var emission = ps.emission; emission.rateOverTime = rate;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = size;
        var noise = ps.noise; noise.enabled = true; noise.strength = .15f; noise.frequency = .3f;
        var fade = ps.colorOverLifetime; fade.enabled = true; fade.color = FadeInOut(color);
        ps.Play();
        return ps;
    }

    // Warm sparks rising around a find.
    public static ParticleSystem Sparks(Transform parent, Vector3 localPosition, Color color)
    {
        var ps = Create(parent, "Destellos del hallazgo", localPosition);
        var main = ps.main; main.loop = true; main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f); main.startSpeed = new ParticleSystem.MinMaxCurve(.1f, .35f);
        main.startSize = new ParticleSystem.MinMaxCurve(.03f, .07f); main.startColor = color; main.maxParticles = 60; main.gravityModifier = -.05f;
        var emission = ps.emission; emission.rateOverTime = 12;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .35f;
        var fade = ps.colorOverLifetime; fade.enabled = true; fade.color = FadeInOut(color);
        ps.Play();
        return ps;
    }

    // Water drips from the ceiling line of a cave wall.
    public static ParticleSystem Drips(Transform parent, Vector3 localPosition, Vector3 size)
    {
        var ps = Create(parent, "Goteo", localPosition);
        var main = ps.main; main.loop = true; main.startLifetime = 1.4f; main.startSpeed = 0; main.gravityModifier = 1f;
        main.startSize = new ParticleSystem.MinMaxCurve(.03f, .05f); main.startColor = new Color(.6f, .85f, 1f, .7f); main.maxParticles = 40;
        var emission = ps.emission; emission.rateOverTime = 1.6f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = size;
        ps.Play();
        return ps;
    }

    // One-shot dust or light burst (find confirmed, slab breaking, rock impact, shield break).
    public static ParticleSystem Burst(Vector3 position, Color color, int count = 40, float speed = 2.5f, float size = .12f, float gravity = .4f)
    {
        var ps = Create(null, "Ráfaga", position);
        var main = ps.main; main.loop = false; main.startLifetime = new ParticleSystem.MinMaxCurve(.6f, 1.3f); main.startSpeed = new ParticleSystem.MinMaxCurve(speed * .4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * .5f, size); main.startColor = color; main.gravityModifier = gravity; main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = ps.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)count) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .3f;
        var fade = ps.colorOverLifetime; fade.enabled = true; fade.color = FadeInOut(color);
        ps.Play();
        return ps;
    }

    private static ParticleSystem.MinMaxGradient FadeInOut(Color color)
    {
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(color.a, .2f), new GradientAlphaKey(color.a, .7f), new GradientAlphaKey(0, 1) });
        return new ParticleSystem.MinMaxGradient(gradient);
    }
}

public static class MIBurst
{
    public static void Spawn(Vector3 position, Color color) => MIParticles.Burst(position, color, 60, 2.2f, .1f, -.2f);
}

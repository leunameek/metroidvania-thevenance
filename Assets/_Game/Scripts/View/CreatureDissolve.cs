using System;
using System.Collections;
using UnityEngine;

// A beaten enemy leaves no remains: motes rise from the surface of its body for a moment, the body
// sinks a little and goes out in a last burst, and the motes drift away and fade.
public sealed class CreatureDissolve : MonoBehaviour
{
    public const float Duration = 1.6f;

    // root: what disappears (deactivated at the end). color: the light of the motes.
    public static void Run(Transform root, Color color, float delay = .5f, Action done = null)
    {
        if (root == null) { done?.Invoke(); return; }
        var host = new GameObject("Disolución").AddComponent<CreatureDissolve>();
        host.StartCoroutine(host.Play(root, color, delay, done));
    }

    // A freed creature leaves no invisible wall: every solid collider of its figure goes off
    // (2026-10-07 playtest: the great guardians left a hitbox where they stood).
    public static void Unblock(Transform root)
    {
        if (root == null) return;
        foreach (var c in root.GetComponentsInChildren<Collider>(true)) if (!c.isTrigger) c.enabled = false;
    }

    // Hides at once (a creature already beaten when the scene loads).
    public static void HideNow(Transform root) { if (root != null) root.gameObject.SetActive(false); }

    private IEnumerator Play(Transform root, Color color, float delay, Action done)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (root == null) { Finish(done); yield break; }
        var bounds = Bounds(root);
        var motes = Motes(root, bounds, color);
        Vector3 start = root.position;
        for (float t = 0; t < Duration && root != null; t += Time.deltaTime)
        {
            float k = t / Duration;
            root.position = start + Vector3.down * (k * k * .35f);
            yield return null;
        }
        if (motes != null)
        {
            motes.transform.SetParent(null, true);
            motes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (root != null)
        {
            MIParticles.Burst(bounds.center, new Color(color.r, color.g, color.b, .9f), 70, 2.8f, .12f, -.35f);
            root.gameObject.SetActive(false);
            root.position = start;
        }
        yield return new WaitForSeconds(2.2f);
        if (motes != null) Destroy(motes.gameObject);
        Finish(done);
    }

    private void Finish(Action done) { done?.Invoke(); Destroy(gameObject); }

    private static Bounds Bounds(Transform root)
    {
        bool any = false; var b = new Bounds(root.position + Vector3.up, Vector3.one);
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return b;
    }

    // Motes from the body surface when there is a skinned body, else from its volume.
    private static ParticleSystem Motes(Transform root, Bounds bounds, Color color)
    {
        var go = new GameObject("Motas");
        go.transform.SetParent(root, false);
        go.transform.position = bounds.center;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = MIParticles.Material;
        var main = ps.main;
        main.loop = true; main.duration = Duration;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(.1f, .5f);
        main.startSize = new ParticleSystem.MinMaxCurve(.05f, .14f);
        main.startColor = color;
        main.gravityModifier = -.12f;
        main.maxParticles = 600;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission; emission.rateOverTime = 260;
        var shape = ps.shape;
        SkinnedMeshRenderer body = null; int best = -1;
        foreach (var s in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (s.sharedMesh != null && s.sharedMesh.vertexCount > best) { best = s.sharedMesh.vertexCount; body = s; }
        if (body != null)
        {
            go.transform.localPosition = Vector3.zero;
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = body;
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = bounds.size;
        }
        var noise = ps.noise; noise.enabled = true; noise.strength = .4f; noise.frequency = .6f;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(color, .4f), new GradientColorKey(color, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .15f), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) });
        fade.color = new ParticleSystem.MinMaxGradient(gradient);
        var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, .2f));
        ps.Play();
        return ps;
    }
}

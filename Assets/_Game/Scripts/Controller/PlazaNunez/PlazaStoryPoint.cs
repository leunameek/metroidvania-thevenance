using System;
using System.Collections.Generic;
using UnityEngine;

// Something of the story the player uses with E in the plaza (Bachué, the urns, the circle of the
// final duel). TechnicalDemoController offers the nearest available one after the stations and
// before the portals; the HUD shows its prompt.
public sealed class PlazaStoryPoint : MonoBehaviour
{
    public static readonly List<PlazaStoryPoint> All = new List<PlazaStoryPoint>();
    public float range = 2.6f;
    public Func<bool> available = () => true;
    public Func<string> prompt = () => "";
    public Action use;

    public bool Available => isActiveAndEnabled && (available == null || available());
    public string Prompt => prompt != null ? prompt() : "";

    private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    private void OnDisable() { All.Remove(this); }

    public static PlazaStoryPoint Nearest(Vector3 position)
    {
        PlazaStoryPoint best = null; float bestDistance = float.MaxValue;
        foreach (var point in All)
        {
            if (point == null || !point.Available) continue;
            Vector3 d = point.transform.position - position; d.y = 0;
            float distance = d.magnitude;
            if (distance <= point.range && distance < bestDistance) { best = point; bestDistance = distance; }
        }
        return best;
    }

    public static PlazaStoryPoint Create(string name, Transform parent, Vector3 position, float range,
        Func<bool> available, Func<string> prompt, Action use)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        var point = go.AddComponent<PlazaStoryPoint>();
        point.range = range; point.available = available; point.prompt = prompt; point.use = use;
        return point;
    }
}

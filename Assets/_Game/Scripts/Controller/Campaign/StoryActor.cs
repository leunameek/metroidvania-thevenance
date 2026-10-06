using System.Collections.Generic;
using UnityEngine;

// Marks who is speaking in the scene, so the story camera can frame them (speaker name as in
// historia.json: "Bachué", "Custodio de Raíces"...). The player is registered as "Nemequene".
public sealed class StoryActor : MonoBehaviour
{
    public string speaker;
    [Tooltip("Height of the face above the pivot, in metres.")]
    public float faceHeight = 1.6f;
    private static readonly List<StoryActor> Actors = new List<StoryActor>();

    public Vector3 Face => transform.position + Vector3.up * faceHeight;

    private void OnEnable() { if (!Actors.Contains(this)) Actors.Add(this); }
    private void OnDisable() { Actors.Remove(this); }

    public static StoryActor Find(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var actor in Actors) if (actor != null && actor.isActiveAndEnabled && actor.speaker == name) return actor;
        return null;
    }

    public static StoryActor Ensure(GameObject target, string name, float faceHeight)
    {
        if (target == null) return null;
        var actor = target.GetComponent<StoryActor>() ?? target.AddComponent<StoryActor>();
        actor.speaker = name; actor.faceHeight = faceHeight;
        return actor;
    }
}

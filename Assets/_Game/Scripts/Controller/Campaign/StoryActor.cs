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

    // The face: the head bone of a rigged body (so someone lying or kneeling is framed where their
    // head really is), otherwise the authored height above the pivot.
    public Vector3 Face
    {
        get
        {
            var head = Head;
            return head != null ? head.position + Vector3.up * .06f : transform.position + Vector3.up * faceHeight;
        }
    }

    // Lying down (the head near the floor): the story camera frames from above.
    public bool Lying => Head != null && Head.position.y - transform.position.y < faceHeight * .45f;

    private Transform _head;
    private bool _headSearched;
    private Transform Head
    {
        get
        {
            if (_head != null || _headSearched && Time.frameCount % 30 != 0) return _head;
            _headSearched = true;
            foreach (var animator in GetComponentsInChildren<Animator>())
            {
                if (animator == null || !animator.isActiveAndEnabled || !animator.isHuman) continue;
                _head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (_head != null) break;
            }
            return _head;
        }
    }

    private void OnEnable() { if (!Actors.Contains(this)) Actors.Add(this); }
    private void OnDisable() { Actors.Remove(this); }

    public static IReadOnlyList<StoryActor> All => Actors;

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

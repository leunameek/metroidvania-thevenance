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
    // Who is saying the current line: they look at the camera while they speak.
    public static StoryActor Speaking;

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

    private Transform _head, _neck;
    private bool _headSearched;
    private Quaternion _faceLocal = Quaternion.identity;
    private float _look;

    // While speaking, the head (and a little the neck) turns toward the camera, after the clip
    // has posed the body (2026-10-07 playtest: the characters should look at us). Limited, eased in
    // and out; someone lying only turns the head a little.
    private void LateUpdate()
    {
        float target = Speaking == this ? 1f : 0f;
        _look = Mathf.MoveTowards(_look, target, Time.unscaledDeltaTime * 2.5f);
        if (_look <= 0f) return;
        var head = Head; var camera = Camera.main;
        if (head == null || camera == null) return;
        Vector3 face = head.rotation * (_faceLocal * Vector3.forward);
        Vector3 toCamera = camera.transform.position - head.position;
        if (toCamera.sqrMagnitude < .01f) return;
        float limit = Lying ? 25f : 55f;
        var turn = Quaternion.FromToRotation(face, toCamera.normalized);
        turn.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Clamp(angle, -limit, limit) * Mathf.SmoothStep(0, 1, _look);
        if (Mathf.Abs(angle) < .01f || float.IsNaN(axis.x)) return;
        if (_neck != null && !Lying)
        {
            _neck.rotation = Quaternion.AngleAxis(angle * .35f, axis) * _neck.rotation;
            head.rotation = Quaternion.AngleAxis(angle * .65f, axis) * head.rotation;
        }
        else head.rotation = Quaternion.AngleAxis(angle, axis) * head.rotation;
    }
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
                _neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                // The face looks along the body's forward when the character stands as built.
                if (_head != null) { _faceLocal = Quaternion.Inverse(_head.rotation) * Quaternion.LookRotation(transform.forward, Vector3.up); break; }
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

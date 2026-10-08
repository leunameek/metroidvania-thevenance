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
    // Who is saying the current line, and who Nemequene is talking with (the last one who spoke
    // to him). While a conversation lasts everyone near looks at Nemequene and he looks at whoever
    // talks to him (2026-10-07 playtest: they should look at each other, not at the camera).
    public static StoryActor Speaking, Partner;
    public static bool Conversing;
    public const string Hero = "Nemequene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Speaking = null; Partner = null; Conversing = false; }

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
    private Vector3 _lookAt;
    // The facing before the body turned to talk, to go back to once the conversation is over
    // (2026-10-07 playtest: Bachué and Quimue stayed turned after their lines).
    private bool _turned;
    private Quaternion _restRotation, _setRotation;
    private Vector3 _setPosition;

    public bool IsHero => speaker == Hero;

    // Who this one looks at in the current conversation: Nemequene looks at whoever speaks to
    // him (or at the one he is answering); everyone else near looks at Nemequene.
    public StoryActor LookTarget
    {
        get
        {
            if (!Conversing || !isActiveAndEnabled) return null;
            if (IsHero) return Speaking != null && Speaking != this ? Speaking : Partner != this ? Partner : null;
            var hero = Find(Hero);
            if (hero == null || hero == this) return null;
            return Speaking == this || Vector3.Distance(hero.transform.position, transform.position) < 12f ? hero : null;
        }
    }

    // During a conversation the head (and a little the neck) turns to the one this actor looks at,
    // after the clip has posed the body. Whoever speaks, and Nemequene listening, also turn the
    // whole body when the other is behind them (the Custodio de Raíces talked with his back to
    // Nemequene). Limited and eased; someone lying, kneeling or sitting only turns the head.
    private void LateUpdate()
    {
        var other = LookTarget;
        if (other != null) _lookAt = other.Face;
        if (other == null && _turned) TurnBack();
        _look = Mathf.MoveTowards(_look, other != null ? 1f : 0f, Time.unscaledDeltaTime * 2.5f);
        if (_look <= 0f) return;
        var head = Head;
        if (head == null) return;
        if (other != null && (Speaking == this || IsHero)) TurnBody(_lookAt);
        Vector3 face = head.rotation * (_faceLocal * Vector3.forward);
        Vector3 toTarget = _lookAt - head.position;
        if (toTarget.sqrMagnitude < .01f) return;
        float limit = Lying ? 25f : 60f;
        var turn = Quaternion.FromToRotation(face, toTarget.normalized);
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

    // The body comes round (about the vertical, from the feet) until the other is within a
    // comfortable turn of the head; held poses and creatures stay as they are.
    private void TurnBody(Vector3 point)
    {
        var actions = CharacterActions.Of(this);
        if (Lying || actions != null && actions.Posed) return;
        Vector3 to = point - transform.position; to.y = 0;
        if (to.sqrMagnitude < .04f) return;
        float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), to, Vector3.up);
        if (Mathf.Abs(angle) < 20f) return;
        float step = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle) - 15f, Time.unscaledDeltaTime * 150f);
        // Moved or turned by the scene since our last turn: that is its new resting facing.
        if (!_turned || Moved()) { _restRotation = transform.rotation; _turned = !IsHero; }
        transform.rotation = Quaternion.AngleAxis(step, Vector3.up) * transform.rotation;
        Remember();
    }

    private bool Moved() => Quaternion.Angle(transform.rotation, _setRotation) > 1f || (transform.position - _setPosition).sqrMagnitude > .01f;
    private void Remember() { _setRotation = transform.rotation; _setPosition = transform.position; }

    // Back to the facing it had before the conversation, unless the scene has placed it since.
    private void TurnBack()
    {
        if (Moved()) { _turned = false; return; }
        var actions = CharacterActions.Of(this);
        if (actions != null && actions.Posed) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _restRotation, Time.unscaledDeltaTime * 150f);
        Remember();
        if (Quaternion.Angle(transform.rotation, _restRotation) < .5f) _turned = false;
    }

    // Where the shot of this speaker is taken from: in front of them, toward the one they talk to.
    public Vector3? Facing
    {
        get
        {
            var other = LookTarget;
            if (other == null) return null;
            Vector3 to = other.transform.position - transform.position; to.y = 0;
            return to.sqrMagnitude > .04f ? to.normalized : (Vector3?)null;
        }
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

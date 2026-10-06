using System.Collections.Generic;
using UnityEngine;

// Procedural animation of the rigged wings (Assets/_Game/Art/Equipment/Alas, Tripo rig bone_0..bone_22).
// Each wing is a chain shoulder -> elbow -> hand plus feather bones. MSWings feeds three values:
// open (0 folded upright behind the shoulders, 1 spread), flap (0 still, 1 flying) and lift (-1 descending,
// +1 rising). The beat starts at the shoulder and reaches elbow, hand and feathers with a lag,
// so the wing travels like a wave; rising beats faster and wider, descending glides.
// Rotations are expressed in the wings' own space (span / up / back) measured from the bind
// pose, so they do not depend on how each bone's axes were exported.
public sealed class MSWingRig : MonoBehaviour
{
    private sealed class Joint
    {
        public Transform Bone;
        public Quaternion Bind, BindInRoot;
        public float Weight, Lag, Fold, Flutter, BeatSign;
        public Vector3 Dir, Side; // bind direction of this wing (root space) and its outward side
    }

    // (bone, wing A or B, beat weight, lag in cycles, fold weight, flutter weight).
    private static readonly (string, bool, float, float, float, float)[] Layout =
    {
        ("bone_1", true, 1f, 0f, 1f, 0f), ("bone_2", true, .55f, .12f, 0f, 0f), ("bone_3", true, .45f, .24f, 0f, 0f),
        ("bone_6", true, .3f, .2f, 0f, .5f), ("bone_7", true, .25f, .3f, 0f, .6f), ("bone_4", true, .2f, .36f, 0f, 1f), ("bone_5", true, .2f, .4f, 0f, 1f),
        ("bone_8", false, 1f, 0f, 1f, 0f), ("bone_9", false, .55f, .12f, 0f, 0f), ("bone_10", false, .45f, .24f, 0f, 0f),
        ("bone_13", false, .3f, .2f, 0f, .5f), ("bone_14", false, .25f, .3f, 0f, .6f), ("bone_15", false, .25f, .3f, 0f, .6f),
        ("bone_11", false, .2f, .36f, 0f, 1f), ("bone_12", false, .2f, .4f, 0f, 1f),
    };

    // Folded pose: each wing turns from its spread direction to point up (1) or hang down (-1)
    // behind the back, a little outward and a little behind the body.
    [SerializeField, Range(-1f, 1f)] private float foldUp = 1f;
    [SerializeField] private float foldOutward = .3f, foldBehind = .35f;
    [SerializeField] private float beatAngle = 30f, beatPeriod = .85f, glideLift = 10f, flutterAngle = 6f;
    // The body the wings hang on: its -forward is "behind" for the fold.
    [SerializeField] private Transform back;

    private readonly List<Joint> _joints = new List<Joint>();
    private Vector3 _up, _back;
    private float _open, _flap, _lift, _phase, _rate = 1f, _amplitude;
    private bool _ready;

    public void SetBack(Transform body) { back = body; _ready = false; _joints.Clear(); }
    // Completed beats (MSWings plays one wing-beat sound per cycle) and the current beat rate.
    public int Beats => Mathf.FloorToInt(_phase);
    public float Rate => _rate;
    public float FoldUp { get => foldUp; set => foldUp = Mathf.Clamp(value, -1f, 1f); }

    // Called every frame by MSWings.
    public void Drive(float open01, float flap01, float lift)
    {
        _open = Mathf.Clamp01(open01); _flap = Mathf.Clamp01(flap01); _lift = Mathf.Clamp(lift, -1f, 1f);
    }

    private void Bind()
    {
        var bones = new Dictionary<string, Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true)) bones[t.name] = t;
        if (!bones.TryGetValue("bone_0", out var root) || !bones.TryGetValue("bone_3", out var handA) || !bones.TryGetValue("bone_10", out var handB)) return;
        // Wing space from the bind pose (in this transform's space).
        Vector3 center = transform.InverseTransformPoint(root.position);
        Vector3 sideA = (transform.InverseTransformPoint(handA.position) - center).normalized;
        Vector3 sideB = (transform.InverseTransformPoint(handB.position) - center).normalized;
        Vector3 span = (sideA - sideB).normalized;
        _up = Vector3.ProjectOnPlane(transform.InverseTransformDirection(Vector3.up), span).normalized;
        Vector3 behind = transform.InverseTransformDirection(back != null ? -back.forward : -transform.forward);
        _back = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(behind, span), _up);
        if (_back.sqrMagnitude < 1e-4f) _back = Vector3.Cross(span, _up);
        _back.Normalize();
        foreach (var (name, wingA, weight, lag, fold, flutter) in Layout)
        {
            if (!bones.TryGetValue(name, out var bone)) continue;
            Vector3 dir = wingA ? sideA : sideB;
            // Signs that raise the tip (beat) and swing it behind the body (fold) for this wing.
            float beatSign = Mathf.Sign(Vector3.Dot(Quaternion.AngleAxis(10f, _back) * dir - dir, _up));
            _joints.Add(new Joint
            {
                Bone = bone, Bind = bone.localRotation, BindInRoot = Quaternion.Inverse(transform.rotation) * bone.rotation,
                Weight = weight, Lag = lag, Fold = fold, Flutter = flutter, BeatSign = beatSign,
                Dir = dir, Side = Vector3.ProjectOnPlane(dir, _up).normalized,
            });
        }
        _ready = _joints.Count > 0;
    }

    private void LateUpdate()
    {
        if (!_ready) { Bind(); if (!_ready) return; }
        float dt = Time.deltaTime;
        // Rising beats faster and wider, descending holds the wings up in a glide.
        float targetRate = _lift > .1f ? 1.45f : _lift < -.1f ? .55f : 1f;
        float targetAmplitude = _flap * (_lift > .1f ? 1.15f : _lift < -.1f ? .35f : .8f);
        _rate = Mathf.MoveTowards(_rate, targetRate, dt * 3f);
        _amplitude = Mathf.MoveTowards(_amplitude, targetAmplitude, dt * 2.5f);
        _phase += dt * _rate / Mathf.Max(.2f, beatPeriod);
        float glide = _flap * Mathf.Max(0f, -_lift) * glideLift;
        float closed = 1f - _open;
        foreach (var j in _joints)
        {
            float beat = Mathf.Sin((_phase - j.Lag) * Mathf.PI * 2f) * beatAngle * j.Weight * _amplitude + glide * j.Weight;
            float flutter = Mathf.Sin((_phase * 2.3f - j.Lag) * Mathf.PI * 2f) * flutterAngle * j.Flutter * (.3f + _amplitude);
            Quaternion beatRot = Quaternion.AngleAxis((beat + flutter) * j.BeatSign, _back);
            // Fold: the rotation that takes this wing's spread direction to its folded direction.
            Vector3 folded = (_up * foldUp + j.Side * foldOutward + _back * foldBehind).normalized;
            Quaternion foldRot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(j.Dir, folded), closed * j.Fold);
            Quaternion inRoot = beatRot * foldRot;
            // The wing-space delta expressed in the bone's own bind frame.
            j.Bone.localRotation = j.Bind * (Quaternion.Inverse(j.BindInRoot) * inRoot * j.BindInRoot);
        }
    }
}

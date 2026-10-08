using UnityEngine;

// The acting of a rigged character: plays an Animator state by name (Talk, Point, Attack, Kneel,
// AtaqueA...) with a short cross-fade and goes back to the resting state when asked. States
// that are missing on a character are skipped, so the same call works on every cast member:
// humanoids share Bacata_Humanoid.controller (Nemequene's Mixamo clips), the player has the same
// action states in Nemequene_Player.controller, and the serpent, jaguar and macaw have their own.
// Built by Editor/Characters/CharacterLibrarySetup (prefabs in Resources/Characters).
public sealed class CharacterActions : MonoBehaviour
{
    [Tooltip("State the character rests in (Locomotion for humanoids, Idle for creatures).")]
    public string restState = "Locomotion";
    [SerializeField] private Animator animator;
    [Tooltip("Half width at the hips, half depth, and half width at the thighs (a skirt, a robe; 0 = as at the hips), in m at scale 1; hands are kept outside. 0 = off.")]
    [SerializeField] private Vector3 bodyClearance;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private bool _hasSpeed;
    private int _windupHash;
    private float _windupAt;
    private bool _frozen;
    private float _since;

    public Animator Animator => animator;
    public string Current { get; private set; } = "";
    // A state to stand in instead of the resting one while it is set (CombatIdle during a duel):
    // one-shot actions that return to rest flow into it.
    public string Stance { get; set; }
    // A wind-up is holding its pose until Release.
    public bool WindingUp => _windupHash != 0;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) return;
        foreach (var p in animator.parameters) if (p.nameHash == SpeedHash) _hasSpeed = true;
    }

    private void Update()
    {
        if (animator == null || !animator.isActiveAndEnabled) return;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        bool settled = !animator.IsInTransition(0);
        // The wind-up plays to its loaded moment and almost stops there (a held breath, not a
        // freeze), until the blow is released.
        if (_windupHash != 0 && !_frozen && settled && info.shortNameHash == _windupHash && info.normalizedTime >= _windupAt)
        { animator.speed = .03f; _frozen = true; }
        if (!string.IsNullOrEmpty(Stance) && _windupHash == 0 && settled && info.shortNameHash == Animator.StringToHash(restState) && Has(Stance))
            animator.CrossFadeInFixedTime(Stance, .3f, 0);
        Pin(info, settled);
    }

    // A final pose (lying dead, kneeling, crouched, the freed serpent lowered) stays on its last
    // frame until another state is asked for, whatever the controller's own transitions say
    // (2026-10-07 playtest: Nemequene stood up again in the refuge after lying a while).
    private void Pin(AnimatorStateInfo info, bool settled)
    {
        if (_windupHash != 0 || System.Array.IndexOf(FinalPoses, Current) < 0 || Time.time - _since < .5f) return;
        int hash = Animator.StringToHash(Current);
        if (!settled)
        {
            // Still fading into the pose: let it come. Fading out of it on its own: back to it.
            if (animator.GetNextAnimatorStateInfo(0).shortNameHash == hash) return;
            animator.Play(hash, 0, .99f); animator.speed = 0; return;
        }
        if (info.shortNameHash != hash) { animator.Play(hash, 0, .99f); animator.speed = 0; return; }
        if (info.normalizedTime >= .97f) animator.speed = 0;
    }

    // Clips made for a slim body put the hands of a wider one inside it (2026-10-07 playtest: Bachué's
    // hands went through her hips and belly). After the clip has posed the body, a hand that falls
    // inside the body (an ellipse around the line from the hips to the chest, from the thighs to the
    // chest) is carried out to its edge by turning the upper arm at the shoulder.
    private Transform _hips, _chest, _leftArm, _leftHand, _rightArm, _rightHand;
    private void LateUpdate()
    {
        if (bodyClearance.x <= 0 || animator == null || !animator.isActiveAndEnabled || !animator.isHuman) return;
        if (_hips == null)
        {
            _hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            _chest = animator.GetBoneTransform(HumanBodyBones.Chest) ?? animator.GetBoneTransform(HumanBodyBones.Spine);
            _leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm); _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm); _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (_hips == null || _chest == null) { bodyClearance = Vector3.zero; return; }
        }
        Clear(_leftArm, _leftHand);
        Clear(_rightArm, _rightHand);
    }

    private void Clear(Transform arm, Transform hand)
    {
        if (arm == null || hand == null) return;
        float s = transform.lossyScale.x;
        Vector3 up = (_chest.position - _hips.position);
        float span = up.magnitude;
        // Someone lying or bent double is left as the clip has them.
        if (span < .05f * s || Vector3.Dot(up / span, transform.up) < .7f) return;
        up /= span;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, up).normalized, forward = Vector3.Cross(right, up);
        Vector3 h = hand.position;
        float y = Vector3.Dot(h - _hips.position, up);
        // From the thighs to the chest, fading at both ends.
        float band = Mathf.Clamp01((y + .62f * s) / (.12f * s)) * Mathf.Clamp01((span - y) / (.1f * s));
        if (band <= 0) return;
        Vector3 center = _hips.position + up * y;
        float x = Vector3.Dot(h - center, right), z = Vector3.Dot(h - center, forward);
        // Wider below the hips when the body flares there (Saguanmachica's robe).
        float thighs = bodyClearance.z > 0 ? bodyClearance.z : bodyClearance.x;
        float a = Mathf.Lerp(bodyClearance.x, thighs, Mathf.Clamp01(-y / (.3f * s))) * s, b = bodyClearance.y * s;
        float e = x * x / (a * a) + z * z / (b * b);
        if (e >= 1f) return;
        if (e < 1e-4f) { x = (arm == _leftArm ? -1 : 1) * a * .1f; e = x * x / (a * a); }
        float k = 1f / Mathf.Sqrt(e);
        Vector3 target = center + right * (x * k) + forward * (z * k);
        var turn = Quaternion.FromToRotation(h - arm.position, target - arm.position);
        arm.rotation = Quaternion.Slerp(Quaternion.identity, turn, band) * arm.rotation;
    }

    // The nearest acting component of a character: on it, under it, or above it.
    public static CharacterActions Of(Component c)
    {
        if (c == null) return null;
        var a = c.GetComponentInChildren<CharacterActions>();
        return a != null ? a : c.GetComponentInParent<CharacterActions>();
    }

    public bool Talking => Current == "Talk" || Current == "Talk2";
    // Held poses (kneeling, lying, sitting, meditating, crouched) that a line must not break.
    public bool Posed => System.Array.IndexOf(HeldPoses, Current) >= 0;
    private static readonly string[] HeldPoses = { "Kneel", "Death", "DeathBack", "Sit", "Pray", "Crouch", "Liberada", "Reposo" };
    // The held poses that do not loop: they end on a frame and stay there.
    private static readonly string[] FinalPoses = { "Kneel", "Death", "DeathBack", "Crouch", "Liberada" };

    public bool Has(string state) => animator != null && animator.isActiveAndEnabled && !string.IsNullOrEmpty(state)
        && animator.HasState(0, Animator.StringToHash(state));

    public bool Play(string state, float fade = .22f)
    {
        if (!Has(state)) return false;
        ClearWindup();
        animator.CrossFadeInFixedTime(state, fade, 0);
        Current = state; _since = Time.time;
        return true;
    }

    // First state of the list this character has.
    public bool PlayAny(params string[] states)
    {
        foreach (var s in states) if (Play(s)) return true;
        return false;
    }

    // The announced blow: the first state the character has plays up to `at` (normalized) and
    // holds there; Release lets it land.
    public bool Windup(float at, params string[] states)
    {
        foreach (var s in states)
        {
            if (!Play(s, .25f)) continue;
            _windupHash = Animator.StringToHash(s); _windupAt = at; _frozen = false;
            return true;
        }
        return false;
    }

    public bool Release()
    {
        if (_windupHash == 0) return false;
        ClearWindup();
        return true;
    }

    private void ClearWindup()
    {
        _windupHash = 0; _frozen = false;
        if (animator != null) animator.speed = 1;
    }

    // Straight into the last frame of a pose (someone already lying or kneeling when the shot opens).
    public bool Hold(string state)
    {
        if (!Has(state)) return false;
        ClearWindup();
        animator.Play(state, 0, .99f);
        Current = state;
        if (System.Array.IndexOf(FinalPoses, state) >= 0) animator.speed = 0;
        return true;
    }

    public void Rest(float fade = .25f)
    {
        if (!string.IsNullOrEmpty(Stance) && Play(Stance, fade)) { Current = ""; return; }
        if (Play(restState, fade)) Current = "";
    }

    // Walking speed for the locomotion blend of NPCs moved by code (m/s).
    public void SetSpeed(float metersPerSecond)
    {
        if (_hasSpeed) animator.SetFloat(SpeedHash, metersPerSecond, .1f, Time.deltaTime);
    }
}

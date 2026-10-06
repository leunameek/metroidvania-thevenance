using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// Controlled flight of the wings (guide 4.4 and 6.3). F opens them (from the ground: a short
// 0.8 m take-off inside the budget), WASD flies at 8 m/s relative to the camera, Space / Ctrl rise
// or descend at 2.5 m/s. One budget of 4 s per flight: closing keeps what is left, reopening uses
// it, and it only refills after 0.6 s on safe ground (the director calls Recharge). At 25 % a soft
// warning plays; at 0 the wings close and the normal fall continues.
public sealed class MSWings : MonoBehaviour, IPlayerMotor, IPlayerMotorPose
{
    [SerializeField] private float horizontalSpeed = 8f, verticalSpeed = 2.5f, budget = 4f, takeoffRise = .8f;
    [SerializeField] private Transform movementReference;
    [SerializeField] private GameObject visual;

    private PlayerController _player;
    private float _remaining, _takeoffLeft, _airborne;
    private bool _flying, _warned;
    private Vector3 _velocity;
    private Vector3 _wingScale = Vector3.one;
    private Quaternion _wingRotation = Quaternion.identity;
    private float _open, _lift;
    private MSWingRig _rig;
    private AudioSource _wind;
    private int _beats;
    // Where the wings sit on the back, from the chest bone in the player's frame (metres).
    [SerializeField] private Vector3 backOffset = new Vector3(0f, -.15f, -.24f);

    public bool Flying => _flying;
    public PlayerMotorPose Pose => PlayerMotorPose.Fly;
    public bool Owned => MSProgress.Has(MSProgress.Wings);
    public float Remaining01 => budget > 0 ? Mathf.Clamp01(_remaining / budget) : 0;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _remaining = budget;
    }

    // The equipped wings ride on the chest bone, so they follow the flight pose of the body.
    private void Start()
    {
        if (visual == null) return;
        var animator = GetComponentInChildren<Animator>();
        Transform chest = null;
        if (animator != null && animator.isHuman)
            chest = animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? animator.GetBoneTransform(HumanBodyBones.Chest);
        if (chest != null)
        {
            // Centred between the shoulder blades and upright with the player, then carried by the chest.
            visual.transform.SetPositionAndRotation(chest.position + transform.rotation * backOffset, transform.rotation);
            visual.transform.SetParent(chest, true);
        }
        _wingScale = visual.transform.localScale;
        _wingRotation = visual.transform.localRotation;
        _rig = visual.GetComponentInChildren<MSWingRig>(true);
        if (_rig != null) _rig.SetBack(transform);
        // S09: the flight wind rises with the wings open and stops on pause or teleport.
        _wind = MSAudio.Loop(gameObject, "vuelo_viento", 0f, false, MSAudio.Channel.Effects);
    }

    private void Update()
    {
        if (visual != null && visual.activeSelf != Owned) visual.SetActive(Owned);
        if (_wind != null) MSAudio.SetLoopVolume(_wind, _flying ? .35f + .25f * Mathf.Clamp01(_velocity.magnitude / horizontalSpeed) : 0f);
        if (visual != null && Owned)
        {
            // Folded on the back while walking; open in 0.25 s / close in 0.2 s; a 0.9 s flap in flight.
            _open = Mathf.MoveTowards(_open, _flying ? 1f : 0f, Time.deltaTime / (_flying ? .25f : .2f));
            if (_rig != null)
            {
                _rig.Drive(_open, _flying ? 1f : 0f, _lift);
                // One beat sound per down-stroke of the rig, pitched with the beat rate.
                if (_flying && _rig.Beats != _beats) MSAudio.Play("aleteo", .55f, .9f + .15f * _rig.Rate);
                _beats = _rig.Beats;
            }
            else
            {
            float flap = Mathf.Sin(Time.time * Mathf.PI * 2f / .9f) * _open;
            float span = Mathf.Lerp(.35f, 1f, _open) * (1f + .1f * flap);
            visual.transform.localScale = Vector3.Scale(_wingScale, new Vector3(span, 1f, 1f));
            visual.transform.localRotation = _wingRotation * Quaternion.Euler(flap * 10f, 0, 0);
            }
        }
        if (!Owned || _player == null) return;
        var keyboard = Keyboard.current;
        var director = MundoSuperiorDirector.Instance;
        if (keyboard == null || !keyboard.fKey.wasPressedThisFrame) return;
        if (_player.InputLocked || (director != null && (director.Busy || director.InCombat))) return;
        if (_flying) Close(false);
        else if (!_player.HasMotor && _remaining > .05f) Open();
    }

    private void Open()
    {
        var controller = GetComponent<CharacterController>();
        _takeoffLeft = controller != null && controller.isGrounded ? takeoffRise : 0f;
        _velocity = Vector3.zero; _airborne = 0f; _warned = Remaining01 <= .25f;
        _flying = true;
        _player.SetMotor(this);
        MSAudio.Play("alas_abrir", .8f);
        MundoSuperiorDirector.Instance?.OnFlightChanged(true);
    }

    // Exhausted, landed, F again, or interrupted: back to normal locomotion, budget kept.
    public void Close(bool exhausted)
    {
        if (!_flying) return;
        _flying = false;
        MSAudio.Play("alas_cerrar", .7f);
        _player.ClearMotor(this, exhausted ? -1f : 0f);
        MundoSuperiorDirector.Instance?.OnFlightChanged(false);
    }

    public void Cancel() => Close(false);

    public void Recharge()
    {
        if (_flying || _remaining >= budget) return;
        _remaining = budget; _warned = false;
    }

    public void Tick(CharacterController controller, float dt)
    {
        if (dt <= 0f) return;
        var keyboard = Keyboard.current;
        _remaining -= dt;
        _airborne += dt;
        float x = 0, z = 0, y = 0;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) x -= 1; if (keyboard.dKey.isPressed) x += 1;
            if (keyboard.wKey.isPressed) z += 1; if (keyboard.sKey.isPressed) z -= 1;
            if (keyboard.spaceKey.isPressed) y += 1;
            if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) y -= 1;
        }
        Vector3 forward = movementReference != null ? Vector3.ProjectOnPlane(movementReference.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 move = right * x + forward * z;
        if (move.sqrMagnitude > 1f) move.Normalize(); // diagonals never fly faster
        float vertical = y * verticalSpeed;
        _lift = y;
        if (_takeoffLeft > 0f)
        {
            float rise = Mathf.Min(_takeoffLeft, verticalSpeed * 1.6f * dt);
            _takeoffLeft -= rise; vertical = rise / dt;
        }
        // Short easing so the wings read as gliding, not as an instant velocity change.
        _velocity = Vector3.MoveTowards(_velocity, move * horizontalSpeed + Vector3.up * vertical, 30f * dt);
        controller.Move(_velocity * dt);
        if (move.sqrMagnitude > .0001f)
            controller.transform.rotation = Quaternion.RotateTowards(controller.transform.rotation, Quaternion.LookRotation(move), 540f * dt);

        if (!_warned && Remaining01 <= .25f)
        {
            _warned = true;
            MSAudio.Play("alas_aviso", .8f, 1f, MSAudio.Channel.Interface);
            MundoSuperiorDirector.Instance?.Hud?.Notify("Alas", "Queda poco vuelo: busca dónde aterrizar.", UIIcon.Info, UIPalette.Danger);
        }
        if (_remaining <= 0f) { _remaining = 0f; Close(true); return; }
        // Landing folds the wings only with floor under the body: brushing the side of an island
        // or a rail also reports isGrounded, and must not drop the player.
        if (_takeoffLeft <= 0f && _airborne > .3f && controller.isGrounded && vertical <= 0f
            && Physics.Raycast(controller.transform.position, Vector3.down, controller.height * .5f + .3f, ~0, QueryTriggerInteraction.Ignore))
            Close(false);
    }
}

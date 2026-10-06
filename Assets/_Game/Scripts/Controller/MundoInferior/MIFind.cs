using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// A find on its altar (guide 3.2): E opens the inspection, the piece turns with the mouse or
// A/D, E confirms once (reward granted in a single operation through MIProgress), Escape gives
// it back untouched. The pickup is an independent child of the altar; collected pieces vanish
// and the altar light dims. Offerings are optional pieces for the cultural archive.
public sealed class MIFind : MIInteractable
{
    public enum Kind { Seed, Bracelets, Horn, Offering }

    [SerializeField] private string findId;
    [SerializeField] private Kind kind;
    [SerializeField] private Transform item;
    [SerializeField] private Light halo;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField, TextArea] private string description = "";
    [SerializeField] private string rewardTitle = "";
    [SerializeField, TextArea] private string rewardText = "";

    public static MIFind Inspecting { get; private set; }
    public string FindId => findId;
    public Kind FindKind => kind;
    public bool Collected => MIProgress.Has(findId);
    public override bool Available => base.Available && !Collected && Inspecting == null;
    public override string Prompt => "Examinar " + displayName.ToLowerInvariant();

    private Vector3 _itemPosition, _cameraPosition;
    private Quaternion _itemRotation, _cameraRotation;
    private Camera _camera;
    private ExplorationOrbitCamera _orbit;
    private PlayerController _player;
    private float _blend, _haloIntensity;
    private bool _confirmFrame;

    private void Start()
    {
        if (halo != null) _haloIntensity = halo.intensity;
        Refresh();
    }

    private void Refresh()
    {
        bool collected = Collected;
        if (item != null) item.gameObject.SetActive(!collected);
        float intensity = collected ? _haloIntensity * .18f : _haloIntensity;
        if (halo != null) { halo.intensity = intensity; var glow = halo.GetComponent<MIGlow>(); if (glow != null) glow.SetBase(intensity); }
        if (sparks != null) sparks.gameObject.SetActive(!collected);
    }

    public override void Interact(PlayerController player)
    {
        if (!Available || item == null) return;
        _camera = Camera.main;
        if (_camera == null) return;
        Inspecting = this; _player = player; _blend = 0; _confirmFrame = true;
        _orbit = _camera.GetComponent<ExplorationOrbitCamera>();
        if (_orbit != null) _orbit.enabled = false;
        _cameraPosition = _camera.transform.position; _cameraRotation = _camera.transform.rotation;
        _itemPosition = item.position; _itemRotation = item.rotation;
        var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = false;
        player.SetInputLocked(true);
        MIAudio.Play("hallazgo_abrir", .8f);
        string kindLabel = kind == Kind.Offering ? "Ofrenda opcional · archivo" : "Hallazgo";
        MundoInferiorBlockout.Instance?.Hud?.ShowInspection(displayName, kindLabel, description);
    }

    private void Update()
    {
        if (Inspecting != this) return;
        _blend = Mathf.MoveTowards(_blend, 1, Time.unscaledDeltaTime / .35f);
        // Frame the piece from the player's side, a little above, the panel on the right.
        Vector3 toCamera = _cameraPosition - _itemPosition; toCamera.y = 0;
        if (toCamera.sqrMagnitude < .01f) toCamera = -transform.forward;
        Vector3 side = Vector3.Cross(Vector3.up, toCamera.normalized);
        Vector3 framed = _itemPosition + toCamera.normalized * 1.7f + Vector3.up * .25f - side * .55f;
        // Camera right is -side: shifting both eye and target keeps the piece left of the panel.
        Quaternion look = Quaternion.LookRotation(_itemPosition - side * .55f - framed);
        float t = Mathf.SmoothStep(0, 1, _blend);
        _camera.transform.SetPositionAndRotation(Vector3.Lerp(_cameraPosition, framed, t), Quaternion.Slerp(_cameraRotation, look, t));

        var mouse = Mouse.current; var keyboard = Keyboard.current;
        Vector2 turn = Vector2.zero;
        if (mouse != null && mouse.leftButton.isPressed) turn += mouse.delta.ReadValue() * .35f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) turn.x -= 140 * Time.unscaledDeltaTime;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turn.x += 140 * Time.unscaledDeltaTime;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) turn.y += 100 * Time.unscaledDeltaTime;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) turn.y -= 100 * Time.unscaledDeltaTime;
        }
        item.Rotate(Vector3.up, -turn.x, Space.World);
        item.Rotate(_camera.transform.right, turn.y, Space.World);

        if (_confirmFrame) { _confirmFrame = false; return; } // the E that opened it does not confirm
        if (keyboard == null) return;
        if (keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame) Confirm();
        else if (keyboard.escapeKey.wasPressedThisFrame) Close();
    }

    private void Confirm()
    {
        bool first = MIProgress.Set(findId);
        Close();
        Refresh();
        if (!first) return;
        var director = MundoInferiorBlockout.Instance;
        director?.ApplyAbilities();
        MIAudio.Play(kind == Kind.Offering ? "ofrenda" : "hallazgo_confirmar", .9f);
        MIBurst.Spawn(_itemPosition, kind == Kind.Offering ? new Color(.55f, .85f, 1f) : new Color(1f, .78f, .4f));
        if (director != null && director.Hud != null)
        {
            if (kind == Kind.Offering)
                director.Hud.Notify("Ofrenda · " + MIProgress.OfferingsCount + " / " + MIProgress.OfferingTotal, displayName + " pasa al archivo cultural.", UIIcon.Journal, UIPalette.Jade);
            else
                director.Hud.Notify(rewardTitle, rewardText, kind == Kind.Horn ? UIIcon.Objective : kind == Kind.Seed ? UIIcon.Bird : UIIcon.Dodge, UIPalette.GoldLight);
            director.RefreshObjective();
        }
    }

    private void Close()
    {
        if (Inspecting != this) return;
        Inspecting = null;
        if (item != null)
        {
            item.SetPositionAndRotation(_itemPosition, _itemRotation);
            var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = true;
        }
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_orbit != null) { _orbit.enabled = true; _orbit.SnapAfterTeleport(); }
        if (_player != null) _player.SetInputLocked(false);
        MundoInferiorBlockout.Instance?.Hud?.HideInspection();
        MundoInferiorBlockout.Instance?.SuppressEscapeThisFrame();
    }

    protected override void OnDisable() { base.OnDisable(); Close(); }
}

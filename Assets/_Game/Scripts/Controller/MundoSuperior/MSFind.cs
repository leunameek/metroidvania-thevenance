using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// A find of the upper world (guide 4.3): from firm ground E opens the inspection, the piece turns
// with the mouse or WASD, a new E or Enter confirms once (flag written before the reward),
// Escape gives it back untouched. The piece is an independent child: the pedestal and support
// stay when it is collected, and a collected find never shows again after loading.
public sealed class MSFind : MIInteractable
{
    [SerializeField] private string findId;
    [SerializeField] private Transform item;
    [SerializeField] private Light halo;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private string kindLabel = "Hallazgo";
    [SerializeField, TextArea] private string description = "";
    [SerializeField] private string rewardTitle = "";
    [SerializeField, TextArea] private string rewardText = "";
    [SerializeField] private string[] requires = new string[0];

    public static MSFind Inspecting { get; private set; }
    public string FindId => findId;
    public bool Collected => MSProgress.Has(findId);
    // Every trial is required (2026-10-06 playtest), also when the scene is opened on its own: the
    // wings are offered once the condor has recognised the visitor (E09), the key plate once the
    // eagle has (guion O-S06, E10).
    public override bool Available => base.Available && !Collected && Inspecting == null && RequirementsMet && TrialMet;
    public string Trial => findId == MSProgress.Key ? MSProgress.Eagle : findId == MSProgress.Wings ? MSProgress.Condor : null;
    private bool TrialMet => Trial == null || MSProgress.Has(Trial);
    public override string Prompt => "Examinar " + displayName.ToLowerInvariant();

    private Vector3 _itemPosition, _cameraPosition;
    private Quaternion _itemRotation, _cameraRotation;
    private Camera _camera;
    private ExplorationOrbitCamera _orbit;
    private PlayerController _player;
    private float _blend, _haloIntensity;
    private bool _confirmFrame;
    private FitPuzzle _fit;
    private Renderer[] _hiddenPlayer;
    private Vector3 _frameSide = Vector3.back;

    // The runes, the wings and the key medallion are fitted in their table (FitPuzzle). The runes
    // stay there; the wings and the medallion are taken once fitted (he wears and uses them).
    public bool Fits => findId == MSProgress.RunePortals || findId == MSProgress.RuneClimb || findId == MSProgress.Wings || findId == MSProgress.Key;
    private bool Stays => _fit != null && (findId == MSProgress.RunePortals || findId == MSProgress.RuneClimb);
    private bool ItemShown => (!Collected || Stays) && RequirementsMet && TrialMet;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Inspecting = null;

    private void Start()
    {
        if (halo != null) _haloIntensity = halo.intensity;
        if (Fits && item != null) _fit = new FitPuzzle(item);
        Refresh();
    }

    // Story pieces of the campaign (Sué) are built at runtime next to an existing altar: same
    // inspection, shown and offered only once their requirements are met.
    public static MSFind CreateStory(Transform parent, Vector3 position, string id, string name, string description,
        string rewardTitle, string rewardText, Transform item, params string[] requires)
    {
        var go = new GameObject("Hallazgo_" + id);
        go.SetActive(false);
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        var find = go.AddComponent<MSFind>();
        find.findId = id; find.kindLabel = "Máscara"; find.displayName = name; find.description = description;
        find.rewardTitle = rewardTitle; find.rewardText = rewardText; find.item = item; find.requires = requires;
        find.range = 2.6f;
        item.SetParent(go.transform, true);
        go.SetActive(true);
        return find;
    }
    private bool RequirementsMet
    {
        get { foreach (var r in requires) if (!MSProgress.Has(r)) return false; return true; }
    }

    private void Refresh()
    {
        bool collected = Collected;
        if (item != null) item.gameObject.SetActive(ItemShown);
        if (collected && Stays && item != null)
        {
            var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = false;
            if (Inspecting != this) _fit.PlaceFitted();
        }
        float intensity = collected ? 0f : _haloIntensity;
        if (halo != null) { halo.intensity = intensity; var glow = halo.GetComponent<MIGlow>(); if (glow != null) glow.SetBase(intensity); }
        if (sparks != null) sparks.gameObject.SetActive(!collected);
    }

    // Near the empty altar while its trial waits: one line says who keeps the piece.
    private float _nextTrialNotice;
    private void WarnTrial()
    {
        var director = MundoSuperiorDirector.Instance;
        var p = director != null ? director.Player : null;
        if (p == null || Time.time < _nextTrialNotice || Vector3.Distance(p.position, transform.position) > range + 2f) return;
        _nextTrialNotice = Time.time + 8f;
        bool eagle = Trial == MSProgress.Eagle;
        director.Hud?.Notify(eagle ? "La llave está guardada" : "Las alas están guardadas",
            eagle ? "La mujer-águila guarda la llave: responde primero a su prueba en la terraza."
                  : "La mujer-cóndor guarda las alas: responde primero a su prueba junto a la runa.",
            UIIcon.Guardian, UIPalette.Muted);
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
        _frameSide = InspectionFraming.ClearSide(_itemPosition, _cameraPosition, 1.5f, item, player.transform);
        var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = false;
        player.SetInputLocked(true);
        WorldNaturalInput.Instance?.ConsumeHandTurn(out _, out _); // movement from before does not count
        MSAudio.Play("hallazgo_abrir", .8f);
        _hiddenPlayer = FitView.HidePlayer(player);
        _fit?.Scramble();
        var hud = MundoSuperiorDirector.Instance?.Hud;
        hud?.ShowInspection(displayName, kindLabel, description);
        hud?.SetInspectionFit(_fit != null ? FitView.Status(false) : null);
    }

    private void Update()
    {
        // A story piece appears on its altar as soon as its requirements are met.
        if ((requires.Length > 0 || Trial != null) && item != null && item.gameObject.activeSelf != ItemShown) Refresh();
        if (!Collected && !TrialMet) WarnTrial();
        if (Inspecting != this) return;
        _blend = Mathf.MoveTowards(_blend, 1, Time.unscaledDeltaTime / .35f);
        // Frame the piece from the player's side, a little above, the panel on the right.
        Vector3 toCamera = _frameSide;
        if (toCamera.sqrMagnitude < .01f) toCamera = -transform.forward;
        Vector3 side = Vector3.Cross(Vector3.up, toCamera.normalized);
        Vector3 framed = _itemPosition + toCamera.normalized * 1.5f + Vector3.up * .2f - side * .5f;
        Quaternion look = Quaternion.LookRotation(_itemPosition - side * .5f - framed);
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
        // Hands as in the first prototype: the open left hand turns the piece, the open right hand
        // tilts it, two fists hold it still and a held fist takes it; words too.
        var natural = WorldNaturalInput.Instance;
        float handYaw = 0, handPitch = 0;
        if (natural != null)
        {
            natural.SetContext(NaturalContext.Inspect);
            natural.ConsumeHandTurn(out handYaw, out handPitch);
            MundoSuperiorDirector.Instance?.Hud?.SetInspectionGuide(natural.InspectionGuide);
        }
        item.Rotate(Vector3.up, -turn.x + handYaw, Space.World);
        item.Rotate(_camera.transform.right, turn.y + handPitch, Space.World);
        if (_fit != null)
        {
            bool was = _fit.Seated;
            _fit.Tick(halo, _haloIntensity, item.position);
            if (was != _fit.Seated) MundoSuperiorDirector.Instance?.Hud?.SetInspectionFit(FitView.Status(_fit.Seated));
        }
        // The piece turning in the hands: a soft friction, rate-limited by the mixer.
        if (Mathf.Abs(turn.x - handYaw) + Mathf.Abs(turn.y + handPitch) > .35f) GameAudio.Play("Foley/objeto_girar", .3f, AudioChannel.Effects, 1f, .06f, .2f, 1);

        if (_confirmFrame) { _confirmFrame = false; return; } // the E that opened it does not confirm
        if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) Confirm();
        else if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) { GameAudio.Play("Foley/examinar_cerrar", .7f); Close(); }
        else if (natural != null && natural.ConsumeConfirm((_fit != null ? "Encajar " : "Tomar ") + displayName.ToLowerInvariant())) Confirm();
        else if (natural != null && natural.ConsumeBack("Devolver al altar")) Close();
    }

    // The piece goes back to its altar, Nemequene takes it with his hands, and it is his at the
    // moment of the grasp.
    private void Confirm()
    {
        bool first = !MSProgress.Has(findId);
        if (_fit != null && first)
        {
            // Only a seated piece goes into its table.
            if (!_fit.Seated) { FitView.NotYet(MundoSuperiorDirector.Instance?.Hud); return; }
            Close(keepItem: true);
            StartCoroutine(_fit.Settle());
            if (_player != null) PlayerInteraction.Perform(_player, item, Take, Stays ? "Reach" : "Pickup", "Reach");
            else Take();
            return;
        }
        Close();
        if (!first) { Refresh(); return; }
        if (_player != null) PlayerInteraction.Perform(_player, item != null ? item : transform, Take, "Pickup");
        else Take();
    }

    private void Take()
    {
        if (!MSProgress.Set(findId)) { Refresh(); return; }
        Refresh();
        MSAudio.Play("hallazgo_confirmar", .9f);
        MIBurst.Spawn(_itemPosition, new Color(1f, .82f, .45f));
        MundoSuperiorDirector.Instance?.OnFound(this, rewardTitle, rewardText);
    }

    private void Close() => Close(false);

    // keepItem: the piece is being fitted (it stays where the player seated it, then settles).
    private void Close(bool keepItem)
    {
        if (Inspecting != this) return;
        Inspecting = null;
        FitView.ShowPlayer(_hiddenPlayer); _hiddenPlayer = null;
        if (item != null && !keepItem)
        {
            item.SetPositionAndRotation(_itemPosition, _itemRotation);
            var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = !(Collected && Stays);
            if (halo != null) halo.intensity = _haloIntensity;
        }
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_orbit != null) { _orbit.enabled = true; _orbit.SnapAfterTeleport(); }
        if (_player != null) _player.SetInputLocked(false);
        MundoSuperiorDirector.Instance?.Hud?.HideInspection();
        MundoSuperiorDirector.Instance?.SuppressEscapeThisFrame();
    }

    protected override void OnDisable() { base.OnDisable(); Close(); }
}

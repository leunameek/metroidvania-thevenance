using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// A find on its altar (guide 3.2): E opens the inspection, the piece turns with the mouse or
// A/D, E confirms once (reward granted in a single operation through MIProgress), Escape gives
// it back untouched. The pickup is an independent child of the altar; collected pieces vanish
// and the altar light dims. Offerings are optional pieces for the cultural archive.
// Bracelets and offerings (rings, discs, coins) are fitted in their table (FitPuzzle): they come
// out turned and count only once seated and fitted. Offerings then stay in their table; the
// bracelets, once fitted, are taken by Nemequene (he wears them).
public sealed class MIFind : MIInteractable
{
    public enum Kind { Seed, Bracelets, Horn, Offering, Story }

    [SerializeField] private string findId;
    [SerializeField] private Kind kind;
    [SerializeField] private Transform item;
    [SerializeField] private Light halo;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField, TextArea] private string description = "";
    [SerializeField] private string rewardTitle = "";
    [SerializeField, TextArea] private string rewardText = "";
    [SerializeField] private string[] requires = new string[0];

    public static MIFind Inspecting { get; private set; }
    public string FindId => findId;
    public Kind FindKind => kind;
    public bool Collected => MIProgress.Has(findId);
    public override bool Available => base.Available && !Collected && Inspecting == null && RequirementsMet;
    public override string Prompt => "Examinar " + displayName.ToLowerInvariant();

    private Vector3 _itemPosition, _cameraPosition;
    private Quaternion _itemRotation, _cameraRotation;
    private Camera _camera;
    private ExplorationOrbitCamera _orbit;
    private PlayerController _player;
    private float _blend, _haloIntensity;
    private bool _confirmFrame;
    private FitPuzzle _fit;
    private bool _seedSized;
    private Renderer[] _hiddenPlayer;
    private Vector3 _frameSide = Vector3.back;

    public bool Fits => kind == Kind.Bracelets || kind == Kind.Offering;
    private bool Stays => _fit != null && kind == Kind.Offering;
    // The inspection looks at the piece, or at the piece held over its table and the table.
    private Vector3 Focus => _fit != null ? _fit.FramePoint : _itemPosition;
    private float Distance => _fit != null ? 2.1f : 1.7f;

    private void Start()
    {
        if (halo != null) _haloIntensity = halo.intensity;
        // The seed of the sanctuary was larger than its altar (2026-10-07 playtest): half its size,
        // resting closer to the stone.
        if (kind == Kind.Seed && item != null && !_seedSized)
        {
            _seedSized = true;
            var floating = item.GetComponent<MIFloat>();
            bool floats = floating != null && floating.enabled;
            if (floats) floating.enabled = false; // back to its rest, so the float takes the new one
            item.localScale *= .5f;
            item.position -= Vector3.up * .35f;
            if (floats) floating.enabled = true;
        }
        bool cradled = kind == Kind.Bracelets && item != null && CradleBracelets();
        if (!cradled && Fits && item != null) _fit = new FitPuzzle(item);
        // It glows while it waits to be examined; the main finds also raise a column of light.
        if (item != null) Beacon.Attach(gameObject, item, () => Available, beam: kind != Kind.Offering);
        Refresh();
    }

    // The pair of bracelets stands each in one saddle of its stone support, and that is the pose the
    // sketch shows and the fitting ends in (2026-10-07 playtest: the sketch lay flat on the ledge in
    // front, nowhere near the saddles). The support has no collider, so its saddles are found from
    // its bounds: measured on the O04b model, they sit at 31 % and 69 % of its length, their bottom
    // at 52 % of its height, centred in its depth. Shown, the pair floats above them as before.
    private bool CradleBracelets()
    {
        Transform support = null;
        foreach (Transform child in transform) if (child.name.EndsWith("Soporte")) support = child;
        var rings = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in item) if (child.GetComponentInChildren<MeshRenderer>() != null) rings.Add(child);
        if (support == null || rings.Count != 2) return false;
        // The support's bounds in the altar's frame.
        bool any = false; Bounds local = default;
        foreach (var r in support.GetComponentsInChildren<MeshRenderer>())
        {
            var b = r.localBounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var p = transform.InverseTransformPoint(r.transform.TransformPoint(corner));
                if (!any) { local = new Bounds(p, Vector3.zero); any = true; } else local.Encapsulate(p);
            }
        }
        if (!any) return false;
        bool alongX = local.size.x >= local.size.z;
        Vector3 row = alongX ? Vector3.right : Vector3.forward, groove = alongX ? Vector3.forward : Vector3.right;
        float length = alongX ? local.size.x : local.size.z;
        Vector3 bottom = new Vector3(local.center.x, local.min.y + local.size.y * .52f, local.center.z);
        Vector3[] saddles = { bottom - row * length * .19f, bottom + row * length * .19f };
        var floating = item.GetComponent<MIFloat>();
        bool floats = floating != null && floating.enabled;
        if (floats) floating.enabled = false;
        rings.Sort((a, b) => Vector3.Dot(transform.InverseTransformPoint(a.position), row).CompareTo(Vector3.Dot(transform.InverseTransformPoint(b.position), row)));
        Vector3 grooveWorld = transform.TransformDirection(groove);
        for (int i = 0; i < 2; i++)
        {
            var ring = rings[i];
            var mesh = ring.GetComponentInChildren<MeshRenderer>();
            Vector3 e = mesh.localBounds.extents;
            Vector3 thin = e.x <= e.y && e.x <= e.z ? Vector3.right : e.y <= e.z ? Vector3.up : Vector3.forward;
            // Standing in its saddle: the ring's axis along the groove.
            Vector3 axis = mesh.transform.TransformDirection(thin);
            if (Vector3.Dot(axis, grooveWorld) < 0) axis = -axis;
            ring.rotation = Quaternion.FromToRotation(axis, grooveWorld) * ring.rotation;
            var b = mesh.bounds;
            Vector3 seat = transform.TransformPoint(saddles[i]) + Vector3.up * b.extents.y * .92f;
            ring.position += seat - b.center;
        }
        _fit = new FitPuzzle(item, item.position);
        // Shown floating above its saddles, as before.
        item.position += Vector3.up * .32f;
        if (floats) floating.enabled = true;
        return true;
    }

    // Story pieces of the campaign (coca, Chía) are built at runtime next to an existing altar:
    // same inspection, shown and offered only once their requirements are met.
    public static MIFind CreateStory(Transform parent, Vector3 position, string id, string name, string description,
        string rewardTitle, string rewardText, Transform item, params string[] requires)
    {
        var go = new GameObject("Hallazgo_" + id);
        go.SetActive(false);
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        var find = go.AddComponent<MIFind>();
        find.findId = id; find.kind = Kind.Story; find.displayName = name; find.description = description;
        find.rewardTitle = rewardTitle; find.rewardText = rewardText; find.item = item; find.requires = requires;
        find.range = 2.4f;
        item.SetParent(go.transform, true);
        go.SetActive(true);
        return find;
    }
    private bool RequirementsMet
    {
        get { foreach (var r in requires) if (!MIProgress.Has(r)) return false; return true; }
    }

    private void Refresh()
    {
        bool collected = Collected;
        if (item != null) item.gameObject.SetActive((!collected || Stays) && RequirementsMet);
        // A fitted offering rests in its table, still.
        if (collected && Stays && item != null)
        {
            var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = false;
            if (Inspecting != this) _fit.PlaceFitted();
        }
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
        _frameSide = InspectionFraming.ClearSide(Focus, _cameraPosition, Distance, item, player.transform);
        var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = false;
        player.SetInputLocked(true);
        WorldNaturalInput.Instance?.ConsumeHandTurn(out _, out _); // movement from before does not count
        MIAudio.Play("hallazgo_abrir", .8f);
        string kindLabel = kind == Kind.Offering ? "Ofrenda opcional · archivo" : "Hallazgo";
        // Nemequene steps out of the shot: the piece and the panel have the screen.
        _hiddenPlayer = FitView.HidePlayer(player);
        _fit?.Scramble();
        var hud = MundoInferiorBlockout.Instance?.Hud;
        hud?.ShowInspection(displayName, kindLabel, description);
        hud?.SetInspectionFit(_fit != null ? FitView.Status(false) : null);
    }

    private void Update()
    {
        // A story piece appears on its altar as soon as its requirements are met.
        if (requires.Length > 0 && item != null && item.gameObject.activeSelf != (!Collected && RequirementsMet)) Refresh();
        if (Inspecting != this) return;
        _blend = Mathf.MoveTowards(_blend, 1, Time.unscaledDeltaTime / .35f);
        // Frame the piece from the player's side, a little above, the panel on the right.
        Vector3 toCamera = _frameSide;
        if (toCamera.sqrMagnitude < .01f) toCamera = -transform.forward;
        Vector3 side = Vector3.Cross(Vector3.up, toCamera.normalized);
        Vector3 framed = Focus + toCamera.normalized * Distance + Vector3.up * (_fit != null ? .6f : .25f) - side * .55f;
        // Camera right is -side: shifting both eye and target keeps the piece left of the panel.
        Quaternion look = Quaternion.LookRotation(Focus - side * .55f - framed);
        float t = Mathf.SmoothStep(0, 1, _blend);
        // A piece to fit is held just above its table, where its sketch is drawn.
        if (_fit != null) item.position = Vector3.Lerp(_itemPosition, _fit.Hover, t);
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
            MundoInferiorBlockout.Instance?.Hud?.SetInspectionGuide(natural.InspectionGuide);
        }
        item.Rotate(Vector3.up, -turn.x + handYaw, Space.World);
        item.Rotate(_camera.transform.right, turn.y + handPitch, Space.World);
        if (_fit != null)
        {
            bool was = _fit.Seated;
            _fit.Tick(halo, _haloIntensity, item.position);
            if (was != _fit.Seated) MundoInferiorBlockout.Instance?.Hud?.SetInspectionFit(FitView.Status(_fit.Seated));
        }
        // The piece turning in the hands: a soft friction, rate-limited by the mixer.
        if (Mathf.Abs(turn.x - handYaw) + Mathf.Abs(turn.y + handPitch) > .35f) GameAudio.Play("Foley/objeto_girar", .3f, AudioChannel.Effects, 1f, .06f, .2f, 1);

        if (_confirmFrame) { _confirmFrame = false; return; } // the E that opened it does not confirm
        if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) Confirm();
        else if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) { GameAudio.Play("Foley/examinar_cerrar", .7f); Close(); }
        else if (natural != null && natural.ConsumeConfirm((_fit != null ? "Encajar " : "Tomar ") + displayName.ToLowerInvariant())) Confirm();
        else if (natural != null && natural.ConsumeBack("Devolver al altar")) Close();
    }

    // The piece goes back to its altar, Nemequene takes it with his hands, and it is his (flag,
    // ability, reward) at the moment of the grasp.
    private void Confirm()
    {
        bool first = !MIProgress.Has(findId);
        if (_fit != null && first)
        {
            // Only a seated piece goes into its table.
            if (!_fit.Seated) { FitView.NotYet(MundoInferiorBlockout.Instance?.Hud); return; }
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
        if (!MIProgress.Set(findId)) { Refresh(); return; }
        Refresh();
        var director = MundoInferiorBlockout.Instance;
        director?.ApplyAbilities();
        MIAudio.Play(kind == Kind.Offering ? "ofrenda" : "hallazgo_confirmar", .9f);
        MIBurst.Spawn(_itemPosition, kind == Kind.Offering ? new Color(.55f, .85f, 1f) : new Color(1f, .78f, .4f));
        if (director != null && director.Hud != null)
        {
            if (kind == Kind.Offering)
                director.Hud.Notify("Ofrenda · " + MIProgress.OfferingsCount + " / " + MIProgress.OfferingTotal, displayName + " pasa al archivo cultural.", UIIcon.Journal, UIPalette.Jade);
            else
                director.Hud.Notify(rewardTitle, rewardText, kind == Kind.Horn || kind == Kind.Story ? UIIcon.Objective : kind == Kind.Seed ? UIIcon.Bird : UIIcon.Dodge, UIPalette.GoldLight);
            director.RefreshObjective();
        }
    }

    private void Close() => Close(false);

    // keepItem: the piece is being fitted (it stays where the player seated it, then settles).
    private void Close(bool keepItem)
    {
        if (Inspecting != this) return;
        Inspecting = null;
        _fit?.HideGuide();
        FitView.ShowPlayer(_hiddenPlayer); _hiddenPlayer = null;
        if (item != null && !keepItem)
        {
            item.SetPositionAndRotation(_itemPosition, _itemRotation);
            var floating = item.GetComponent<MIFloat>(); if (floating != null) floating.enabled = !(Collected && Stays);
            if (halo != null) halo.intensity = Collected ? _haloIntensity * .18f : _haloIntensity;
        }
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_orbit != null) { _orbit.enabled = true; _orbit.SnapAfterTeleport(); }
        if (_player != null) _player.SetInputLocked(false);
        MundoInferiorBlockout.Instance?.Hud?.HideInspection();
        MundoInferiorBlockout.Instance?.SuppressEscapeThisFrame();
    }

    protected override void OnDisable() { base.OnDisable(); Close(); }
}

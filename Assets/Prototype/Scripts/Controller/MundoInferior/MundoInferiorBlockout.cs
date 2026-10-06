using System;
using System.Collections;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Director of the Mundo Inferior: loads the persistent state (MIProgress) and derives the player's
// abilities from it, runs the room camera, the E interactions, damage and recovery, defeat and
// checkpoints, the pause and the level HUD, objectives and ambient sound. The class keeps its
// blockout name so the generated scene and the travel code keep their references.
[DefaultExecutionOrder(-100)]
public sealed class MundoInferiorBlockout : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ExplorationOrbitCamera orbitCamera;
    [SerializeField] private Transform[] roomSpawns = new Transform[9];
    [SerializeField] private string[] roomNames = new string[9];
    // Below every pit floor (rooms 07-09 at -14, spikes 3 m lower) with margin.
    [SerializeField] private float fallbackLimitY = -30f;
    [SerializeField] private bool showHelp;

    private const string TitleScene = "Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity";
    private int _room;
    private MIGate[] _gates;
    private MIRest[] _rests;
    private MIGuardian _guardian;
    private MIShieldSentinel _sentinel;
    private Health _health;
    private bool _gatesOpen, _paused, _dead, _recovering;
    private int _escapeSuppressedFrame = -1;
    private float _nextDrip, _invulnerableUntil;
    private GameObject _helpPanel;
    private TMPro.TMP_Text _helpLabel;

    public static MundoInferiorBlockout Instance { get; private set; }
    // Raised when an attempt restarts (fall, defeat): slabs, stones and encounters rebuild.
    public static event Action AttemptReset;
    public int CurrentRoom => _room;
    public MIHud Hud { get; private set; }
    public bool Busy => _paused || _dead || MIFind.Inspecting != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { AttemptReset = null; Instance = null; }

    private void Awake()
    {
        Instance = this;
        MIProgress.Load();
        _gates = FindObjectsByType<MIGate>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        Time.timeScale = 1;
        _rests = FindObjectsByType<MIRest>(FindObjectsSortMode.None);
        _guardian = FindFirstObjectByType<MIGuardian>();
        _sentinel = FindFirstObjectByType<MIShieldSentinel>();
        var zone = roomSpawns[0] != null ? roomSpawns[0].GetComponentInParent<MICameraZone>() : null;
        if (zone != null && orbitCamera != null) orbitCamera.SetYaw(zone.Yaw, true);
        if (player != null)
        {
            _health = player.GetComponent<Health>();
            if (_health != null) _health.Died += OnDied;
        }
        var hud = new GameObject("MI_Interfaz");
        hud.transform.SetParent(transform, false);
        Hud = hud.AddComponent<MIHud>();
        Hud.Build(_health, Resume, () => Leave(true), () => Leave(false));
        BuildHelp();
        ApplyAbilities();
        EnterRoom(0);
        MIAudio.Loop(gameObject, "ambiente_caverna", .45f, false);
        MIProgress.Changed += OnProgress;
        if (MIProgress.FindsCount == 0) Hud.Notify("Mundo inferior", "Encuentra el santuario de raíces siguiendo el camino de piedra.", UIIcon.Objective, UIPalette.GoldLight);
    }

    private void OnDestroy()
    {
        MIProgress.Changed -= OnProgress;
        if (_health != null) _health.Died -= OnDied;
        if (Instance == this) Instance = null;
        Time.timeScale = 1;
    }

    private void OnProgress(string id) { RefreshObjective(); }

    // Abilities come from the found pieces only (guide 3.3): no inherited training dash.
    public void ApplyAbilities()
    {
        if (player == null) return;
        if (MIProgress.HasDoubleJump) player.GrantDoubleJump();
        if (MIProgress.DashTier > 0) player.GrantDash(MIProgress.DashTier);
    }

    public void EnterRoom(int index)
    {
        _room = Mathf.Clamp(index, 0, roomSpawns.Length - 1);
        bool first = (MIProgress.RoomsMask & (1 << _room)) == 0;
        MIProgress.Discover(_room);
        string name = RoomTitle(_room);
        Hud?.SetZone(name);
        if (first && _room > 0) Hud?.Notify("Zona descubierta", name, UIIcon.Map, UIPalette.GoldLight);
        RefreshObjective();
    }

    private string RoomTitle(int index)
    {
        string raw = index < roomNames.Length ? roomNames[index] : "";
        int space = raw.IndexOf(' ');
        return space > 0 ? raw.Substring(space + 1) : raw;
    }

    // Objective in one sentence from the real state (guide 2.1 route).
    public void RefreshObjective()
    {
        if (Hud == null) return;
        string objective =
            !MIProgress.Has(MIProgress.Seed) ? "Examina la semilla del altar en el santuario de raíces" :
            !MIProgress.Has(MIProgress.Bracelets1) ? "Asciende a la galería y recoge los brazaletes" :
            !(MIProgress.Has(MIProgress.Bracelets2) && MIProgress.Has(MIProgress.Bracelets3)) ? "Recoge las dos mejoras del patio de centinelas" :
            !MIProgress.Has(MIProgress.Shield04) ? "Rompe la defensa del centinela con tres impulsos encadenados" :
            !MIProgress.Has(MIProgress.Horn) ? "Cruza péndulos y derrumbe hasta la cámara del cuerno" :
            !MIProgress.Has(MIProgress.HornGate) ? "Lleva el cuerno al soporte de la antesala" :
            !MIProgress.Has(MIProgress.Guardian) ? "Vence al guardián de la cámara del fondo" :
            "Vuelve a Plaza Núñez por el portal de la cámara";
        Hud.SetObjective(objective);
        Hud.SetCounters("Hallazgos " + MIProgress.FindsCount + " / 5   ·   Ofrendas " + MIProgress.OfferingsCount + " / " + MIProgress.OfferingTotal
            + "   ·   Zonas " + MIProgress.DiscoveredCount + " / 9");
    }

    public void SuppressEscapeThisFrame() => _escapeSuppressedFrame = Time.frameCount;

    // Feet marker -> CharacterController root (2 m capsule, centre 0): about 1 m above the floor.
    public void Recover(Transform anchor, float damage = 0)
    {
        if (player == null || anchor == null || _dead || _recovering) return;
        StartCoroutine(RecoverRoutine(anchor, damage));
    }

    private IEnumerator RecoverRoutine(Transform anchor, float damage)
    {
        _recovering = true;
        // One event per fall: damage first; if it is lethal the defeat takes over the transfer.
        if (damage > 0 && _health != null && Time.time >= _invulnerableUntil)
        {
            MIAudio.Play("caida", .8f);
            _health.TakeDamage(damage);
            if (_health.IsDead) { _recovering = false; yield break; }
        }
        Hud?.SetFade(.85f);
        player.Teleport(anchor.position + Vector3.up * 1.05f);
        if (orbitCamera != null) orbitCamera.SnapAfterTeleport();
        _invulnerableUntil = Time.time + 1f;
        AttemptReset?.Invoke();
        yield return new WaitForSecondsRealtime(.15f);
        Hud?.SetFade(0);
        _recovering = false;
    }

    public void Damage(float amount, Vector3 source)
    {
        if (_health == null || _dead || Time.time < _invulnerableUntil) return;
        _health.TakeDamage(amount);
        MIAudio.Play("dano", .8f);
    }

    private void OnDied()
    {
        if (_dead) return;
        StartCoroutine(DefeatRoutine());
    }

    // Defeat (guide 6.2): one defeat, back to the checkpoint with full health, attempts rebuilt,
    // finds and shortcuts kept, the guardian reset with full health.
    private IEnumerator DefeatRoutine()
    {
        _dead = true;
        player.SetInputLocked(true);
        MIAudio.Play("derrota", .9f);
        Hud.ShowDeath(true, "Vuelves al último descanso. Tus hallazgos, atajos y el cuerno se conservan.");
        yield return new WaitForSecondsRealtime(2.4f);
        Hud.SetFade(1);
        yield return new WaitForSecondsRealtime(.35f);
        Transform spawn = CheckpointSpawn();
        _health.Revive();
        player.Teleport(spawn.position + Vector3.up * 1.05f);
        if (orbitCamera != null) orbitCamera.SnapAfterTeleport();
        var zone = spawn.GetComponentInParent<MICameraZone>();
        if (zone != null && orbitCamera != null) { orbitCamera.SetYaw(zone.Yaw, true); orbitCamera.SetFraming(zone.Distance, zone.Pitch, true); }
        if (_guardian != null) _guardian.ResetEncounter();
        AttemptReset?.Invoke();
        Hud.ShowDeath(false);
        Hud.SetFade(0);
        _invulnerableUntil = Time.time + 1.5f;
        player.SetInputLocked(false);
        _dead = false;
    }

    private Transform CheckpointSpawn()
    {
        int room = MIProgress.Checkpoint;
        if (_rests != null) foreach (var rest in _rests) if (rest != null && rest.Room == room) return rest.Spawn;
        return roomSpawns[Mathf.Clamp(room, 0, roomSpawns.Length - 1)] ?? roomSpawns[0];
    }

    private void Update()
    {
        if (player == null) return;
        if (player.transform.position.y < fallbackLimitY) Recover(roomSpawns[_room], 10);
        var keyboard = Keyboard.current;
        Hud?.SetHintsVisible(!Busy);
        UpdateBars();
        UpdateAmbience();
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && _escapeSuppressedFrame != Time.frameCount && MIFind.Inspecting == null && !_dead)
        {
            if (_paused) Resume(); else Pause();
            return;
        }
        if (Busy) { UIWorldPrompt.Hide(this); return; }

        var target = player.InputLocked ? null : MIInteractable.Nearest(player.transform.position);
        if (target != null)
        {
            UIWorldPrompt.Show(this, "E", target.Prompt);
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame) target.Interact(player);
        }
        else UIWorldPrompt.Hide(this);

        if (keyboard == null) return;
        if (keyboard.f12Key.wasPressedThisFrame) showHelp = !showHelp;
        if (!showHelp) return;
        // Test keys of the blockout (visible only with F12 help).
        if (keyboard.digit1Key.wasPressedThisFrame) player.GrantDoubleJump();
        if (keyboard.digit2Key.wasPressedThisFrame && player.DashTier < 3) player.GrantDash(player.DashTier + 1);
        if (keyboard.gKey.wasPressedThisFrame)
        {
            _gatesOpen = !_gatesOpen;
            foreach (var gate in _gates) if (gate != null) { if (_gatesOpen) gate.SetOpen(true); else gate.Release(); }
        }
        Key[] keys = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9 };
        for (int i = 0; i < keys.Length && i < roomSpawns.Length; i++)
            if (keyboard[keys[i]].wasPressedThisFrame && roomSpawns[i] != null)
            {
                player.Teleport(roomSpawns[i].position + Vector3.up * 1.05f);
                var zone = roomSpawns[i].GetComponentInParent<MICameraZone>();
                if (zone != null && orbitCamera != null) orbitCamera.SetYaw(zone.Yaw, true);
                if (orbitCamera != null) orbitCamera.SnapAfterTeleport();
                EnterRoom(i);
            }
    }

    private void UpdateBars()
    {
        if (Hud == null) return;
        if (_guardian != null && _guardian.Fighting) Hud.SetBoss(_guardian.DisplayName, _guardian.Health01, true);
        else if (_sentinel != null && _sentinel.isActiveAndEnabled && !_sentinel.Defeated && _room == 3)
            Hud.SetBoss(_sentinel.ShieldUp ? "Centinela de escudo · defensa intacta" : "Centinela de escudo · defensa rota", _sentinel.Health01, Vector3.Distance(player.transform.position, _sentinel.transform.position) < 16f);
        else Hud.SetBoss("", 0, false);
    }

    // Base cavern bed plus drips around the player (guide 7.3).
    private void UpdateAmbience()
    {
        if (Time.time < _nextDrip) return;
        _nextDrip = Time.time + UnityEngine.Random.Range(2.5f, 6.5f);
        Vector2 offset = UnityEngine.Random.insideUnitCircle * 9f;
        MIAudio.PlayAt("gota", player.transform.position + new Vector3(offset.x, 3f, offset.y), UnityEngine.Random.Range(.35f, .7f), UnityEngine.Random.Range(.85f, 1.2f));
    }

    private void Pause()
    {
        if (_paused) return;
        _paused = true; Time.timeScale = 0; AudioListener.pause = false;
        player.SetInputLocked(true);
        UIWorldPrompt.Hide(this);
        Hud.ShowPause(true);
        MIAudio.Play("ui_abrir", .6f);
    }

    private void Resume()
    {
        if (!_paused) return;
        _paused = false; Time.timeScale = 1;
        Hud.ShowPause(false);
        player.SetInputLocked(false);
    }

    private void Leave(bool toPlaza)
    {
        Time.timeScale = 1; _paused = false;
        player.SetInputLocked(true);
        if (toPlaza) WorldTravel.ReturnToPlaza(-1);
        else SceneManager.LoadScene(TitleScene);
    }

    // Test keys of the blockout on a light plate under the vitality (F12).
    private void BuildHelp()
    {
        var canvas = UIKit.ScreenCanvas(transform, "MI_Ayuda", 906);
        _helpPanel = UIKit.Place(UIKit.HudPanel(canvas, "BlockoutHelp"), new Vector2(0f, 1f), new Vector2(64f, -196f), new Vector2(440f, 170f)).gameObject;
        _helpLabel = UIKit.Label(_helpPanel.transform, "", 20f, UIPalette.Muted);
        _helpLabel.alignment = TMPro.TextAlignmentOptions.TopLeft;
        _helpLabel.rectTransform.offsetMin = new Vector2(26f, 18f); _helpLabel.rectTransform.offsetMax = new Vector2(-26f, -18f);
    }

    private void LateUpdate()
    {
        if (_helpPanel == null || player == null) return;
        _helpPanel.SetActive(showHelp && !Busy);
        if (!showHelp) return;
        string help = "Pruebas · F12 oculta\n" +
            "Doble salto: " + (player.HasDoubleJump ? "sí" : "no") + "   Impulso: nivel " + player.DashTier + "\n" +
            "1 doble salto · 2 subir impulso\n" +
            "G abrir o cerrar rejas · F1–F9 ir a sala";
        if (_helpLabel.text != help) _helpLabel.text = help;
    }
}

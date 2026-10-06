using System;
using System.Collections;
using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Director of the Mundo Superior (guide 3, 8 and 11.2): loads MSProgress, decides the player's
// movement mode (walk, climb, flight, transport, combat), runs E interactions, zone discovery and
// camera presets, safe-support tracking and fall recovery without damage, internal portal travel,
// rests and checkpoints, the guardian duel, defeat, pause, objective and HUD.
[DefaultExecutionOrder(-100)]
public sealed class MundoSuperiorDirector : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ExplorationOrbitCamera orbitCamera;
    [SerializeField] private MSWings wings;
    [SerializeField] private Transform entrySpawn;
    [SerializeField] private Transform[] zoneTestSpawns = new Transform[8];
    [SerializeField] private GameObject teamLabels;
    // Feet below the last safe support that count as a fall (guide 12.3: about H - 6 m).
    [SerializeField] private float fallDepth = 7f, fallbackLimitY = -12f;
    [SerializeField] private bool showHelp;

    private const string TitleScene = "Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity";
    private Health _health;
    private CharacterController _controller;
    private MSZone _zone;
    private MSRest[] _rests;
    private MSClimbWall _climb;
    private MSGuardian _guardian;
    private Transform _safe;
    private MSSafeGround _ground;
    private float _groundTime;
    private bool _paused, _dead, _moving, _inCombat;
    private int _escapeSuppressedFrame = -1;
    private GameObject _helpPanel;
    // S01 wind, S02 exploration music (melody + percussion layer), S03 guardian music.
    private AudioSource _wind, _music, _drums, _bossMusic;
    private float _musicWeight = 1f, _drumWeight, _bossWeight;
    private TMPro.TMP_Text _helpLabel;

    public static MundoSuperiorDirector Instance { get; private set; }
    public MIHud Hud { get; private set; }
    public bool Paused => _paused;
    public bool InCombat => _inCombat;
    public bool Busy => _paused || _dead || _moving || MSFind.Inspecting != null;
    // Accessibility multiplier of the reaction window (1-3, guide 7.5).
    public float ReactionMultiplier { get; set; } = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    private void Awake()
    {
        Instance = this;
        MSProgress.Load();
    }

    private void Start()
    {
        Time.timeScale = 1;
        _rests = FindObjectsByType<MSRest>(FindObjectsSortMode.None);
        _guardian = FindFirstObjectByType<MSGuardian>();
        _controller = player != null ? player.GetComponent<CharacterController>() : null;
        if (player != null)
        {
            _health = player.GetComponent<Health>();
            if (_health != null) _health.Died += OnDied;
        }
        var hud = new GameObject("MS_Interfaz");
        hud.transform.SetParent(transform, false);
        Hud = hud.AddComponent<MIHud>();
        Hud.Build(_health, Resume, () => Leave(true), () => Leave(false));
        hud.AddComponent<MSFlightMeter>().Build(wings);
        BuildHelp();
        MSAudio.ReadSettings();
        _wind = MSAudio.Loop(gameObject, "viento_alturas", .8f, false, MSAudio.Channel.Ambience);
        _music = MSAudio.Loop(gameObject, "musica_exploracion", .7f, false, MSAudio.Channel.Music);
        _drums = MSAudio.Loop(gameObject, "musica_percusion", 0f, false, MSAudio.Channel.Music);
        _bossMusic = MSAudio.Loop(gameObject, "musica_jefe", 0f, false, MSAudio.Channel.Music);
        if (teamLabels != null) teamLabels.SetActive(showHelp);
        // Every visit from the plaza enters at 01 (guide 3.2); the rests matter for defeat.
        _safe = entrySpawn;
        var first = entrySpawn != null ? entrySpawn.GetComponentInParent<MSZone>() : null;
        if (first != null) EnterZone(first, true);
        MSProgress.Changed += OnProgress;
        RefreshObjective();
        if (MSProgress.FindsCount == 0)
            Hud.Notify("Mundo superior", "Sube las escaleras hacia la primera terraza y busca el altar de la runa.", UIIcon.Objective, UIPalette.GoldLight);
    }

    private void OnDestroy()
    {
        MSProgress.Changed -= OnProgress;
        if (_health != null) _health.Died -= OnDied;
        if (Instance == this) Instance = null;
        Time.timeScale = 1;
    }

    private void OnProgress(string id) => RefreshObjective();

    // ------------------------------------------------------------------ zones and camera

    public void EnterZone(MSZone zone, bool instant = false)
    {
        if (zone == null) return;
        _zone = zone;
        if (zone.Index == 8) MSProgress.Set(MSProgress.Branch);
        else
        {
            bool first = (MSProgress.ZonesMask & (1 << zone.Index)) == 0;
            MSProgress.Discover(zone.Index);
            if (first && zone.Index > 0) Hud?.Notify("Zona descubierta", zone.Title, UIIcon.Map, UIPalette.GoldLight);
        }
        Hud?.SetZone(zone.Title);
        // In flight the preset of the stretch stays: no 90-degree turn over a small trigger.
        if (instant || wings == null || !wings.Flying) ApplyCamera(zone, instant);
        RefreshObjective();
    }

    private void ApplyCamera(MSZone zone, bool instant)
    {
        if (orbitCamera == null || zone == null || (_climb != null && _climb.Climbing) || _inCombat) return;
        orbitCamera.SetYaw(zone.Yaw, instant);
        orbitCamera.SetFraming(zone.Distance, zone.Pitch, instant);
    }

    public void OnFlightChanged(bool flying)
    {
        _groundTime = 0;
        if (!flying) ApplyCamera(_zone, false);
    }

    public void OnClimbChanged(bool climbing, MSClimbWall wall)
    {
        _climb = climbing ? wall : null;
        // A fall from the wall returns to its base annex (guide 8.3), whichever end it started from.
        if (climbing && wall.BaseSafe != null) _safe = wall.BaseSafe;
        if (orbitCamera == null) return;
        if (climbing) { orbitCamera.SetYaw(wall.CameraYaw); orbitCamera.SetFraming(wall.CameraDistance, wall.CameraPitch); }
        else ApplyCamera(_zone, false);
    }

    // ------------------------------------------------------------------ objective (guide 3.3)

    public void RefreshObjective()
    {
        if (Hud == null) return;
        string objective =
            !MSProgress.Has(MSProgress.RunePortals) ? "Examina el altar de la runa en la primera terraza" :
            !MSProgress.Has(MSProgress.RuneClimb) ? "Activa el portal rosado y recoge la runa de escalada" :
            !MSProgress.Has(MSProgress.Wings) ? "Vuelve por el portal y sube la pared de apoyos" :
            (MSProgress.ZonesMask & (1 << 5)) == 0 ? "Vuela hasta la isla del portal azul" + (MSProgress.Has(MSProgress.Yopo2) ? "" : " · opcional: la isla del yopo") :
            !MSProgress.Has(MSProgress.Key) ? "Sube volando por los apoyos hasta el altar de la llave" :
            !MSProgress.Has(MSProgress.LockOpen) ? "Toma el transporte y coloca el medallón en el cierre" :
            !MSProgress.Has(MSProgress.Guardian) ? "Sube a la cima y enfrenta al guardián" :
            "Usa el portal de la cima para volver a Plaza Núñez";
        Hud.SetObjective(objective);
        Hud.SetCounters("Hallazgos " + MSProgress.FindsCount + " / 6   ·   Zonas " + MSProgress.DiscoveredCount + " / 8   ·   Ataque " + MSProgress.AttackDamage.ToString("0"));
    }

    public void OnFound(MSFind find, string title, string text)
    {
        UIIcon icon = find.FindId == MSProgress.Wings ? UIIcon.Bird : find.FindId == MSProgress.Key ? UIIcon.Objective : UIIcon.Journal;
        Hud?.Notify(title, text, icon, UIPalette.GoldLight);
        RefreshObjective();
    }

    // ------------------------------------------------------------------ travel and recovery

    public void TravelThroughPortal(MSPortal from, MSPortal to)
    {
        if (_moving || to == null || to.Arrival == null) return;
        StartCoroutine(Move(to.Arrival, true, to));
    }

    // A fall of exploration (guide 8.3): no damage, back on the last safe support.
    public void Recover(Transform anchor)
    {
        if (_moving || _dead || anchor == null) return;
        MSAudio.Play("recuperacion", .8f);
        StartCoroutine(Move(anchor, false, null));
    }

    private IEnumerator Move(Transform target, bool portal, MSPortal destination)
    {
        _moving = true;
        player.SetInputLocked(true);
        UIWorldPrompt.Hide(this);
        Hud?.SetFade(1);
        yield return new WaitForSeconds(portal ? .2f : .12f);
        CancelMotors();
        Teleport(target);
        destination?.Arrived();
        if (portal) MSAudio.Play("portal_llegada", .8f);
        var zone = target.GetComponentInParent<MSZone>();
        if (zone != null) { _zone = zone; ApplyCamera(zone, true); EnterZone(zone, true); }
        yield return new WaitForSeconds(.15f);
        Hud?.SetFade(0);
        player.SetInputLocked(false);
        _moving = false;
    }

    private void CancelMotors()
    {
        if (_climb != null) _climb.Cancel();
        if (wings != null) wings.Cancel();
    }

    // Feet marker -> CharacterController root: compensate centre and half height (guide 2.1).
    private void Teleport(Transform feet)
    {
        float lift = _controller != null ? _controller.height * .5f - _controller.center.y + _controller.skinWidth : 1.05f;
        player.Teleport(feet.position + Vector3.up * lift);
        Vector3 forward = Vector3.ProjectOnPlane(feet.forward, Vector3.up);
        if (forward.sqrMagnitude > .01f) player.transform.rotation = Quaternion.LookRotation(forward);
        if (orbitCamera != null) orbitCamera.SnapAfterTeleport();
        _groundTime = 0;
    }

    private void Update()
    {
        if (player == null) return;
        UpdateMusic();
        var keyboard = Keyboard.current;
        Hud?.SetHintsVisible(!Busy && !_inCombat);
        if (_guardian != null && _inCombat) Hud?.SetBoss("Guardián de la cima", _guardian.Health01, true);
        else Hud?.SetBoss("", 0, false);
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && _escapeSuppressedFrame != Time.frameCount && MSFind.Inspecting == null && !_dead)
        {
            if (_paused) Resume(); else Pause();
            return;
        }
        if (_paused || _dead) { UIWorldPrompt.Hide(this); return; }
        TrackSupport();
        if (Busy) { UIWorldPrompt.Hide(this); return; }

        // E from firm ground only: never from the air, the wall or the duel.
        bool canInteract = !player.InputLocked && !player.HasMotor && player.IsGrounded;
        var target = canInteract ? MIInteractable.Nearest(player.transform.position) : null;
        if (target != null)
        {
            UIWorldPrompt.Show(this, "E", target.Prompt);
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame) target.Interact(player);
        }
        else UIWorldPrompt.Hide(this);
        DevKeys(keyboard);
    }

    // Safe support (guide 8.2): 0.6 s grounded on an MSSafeGround refills the wings and, if it
    // has a marker, becomes the fall target. Falls are measured against that support.
    private void TrackSupport()
    {
        bool climbing = _climb != null && _climb.Climbing;
        if (!climbing && !_inCombat)
        {
            float feetY = player.transform.position.y + (_controller != null ? _controller.center.y - _controller.height * .5f : -1f);
            if (_safe != null && (feetY < _safe.position.y - fallDepth || feetY < fallbackLimitY)) { Recover(_safe); return; }
        }
        MSSafeGround ground = null;
        if (player.IsGrounded && !player.HasMotor
            && Physics.Raycast(player.transform.position, Vector3.down, out var hit, 1.6f, ~0, QueryTriggerInteraction.Ignore))
            ground = hit.collider.GetComponentInParent<MSSafeGround>();
        if (ground == null) { _ground = null; _groundTime = 0; return; }
        if (ground != _ground) { _ground = ground; _groundTime = 0; }
        _groundTime += Time.deltaTime;
        if (_groundTime < .6f) return;
        if (wings != null) wings.Recharge();
        if (ground.Recovery != null) _safe = ground.Recovery;
    }

    // Music (guide 16.1 S02-S03): the percussion layer grows towards the summit, the guardian's
    // theme replaces both during the duel; the pause keeps a softer mix.
    private void UpdateMusic()
    {
        if (_music == null) return;
        int zone = _zone != null ? _zone.Index : 0;
        float drums = zone == 8 ? .35f : new[] { .1f, .15f, .2f, .35f, .5f, .6f, .75f, .9f }[Mathf.Clamp(zone, 0, 7)];
        float step = Time.unscaledDeltaTime / 1.5f;
        _musicWeight = Mathf.MoveTowards(_musicWeight, _inCombat ? 0f : 1f, step);
        _drumWeight = Mathf.MoveTowards(_drumWeight, _inCombat ? 0f : drums, step);
        _bossWeight = Mathf.MoveTowards(_bossWeight, _inCombat ? 1f : 0f, step);
        float pause = _paused ? .45f : 1f;
        MSAudio.SetLoopVolume(_music, .7f * _musicWeight * pause);
        MSAudio.SetLoopVolume(_drums, .65f * _drumWeight * pause);
        MSAudio.SetLoopVolume(_bossMusic, .8f * _bossWeight * pause);
        MSAudio.SetLoopVolume(_wind, _paused ? .45f : .8f);
    }

    // ------------------------------------------------------------------ combat (guide 7.3)

    public void BeginCombat(MSGuardian guardian, Transform mark)
    {
        _inCombat = true;
        CancelMotors();
        MSProgress.SetCheckpoint(MSProgress.Rest07); // retry from 07 even without using its disc
        player.SetInputLocked(true);
        if (mark != null) Teleport(mark);
        if (orbitCamera != null) { orbitCamera.SetYaw(0); orbitCamera.SetFraming(13, 24); }
        UIWorldPrompt.Hide(this);
    }

    public void EndCombat(bool victory)
    {
        _inCombat = false;
        player.SetInputLocked(false);
        ApplyCamera(_zone, false);
        if (victory) Hud?.Notify("Guardián vencido", "La salida de la cima está activa. Tu progreso se conserva.", UIIcon.Objective, UIPalette.GoldLight);
        RefreshObjective();
    }

    public void Damage(float amount)
    {
        if (_health == null || _dead) return;
        _health.TakeDamage(amount);
        MSAudio.Play("dano", .8f);
    }

    private void OnDied()
    {
        if (_dead) return;
        StartCoroutine(Defeat());
    }

    // Defeat (guide 7.3 step 8 and 8.3): back to 07 with full health, guardian reset,
    // finds and the open lock kept.
    private IEnumerator Defeat()
    {
        _dead = true;
        player.SetInputLocked(true);
        UIWorldPrompt.Hide(this);
        MSAudio.Play("derrota", .9f);
        Hud.ShowDeath(true, "Vuelves a la antesala. Tus hallazgos y el cierre abierto se conservan.");
        yield return new WaitForSecondsRealtime(2.4f);
        Hud.SetFade(1);
        yield return new WaitForSecondsRealtime(.35f);
        if (_guardian != null) _guardian.ResetEncounter();
        _inCombat = false;
        CancelMotors();
        _health.Revive();
        var spawn = CheckpointSpawn();
        Teleport(spawn);
        _safe = spawn;
        var zone = spawn.GetComponentInParent<MSZone>();
        if (zone != null) EnterZone(zone, true);
        Hud.ShowDeath(false);
        Hud.SetFade(0);
        player.SetInputLocked(false);
        _dead = false;
    }

    private Transform CheckpointSpawn()
    {
        if (_rests != null) foreach (var rest in _rests) if (rest != null && rest.Zone == MSProgress.Checkpoint) return rest.Spawn;
        return entrySpawn;
    }

    // ------------------------------------------------------------------ pause and exits

    public void SuppressEscapeThisFrame() => _escapeSuppressedFrame = Time.frameCount;

    private void Pause()
    {
        if (_paused) return;
        _paused = true; Time.timeScale = 0;
        player.SetInputLocked(true);
        UIWorldPrompt.Hide(this);
        Hud.ShowPause(true);
        MSAudio.PauseActions(true);
        MSAudio.Play("ui_abrir", .8f, 1f, MSAudio.Channel.Interface);
    }

    private void Resume()
    {
        if (!_paused) return;
        _paused = false; Time.timeScale = 1;
        Hud.ShowPause(false);
        MSAudio.ReadSettings(); // the pause menu may have changed the volumes
        MSAudio.PauseActions(false);
        player.SetInputLocked(_inCombat || _moving);
    }

    public void LeaveToPlaza() => Leave(true);

    private void Leave(bool toPlaza)
    {
        Time.timeScale = 1; _paused = false;
        player.SetInputLocked(true);
        if (toPlaza) WorldTravel.ReturnToPlaza(1);
        else SceneManager.LoadScene(TitleScene);
    }

    // ------------------------------------------------------------------ blockout test keys (F12)

    private void DevKeys(Keyboard keyboard)
    {
        if (keyboard == null) return;
        if (keyboard.f12Key.wasPressedThisFrame)
        {
            showHelp = !showHelp;
            if (teamLabels != null) teamLabels.SetActive(showHelp);
        }
        if (!showHelp) return;
        if (keyboard.digit1Key.wasPressedThisFrame) MSProgress.Set(MSProgress.RunePortals);
        if (keyboard.digit2Key.wasPressedThisFrame) MSProgress.Set(MSProgress.RuneClimb);
        if (keyboard.digit3Key.wasPressedThisFrame) MSProgress.Set(MSProgress.Wings);
        if (keyboard.digit4Key.wasPressedThisFrame) MSProgress.Set(MSProgress.Key);
        if (keyboard.digit5Key.wasPressedThisFrame) { MSProgress.Set(MSProgress.Guardian); FindFirstObjectByType<MSGuardian>()?.ResetEncounter(); }
        Key[] keys = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8 };
        for (int i = 0; i < keys.Length && i < zoneTestSpawns.Length; i++)
            if (keyboard[keys[i]].wasPressedThisFrame && zoneTestSpawns[i] != null)
            {
                _safe = zoneTestSpawns[i];
                StartCoroutine(Move(zoneTestSpawns[i], true, null));
            }
    }

    private void BuildHelp()
    {
        var canvas = UIKit.ScreenCanvas(transform, "MS_Ayuda", 906);
        _helpPanel = UIKit.Place(UIKit.HudPanel(canvas, "BlockoutHelp"), new Vector2(0f, 1f), new Vector2(64f, -196f), new Vector2(470f, 200f)).gameObject;
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
            "1 runa portales · 2 runa escalada · 3 alas\n" +
            "4 llave · 5 guardián vencido\n" +
            "F1–F8 ir a zona 01–08\n" +
            "Apoyo seguro: " + (_safe != null ? _safe.name : "—");
        if (_helpLabel.text != help) _helpLabel.text = help;
    }
}

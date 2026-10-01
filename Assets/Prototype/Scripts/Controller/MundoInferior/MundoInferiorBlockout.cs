using UnityEngine;
using UnityEngine.InputSystem;

// Test harness of the Mundo Inferior blockout (guide stage 1-3): the single recovery authority
// below the lowest pit, room tracking for the camera zones, and debug keys to measure jumps
// with and without the upgrades. Progression, saving and the real rewards come later.
public sealed class MundoInferiorBlockout : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ExplorationOrbitCamera orbitCamera;
    [SerializeField] private Transform[] roomSpawns = new Transform[9];
    [SerializeField] private string[] roomNames = new string[9];
    // Below every pit floor (rooms 07-09 at -14, spikes 3 m lower) with margin.
    [SerializeField] private float fallbackLimitY = -30f;
    [SerializeField] private bool showHelp = true;

    private int _room;
    private MIGate[] _gates;
    private bool _gatesOpen;

    public static MundoInferiorBlockout Instance { get; private set; }
    public int CurrentRoom => _room;

    private void Awake()
    {
        Instance = this;
        _gates = FindObjectsByType<MIGate>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        var zone = roomSpawns[0] != null ? roomSpawns[0].GetComponentInParent<MICameraZone>() : null;
        if (zone != null && orbitCamera != null) orbitCamera.SetYaw(zone.Yaw, true);
    }

    public void EnterRoom(int index) => _room = Mathf.Clamp(index, 0, roomSpawns.Length - 1);

    // Feet marker -> CharacterController root (2 m capsule, centre 0): about 1 m above the floor.
    public void Recover(Transform anchor)
    {
        if (player == null || anchor == null) return;
        player.Teleport(anchor.position + Vector3.up * 1.05f);
        if (orbitCamera != null) orbitCamera.SnapAfterTeleport();
    }

    private void Update()
    {
        if (player == null) return;
        if (player.transform.position.y < fallbackLimitY) Recover(roomSpawns[_room]);

        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.digit1Key.wasPressedThisFrame) player.GrantDoubleJump();
        if (keyboard.digit2Key.wasPressedThisFrame && player.DashTier < 3) player.GrantDash(player.DashTier + 1);
        if (keyboard.gKey.wasPressedThisFrame)
        {
            _gatesOpen = !_gatesOpen;
            foreach (var gate in _gates) if (gate != null) gate.SetOpen(_gatesOpen);
        }
        if (keyboard.f12Key.wasPressedThisFrame) showHelp = !showHelp;
        Key[] keys = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9 };
        for (int i = 0; i < keys.Length && i < roomSpawns.Length; i++)
            if (keyboard[keys[i]].wasPressedThisFrame && roomSpawns[i] != null)
            {
                EnterRoom(i);
                Recover(roomSpawns[i]);
                var zone = roomSpawns[i].GetComponentInParent<MICameraZone>();
                if (zone != null && orbitCamera != null) orbitCamera.SetYaw(zone.Yaw, true);
            }
    }

    private void OnGUI()
    {
        if (!showHelp || player == null) return;
        string room = _room < roomNames.Length ? roomNames[_room] : "";
        GUI.Box(new Rect(12, 12, 330, 132), "");
        GUI.Label(new Rect(22, 18, 320, 128),
            "BLOQUEO · MUNDO INFERIOR\n" +
            "Sala: " + room + "\n" +
            "Doble salto: " + (player.HasDoubleJump ? "sí" : "no") + "   Impulso: nivel " + player.DashTier + "\n" +
            "1 doble salto · 2 subir impulso · Q impulso\n" +
            "G abrir/cerrar rejas · F1–F9 ir a sala\n" +
            "F12 ocultar ayuda");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

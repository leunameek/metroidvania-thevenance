using UnityEngine;
using UnityEngine.InputSystem;

// A04 of the Mundo Inferior: standing in the passage shows the destination and E travels to
// Plaza Núñez (guide 01/09: contextual action, no teleport on contact, so no bouncing).
// The victory portal of 09 starts inactive until the guardian fight exists.
public sealed class MIPortal : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private GameObject effect;
    [SerializeField] private float range = 2.4f;

    private PlayerController _player;
    private bool _near;

    public bool Active => active;

    public void SetActive(bool value)
    {
        active = value;
        if (effect != null) effect.SetActive(value);
    }

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (effect != null) effect.SetActive(active);
    }

    private void Update()
    {
        if (_player == null) return;
        Vector3 offset = _player.transform.position - transform.position;
        _near = Mathf.Abs(offset.y - 1f) < 1.6f && new Vector2(offset.x, offset.z).magnitude < range;
        var keyboard = Keyboard.current;
        if (_near && active && !_player.InputLocked && keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            _player.SetInputLocked(true);
            WorldTravel.ReturnToPlaza(-1);
        }
    }

    private void OnGUI()
    {
        if (!_near) return;
        string text = active ? "E · Volver a Plaza Núñez" : "El portal se activará al vencer al guardián";
        var size = new Vector2(360, 34);
        GUI.Box(new Rect((Screen.width - size.x) * 0.5f, Screen.height - 90, size.x, size.y), text);
    }
}

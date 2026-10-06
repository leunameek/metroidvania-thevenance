using Nemequene.UI;
using UnityEngine;
using UnityEngine.InputSystem;

// A04: standing in the passage shows the destination and E travels to Plaza Núñez (guide 01/09:
// contextual action, no teleport on contact, so no bouncing). The victory portal of 09 stays dark
// until the guardian falls; its veil brightens and pulses once it is active.
public sealed class MIPortal : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private string requiredFlag;
    [SerializeField] private GameObject effect;
    [SerializeField] private Light glow;
    [SerializeField] private float range = 2.4f;

    private PlayerController _player;
    private Renderer _veil;
    private bool _near;
    private float _intensity;

    public bool Active => active && (string.IsNullOrEmpty(requiredFlag) || MIProgress.Has(requiredFlag));

    public void SetActive(bool value)
    {
        active = value;
        if (effect != null) effect.SetActive(Active);
    }

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (effect != null) { effect.SetActive(Active); _veil = effect.GetComponent<Renderer>(); }
        if (glow != null) _intensity = glow.intensity;
    }

    private void Update()
    {
        if (effect != null && effect.activeSelf != Active) effect.SetActive(Active);
        if (_veil != null && _veil.gameObject.activeSelf)
        {
            // Slow scroll and breathing of the energy plane.
            var material = _veil.material;
            material.mainTextureOffset = new Vector2(0, Time.time * .08f);
            float pulse = .8f + .2f * Mathf.Sin(Time.time * 2.1f);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", new Color(.25f, .6f, 1f) * 1.6f * pulse);
        }
        if (glow != null) glow.intensity = Active ? _intensity * (.85f + .15f * Mathf.Sin(Time.time * 2.1f)) : _intensity * .15f;
        if (_player == null) return;
        Vector3 offset = _player.transform.position - transform.position;
        _near = Mathf.Abs(offset.y - 1f) < 1.6f && new Vector2(offset.x, offset.z).magnitude < range;
        var keyboard = Keyboard.current;
        var natural = WorldNaturalInput.Instance;
        if (_near && Active && !_player.InputLocked
            && (keyboard != null && keyboard.eKey.wasPressedThisFrame || natural != null && natural.ConsumeInteract("Regresar a Plaza Núñez"))
            && (MundoInferiorBlockout.Instance == null || !MundoInferiorBlockout.Instance.Busy))
        {
            _player.SetInputLocked(true);
            MIAudio.Play("portal", .9f);
            WorldTravel.ReturnToPlaza(-1);
        }
    }

    private void LateUpdate()
    {
        // Screen 18: the portal names its destination on the interaction ribbon.
        if (_near && (MundoInferiorBlockout.Instance == null || !MundoInferiorBlockout.Instance.Busy))
            UIWorldPrompt.Show(this, Active ? VoicePrompt.Cap("entrar", "E") : null, Active ? "Regresar a Plaza Núñez" : "El portal se activará al vencer al guardián");
        else UIWorldPrompt.Hide(this);
    }

    private void OnDisable() => UIWorldPrompt.Hide(this);
}

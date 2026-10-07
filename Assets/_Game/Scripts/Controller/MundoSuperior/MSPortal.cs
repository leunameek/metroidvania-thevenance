using Nemequene.UI;
using UnityEngine;

// A01+A02 portal (guide 6.1). Internal pairs (P1/P2 pink, P3/P4 blue) need Runa 1 and move the
// player to the arrival marker of the other portal without loading a scene; the return of 01 and
// the summit exit (after the guardian) load Plaza Núñez. Touching the surface never travels: a new
// E is required, and after arriving the portal stays dark to E until the player has left its range.
public sealed class MSPortal : MIInteractable
{
    [SerializeField] private string requiredFlag = "";
    [SerializeField] private string lockedText = "";
    [SerializeField] private string destinationName = "";
    [SerializeField] private bool toPlaza;
    [SerializeField] private Transform arrival;
    [SerializeField] private MSPortal pair;
    [SerializeField] private Renderer veil;
    [SerializeField] private Light glow;
    [SerializeField] private Color color = Color.white;

    private bool _armed = true;
    private float _lockedUntil, _glowIntensity;
    private Transform _player;
    private AudioSource _hum;

    public bool Active => string.IsNullOrEmpty(requiredFlag) || MSProgress.Has(requiredFlag);
    public Transform Arrival => arrival;
    public override bool Available => base.Available && _armed && Time.time >= _lockedUntil;
    public override string Prompt => Active ? "Viajar a " + destinationName : lockedText;

    private void Start()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null) _player = player.transform;
        if (glow != null) _glowIntensity = glow.intensity;
        // S11: the portal only hums while it is active (guide 16.2).
        _hum = MSAudio.Loop(gameObject, "portal_zumbido", 0f, true, MSAudio.Channel.Effects, 12f);
    }

    // Called on the destination portal: no E until the player walks out of range (guide 6.1).
    public void Arrived()
    {
        _armed = false;
        _lockedUntil = Time.time + .75f;
    }

    public override void Interact(PlayerController player)
    {
        var director = MundoSuperiorDirector.Instance;
        if (!Active)
        {
            director?.Hud?.Notify(displayName, lockedText, UIIcon.Info, UIPalette.Muted);
            MSAudio.Unavailable();
            return;
        }
        MSAudio.Play("portal_viajar", .9f);
        if (toPlaza) { director?.LeaveToPlaza(); return; }
        if (pair != null && director != null) director.TravelThroughPortal(this, pair);
    }

    private void Update()
    {
        bool active = Active;
        MSAudio.SetLoopVolume(_hum, active ? .55f : 0f);
        if (veil != null)
        {
            if (veil.gameObject.activeSelf != active) veil.gameObject.SetActive(active);
            if (active)
            {
                var material = veil.material;
                material.mainTextureOffset = new Vector2(0, Time.time * .08f);
                float pulse = .8f + .2f * Mathf.Sin(Time.time * 2.1f);
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 1.6f * pulse);
            }
        }
        if (glow != null) glow.intensity = active ? _glowIntensity * (.85f + .15f * Mathf.Sin(Time.time * 2.1f)) : 0f;
        if (!_armed && _player != null)
        {
            Vector3 d = _player.position - transform.position;
            if (new Vector2(d.x, d.z).magnitude > range + .4f || Mathf.Abs(d.y - 1f) > 2.5f) _armed = true;
        }
    }
}

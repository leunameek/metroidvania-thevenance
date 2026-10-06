using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlazaCombatController : MonoBehaviour
{
    [SerializeField] private TechnicalDemoController demo;
    [SerializeField] private Transform entry;
    [SerializeField] private Transform playerMark;
    [SerializeField] private Transform guardian;
    [SerializeField] private Renderer warningRing;
    private Vector3 _returnPosition, _guardianHome;
    private Quaternion _returnRotation;
    private MaterialPropertyBlock _properties;
    private int _previousMistakes;
    private float _flash;
    private int _beginFrame;
    public PlazaCombatModel Model { get; private set; }
    public bool Completed { get; private set; }
    public float FlashIntensity { get; set; } = 1;
    public Vector3 EntryPosition => entry.position;
    public bool Active => demo.State == TechnicalDemoState.Combat;

    public void RestoreCompleted() { Completed = true; }

    private void Awake()
    {
        Model = new PlazaCombatModel();
        Model.Changed += OnPhase;
        _properties = new MaterialPropertyBlock();
        if (guardian != null) _guardianHome = guardian.position;
    }
    public void Begin()
    {
        if (demo.State != TechnicalDemoState.Exploration) return;
        _beginFrame = Time.frameCount;
        _returnPosition = demo.Player.transform.position;
        _returnRotation = demo.Player.transform.rotation;
        demo.SetCombat();
        demo.Player.Teleport(playerMark.position);
        demo.Player.transform.rotation = playerMark.rotation;
        demo.PlayerHealth.Revive();
        _previousMistakes = 0;
        var camera = Camera.main.transform;
        camera.position = transform.position + new Vector3(7.2f, 5.8f, -8.6f);
        camera.LookAt(transform.position + Vector3.up * 1.2f);
        Model.Start();
    }
    private void Update()
    {
        if (!Active || demo.HelpOpen || Time.frameCount == _beginFrame) return;
        Keyboard k = Keyboard.current;
        if (!demo.ManagedUI && k != null && k.escapeKey.wasPressedThisFrame) { Cancel(); return; }
        if (Model.Phase == PlazaCombatPhase.Won)
        {
            if (k != null && k.eKey.wasPressedThisFrame) Cancel();
            return;
        }
        // Tick before input so a key at the end of the reaction window cannot arrive late.
        Model.Tick(Time.deltaTime);
        if (k != null && k.eKey.wasPressedThisFrame) Attack();
        if (k != null && k.spaceKey.wasPressedThisFrame) Defend(PlazaDefense.Dodge);
        if (k != null && k.fKey.wasPressedThisFrame) Defend(PlazaDefense.Guard);
    }
    public void Attack()
    {
        if (!Active || demo.HelpOpen || !Model.Attack()) return;
        demo.Audio.Play(PlazaSound.Attack);
        demo.Audio.Play(PlazaSound.Impact, 0.65f);
        _flash = 0.35f;
        StartCoroutine(Strike());
    }
    public void Defend(PlazaDefense defense)
    {
        if (!Active || demo.HelpOpen) return;
        if (Model.Phase != PlazaCombatPhase.React)
        {
            demo.SetStatus("Espera el aviso ¡AHORA! para defenderte.");
            return;
        }
        bool correct = Model.Expected == defense;
        if (!Model.Defend(defense) || !correct) return;
        if (defense == PlazaDefense.Dodge)
        {
            demo.Audio.Play(PlazaSound.Dash);
            demo.Player.PerformDodge(-1);
            StartCoroutine(ReturnToMark());
        }
        else { demo.Audio.Play(PlazaSound.Guard); _flash = 0.25f; }
    }
    private IEnumerator ReturnToMark()
    {
        yield return new WaitForSeconds(0.32f);
        if (Active) demo.Player.Teleport(playerMark.position);
    }
    private IEnumerator Strike()
    {
        Vector3 start = playerMark.position;
        for (float t = 0; t < 0.32f && Active; t += Time.deltaTime)
        {
            float pulse = Mathf.Sin(t / 0.32f * Mathf.PI);
            demo.Player.Teleport(start + Vector3.forward * pulse * 1.2f);
            guardian.position = _guardianHome + Vector3.forward * pulse * 0.32f;
            yield return null;
        }
        guardian.position = _guardianHome;
        if (Active) demo.Player.Teleport(start);
    }
    private void OnPhase()
    {
        if (Model.Phase == PlazaCombatPhase.Telegraph) demo.Audio.Play(PlazaSound.Warning, 0.55f);
        if (Model.Phase == PlazaCombatPhase.React) demo.Audio.Play(PlazaSound.Inspect, 0.6f);
        if (Model.Mistakes > _previousMistakes)
        {
            _previousMistakes = Model.Mistakes;
            demo.PlayerHealth.TakeDamage(10);
            if (demo.PlayerHealth.CurrentHealth <= 30) demo.PlayerHealth.Revive();
            demo.Audio.Play(PlazaSound.Impact, 0.7f);
            demo.SetStatus("No pasa nada. Repetimos la misma defensa; espera el aviso y pulsa la tecla indicada.");
        }
        if (Model.Phase == PlazaCombatPhase.Won)
        {
            Completed = true;
            demo.PlayerHealth.Revive();
            demo.Audio.Play(PlazaSound.Victory);
            demo.SetStatus(demo.PortalsUnlocked ? "Entrenamiento superado. Los dos portales están abiertos." : "Entrenamiento superado. Completa las estaciones de objetos para abrir los portales.");
        }
    }
    private void LateUpdate()
    {
        if (warningRing == null) return;
        _flash = Mathf.Max(0, _flash - Time.deltaTime);
        Color color = !Active ? new Color(0.18f, 0.48f, 0.38f)
            : Model.Phase == PlazaCombatPhase.Telegraph ? new Color(1, 0.24f, 0.08f)
            : Model.Phase == PlazaCombatPhase.React ? new Color(1, 0.75f, 0.17f) : new Color(0.1f, 0.8f, 0.62f);
        _properties.SetColor("_BaseColor", color);
        _properties.SetColor("_EmissionColor", color * (Active ? 0.7f + _flash * 3 * FlashIntensity : 0.12f));
        warningRing.SetPropertyBlock(_properties);
        if (guardian != null && Active && Model.Phase == PlazaCombatPhase.Telegraph)
            guardian.localRotation = Quaternion.Euler(-Mathf.Sin(Time.time * 9) * 3, 180, 0);
        else if (guardian != null) guardian.localRotation = Quaternion.Euler(0, 180, 0);
    }
    public void Cancel()
    {
        if (!Active) return;
        StopAllCoroutines();
        Model.Cancel();
        guardian.position = _guardianHome;
        demo.Player.Teleport(_returnPosition);
        demo.Player.transform.rotation = _returnRotation;
        demo.PlayerHealth.Revive();
        demo.SetExploration();
    }
    private void OnDestroy() { if (Model != null) Model.Changed -= OnPhase; }
}

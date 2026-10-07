using Nemequene.UI;
using UnityEngine;

// Horn socket of 08 (guide zone 08): without the horn it says where to find it; with it, E
// places a mark of the horn on the socket, sounds it and opens the ritual gate for good.
public sealed class MIHornSocket : MIInteractable
{
    [SerializeField] private GameObject placedHorn;
    [SerializeField] private Light glow;
    public override bool Available => base.Available && !MIProgress.Has(MIProgress.HornGate);
    public override string Prompt => MIProgress.Has(MIProgress.Horn) ? "Colocar el cuerno" : "Examinar el soporte";
    private void Start() { Refresh(); }
    private void Refresh()
    {
        bool open = MIProgress.Has(MIProgress.HornGate);
        if (placedHorn != null) placedHorn.SetActive(open);
        if (glow != null) glow.intensity = open ? 3f : .6f;
    }
    public override void Interact(PlayerController player)
    {
        var director = MundoInferiorBlockout.Instance;
        CharacterActions.Of(player)?.PlayAny(MIProgress.Has(MIProgress.Horn) ? "Button" : "Reach");
        if (!MIProgress.Has(MIProgress.Horn))
        {
            MIAudio.Play("ui_error", .6f);
            director?.Hud?.Notify("Reja cerrada", "Necesitas el cuerno de la cámara inferior (zona 07).", UIIcon.Lock, UIPalette.Danger);
            return;
        }
        if (!MIProgress.Set(MIProgress.HornGate)) return;
        MIAudio.Play("cuerno", 1f);
        Refresh();
        MIParticles.Burst(transform.position + Vector3.up * 1.3f, new Color(1f, .8f, .45f), 70, 2f, .1f, -.3f);
        director?.Hud?.Notify("La reja se abre", "El cuerno queda registrado; el guardián espera al fondo.", UIIcon.Portal, UIPalette.GoldLight);
        director?.RefreshObjective();
    }
}

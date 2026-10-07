using Nemequene.UI;
using UnityEngine;

// H05 lever (guide 5.3): proximity and E. The first use sets its flag, pulls the handle and
// opens every gate reading that flag; later uses never close anything.
public sealed class MILever : MIInteractable
{
    [SerializeField] private string flagId;
    [SerializeField] private Transform handle;
    [SerializeField] private float pulledAngle = 55f;
    [SerializeField] private string doneTitle = "Atajo abierto";
    [SerializeField, TextArea] private string doneText = "";
    private float _t;
    public override bool Available => base.Available && !MIProgress.Has(flagId);
    public override string Prompt => "Accionar " + displayName.ToLowerInvariant();
    private void Start() { _t = MIProgress.Has(flagId) ? 1 : 0; Apply(); }
    // The handle moves (and the gates open) when Nemequene's hands pull it.
    public override void Interact(PlayerController player)
    {
        if (MIProgress.Has(flagId)) return;
        PlayerInteraction.Perform(player, handle != null ? handle : transform, () =>
        {
            if (!MIProgress.Set(flagId)) return;
            MIAudio.PlayAt("palanca", transform.position);
            var director = MundoInferiorBlockout.Instance;
            director?.Hud?.Notify(doneTitle, doneText, UIIcon.Rotate, UIPalette.GoldLight);
            director?.RefreshObjective();
        }, "Lever");
    }
    private void Update()
    {
        float target = MIProgress.Has(flagId) ? 1 : 0;
        if (Mathf.Approximately(_t, target)) return;
        _t = Mathf.MoveTowards(_t, target, Time.deltaTime / .45f); Apply();
    }
    private void Apply() { if (handle != null) handle.localRotation = Quaternion.Euler(Mathf.SmoothStep(0, pulledAngle, _t), 0, 0); }
}

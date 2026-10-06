using Nemequene.UI;
using UnityEngine;

// Engage point of the climbing wall (guide 5.4): at the base annex E starts the climb, at the
// edge of 04 E starts the descent. Without Runa 2 it only explains what is missing.
public sealed class MSClimbAccess : MIInteractable
{
    [SerializeField] private MSClimbWall wall;
    [SerializeField] private bool fromTop;
    public override bool Available => base.Available && wall != null && !wall.Climbing;
    public override string Prompt => !MSProgress.Has(MSProgress.RuneClimb) ? "Pared con apoyos · necesitas la runa de escalada"
        : fromTop ? "Descender por la pared" : "Escalar la pared";
    public override void Interact(PlayerController player)
    {
        if (!MSProgress.Has(MSProgress.RuneClimb))
        {
            MundoSuperiorDirector.Instance?.Hud?.Notify("Pared con apoyos", "Necesitas la runa de escalada.", UIIcon.Info, UIPalette.Muted);
            MSAudio.Unavailable();
            return;
        }
        wall.Begin(player, fromTop);
    }
}

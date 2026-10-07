using Nemequene.UI;
using UnityEngine;

// O09 rest disc (guide 5.1, 5.5, 5.10): E heals and makes it the persistent checkpoint.
public sealed class MSRest : MIInteractable
{
    [SerializeField] private int zone;
    [SerializeField] private Transform spawn;
    [SerializeField] private Light glow;
    public Transform Spawn => spawn != null ? spawn : transform;
    public int Zone => zone;
    public override string Prompt => "Descansar";
    public override void Interact(PlayerController player)
    {
        var health = player.GetComponent<Health>();
        if (health != null) health.Heal(health.MaxHealth);
        CharacterActions.Of(player)?.PlayAny("Kneel");
        MSProgress.SetCheckpoint(zone);
        MSAudio.Play("descanso", .9f);
        MIParticles.Burst(transform.position + Vector3.up * .3f, new Color(1f, .86f, .55f, .9f), 50, 1.6f, .09f, -.6f);
        MundoSuperiorDirector.Instance?.Hud?.Notify("Descanso", "Vida restaurada. Al recargar la partida volverás a este punto.", UIIcon.Save, UIPalette.Jade);
        if (glow != null) glow.intensity = 3f;
    }
}

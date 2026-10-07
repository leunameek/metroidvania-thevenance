using Nemequene.UI;
using UnityEngine;

// O04e rest disc (guide 5.5): E heals and fixes the persistent checkpoint of its room.
public sealed class MIRest : MIInteractable
{
    [SerializeField] private int room;
    [SerializeField] private Transform spawn;
    [SerializeField] private Light glow;
    public Transform Spawn => spawn != null ? spawn : transform;
    public int Room => room;
    public override string Prompt => "Descansar";
    public override void Interact(PlayerController player)
    {
        // He kneels on the disc; the rest takes hold when his knee touches it.
        PlayerInteraction.Perform(player, null, () => Rest(player), "Kneel");
    }

    private void Rest(PlayerController player)
    {
        var health = player.GetComponent<Health>();
        if (health != null) health.Heal(health.MaxHealth);
        MIProgress.SetCheckpoint(room);
        MIAudio.Play("descanso", .9f);
        MIParticles.Burst(transform.position + Vector3.up * .3f, new Color(.45f, .9f, .8f, .9f), 50, 1.6f, .09f, -.6f);
        MundoInferiorBlockout.Instance?.Hud?.Notify("Descanso", "Vida restaurada. Si caes, volverás a este punto.", UIIcon.Save, UIPalette.Jade);
        if (glow != null) glow.intensity = 3f;
    }
}

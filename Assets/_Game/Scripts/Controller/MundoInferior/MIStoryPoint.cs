using System;
using UnityEngine;

// A character of the story the player talks to with E in a world (the Custodio de Raíces): the
// same interaction ribbon as the finds; what it says depends on the campaign.
public sealed class MIStoryPoint : MIInteractable
{
    private Func<string> _prompt = () => "Hablar";
    private Action _use;

    public override string Prompt => _prompt();
    public override bool Available => base.Available && !StoryPlayer.Pending;
    public override void Interact(PlayerController player) => _use?.Invoke();

    public static MIStoryPoint Create(string name, Transform parent, Vector3 position, float range, Func<string> prompt, Action use)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        var point = go.AddComponent<MIStoryPoint>();
        point.range = range; point.displayName = name; point._prompt = prompt; point._use = use;
        return point;
    }
}

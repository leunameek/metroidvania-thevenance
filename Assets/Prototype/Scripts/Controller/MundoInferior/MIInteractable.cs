using System.Collections.Generic;
using UnityEngine;

// Something the player uses with E from firm ground (guide 3.2 and 5.3): finds, levers, rests,
// the horn socket. The level director picks the nearest available one on the same terrace,
// shows its prompt on the interaction ribbon and calls Interact on E.
public abstract class MIInteractable : MonoBehaviour
{
    [SerializeField] protected float range = 2.2f;
    [SerializeField] protected string displayName = "";
    public static readonly List<MIInteractable> All = new List<MIInteractable>();

    public string DisplayName => displayName;
    public virtual bool Available => isActiveAndEnabled;
    public abstract string Prompt { get; }
    public abstract void Interact(PlayerController player);

    protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    protected virtual void OnDisable() { All.Remove(this); }

    // Same terrace only: a find one level up or down is never offered through the floor.
    public float Score(Vector3 player)
    {
        Vector3 d = player - transform.position;
        if (Mathf.Abs(d.y - 1f) > 1.7f) return float.MaxValue;
        float flat = new Vector2(d.x, d.z).magnitude;
        return flat <= range ? flat : float.MaxValue;
    }

    public static MIInteractable Nearest(Vector3 player)
    {
        MIInteractable best = null; float bestScore = float.MaxValue;
        foreach (var item in All)
        {
            if (item == null || !item.Available) continue;
            float score = item.Score(player);
            if (score < bestScore) { bestScore = score; best = item; }
        }
        return best;
    }
}

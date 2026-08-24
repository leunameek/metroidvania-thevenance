using UnityEngine;

public class DashPickupReward : MonoBehaviour, IPickupReward
{
    [SerializeField, Range(1, 3)] private int dashTier = 1;

    public void Grant(PlayerController player)
    {
        player.GrantDash(dashTier);
    }
}

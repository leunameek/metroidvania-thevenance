using UnityEngine;

public class DashPickupReward : MonoBehaviour, IPickupReward
{
    public void Grant(PlayerController player)
    {
        player.GrantDashUpgrade();
    }
}

using UnityEngine;

public class DoubleJumpPickupReward : MonoBehaviour, IPickupReward
{
    public void Grant(PlayerController player)
    {
        player.GrantDoubleJump();
    }
}

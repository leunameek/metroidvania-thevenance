using UnityEngine;

// Walkable surface that counts as real support (guide 4.4 and 8.2): standing on it 0.6 s
// recharges the wings and, when it has a recovery marker, makes that marker the place a fall
// returns to. Railings, walls, climbing holds and decoration never carry it.
public sealed class MSSafeGround : MonoBehaviour
{
    [SerializeField] private Transform recovery;
    public Transform Recovery => recovery;
}

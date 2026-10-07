using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fireCooldown = 2f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject arrowPrefab;

    private PlayerController _player;
    private ArcherModel _archer;

    private void Awake()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _archer = new ArcherModel(detectionRange, fireCooldown);
    }

    private void Update()
    {
        if (_player == null) return;

        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (!_archer.IsPlayerInRange(distance)) return;

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (_archer.TickShouldFire(Time.deltaTime)) Fire();
    }

    private void Fire()
    {
        if (arrowPrefab == null || muzzle == null) return;
        Instantiate(arrowPrefab, muzzle.position, muzzle.rotation);
    }
}

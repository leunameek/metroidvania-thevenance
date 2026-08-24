using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerController))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private float fallThresholdY = -10f;
    [SerializeField] private float fallDamage = 30f;

    private Health _health;
    private PlayerController _player;
    private Vector3 _spawnPosition;
    private Vector3 _lastGroundedPosition;
    private bool _justDied;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _player = GetComponent<PlayerController>();
        _spawnPosition = transform.position;
        _lastGroundedPosition = transform.position;
    }

    private void OnEnable()
    {
        _health.Died += HandleDied;
    }

    private void OnDisable()
    {
        _health.Died -= HandleDied;
    }

    private void Update()
    {
        if (_player.IsGrounded) _lastGroundedPosition = transform.position;

        if (transform.position.y < fallThresholdY) HandleFall();
    }

    private void HandleFall()
    {
        _health.TakeDamage(fallDamage);

        if (_justDied)
        {
            _justDied = false;
            return;
        }

        _player.Teleport(_lastGroundedPosition);
    }

    private void HandleDied()
    {
        _justDied = true;
        _player.Teleport(_spawnPosition);
        _health.Revive();
    }
}

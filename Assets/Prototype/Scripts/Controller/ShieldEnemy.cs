using Nemequene.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class ShieldEnemy : MonoBehaviour
{
    [SerializeField] private DashHurtbox dashHurtbox;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Collider shieldCollider;

    [Header("Charge Attack")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float minChargeRange = 2.5f;
    [SerializeField] private float chargeSpeed = 18f;
    [SerializeField] private float windupDuration = 0.9f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private float cooldown = 6f;
    [SerializeField] private float hitRadius = 1.2f;
    [SerializeField] private float chargeDamage = 50f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolPauseDuration = 1.5f;

    [Header("Alert")]
    [SerializeField] private Vector2 alertMargin = new Vector2(0f, 150f);

    public bool IsShielded => _shield.IsShielded;

    private PlayerController _player;
    private Health _health;
    private ShieldEnemyModel _shield;

    private static GameObject _alertGo;

    private void Awake()
    {
        if (dashHurtbox != null) dashHurtbox.enabled = false;
        _player = FindFirstObjectByType<PlayerController>();

        _shield = new ShieldEnemyModel(transform.position, detectionRange, minChargeRange, chargeSpeed,
            windupDuration, chargeDuration, cooldown, hitRadius, chargeDamage,
            patrolRadius, patrolSpeed, patrolPauseDuration);

        _health = GetComponent<Health>();
        _health.Died += HandleDied;
        if (_alertGo == null) BuildAlertIcon();
    }

    private void HandleDied()
    {
        if (_alertGo != null) _alertGo.SetActive(false);
    }

    private void BuildAlertIcon()
    {
        _alertGo = UIKit.Alert(null, "ShieldChargeAlertCanvas", 950, "¡Embestida!", alertMargin.y);
    }

    public void RegisterDashChainHit(int chainCount)
    {
        if (!_shield.RegisterDashChainHit(chainCount)) return;

        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.isTrigger = true;
        if (dashHurtbox != null) dashHurtbox.enabled = true;
    }

    private void Update()
    {
        if (_player == null) return;

        switch (_shield.State)
        {
            case ShieldEnemyModel.ChargeState.Idle:
                UpdateIdle();
                break;
            case ShieldEnemyModel.ChargeState.Windup:
                UpdateWindup();
                break;
            case ShieldEnemyModel.ChargeState.Charging:
                UpdateCharge();
                break;
            case ShieldEnemyModel.ChargeState.Recovering:
                _shield.TickRecovery(Time.deltaTime);
                break;
        }
    }

    private void UpdateIdle()
    {
        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;

        if (_shield.TryStartWindup(toPlayer))
        {
            if (_alertGo != null) _alertGo.SetActive(true);
            return;
        }

        Vector3 moveDelta = _shield.TickPatrol(transform.position, Time.deltaTime);
        if (moveDelta != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDelta.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        transform.position += moveDelta;
    }

    private void UpdateWindup()
    {
        Quaternion targetRotation = Quaternion.LookRotation(_shield.ChargeDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        if (_shield.TickWindup(Time.deltaTime) && _alertGo != null) _alertGo.SetActive(false);
    }

    private void UpdateCharge()
    {
        transform.position += _shield.GetChargeMoveDelta(Time.deltaTime);

        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;

        if (_shield.CheckChargeHit(toPlayer))
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null) playerHealth.TakeDamage(_shield.ChargeDamage);
            return;
        }

        _shield.TickChargeExpiry(Time.deltaTime);
    }
}

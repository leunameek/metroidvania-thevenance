using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class ShieldEnemy : MonoBehaviour
{
    private enum ChargeState { Idle, Windup, Charging, Recovering }

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
    [SerializeField] private Vector2 alertSize = new Vector2(28f, 90f);
    [SerializeField] private Vector2 alertMargin = new Vector2(0f, 40f);

    public bool IsShielded { get; private set; } = true;

    private PlayerController _player;
    private Health _health;
    private ChargeState _state;
    private float _stateTimeRemaining;
    private Vector3 _chargeDirection;

    private Vector3 _spawnPosition;
    private Vector3 _patrolTarget;
    private float _patrolPauseRemaining;

    private static GameObject _alertGo;

    private void Awake()
    {
        if (dashHurtbox != null) dashHurtbox.enabled = false;
        _player = FindFirstObjectByType<PlayerController>();
        _spawnPosition = transform.position;
        _patrolTarget = _spawnPosition;

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
        GameObject canvasGo = new GameObject("ShieldChargeAlertCanvas", typeof(RectTransform));
        _alertGo = canvasGo;

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        RectTransform anchorRect = canvasGo.GetComponent<RectTransform>();
        anchorRect.anchorMin = new Vector2(0.5f, 1f);
        anchorRect.anchorMax = new Vector2(0.5f, 1f);
        anchorRect.pivot = new Vector2(0.5f, 1f);
        anchorRect.anchoredPosition = new Vector2(alertMargin.x, -alertMargin.y);
        anchorRect.sizeDelta = new Vector2(60f, 120f);

        Color alertColor = new Color(1f, 0.15f, 0.1f);

        GameObject barGo = new GameObject("AlertBar", typeof(RectTransform));
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.SetParent(anchorRect, false);
        barRect.sizeDelta = new Vector2(alertSize.x, alertSize.y * 0.72f);
        barRect.anchoredPosition = new Vector2(0f, -alertSize.y * 0.14f);
        barGo.AddComponent<Image>().color = alertColor;

        GameObject dotGo = new GameObject("AlertDot", typeof(RectTransform));
        RectTransform dotRect = dotGo.GetComponent<RectTransform>();
        dotRect.SetParent(anchorRect, false);
        dotRect.sizeDelta = new Vector2(alertSize.x, alertSize.x);
        dotRect.anchoredPosition = new Vector2(0f, -alertSize.y + alertSize.x * 0.5f);
        dotGo.AddComponent<Image>().color = alertColor;

        canvasGo.SetActive(false);
    }

    public void RegisterDashChainHit(int chainCount)
    {
        if (!IsShielded || chainCount < 3) return;
        BreakShield();
    }

    private void BreakShield()
    {
        IsShielded = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.isTrigger = true;
        if (dashHurtbox != null) dashHurtbox.enabled = true;
    }

    private void Update()
    {
        if (_player == null) return;

        switch (_state)
        {
            case ChargeState.Idle:
                if (!TryStartWindup()) UpdatePatrol();
                break;
            case ChargeState.Windup:
                UpdateWindup();
                break;
            case ChargeState.Charging:
                UpdateCharge();
                break;
            case ChargeState.Recovering:
                _stateTimeRemaining -= Time.deltaTime;
                if (_stateTimeRemaining <= 0f) _state = ChargeState.Idle;
                break;
        }
    }

    private bool TryStartWindup()
    {
        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange || distance < minChargeRange) return false;

        _chargeDirection = toPlayer.normalized;
        _state = ChargeState.Windup;
        _stateTimeRemaining = windupDuration;
        if (_alertGo != null) _alertGo.SetActive(true);
        return true;
    }

    private void UpdatePatrol()
    {
        Vector3 toTarget = _patrolTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < 0.3f)
        {
            _patrolPauseRemaining -= Time.deltaTime;
            if (_patrolPauseRemaining <= 0f) PickNewPatrolTarget();
            return;
        }

        Vector3 direction = toTarget.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        Vector3 delta = direction * patrolSpeed * Time.deltaTime;
        delta.y = 0f;
        transform.position += delta;
    }

    private void PickNewPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        _patrolTarget = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
        _patrolPauseRemaining = patrolPauseDuration;
    }

    private void UpdateWindup()
    {
        Quaternion targetRotation = Quaternion.LookRotation(_chargeDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        _stateTimeRemaining -= Time.deltaTime;
        if (_stateTimeRemaining <= 0f)
        {
            _state = ChargeState.Charging;
            _stateTimeRemaining = chargeDuration;
            if (_alertGo != null) _alertGo.SetActive(false);
        }
    }

    private void UpdateCharge()
    {
        transform.position += _chargeDirection * chargeSpeed * Time.deltaTime;

        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.magnitude <= hitRadius)
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null) playerHealth.TakeDamage(chargeDamage);
            EndCharge();
            return;
        }

        _stateTimeRemaining -= Time.deltaTime;
        if (_stateTimeRemaining <= 0f) EndCharge();
    }

    private void EndCharge()
    {
        _state = ChargeState.Recovering;
        _stateTimeRemaining = cooldown;
    }
}

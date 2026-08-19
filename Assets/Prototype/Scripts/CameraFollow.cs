using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followHeight = true;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Boss Fight (Third Person)")]
    [SerializeField] private float thirdPersonDistance = 3f;
    [SerializeField] private float thirdPersonHeight = 1.4f;
    [SerializeField] private float thirdPersonSmoothTime = 0.25f;

    private Vector3 _offset;
    private Vector3 _velocity;
    private bool _thirdPerson;

    private void Start()
    {
        if (target == null) return;
        _offset = transform.position - target.position;
    }

    public void EnterThirdPerson()
    {
        _thirdPerson = true;
    }

    public void ExitThirdPerson()
    {
        _thirdPerson = false;
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition;
        if (_thirdPerson)
        {
            desiredPosition = target.position - target.forward * thirdPersonDistance + Vector3.up * thirdPersonHeight;
        }
        else
        {
            desiredPosition = target.position + _offset;
            if (!followHeight) desiredPosition.y = transform.position.y;
        }

        float currentSmoothTime = _thirdPerson ? thirdPersonSmoothTime : smoothTime;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, currentSmoothTime);

        if (_thirdPerson) transform.rotation = Quaternion.LookRotation(target.forward, Vector3.up);
    }
}

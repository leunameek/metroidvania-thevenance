using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followHeight = true;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 _offset;
    private Vector3 _velocity;

    private void Start()
    {
        if (target == null) return;
        _offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + _offset;
        if (!followHeight) desiredPosition.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
    }
}

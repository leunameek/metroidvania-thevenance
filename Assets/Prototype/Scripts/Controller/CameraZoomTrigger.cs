using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class CameraZoomTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Zoom Out")]
    [SerializeField, Range(1f, 179f)] private float zoomedOutFieldOfView = 80f;
    [SerializeField, Min(0f)] private float transitionDuration = 0.5f;

    private readonly HashSet<Collider> _playerCollidersInside = new();
    private float _defaultFieldOfView;
    private float _fieldOfViewVelocity;
    private bool _cameraInitialized;

    private void Awake()
    {
        ResolveCamera();
    }

    private void Update()
    {
        if (!ResolveCamera()) return;

        _playerCollidersInside.RemoveWhere(colliderInside => colliderInside == null);
        float desiredFieldOfView = _playerCollidersInside.Count > 0
            ? zoomedOutFieldOfView
            : _defaultFieldOfView;

        if (transitionDuration <= 0f)
        {
            targetCamera.fieldOfView = desiredFieldOfView;
            _fieldOfViewVelocity = 0f;
            return;
        }

        targetCamera.fieldOfView = Mathf.SmoothDamp(
            targetCamera.fieldOfView,
            desiredFieldOfView,
            ref _fieldOfViewVelocity,
            transitionDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            _playerCollidersInside.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _playerCollidersInside.Remove(other);
    }

    private void OnDisable()
    {
        _playerCollidersInside.Clear();
        _fieldOfViewVelocity = 0f;

        if (_cameraInitialized && targetCamera != null)
            targetCamera.fieldOfView = _defaultFieldOfView;
    }

    private bool ResolveCamera()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return false;

        if (!_cameraInitialized)
        {
            _defaultFieldOfView = targetCamera.fieldOfView;
            _cameraInitialized = true;
        }

        return true;
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnValidate()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger != null) trigger.isTrigger = true;
    }
}

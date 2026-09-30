using UnityEngine;

public sealed class PlazaWorldLabel : MonoBehaviour
{
    private Camera _camera;
    private Renderer _surface;
    private TextMesh _text;
    private TechnicalDemoController _demo;
    private void Start()
    {
        _surface = GetComponent<Renderer>();
        _text = GetComponent<TextMesh>();
        _text.characterSize = Mathf.Min(_text.characterSize, 0.055f);
        _demo = FindFirstObjectByType<TechnicalDemoController>();
    }
    private void LateUpdate()
    {
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return;
        float distance = Vector3.Distance(_camera.transform.position, transform.position);
        _surface.enabled = distance > 5 && distance < 32 && (_demo == null || !_demo.ManagedUI && _demo.State == TechnicalDemoState.Exploration);
        transform.localScale = Vector3.one * Mathf.Clamp(distance / 18, 0.45f, 1);
        transform.rotation = _camera.transform.rotation;
    }
}

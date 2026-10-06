using UnityEngine;

public class PulsingOrb : MonoBehaviour
{
    [SerializeField] private Light glowLight;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 4f;
    [SerializeField] private Color emissionColor = Color.cyan;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Vector3 _startPosition;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _floatingEnabled = true;

    private void Awake()
    {
        _startPosition = transform.localPosition;
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void SetFloating(bool enabled)
    {
        _floatingEnabled = enabled;
    }

    private void Update()
    {
        float t = Time.time;

        if (_floatingEnabled)
        {
            Vector3 position = _startPosition;
            position.y += Mathf.Sin(t * floatSpeed) * floatAmplitude;
            transform.localPosition = position;
        }

        float pulse = (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

        if (glowLight != null) glowLight.intensity = intensity;

        if (_renderer != null)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(EmissionColorId, emissionColor * intensity);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}

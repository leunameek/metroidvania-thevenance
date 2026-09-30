using UnityEngine;

public enum PlazaSound { Step, Jump, Land, Dash, Inspect, Rotate, Complete, Attack, Impact, Guard, Warning, Portal, Victory }

public sealed class PlazaAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] cues;
    [SerializeField] private AudioClip plazaAmbience;
    [SerializeField] private AudioClip lowerAmbience;
    [SerializeField] private AudioClip upperAmbience;
    [SerializeField, Range(0, 1)] private float effectsVolume = 0.7f;
    [SerializeField, Range(0, 1)] private float ambienceVolume = 0.35f;
    private AudioSource _effects, _ambient;
    private PlayerController _player;
    private Vector3 _previous;
    private bool _grounded;
    private int _dash;
    private float _stride, _lastRotate = -1;
    public bool Muted { get; private set; }
    public float EffectsVolume => effectsVolume;
    public float AmbienceVolume => ambienceVolume;

    private void Awake()
    {
        _effects = gameObject.AddComponent<AudioSource>();
        _effects.playOnAwake = false;
        _effects.spatialBlend = 0;
        _ambient = gameObject.AddComponent<AudioSource>();
        _ambient.playOnAwake = false;
        _ambient.loop = true;
        _ambient.clip = plazaAmbience;
        _ambient.volume = ambienceVolume;
        _ambient.Play();
    }
    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null) _previous = _player.transform.position;
    }
    private void LateUpdate()
    {
        if (_player == null) return;
        Vector3 motion = _player.transform.position - _previous;
        float distance = Vector3.ProjectOnPlane(motion, Vector3.up).magnitude;
        bool grounded = _player.IsGrounded;
        if (!_player.InputLocked && distance < 1.2f)
        {
            if (grounded && !_player.IsDashing)
            {
                _stride += distance;
                if (_stride > 1.55f) { Play(PlazaSound.Step, 0.48f); _stride = 0; }
            }
            if (!grounded && _grounded && motion.y > 0.04f) Play(PlazaSound.Jump, 0.5f);
            if (grounded && !_grounded && motion.y < 0) Play(PlazaSound.Land, 0.5f);
            if (_player.DashInstanceId != _dash) Play(PlazaSound.Dash);
        }
        else _stride = 0;
        _dash = _player.DashInstanceId;
        _grounded = grounded;
        _previous = _player.transform.position;
    }
    public void Play(PlazaSound sound, float gain = 1)
    {
        int index = (int)sound;
        if (_effects == null || cues == null || index >= cues.Length || cues[index] == null) return;
        if (sound == PlazaSound.Rotate)
        {
            if (Time.unscaledTime - _lastRotate < 0.16f) return;
            _lastRotate = Time.unscaledTime;
        }
        _effects.PlayOneShot(cues[index], effectsVolume * gain);
    }
    public void SetWorld(int world)
    {
        AudioClip clip = world < 0 ? lowerAmbience : world > 0 ? upperAmbience : plazaAmbience;
        if (clip == _ambient.clip) return;
        _ambient.clip = clip;
        _ambient.Play();
    }
    public void SetVolumes(float effects, float ambience)
    {
        effectsVolume = Mathf.Clamp01(effects);
        ambienceVolume = Mathf.Clamp01(ambience);
        _ambient.volume = ambienceVolume;
    }
    public void ToggleMute()
    {
        Muted = !Muted;
        _effects.mute = _ambient.mute = Muted;
    }
}

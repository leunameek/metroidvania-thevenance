using UnityEngine;

public enum PlazaSound { Step, Jump, Land, Dash, Inspect, Rotate, Complete, Attack, Impact, Guard, Warning, Portal, Victory }

// Sound of Plaza Núñez. The scene keeps its serialized cues (same files, revised content); they
// play through GameAudio, so channels, pause and variation match the rest of the game. The
// player's steps, jumps, landings and dash come from PlayerFootsteps. The plaza adds its bed
// (wind, leaves), a low music, the fountain as a spatial source and birds scattered around.
public sealed class PlazaAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] cues;
    [SerializeField] private AudioClip plazaAmbience;
    [SerializeField] private AudioClip lowerAmbience;
    [SerializeField] private AudioClip upperAmbience;
    [SerializeField, Range(0, 1)] private float effectsVolume = 0.7f;
    [SerializeField, Range(0, 1)] private float ambienceVolume = 0.35f;
    private float _lastRotate = -1;
    private AudioClip _bed;
    public bool Muted => GameAudio.Muted;
    public float EffectsVolume => GameAudio.Mix.effects;
    public float AmbienceVolume => GameAudio.Mix.ambience;

    private void Start()
    {
        GameAudio.DefaultSurface = "piedra";
        _bed = plazaAmbience;
        GameAudio.AmbienceClip(_bed, 1f, 2f);
        // Guion C04: "música baja" in the plaza.
        GameAudio.Music("Musica/musica_plaza", .6f, 4f);
        PlaceFountain();
        AmbientScatter.On(gameObject)
            .Add("Ambiente/ave_canto", 6f, 16f, 8f, 22f, .45f, 5f)
            .Add("Ambiente/hojas_rafaga", 14f, 30f, 6f, 14f, .35f, 2f)
            .Add("Ambiente/ave_lejana", 35f, 70f, 25f, 40f, .3f, 12f);
    }

    // The fountain sounds where it stands (MODEL_Fuente, or the falling water of the old graybox).
    private void PlaceFountain()
    {
        var fountain = GameObject.Find("MODEL_Fuente");
        if (fountain == null) fountain = GameObject.Find("Falling water");
        if (fountain == null) return;
        var host = new GameObject("Sonido_Fuente");
        host.transform.SetParent(transform, false);
        var renderers = fountain.GetComponentsInChildren<Renderer>();
        Vector3 at = fountain.transform.position;
        if (renderers.Length > 0) { var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds); at = b.center; }
        host.transform.position = at;
        GameAudio.Loop(host, "Ambiente/fuente_bucle", .8f, true, AudioChannel.Ambience, 22f);
    }

    public void Play(PlazaSound sound, float gain = 1)
    {
        int index = (int)sound;
        if (cues == null || index >= cues.Length || cues[index] == null) return;
        if (sound == PlazaSound.Rotate)
        {
            if (Time.unscaledTime - _lastRotate < 0.16f) return;
            _lastRotate = Time.unscaledTime;
        }
        // Completions and the end of the training are short phrases: the music dips under them.
        if (sound == PlazaSound.Complete || sound == PlazaSound.Victory || sound == PlazaSound.Portal)
        {
            var source = GameAudio.PlayClip(cues[index], gain, AudioChannel.Music, 1f, 0f);
            if (source != null) GameAudioHost.Current?.DuckFor("plaza_frase", DuckRequest.Stinger, cues[index].length * .8f);
            return;
        }
        GameAudio.PlayClip(cues[index], gain, AudioChannel.Effects);
    }

    public void SetWorld(int world)
    {
        AudioClip clip = world < 0 ? lowerAmbience : world > 0 ? upperAmbience : plazaAmbience;
        if (clip == null || clip == _bed) return;
        _bed = clip;
        GameAudio.AmbienceClip(clip, 1f, 2f);
    }

    // The old help panel's sliders and the accessibility settings set the shared mix.
    public void SetVolumes(float effects, float ambience)
    {
        effectsVolume = Mathf.Clamp01(effects);
        ambienceVolume = Mathf.Clamp01(ambience);
        GameAudio.Mix.effects = effectsVolume;
        GameAudio.Mix.ambience = ambienceVolume;
    }

    public void ToggleMute() => GameAudio.Muted = !GameAudio.Muted;
}

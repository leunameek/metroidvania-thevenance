using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// The shared mixer without a game scene: limits, pause, beds, ducking and scene changes.
public sealed class GameAudioTests
{
    private readonly List<string> _played = new List<string>();
    private GameObject _listener;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1;
        _listener = new GameObject("Oyente", typeof(AudioListener));
        GameAudioHost.Played += Record;
        _played.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        GameAudioHost.Played -= Record;
        Time.timeScale = 1;
        if (GameAudioHost.Current != null) Object.Destroy(GameAudioHost.Current.gameObject);
        Object.Destroy(_listener);
    }

    private void Record(string id, AudioChannel channel) => _played.Add(id);

    [UnityTest]
    public IEnumerator BankResolvesVariantsAndMissingCuesAreSilent()
    {
        Assert.IsTrue(GameAudio.Has("Foley/paso_piedra"), "numbered variants form one cue");
        Assert.IsTrue(GameAudio.Has("MIAudio/caida"), "full Resources paths work");
        Assert.IsNull(GameAudio.Play("Foley/no_existe"));
        var clips = new HashSet<AudioClip>();
        for (int i = 0; i < 12; i++) clips.Add(GameAudio.Clip("Foley/paso_piedra"));
        Assert.Greater(clips.Count, 2, "steps vary");
        yield return null;
    }

    [UnityTest]
    public IEnumerator TheSameCueDoesNotPileUp()
    {
        for (int i = 0; i < 6; i++) GameAudio.Play("Combate/impacto_piedra", 1f, AudioChannel.Effects, 1f, .03f, 0f, 2);
        yield return null;
        Assert.AreEqual(2, _played.FindAll(p => p == "Combate/impacto_piedra").Count, "at most two at once");
        _played.Clear();
        GameAudio.Play("UI/ui_foco"); GameAudio.Play("UI/ui_foco");
        Assert.AreEqual(1, _played.Count, "a second focus tick in the same frame is dropped");
    }

    [UnityTest]
    public IEnumerator PauseStopsActionSoundsButNotTheInterface()
    {
        var effect = GameAudio.Play("Ambiente/agua_corriente_bucle", .5f, AudioChannel.Effects);
        Assert.IsNotNull(effect);
        yield return null;
        Time.timeScale = 0;
        yield return null; yield return null;
        Assert.IsTrue(GameAudioHost.Current.Paused);
        Assert.IsFalse(effect.isPlaying, "effects wait behind the pause");
        Assert.IsNull(GameAudio.Play("Combate/bloqueo"), "nothing new starts behind the pause");
        var ui = GameAudio.Play("UI/ui_confirmar", 1f, AudioChannel.Interface);
        Assert.IsNotNull(ui, "the pause menu still answers");
        Time.timeScale = 1;
        yield return null; yield return null;
        Assert.IsFalse(GameAudioHost.Current.Paused);
        Assert.IsTrue(effect.isPlaying, "the effect resumes where it was");
    }

    [UnityTest]
    public IEnumerator MusicCrossfadesAndListeningDucksIt()
    {
        GameAudio.Music("Musica/musica_plaza", 1f, .2f);
        yield return new WaitForSecondsRealtime(.4f);
        var first = GameAudioHost.Current.MusicClip;
        Assert.IsNotNull(first);
        GameAudio.Music("Musica/musica_legado", 1f, .2f);
        yield return new WaitForSecondsRealtime(.4f);
        Assert.AreNotEqual(first, GameAudioHost.Current.MusicClip);
        GameAudio.SetDuck("escucha", DuckRequest.Listening);
        yield return new WaitForSecondsRealtime(1.2f);
        Assert.Less(GameAudioHost.Current.DuckGain(AudioChannel.Music), .4f, "about -9 dB while the voice is heard");
        Assert.AreEqual(1f, GameAudioHost.Current.DuckGain(AudioChannel.Interface));
        GameAudio.ClearDuck("escucha");
        yield return new WaitForSecondsRealtime(1.2f);
        Assert.AreEqual(1f, GameAudioHost.Current.DuckGain(AudioChannel.Music), 1e-3);
    }

    [UnityTest]
    public IEnumerator ANewSceneStopsTheOldWorldAndReleasesUnclaimedBeds()
    {
        GameAudio.Ambience("Ambiente/amb_plaza", 1f, .1f);
        var world = GameAudio.PlayAt("Ambiente/fuente_bucle", Vector3.one, 1f, AudioChannel.Effects);
        yield return new WaitForSecondsRealtime(.3f);
        Assert.IsNotNull(GameAudioHost.Current.AmbienceClip);
        var previous = SceneManager.GetActiveScene();
        var scene = SceneManager.CreateScene("AudioTestScene");
        SceneManager.SetActiveScene(scene);
        yield return null;
        Assert.IsFalse(world.isPlaying, "world sounds stop with their scene");
        yield return new WaitForSecondsRealtime(2.8f);
        Assert.IsNull(GameAudioHost.Current.AmbienceClip, "nobody asked for the bed again: it fades out");
        SceneManager.SetActiveScene(previous);
        yield return SceneManager.UnloadSceneAsync(scene);
    }
}

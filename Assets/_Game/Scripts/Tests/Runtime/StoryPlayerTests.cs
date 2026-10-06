using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// The story player in a bare scene: no save slot (campaign in memory), a camera and a player.
public sealed class StoryPlayerTests
{
    private GameObject _camera, _player, _bachue;

    [SetUp]
    public void SetUp()
    {
        WorldTravel.SaveSlot = -1;
        CampaignProgress.Model.Clear();
        _camera = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" };
        _camera.transform.position = new Vector3(0, 3, -8);
        _player = new GameObject("Player");
        _player.AddComponent<CharacterController>();
        _player.AddComponent<PlayerController>().enabled = false;
        _bachue = new GameObject("Bachue");
        _bachue.transform.position = new Vector3(3, 0, 2);
        StoryActor.Ensure(_bachue, "Bachué", 1.6f);
    }

    [TearDown]
    public void TearDown()
    {
        StoryPlayer.SkipAll();
        Object.Destroy(_camera); Object.Destroy(_player); Object.Destroy(_bachue);
        var story = GameObject.Find("StoryPlayer");
        if (story != null) Object.Destroy(story);
    }

    [UnityTest]
    public IEnumerator SequencePlaysLineByLineLocksThePlayerAndAppliesItsFlags()
    {
        bool done = false;
        StoryPlayer.PlaySequence("H03", () => done = true);
        yield return null;
        Assert.IsTrue(StoryPlayer.Active);
        var player = _player.GetComponent<PlayerController>();
        Assert.IsTrue(player.InputLocked);
        int lines = CampaignProgress.Script.Get("H03").lines.Length;
        for (int i = 0; i < lines * 2 + 2 && StoryPlayer.Active; i++)
        {
            StoryPlayer.Next();   // first press reveals the line, the second goes on
            yield return null;
        }
        Assert.IsFalse(StoryPlayer.Active);
        Assert.IsTrue(done);
        Assert.IsFalse(player.InputLocked);
        Assert.IsTrue(CampaignProgress.Has(CampaignFlags.PoporoOwned));
        Assert.IsTrue(CampaignProgress.Has(CampaignFlags.MapOwned));
        Assert.IsTrue(CampaignProgress.Seen("H03"));
        Assert.That(Vector3.Distance(_camera.transform.position, new Vector3(0, 3, -8)), Is.LessThan(.001f), "camera restored");
    }

    [UnityTest]
    public IEnumerator SkippingAppliesTheSameFlagsAsFinishing()
    {
        StoryPlayer.PlaySequence("H02");
        yield return null;
        StoryPlayer.SkipAll();
        yield return null;
        Assert.IsTrue(CampaignProgress.Has(CampaignFlags.VisionSeen));
        Assert.IsFalse(StoryPlayer.Active);
    }

    [UnityTest]
    public IEnumerator TriggersPlayOncePerSaveAndWaitForTheirGate()
    {
        bool open = false;
        StoryPlayer.AddGate(_player, () => open);
        Assert.IsTrue(StoryPlayer.Trigger(StoryTriggers.PlazaStation(0)));
        yield return null;
        Assert.IsFalse(StoryPlayer.Active, "gate closed");
        open = true;
        yield return null;
        Assert.IsTrue(StoryPlayer.Active);
        StoryPlayer.SkipAll();
        Assert.IsFalse(StoryPlayer.Trigger(StoryTriggers.PlazaStation(0)), "already seen");
    }

    [UnityTest]
    public IEnumerator FlagTriggersPlayWhenTheFlagIsFirstSet()
    {
        StoryPlayer.Listen();
        CampaignProgress.Set(CampaignFlags.MiSeed);
        yield return null;
        Assert.IsTrue(StoryPlayer.Active);
        StoryPlayer.SkipAll();
        Assert.IsTrue(CampaignProgress.Seen("H06"), "the seed line ends H06");
    }
}

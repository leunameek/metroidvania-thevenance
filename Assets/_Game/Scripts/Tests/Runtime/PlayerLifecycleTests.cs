using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class PlayerLifecycleTests
{
    private readonly List<GameObject> _objects = new List<GameObject>();

    private GameObject Create(string name)
    {
        var obj = new GameObject(name);
        _objects.Add(obj);
        return obj;
    }

    private static void Set(object instance, string field, object value)
    {
        instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);
    }

    private PlayerController Player()
    {
        var player = Create("Player").AddComponent<PlayerController>();
        player.enabled = false;
        return player;
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = _objects.Count - 1; i >= 0; i--) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void CombatDeathDoesNotPreventTheNextFallReturningToSafeGround()
    {
        var player = Player();
        var respawn = player.gameObject.AddComponent<PlayerRespawn>();
        var health = player.GetComponent<Health>();
        Set(health, "invulnerabilityDuration", 0f);
        var safePosition = new Vector3(8, 2, 5);
        Set(respawn, "_lastGroundedPosition", safePosition);
        health.TakeDamage(health.MaxHealth);
        player.Teleport(Vector3.down * 20);
        respawn.SendMessage("HandleFall");
        Assert.That(player.transform.position, Is.EqualTo(safePosition));
        Assert.That(health.CurrentHealth, Is.EqualTo(70f));
    }

    [Test]
    public void LethalFallReturnsToSpawnInsteadOfLastGroundedPosition()
    {
        var player = Player();
        var respawn = player.gameObject.AddComponent<PlayerRespawn>();
        Set(respawn, "fallDamage", 100f);
        Set(respawn, "_lastGroundedPosition", new Vector3(8, 2, 5));
        player.Teleport(Vector3.down * 20);
        respawn.SendMessage("HandleFall");
        Assert.That(player.transform.position, Is.EqualTo(Vector3.zero));
        Assert.That(player.GetComponent<Health>().IsDead, Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DisablingAnInspectedPickupRestoresPlayerAndPreviousCameraState(bool followEnabled)
    {
        var player = Player();
        var camera = Create("Inspection camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 3, -8);
        var follow = camera.AddComponent<CameraFollow>();
        follow.enabled = followEnabled;
        var pickup = Create("Pickup").AddComponent<InspectablePickup>();
        pickup.transform.position = Vector3.forward * 4;
        Set(pickup, "_player", player);
        pickup.SendMessage("BeginInspect");
        Assert.That(player.InputLocked, Is.True);
        camera.transform.position = Vector3.one;
        pickup.gameObject.SetActive(false);
        Assert.That(player.InputLocked, Is.False);
        Assert.That(follow.enabled, Is.EqualTo(followEnabled));
        Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0, 3, -8)));
    }

    [Test]
    public void ACollectedDashPickupCannotGrantItsRewardTwice()
    {
        var player = Player();
        var camera = Create("Inspection camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>();
        var obj = Create("Dash pickup");
        obj.transform.position = Vector3.forward * 4;
        obj.AddComponent<DashPickupReward>();
        var pickup = obj.AddComponent<InspectablePickup>();
        Set(pickup, "_player", player);
        Set(pickup, "_inspection", new InspectionModel(0f, 1f, 1f, false, false));
        pickup.SendMessage("BeginInspect");
        pickup.SendMessage("BlendIntoInspect");
        pickup.SendMessage("Claim");
        Assert.That(obj.activeSelf, Is.False);
        obj.SetActive(true);
        pickup.SendMessage("Claim");
        Assert.That(player.DashTier, Is.EqualTo(1));
        Assert.That(player.InputLocked, Is.False);
    }
}

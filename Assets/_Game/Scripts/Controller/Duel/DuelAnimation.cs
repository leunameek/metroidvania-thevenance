using System;
using System.Collections;
using UnityEngine;

// The bodies of a turn duel (A16-A25, A37-A55), choreographed so the sequence reads (2026-10-06
// playtest: the clips looked rigid and the order got lost):
//  - Nemequene stands in a combat stance (CombatIdle) for the whole duel.
//  - His action plays and the enemy flinches when the blow arrives, not when the key is pressed.
//  - The warning is a wind-up: the enemy raises the blow and holds it while the answer is awaited;
//    it lands when Nemequene answers (his defense meets it) or when the time runs out, and a missed
//    defense shakes him a moment after the impact.
//  - Dodges move him aside; at the end of every exchange he steps back to where the duel began, so
//    a long fight never carries him off the terrace.
// Characters without a state simply skip it. The jaguar affinity calls the jaguar itself.
public sealed class DuelAnimation
{
    private const float ImpactDelay = .38f, HitDelay = .28f, ReturnSeconds = .45f;
    private readonly CharacterActions _player, _enemy;
    private readonly PlayerController _playerController;
    private readonly Transform _playerBody, _enemyFocus;
    private readonly DuelChoreography _runner;
    private string _strike;
    private bool _dodgeRight, _anchored, _struck;
    private Vector3 _anchor;
    private Quaternion _anchorRotation;

    public DuelAnimation(IDuelStage stage)
    {
        _playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        _playerBody = _playerController != null ? _playerController.transform : null;
        _player = _playerController != null ? CharacterActions.Of(_playerController) : null;
        _enemyFocus = stage?.Focus;
        _enemy = _enemyFocus != null ? CharacterActions.Of(_enemyFocus) : null;
        if (_enemy == null && stage is Component c) _enemy = c.GetComponentInChildren<CharacterActions>();
        _runner = DuelChoreography.Create();
        if (_player != null) { _player.Stance = "CombatIdle"; _player.PlayAny("CombatIdle"); }
    }

    // Where the duel stands: taken the first time the duel moves (the stage has placed him by then).
    private void Anchor()
    {
        if (_anchored || _playerBody == null) return;
        _anchored = true; _anchor = _playerBody.position; _anchorRotation = _playerBody.rotation;
    }

    // ------------------------------------------------------------------ Nemequene acts

    public void PlayerAction(DuelAction action, DuelTarget target, bool hurt, bool last)
    {
        Anchor();
        switch (action)
        {
            case DuelAction.Attack: _player?.PlayAny("Attack"); break;
            case DuelAction.Counter: _player?.PlayAny("Combo", "Attack"); break;
            case DuelAction.Interrupt: _player?.PlayAny("Spin", "Attack"); break;
            case DuelAction.Anchor: _player?.PlayAny("PowerUp"); break;
            case DuelAction.Horn: _player?.PlayAny("Button", "Point"); break;
            case DuelAction.Bind: _player?.PlayAny("Reach", "Point"); break;
            case DuelAction.Jaguar:
                _player?.PlayAny("PowerUp");
                if (_playerBody != null && _enemyFocus != null) JaguarSpirit.Call(_playerBody, _enemyFocus);
                break;
        }
        // The flinch waits for the blow (the jaguar's lunge takes a little longer); the last blow
        // leaves the enemy to the ending.
        float delay = action == DuelAction.Jaguar ? JaguarSpirit.Impact : action == DuelAction.Counter ? .55f : ImpactDelay;
        if (hurt && !last) _runner.After(delay, () => EnemyHurt(target));
    }

    // ------------------------------------------------------------------ the enemy's blow

    // The warning: the enemy raises the blow and holds it.
    public void EnemyTelegraph(DuelMove move)
    {
        Anchor();
        ReturnToAnchor();
        _struck = false; _strike = null;
        if (_enemy == null || move == null) return;
        string id = (move.Id ?? "").ToLowerInvariant();
        if (move.Origin == DuelTarget.HeadA) Wind(.32f, id.Contains("sacud") ? "SacudidaA" : "AtaqueA", "Attack");
        else if (move.Origin == DuelTarget.HeadB) Wind(.32f, id.Contains("puls") ? "PulsoB" : "AtaqueB", "Spin");
        else if (id.Contains("barr") || id.Contains("ala")) Wind(.3f, "Spin", "Embestida", "Attack");
        else if (id.Contains("onda") || id.Contains("vient") || id.Contains("union") || id.Contains("unión"))
        {
            // A charged power: the gathering plays whole, the release is its own blow.
            _enemy.PlayAny("PowerUp", "Rugido", "Attack");
            _strike = _enemy.Has("Spin") ? "Spin" : _enemy.Has("Embestida") ? "Embestida" : null;
        }
        else Wind(.3f, "Combo", "Attack", "Embestida", "AtaqueA");
    }

    private void Wind(float at, params string[] states)
    {
        if (_enemy.Windup(at, states)) return;
        _enemy.PlayAny(states);
    }

    // The blow lands now (an answer was given, or the time ran out).
    private void Strike()
    {
        if (_struck) return;
        _struck = true;
        if (_enemy == null) return;
        if (_enemy.Release()) return;
        if (_strike != null) _enemy.PlayAny(_strike);
    }

    // Nemequene's answer meets the blow.
    public void PlayerDefense(DuelDefense defense)
    {
        Strike();
        switch (defense)
        {
            case DuelDefense.Block: case DuelDefense.Parry: _player?.PlayAny("Block"); break;
            case DuelDefense.Cover: _player?.PlayAny("Crouch", "Block"); break;
            default:
                _dodgeRight = !_dodgeRight;
                _player?.PlayAny(_dodgeRight ? "DodgeRight" : "DodgeLeft", "DodgeLeft");
                break;
        }
    }

    // After the response: a missed defense shakes Nemequene when the blow arrives.
    public void Resolved(bool correct)
    {
        Strike();
        if (!correct) _runner.After(HitDelay, () => _player?.PlayAny("HitGut", "HitLeft"));
    }

    // Side of the next dodge, for the stage's sidestep (alternates so he never drifts one way).
    public int DodgeSide => _dodgeRight ? 1 : -1;

    public void EnemyHurt(DuelTarget target)
    {
        if (_enemy == null) return;
        if (target == DuelTarget.HeadA) _enemy.PlayAny("GolpeA", "HitLeft");
        else if (target == DuelTarget.HeadB) _enemy.PlayAny("GolpeB", "HitRight");
        else _enemy.PlayAny("HitGut", "HitLeft", "Golpe", "GolpeA");
    }

    // ------------------------------------------------------------------ turn and end

    // Nemequene's turn again: back to his place, in guard.
    public void Decide()
    {
        ReturnToAnchor();
        _enemy?.Release();
    }

    public void Ended(bool victory)
    {
        var player = _player; var enemy = _enemy;
        _enemy?.Release();
        if (player != null) player.Stance = null;
        ReturnToAnchor();
        if (victory)
        {
            // The last blow lands first; then the enemy yields and Nemequene lowers the staff.
            _runner.After(.5f, () => enemy?.PlayAny("Liberada", "Kneel"));
            _runner.After(.9f, () => player?.Rest());
        }
        else player?.PlayAny("HitGut");
        _runner.FinishAfter(2.5f);
    }

    private void ReturnToAnchor()
    {
        if (!_anchored || _playerController == null) return;
        if ((_playerBody.position - _anchor).sqrMagnitude < .0025f) return;
        _runner.Glide(_playerController, _anchor, _anchorRotation, ReturnSeconds);
    }
}

// Runs the timed parts of a duel's choreography. It outlives the duel object (the last blow and the
// enemy's yielding play after the duel has ended) and removes itself afterwards.
public sealed class DuelChoreography : MonoBehaviour
{
    private Coroutine _glide;

    public static DuelChoreography Create() => new GameObject("Duelo_Coreografia").AddComponent<DuelChoreography>();

    public void After(float seconds, Action action)
    {
        if (this != null && isActiveAndEnabled) StartCoroutine(Wait(seconds, action));
    }

    private static IEnumerator Wait(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    public void FinishAfter(float seconds) { if (this != null) Destroy(gameObject, seconds); }

    // Eases the player back to a spot (the CharacterController is moved by teleport steps, so walls
    // and slopes never stop the return).
    public void Glide(PlayerController player, Vector3 to, Quaternion rotation, float seconds)
    {
        if (this == null || !isActiveAndEnabled) return;
        if (_glide != null) StopCoroutine(_glide);
        _glide = StartCoroutine(GlideRoutine(player, to, rotation, seconds));
    }

    private IEnumerator GlideRoutine(PlayerController player, Vector3 to, Quaternion rotation, float seconds)
    {
        if (player == null) yield break;
        Vector3 from = player.transform.position; Quaternion turn = player.transform.rotation;
        for (float t = 0; t < seconds && player != null; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0, 1, t / seconds);
            player.Teleport(Vector3.Lerp(from, to, k));
            player.transform.rotation = Quaternion.Slerp(turn, rotation, k);
            yield return null;
        }
        if (player != null) { player.Teleport(to); player.transform.rotation = rotation; }
        _glide = null;
    }
}

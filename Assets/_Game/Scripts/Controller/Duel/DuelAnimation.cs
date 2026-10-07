using UnityEngine;

// The bodies of a turn duel (A16-A25, A37-A55): Nemequene answers every accepted action and
// defense with its clip, the enemy winds up on the warning, flinches when its health drops and
// kneels (or withdraws, the serpent) when freed. Characters without a state simply skip it.
// The jaguar affinity calls the jaguar itself: it roars beside Nemequene and lunges at the enemy.
public sealed class DuelAnimation
{
    private readonly CharacterActions _player, _enemy;
    private readonly Transform _playerBody, _enemyFocus;
    private bool _dodgeRight;

    public DuelAnimation(IDuelStage stage)
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        _playerBody = player != null ? player.transform : null;
        _player = player != null ? CharacterActions.Of(player) : null;
        _enemyFocus = stage?.Focus;
        _enemy = _enemyFocus != null ? CharacterActions.Of(_enemyFocus) : null;
        if (_enemy == null && stage is Component c) _enemy = c.GetComponentInChildren<CharacterActions>();
        _player?.PlayAny("CombatIdle");
    }

    public void PlayerAction(DuelAction action, DuelTarget target)
    {
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
    }

    public void PlayerDefense(DuelDefense defense)
    {
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

    // The warning: the enemy shows the blow before the response window opens.
    public void EnemyTelegraph(DuelMove move)
    {
        if (_enemy == null || move == null) return;
        string id = (move.Id ?? "").ToLowerInvariant();
        if (move.Origin == DuelTarget.HeadA) _enemy.PlayAny(id.Contains("sacud") ? "SacudidaA" : "AtaqueA", "Attack");
        else if (move.Origin == DuelTarget.HeadB) _enemy.PlayAny(id.Contains("puls") ? "PulsoB" : "AtaqueB", "Spin");
        else if (id.Contains("barr") || id.Contains("ala")) _enemy.PlayAny("Spin", "Embestida", "Attack");
        else if (id.Contains("onda") || id.Contains("vient") || id.Contains("union") || id.Contains("unión")) _enemy.PlayAny("PowerUp", "Rugido", "Attack");
        else _enemy.PlayAny("Combo", "Attack", "Embestida", "AtaqueA");
    }

    // After the response: a missed defense lands on Nemequene.
    public void Resolved(bool correct)
    {
        if (!correct) _player?.PlayAny("HitGut", "HitLeft");
    }

    public void EnemyHurt(DuelTarget target)
    {
        if (_enemy == null) return;
        if (target == DuelTarget.HeadA) _enemy.PlayAny("GolpeA", "HitLeft");
        else if (target == DuelTarget.HeadB) _enemy.PlayAny("GolpeB", "HitRight");
        else _enemy.PlayAny("HitGut", "HitLeft", "Golpe", "GolpeA");
    }

    public void Ended(bool victory)
    {
        if (victory) { _enemy?.PlayAny("Liberada", "Kneel"); _player?.Rest(); }
        else _player?.PlayAny("HitGut");
    }
}

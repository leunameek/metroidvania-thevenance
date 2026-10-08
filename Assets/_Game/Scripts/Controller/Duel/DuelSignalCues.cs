using System.Collections.Generic;
using UnityEngine;

// The signal language in the world (DuelSignal), the same in every duel and on top of each
// stage's own effects: body, sound and ground say what is coming, the HUD does not.
//  - Front: dust kicked up at the enemy's feet toward Nemequene and a low frame drum.
//  - Sweep: a streak of air crossing Nemequene from one side and a rising whistle.
//  - Above: a shadow growing under Nemequene, grit falling from above and a creak.
//  - Glint: when the answer opens, a golden flash on the enemy and a metal chime (Parar).
// Every sound also raises its caption, so the signal reaches players who play without sound.
public sealed class DuelSignalCues
{
    private static readonly Color Dust = new Color(.72f, .64f, .52f, .85f);
    private static readonly Color Air = new Color(.86f, .92f, 1f, .6f);
    private static readonly Color Grit = new Color(.5f, .47f, .44f, .9f);
    private static readonly Color Gold = new Color(1f, .82f, .35f, 1f);
    private static readonly Color Shade = new Color(.05f, .04f, .06f, .55f);

    private readonly Transform _player, _enemy;
    private TelegraphMark _shadow;
    private bool _sweepRight;

    // Side the last sweep comes from (+1 right of Nemequene, -1 left), for bodies that wind to it.
    public float SweepSide => _sweepRight ? 1f : -1f;

    public DuelSignalCues(Transform player, Transform enemy)
    {
        _player = player; _enemy = enemy;
    }

    private Vector3 PlayerFloor => _player != null ? TelegraphMark.Floor(_player.position, _player.position.y) : Vector3.zero;
    private Vector3 EnemyChest => _enemy != null ? _enemy.position + Vector3.up * 1.4f : PlayerFloor + Vector3.forward * 3f + Vector3.up * 1.4f;

    // The warning starts.
    public void Warn(DuelMove move)
    {
        Clear();
        if (move == null || _player == null) return;
        Vector3 feet = PlayerFloor;
        switch (move.Signal)
        {
            case DuelSignal.Front:
            {
                Vector3 from = _enemy != null ? TelegraphMark.Floor(_enemy.position, feet.y) : feet + Vector3.forward * 3f;
                Vector3 toward = feet - from; toward.y = 0;
                // Three puffs of dust from the enemy's feet toward Nemequene: the blow's straight line.
                for (int i = 0; i < 3; i++)
                    MIParticles.Burst(Vector3.Lerp(from, feet, .2f + .22f * i) + Vector3.up * .15f, Dust, 22, 1.2f, .22f, .1f);
                GameAudio.PlayAt("Combate/senal_frente", from + Vector3.up, .95f);
                GameAudio.Caption("Tambor grave");
                break;
            }
            case DuelSignal.Sweep:
            {
                _sweepRight = !_sweepRight;
                Vector3 side = Vector3.Cross(Vector3.up, Forward(feet)) * (_sweepRight ? 1f : -1f);
                // A line of air across his waist, from the side the blow will come from.
                for (int i = 0; i < 5; i++)
                    MIParticles.Burst(feet + Vector3.up * 1f + side * (2.2f - i * .9f), Air, 14, .8f, .12f, -.05f);
                GameAudio.PlayAt("Combate/senal_barrido", feet + side * 2f + Vector3.up, .95f);
                GameAudio.Caption("Silbido de lado");
                break;
            }
            default:
            {
                if (_shadow == null) _shadow = TelegraphMark.Create(null, "Sombra_Senal");
                _shadow.Show(feet, 1.5f, Shade);
                MIParticles.Burst(feet + Vector3.up * 4.5f, Grit, 30, .6f, .1f, 1f);
                GameAudio.PlayAt("Combate/senal_arriba", feet + Vector3.up * 4f, .95f);
                GameAudio.Caption("Crujido arriba");
                break;
            }
        }
    }

    // 0 at the warning, 1 when the blow lands: the shadow from above grows toward its edge.
    public void Progress(float k)
    {
        if (_shadow != null && _shadow.Visible) _shadow.SetProgress(k);
    }

    // The answer opens: a parriable blow flashes gold.
    public void Open(DuelMove move)
    {
        if (move == null || !move.Glint) return;
        MIParticles.Burst(EnemyChest, Gold, 36, 1.6f, .14f, -.2f);
        GameAudio.PlayAt("Combate/senal_destello", EnemyChest, 1f);
        GameAudio.Caption("Destello dorado");
    }

    public void Clear()
    {
        if (_shadow != null) _shadow.Hide();
    }

    public void Dispose()
    {
        if (_shadow != null) Object.Destroy(_shadow.gameObject);
        _shadow = null;
    }

    private Vector3 Forward(Vector3 feet)
    {
        Vector3 f = _enemy != null ? _enemy.position - feet : _player.forward;
        f.y = 0;
        return f.sqrMagnitude > .01f ? f.normalized : Vector3.forward;
    }
}

// Which signals this player has already read correctly (all slots: it is the player who learns,
// not the save). A signal not learned yet gets its one-time card in the duel HUD; the plaza
// training teaches Front and Sweep.
public static class DuelSignalMemory
{
    private const string Key = "Bacata.DuelSignals.v1";
    private static HashSet<string> _learned;

    public static IReadOnlyCollection<string> Learned
    {
        get
        {
            if (_learned != null) return _learned;
            _learned = new HashSet<string>();
            foreach (var k in PlayerPrefs.GetString(Key, "").Split(','))
                if (k.Length > 0) _learned.Add(k);
            return _learned;
        }
    }

    public static void Learn(params string[] keys)
    {
        var set = (HashSet<string>)Learned;
        bool changed = false;
        foreach (var k in keys) changed |= set.Add(k);
        if (!changed) return;
        PlayerPrefs.SetString(Key, string.Join(",", set));
        PlayerPrefs.Save();
    }

    public static void Learn(DuelMove move)
    {
        if (move == null) return;
        if (move.Glint) Learn(DuelSignals.Key(move.Signal), DuelSignals.GlintKey);
        else Learn(DuelSignals.Key(move.Signal));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _learned = null;
}

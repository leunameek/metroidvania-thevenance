using UnityEngine;

// The sound of a turn duel (guion 04): one place for every action, defense, warning, blow, turn
// change and ending, so the stages only add what belongs to their space (a gate, falling stones).
// Each enemy has its own voice and body: the caimán-murciélago bellows and hisses with leathery
// wings, the two-headed serpent hisses low (head A) or high (head B), the condor wheezes with
// heavy wings, the eagle cries, Quimue answers with gold (sun) or silver (moon). Staff blows sound
// on the enemy's material. While the turn listens for voice commands, music and beds step down so
// the microphone hears the player; the warnings are also written as sound captions.
public sealed class DuelAudio
{
    private sealed class Profile
    {
        public string Material, Presence, Warn, WarnA, WarnB, Attack, Hurt, Freed, Music;
        public string Caption, CaptionA, CaptionB;
    }

    private readonly Profile _p;
    private readonly Transform _focus;
    private readonly bool _ownDefeat;
    private AudioSource _presence;
    private GameObject _presenceHost;
    private AudioClip _previousMusic;
    private float _previousMusicVolume;

    public DuelAudio(string encounterId, Transform focus, bool worldHandlesDefeat)
    {
        _focus = focus;
        _ownDefeat = !worldHandlesDefeat;
        _p = For(encounterId);
        if (_focus != null && _p.Presence != null)
        {
            _presenceHost = new GameObject("VozDeCriatura");
            _presenceHost.transform.SetParent(_focus, false);
            _presenceHost.transform.localPosition = Vector3.up * 1.5f;
            _presence = GameAudio.Loop(_presenceHost, "Criaturas/" + _p.Presence, .55f, true, AudioChannel.Voice, 28f);
        }
        if (_p.Music != null)
        {
            var host = GameAudioHost.Current;
            if (host != null) { _previousMusic = host.MusicClip; _previousMusicVolume = host.MusicVolume; }
            GameAudio.Music("Musica/" + _p.Music, .9f, 2f);
        }
    }

    private static Profile For(string id)
    {
        switch (id)
        {
            case "E07": return new Profile { Material = "criatura", Presence = "caiman_presencia", Warn = "caiman_aviso", Attack = "caiman_ataque",
                Hurt = "caiman_herido", Freed = "caiman_liberado", Caption = "Gruñido del guardián" };
            case "E08": return new Profile { Material = "escamas", Presence = "serpiente_presencia", WarnA = "serpiente_aviso_a", WarnB = "serpiente_aviso_b",
                Warn = "serpiente_aviso_a", Attack = "serpiente_ataque", Hurt = "serpiente_herida", Freed = "serpiente_liberada",
                CaptionA = "Siseo grave · cabeza A", CaptionB = "Siseo agudo · cabeza B", Caption = "Siseo" };
            case "E09": return new Profile { Material = "plumas", Warn = "condor_aviso", Attack = "condor_ataque", Hurt = "condor_herido",
                Freed = "condor_liberado", Caption = "Resoplido y alas del cóndor" };
            case "E10": return new Profile { Material = "plumas", Warn = "aguila_grito", Attack = "aguila_ataque", Hurt = "aguila_herida",
                Freed = "aguila_liberada", Caption = "Grito del águila" };
            case "E11": return new Profile { Material = "espiritu", Presence = "quimue_zumbido", Warn = "quimue_aviso_luna", WarnA = "quimue_aviso_luna",
                WarnB = "quimue_aviso_sol", Attack = "quimue_ataque", Hurt = "quimue_herido", Freed = "quimue_rotura", Music = "musica_duelo_quimue",
                CaptionA = "Pulso lunar", CaptionB = "Pulso solar", Caption = "Zumbido de dos tonos" };
            default: return new Profile { Material = "criatura", Attack = "caiman_ataque" };
        }
    }

    private Vector3 At => _focus != null ? _focus.position + Vector3.up * 1.6f : Vector3.zero;

    private void Creature(string id, float volume = 1f)
    {
        if (id == null) return;
        if (_focus != null) GameAudio.PlayAt("Criaturas/" + id, At, volume, AudioChannel.Voice, 45f, 1f, .03f, .3f, 1);
        else GameAudio.Play("Criaturas/" + id, volume, AudioChannel.Voice, 1f, .03f, .3f, 1);
    }

    private void Impact(float delay, bool damaged)
    {
        string id = "Combate/impacto_" + _p.Material;
        if (_focus != null) GameAudio.PlayDelayed(id, delay, damaged ? 1f : .6f, AudioChannel.Effects, At);
        else GameAudio.PlayDelayed(id, delay, damaged ? 1f : .6f);
        if (damaged && _p.Hurt != null)
            GameAudioHost.Current?.Delay(delay + .12f, () => Creature(_p.Hurt, .8f));
    }

    // ------------------------------------------------------------------ duel events

    // A new turn: the player decides. While listening, the game steps down for the microphone.
    public void Decide(bool listening)
    {
        GameAudio.Play("Combate/turno_jugador", .7f, AudioChannel.Effects, 1f, 0f, .8f, 1);
        Listen(listening);
    }

    public void Listen(bool listening)
    {
        if (listening) GameAudio.SetDuck("escucha", DuckRequest.Listening);
        else GameAudio.ClearDuck("escucha");
    }

    public void Telegraph(DuelMove move)
    {
        if (move == null) return;
        bool b = move.Origin == DuelTarget.HeadB || move.Origin == DuelTarget.Sun;
        bool a = move.Origin == DuelTarget.HeadA || move.Origin == DuelTarget.Moon;
        string id = a && _p.WarnA != null ? _p.WarnA : b && _p.WarnB != null ? _p.WarnB : _p.Warn;
        Creature(id);
        // The caimán-murciélago's sweep comes with the bat's chitter: the wing announces the side.
        if (_p.Material == "criatura" && (move.Id ?? "").Contains("barr")) Creature("murcielago_chillido", .7f);
        if (_p.Material == "plumas" && _p.Warn == "condor_aviso") Creature("condor_alas", .6f);
        GameAudio.Caption(a && _p.CaptionA != null ? _p.CaptionA : b && _p.CaptionB != null ? _p.CaptionB : _p.Caption);
    }

    public void Defense(DuelDefense defense)
    {
        switch (defense)
        {
            case DuelDefense.Block: GameAudio.Play("Combate/bloqueo", .9f); break;
            case DuelDefense.Parry: GameAudio.Play("Combate/parada", .9f); break;
            case DuelDefense.Cover: GameAudio.Play("Combate/cubrirse", .9f); break;
            default: GameAudio.Play("Combate/esquiva", .9f); break;
        }
    }

    public void Resolved(bool correct)
    {
        Creature(_p.Attack, correct ? .7f : 1f);
        if (!correct) GameAudio.PlayDelayed("Combate/dano_recibido", .18f, .9f);
    }

    public void PlayerAction(DuelAction action, bool damaged)
    {
        switch (action)
        {
            case DuelAction.Attack:
                GameAudio.PlayDelayed("Combate/baston_aire", .12f, .9f);
                Impact(.38f, damaged);
                break;
            case DuelAction.Counter:
                GameAudio.Play("Combate/contraataque", .9f);
                Impact(.28f, false); Impact(.5f, damaged);
                break;
            case DuelAction.Interrupt:
                GameAudio.Play("Combate/interrumpir", .9f);
                Impact(.55f, damaged);
                break;
            case DuelAction.Anchor:
                GameAudio.PlayDelayed("Combate/anclar", .1f, .9f);
                break;
            case DuelAction.Bind:
                GameAudio.PlayDelayed("Combate/vincular", .1f, .9f);
                if (damaged) GameAudio.PlayDelayed("Combate/lazo_roto", .9f, .8f);
                break;
            case DuelAction.Horn:
                GameAudio.Play("MIAudio/cuerno", .9f, AudioChannel.Effects, 1f, 0f, .5f, 1);
                GameAudio.Caption("Cuerno responde");
                break;
            case DuelAction.Jaguar:
                // C08 stand-in: the warm pulse of the affinity; JaguarSpirit adds its call and lunge.
                GameAudio.Play("Combate/jaguar_pulso", .9f, AudioChannel.Effects, 1f, 0f, .5f, 1);
                GameAudio.Caption("Pulso cálido · Jaguar");
                Impact(1.25f, damaged);
                if (damaged) GameAudio.PlayDelayed("Combate/lazo_roto", 1.3f, .7f);
                break;
        }
    }

    // Victory frees the enemy (no triumph: guion C09); defeat in the plaza has no world flow.
    public void Ended(bool victory)
    {
        Listen(false);
        if (_presenceHost != null) Object.Destroy(_presenceHost);
        if (victory)
        {
            Creature(_p.Freed);
            if (_p.Material == "espiritu") GameAudio.Caption("Rotura de resonancia");
            if (_ownDefeat) GameAudio.Stinger("Musica/estinger_liberacion", .9f);
        }
        else if (_ownDefeat) GameAudio.Stinger("Musica/estinger_derrota", .9f);
        if (_p.Music != null)
        {
            if (_previousMusic != null) GameAudio.MusicClip(_previousMusic, _previousMusicVolume, 3f);
            else GameAudio.StopMusic(3f);
        }
    }
}

using UnityEngine;

// A25 «Jaguar»: the coca affinity calls the jaguar for one blow. It appears beside Nemequene in a
// burst of light, roars, crouches and springs at the enemy, lands and goes out in motes; the
// transformation cinematic waits for the final models (C08). The clips (Rugido, Embestida) are the
// IK-baked ones of tools/Blender/animate_creatures.py: the body travels only during the leap of
// the pounce, so the paws never slide. Needs Resources/Characters/Jaguar; without it nothing shows.
public sealed class JaguarSpirit : MonoBehaviour
{
    // Rugido lasts 1.47 s (it is cut at the end of the roar); Embestida 1.33 s, airborne 34-62 %.
    public const float Roar = 1.15f, Pounce = 1.33f, LeapFrom = .34f, LeapTo = .62f;
    // When the blow lands, counted from the call (DuelAnimation waits for it).
    public const float Impact = Roar + Pounce * LeapTo;
    private CharacterActions _actor;
    private Vector3 _from, _to;
    private float _t;
    private bool _pouncing, _gone;

    public static void Call(Transform player, Transform enemy)
    {
        Vector3 toEnemy = enemy.position - player.position; toEnemy.y = 0;
        if (toEnemy.sqrMagnitude < .01f) return;
        Vector3 side = Vector3.Cross(Vector3.up, toEnemy.normalized);
        var host = new GameObject("Jaguar (afinidad)").AddComponent<JaguarSpirit>();
        host.transform.position = player.position + side * 1.3f;
        host.transform.rotation = Quaternion.LookRotation(toEnemy);
        host._actor = CharacterModels.Spawn("Jaguar", host.transform, Vector3.zero, Quaternion.identity);
        if (host._actor == null) { Destroy(host.gameObject); return; }
        host._from = host.transform.position;
        host._to = enemy.position - toEnemy.normalized * 1.8f; host._to.y = host._from.y;
        host._actor.PlayAny("Rugido");
        GameAudio.PlayAt("Criaturas/jaguar_llamada", host._from + Vector3.up, 1f, AudioChannel.Voice, 40f);
        MIBurst.Spawn(host._from + Vector3.up * .6f, new Color(1f, .75f, .3f));
    }

    private void Update()
    {
        if (_gone) return;
        _t += Time.deltaTime;
        if (!_pouncing && _t >= Roar)
        {
            _pouncing = true;
            _actor.Play("Embestida", .12f);
            GameAudio.PlayAt("Combate/jaguar_embestida", transform.position + Vector3.up * .5f, .9f, AudioChannel.Effects, 35f);
        }
        if (_pouncing)
        {
            float k = Mathf.InverseLerp(LeapFrom, LeapTo, (_t - Roar) / Pounce);
            transform.position = Vector3.Lerp(_from, _to, Mathf.SmoothStep(0, 1, k));
        }
        if (_t > Roar + Pounce + .15f)
        {
            _gone = true;
            CreatureDissolve.Run(transform, new Color(1f, .78f, .35f), 0f, () => { if (this != null) Destroy(gameObject); });
        }
    }
}

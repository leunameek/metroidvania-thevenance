using UnityEngine;

// A25 «Jaguar»: the coca affinity calls the jaguar for one blow. It appears beside Nemequene with
// a roar, lunges at the enemy and fades out; the transformation cinematic waits for the final
// models (C08). Needs Resources/Characters/Jaguar; without it nothing is shown.
public sealed class JaguarSpirit : MonoBehaviour
{
    private const float Roar = .7f, Lunge = .55f, Stay = .5f;
    private CharacterActions _actor;
    private Vector3 _from, _to;
    private float _t;

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
        host._to = enemy.position - toEnemy.normalized * 1.6f; host._to.y = host._from.y;
        host._actor.PlayAny("Rugido");
        MIBurst.Spawn(host._from + Vector3.up * .6f, new Color(1f, .75f, .3f));
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t > Roar && _t - Time.deltaTime <= Roar) _actor.PlayAny("Embestida", "Correr");
        if (_t > Roar) transform.position = Vector3.Lerp(_from, _to, Mathf.SmoothStep(0, 1, (_t - Roar) / Lunge));
        if (_t > Roar + Lunge + Stay)
        {
            MIBurst.Spawn(transform.position + Vector3.up * .6f, new Color(1f, .75f, .3f));
            Destroy(gameObject);
        }
    }
}

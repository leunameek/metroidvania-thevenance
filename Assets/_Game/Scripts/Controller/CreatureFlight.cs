using UnityEngine;

// A short flight of a rigged bird (C11 the guacamaya leaving the urn): it takes off where it
// appears, flaps along a rising arc to the target and fades away there. Needs
// Resources/Characters/<key>; without the model nothing is shown.
public sealed class CreatureFlight : MonoBehaviour
{
    private const float TakeOff = 1.1f;
    private CharacterActions _actor;
    private Vector3 _from, _to;
    private float _t, _duration;

    public static void Launch(string key, Vector3 from, Vector3 to, float duration = 4.5f)
    {
        var host = new GameObject(key + " (vuelo)").AddComponent<CreatureFlight>();
        Vector3 flat = to - from; flat.y = 0;
        host.transform.SetPositionAndRotation(from, Quaternion.LookRotation(flat.sqrMagnitude > .01f ? flat : Vector3.forward));
        host._actor = CharacterModels.Spawn(key, host.transform, Vector3.zero, Quaternion.identity);
        if (host._actor == null) { Destroy(host.gameObject); return; }
        host._from = from; host._to = to; host._duration = duration;
        host._actor.PlayAny("Despegue", "Vuelo");
        // Its call on take-off and its wings along the flight (Criaturas/<key>_grito, _alas).
        string voice = "Criaturas/" + key.ToLowerInvariant();
        GameAudio.PlayAt(voice + "_grito", from + Vector3.up, .9f, AudioChannel.Voice, 40f);
        if (GameAudio.Has(voice + "_alas")) GameAudio.Loop(host.gameObject, voice + "_alas", .55f, true, AudioChannel.Voice, 35f);
        MIBurst.Spawn(from + Vector3.up * .6f, new Color(.95f, .45f, .25f));
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t > TakeOff && _t - Time.deltaTime <= TakeOff) _actor.PlayAny("Vuelo");
        float k = Mathf.Clamp01((_t - TakeOff * .5f) / _duration);
        Vector3 p = Vector3.Lerp(_from, _to, k * k * (3 - 2 * k));
        p.y += Mathf.Sin(k * Mathf.PI) * 1.5f;
        transform.position = p;
        if (k >= 1)
        {
            MIBurst.Spawn(transform.position, new Color(.95f, .45f, .25f));
            Destroy(gameObject);
        }
    }
}

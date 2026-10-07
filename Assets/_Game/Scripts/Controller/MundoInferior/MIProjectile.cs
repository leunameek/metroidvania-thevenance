using UnityEngine;

// The vigía's shot (E04): a slow glowing dart aimed where Nemequene stood when it left. Walls and
// the patio pillars stop it (the script's "Cubrir"), a step aside avoids it.
public sealed class MIProjectile : MonoBehaviour
{
    private const float Speed = 9f, Life = 4f, HitRadius = .75f;
    private Vector3 _direction;
    private float _damage, _age;
    private PlayerController _player;

    public static void Fire(Vector3 from, PlayerController player, float damage)
    {
        if (player == null) return;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.name = "Dardo del vigía";
        go.transform.position = from; go.transform.localScale = Vector3.one * .3f;
        var renderer = go.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) renderer.material = new Material(shader);
        renderer.material.color = new Color(.75f, .6f, 1f);
        var light = go.AddComponent<Light>(); light.type = LightType.Point; light.range = 3f; light.intensity = 3f; light.color = new Color(.7f, .55f, 1f);
        var p = go.AddComponent<MIProjectile>();
        p._player = player; p._damage = damage;
        p._direction = (player.transform.position + Vector3.up - from).normalized;
        MundoInferiorBlockout.AttemptReset += p.Vanish;
        MIAudio.PlayAt("ui_foco", from, .7f, .6f);
    }

    private void Update()
    {
        _age += Time.deltaTime;
        if (MundoInferiorBlockout.Instance != null && MundoInferiorBlockout.Instance.Busy) return;
        float step = Speed * Time.deltaTime;
        bool blocked = Physics.Raycast(transform.position, _direction, out var hit, step, ~0, QueryTriggerInteraction.Ignore);
        if (blocked && hit.collider.GetComponentInParent<PlayerController>() != null)
        {
            MundoInferiorBlockout.Instance?.Damage(_damage, transform.position);
            Vanish(); return;
        }
        if (_age > Life || blocked)
        {
            MIParticles.Burst(transform.position, new Color(.7f, .55f, 1f, .8f), 12, 2f, .06f, .2f);
            Vanish(); return;
        }
        transform.position += _direction * step;
        if (_player != null && Vector3.Distance(transform.position, _player.transform.position + Vector3.up) < HitRadius)
        {
            MundoInferiorBlockout.Instance?.Damage(_damage, transform.position);
            Vanish();
        }
    }

    private void Vanish()
    {
        MundoInferiorBlockout.AttemptReset -= Vanish;
        if (this != null) Destroy(gameObject);
    }

    private void OnDestroy() => MundoInferiorBlockout.AttemptReset -= Vanish;
}

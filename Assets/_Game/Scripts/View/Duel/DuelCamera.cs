using System.Collections.Generic;
using UnityEngine;

// A three-quarter view of a turn duel (2026-10-07 playtest: from behind and above, the guardian
// was hard to read). The camera stands off to one side behind Nemequene, high enough to see
// both, and frames the space between them; it follows his dodges and glides softly. The
// exploration camera is paused meanwhile and comes back where it was. Story lines during the duel
// take the camera as usual (StoryPlayer pauses this component and gives it back).
public sealed class DuelCamera : MonoBehaviour
{
    private Transform _player, _enemy;
    private float _side = 1f, _back = 4.2f, _height = 3.2f, _aside = 4.6f;
    private readonly List<Behaviour> _paused = new List<Behaviour>();
    private Vector3 _velocity;

    // side: +1 the camera on Nemequene's right, -1 on his left.
    public static DuelCamera Begin(Transform player, Transform enemy, float side = 1f)
    {
        var camera = Camera.main;
        if (camera == null || player == null || enemy == null) return null;
        var existing = camera.GetComponent<DuelCamera>();
        if (existing != null) existing.End();
        var duel = camera.gameObject.AddComponent<DuelCamera>();
        duel._player = player; duel._enemy = enemy; duel._side = side;
        foreach (var b in camera.GetComponents<Behaviour>())
        {
            if (b == null || b == duel || b is Camera || b is AudioListener || !b.enabled) continue;
            b.enabled = false; duel._paused.Add(b);
        }
        return duel;
    }

    public void End()
    {
        foreach (var b in _paused) if (b != null) b.enabled = true;
        _paused.Clear();
        var orbit = GetComponent<ExplorationOrbitCamera>();
        if (orbit != null) orbit.SnapAfterTeleport();
        Destroy(this);
    }

    private void LateUpdate()
    {
        if (_player == null || _enemy == null) { End(); return; }
        Vector3 player = _player.position, enemy = _enemy.position;
        Vector3 toEnemy = enemy - player; toEnemy.y = 0;
        if (toEnemy.sqrMagnitude < .01f) toEnemy = _player.forward;
        Vector3 forward = toEnemy.normalized, right = Vector3.Cross(Vector3.up, forward);
        float span = toEnemy.magnitude;
        // Further back and higher the farther apart they stand, so both stay in the frame.
        float pull = Mathf.Clamp(span / 7f, .8f, 1.6f);
        Vector3 eye = player - forward * _back * pull + right * _side * _aside * pull + Vector3.up * _height * pull;
        Vector3 look = Vector3.Lerp(player, enemy, .45f) + Vector3.up * 1.6f;
        transform.position = Vector3.SmoothDamp(transform.position, eye, ref _velocity, .45f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look - transform.position), 1f - Mathf.Exp(-Time.deltaTime * 5f));
    }
}

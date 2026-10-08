using System;
using System.Collections;
using UnityEngine;

// A hand interaction acted in time (2026-10-06 playtest: the lever moved before the arm did):
// Nemequene turns to the object, plays the gesture, and the effect (handle, gate, pickup, sound)
// happens at the moment of contact; input comes back when the gesture has finished.
public sealed class PlayerInteraction : MonoBehaviour
{
    // Moment of contact as a fraction of each gesture (Nemequene's Mixamo clips).
    private static float ContactOf(string state)
    {
        switch (state)
        {
            case "Lever": return .5f;
            case "Button": return .45f;
            case "Open": return .5f;
            case "Pickup": return .42f;
            case "Reach": return .45f;
            case "Kneel": return .55f;
            default: return .5f;
        }
    }

    private PlayerController _player;
    private Coroutine _running;
    private Action _pending;

    public static bool Busy(PlayerController player)
    {
        var p = player != null ? player.GetComponent<PlayerInteraction>() : null;
        return p != null && p._running != null;
    }

    // target may be null (no turn). atContact runs once, even when the character has no clip.
    public static void Perform(PlayerController player, Transform target, Action atContact, params string[] states)
    {
        if (player == null) { atContact?.Invoke(); return; }
        var p = player.GetComponent<PlayerInteraction>() ?? player.gameObject.AddComponent<PlayerInteraction>();
        p._player = player;
        if (p._running != null) { p.StopCoroutine(p._running); p._running = null; p.Flush(); player.SetInputLocked(false); }
        p._pending = atContact;
        p._running = p.StartCoroutine(p.Run(target, states));
    }

    // An interrupted gesture still applies its effect: the world never misses a lever.
    private void Flush()
    {
        var action = _pending; _pending = null;
        try { action?.Invoke(); }
        catch (Exception e) { Debug.LogException(e); }
    }

    private IEnumerator Run(Transform target, string[] states)
    {
        _player.SetInputLocked(true);
        // Turn to the object (a quick step of the shoulders, not a snap).
        if (target != null)
        {
            Vector3 to = target.position - _player.transform.position; to.y = 0;
            if (to.sqrMagnitude > .01f)
            {
                Quaternion from = _player.transform.rotation, look = Quaternion.LookRotation(to.normalized);
                for (float t = 0; t < .15f; t += Time.deltaTime)
                {
                    _player.transform.rotation = Quaternion.Slerp(from, look, t / .15f);
                    yield return null;
                }
                _player.transform.rotation = look;
            }
        }
        var actions = CharacterActions.Of(_player);
        string played = null;
        if (actions != null) foreach (var s in states) if (actions.Play(s, .18f)) { played = s; break; }
        float length = 0;
        if (played != null)
        {
            // The clip length is known once the cross-fade has begun.
            yield return null;
            var animator = actions.Animator;
            int hash = Animator.StringToHash(played);
            var next = animator.GetNextAnimatorStateInfo(0);
            var current = animator.GetCurrentAnimatorStateInfo(0);
            length = next.shortNameHash == hash ? next.length : current.shortNameHash == hash ? current.length : 1.2f;
            length = Mathf.Clamp(length, .4f, 3f);
            yield return new WaitForSeconds(length * ContactOf(played) - Time.deltaTime);
        }
        Flush();
        if (played != null) yield return new WaitForSeconds(length * (.85f - ContactOf(played)));
        // A gesture is not a pose: he gets up again (2026-10-07 playtest: after kneeling at a rest
        // disc the kneel was held as a final pose and he never stood up).
        if (played != null && actions != null && actions.Current == played) actions.Rest(.3f);
        _running = null;
        // Lines started by the effect keep the player still; they release the lock when they end.
        if (!StoryPlayer.Active) _player.SetInputLocked(false);
    }

    private void OnDisable()
    {
        if (_running == null || _player == null) return;
        _running = null;
        Flush();
        _player.SetInputLocked(false);
    }
}

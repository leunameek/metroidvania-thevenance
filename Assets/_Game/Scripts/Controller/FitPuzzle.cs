using Nemequene.UI;
using System.Collections;
using UnityEngine;

// Seating a piece in its table (2026-10-06 playtest: turning the pieces had no purpose). Each
// bracelet, ring, wing or coin rests on a table shaped for it. The inspection lifts it out turned
// at random; the player turns it until it sits as the table expects — there is no silhouette, only
// the piece's light warming and a soft resonance growing as it nears its place — and it seats with
// a click. Only a seated piece can be fitted, and only a fitted piece counts as found. Fitted, it
// stays in its table.
//  - Flat pieces (rings, discs, coins, bracelets seen edge-on) only need their face the right way;
//    the turn around that face is free. Other pieces need their whole orientation.
public sealed class FitPuzzle
{
    private const float SeatFlat = 14f, SeatFull = 20f, Unseat = 27f;
    private readonly Transform _item;
    private readonly Quaternion _rest;
    private readonly Vector3 _slot, _faceLocal;
    private readonly bool _flat;
    private float _bestBand = 999;

    public bool Seated { get; private set; }
    // 0 far, 1 in place (drives the piece's light).
    public float Closeness => Mathf.Clamp01(1f - Error / 150f);
    public Vector3 Slot => _slot;
    public Quaternion Rest => _rest;

    // item: the piece at its authored pose on the table (call before it starts to float and spin).
    public FitPuzzle(Transform item)
    {
        _item = item;
        _rest = item.rotation;
        var bounds = Bounds(item);
        // Thin axis at rest = the face of a flat piece.
        Vector3 e = bounds.extents;
        int thin = e.x <= e.y && e.x <= e.z ? 0 : e.y <= e.z ? 1 : 2;
        float min = thin == 0 ? e.x : thin == 1 ? e.y : e.z, max = Mathf.Max(e.x, Mathf.Max(e.y, e.z));
        _flat = max > 0 && min < max * .45f;
        Vector3 worldAxis = thin == 0 ? Vector3.right : thin == 1 ? Vector3.up : Vector3.forward;
        _faceLocal = Quaternion.Inverse(_rest) * worldAxis;
        // The slot: the piece lowered onto the surface right under it.
        Vector3 offset = item.position - bounds.center;
        _slot = item.position;
        foreach (var hit in Physics.RaycastAll(bounds.center, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(item)) continue;
            Vector3 seat = new Vector3(bounds.center.x, hit.point.y + e.y * .85f, bounds.center.z) + offset;
            if (seat.y < _slot.y && _slot.y - seat.y < 2.5f) _slot = seat;
        }
    }

    // How far the piece is from its place, in degrees.
    public float Error => _flat ? Vector3.Angle(_item.rotation * _faceLocal, _rest * _faceLocal) : Quaternion.Angle(_item.rotation, _rest);

    // Lifted out, turned at random (well away from its place).
    public void Scramble()
    {
        Seated = false; _bestBand = 999;
        Vector3 axis = Random.onUnitSphere;
        if (_flat) axis = Vector3.ProjectOnPlane(axis, _rest * _faceLocal).normalized;
        if (axis.sqrMagnitude < .01f) axis = Vector3.right;
        _item.rotation = Quaternion.AngleAxis(Random.Range(105f, 165f), axis) * _rest * Quaternion.AngleAxis(Random.Range(0f, 360f), _faceLocal);
    }

    // Called each inspection frame after the player's turn: seats, holds, and sounds the resonance.
    public void Tick(Light glow, float baseGlow, Vector3 soundAt)
    {
        float error = Error;
        if (!Seated && error < (_flat ? SeatFlat : SeatFull))
        {
            Seated = true;
            GameAudio.PlayAt("Foley/objeto_soltar", soundAt, .9f, AudioChannel.Effects, 20f);
            MIAudio.PlayAt("golpe_piedra", soundAt, .45f, 1.6f);
            MIParticles.Burst(soundAt, new Color(1f, .82f, .45f, .9f), 26, 1.4f, .06f, -.2f);
        }
        else if (Seated && error > Unseat) Seated = false;
        // A seated piece is drawn into place (the face, for a flat one; the whole pose otherwise).
        if (Seated)
        {
            Quaternion target = _flat ? Quaternion.FromToRotation(_item.rotation * _faceLocal, _rest * _faceLocal) * _item.rotation : _rest;
            _item.rotation = Quaternion.Slerp(_item.rotation, target, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 12f));
        }
        // The light warms as the piece nears its place; each 20° closer, a soft resonance.
        if (glow != null) glow.intensity = baseGlow * Mathf.Lerp(.35f, 2.4f, Seated ? 1f : Closeness * Closeness);
        float band = Mathf.Floor(error / 20f);
        if (band < _bestBand)
        {
            if (_bestBand < 999 && !Seated) MIAudio.PlayAt("ui_foco", soundAt, .35f, Mathf.Lerp(1.6f, .8f, band / 8f));
            _bestBand = band;
        }
    }

    // Fitted: the piece goes down into its table and settles with its authored pose.
    public IEnumerator Settle(float seconds = .45f)
    {
        Vector3 from = _item.position; Quaternion turn = _item.rotation;
        for (float t = 0; t < seconds; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0, 1, t / seconds);
            _item.SetPositionAndRotation(Vector3.Lerp(from, _slot, k), Quaternion.Slerp(turn, _rest, k));
            yield return null;
        }
        PlaceFitted();
        MIAudio.PlayAt("golpe_piedra", _slot, .7f, 1.2f);
        MIParticles.Burst(_slot + Vector3.up * .1f, new Color(1f, .8f, .4f, .9f), 50, 2f, .08f, -.3f);
    }

    // A piece fitted in an earlier visit: already in its table.
    public void PlaceFitted() { _item.SetPositionAndRotation(_slot, _rest); }

    private static Bounds Bounds(Transform root)
    {
        bool any = false; var b = new Bounds(root.position, Vector3.one * .2f);
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return b;
    }
}

// What the find inspections share around the fitting: the panel's status line, the refusal of a
// piece that is not seated yet, and Nemequene stepping out of the inspection shot.
public static class FitView
{
    public static string Status(bool seated) => seated
        ? "Encaja: está en su posición. " + (VoicePrompt.Enabled ? "Di «tomar» o pulsa E para encajarla en su mesa." : "Pulsa E para encajarla en su mesa.")
        : "Gíralo hasta que encaje en su mesa: su luz se aviva al acercarte y suena al asentar.";

    public static void NotYet(MIHud hud)
    {
        GameAudio.UI(UICue.Blocked);
        hud?.SetInspectionFit("Aún no encaja. Sigue girándola: su luz se aviva al acercarte y suena al asentar.");
    }

    public static Renderer[] HidePlayer(PlayerController player)
    {
        if (player == null) return null;
        var hidden = new System.Collections.Generic.List<Renderer>();
        foreach (var r in player.GetComponentsInChildren<Renderer>()) if (r.enabled) { r.enabled = false; hidden.Add(r); }
        return hidden.ToArray();
    }

    public static void ShowPlayer(Renderer[] hidden)
    {
        if (hidden == null) return;
        foreach (var r in hidden) if (r != null) r.enabled = true;
    }
}

public static class InspectionFraming
{
    // Horizontal direction from the piece toward the inspection eye: the player's side first, then
    // turning around the piece until no wall or rock stands between the eye and it.
    public static Vector3 ClearSide(Vector3 piece, Vector3 camera, float distance, Transform item, Transform player)
    {
        Vector3 side = camera - piece; side.y = 0;
        side = side.sqrMagnitude < .01f ? Vector3.back : side.normalized;
        foreach (float angle in new[] { 0f, 40f, -40f, 80f, -80f, 120f, -120f, 180f })
        {
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * side;
            Vector3 eye = piece + dir * distance + Vector3.up * .25f;
            bool clear = true;
            foreach (var hit in Physics.RaycastAll(piece, eye - piece, Vector3.Distance(piece, eye), ~0, QueryTriggerInteraction.Ignore))
            {
                if ((item != null && hit.collider.transform.IsChildOf(item)) || (player != null && hit.collider.transform.IsChildOf(player))) continue;
                clear = false; break;
            }
            if (clear) return dir;
        }
        return side;
    }
}

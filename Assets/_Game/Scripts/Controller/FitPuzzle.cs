using Nemequene.UI;
using System.Collections;
using UnityEngine;

// Seating a piece in its table (2026-10-06 playtest: turning the pieces had no purpose). Each
// bracelet, ring, wing or coin rests on a table shaped for it. The inspection lifts it out turned
// at random and holds it just above its table; the player turns it until it sits as the table
// expects. A golden sketch of the piece is drawn in the table, exactly where and how it must rest
// (2026-10-07 playtests: some pieces were too hard to place without one, and a figure floating
// above the piece did not say where it went). Near its pose the piece is drawn in by itself, the
// sketch brightens and a resonance sounds, and it seats with a click. Only a seated piece can be
// fitted, and only a fitted piece counts as found. Fitted, it stays in its table.
//  - Flat pieces (rings, discs, coins, bracelets seen edge-on) only need their face the right way;
//    the turn around that face is free. Other pieces need their whole orientation.
public sealed class FitPuzzle
{
    // Seating is generous and the last stretch helps (2026-10-07 playtest: too hard to hit).
    private const float SeatFlat = 22f, SeatFull = 28f, Unseat = 38f, Assist = 50f;
    private readonly Transform _item;
    private readonly Quaternion _rest;
    private readonly Vector3 _slot, _faceLocal;
    private readonly bool _flat;
    private float _bestBand = 999;
    private Transform _guide;
    private Material _guideMaterial, _lineMaterial;
    private float _flash, _height;
    private bool _near;

    public bool Seated { get; private set; }
    // 0 far, 1 in place (drives the piece's light).
    public float Closeness => Mathf.Clamp01(1f - Error / 150f);
    public Vector3 Slot => _slot;
    public Quaternion Rest => _rest;
    // Where the piece is held while it is turned: just above its table, so both are seen.
    public Vector3 Hover => _slot + Vector3.up * (_height * 1.3f + .22f);
    // What the inspection camera looks at: the held piece and its table.
    public Vector3 FramePoint => Vector3.Lerp(Hover, _slot, .45f);

    // item: the piece at its authored pose on the table (call before it starts to float and spin).
    // slot: where it rests, when its table has no collider to find it (the bracelets' cradle).
    public FitPuzzle(Transform item, Vector3? slot = null)
    {
        _item = item;
        _rest = item.rotation;
        var bounds = Bounds(item);
        // Thin axis at rest = the face of a flat piece.
        Vector3 e = bounds.extents;
        _height = Mathf.Min(bounds.size.y, .6f);
        int thin = e.x <= e.y && e.x <= e.z ? 0 : e.y <= e.z ? 1 : 2;
        float min = thin == 0 ? e.x : thin == 1 ? e.y : e.z, max = Mathf.Max(e.x, Mathf.Max(e.y, e.z));
        _flat = max > 0 && min < max * .45f;
        Vector3 worldAxis = thin == 0 ? Vector3.right : thin == 1 ? Vector3.up : Vector3.forward;
        _faceLocal = Quaternion.Inverse(_rest) * worldAxis;
        // The slot: the piece lowered onto its table. Each part of it (each bracelet of a pair)
        // looks straight down for the first surface under it, and the piece rests on the highest
        // of them: its cradle, not a lower step or the floor beneath the table between them.
        Vector3 offset = item.position - bounds.center;
        _slot = item.position;
        if (slot.HasValue) { _slot = slot.Value; return; }
        float surface = float.MinValue;
        foreach (var r in item.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer || !r.enabled) continue;
            float first = float.MaxValue, y = float.MinValue;
            foreach (var hit in Physics.RaycastAll(r.bounds.center, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(item) || hit.distance >= first) continue;
                first = hit.distance; y = hit.point.y;
            }
            if (y > surface) surface = y;
        }
        if (surface > float.MinValue)
        {
            Vector3 seat = new Vector3(bounds.center.x, surface + e.y * .85f, bounds.center.z) + offset;
            if (seat.y < item.position.y && item.position.y - seat.y < 2.5f) _slot = seat;
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
        // Close to its pose, the piece is drawn in a little by itself, more the closer it is.
        if (!Seated && error < Assist)
        {
            Quaternion target = _flat ? Quaternion.FromToRotation(_item.rotation * _faceLocal, _rest * _faceLocal) * _item.rotation : _rest;
            float pull = (1f - error / Assist) * 3.2f;
            _item.rotation = Quaternion.Slerp(_item.rotation, target, 1f - Mathf.Exp(-Time.unscaledDeltaTime * pull));
        }
        // Coming near announces itself: a bright flash of the sketch and a clear chime.
        bool near = error < Assist;
        if (near && !_near && !Seated)
        {
            _flash = 1f;
            MIAudio.PlayAt("ui_foco", soundAt, .7f, 1.35f);
            MIParticles.Burst(_slot + Vector3.up * .05f, new Color(1f, .85f, .5f, .8f), 18, 1f, .05f, -.3f);
        }
        _near = near;
        // A seated piece is drawn into the very pose of its sketch: a flat one seats as soon as its
        // face is right, then turns about that face until it matches the drawing (2026-10-07
        // playtest: pieces seated turned otherwise than the sketch showed).
        if (Seated) _item.rotation = Quaternion.Slerp(_item.rotation, _rest, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 9f));
        Guide(!Seated);
        // The light warms as the piece nears its place; each 20° closer, a soft resonance.
        if (glow != null) glow.intensity = baseGlow * Mathf.Lerp(.35f, 2.4f, Seated ? 1f : Closeness * Closeness);
        float band = Mathf.Floor(error / 20f);
        if (band < _bestBand)
        {
            if (_bestBand < 999 && !Seated) { MIAudio.PlayAt("ui_foco", soundAt, .5f, Mathf.Lerp(1.6f, .8f, band / 8f)); _flash = Mathf.Max(_flash, .5f); }
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

    // ---------- the guide: a golden sketch of the piece in its table, in the pose it must take ----------

    private void Guide(bool visible)
    {
        if (_guide == null)
        {
            if (!visible) return;
            BuildGuide();
            if (_guide == null) return;
        }
        if (_guide.gameObject.activeSelf != visible) _guide.gameObject.SetActive(visible);
        if (!visible) return;
        _guide.SetPositionAndRotation(_slot, _rest);
        _flash = Mathf.MoveTowards(_flash, 0, Time.unscaledDeltaTime * 1.6f);
        if (_guideMaterial == null) return;
        // It breathes slowly when far, and glows brighter as the piece nears its pose.
        float near = Mathf.SmoothStep(0, 1, 1f - Mathf.Clamp01(Error / 90f));
        float breath = .5f + .5f * Mathf.Sin(Time.unscaledTime * (3f + near * 5f));
        float glow = Mathf.Clamp01(near * .75f + _flash * .8f + breath * .12f);
        if (_guideMaterial.HasProperty("_Glow")) _guideMaterial.SetFloat("_Glow", glow);
        else _guideMaterial.SetColor("_BaseColor", new Color(1f, .82f, .42f, .35f + .4f * glow));
        if (_lineMaterial != null) _lineMaterial.SetFloat("_Glow", glow);
    }

    public void HideGuide()
    {
        if (_guide != null) Object.Destroy(_guide.gameObject);
        _guide = null;
        if (_guideMaterial != null) Object.Destroy(_guideMaterial);
        if (_lineMaterial != null) Object.Destroy(_lineMaterial);
        _guideMaterial = null; _lineMaterial = null;
    }

    private void BuildGuide()
    {
        var root = new GameObject("Guía de encaje").transform;
        var toItem = _item.worldToLocalMatrix;
        var material = GuideMaterial();
        // The line around the fill (drawn after it, only outside it).
        var lineShader = Resources.Load<Shader>("Effects/FitSketchLine");
        var line = lineShader != null && material.shader.name == "Nemequene/Fit Sketch" ? new Material(lineShader) { name = "Guia_Encaje_Linea" } : null;
        if (line != null) line.SetColor("_Color", new Color(1f, .82f, .42f, .6f));
        foreach (var filter in _item.GetComponentsInChildren<MeshFilter>())
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || !renderer.enabled || filter.sharedMesh == null) continue;
            var relative = toItem * filter.transform.localToWorldMatrix;
            var part = new GameObject(filter.name).transform;
            part.SetParent(root, false);
            part.localPosition = relative.GetColumn(3);
            part.localRotation = relative.rotation;
            part.localScale = relative.lossyScale;
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var r = part.gameObject.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            if (line == null) continue;
            var edge = new GameObject("Línea").transform;
            edge.SetParent(part, false);
            edge.gameObject.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var lr = edge.gameObject.AddComponent<MeshRenderer>();
            lr.sharedMaterial = line;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; lr.receiveShadows = false;
        }
        if (root.childCount == 0) { Object.Destroy(root.gameObject); Object.Destroy(material); if (line != null) Object.Destroy(line); return; }
        // The piece's own size: it is the place it will fill.
        root.localScale = _item.lossyScale;
        _guide = root; _guideMaterial = material; _lineMaterial = line;
    }

    private static Material GuideMaterial()
    {
        var sketch = Resources.Load<Shader>("Effects/FitSketch");
        // Soft: a drawing of the place, never as strong as the piece itself.
        if (sketch != null) { var soft = new Material(sketch) { name = "Guia_Encaje" }; soft.SetColor("_Color", new Color(1f, .82f, .42f, .45f)); return soft; }
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        var m = new Material(shader) { name = "Guia_Encaje" };
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = 3050;
        m.SetColor("_BaseColor", new Color(1f, .82f, .42f, .5f));
        return m;
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
        ? "Encaja: está en su posición. " + (VoicePrompt.Enabled ? "Di «tomar» o pulsa " + GameBindings.Cap(GameAction.Interact) + " para encajarla en su mesa." : "Pulsa " + GameBindings.Cap(GameAction.Interact) + " para encajarla en su mesa.")
        : "Gírala hasta que coincida con el dibujo dorado de su mesa: brilla más al acercarte y encaja sola al final.";

    public static void NotYet(MIHud hud)
    {
        GameAudio.UI(UICue.Blocked);
        hud?.SetInspectionFit("Aún no encaja. Sigue girándola hasta que coincida con el dibujo dorado de su mesa.");
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

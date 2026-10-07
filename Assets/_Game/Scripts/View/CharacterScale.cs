using UnityEngine;

// One size per character in every scene. Each scene placed Nemequene's model with its own scale
// (2.83 m in the plaza, 1.96 m in the worlds, 1.78 m in Bacatá), so next to the cast he looked
// a giant in one place and small in another. The player visual and the cast prefabs measure the
// body mesh in its bind pose and scale it to the canonical height.
public static class CharacterScale
{
    public const float Nemequene = 1.85f;

    // Height of the body (largest skinned mesh hanging from the model root, so worn equipment such
    // as the wings is left out), in world units, independent of the animation playing.
    public static float BodyHeight(Transform model)
    {
        SkinnedMeshRenderer body = null; int best = -1;
        foreach (var s in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (s.sharedMesh == null) continue;
            int score = s.sharedMesh.vertexCount + (s.transform.parent == model ? 1000000 : 0);
            if (score > best) { best = score; body = s; }
        }
        if (body == null) return 0;
        var b = body.sharedMesh.bounds;
        var m = body.transform.localToWorldMatrix;
        float lo = float.MaxValue, hi = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            float y = m.MultiplyPoint3x4(corner).y;
            lo = Mathf.Min(lo, y); hi = Mathf.Max(hi, y);
        }
        return hi - lo;
    }

    // Scales `model` about its own origin (the feet) so the body measures `height` metres.
    public static void Fit(Transform model, float height)
    {
        float current = BodyHeight(model);
        if (current > .01f) model.localScale *= height / current;
    }
}

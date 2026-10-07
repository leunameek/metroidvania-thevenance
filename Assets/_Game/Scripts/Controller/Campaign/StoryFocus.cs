using System;
using System.Collections.Generic;
using UnityEngine;

// What a line of the story is about, when it is something in the place and not the speaker: the
// story camera looks at it for that line (by the line's cue, as in historia.json). A scene
// registers its points when it builds; the function may also set the moment in motion (the crack
// that warns before giving way lets a stone fall while it is said).
public static class StoryFocus
{
    private static readonly Dictionary<string, Func<Vector3?>> Points = new Dictionary<string, Func<Vector3?>>();

    public static void Register(string cue, Func<Vector3?> point) { if (!string.IsNullOrEmpty(cue)) Points[cue] = point; }
    public static void Unregister(string cue) { if (!string.IsNullOrEmpty(cue)) Points.Remove(cue); }

    public static Vector3? Find(string cue)
    {
        if (string.IsNullOrEmpty(cue) || !Points.TryGetValue(cue, out var point) || point == null) return null;
        return point();
    }
}

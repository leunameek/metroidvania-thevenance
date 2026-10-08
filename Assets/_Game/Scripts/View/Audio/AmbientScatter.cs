using System.Collections.Generic;
using UnityEngine;

// Details of an ambience played now and then around the listener (birds over the plaza, stones
// settling in the cave, a distant raptor above the clouds): each entry waits a random gap, then
// plays one variant at a random point between the given distances. Paused games wait too.
public sealed class AmbientScatter : MonoBehaviour
{
    private sealed class Entry
    {
        public string Id;
        public float MinGap, MaxGap, Near, Far, Volume, Height, Next;
    }

    private readonly List<Entry> _entries = new List<Entry>();

    public static AmbientScatter On(GameObject owner)
    {
        var scatter = owner.GetComponent<AmbientScatter>();
        return scatter != null ? scatter : owner.AddComponent<AmbientScatter>();
    }

    public AmbientScatter Add(string id, float minGap, float maxGap, float near, float far, float volume, float height = 2f)
    {
        _entries.Add(new Entry { Id = id, MinGap = minGap, MaxGap = maxGap, Near = near, Far = far, Volume = volume, Height = height,
            Next = Time.time + Random.Range(minGap * .3f, maxGap) });
        return this;
    }

    public void Clear() => _entries.Clear();

    private void Update()
    {
        var listener = Camera.main != null ? Camera.main.transform : null;
        if (listener == null) return;
        foreach (var e in _entries)
        {
            if (Time.time < e.Next) continue;
            e.Next = Time.time + Random.Range(e.MinGap, e.MaxGap);
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(e.Near, e.Far);
            Vector3 at = listener.position + new Vector3(dir.x, e.Height + Random.Range(-1f, 1f), dir.y);
            GameAudio.PlayAt(e.Id, at, e.Volume * Random.Range(.7f, 1f), AudioChannel.Ambience, e.Far * 1.8f, 1f, .06f, .5f, 2);
        }
    }
}

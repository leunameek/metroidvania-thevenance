using System;
using System.Collections.Generic;
using UnityEngine;

// The story text of the campaign (Resources/Narrative/historia.json, built from the editable
// script by tools/narrative/build_story_json.py). Plain data so JsonUtility can read it.
[Serializable]
public sealed class StoryLine
{
    public string speaker;
    public string note;
    public string text;
    public string cue;
}

[Serializable]
public sealed class StorySequence
{
    public string id;
    public string title;
    public string place;
    public string summary;
    public string objective;
    public string storyboard;
    public string[] requires = new string[0];
    public string[] sets = new string[0];
    public StoryLine[] lines = new StoryLine[0];

    public bool Available(CampaignModel campaign)
    {
        foreach (var f in requires) if (!campaign.Has(f)) return false;
        return true;
    }
    public bool Seen(CampaignModel campaign) => campaign.Has(SeenFlag(id));
    public static string SeenFlag(string sequenceId) => "seen_" + sequenceId;

    // Lines said at one moment of play ("" = every line of the sequence, in order).
    public List<StoryLine> Cue(string cue)
    {
        var result = new List<StoryLine>();
        foreach (var line in lines)
            if (string.IsNullOrEmpty(cue) || line.cue == cue) result.Add(line);
        return result;
    }
}

[Serializable]
public sealed class StoryHint
{
    public string id;
    public string speaker;
    public string question;
    public string answer;
    public string rule;
}

[Serializable]
public sealed class StoryScript
{
    public int version;
    public string source;
    public StorySequence[] sequences = new StorySequence[0];
    public StoryHint[] contextual = new StoryHint[0];

    public static StoryScript Parse(string json)
    {
        if (string.IsNullOrEmpty(json)) return new StoryScript();
        try { return JsonUtility.FromJson<StoryScript>(json) ?? new StoryScript(); }
        catch (Exception) { return new StoryScript(); }
    }

    public StorySequence Get(string id)
    {
        foreach (var s in sequences) if (s.id == id) return s;
        return null;
    }
    public StoryHint Hint(string id)
    {
        foreach (var h in contextual) if (h.id == id) return h;
        return null;
    }

    // Marks a sequence as played (or skipped): the same flags in both cases (guion C "Salto").
    public static void Complete(CampaignModel campaign, StorySequence sequence)
    {
        if (sequence == null) return;
        foreach (var f in sequence.sets) campaign.Set(f);
        campaign.Set(StorySequence.SeenFlag(sequence.id));
    }

    // Journal "Historia": sequences already lived, in campaign order.
    public List<StorySequence> Lived(CampaignModel campaign)
    {
        var result = new List<StorySequence>();
        foreach (var s in sequences) if (s.Seen(campaign)) result.Add(s);
        return result;
    }
}

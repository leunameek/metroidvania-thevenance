using System;
using System.Collections.Generic;

public sealed class ExplorationObjectiveModel
{
    private readonly HashSet<string> _required;
    private readonly HashSet<string> _analyzed = new HashSet<string>();
    public int AnalyzedCount => _analyzed.Count;
    public int RequiredCount => _required.Count;
    public bool IsComplete => RequiredCount > 0 && AnalyzedCount == RequiredCount;
    public event Action Changed;

    public ExplorationObjectiveModel(IEnumerable<string> requiredIds)
    {
        _required = new HashSet<string>();
        foreach (string id in requiredIds)
            if (!string.IsNullOrWhiteSpace(id)) _required.Add(id);
    }

    public bool Analyze(string id)
    {
        if (id == null || !_required.Contains(id) || !_analyzed.Add(id)) return false;
        Changed?.Invoke();
        return true;
    }
}

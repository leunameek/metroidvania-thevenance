using UnityEngine;

public enum CulturalEvidenceStatus { PendingValidation, HistoricalValidated, ArtisticInterpretation, GameplayAdaptation, Fiction }

[CreateAssetMenu(menuName = "Nemequene/Week08/Analyzable object")]
public sealed class AnalyzableObjectData : ScriptableObject
{
    public string objectId;
    public string displayName;
    [TextArea(3, 7)] public string description;
    public CulturalEvidenceStatus evidenceStatus = CulturalEvidenceStatus.PendingValidation;
    [TextArea] public string sourceOrValidationNote = "Placeholder sin identificación cultural; pendiente de investigación.";
}

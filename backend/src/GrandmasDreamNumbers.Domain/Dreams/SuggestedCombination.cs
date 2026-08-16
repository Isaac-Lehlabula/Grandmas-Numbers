using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

public class SuggestedCombination : BaseEntity
{
    public Guid DreamAnalysisId { get; set; }
    public required int[] Numbers { get; set; }
    public required string CombinationType { get; set; }

    public DreamAnalysis? DreamAnalysis { get; set; }
}

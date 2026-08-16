using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

public class DreamAnalysis : BaseEntity
{
    public Guid DreamId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public required string ModelUsed { get; set; }
    public string? RawAnalysisResponse { get; set; }

    public Dream? Dream { get; set; }
    public List<DreamSymbolMatch> Matches { get; set; } = [];
    public List<SuggestedCombination> SuggestedCombinations { get; set; } = [];
}

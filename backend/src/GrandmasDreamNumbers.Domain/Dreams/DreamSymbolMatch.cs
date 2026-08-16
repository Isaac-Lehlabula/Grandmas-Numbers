using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

public class DreamSymbolMatch : BaseEntity
{
    public Guid DreamAnalysisId { get; set; }
    public Guid DreamSymbolId { get; set; }
    public required string MatchedText { get; set; }
    public double ConfidenceScore { get; set; }
    public required string Explanation { get; set; }

    public DreamAnalysis? DreamAnalysis { get; set; }
    public DreamSymbol? DreamSymbol { get; set; }
}

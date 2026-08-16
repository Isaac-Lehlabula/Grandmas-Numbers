namespace GrandmasDreamNumbers.Application.Dreams.Dtos;

public sealed record SubmitDreamRequest(string DreamText);

public sealed record DreamSymbolMatchResponse(
    Guid SymbolId,
    string SymbolName,
    string MatchedText,
    string TraditionalMeaning,
    string Explanation,
    double ConfidenceScore,
    IReadOnlyCollection<int> LuckyNumbers);

public sealed record SuggestedCombinationResponse(IReadOnlyCollection<int> Numbers, string CombinationType);

public sealed record DreamAnalysisResponse(
    Guid DreamId,
    Guid DreamAnalysisId,
    string OriginalText,
    string Summary,
    IReadOnlyCollection<DreamSymbolMatchResponse> Matches,
    IReadOnlyCollection<SuggestedCombinationResponse> SuggestedCombinations,
    string Disclaimer,
    DateTimeOffset CreatedAt);

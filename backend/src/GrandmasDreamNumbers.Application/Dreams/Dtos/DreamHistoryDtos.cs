namespace GrandmasDreamNumbers.Application.Dreams.Dtos;

public sealed record DreamSummaryResponse(
    Guid Id,
    string OriginalText,
    string? Summary,
    DateTimeOffset CreatedAt,
    bool HasAnalysis);

public sealed record DreamSymbolSearchResultResponse(Guid Id, string Name, string Description);

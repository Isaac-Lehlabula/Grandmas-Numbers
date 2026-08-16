namespace GrandmasDreamNumbers.Application.Common.Interfaces;

/// <summary>
/// A symbol the AI is allowed to reference. Only symbol IDs already present
/// in the database are ever passed in, and only IDs from this set are
/// accepted back - see the validation step in DreamAnalysisService.
/// </summary>
public sealed record AvailableDreamSymbol(
    Guid SymbolId,
    string Name,
    string Description,
    IReadOnlyCollection<string> Aliases);

public sealed record DreamSymbolInterpretationMatch(
    Guid SymbolId,
    string MatchedText,
    double ConfidenceScore,
    string Explanation);

public sealed record DreamInterpretationResult(
    string Summary,
    IReadOnlyCollection<DreamSymbolInterpretationMatch> Matches);

/// <summary>
/// Provides natural-language interpretation of a dream. Implementations
/// may only identify and explain symbols - they must never invent lucky
/// numbers, and any symbol ID they return that isn't in
/// <paramref name="availableSymbols"/> is discarded by the caller.
/// </summary>
public interface IDreamInterpretationService
{
    Task<DreamInterpretationResult> InterpretAsync(
        string dreamText,
        IReadOnlyCollection<AvailableDreamSymbol> availableSymbols,
        CancellationToken cancellationToken);
}

using System.Text.Json;
using GrandmasDreamNumbers.Application.Common.Exceptions;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrandmasDreamNumbers.Application.Dreams.Services;

/// <summary>
/// Orchestrates a dream analysis: normalize, match directly/via alias
/// first, only then ask the AI about whatever's left, validate every
/// AI-returned symbol ID against the database, load lucky numbers, build
/// suggested combinations, and persist the whole result.
/// </summary>
public class DreamAnalysisService(
    IApplicationDbContext dbContext,
    IDreamInterpretationService interpretationService,
    DreamSymbolMatcher matcher,
    CombinationGenerator combinationGenerator,
    IOptions<DreamAnalysisSettings> settings)
{
    public async Task<DreamAnalysisResponse> AnalyseAsync(
        Guid userId, SubmitDreamRequest request, CancellationToken cancellationToken)
    {
        var dream = new Dream
        {
            UserId = userId,
            OriginalText = request.DreamText.Trim(),
        };

        dbContext.Dreams.Add(dream);

        return await RunAnalysisAsync(dream, cancellationToken);
    }

    public async Task<DreamAnalysisResponse> ReanalyseAsync(
        Guid userId, Guid dreamId, CancellationToken cancellationToken)
    {
        var dream = await dbContext.Dreams
            .FirstOrDefaultAsync(d => d.Id == dreamId, cancellationToken)
            ?? throw new NotFoundException($"Dream '{dreamId}' was not found.");

        if (dream.UserId != userId)
        {
            throw new ForbiddenAccessException("You can't re-analyse another user's dream.");
        }

        return await RunAnalysisAsync(dream, cancellationToken);
    }

    private async Task<DreamAnalysisResponse> RunAnalysisAsync(Dream dream, CancellationToken cancellationToken)
    {
        var activeSymbols = await dbContext.DreamSymbols
            .Include(s => s.Aliases)
            .Include(s => s.LuckyNumbers)
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        var directMatches = matcher.FindDirectMatches(dream.OriginalText, activeSymbols);
        var directlyMatchedIds = directMatches.Select(m => m.SymbolId).ToHashSet();

        var remainingSymbols = activeSymbols
            .Where(s => !directlyMatchedIds.Contains(s.Id))
            .Select(s => new AvailableDreamSymbol(s.Id, s.Name, s.Description, s.Aliases.Select(a => a.Alias).ToArray()))
            .ToList();

        DreamInterpretationResult? aiResult = null;
        if (remainingSymbols.Count > 0)
        {
            aiResult = await interpretationService.InterpretAsync(dream.OriginalText, remainingSymbols, cancellationToken);
        }

        // Only symbols that exist in the DB and weren't already matched
        // directly are accepted - anything else the AI returns is discarded.
        var validSymbolIds = activeSymbols.Select(s => s.Id).ToHashSet();
        var aiMatches = (aiResult?.Matches ?? [])
            .Where(m => validSymbolIds.Contains(m.SymbolId) && !directlyMatchedIds.Contains(m.SymbolId))
            .GroupBy(m => m.SymbolId)
            .Select(g => g.First())
            .ToList();

        var symbolsById = activeSymbols.ToDictionary(s => s.Id);

        var combinedMatches = directMatches
            .Select(m => (Symbol: symbolsById[m.SymbolId], m.MatchedText, m.ConfidenceScore, m.Explanation))
            .Concat(aiMatches.Select(m => (Symbol: symbolsById[m.SymbolId], m.MatchedText, m.ConfidenceScore, m.Explanation)))
            .ToList();

        var analysis = new DreamAnalysis
        {
            DreamId = dream.Id,
            ModelUsed = aiResult is not null ? "Mock" : "DirectMatchOnly",
            RawAnalysisResponse = aiResult is not null ? JsonSerializer.Serialize(aiResult) : null,
        };

        var luckyNumbersForCombinations = new List<MatchedLuckyNumber>();
        var matchResponses = new List<DreamSymbolMatchResponse>();

        foreach (var (symbol, matchedText, confidence, explanation) in combinedMatches)
        {
            analysis.Matches.Add(new DreamSymbolMatch
            {
                DreamAnalysisId = analysis.Id,
                DreamSymbolId = symbol.Id,
                MatchedText = matchedText,
                ConfidenceScore = confidence,
                Explanation = explanation,
            });

            luckyNumbersForCombinations.AddRange(
                symbol.LuckyNumbers.Select(n => new MatchedLuckyNumber(symbol.Id, n.Number, n.IsPrimary)));

            matchResponses.Add(new DreamSymbolMatchResponse(
                symbol.Id,
                symbol.Name,
                matchedText,
                symbol.TraditionalMeaning,
                explanation,
                confidence,
                symbol.LuckyNumbers.Select(n => n.Number).ToArray()));
        }

        var combinations = combinationGenerator.Generate(
            luckyNumbersForCombinations,
            settings.Value.CombinationSize,
            settings.Value.MaxSuggestedCombinations);

        foreach (var combination in combinations)
        {
            analysis.SuggestedCombinations.Add(new SuggestedCombination
            {
                DreamAnalysisId = analysis.Id,
                Numbers = combination.Numbers.ToArray(),
                CombinationType = combination.CombinationType,
            });
        }

        // The AI is only ever shown symbols that weren't already matched
        // directly, so its summary only speaks for the whole dream when it
        // actually found something itself, or there was nothing else
        // already matched for it to be blind to.
        var useAiSummary = !string.IsNullOrWhiteSpace(aiResult?.Summary)
            && (aiMatches.Count > 0 || directMatches.Count == 0);

        var summary = useAiSummary ? aiResult!.Summary : BuildFallbackSummary(combinedMatches.Count);

        dream.Summary = summary;
        dbContext.DreamAnalyses.Add(analysis);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DreamAnalysisResponse(
            dream.Id,
            analysis.Id,
            dream.OriginalText,
            summary,
            matchResponses,
            combinations.Select(c => new SuggestedCombinationResponse(c.Numbers, c.CombinationType)).ToList(),
            DisclaimerText.Text,
            analysis.CreatedAt);
    }

    private static string BuildFallbackSummary(int matchCount) => matchCount > 0
        ? "Gogo recognized some familiar symbols in this dream."
        : "Gogo couldn't find a familiar symbol in this dream this time.";
}

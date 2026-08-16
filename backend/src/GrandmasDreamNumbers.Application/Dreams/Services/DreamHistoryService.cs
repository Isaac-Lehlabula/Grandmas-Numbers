using GrandmasDreamNumbers.Application.Common.Exceptions;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Application.Dreams.Services;

public class DreamHistoryService(IApplicationDbContext dbContext)
{
    public async Task<IReadOnlyCollection<DreamSummaryResponse>> GetHistoryAsync(
        Guid userId, string? searchQuery, CancellationToken cancellationToken)
    {
        var query = dbContext.Dreams.Where(d => d.UserId == userId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var normalizedQuery = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(d =>
                d.OriginalText.ToLower().Contains(normalizedQuery)
                || (d.Summary != null && d.Summary.ToLower().Contains(normalizedQuery)));
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DreamSummaryResponse(d.Id, d.OriginalText, d.Summary, d.CreatedAt, d.Analyses.Any()))
            .ToListAsync(cancellationToken);
    }

    public async Task<DreamAnalysisResponse> GetDetailAsync(
        Guid userId, Guid dreamId, CancellationToken cancellationToken)
    {
        var dream = await dbContext.Dreams
            .Include(d => d.Analyses).ThenInclude(a => a.Matches).ThenInclude(m => m.DreamSymbol).ThenInclude(s => s!.LuckyNumbers)
            .Include(d => d.Analyses).ThenInclude(a => a.SuggestedCombinations)
            .FirstOrDefaultAsync(d => d.Id == dreamId, cancellationToken)
            ?? throw new NotFoundException($"Dream '{dreamId}' was not found.");

        EnsureOwnership(dream, userId);

        var analysis = dream.Analyses.OrderByDescending(a => a.CreatedAt).FirstOrDefault()
            ?? throw new NotFoundException($"Dream '{dreamId}' has not been analysed yet.");

        var matchResponses = analysis.Matches.Select(m => new DreamSymbolMatchResponse(
            m.DreamSymbolId,
            m.DreamSymbol?.Name ?? "Unknown",
            m.MatchedText,
            m.DreamSymbol?.TraditionalMeaning ?? string.Empty,
            m.Explanation,
            m.ConfidenceScore,
            m.DreamSymbol?.LuckyNumbers.Select(n => n.Number).ToArray() ?? [])).ToList();

        var combinationResponses = analysis.SuggestedCombinations
            .Select(c => new SuggestedCombinationResponse(c.Numbers, c.CombinationType))
            .ToList();

        return new DreamAnalysisResponse(
            dream.Id,
            analysis.Id,
            dream.OriginalText,
            dream.Summary ?? string.Empty,
            matchResponses,
            combinationResponses,
            DisclaimerText.Text,
            analysis.CreatedAt);
    }

    public async Task DeleteAsync(Guid userId, Guid dreamId, CancellationToken cancellationToken)
    {
        var dream = await dbContext.Dreams
            .FirstOrDefaultAsync(d => d.Id == dreamId, cancellationToken)
            ?? throw new NotFoundException($"Dream '{dreamId}' was not found.");

        EnsureOwnership(dream, userId);

        dbContext.Dreams.Remove(dream);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureOwnership(Dream dream, Guid userId)
    {
        if (dream.UserId != userId)
        {
            throw new ForbiddenAccessException("You can't access another user's dream.");
        }
    }
}

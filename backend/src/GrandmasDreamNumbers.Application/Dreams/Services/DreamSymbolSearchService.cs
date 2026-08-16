using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Application.Dreams.Services;

/// <summary>
/// Backs the authenticated symbol-search endpoint. Deliberately returns
/// only name/description - never the lucky numbers - so this can't be used
/// to dump the private cheat sheet.
/// </summary>
public class DreamSymbolSearchService(IApplicationDbContext dbContext)
{
    private const int MaxResults = 20;

    public async Task<IReadOnlyCollection<DreamSymbolSearchResultResponse>> SearchAsync(
        string? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await dbContext.DreamSymbols
            .Where(s => s.IsActive)
            .Where(s => s.Name.ToLower().Contains(normalizedQuery)
                || s.Aliases.Any(a => a.Alias.ToLower().Contains(normalizedQuery)))
            .OrderBy(s => s.Name)
            .Take(MaxResults)
            .Select(s => new DreamSymbolSearchResultResponse(s.Id, s.Name, s.Description))
            .ToListAsync(cancellationToken);
    }
}

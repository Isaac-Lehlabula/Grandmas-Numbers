using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Application.Common.Interfaces;

/// <summary>
/// Persistence seam so Application-layer services can query and persist
/// without depending on the concrete EF Core DbContext in Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Dream> Dreams { get; }
    DbSet<DreamSymbol> DreamSymbols { get; }
    DbSet<DreamSymbolAlias> DreamSymbolAliases { get; }
    DbSet<LuckyNumber> LuckyNumbers { get; }
    DbSet<DreamAnalysis> DreamAnalyses { get; }
    DbSet<DreamSymbolMatch> DreamSymbolMatches { get; }
    DbSet<SuggestedCombination> SuggestedCombinations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

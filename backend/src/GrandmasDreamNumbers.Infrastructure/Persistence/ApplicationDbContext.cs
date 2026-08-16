using System.Reflection;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Domain.Dreams;
using GrandmasDreamNumbers.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Dream> Dreams => Set<Dream>();
    public DbSet<DreamSymbol> DreamSymbols => Set<DreamSymbol>();
    public DbSet<DreamSymbolAlias> DreamSymbolAliases => Set<DreamSymbolAlias>();
    public DbSet<LuckyNumber> LuckyNumbers => Set<LuckyNumber>();
    public DbSet<DreamAnalysis> DreamAnalyses => Set<DreamAnalysis>();
    public DbSet<DreamSymbolMatch> DreamSymbolMatches => Set<DreamSymbolMatch>();
    public DbSet<SuggestedCombination> SuggestedCombinations => Set<SuggestedCombination>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

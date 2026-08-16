using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only sample data. These four symbols are placeholders for
/// demoing/testing the matching pipeline - replace them with the real
/// grandmother's cheat sheet before any real deployment (see
/// docs/cheat-sheet-import.md once that's written).
/// </summary>
public static class DevelopmentSeeder
{
    private static readonly string[] Roles = ["Admin", "User"];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(roleManager);
        await SeedDreamSymbolsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task SeedDreamSymbolsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.DreamSymbols.AnyAsync(cancellationToken))
        {
            return;
        }

        var symbols = new[]
        {
            CreateSampleSymbol(
                "Snake",
                "A snake, serpent or similar reptile appearing in the dream.",
                "Conflict, danger or an enemy.",
                ["serpent", "cobra", "python"],
                [(17, true), (42, false)]),
            CreateSampleSymbol(
                "River",
                "Flowing or moving water in the dream.",
                "Journey, transition or movement.",
                ["stream", "flowing water"],
                [(8, true), (29, false)]),
            CreateSampleSymbol(
                "Money",
                "Cash, coins or banknotes appearing in the dream.",
                "Wealth, opportunity or financial concerns.",
                ["cash", "coins", "banknotes"],
                [(11, true), (75, false)]),
            CreateSampleSymbol(
                "Baby",
                "An infant or newborn appearing in the dream.",
                "A new beginning.",
                ["infant", "newborn"],
                [(15, true), (39, false)]),
        };

        dbContext.DreamSymbols.AddRange(symbols);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DreamSymbol CreateSampleSymbol(
        string name,
        string description,
        string traditionalMeaning,
        string[] aliases,
        (int Number, bool IsPrimary)[] luckyNumbers)
    {
        var symbol = new DreamSymbol
        {
            Name = name,
            Description = description,
            TraditionalMeaning = traditionalMeaning,
        };

        symbol.Aliases = aliases
            .Select(alias => new DreamSymbolAlias { Alias = alias, DreamSymbolId = symbol.Id })
            .ToList();

        symbol.LuckyNumbers = luckyNumbers
            .Select(n => new LuckyNumber { Number = n.Number, IsPrimary = n.IsPrimary, DreamSymbolId = symbol.Id })
            .ToList();

        return symbol;
    }
}

using FluentAssertions;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Application.Dreams.Services;
using GrandmasDreamNumbers.Domain.Dreams;
using GrandmasDreamNumbers.Infrastructure.Persistence;
using GrandmasDreamNumbers.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrandmasDreamNumbers.UnitTests.Dreams.Services;

public class DreamAnalysisServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static DreamSymbol CreateSymbol(string name, int[] numbers)
    {
        var symbol = new DreamSymbol { Name = name, Description = "d", TraditionalMeaning = "m", IsActive = true };
        symbol.LuckyNumbers = numbers
            .Select((n, index) => new LuckyNumber { Number = n, IsPrimary = index == 0, DreamSymbolId = symbol.Id })
            .ToList();
        return symbol;
    }

    private static DreamAnalysisService CreateService(ApplicationDbContext dbContext, IDreamInterpretationService ai) =>
        new(dbContext, ai, new DreamSymbolMatcher(), new CombinationGenerator(), Options.Create(new DreamAnalysisSettings()));

    [Fact]
    public async Task AnalyseAsync_IgnoresAiReturnedSymbolIdNotInDatabase()
    {
        using var dbContext = CreateDbContext();
        var riverSymbol = CreateSymbol("River", [8, 29]);
        dbContext.DreamSymbols.Add(riverSymbol);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var bogusSymbolId = Guid.NewGuid();
        var ai = new FakeDreamInterpretationService(new DreamInterpretationResult(
            "summary",
            [new DreamSymbolInterpretationMatch(bogusSymbolId, "ghost", 0.9, "not a real symbol")]));

        var service = CreateService(dbContext, ai);

        var response = await service.AnalyseAsync(
            Guid.NewGuid(), new SubmitDreamRequest("I saw something strange"), CancellationToken.None);

        response.Matches.Should().BeEmpty();
        response.SuggestedCombinations.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyseAsync_LuckyNumbersOnlyComeFromDatabase()
    {
        using var dbContext = CreateDbContext();
        var riverSymbol = CreateSymbol("River", [8, 29]);
        dbContext.DreamSymbols.Add(riverSymbol);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var ai = new FakeDreamInterpretationService(new DreamInterpretationResult(
            "A river flowed through the dream",
            [new DreamSymbolInterpretationMatch(riverSymbol.Id, "river", 0.8, "matches River")]));

        var service = CreateService(dbContext, ai);

        var response = await service.AnalyseAsync(
            Guid.NewGuid(), new SubmitDreamRequest("Something about water"), CancellationToken.None);

        var allNumbersReturned = response.SuggestedCombinations.SelectMany(c => c.Numbers)
            .Concat(response.Matches.SelectMany(m => m.LuckyNumbers))
            .Distinct();

        allNumbersReturned.Should().BeSubsetOf([8, 29]);
        response.Matches.Should().ContainSingle(m => m.SymbolId == riverSymbol.Id);
    }

    [Fact]
    public async Task AnalyseAsync_DirectMatch_SkipsAiForThatSymbol()
    {
        using var dbContext = CreateDbContext();
        var snakeSymbol = CreateSymbol("Snake", [17, 42]);
        dbContext.DreamSymbols.Add(snakeSymbol);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var aiCalls = 0;
        var ai = new CountingFakeInterpretationService(() => aiCalls++);

        var service = CreateService(dbContext, ai);

        var response = await service.AnalyseAsync(
            Guid.NewGuid(), new SubmitDreamRequest("A snake chased me"), CancellationToken.None);

        response.Matches.Should().ContainSingle(m => m.SymbolId == snakeSymbol.Id && m.ConfidenceScore == 1.0);
        aiCalls.Should().Be(0, "the only active symbol was already matched directly, so the AI shouldn't be called");
    }

    [Fact]
    public async Task AnalyseAsync_DirectMatchFound_SummaryReflectsIt_EvenWhenAiFindsNothingForRemainder()
    {
        using var dbContext = CreateDbContext();
        var snakeSymbol = CreateSymbol("Snake", [17, 42]);
        var babySymbol = CreateSymbol("Baby", [15, 39]);
        dbContext.DreamSymbols.AddRange(snakeSymbol, babySymbol);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // The AI is only asked about "Baby" (Snake was already matched
        // directly) and, correctly, finds nothing there.
        var ai = new FakeDreamInterpretationService(new DreamInterpretationResult(
            "This dream doesn't clearly reference any known symbol.", []));

        var service = CreateService(dbContext, ai);

        var response = await service.AnalyseAsync(
            Guid.NewGuid(), new SubmitDreamRequest("A snake chased me"), CancellationToken.None);

        response.Matches.Should().ContainSingle(m => m.SymbolId == snakeSymbol.Id);
        response.Summary.Should().NotBe(
            "This dream doesn't clearly reference any known symbol.",
            "the AI's summary only covers what it was asked about, not the symbol already matched directly");
    }

    private sealed class CountingFakeInterpretationService(Action onCalled) : IDreamInterpretationService
    {
        public Task<DreamInterpretationResult> InterpretAsync(
            string dreamText, IReadOnlyCollection<AvailableDreamSymbol> availableSymbols, CancellationToken cancellationToken)
        {
            onCalled();
            return Task.FromResult(new DreamInterpretationResult("summary", []));
        }
    }
}

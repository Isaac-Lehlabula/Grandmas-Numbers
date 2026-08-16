using FluentAssertions;
using GrandmasDreamNumbers.Application.Dreams.Services;

namespace GrandmasDreamNumbers.UnitTests.Dreams.Services;

public class CombinationGeneratorTests
{
    private readonly CombinationGenerator _generator = new();

    [Fact]
    public void Generate_NoDuplicateNumbersWithinACombination()
    {
        var symbolA = Guid.NewGuid();
        var numbers = new[]
        {
            new MatchedLuckyNumber(symbolA, 1, true),
            new MatchedLuckyNumber(symbolA, 2, false),
            new MatchedLuckyNumber(symbolA, 3, false),
        };

        var combinations = _generator.Generate(numbers, combinationSize: 3, maxCombinations: 5);

        combinations.Should().AllSatisfy(c => c.Numbers.Should().OnlyHaveUniqueItems());
    }

    [Fact]
    public void Generate_DedupesRepeatedNumbersAcrossSymbols()
    {
        var symbolA = Guid.NewGuid();
        var symbolB = Guid.NewGuid();
        var numbers = new[]
        {
            new MatchedLuckyNumber(symbolA, 17, true),
            new MatchedLuckyNumber(symbolB, 17, false),
            new MatchedLuckyNumber(symbolB, 42, false),
        };

        var combinations = _generator.Generate(numbers, combinationSize: 3, maxCombinations: 5);

        var allNumbersUsed = combinations.SelectMany(c => c.Numbers).ToHashSet();
        allNumbersUsed.Should().BeEquivalentTo([17, 42]);
    }

    [Fact]
    public void Generate_NeverExceedsRequestedMaxCombinations()
    {
        var symbolA = Guid.NewGuid();
        var numbers = Enumerable.Range(1, 20)
            .Select(n => new MatchedLuckyNumber(symbolA, n, false))
            .ToArray();

        var combinations = _generator.Generate(numbers, combinationSize: 4, maxCombinations: 5);

        combinations.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void Generate_RespectsConfigurableCombinationSize()
    {
        var symbolA = Guid.NewGuid();
        var numbers = Enumerable.Range(1, 10)
            .Select(n => new MatchedLuckyNumber(symbolA, n, false))
            .ToArray();

        var combinations = _generator.Generate(numbers, combinationSize: 4, maxCombinations: 3);

        combinations.Should().AllSatisfy(c => c.Numbers.Should().HaveCount(4));
    }

    [Fact]
    public void Generate_PrefersPrimaryNumbers()
    {
        var symbolA = Guid.NewGuid();
        var numbers = new[]
        {
            new MatchedLuckyNumber(symbolA, 1, false),
            new MatchedLuckyNumber(symbolA, 2, false),
            new MatchedLuckyNumber(symbolA, 99, true),
        };

        var combinations = _generator.Generate(numbers, combinationSize: 1, maxCombinations: 1);

        combinations.Single().Numbers.Should().Contain(99);
    }

    [Fact]
    public void Generate_NoLuckyNumbers_ReturnsEmpty()
    {
        var combinations = _generator.Generate([], combinationSize: 6, maxCombinations: 5);

        combinations.Should().BeEmpty();
    }
}

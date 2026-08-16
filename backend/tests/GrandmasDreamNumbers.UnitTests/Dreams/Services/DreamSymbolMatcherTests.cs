using FluentAssertions;
using GrandmasDreamNumbers.Application.Dreams.Services;
using GrandmasDreamNumbers.Domain.Dreams;

namespace GrandmasDreamNumbers.UnitTests.Dreams.Services;

public class DreamSymbolMatcherTests
{
    private readonly DreamSymbolMatcher _matcher = new();

    [Fact]
    public void FindDirectMatches_ExactSymbolName_ReturnsMatch()
    {
        var snake = CreateSymbol("Snake", ["serpent", "cobra"]);

        var matches = _matcher.FindDirectMatches("I dreamed a snake was chasing me", [snake]);

        matches.Should().ContainSingle(m => m.SymbolId == snake.Id && m.MatchedText == "Snake");
    }

    [Fact]
    public void FindDirectMatches_Alias_ReturnsMatch()
    {
        var snake = CreateSymbol("Snake", ["serpent", "cobra"]);

        var matches = _matcher.FindDirectMatches("A cobra slithered across the floor", [snake]);

        matches.Should().ContainSingle(m => m.SymbolId == snake.Id && m.MatchedText == "cobra");
    }

    [Fact]
    public void FindDirectMatches_NoMention_ReturnsNoMatch()
    {
        var snake = CreateSymbol("Snake", ["serpent", "cobra"]);

        var matches = _matcher.FindDirectMatches("I was flying over mountains", [snake]);

        matches.Should().BeEmpty();
    }

    [Fact]
    public void FindDirectMatches_WordInsideAnotherWord_DoesNotMatch()
    {
        var cat = CreateSymbol("Cat", []);

        var matches = _matcher.FindDirectMatches("I went shopping at the catalog store", [cat]);

        matches.Should().BeEmpty();
    }

    [Fact]
    public void FindDirectMatches_MultipleSymbols_ReturnsOneMatchPerSymbol()
    {
        var snake = CreateSymbol("Snake", ["serpent"]);
        var river = CreateSymbol("River", ["stream"]);

        var matches = _matcher.FindDirectMatches(
            "A snake was near the river", [snake, river]);

        matches.Should().HaveCount(2);
        matches.Select(m => m.SymbolId).Should().BeEquivalentTo([snake.Id, river.Id]);
    }

    private static DreamSymbol CreateSymbol(string name, string[] aliases)
    {
        var symbol = new DreamSymbol { Name = name, Description = "d", TraditionalMeaning = "m" };
        symbol.Aliases = aliases
            .Select(alias => new DreamSymbolAlias { Alias = alias, DreamSymbolId = symbol.Id })
            .ToList();
        return symbol;
    }
}

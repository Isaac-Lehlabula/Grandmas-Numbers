using System.Text.RegularExpressions;
using GrandmasDreamNumbers.Application.Common.Interfaces;

namespace GrandmasDreamNumbers.Infrastructure.Ai;

/// <summary>
/// Deterministic, offline stand-in for a real AI provider. It only ever
/// looks at the symbol names/aliases it's handed and never returns a symbol
/// ID outside that set, so the project runs end to end without an API key.
/// Replace with a real provider by adding another case in
/// <see cref="DependencyInjection.AddInfrastructureServices"/> behind the
/// "Ai:Provider" configuration value.
/// </summary>
public sealed class MockDreamInterpretationService : IDreamInterpretationService
{
    public Task<DreamInterpretationResult> InterpretAsync(
        string dreamText,
        IReadOnlyCollection<AvailableDreamSymbol> availableSymbols,
        CancellationToken cancellationToken)
    {
        var matches = new List<DreamSymbolInterpretationMatch>();

        foreach (var symbol in availableSymbols)
        {
            var keywords = new[] { symbol.Name }.Concat(symbol.Aliases);
            var matchedKeyword = keywords.FirstOrDefault(keyword => ContainsWholeWord(dreamText, keyword));

            if (matchedKeyword is not null)
            {
                matches.Add(new DreamSymbolInterpretationMatch(
                    symbol.SymbolId,
                    matchedKeyword,
                    0.75,
                    $"The mention of \"{matchedKeyword}\" suggests the {symbol.Name} symbol: {symbol.Description}"));
            }
        }

        var summary = matches.Count > 0
            ? $"This dream touches on {string.Join(", ", matches.Select(m => m.MatchedText))}."
            : "This dream doesn't clearly reference any known symbol.";

        return Task.FromResult(new DreamInterpretationResult(summary, matches));
    }

    private static bool ContainsWholeWord(string text, string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);
    }
}

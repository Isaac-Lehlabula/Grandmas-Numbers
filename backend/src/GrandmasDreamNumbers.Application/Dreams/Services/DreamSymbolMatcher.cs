using System.Text.RegularExpressions;
using GrandmasDreamNumbers.Domain.Dreams;

namespace GrandmasDreamNumbers.Application.Dreams.Services;

public sealed record DirectMatchResult(
    Guid SymbolId,
    string MatchedText,
    double ConfidenceScore,
    string Explanation);

/// <summary>
/// Pure exact/alias matcher. Runs before any AI call is considered, per the
/// business rule that direct and alias matching always happens first.
/// </summary>
public sealed class DreamSymbolMatcher
{
    public IReadOnlyCollection<DirectMatchResult> FindDirectMatches(
        string dreamText, IReadOnlyCollection<DreamSymbol> activeSymbols)
    {
        var normalizedText = Normalize(dreamText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return [];
        }

        var results = new List<DirectMatchResult>();

        foreach (var symbol in activeSymbols)
        {
            var candidates = new List<string> { symbol.Name };
            candidates.AddRange(symbol.Aliases.Select(a => a.Alias));

            foreach (var candidate in candidates)
            {
                var normalizedCandidate = Normalize(candidate);
                if (string.IsNullOrWhiteSpace(normalizedCandidate))
                {
                    continue;
                }

                if (ContainsWholeWord(normalizedText, normalizedCandidate))
                {
                    results.Add(new DirectMatchResult(
                        symbol.Id,
                        candidate,
                        1.0,
                        $"\"{candidate}\" in your dream matches the {symbol.Name} symbol."));
                    break;
                }
            }
        }

        return results;
    }

    private static bool ContainsWholeWord(string normalizedText, string normalizedCandidate)
    {
        var pattern = $@"\b{Regex.Escape(normalizedCandidate)}\b";
        return Regex.IsMatch(normalizedText, pattern, RegexOptions.IgnoreCase);
    }

    private static string Normalize(string value) => value.Trim();
}

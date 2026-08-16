namespace GrandmasDreamNumbers.Application.Dreams.Services;

public sealed record MatchedLuckyNumber(Guid DreamSymbolId, int Number, bool IsPrimary);

public sealed record GeneratedCombination(IReadOnlyCollection<int> Numbers, string CombinationType);

/// <summary>
/// Builds suggested number combinations strictly from matched lucky numbers.
/// Never invents numbers - only rearranges what was already loaded from the
/// database for the symbols matched in this dream.
/// </summary>
public sealed class CombinationGenerator
{
    private const string DefaultCombinationType = "Balanced";

    public IReadOnlyCollection<GeneratedCombination> Generate(
        IReadOnlyCollection<MatchedLuckyNumber> luckyNumbers,
        int combinationSize,
        int maxCombinations)
    {
        if (luckyNumbers.Count == 0 || combinationSize <= 0 || maxCombinations <= 0)
        {
            return [];
        }

        var pool = BuildInterleavedPool(luckyNumbers);
        var effectiveSize = Math.Min(combinationSize, pool.Count);

        var seenCombinations = new HashSet<string>();
        var combinations = new List<GeneratedCombination>();

        for (var offset = 0; offset < maxCombinations; offset++)
        {
            var combo = new int[effectiveSize];
            for (var position = 0; position < effectiveSize; position++)
            {
                combo[position] = pool[(offset + position) % pool.Count];
            }

            var dedupeKey = string.Join(",", combo.OrderBy(n => n));
            if (seenCombinations.Add(dedupeKey))
            {
                combinations.Add(new GeneratedCombination(combo, DefaultCombinationType));
            }
        }

        return combinations;
    }

    /// <summary>
    /// Dedupes numbers, keeps a number "primary" if any matched symbol
    /// marks it primary, and interleaves across symbols (primaries first
    /// within each symbol) so combinations pull from different symbols
    /// rather than exhausting one before touching the next.
    /// </summary>
    private static List<int> BuildInterleavedPool(IReadOnlyCollection<MatchedLuckyNumber> luckyNumbers)
    {
        var bySymbol = new Dictionary<Guid, List<(int Number, bool IsPrimary)>>();
        var seenNumbers = new HashSet<int>();

        foreach (var luckyNumber in luckyNumbers)
        {
            if (!seenNumbers.Add(luckyNumber.Number))
            {
                continue;
            }

            if (!bySymbol.TryGetValue(luckyNumber.DreamSymbolId, out var numbers))
            {
                numbers = [];
                bySymbol[luckyNumber.DreamSymbolId] = numbers;
            }

            numbers.Add((luckyNumber.Number, luckyNumber.IsPrimary));
        }

        var groups = bySymbol.Values
            .Select(group => group.OrderByDescending(n => n.IsPrimary).Select(n => n.Number).ToList())
            .ToList();

        var pool = new List<int>();
        var maxPerSymbol = groups.Count == 0 ? 0 : groups.Max(g => g.Count);

        for (var column = 0; column < maxPerSymbol; column++)
        {
            foreach (var group in groups)
            {
                if (column < group.Count)
                {
                    pool.Add(group[column]);
                }
            }
        }

        return pool;
    }
}

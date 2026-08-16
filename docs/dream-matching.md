# Dream matching and lucky-number rules

This is the part of the spec with the hardest constraint: **the AI may
identify and explain symbols, but a lucky number must only ever come from
the database.** Everything below exists to make that true mechanically, not
just by convention.

## 1. Direct/alias matching (`DreamSymbolMatcher`)

`Application/Dreams/Services/DreamSymbolMatcher.cs`. Pure function, no
database or network access — takes the dream text and the already-loaded
list of active `DreamSymbol` entities (with their `Aliases` collections
populated) and returns matches.

Algorithm:

1. Normalize the dream text (trim only — matching is case-insensitive via
   the regex, so no lowercasing is needed).
2. For each active symbol, build a candidate list: the symbol's `Name` plus
   every `DreamSymbolAlias.Alias`.
3. For each candidate, test the dream text with a word-boundary,
   case-insensitive regex: `\b{escaped candidate}\b`. This is why "cat"
   doesn't match inside "catalog" — `\b` requires a real word boundary on
   both sides, not just a substring.
4. First candidate that matches wins for that symbol (one match per symbol
   from direct matching); confidence is always `1.0` since it's an exact
   textual match, not an inference.

This runs **before** any AI call, per the business rule "perform direct and
alias matching before calling an AI model."

## 2. AI-assisted matching (only for what's left)

Only symbols that direct/alias matching did **not** already find get passed
to `IDreamInterpretationService.InterpretAsync` — as `AvailableDreamSymbol`
records containing just `SymbolId`, `Name`, `Description`, and `Aliases`.
The AI never sees the full `DreamSymbol` entity, never sees `LuckyNumber`
rows, and has no code path that could return a number.

This matches the rule "use AI only when natural-language interpretation is
required" — if direct matching already found every active symbol, the AI is
never called at all (see
`DreamAnalysisServiceTests.AnalyseAsync_DirectMatch_SkipsAiForThatSymbol`).

## 3. Validating what the AI returns

Back in `DreamAnalysisService.RunAnalysisAsync`, every symbol ID the AI
returns is checked against the set of active symbol IDs that were loaded
from the database *before* the AI was ever called:

```csharp
var validSymbolIds = activeSymbols.Select(s => s.Id).ToHashSet();
var aiMatches = (aiResult?.Matches ?? [])
    .Where(m => validSymbolIds.Contains(m.SymbolId) && !directlyMatchedIds.Contains(m.SymbolId))
    .GroupBy(m => m.SymbolId)
    .Select(g => g.First())
    .ToList();
```

Any symbol ID outside that set — hallucinated, malformed, or belonging to
an inactive/deleted symbol — is silently dropped. This is deliberately
redundant with the fact that the AI was only ever *given* valid IDs to
choose from: a real AI provider swapped in later might not honor that
constraint as reliably as the mock does, so the validation stays regardless
of which provider is behind `IDreamInterpretationService`.
(`DreamAnalysisServiceTests.AnalyseAsync_IgnoresAiReturnedSymbolIdNotInDatabase`
covers this directly.)

## 4. Loading lucky numbers

Only after a symbol is confirmed matched (directly or via the validated AI
result) are its `LuckyNumber` rows read — via the `LuckyNumbers` navigation
already eagerly loaded on the `DreamSymbol` entities at the top of
`RunAnalysisAsync`. There's no separate "generate a number" step anywhere;
numbers only ever come from rows that existed in the database before the
request began.

## 5. Suggested combinations (`CombinationGenerator`)

`Application/Dreams/Services/CombinationGenerator.cs`. Also pure — takes a
flat list of `(DreamSymbolId, Number, IsPrimary)` tuples plus a
`combinationSize` and `maxCombinations` (both configurable via
`DreamAnalysisSettings`, defaults 6 and 5).

1. **Dedupe** — the same number can appear under two different symbols
   (e.g. two symbols both listing `17`); it's collapsed to one entry,
   marked primary if *any* occurrence was primary.
2. **Interleave by symbol** — numbers are grouped by their owning symbol,
   primary-first within each group, then merged round-robin across symbols
   (one from each symbol per pass). This is what satisfies "include numbers
   from different matched symbols where possible" instead of exhausting one
   symbol's numbers before touching the next.
3. **Generate combinations** — for `offset` from `0` to `maxCombinations`,
   take `combinationSize` numbers starting at `pool[offset % pool.Count]`
   and wrapping. Because `combinationSize` never exceeds `pool.Count`, the
   indices touched within one combination are always distinct — no number
   repeats within a single combination.
4. **Dedupe combinations** — if two offsets happen to produce the same set
   of numbers (small pools), the repeat is dropped rather than counted
   toward the max, so you never get two identical "different" suggestions.

No randomness is involved — the same matched symbols always produce the
same combinations. That's intentional: it keeps the algorithm testable
without seeding a PRNG, and there's no requirement anywhere in the spec for
variety across identical inputs.

## Where the tests live

`backend/tests/GrandmasDreamNumbers.UnitTests/Dreams/Services/`:

- `DreamSymbolMatcherTests.cs` — exact match, alias match, no false
  positives on partial words, multiple symbols in one dream.
- `CombinationGeneratorTests.cs` — no duplicates within a combination,
  cross-symbol dedupe, respects `maxCombinations` and `combinationSize`,
  prefers primary numbers, empty input → empty output.
- `DreamAnalysisServiceTests.cs` — invalid AI symbol IDs ignored, returned
  numbers are a subset of what's in the database, AI isn't called when
  direct matching already covered everything.

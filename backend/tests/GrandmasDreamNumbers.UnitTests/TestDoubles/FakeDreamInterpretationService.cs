using GrandmasDreamNumbers.Application.Common.Interfaces;

namespace GrandmasDreamNumbers.UnitTests.TestDoubles;

public class FakeDreamInterpretationService(DreamInterpretationResult result) : IDreamInterpretationService
{
    public Task<DreamInterpretationResult> InterpretAsync(
        string dreamText,
        IReadOnlyCollection<AvailableDreamSymbol> availableSymbols,
        CancellationToken cancellationToken) => Task.FromResult(result);
}

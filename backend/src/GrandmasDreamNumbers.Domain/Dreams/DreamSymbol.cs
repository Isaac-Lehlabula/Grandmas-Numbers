using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

/// <summary>
/// An entry in the gogo's private cheat sheet. Never exposed to
/// public API consumers in full - only matched symbols are returned.
/// </summary>
public class DreamSymbol : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string TraditionalMeaning { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<DreamSymbolAlias> Aliases { get; set; } = [];
    public List<LuckyNumber> LuckyNumbers { get; set; } = [];
}

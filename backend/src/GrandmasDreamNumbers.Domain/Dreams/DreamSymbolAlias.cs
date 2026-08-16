using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

/// <summary>
/// An alternative word or phrase for a <see cref="DreamSymbol"/>
/// (e.g. "serpent", "cobra" for "Snake").
/// </summary>
public class DreamSymbolAlias : BaseEntity
{
    public Guid DreamSymbolId { get; set; }
    public required string Alias { get; set; }

    public DreamSymbol? DreamSymbol { get; set; }
}

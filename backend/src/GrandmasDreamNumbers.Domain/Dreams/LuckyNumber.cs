using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

public class LuckyNumber : BaseEntity
{
    public Guid DreamSymbolId { get; set; }
    public int Number { get; set; }
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }

    public DreamSymbol? DreamSymbol { get; set; }
}

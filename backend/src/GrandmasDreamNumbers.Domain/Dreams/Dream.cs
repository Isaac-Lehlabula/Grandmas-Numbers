using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.Domain.Dreams;

public class Dream : BaseEntity
{
    public Guid UserId { get; set; }
    public required string OriginalText { get; set; }
    public string? TranscribedText { get; set; }
    public string? Summary { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<DreamAnalysis> Analyses { get; set; } = [];
}

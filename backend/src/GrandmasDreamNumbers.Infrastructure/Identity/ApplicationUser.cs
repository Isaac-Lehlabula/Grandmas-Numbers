using Microsoft.AspNetCore.Identity;

namespace GrandmasDreamNumbers.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

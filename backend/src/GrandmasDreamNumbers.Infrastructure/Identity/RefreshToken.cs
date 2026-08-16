namespace GrandmasDreamNumbers.Infrastructure.Identity;

/// <summary>
/// Persisted refresh token, kept separate from the JWT access token so
/// logout/refresh can revoke it server-side.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRevoked { get; set; }

    public ApplicationUser? User { get; set; }

    public bool IsActive => !IsRevoked && ExpiresAt > DateTimeOffset.UtcNow;
}

namespace GrandmasDreamNumbers.Application.Auth.Dtos;

public sealed record RegisterRequest(string FirstName, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserProfileResponse User);

public sealed record UserProfileResponse(
    Guid Id,
    string FirstName,
    string Email,
    DateTimeOffset CreatedAt);

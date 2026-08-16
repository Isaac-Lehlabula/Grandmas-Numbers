namespace GrandmasDreamNumbers.Application.Common.Interfaces;

public sealed record TokenSubject(
    Guid UserId,
    string Email,
    string FirstName,
    IReadOnlyCollection<string> Roles);

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public sealed record RefreshTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface ITokenGenerator
{
    AccessTokenResult GenerateAccessToken(TokenSubject subject);
    RefreshTokenResult GenerateRefreshToken();
}

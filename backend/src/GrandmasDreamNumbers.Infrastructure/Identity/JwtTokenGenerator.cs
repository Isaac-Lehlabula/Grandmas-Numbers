using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GrandmasDreamNumbers.Infrastructure.Identity;

public sealed class JwtTokenGenerator(IOptions<JwtSettings> jwtOptions) : ITokenGenerator
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public AccessTokenResult GenerateAccessToken(TokenSubject subject)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, subject.UserId.ToString()),
            new(ClaimTypes.Email, subject.Email),
            new(ClaimTypes.GivenName, subject.FirstName),
        };

        claims.AddRange(subject.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessTokenResult(tokenString, expiresAt);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

        return new RefreshTokenResult(token, expiresAt);
    }
}

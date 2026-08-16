using GrandmasDreamNumbers.Application.Auth.Dtos;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    ITokenGenerator tokenGenerator,
    ApplicationDbContext dbContext) : IIdentityService
{
    private const string DefaultRole = "User";

    public async Task<IdentityOperationResult<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return IdentityOperationResult<AuthResponse>.Failure(["Email is already registered."]);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return IdentityOperationResult<AuthResponse>.Failure(
                createResult.Errors.Select(e => e.Description));
        }

        await userManager.AddToRoleAsync(user, DefaultRole);

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken);
        return IdentityOperationResult<AuthResponse>.Success(authResponse);
    }

    public async Task<IdentityOperationResult<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return IdentityOperationResult<AuthResponse>.Failure(["Invalid email or password."]);
        }

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken);
        return IdentityOperationResult<AuthResponse>.Success(authResponse);
    }

    public async Task<IdentityOperationResult<AuthResponse>> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return IdentityOperationResult<AuthResponse>.Failure(["Refresh token is invalid or expired."]);
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return IdentityOperationResult<AuthResponse>.Failure(["Refresh token is invalid or expired."]);
        }

        stored.IsRevoked = true;

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken);
        return IdentityOperationResult<AuthResponse>.Success(authResponse);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is null)
        {
            return;
        }

        stored.IsRevoked = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToProfileResponse(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var subject = new TokenSubject(user.Id, user.Email!, user.FirstName, roles.ToArray());

        var accessToken = tokenGenerator.GenerateAccessToken(subject);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            ToProfileResponse(user));
    }

    private static UserProfileResponse ToProfileResponse(ApplicationUser user) =>
        new(user.Id, user.FirstName, user.Email!, user.CreatedAt);
}

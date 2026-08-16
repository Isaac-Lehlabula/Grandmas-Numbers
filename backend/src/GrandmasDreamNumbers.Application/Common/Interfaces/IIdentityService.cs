using GrandmasDreamNumbers.Application.Auth.Dtos;

namespace GrandmasDreamNumbers.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<IdentityOperationResult<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken);

    Task<IdentityOperationResult<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken);

    Task<IdentityOperationResult<AuthResponse>> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class IdentityOperationResult<T>
{
    public bool Succeeded { get; private init; }
    public T? Value { get; private init; }
    public IReadOnlyCollection<string> Errors { get; private init; } = [];

    public static IdentityOperationResult<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static IdentityOperationResult<T> Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToArray() };
}

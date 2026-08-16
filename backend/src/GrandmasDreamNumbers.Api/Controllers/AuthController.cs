using GrandmasDreamNumbers.Application.Auth.Dtos;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmasDreamNumbers.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Value)
            : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        return result.Succeeded
            ? Ok(result.Value)
            : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await identityService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }
}

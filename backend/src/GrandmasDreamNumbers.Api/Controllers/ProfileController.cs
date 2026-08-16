using GrandmasDreamNumbers.Api.Common;
using GrandmasDreamNumbers.Application.Auth.Dtos;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmasDreamNumbers.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(IIdentityService identityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await identityService.GetProfileAsync(User.GetUserId(), cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }
}

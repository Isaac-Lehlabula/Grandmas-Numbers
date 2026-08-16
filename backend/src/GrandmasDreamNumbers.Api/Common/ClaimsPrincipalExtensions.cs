using System.Security.Claims;

namespace GrandmasDreamNumbers.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The current principal has no user id claim.");

        return Guid.Parse(value);
    }
}

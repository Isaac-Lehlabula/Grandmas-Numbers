using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Application.Dreams.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmasDreamNumbers.Api.Controllers;

[ApiController]
[Route("api/dream-symbols")]
[Authorize]
public class DreamSymbolsController(DreamSymbolSearchService searchService) : ControllerBase
{
    /// <summary>
    /// Name/description search only - never returns lucky numbers, so this
    /// can't be used to reconstruct the private cheat sheet.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyCollection<DreamSymbolSearchResultResponse>>> Search(
        [FromQuery] string? query, CancellationToken cancellationToken)
    {
        var response = await searchService.SearchAsync(query, cancellationToken);
        return Ok(response);
    }
}

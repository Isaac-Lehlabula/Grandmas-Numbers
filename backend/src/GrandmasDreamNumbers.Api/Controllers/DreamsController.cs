using GrandmasDreamNumbers.Api.Common;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Application.Dreams.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GrandmasDreamNumbers.Api.Controllers;

[ApiController]
[Route("api/dreams")]
[Authorize]
public class DreamsController(
    DreamAnalysisService analysisService,
    DreamHistoryService historyService) : ControllerBase
{
    [HttpPost("analyse")]
    [EnableRateLimiting("dream-analysis")]
    public async Task<ActionResult<DreamAnalysisResponse>> Analyse(SubmitDreamRequest request, CancellationToken cancellationToken)
    {
        var response = await analysisService.AnalyseAsync(User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DreamSummaryResponse>>> GetHistory(
        [FromQuery] string? query, CancellationToken cancellationToken)
    {
        var response = await historyService.GetHistoryAsync(User.GetUserId(), query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DreamAnalysisResponse>> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var response = await historyService.GetDetailAsync(User.GetUserId(), id, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await historyService.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/analyse-again")]
    [EnableRateLimiting("dream-analysis")]
    public async Task<ActionResult<DreamAnalysisResponse>> AnalyseAgain(Guid id, CancellationToken cancellationToken)
    {
        var response = await analysisService.ReanalyseAsync(User.GetUserId(), id, cancellationToken);
        return Ok(response);
    }
}

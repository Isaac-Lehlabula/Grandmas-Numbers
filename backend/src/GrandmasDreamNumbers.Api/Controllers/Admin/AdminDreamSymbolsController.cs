using GrandmasDreamNumbers.Application.Admin.Dtos;
using GrandmasDreamNumbers.Application.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmasDreamNumbers.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dream-symbols")]
[Authorize(Roles = "Admin")]
public class AdminDreamSymbolsController(AdminDreamSymbolService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AdminDreamSymbolResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AdminDreamSymbolResponse>> Create(CreateDreamSymbolRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminDreamSymbolResponse>> Update(Guid id, UpdateDreamSymbolRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/aliases")]
    public async Task<ActionResult<AdminDreamSymbolResponse>> AddAlias(Guid id, AddDreamSymbolAliasRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.AddAliasAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/numbers")]
    public async Task<ActionResult<AdminDreamSymbolResponse>> AddNumber(Guid id, AddLuckyNumberRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.AddLuckyNumberAsync(id, request, cancellationToken));
    }
}

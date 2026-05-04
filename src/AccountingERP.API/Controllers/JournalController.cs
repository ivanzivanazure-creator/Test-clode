using AccountingERP.Application.Commands.Journal;
using AccountingERP.Application.Queries.Journal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingERP.API.Controllers;

[ApiController]
[Route("api/v1/journal")]
[Authorize]
public class JournalController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetJournalEntriesQuery(page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances([FromQuery] DateOnly? asOfDate, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAccountBalancesQuery(asOfDate ?? DateOnly.FromDateTime(DateTime.Today)), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAndPostJournalEntryCommand command, CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPost("{id:int}/reverse")]
    public async Task<IActionResult> Reverse(int id, [FromBody] ReverseJournalEntryCommand command, CancellationToken ct = default)
    {
        var cmd = command with { EntryId = id };
        var result = await sender.Send(cmd, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(result.Error);
    }
}

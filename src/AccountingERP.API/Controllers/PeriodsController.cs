namespace AccountingERP.API.Controllers;

using AccountingERP.Application.Commands.Periods;
using AccountingERP.Application.DTOs;
using AccountingERP.Application.Queries.Periods;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/periods")]
[Authorize]
[Produces("application/json")]
public sealed class PeriodsController : ControllerBase
{
    private readonly ISender _sender;

    public PeriodsController(ISender sender) => _sender = sender;

    // ── GET api/v1/periods?year=2024 ──────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountingPeriodDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountingPeriodDto>>> GetByYear(
        [FromQuery] int   year = 0,
        CancellationToken ct   = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(Problem("TenantId nije prisutan u zahtjevu."));

        if (year == 0) year = DateTime.Today.Year;

        var query  = new GetPeriodsQuery(tenantId.Value, year);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(Problem(result.Error));
    }

    // ── POST api/v1/periods/{id}/lock ─────────────────────────────────────────

    [HttpPost("{id:int}/lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lock(
        int               id,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var userId   = GetUserId() ?? "system";

        var result = await _sender.Send(new LockPeriodCommand(tenantId ?? 0, id, userId), ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── POST api/v1/periods/{id}/unlock ───────────────────────────────────────

    [HttpPost("{id:int}/unlock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlock(
        int               id,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var userId   = GetUserId() ?? "system";

        var result = await _sender.Send(new UnlockPeriodCommand(tenantId ?? 0, id, userId), ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? GetTenantId()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var value) && value is int tid)
            return tid;
        return null;
    }

    private string? GetUserId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;

    private IActionResult MapFailure(string error)
    {
        if (error.Contains("nije pronađen", StringComparison.OrdinalIgnoreCase))
            return NotFound(Problem(error));
        return BadRequest(Problem(error));
    }

    private static ProblemDetails Problem(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title  = "Zahtjev nije ispravan",
        Detail = detail,
    };
}

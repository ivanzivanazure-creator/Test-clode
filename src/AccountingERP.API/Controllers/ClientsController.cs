namespace AccountingERP.API.Controllers;

using AccountingERP.Application.Commands.Clients;
using AccountingERP.Application.DTOs;
using AccountingERP.Application.Queries.Clients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
[Produces("application/json")]
public sealed class ClientsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientsController(ISender sender) => _sender = sender;

    // ── GET api/v1/clients ────────────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll(
        [FromQuery] string? search = null,
        CancellationToken   ct     = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(Problem("TenantId nije prisutan u zahtjevu."));

        var query  = new GetClientsQuery(tenantId.Value, search);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(Problem(result.Error));
    }

    // ── POST api/v1/clients ───────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(
        [FromBody]        CreateClientCommand command,
        CancellationToken                     ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(Problem("TenantId nije prisutan u zahtjevu."));

        var cmd    = command with { TenantId = tenantId.Value };
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return BadRequest(Problem(result.Error));

        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, result.Value);
    }

    // ── PUT api/v1/clients/{id} ───────────────────────────────────────────────

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int               id,
        [FromBody]        UpdateClientCommand command,
        CancellationToken                     ct = default)
    {
        var cmd    = command with { Id = id };
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── DELETE api/v1/clients/{id} ────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        int               id,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new DeactivateClientCommand(id), ct);

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

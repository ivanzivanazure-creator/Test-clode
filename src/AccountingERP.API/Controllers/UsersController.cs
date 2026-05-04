namespace AccountingERP.API.Controllers;

using AccountingERP.Application.Commands.Users;
using AccountingERP.Application.DTOs;
using AccountingERP.Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    // ── GET api/v1/users ──────────────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(Problem("TenantId nije prisutan u zahtjevu."));

        var result = await _sender.Send(new GetUsersQuery(tenantId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(Problem(result.Error));
    }

    // ── POST api/v1/users ─────────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(
        [FromBody]        CreateUserCommand command,
        CancellationToken                   ct = default)
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

    // ── POST api/v1/users/{id}/change-password ────────────────────────────────

    [HttpPost("{id:int}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        int                         id,
        [FromBody] ChangePasswordRequest request,
        CancellationToken               ct = default)
    {
        var cmd    = new ChangePasswordCommand(id, request.CurrentPassword, request.NewPassword);
        var result = await _sender.Send(cmd, ct);

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

// ── Request bodies ────────────────────────────────────────────────────────────

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

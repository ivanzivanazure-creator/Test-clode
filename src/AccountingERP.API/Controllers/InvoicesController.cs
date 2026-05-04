namespace AccountingERP.API.Controllers;

using AccountingERP.Application.Commands.Invoices;
using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Application.Queries.Invoices;
using AccountingERP.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
[Produces("application/json")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public InvoicesController(ISender sender) => _sender = sender;

    // ── GET api/v1/invoices ───────────────────────────────────────────────────

    /// <summary>Returns a paginated list of invoices for the current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? status   = null,
        [FromQuery] int?    clientId = null,
        [FromQuery] string? search   = null,
        CancellationToken   ct       = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        var query  = new GetInvoicesQuery(tenantId.Value, page, pageSize, status, clientId, search);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── GET api/v1/invoices/summary ───────────────────────────────────────────

    /// <summary>Returns aggregated invoice KPIs (total count, revenue, overdue).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(InvoiceSummaryData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceSummaryData>> GetSummary(
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        var query  = new GetInvoiceSummaryQuery(tenantId.Value);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── GET api/v1/invoices/{id} ──────────────────────────────────────────────

    /// <summary>Returns full invoice detail including all line items.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InvoiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDetailDto>> GetById(
        int               id,
        CancellationToken ct = default)
    {
        var query  = new GetInvoiceByIdQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(ProblemDetailsFor(result.Error));

        return Ok(result.Value);
    }

    // ── POST api/v1/invoices ──────────────────────────────────────────────────

    /// <summary>Creates a new invoice in Nacrt (draft) status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(
        [FromBody]        CreateInvoiceCommand command,
        CancellationToken                      ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        // Bind TenantId from the authenticated context, not the request body.
        var cmd    = command with { TenantId = tenantId.Value };
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return BadRequest(ProblemDetailsFor(result.Error));

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    // ── POST api/v1/invoices/{id}/issue ───────────────────────────────────────

    /// <summary>Issues a draft invoice (Nacrt → Izdata).</summary>
    [HttpPost("{id:int}/issue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Issue(
        int               id,
        CancellationToken ct = default)
    {
        var userId = GetUserId() ?? "system";
        var cmd    = new IssueInvoiceCommand(id, userId);
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── POST api/v1/invoices/{id}/pay ─────────────────────────────────────────

    /// <summary>Marks an invoice as paid. Accepts a payment date in the request body.</summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(
        int                      id,
        [FromBody] PayInvoiceRequest request,
        CancellationToken        ct = default)
    {
        var cmd    = new MarkInvoicePaidCommand(id, request.PaymentDate);
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── GET api/v1/invoices/{id}/pdf ──────────────────────────────────────────

    /// <summary>Generates and returns the invoice as a PDF file.</summary>
    [HttpGet("{id:int}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdf(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetInvoicePdfQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return File(result.Value, MediaTypeNames.Application.Pdf, $"faktura-{id}.pdf");
    }

    // ── DELETE api/v1/invoices/{id} ───────────────────────────────────────────

    /// <summary>Cancels an invoice (moves to Otkazana status). Cannot cancel paid invoices.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        int               id,
        CancellationToken ct = default)
    {
        var cmd    = new CancelInvoiceCommand(id);
        var result = await _sender.Send(cmd, ct);

        if (result.IsFailure)
            return MapFailure(result.Error);

        return NoContent();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private int? GetTenantId()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var value)
            && value is int tenantId)
            return tenantId;
        return null;
    }

    private string? GetUserId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;

    private IActionResult MapFailure(string error)
    {
        // If the error message contains a "not found" indicator, return 404.
        if (error.Contains("nije pronađen", StringComparison.OrdinalIgnoreCase)
         || error.Contains("nije pronađena", StringComparison.OrdinalIgnoreCase))
            return NotFound(ProblemDetailsFor(error));

        return BadRequest(ProblemDetailsFor(error));
    }

    private static ProblemDetails ProblemDetailsFor(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title  = "Zahtjev nije ispravan",
        Detail = detail,
    };
}

// ── Request bodies ────────────────────────────────────────────────────────────

/// <summary>Request body for the pay endpoint.</summary>
public sealed record PayInvoiceRequest(DateOnly PaymentDate);

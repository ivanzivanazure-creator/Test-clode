namespace AccountingERP.API.Controllers;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Application.Queries.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    // ── GET api/v1/reports/balance-sheet?asOfDate=2024-12-31 ─────────────────

    /// <summary>Returns a balance sheet (bilans stanja) as of the given date.</summary>
    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBalanceSheet(
        [FromQuery] DateOnly asOfDate,
        CancellationToken    ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        var query  = new GetBalanceSheetQuery(tenantId.Value, asOfDate);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── GET api/v1/reports/income-statement?month=12&year=2024 ───────────────

    /// <summary>Returns an income statement (bilans uspjeha) for the given period.</summary>
    [HttpGet("income-statement")]
    [ProducesResponseType(typeof(IncomeStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetIncomeStatement(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        if (month < 1 || month > 12)
            return BadRequest(ProblemDetailsFor("Mesec mora biti između 1 i 12."));

        var query  = new GetIncomeStatementQuery(tenantId.Value, month, year);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── GET api/v1/reports/vat-return?month=12&year=2024 ─────────────────────

    /// <summary>Returns a VAT return (PP PDV obrazac) for the given period.</summary>
    [HttpGet("vat-return")]
    [ProducesResponseType(typeof(VatReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetVatReturn(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        if (month < 1 || month > 12)
            return BadRequest(ProblemDetailsFor("Mesec mora biti između 1 i 12."));

        var query  = new GetVatReturnQuery(tenantId.Value, month, year);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── GET api/v1/reports/payroll?month=12&year=2024 ────────────────────────

    /// <summary>Returns a payroll report (obračun zarada) for the given period.</summary>
    [HttpGet("payroll")]
    [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPayroll(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId is null)
            return BadRequest(ProblemDetailsFor("TenantId nije prisutan u zahtjevu."));

        if (month < 1 || month > 12)
            return BadRequest(ProblemDetailsFor("Mesec mora biti između 1 i 12."));

        var query  = new GetPayrollReportQuery(tenantId.Value, month, year);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ProblemDetailsFor(result.Error));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private int? GetTenantId()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var value)
            && value is int tenantId)
            return tenantId;
        return null;
    }

    private static ProblemDetails ProblemDetailsFor(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title  = "Zahtjev nije ispravan",
        Detail = detail,
    };
}

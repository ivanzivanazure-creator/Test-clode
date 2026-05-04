using AccountingERP.Application.Commands.Employees;
using AccountingERP.Application.Queries.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingERP.API.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEmployeesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:int}/payroll")]
    public async Task<IActionResult> GetPayroll(int id, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetPayrollCalculationQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command, CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:int}/salary")]
    public async Task<IActionResult> UpdateSalary(int id, [FromBody] UpdateSalaryCommand command, CancellationToken ct = default)
    {
        var cmd = command with { EmployeeId = id };
        var result = await sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:int}/terminate")]
    public async Task<IActionResult> Terminate(int id, [FromBody] TerminateEmployeeCommand command, CancellationToken ct = default)
    {
        var cmd = command with { EmployeeId = id };
        var result = await sender.Send(cmd, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

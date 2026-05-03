namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.Employee;

/// <summary>List/detail DTO for an employee. Sensitive fields are never exposed here.</summary>
public sealed record EmployeeDto(
    int      Id,
    int      TenantId,
    string   FirstName,
    string   LastName,
    string   FullName,
    string   Position,
    decimal  GrossSalary,
    DateOnly HireDate,
    DateOnly? TermDate,
    string   TaxExemption,
    bool     IsActive,
    bool     IsPseudonymized)
{
    public static EmployeeDto FromDomain(Employee emp) => new(
        Id:              emp.Id,
        TenantId:        emp.TenantId.Value,
        FirstName:       emp.FirstName,
        LastName:        emp.LastName,
        FullName:        emp.FullName,
        Position:        emp.Position,
        GrossSalary:     emp.GrossSalary.Amount,
        HireDate:        emp.HireDate,
        TermDate:        emp.TermDate,
        TaxExemption:    emp.TaxExemption.ToString(),
        IsActive:        emp.IsActive,
        IsPseudonymized: emp.IsPseudonymized);
}

/// <summary>
/// DTO representing the result of a payroll calculation.
/// Maps directly from the <see cref="PayrollCalculation"/> domain record.
/// All monetary values are in RSD.
/// </summary>
public sealed record PayrollResultDto(
    decimal GrossBase,
    decimal PensionEmployee,
    decimal HealthEmployee,
    decimal UnemploymentEmployee,
    decimal EmployeeContributions,
    decimal TaxableIncome,
    decimal PersonalDeduction,
    decimal IncomeTax,
    decimal NetSalary,
    decimal PensionEmployer,
    decimal HealthEmployer,
    decimal UnemploymentEmployer,
    decimal EmployerContributions,
    decimal TotalCost)
{
    public static PayrollResultDto FromDomain(PayrollCalculation calc) => new(
        GrossBase:               calc.GrossBase.Amount,
        PensionEmployee:         calc.PensionEmployee.Amount,
        HealthEmployee:          calc.HealthEmployee.Amount,
        UnemploymentEmployee:    calc.UnemploymentEmp.Amount,
        EmployeeContributions:   calc.EmployeeContributions.Amount,
        TaxableIncome:           calc.TaxableIncome.Amount,
        PersonalDeduction:       calc.PersonalDeduction.Amount,
        IncomeTax:               calc.IncomeTax.Amount,
        NetSalary:               calc.NetSalary.Amount,
        PensionEmployer:         calc.PensionEmployer.Amount,
        HealthEmployer:          calc.HealthEmployer.Amount,
        UnemploymentEmployer:    calc.UnemploymentEmployer.Amount,
        EmployerContributions:   calc.EmployerContributions.Amount,
        TotalCost:               calc.TotalCost.Amount);
}

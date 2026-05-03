namespace AccountingERP.Domain.Aggregates.Employee;

using AccountingERP.Domain.ValueObjects;

public record PayrollCalculation(
    Money GrossBase,
    Money PensionEmployee, Money HealthEmployee, Money UnemploymentEmp,
    Money TaxableIncome, Money PersonalDeduction, Money IncomeTax, Money NetSalary,
    Money PensionEmployer, Money HealthEmployer, Money UnemploymentEmployer)
{
    public Money EmployeeContributions => PensionEmployee.Add(HealthEmployee).Add(UnemploymentEmp);
    public Money EmployerContributions => PensionEmployer.Add(HealthEmployer).Add(UnemploymentEmployer);
    public Money TotalCost             => GrossBase.Add(EmployerContributions);
}

namespace AccountingERP.Tests.Application;

using AccountingERP.Domain.Aggregates.Employee;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

/// <summary>
/// Verifies the payroll calculation logic for the 2024 Serbian tax rates.
///
/// Rates used:
///   Employee: PIO 14%, Health 5.15%, Unemployment 0.75%
///   Employer: PIO 11.5%, Health 5.15%, Unemployment 0.75%
///   Personal deduction (None):     21 712 RSD
///   Personal deduction (FirstJob): 43 424 RSD
///   Income tax rate: 10%
/// </summary>
public class PayrollCalculationTests
{
    private static TenantId Tenant => TenantId.From(1);

    private static Employee MakeEmployee(decimal gross, TaxExemption exemption = TaxExemption.None) =>
        Employee.Create(
            Tenant,
            "Test", "User",
            "Tester",
            Money.FromRSD(gross),
            DateOnly.FromDateTime(DateTime.Today),
            exemption);

    // ── 80 000 RSD, TaxExemption.None ────────────────────────────────────────

    [Fact]
    public void Gross80000_None_PensionEmployee_ShouldBe11200()
    {
        // 80 000 * 0.14 = 11 200
        var calc = MakeEmployee(80_000m).CalculateNetSalary();

        calc.PensionEmployee.Amount.Should().Be(11_200m);
    }

    [Fact]
    public void Gross80000_None_HealthEmployee_ShouldBe4120()
    {
        // 80 000 * 0.0515 = 4 120
        var calc = MakeEmployee(80_000m).CalculateNetSalary();

        calc.HealthEmployee.Amount.Should().Be(4_120m);
    }

    [Fact]
    public void Gross80000_None_IncomeTax_ShouldBeCorrect()
    {
        // Taxable = 80 000 - 11 200 - 4 120 - 600 - 21 712 = 42 368
        // Tax = 42 368 * 0.10 = 4 236.80
        var calc = MakeEmployee(80_000m).CalculateNetSalary();

        calc.UnemploymentEmp.Amount.Should().Be(600m);           // 80 000 * 0.0075
        calc.TaxableIncome.Amount.Should().Be(42_368m);
        calc.IncomeTax.Amount.Should().Be(4_236.80m);
    }

    [Fact]
    public void Gross80000_None_TotalCost_ShouldEqualGrossPlusEmployerContributions()
    {
        var calc = MakeEmployee(80_000m).CalculateNetSalary();

        // Employer: PIO 9 200 + Health 4 120 + Unemp 600 = 13 920
        // TotalCost = 80 000 + 13 920 = 93 920
        var expectedEmployerContribs =
            Math.Round(80_000m * 0.115m, 2)  // pension
            + Math.Round(80_000m * 0.0515m, 2) // health
            + Math.Round(80_000m * 0.0075m, 2); // unemployment

        calc.TotalCost.Amount.Should().Be(80_000m + expectedEmployerContribs);
    }

    [Fact]
    public void Gross80000_None_EmployeeContributions_ShouldEqualPioPlusHealthPlusUnemp()
    {
        var calc = MakeEmployee(80_000m).CalculateNetSalary();

        var expected = calc.PensionEmployee.Amount
                       + calc.HealthEmployee.Amount
                       + calc.UnemploymentEmp.Amount;

        calc.EmployeeContributions.Amount.Should().Be(expected);
    }

    // ── FirstJob exemption changes personal deduction ─────────────────────────

    [Fact]
    public void Gross80000_FirstJob_PersonalDeduction_ShouldBe43424()
    {
        var calc = MakeEmployee(80_000m, TaxExemption.FirstJob).CalculateNetSalary();

        calc.PersonalDeduction.Amount.Should().Be(43_424m);
    }

    [Fact]
    public void Gross80000_FirstJob_TaxableIncome_ShouldBeLowerThanNoneExemption()
    {
        var calcNone     = MakeEmployee(80_000m, TaxExemption.None).CalculateNetSalary();
        var calcFirstJob = MakeEmployee(80_000m, TaxExemption.FirstJob).CalculateNetSalary();

        calcFirstJob.TaxableIncome.Amount.Should().BeLessThan(calcNone.TaxableIncome.Amount);
    }
}

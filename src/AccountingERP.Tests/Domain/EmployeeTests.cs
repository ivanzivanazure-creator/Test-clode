namespace AccountingERP.Tests.Domain;

using AccountingERP.Domain.Aggregates.Employee;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class EmployeeTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static TenantId Tenant => TenantId.From(1);

    private static Employee MakeEmployee(
        decimal grossAmount     = 60_000m,
        TaxExemption exemption  = TaxExemption.None,
        DateOnly? hireDate      = null) =>
        Employee.Create(
            Tenant,
            "Marko", "Marković",
            "Developer",
            Money.FromRSD(grossAmount),
            hireDate ?? DateOnly.FromDateTime(DateTime.Today),
            exemption);

    // ── creation ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldBeActive()
    {
        var emp = MakeEmployee();

        emp.IsActive.Should().BeTrue();
        emp.FirstName.Should().Be("Marko");
        emp.LastName.Should().Be("Marković");
    }

    [Fact]
    public void Create_WithNegativeSalary_ShouldThrowDomainException()
    {
        var act = () => Employee.Create(
            Tenant,
            "Marko", "Marković",
            "Developer",
            Money.FromRSD(-1000m),
            DateOnly.FromDateTime(DateTime.Today));

        act.Should().Throw<DomainException>();
    }

    // ── CalculateNetSalary ────────────────────────────────────────────────────

    [Fact]
    public void CalculateNetSalary_ShouldDeductCorrectContributions()
    {
        // Gross = 100 000 RSD, TaxExemption.None  =>  personal deduction = 21 712
        // PIO       = 100 000 * 0.14   = 14 000.00
        // Health    = 100 000 * 0.0515 =  5 150.00
        // Unemp     = 100 000 * 0.0075 =    750.00
        // Taxable   = 100 000 - 14 000 - 5 150 - 750 - 21 712 = 58 388
        // Tax       = 58 388 * 0.10 = 5 838.80
        var emp  = MakeEmployee(grossAmount: 100_000m, exemption: TaxExemption.None);
        var calc = emp.CalculateNetSalary();

        calc.PensionEmployee.Amount.Should().Be(14_000m);
        calc.HealthEmployee.Amount.Should().Be(5_150m);
        calc.UnemploymentEmp.Amount.Should().Be(750m);
        calc.PersonalDeduction.Amount.Should().Be(21_712m);
        calc.TaxableIncome.Amount.Should().Be(58_388m);
        calc.IncomeTax.Amount.Should().Be(5_838.80m);
    }

    [Fact]
    public void CalculateNetSalary_FirstJobExemption_ShouldUseHigherDeduction()
    {
        var emp  = MakeEmployee(grossAmount: 100_000m, exemption: TaxExemption.FirstJob);
        var calc = emp.CalculateNetSalary();

        calc.PersonalDeduction.Amount.Should().Be(43_424m);
    }

    // ── Terminate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Terminate_ActiveEmployee_ShouldDeactivate()
    {
        var emp = MakeEmployee();

        emp.Terminate(DateOnly.FromDateTime(DateTime.Today));

        emp.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Terminate_AlreadyTerminated_ShouldThrowDomainException()
    {
        var emp = MakeEmployee();
        emp.Terminate(DateOnly.FromDateTime(DateTime.Today));

        var act = () => emp.Terminate(DateOnly.FromDateTime(DateTime.Today));

        act.Should().Throw<DomainException>();
    }

    // ── Pseudonymize ──────────────────────────────────────────────────────────

    [Fact]
    public void Pseudonymize_ShouldClearPersonalData()
    {
        var emp = MakeEmployee();
        emp.SetEncryptedData("enc-jmbg", "hash-jmbg", "enc-bank", "enc-email");

        emp.Pseudonymize("GDPR zahtev");

        emp.FirstName.Should().Be("PSEUDONIM");
        emp.JMBGEncrypted.Should().BeNull();
        emp.IsPseudonymized.Should().BeTrue();
    }

    [Fact]
    public void Pseudonymize_AlreadyPseudonymized_ShouldThrowDomainException()
    {
        var emp = MakeEmployee();
        emp.Pseudonymize("GDPR zahtev");

        var act = () => emp.Pseudonymize("duplikat");

        act.Should().Throw<DomainException>();
    }
}

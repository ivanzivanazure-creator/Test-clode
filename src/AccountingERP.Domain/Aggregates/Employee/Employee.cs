namespace AccountingERP.Domain.Aggregates.Employee;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.ValueObjects;

public enum TaxExemption { None, FirstJob, DisabledPerson }

public class Employee : AggregateRoot<int>
{
    public TenantId     TenantId     { get; private set; } = null!;
    public string       FirstName    { get; private set; } = null!;
    public string       LastName     { get; private set; } = null!;
    public string       Position     { get; private set; } = null!;
    public Money        GrossSalary  { get; private set; } = null!;
    public DateOnly     HireDate     { get; private set; }
    public DateOnly?    TermDate     { get; private set; }
    public TaxExemption TaxExemption { get; private set; }
    public bool         IsActive     { get; private set; }

    // ZZPL 87/2018 — enkriptovani osetljivi podaci
    // Format: "<IV_base64>:<ciphertext_base64>" — AES-256-GCM
    // Ključ NIKAD u kodu — samo u Azure Key Vault
    public string? JMBGEncrypted        { get; private set; }
    public string? JMBGHashSha256       { get; private set; }
    public string? BankAccountEncrypted { get; private set; }
    public string? EmailEncrypted       { get; private set; }

    // GDPR pseudonimizacija
    public bool      IsPseudonymized  { get; private set; }
    public DateTime? PseudonymizedAt  { get; private set; }

    private Employee() {}

    public static Employee Create(
        TenantId tenantId,
        string firstName, string lastName,
        string position, Money grossSalary,
        DateOnly hireDate,
        TaxExemption taxExemption = TaxExemption.None)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("Ime je obavezno");
        if (string.IsNullOrWhiteSpace(lastName))  throw new DomainException("Prezime je obavezno");
        if (!grossSalary.IsPositive)               throw new DomainException("Bruto plata mora biti > 0");
        if (hireDate > DateOnly.FromDateTime(DateTime.Today).AddDays(30))
            throw new DomainException("Datum zaposlenja ne može biti previše u budućnosti");

        var emp = new Employee
        {
            TenantId     = tenantId,
            FirstName    = firstName.Trim(),
            LastName     = lastName.Trim(),
            Position     = position.Trim(),
            GrossSalary  = grossSalary,
            HireDate     = hireDate,
            TaxExemption = taxExemption,
            IsActive     = true,
        };

        emp.Raise(new EmployeeCreatedEvent(tenantId.Value, emp.Id, firstName, lastName));
        return emp;
    }

    public void SetEncryptedData(string? jmbgEnc, string? jmbgHash, string? bankEnc, string? emailEnc)
    {
        JMBGEncrypted        = jmbgEnc;
        JMBGHashSha256       = jmbgHash;
        BankAccountEncrypted = bankEnc;
        EmailEncrypted       = emailEnc;
    }

    public void UpdateSalary(Money newSalary, string reason)
    {
        if (!newSalary.IsPositive) throw new DomainException("Bruto plata mora biti > 0");
        var old = GrossSalary;
        GrossSalary = newSalary;
        Raise(new EmployeeSalaryChangedEvent(TenantId.Value, Id, old, newSalary, reason));
    }

    public void Terminate(DateOnly terminationDate)
    {
        if (!IsActive) throw new DomainException("Zaposleni već nije aktivan");
        if (terminationDate < HireDate)
            throw new DomainException("Datum prestanka ne može biti pre datuma zaposlenja");
        IsActive = false;
        TermDate = terminationDate;
        Raise(new EmployeeTerminatedEvent(TenantId.Value, Id, terminationDate));
    }

    public void Pseudonymize(string reason)
    {
        // GDPR čl. 17 — "pravo na zaborav"
        // Ne brišemo finansijske podatke (Zakon o računovodstvu — 10g. čuvanje)
        if (IsPseudonymized) throw new DomainException("Zaposleni je već pseudonimiziran");

        FirstName            = "PSEUDONIM";
        LastName             = $"IZBRISAN-{Id}";
        JMBGEncrypted        = null;
        JMBGHashSha256       = null;
        BankAccountEncrypted = null;
        EmailEncrypted       = null;
        IsPseudonymized      = true;
        PseudonymizedAt      = DateTime.UtcNow;

        Raise(new EmployeePseudonymizedEvent(TenantId.Value, Id, reason));
    }

    public PayrollCalculation CalculateNetSalary()
    {
        var gross     = GrossSalary.Amount;
        var pio       = Math.Round(gross * 0.14m, 2);
        var health    = Math.Round(gross * 0.0515m, 2);
        var unemp     = Math.Round(gross * 0.0075m, 2);
        var deduction = TaxExemption == TaxExemption.FirstJob ? 43424m : 21712m; // 2024 iznosi
        var taxable   = Math.Max(0, gross - pio - health - unemp - deduction);
        var tax       = Math.Round(taxable * 0.10m, 2);
        var net       = gross - pio - health - unemp - tax;

        return new PayrollCalculation(
            GrossBase:            Money.FromRSD(gross),
            PensionEmployee:      Money.FromRSD(pio),
            HealthEmployee:       Money.FromRSD(health),
            UnemploymentEmp:      Money.FromRSD(unemp),
            TaxableIncome:        Money.FromRSD(taxable),
            PersonalDeduction:    Money.FromRSD(deduction),
            IncomeTax:            Money.FromRSD(tax),
            NetSalary:            Money.FromRSD(net),
            PensionEmployer:      Money.FromRSD(Math.Round(gross * 0.115m, 2)),
            HealthEmployer:       Money.FromRSD(Math.Round(gross * 0.0515m, 2)),
            UnemploymentEmployer: Money.FromRSD(Math.Round(gross * 0.0075m, 2)));
    }

    public string FullName => $"{FirstName} {LastName}";
}

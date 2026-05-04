namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.ValueObjects;

public record BalanceSheetDto(
    DateOnly AsOf,
    IEnumerable<ReportLineDto> Assets,
    IEnumerable<ReportLineDto> Liabilities,
    IEnumerable<ReportLineDto> Equity,
    Money TotalAssets,
    Money TotalLiabilities,
    Money TotalEquity);

public record IncomeStatementDto(
    AccountingPeriod Period,
    IEnumerable<ReportLineDto> Revenue,
    IEnumerable<ReportLineDto> Expenses,
    Money TotalRevenue,
    Money TotalExpenses,
    Money NetIncome);

public record VatReturnDto(
    int Month,
    int Year,
    Money TaxableBase20,
    Money VatCollected20,
    Money TaxableBase10,
    Money VatCollected10,
    Money TaxableBase0,
    Money InputVatDeductible,
    Money VatPayable);

public record PayrollReportDto(
    int Month,
    int Year,
    IEnumerable<EmployeePayrollLine> Lines,
    Money TotalGross,
    Money TotalNet,
    Money TotalTax,
    Money TotalEmployerContributions);

public record EmployeePayrollLine(
    string FullName,
    string Position,
    Money Gross,
    Money Net,
    Money Tax,
    Money TotalCost);

public record ReportLineDto(
    string Code,
    string Name,
    Money Balance,
    int Level = 0);

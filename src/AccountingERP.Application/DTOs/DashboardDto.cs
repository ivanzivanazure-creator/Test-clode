namespace AccountingERP.Application.DTOs;

/// <summary>
/// Aggregated KPI snapshot for the main dashboard.
/// Revenue figures are in RSD.
/// </summary>
public sealed record DashboardDto(
    int                           TotalInvoices,
    decimal                       TotalRevenue,
    decimal                       PaidRevenue,
    decimal                       OverdueRevenue,
    int                           OverdueCount,
    int                           ActiveEmployees,
    decimal                       TotalPayrollCost,
    IReadOnlyList<MonthlyRevenueDto> MonthlyRevenue);

/// <summary>Revenue broken down by calendar month for charting purposes.</summary>
public sealed record MonthlyRevenueDto(
    int     Year,
    int     Month,
    string  MonthName,
    decimal IssuedAmount,
    decimal PaidAmount);

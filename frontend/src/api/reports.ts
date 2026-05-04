import client from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface Money {
  amount: number;
  currency: string;
}

export interface ReportLineDto {
  code: string;
  name: string;
  balance: Money;
  level: number;
}

export interface BalanceSheetDto {
  asOf: string;
  assets: ReportLineDto[];
  liabilities: ReportLineDto[];
  equity: ReportLineDto[];
  totalAssets: Money;
  totalLiabilities: Money;
  totalEquity: Money;
}

export interface IncomeStatementDto {
  period: { month: number; year: number };
  revenue: ReportLineDto[];
  expenses: ReportLineDto[];
  totalRevenue: Money;
  totalExpenses: Money;
  netIncome: Money;
}

export interface VatReturnDto {
  month: number;
  year: number;
  taxableBase20: Money;
  vatCollected20: Money;
  taxableBase10: Money;
  vatCollected10: Money;
  taxableBase0: Money;
  inputVatDeductible: Money;
  vatPayable: Money;
}

export interface EmployeePayrollLine {
  fullName: string;
  position: string;
  gross: Money;
  net: Money;
  tax: Money;
  totalCost: Money;
}

export interface PayrollReportDto {
  month: number;
  year: number;
  lines: EmployeePayrollLine[];
  totalGross: Money;
  totalNet: Money;
  totalTax: Money;
  totalEmployerContributions: Money;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getBalanceSheet(asOfDate: string): Promise<BalanceSheetDto> {
  const { data } = await client.get<BalanceSheetDto>('/reports/balance-sheet', {
    params: { asOfDate },
  });
  return data;
}

export async function getIncomeStatement(
  month: number,
  year: number,
): Promise<IncomeStatementDto> {
  const { data } = await client.get<IncomeStatementDto>('/reports/income-statement', {
    params: { month, year },
  });
  return data;
}

export async function getVatReturn(
  month: number,
  year: number,
): Promise<VatReturnDto> {
  const { data } = await client.get<VatReturnDto>('/reports/vat-return', {
    params: { month, year },
  });
  return data;
}

export async function getPayrollReport(
  month: number,
  year: number,
): Promise<PayrollReportDto> {
  const { data } = await client.get<PayrollReportDto>('/reports/payroll', {
    params: { month, year },
  });
  return data;
}

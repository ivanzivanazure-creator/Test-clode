// ─── Enums ───────────────────────────────────────────────────────────────────

export enum InvoiceStatus {
  Nacrt = 'Nacrt',
  Izdata = 'Izdata',
  Placena = 'Plaćena',
  Dospela = 'Dospela',
  Otkazana = 'Otkazana',
}

export enum JournalStatus {
  Proknjizeno = 'Proknjiženo',
  Stornirano = 'Stornirano',
}

// ─── Invoice Types ────────────────────────────────────────────────────────────

export interface InvoiceItemDto {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
  subtotal: number;
  vatAmount: number;
  total: number;
}

export interface InvoiceDto {
  id: string;
  number: string;
  clientId: string;
  clientName: string;
  issueDate: string | null;
  dueDate: string;
  status: InvoiceStatus;
  subtotal: number;
  vatAmount: number;
  totalAmount: number;
  currency: string;
  createdAt: string;
}

export interface InvoiceDetailDto extends InvoiceDto {
  items: InvoiceItemDto[];
  notes: string | null;
  paidDate: string | null;
  cancelledDate: string | null;
}

export interface InvoiceSummaryData {
  totalInvoices: number;
  totalRevenue: number;
  overdueCount: number;
  overdueAmount: number;
  draftCount: number;
  paidCount: number;
  monthlyRevenue: MonthlyRevenueDto[];
}

export interface MonthlyRevenueDto {
  month: string;
  revenue: number;
  count: number;
}

// ─── Invoice Request Types ────────────────────────────────────────────────────

export interface CreateInvoiceRequest {
  number: string;
  clientId: string;
  dueDate: string;
  notes?: string;
  items: CreateInvoiceItemRequest[];
}

export interface CreateInvoiceItemRequest {
  description: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
}

export interface PayInvoiceRequest {
  paidDate: string;
}

// ─── Journal Types ────────────────────────────────────────────────────────────

export interface JournalLineDto {
  id: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description: string | null;
}

export interface JournalEntryDto {
  id: string;
  number: string;
  date: string;
  description: string;
  status: JournalStatus;
  totalDebit: number;
  totalCredit: number;
  createdAt: string;
  lines: JournalLineDto[];
}

export interface AccountBalanceDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  balance: number;
  debitTotal: number;
  creditTotal: number;
}

export interface CreateJournalEntryRequest {
  date: string;
  description: string;
  lines: CreateJournalLineRequest[];
}

export interface CreateJournalLineRequest {
  accountId: string;
  debit: number;
  credit: number;
  description?: string;
}

// ─── Employee Types ───────────────────────────────────────────────────────────

export interface EmployeeDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  position: string;
  department: string;
  grossSalary: number;
  netSalary: number;
  hireDate: string;
  terminatedDate: string | null;
  isActive: boolean;
  taxExemption: string;
}

export interface PayrollResultDto {
  employeeId: string;
  employeeName: string;
  period: string;
  grossSalary: number;
  pensionContribution: number;
  healthContribution: number;
  unemploymentContribution: number;
  incomeTax: number;
  netSalary: number;
  employerPension: number;
  employerHealth: number;
  totalEmployerCost: number;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  position: string;
  department: string;
  grossSalary: number;
  hireDate: string;
  taxExemption: string;
}

export interface UpdateSalaryRequest {
  grossSalary: number;
  effectiveDate: string;
}

export interface TerminateEmployeeRequest {
  terminationDate: string;
  reason?: string;
}

// ─── Dashboard Types ──────────────────────────────────────────────────────────

export interface DashboardDto {
  totalInvoices: number;
  totalRevenue: number;
  overdueInvoices: number;
  activeEmployees: number;
  recentInvoices: InvoiceDto[];
  monthlyRevenue: MonthlyRevenueDto[];
  cashFlow: number;
  pendingPayments: number;
}

// ─── Generic API Types ────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResult<T> {
  data: T;
  success: boolean;
  message: string | null;
  errors: string[] | null;
}

// ─── Filter Types ─────────────────────────────────────────────────────────────

export interface InvoiceFilter {
  status?: InvoiceStatus;
  search?: string;
  page?: number;
  pageSize?: number;
  dateFrom?: string;
  dateTo?: string;
}

export interface JournalFilter {
  search?: string;
  page?: number;
  pageSize?: number;
  dateFrom?: string;
  dateTo?: string;
}

// ─── Auth Types ───────────────────────────────────────────────────────────────

export interface AuthUser {
  id: string;
  username: string;
  fullName: string;
  email: string;
  role: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: AuthUser;
  expiresAt: string;
}

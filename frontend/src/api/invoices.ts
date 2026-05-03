import client from './client';
import type {
  InvoiceDto,
  InvoiceDetailDto,
  InvoiceSummaryData,
  CreateInvoiceRequest,
  PayInvoiceRequest,
  PagedResult,
  InvoiceFilter,
} from '../types';

// ─── GET /api/v1/invoices ─────────────────────────────────────────────────────
export async function getInvoices(filter: InvoiceFilter = {}): Promise<PagedResult<InvoiceDto>> {
  const params: Record<string, string | number> = {
    page: filter.page ?? 1,
    pageSize: filter.pageSize ?? 20,
  };
  if (filter.status) params.status = filter.status;
  if (filter.search) params.search = filter.search;
  if (filter.dateFrom) params.dateFrom = filter.dateFrom;
  if (filter.dateTo) params.dateTo = filter.dateTo;

  const { data } = await client.get<PagedResult<InvoiceDto>>('/invoices', { params });
  return data;
}

// ─── GET /api/v1/invoices/{id} ────────────────────────────────────────────────
export async function getInvoice(id: string): Promise<InvoiceDetailDto> {
  const { data } = await client.get<InvoiceDetailDto>(`/invoices/${id}`);
  return data;
}

// ─── GET /api/v1/invoices/summary ────────────────────────────────────────────
export async function getInvoiceSummary(): Promise<InvoiceSummaryData> {
  const { data } = await client.get<InvoiceSummaryData>('/invoices/summary');
  return data;
}

// ─── POST /api/v1/invoices ────────────────────────────────────────────────────
export async function createInvoice(payload: CreateInvoiceRequest): Promise<InvoiceDetailDto> {
  const { data } = await client.post<InvoiceDetailDto>('/invoices', payload);
  return data;
}

// ─── POST /api/v1/invoices/{id}/issue ────────────────────────────────────────
export async function issueInvoice(id: string): Promise<InvoiceDetailDto> {
  const { data } = await client.post<InvoiceDetailDto>(`/invoices/${id}/issue`);
  return data;
}

// ─── POST /api/v1/invoices/{id}/pay ──────────────────────────────────────────
export async function payInvoice(id: string, paidDate: string): Promise<InvoiceDetailDto> {
  const payload: PayInvoiceRequest = { paidDate };
  const { data } = await client.post<InvoiceDetailDto>(`/invoices/${id}/pay`, payload);
  return data;
}

// ─── DELETE /api/v1/invoices/{id} ────────────────────────────────────────────
export async function cancelInvoice(id: string): Promise<void> {
  await client.delete(`/invoices/${id}`);
}

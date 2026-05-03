import client from './client';
import type {
  JournalEntryDto,
  AccountBalanceDto,
  CreateJournalEntryRequest,
  PagedResult,
  JournalFilter,
} from '../types';

// ─── GET /api/v1/journal ──────────────────────────────────────────────────────
export async function getJournalEntries(params: JournalFilter = {}): Promise<PagedResult<JournalEntryDto>> {
  const queryParams: Record<string, string | number> = {
    page: params.page ?? 1,
    pageSize: params.pageSize ?? 20,
  };
  if (params.search) queryParams.search = params.search;
  if (params.dateFrom) queryParams.dateFrom = params.dateFrom;
  if (params.dateTo) queryParams.dateTo = params.dateTo;

  const { data } = await client.get<PagedResult<JournalEntryDto>>('/journal', { params: queryParams });
  return data;
}

// ─── GET /api/v1/journal/balances?asOfDate= ──────────────────────────────────
export async function getBalances(asOfDate?: string): Promise<AccountBalanceDto[]> {
  const params: Record<string, string> = {};
  if (asOfDate) params.asOfDate = asOfDate;

  const { data } = await client.get<AccountBalanceDto[]>('/journal/balances', { params });
  return data;
}

// ─── POST /api/v1/journal ─────────────────────────────────────────────────────
export async function postJournalEntry(payload: CreateJournalEntryRequest): Promise<JournalEntryDto> {
  const { data } = await client.post<JournalEntryDto>('/journal', payload);
  return data;
}

// ─── POST /api/v1/journal/{id}/reverse ───────────────────────────────────────
export async function reverseEntry(id: string, number: string): Promise<JournalEntryDto> {
  const { data } = await client.post<JournalEntryDto>(`/journal/${id}/reverse`, { number });
  return data;
}

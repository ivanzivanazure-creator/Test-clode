import client from './client';
import type { DashboardDto } from '../types';

// ─── GET /api/v1/dashboard ────────────────────────────────────────────────────
export async function getDashboard(): Promise<DashboardDto> {
  const { data } = await client.get<DashboardDto>('/dashboard');
  return data;
}

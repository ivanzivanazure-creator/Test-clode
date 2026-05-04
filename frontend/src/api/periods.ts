import client from './client'

export interface AccountingPeriodDto {
  id: number
  month: number
  year: number
  isLocked: boolean
  lockedAt: string | null
  lockedByUserId: string | null
}

export async function getPeriods(year: number): Promise<AccountingPeriodDto[]> {
  const res = await client.get<AccountingPeriodDto[]>('/periods', { params: { year } })
  return res.data
}

export async function lockPeriod(id: number): Promise<void> {
  await client.post(`/periods/${id}/lock`)
}

export async function unlockPeriod(id: number): Promise<void> {
  await client.post(`/periods/${id}/unlock`)
}

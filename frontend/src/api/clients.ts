import client from './client'

export type ClientType = 'Kupac' | 'Dobavljac' | 'Oba'

export interface ClientDto {
  id: number
  tenantId: number
  name: string
  pib: string | null
  maticniBroj: string | null
  address: string | null
  city: string | null
  country: string
  iban: string | null
  email: string | null
  phone: string | null
  isActive: boolean
  clientType: ClientType
}

export interface CreateClientData {
  name: string
  clientType: ClientType
  pib?: string
  maticniBroj?: string
  address?: string
  city?: string
  iban?: string
  email?: string
  phone?: string
}

export interface UpdateClientData {
  name: string
  pib?: string
  maticniBroj?: string
  address?: string
  city?: string
  iban?: string
  email?: string
  phone?: string
}

export async function getClients(search?: string): Promise<ClientDto[]> {
  const params = search ? { search } : {}
  const res = await client.get<ClientDto[]>('/clients', { params })
  return res.data
}

export async function createClient(data: CreateClientData): Promise<number> {
  const res = await client.post<number>('/clients', data)
  return res.data
}

export async function updateClient(id: number, data: UpdateClientData): Promise<void> {
  await client.put(`/clients/${id}`, data)
}

export async function deactivateClient(id: number): Promise<void> {
  await client.delete(`/clients/${id}`)
}

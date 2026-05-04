import client from './client'

export type UserRole = 'Admin' | 'Accountant' | 'Viewer'

export interface UserDto {
  id: number
  username: string
  email: string
  fullName: string
  role: UserRole
  isActive: boolean
}

export interface CreateUserData {
  username: string
  password: string
  email: string
  fullName: string
  role: UserRole
}

export interface ChangePasswordData {
  currentPassword: string
  newPassword: string
}

export async function getUsers(): Promise<UserDto[]> {
  const res = await client.get<UserDto[]>('/users')
  return res.data
}

export async function createUser(data: CreateUserData): Promise<number> {
  const res = await client.post<number>('/users', data)
  return res.data
}

export async function changePassword(id: number, data: ChangePasswordData): Promise<void> {
  await client.post(`/users/${id}/change-password`, data)
}

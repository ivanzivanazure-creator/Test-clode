import client from './client';
import type {
  EmployeeDto,
  PayrollResultDto,
  CreateEmployeeRequest,
  UpdateSalaryRequest,
  TerminateEmployeeRequest,
} from '../types';

// ─── GET /api/v1/employees ────────────────────────────────────────────────────
export async function getEmployees(): Promise<EmployeeDto[]> {
  const { data } = await client.get<EmployeeDto[]>('/employees');
  return data;
}

// ─── GET /api/v1/employees/{id}/payroll ──────────────────────────────────────
export async function getPayroll(id: string): Promise<PayrollResultDto> {
  const { data } = await client.get<PayrollResultDto>(`/employees/${id}/payroll`);
  return data;
}

// ─── POST /api/v1/employees ───────────────────────────────────────────────────
export async function createEmployee(payload: CreateEmployeeRequest): Promise<EmployeeDto> {
  const { data } = await client.post<EmployeeDto>('/employees', payload);
  return data;
}

// ─── PUT /api/v1/employees/{id}/salary ───────────────────────────────────────
export async function updateSalary(id: string, payload: UpdateSalaryRequest): Promise<EmployeeDto> {
  const { data } = await client.put<EmployeeDto>(`/employees/${id}/salary`, payload);
  return data;
}

// ─── POST /api/v1/employees/{id}/terminate ───────────────────────────────────
export async function terminateEmployee(id: string, terminationDate: string, reason?: string): Promise<EmployeeDto> {
  const payload: TerminateEmployeeRequest = { terminationDate, reason };
  const { data } = await client.post<EmployeeDto>(`/employees/${id}/terminate`, payload);
  return data;
}

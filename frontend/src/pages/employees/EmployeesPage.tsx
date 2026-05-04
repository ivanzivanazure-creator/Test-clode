import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getEmployees, terminateEmployee } from '../../api/employees'
import { Button } from '../../components/ui/Button'
import { formatMoney, formatDate } from '../../utils/formatters'

export function EmployeesPage() {
  const qc = useQueryClient()
  const { data, isLoading } = useQuery({ queryKey: ['employees'], queryFn: getEmployees })

  const terminateMut = useMutation({
    mutationFn: (id: number) => terminateEmployee(id, new Date().toISOString().slice(0, 10)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['employees'] }),
  })

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Zaposleni</h1>
        <Link to="/zaposleni/novi">
          <Button variant="primary">+ Novi zaposleni</Button>
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Ime i prezime', 'Pozicija', 'Bruto plata', 'Datum zaposlenja', 'Status', 'Akcije'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading
              ? [...Array(6)].map((_, i) => (
                  <tr key={i}>
                    {[...Array(6)].map((_, j) => (
                      <td key={j} className="px-4 py-3"><div className="h-4 bg-gray-200 rounded animate-pulse" /></td>
                    ))}
                  </tr>
                ))
              : data?.map(emp => (
                  <tr key={emp.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{emp.fullName}</td>
                    <td className="px-4 py-3 text-gray-600">{emp.position}</td>
                    <td className="px-4 py-3 font-medium">{formatMoney(emp.grossSalary)}</td>
                    <td className="px-4 py-3 text-gray-600">{formatDate(emp.hireDate)}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        emp.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
                      }`}>
                        {emp.isActive ? 'Aktivan' : 'Neaktivan'}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        {emp.isActive && (
                          <Button size="sm" variant="danger"
                            onClick={() => { if (confirm(`Prekinuti radni odnos za ${emp.fullName}?`)) terminateMut.mutate(emp.id) }}>
                            Prekini
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
            }
          </tbody>
        </table>
      </div>
    </div>
  )
}

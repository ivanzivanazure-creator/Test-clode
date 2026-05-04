import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getJournalEntries, reverseEntry } from '../../api/journal'
import { Badge } from '../../components/ui/Badge'
import { Button } from '../../components/ui/Button'
import { Pagination } from '../../components/ui/Pagination'
import { formatMoney, formatDate } from '../../utils/formatters'

export function JournalPage() {
  const [page, setPage] = useState(1)
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['journal', page],
    queryFn: () => getJournalEntries({ page, pageSize: 20 }),
  })

  const reverseMut = useMutation({
    mutationFn: (id: number) => reverseEntry(id, `STORNO-${id}-${Date.now()}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['journal'] }),
  })

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Temeljnice</h1>
        <Link to="/temeljnice/nova">
          <Button variant="primary">+ Nova temeljnica</Button>
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Broj', 'Datum', 'Opis', 'Duguje', 'Potražuje', 'Status', 'Akcije'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading
              ? [...Array(8)].map((_, i) => (
                  <tr key={i}>
                    {[...Array(7)].map((_, j) => (
                      <td key={j} className="px-4 py-3"><div className="h-4 bg-gray-200 rounded animate-pulse" /></td>
                    ))}
                  </tr>
                ))
              : data?.items.map(entry => (
                  <tr key={entry.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-blue-700">{entry.number}</td>
                    <td className="px-4 py-3 text-gray-600">{formatDate(entry.date)}</td>
                    <td className="px-4 py-3 text-gray-800 max-w-xs truncate">{entry.description}</td>
                    <td className="px-4 py-3 font-medium">{formatMoney(entry.totalDebit)}</td>
                    <td className="px-4 py-3 font-medium">{formatMoney(entry.totalCredit)}</td>
                    <td className="px-4 py-3"><Badge status={entry.status} /></td>
                    <td className="px-4 py-3">
                      {entry.status === 'Proknjiženo' && (
                        <Button size="sm" variant="secondary" onClick={() => reverseMut.mutate(entry.id)}>
                          Storno
                        </Button>
                      )}
                    </td>
                  </tr>
                ))
            }
          </tbody>
        </table>
        {data && (
          <div className="px-4 py-3 border-t border-gray-200">
            <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
          </div>
        )}
      </div>
    </div>
  )
}

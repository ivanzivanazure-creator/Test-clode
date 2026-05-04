import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getInvoices, issueInvoice, cancelInvoice } from '../../api/invoices'
import { Badge } from '../../components/ui/Badge'
import { Button } from '../../components/ui/Button'
import { Pagination } from '../../components/ui/Pagination'
import { formatMoney, formatDate } from '../../utils/formatters'
import { InvoiceStatus } from '../../types'

export function InvoicesPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['invoices', page, status, search],
    queryFn: () => getInvoices({ page, pageSize: 20, status: status || undefined, search: search || undefined }),
  })

  const issueMut   = useMutation({ mutationFn: issueInvoice,  onSuccess: () => qc.invalidateQueries({ queryKey: ['invoices'] }) })
  const cancelMut  = useMutation({ mutationFn: cancelInvoice, onSuccess: () => qc.invalidateQueries({ queryKey: ['invoices'] }) })

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Fakture</h1>
        <Link to="/fakture/nova">
          <Button variant="primary">+ Nova faktura</Button>
        </Link>
      </div>

      <div className="flex gap-3 flex-wrap">
        <input
          type="text"
          placeholder="Pretraga po broju..."
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 w-56"
        />
        <select
          value={status}
          onChange={e => { setStatus(e.target.value); setPage(1) }}
          className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Svi statusi</option>
          {Object.values(InvoiceStatus).map(s => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Broj', 'Klijent', 'Datum', 'Valuta', 'Iznos', 'Status', 'Akcije'].map(h => (
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
              : data?.items.map(inv => (
                  <tr key={inv.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-blue-700">
                      <Link to={`/fakture/${inv.id}`}>{inv.number}</Link>
                    </td>
                    <td className="px-4 py-3 text-gray-600">{inv.clientId}</td>
                    <td className="px-4 py-3 text-gray-600">{formatDate(inv.issueDate)}</td>
                    <td className="px-4 py-3 text-gray-600">{formatDate(inv.dueDate)}</td>
                    <td className="px-4 py-3 font-medium">{formatMoney(inv.totalAmount)}</td>
                    <td className="px-4 py-3"><Badge status={inv.status} /></td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        {inv.status === 'Nacrt' && (
                          <Button size="sm" variant="secondary" onClick={() => issueMut.mutate(inv.id)}>
                            Izdaj
                          </Button>
                        )}
                        {inv.status !== 'Otkazana' && inv.status !== 'Plaćena' && (
                          <Button size="sm" variant="danger" onClick={() => cancelMut.mutate(inv.id)}>
                            Otkaži
                          </Button>
                        )}
                      </div>
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

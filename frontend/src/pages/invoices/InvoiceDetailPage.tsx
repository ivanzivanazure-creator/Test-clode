import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getInvoice, issueInvoice, payInvoice, cancelInvoice } from '../../api/invoices'
import { Badge } from '../../components/ui/Badge'
import { Button } from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { formatMoney, formatDate } from '../../utils/formatters'

export function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data: inv, isLoading } = useQuery({
    queryKey: ['invoice', id],
    queryFn: () => getInvoice(Number(id)),
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['invoice', id] })
  const issueMut  = useMutation({ mutationFn: () => issueInvoice(Number(id)),  onSuccess: invalidate })
  const payMut    = useMutation({ mutationFn: () => payInvoice(Number(id), DateOnly.today()), onSuccess: invalidate })
  const cancelMut = useMutation({ mutationFn: () => cancelInvoice(Number(id)), onSuccess: invalidate })

  if (isLoading) return <div className="animate-pulse h-64 bg-gray-200 rounded-xl" />
  if (!inv) return <div className="text-red-600">Faktura nije pronađena</div>

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="flex items-center justify-between">
        <div>
          <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline mb-1">← Nazad</button>
          <h1 className="text-2xl font-bold text-gray-900">Faktura {inv.number}</h1>
        </div>
        <div className="flex items-center gap-3">
          <Badge status={inv.status} />
          {inv.status === 'Nacrt' && <Button variant="primary" onClick={() => issueMut.mutate()}>Izdaj fakturu</Button>}
          {(inv.status === 'Izdata' || inv.status === 'Dospela') && (
            <Button variant="primary" onClick={() => payMut.mutate()}>Označi plaćenom</Button>
          )}
          {inv.status !== 'Otkazana' && inv.status !== 'Plaćena' && (
            <Button variant="danger" onClick={() => cancelMut.mutate()}>Otkaži</Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          ['Datum izdavanja', formatDate(inv.issueDate)],
          ['Valuta plaćanja', formatDate(inv.dueDate)],
          ['Klijent ID', inv.clientId],
          ['Integritetni hash', inv.integrityHash ? inv.integrityHash.slice(0, 12) + '…' : '—'],
        ].map(([label, value]) => (
          <div key={label} className="bg-white border border-gray-200 rounded-lg p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide">{label}</p>
            <p className="font-semibold text-gray-900 mt-1">{value}</p>
          </div>
        ))}
      </div>

      <Card title="Stavke">
        <table className="w-full text-sm">
          <thead className="border-b border-gray-200">
            <tr>
              {['Opis', 'Kol.', 'Jed.', 'Cijena', 'PDV %', 'PDV', 'Ukupno'].map(h => (
                <th key={h} className="py-2 text-left text-xs font-semibold text-gray-500 uppercase">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {inv.items?.map((item, i) => (
              <tr key={i}>
                <td className="py-2">{item.description}</td>
                <td className="py-2">{item.quantity}</td>
                <td className="py-2 text-gray-500">{item.unit}</td>
                <td className="py-2">{formatMoney(item.unitPrice)}</td>
                <td className="py-2">{item.vatRatePercent}%</td>
                <td className="py-2">{formatMoney(item.vatAmount)}</td>
                <td className="py-2 font-medium">{formatMoney(item.totalWithVat)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="border-t border-gray-200 mt-4 pt-4 flex justify-end">
          <div className="space-y-1 text-sm min-w-48">
            <div className="flex justify-between"><span className="text-gray-600">Osnovica:</span><span>{formatMoney(inv.amount)}</span></div>
            <div className="flex justify-between"><span className="text-gray-600">PDV:</span><span>{formatMoney(inv.taxAmount)}</span></div>
            <div className="flex justify-between font-bold text-base border-t pt-1 mt-1"><span>Ukupno:</span><span className="text-blue-700">{formatMoney(inv.totalAmount)}</span></div>
          </div>
        </div>
      </Card>
    </div>
  )
}

// Minimal shim — replace with real date picker in production
const DateOnly = { today: () => new Date().toISOString().slice(0, 10) as unknown as string }

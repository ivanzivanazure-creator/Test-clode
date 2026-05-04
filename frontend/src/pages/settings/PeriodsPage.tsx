import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getPeriods, lockPeriod, unlockPeriod, type AccountingPeriodDto } from '../../api/periods'
import { Button } from '../../components/ui/Button'

const MONTH_NAMES = [
  'Januar', 'Februar', 'Mart', 'April', 'Maj', 'Jun',
  'Jul', 'Avgust', 'Septembar', 'Oktobar', 'Novembar', 'Decembar',
]

function formatDateTime(iso: string | null): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString('sr-RS', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

// Build a full 12-month list, merging API data in where available
function buildMonths(year: number, data: AccountingPeriodDto[]): (AccountingPeriodDto | null)[] {
  return Array.from({ length: 12 }, (_, i) => {
    const month = i + 1
    return data.find(p => p.month === month) ?? null
  })
}

export function PeriodsPage() {
  const qc = useQueryClient()
  const [year, setYear] = useState(new Date().getFullYear())

  const { data = [], isLoading } = useQuery({
    queryKey: ['periods', year],
    queryFn:  () => getPeriods(year),
  })

  const lockMut = useMutation({
    mutationFn: (id: number) => lockPeriod(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['periods', year] }),
  })

  const unlockMut = useMutation({
    mutationFn: (id: number) => unlockPeriod(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['periods', year] }),
  })

  const months = buildMonths(year, data)

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Računovodstveni periodi</h1>
        <div className="flex items-center gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={() => setYear(y => y - 1)}
          >
            &larr; {year - 1}
          </Button>
          <span className="text-lg font-semibold text-gray-800 w-16 text-center">{year}</span>
          <Button
            size="sm"
            variant="secondary"
            onClick={() => setYear(y => y + 1)}
          >
            {year + 1} &rarr;
          </Button>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Mesec', 'Status', 'Zaključao', 'Datum zaključavanja', 'Akcija'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading
              ? [...Array(12)].map((_, i) => (
                  <tr key={i}>
                    {[...Array(5)].map((_, j) => (
                      <td key={j} className="px-4 py-3">
                        <div className="h-4 bg-gray-200 rounded animate-pulse" />
                      </td>
                    ))}
                  </tr>
                ))
              : months.map((period, idx) => {
                  const monthName = MONTH_NAMES[idx]
                  const isPending = lockMut.isPending || unlockMut.isPending

                  if (!period) {
                    // Period not yet created in DB — show as unlocked/open
                    return (
                      <tr key={idx} className="hover:bg-gray-50">
                        <td className="px-4 py-3 font-medium text-gray-900">{monthName}</td>
                        <td className="px-4 py-3">
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                            <span className="h-1.5 w-1.5 rounded-full bg-green-500" />
                            Otvoreno
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-400">—</td>
                        <td className="px-4 py-3 text-gray-400">—</td>
                        <td className="px-4 py-3 text-gray-400 text-xs italic">Period nije inicijalizovan</td>
                      </tr>
                    )
                  }

                  return (
                    <tr key={period.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">{monthName}</td>
                      <td className="px-4 py-3">
                        {period.isLocked ? (
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">
                            <span className="h-1.5 w-1.5 rounded-full bg-red-500" />
                            Zaključano
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                            <span className="h-1.5 w-1.5 rounded-full bg-green-500" />
                            Otvoreno
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-gray-600 font-mono text-xs">
                        {period.lockedByUserId ?? '—'}
                      </td>
                      <td className="px-4 py-3 text-gray-600 text-xs">
                        {formatDateTime(period.lockedAt)}
                      </td>
                      <td className="px-4 py-3">
                        {period.isLocked ? (
                          <Button
                            size="sm"
                            variant="secondary"
                            loading={isPending}
                            onClick={() => {
                              if (confirm(`Otključati period ${monthName} ${year}?`))
                                unlockMut.mutate(period.id)
                            }}
                          >
                            Otključaj
                          </Button>
                        ) : (
                          <Button
                            size="sm"
                            variant="danger"
                            loading={isPending}
                            onClick={() => {
                              if (confirm(`Zaključati period ${monthName} ${year}? Naknadna knjiženja neće biti moguća.`))
                                lockMut.mutate(period.id)
                            }}
                          >
                            Zaključaj
                          </Button>
                        )}
                      </td>
                    </tr>
                  )
                })
            }
          </tbody>
        </table>
      </div>

      <p className="text-xs text-gray-400">
        Zaključani periodi sprečavaju retroaktivna knjiženja. Samo Administrator može otključati period.
      </p>
    </div>
  )
}

import { useNavigate } from 'react-router-dom'
import { useForm, useFieldArray } from 'react-hook-form'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { postJournalEntry } from '../../api/journal'
import { Button } from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { formatMoney } from '../../utils/formatters'

interface JournalLine {
  accountId: number
  debit: number
  credit: number
  note: string
}
interface JournalForm {
  number: string
  date: string
  description: string
  lines: JournalLine[]
}

export function NewJournalPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<JournalForm>({
    defaultValues: {
      date: new Date().toISOString().slice(0, 10),
      lines: [
        { accountId: 0, debit: 0, credit: 0, note: '' },
        { accountId: 0, debit: 0, credit: 0, note: '' },
      ],
    },
  })
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' })
  const watchedLines = watch('lines')

  const totalDebit  = watchedLines.reduce((s, l) => s + (l.debit  || 0), 0)
  const totalCredit = watchedLines.reduce((s, l) => s + (l.credit || 0), 0)
  const isBalanced  = Math.abs(totalDebit - totalCredit) < 0.01

  const mutation = useMutation({
    mutationFn: postJournalEntry,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['journal'] })
      navigate('/temeljnice')
    },
  })

  const onSubmit = (data: JournalForm) => {
    mutation.mutate({
      number: data.number,
      date: data.date,
      description: data.description,
      lines: data.lines.map(l => ({
        accountId: l.accountId,
        debit: l.debit,
        credit: l.credit,
        note: l.note,
      })),
    })
  }

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">← Nazad</button>
        <h1 className="text-2xl font-bold text-gray-900">Nova temeljnica</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card title="Zaglavlje">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Broj temeljnice *</label>
              <input {...register('number', { required: true })}
                placeholder="TEM-2024-001"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Datum *</label>
              <input type="date" {...register('date', { required: true })}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div className="md:col-span-1">
              <label className="block text-sm font-medium text-gray-700 mb-1">Opis *</label>
              <input {...register('description', { required: true })}
                placeholder="Opis knjiženja"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>
        </Card>

        <Card title="Stavke knjiženja">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200">
                  {['Konto', 'Duguje (RSD)', 'Potražuje (RSD)', 'Napomena', ''].map(h => (
                    <th key={h} className="py-2 text-left text-xs font-semibold text-gray-500 uppercase pr-3">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {fields.map((field, index) => (
                  <tr key={field.id}>
                    <td className="py-2 pr-3">
                      <input type="number" {...register(`lines.${index}.accountId`, { valueAsNumber: true, min: 1 })}
                        placeholder="Konto"
                        className="w-24 border border-gray-300 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </td>
                    <td className="py-2 pr-3">
                      <input type="number" step="0.01" {...register(`lines.${index}.debit`, { valueAsNumber: true, min: 0 })}
                        placeholder="0.00"
                        className="w-32 border border-gray-300 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </td>
                    <td className="py-2 pr-3">
                      <input type="number" step="0.01" {...register(`lines.${index}.credit`, { valueAsNumber: true, min: 0 })}
                        placeholder="0.00"
                        className="w-32 border border-gray-300 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </td>
                    <td className="py-2 pr-3">
                      <input {...register(`lines.${index}.note`)}
                        placeholder="Napomena"
                        className="w-full border border-gray-300 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </td>
                    <td className="py-2">
                      <button type="button" onClick={() => remove(index)} className="text-red-400 hover:text-red-600 text-lg">×</button>
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="border-t-2 border-gray-300">
                <tr>
                  <td className="py-2 font-semibold text-gray-700">Ukupno:</td>
                  <td className={`py-2 font-bold ${isBalanced ? 'text-green-700' : 'text-red-700'}`}>{formatMoney(totalDebit)}</td>
                  <td className={`py-2 font-bold ${isBalanced ? 'text-green-700' : 'text-red-700'}`}>{formatMoney(totalCredit)}</td>
                  <td colSpan={2} className="py-2">
                    {isBalanced
                      ? <span className="text-green-600 text-xs font-semibold">✓ Uravnoteženo</span>
                      : <span className="text-red-600 text-xs font-semibold">✗ Razlika: {formatMoney(Math.abs(totalDebit - totalCredit))}</span>
                    }
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
          <button type="button"
            onClick={() => append({ accountId: 0, debit: 0, credit: 0, note: '' })}
            className="text-sm text-blue-600 hover:underline mt-3">
            + Dodaj stavku
          </button>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Odustani</Button>
          <Button type="submit" variant="primary" disabled={!isBalanced} loading={mutation.isPending}>
            Proknjizi
          </Button>
        </div>
      </form>
    </div>
  )
}

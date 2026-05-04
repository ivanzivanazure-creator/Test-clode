import { useNavigate } from 'react-router-dom'
import { useForm, useFieldArray } from 'react-hook-form'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createInvoice } from '../../api/invoices'
import { Button } from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { formatMoney } from '../../utils/formatters'

interface InvoiceFormItem {
  description: string
  quantity: number
  unitPrice: number
  vatRate: number
}

interface InvoiceForm {
  number: string
  clientId: number
  dueDate: string
  items: InvoiceFormItem[]
}

const VAT_RATES = [0, 8, 10, 20]

export function NewInvoicePage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<InvoiceForm>({
    defaultValues: { items: [{ description: '', quantity: 1, unitPrice: 0, vatRate: 20 }] }
  })
  const { fields, append, remove } = useFieldArray({ control, name: 'items' })
  const watchedItems = watch('items')

  const total = watchedItems.reduce((sum, item) => {
    const base = (item.quantity || 0) * (item.unitPrice || 0)
    return sum + base + base * (item.vatRate || 0) / 100
  }, 0)

  const mutation = useMutation({
    mutationFn: createInvoice,
    onSuccess: (id) => {
      qc.invalidateQueries({ queryKey: ['invoices'] })
      navigate(`/fakture/${id}`)
    },
  })

  const onSubmit = (data: InvoiceForm) => {
    mutation.mutate({
      number: data.number,
      clientId: data.clientId,
      dueDate: data.dueDate,
      items: data.items.map(i => ({
        description: i.description,
        quantity: i.quantity,
        unitPrice: i.unitPrice,
        vatRate: i.vatRate,
      })),
    })
  }

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">← Nazad</button>
        <h1 className="text-2xl font-bold text-gray-900">Nova faktura</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card title="Podaci fakture">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Broj fakture *</label>
              <input {...register('number', { required: true })}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="2024-001" />
              {errors.number && <p className="text-red-500 text-xs mt-1">Obavezno polje</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Klijent ID *</label>
              <input type="number" {...register('clientId', { required: true, valueAsNumber: true })}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Valuta plaćanja *</label>
              <input type="date" {...register('dueDate', { required: true })}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>
        </Card>

        <Card title="Stavke">
          <div className="space-y-3">
            {fields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-12 gap-2 items-start">
                <div className="col-span-4">
                  <input {...register(`items.${index}.description`, { required: true })}
                    placeholder="Opis usluge/robe"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div className="col-span-2">
                  <input type="number" step="0.01" {...register(`items.${index}.quantity`, { valueAsNumber: true, min: 0.01 })}
                    placeholder="Kol."
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div className="col-span-2">
                  <input type="number" step="0.01" {...register(`items.${index}.unitPrice`, { valueAsNumber: true, min: 0 })}
                    placeholder="Cijena"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div className="col-span-2">
                  <select {...register(`items.${index}.vatRate`, { valueAsNumber: true })}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                    {VAT_RATES.map(r => <option key={r} value={r}>{r}% PDV</option>)}
                  </select>
                </div>
                <div className="col-span-1 text-right pt-2 text-sm text-gray-600">
                  {formatMoney(
                    ((watchedItems[index]?.quantity || 0) * (watchedItems[index]?.unitPrice || 0)) *
                    (1 + (watchedItems[index]?.vatRate || 0) / 100)
                  )}
                </div>
                <div className="col-span-1 flex justify-end">
                  <button type="button" onClick={() => remove(index)}
                    className="text-red-500 hover:text-red-700 text-lg font-bold mt-1">×</button>
                </div>
              </div>
            ))}
            <button type="button"
              onClick={() => append({ description: '', quantity: 1, unitPrice: 0, vatRate: 20 })}
              className="text-sm text-blue-600 hover:underline">
              + Dodaj stavku
            </button>
          </div>

          <div className="border-t border-gray-200 mt-4 pt-4 flex justify-end">
            <div className="text-lg font-bold text-blue-700">
              Ukupno (sa PDV): {formatMoney(total)}
            </div>
          </div>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Odustani</Button>
          <Button type="submit" variant="primary" loading={mutation.isPending}>Kreiraj fakturu</Button>
        </div>
      </form>
    </div>
  )
}

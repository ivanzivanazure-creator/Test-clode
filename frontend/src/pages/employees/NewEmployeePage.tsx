import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createEmployee } from '../../api/employees'
import { Button } from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'

interface EmployeeForm {
  firstName: string
  lastName: string
  position: string
  grossSalary: number
  hireDate: string
  taxExemption: string
  jmbg?: string
  bankAccount?: string
  email?: string
}

export function NewEmployeePage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { register, handleSubmit, formState: { errors } } = useForm<EmployeeForm>({
    defaultValues: { hireDate: new Date().toISOString().slice(0, 10), taxExemption: 'None' }
  })

  const mutation = useMutation({
    mutationFn: createEmployee,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['employees'] })
      navigate('/zaposleni')
    },
  })

  const onSubmit = (data: EmployeeForm) => mutation.mutate(data)

  const field = (label: string, key: keyof EmployeeForm, type = 'text', opts?: object) => (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <input type={type} {...register(key, opts)}
        className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      {errors[key] && <p className="text-red-500 text-xs mt-1">Obavezno polje</p>}
    </div>
  )

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">← Nazad</button>
        <h1 className="text-2xl font-bold text-gray-900">Novi zaposleni</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card title="Osnovni podaci">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {field('Ime *', 'firstName', 'text', { required: true })}
            {field('Prezime *', 'lastName', 'text', { required: true })}
            {field('Pozicija *', 'position', 'text', { required: true })}
            {field('Bruto plata (RSD) *', 'grossSalary', 'number', { required: true, valueAsNumber: true, min: 1 })}
            {field('Datum zaposlenja *', 'hireDate', 'date', { required: true })}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Poresko oslobođenje</label>
              <select {...register('taxExemption')}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option value="None">Bez oslobođenja</option>
                <option value="FirstJob">Prvo zaposlenje</option>
                <option value="DisabledPerson">Lice sa invaliditetom</option>
              </select>
            </div>
          </div>
        </Card>

        <Card title="Lični podaci (enkriptovano)">
          <p className="text-xs text-gray-500 mb-4">Podaci se čuvaju enkriptovani (AES-256-GCM) u skladu sa ZZPL 87/2018</p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {field('JMBG (13 cifara)', 'jmbg')}
            {field('Broj bankovnog računa', 'bankAccount')}
            {field('Email adresa', 'email', 'email')}
          </div>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Odustani</Button>
          <Button type="submit" variant="primary" loading={mutation.isPending}>Sačuvaj</Button>
        </div>
      </form>
    </div>
  )
}

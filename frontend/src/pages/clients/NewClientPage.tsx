import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { createClient, type ClientType, type CreateClientData } from '../../api/clients'
import { Button } from '../../components/ui/Button'

const CLIENT_TYPES: { value: ClientType; label: string }[] = [
  { value: 'Kupac',     label: 'Kupac' },
  { value: 'Dobavljac', label: 'Dobavljač' },
  { value: 'Oba',       label: 'Kupac i Dobavljač' },
]

interface FormState {
  name: string
  clientType: ClientType
  pib: string
  maticniBroj: string
  address: string
  city: string
  iban: string
  email: string
  phone: string
}

const initial: FormState = {
  name: '',
  clientType: 'Kupac',
  pib: '',
  maticniBroj: '',
  address: '',
  city: '',
  iban: '',
  email: '',
  phone: '',
}

export function NewClientPage() {
  const navigate = useNavigate()
  const [form, setForm]     = useState<FormState>(initial)
  const [errors, setErrors] = useState<Partial<FormState>>({})

  const mut = useMutation({
    mutationFn: (data: CreateClientData) => createClient(data),
    onSuccess: () => navigate('/klijenti'),
  })

  const set = (key: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setForm(f => ({ ...f, [key]: e.target.value }))

  function validate(): boolean {
    const errs: Partial<FormState> = {}
    if (!form.name.trim()) errs.name = 'Naziv je obavezan.'
    if (form.pib && !/^\d{9}$/.test(form.pib))
      errs.pib = 'PIB mora sadržati tačno 9 cifara.'
    if (form.maticniBroj && !/^\d{8}$/.test(form.maticniBroj))
      errs.maticniBroj = 'Matični broj mora sadržati tačno 8 cifara.'
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      errs.email = 'Email adresa nije ispravna.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return

    const data: CreateClientData = {
      name:        form.name.trim(),
      clientType:  form.clientType,
      pib:         form.pib || undefined,
      maticniBroj: form.maticniBroj || undefined,
      address:     form.address || undefined,
      city:        form.city || undefined,
      iban:        form.iban || undefined,
      email:       form.email || undefined,
      phone:       form.phone || undefined,
    }
    mut.mutate(data)
  }

  const inputCls = (err?: string) =>
    `w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${
      err ? 'border-red-400' : 'border-gray-300'
    }`

  const Field = ({
    label, name, hint, type = 'text',
  }: { label: string; name: keyof FormState; hint?: string; type?: string }) => (
    <div>
      <label className="block text-xs font-medium text-gray-700 mb-1">{label}</label>
      <input
        type={type}
        className={inputCls(errors[name])}
        value={form[name] as string}
        onChange={set(name)}
      />
      {hint && !errors[name] && <p className="mt-1 text-xs text-gray-400">{hint}</p>}
      {errors[name] && <p className="mt-1 text-xs text-red-600">{errors[name]}</p>}
    </div>
  )

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/klijenti')}
          className="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
          </svg>
        </button>
        <h1 className="text-2xl font-bold text-gray-900">Novi klijent</h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 space-y-5">
        {/* Basic */}
        <section className="space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wider border-b pb-2">
            Osnovni podaci
          </h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <Field label="Naziv *" name="name" />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">Tip klijenta *</label>
              <select
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={form.clientType}
                onChange={set('clientType')}
              >
                {CLIENT_TYPES.map(t => (
                  <option key={t.value} value={t.value}>{t.label}</option>
                ))}
              </select>
            </div>
          </div>
        </section>

        {/* Legal */}
        <section className="space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wider border-b pb-2">
            Pravni podaci
          </h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="PIB" name="pib" hint="9 cifara (opciono)" />
            <Field label="Matični broj" name="maticniBroj" hint="8 cifara (opciono)" />
          </div>
        </section>

        {/* Contact */}
        <section className="space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wider border-b pb-2">
            Kontakt i adresa
          </h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <Field label="Adresa" name="address" />
            </div>
            <Field label="Grad" name="city" />
            <Field label="IBAN" name="iban" />
            <Field label="Email" name="email" type="email" />
            <Field label="Telefon" name="phone" type="tel" />
          </div>
        </section>

        {mut.isError && (
          <p className="text-sm text-red-600">Greška pri kreiranju klijenta. Pokušajte ponovo.</p>
        )}

        <div className="flex gap-3 justify-end pt-2">
          <Button type="button" variant="secondary" onClick={() => navigate('/klijenti')}>
            Otkaži
          </Button>
          <Button type="submit" variant="primary" loading={mut.isPending}>
            Kreiraj klijenta
          </Button>
        </div>
      </form>
    </div>
  )
}

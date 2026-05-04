import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getClients, updateClient, deactivateClient,
  type ClientDto, type ClientType, type UpdateClientData,
} from '../../api/clients'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'

// ─── Type badge ───────────────────────────────────────────────────────────────

function ClientTypeBadge({ type }: { type: ClientType }) {
  const map: Record<ClientType, { label: string; cls: string }> = {
    Kupac:     { label: 'Kupac',    cls: 'bg-blue-100 text-blue-800' },
    Dobavljac: { label: 'Dobavljač', cls: 'bg-purple-100 text-purple-800' },
    Oba:       { label: 'Oba',      cls: 'bg-amber-100 text-amber-800' },
  }
  const { label, cls } = map[type]
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${cls}`}>
      {label}
    </span>
  )
}

// ─── Status badge ─────────────────────────────────────────────────────────────

function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
      isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-500'
    }`}>
      {isActive ? 'Aktivan' : 'Neaktivan'}
    </span>
  )
}

// ─── Edit modal ───────────────────────────────────────────────────────────────

interface EditModalProps {
  client: ClientDto
  onClose: () => void
}

function EditModal({ client, onClose }: EditModalProps) {
  const qc = useQueryClient()
  const [form, setForm] = useState<UpdateClientData>({
    name:        client.name,
    pib:         client.pib ?? '',
    maticniBroj: client.maticniBroj ?? '',
    address:     client.address ?? '',
    city:        client.city ?? '',
    iban:        client.iban ?? '',
    email:       client.email ?? '',
    phone:       client.phone ?? '',
  })

  const mut = useMutation({
    mutationFn: (data: UpdateClientData) => updateClient(client.id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['clients'] })
      onClose()
    },
  })

  const set = (key: keyof UpdateClientData) =>
    (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm(f => ({ ...f, [key]: e.target.value }))

  const field = (label: string, key: keyof UpdateClientData, hint?: string) => (
    <div>
      <label className="block text-xs font-medium text-gray-700 mb-1">{label}</label>
      <input
        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        value={(form[key] as string) ?? ''}
        onChange={set(key)}
      />
      {hint && <p className="mt-1 text-xs text-gray-400">{hint}</p>}
    </div>
  )

  return (
    <Modal
      open
      onClose={onClose}
      title="Izmeni klijenta"
      size="lg"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>Otkaži</Button>
          <Button
            variant="primary"
            loading={mut.isPending}
            onClick={() => mut.mutate(form)}
          >
            Sačuvaj
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          {field('Naziv *', 'name')}
        </div>
        {field('PIB', 'pib', '9 cifara')}
        {field('Matični broj', 'maticniBroj', '8 cifara')}
        {field('Adresa', 'address')}
        {field('Grad', 'city')}
        {field('IBAN', 'iban')}
        {field('Email', 'email')}
        {field('Telefon', 'phone')}
      </div>
      {mut.isError && (
        <p className="mt-3 text-sm text-red-600">Greška pri čuvanju. Pokušajte ponovo.</p>
      )}
    </Modal>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ClientsPage() {
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [editClient, setEditClient] = useState<ClientDto | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['clients', search],
    queryFn:  () => getClients(search || undefined),
  })

  const deactivateMut = useMutation({
    mutationFn: (id: number) => deactivateClient(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['clients'] }),
  })

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Klijenti</h1>
        <Link to="/klijenti/novi">
          <Button variant="primary">+ Novi klijent</Button>
        </Link>
      </div>

      {/* Search */}
      <div>
        <input
          className="w-full max-w-sm rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Pretraži po nazivu, PIB-u, gradu..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Naziv', 'PIB', 'Grad', 'Tip', 'Status', 'Akcije'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading
              ? [...Array(6)].map((_, i) => (
                  <tr key={i}>
                    {[...Array(6)].map((_, j) => (
                      <td key={j} className="px-4 py-3">
                        <div className="h-4 bg-gray-200 rounded animate-pulse" />
                      </td>
                    ))}
                  </tr>
                ))
              : data?.map(c => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{c.name}</td>
                    <td className="px-4 py-3 text-gray-600 font-mono">{c.pib ?? '—'}</td>
                    <td className="px-4 py-3 text-gray-600">{c.city ?? '—'}</td>
                    <td className="px-4 py-3"><ClientTypeBadge type={c.clientType} /></td>
                    <td className="px-4 py-3"><StatusBadge isActive={c.isActive} /></td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" onClick={() => setEditClient(c)}>
                          Izmeni
                        </Button>
                        {c.isActive && (
                          <Button
                            size="sm"
                            variant="danger"
                            onClick={() => {
                              if (confirm(`Deaktivirati klijenta "${c.name}"?`))
                                deactivateMut.mutate(c.id)
                            }}
                          >
                            Deaktiviraj
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
            }
            {!isLoading && data?.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-gray-400 text-sm">
                  Nema klijenata koji odgovaraju pretrazi.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {editClient && (
        <EditModal client={editClient} onClose={() => setEditClient(null)} />
      )}
    </div>
  )
}

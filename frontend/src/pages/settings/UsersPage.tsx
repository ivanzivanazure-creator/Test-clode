import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getUsers, createUser,
  type UserDto, type UserRole, type CreateUserData,
} from '../../api/users'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'

// ─── Role badge ───────────────────────────────────────────────────────────────

function RoleBadge({ role }: { role: UserRole }) {
  const map: Record<UserRole, { label: string; cls: string }> = {
    Admin:      { label: 'Administrator', cls: 'bg-red-100 text-red-800' },
    Accountant: { label: 'Računovođa',    cls: 'bg-blue-100 text-blue-800' },
    Viewer:     { label: 'Preglednik',    cls: 'bg-gray-100 text-gray-700' },
  }
  const { label, cls } = map[role]
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${cls}`}>
      {label}
    </span>
  )
}

// ─── Create user modal ────────────────────────────────────────────────────────

interface CreateModalProps {
  onClose: () => void
}

const ROLES: { value: UserRole; label: string }[] = [
  { value: 'Admin',      label: 'Administrator' },
  { value: 'Accountant', label: 'Računovođa' },
  { value: 'Viewer',     label: 'Preglednik' },
]

interface CreateForm {
  username: string
  password: string
  email: string
  fullName: string
  role: UserRole
}

function CreateUserModal({ onClose }: CreateModalProps) {
  const qc = useQueryClient()
  const [form, setForm] = useState<CreateForm>({
    username: '',
    password: '',
    email:    '',
    fullName: '',
    role:     'Accountant',
  })
  const [errors, setErrors] = useState<Partial<CreateForm>>({})

  const mut = useMutation({
    mutationFn: (data: CreateUserData) => createUser(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['users'] })
      onClose()
    },
  })

  function validate(): boolean {
    const errs: Partial<CreateForm> = {}
    if (!form.username.trim()) errs.username = 'Korisničko ime je obavezno.'
    if (!form.password || form.password.length < 6) errs.password = 'Lozinka mora imati najmanje 6 znakova.'
    if (!form.email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      errs.email = 'Unesite ispravnu email adresu.'
    if (!form.fullName.trim()) errs.fullName = 'Puno ime je obavezno.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  const set = (key: keyof CreateForm) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setForm(f => ({ ...f, [key]: e.target.value }))

  const inputCls = (err?: string) =>
    `w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${
      err ? 'border-red-400' : 'border-gray-300'
    }`

  return (
    <Modal
      open
      onClose={onClose}
      title="Novi korisnik"
      size="md"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>Otkaži</Button>
          <Button
            variant="primary"
            loading={mut.isPending}
            onClick={() => { if (validate()) mut.mutate(form as CreateUserData) }}
          >
            Kreiraj
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {/* Username */}
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">Korisničko ime *</label>
          <input className={inputCls(errors.username)} value={form.username} onChange={set('username')} />
          {errors.username && <p className="mt-1 text-xs text-red-600">{errors.username}</p>}
        </div>

        {/* Password */}
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">Lozinka *</label>
          <input type="password" className={inputCls(errors.password)} value={form.password} onChange={set('password')} />
          {errors.password && <p className="mt-1 text-xs text-red-600">{errors.password}</p>}
        </div>

        {/* Email */}
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">Email *</label>
          <input type="email" className={inputCls(errors.email)} value={form.email} onChange={set('email')} />
          {errors.email && <p className="mt-1 text-xs text-red-600">{errors.email}</p>}
        </div>

        {/* Full name */}
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">Puno ime *</label>
          <input className={inputCls(errors.fullName)} value={form.fullName} onChange={set('fullName')} />
          {errors.fullName && <p className="mt-1 text-xs text-red-600">{errors.fullName}</p>}
        </div>

        {/* Role */}
        <div>
          <label className="block text-xs font-medium text-gray-700 mb-1">Uloga</label>
          <select
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.role}
            onChange={set('role')}
          >
            {ROLES.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
          </select>
        </div>

        {mut.isError && (
          <p className="text-sm text-red-600">Greška pri kreiranju korisnika. Pokušajte ponovo.</p>
        )}
      </div>
    </Modal>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function UsersPage() {
  const [showCreate, setShowCreate] = useState(false)

  const { data, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn:  getUsers,
  })

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Korisnici</h1>
        <Button variant="primary" onClick={() => setShowCreate(true)}>
          + Novi korisnik
        </Button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {['Puno ime', 'Korisničko ime', 'Email', 'Uloga', 'Status'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading
              ? [...Array(4)].map((_, i) => (
                  <tr key={i}>
                    {[...Array(5)].map((_, j) => (
                      <td key={j} className="px-4 py-3">
                        <div className="h-4 bg-gray-200 rounded animate-pulse" />
                      </td>
                    ))}
                  </tr>
                ))
              : data?.map((u: UserDto) => (
                  <tr key={u.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{u.fullName}</td>
                    <td className="px-4 py-3 text-gray-600 font-mono text-xs">{u.username}</td>
                    <td className="px-4 py-3 text-gray-600">{u.email}</td>
                    <td className="px-4 py-3"><RoleBadge role={u.role} /></td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        u.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-500'
                      }`}>
                        {u.isActive ? 'Aktivan' : 'Neaktivan'}
                      </span>
                    </td>
                  </tr>
                ))
            }
            {!isLoading && data?.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-400 text-sm">
                  Nema korisnika.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {showCreate && <CreateUserModal onClose={() => setShowCreate(false)} />}
    </div>
  )
}

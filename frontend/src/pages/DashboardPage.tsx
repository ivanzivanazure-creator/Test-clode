import { useQuery } from '@tanstack/react-query'
import { getDashboard } from '../api/dashboard'
import { StatCard } from '../components/ui/StatCard'
import { Card } from '../components/ui/Card'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { formatMoney } from '../utils/formatters'

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: getDashboard })

  if (isLoading) {
    return (
      <div className="space-y-6 animate-pulse">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => <div key={i} className="h-32 bg-gray-200 rounded-xl" />)}
        </div>
        <div className="h-64 bg-gray-200 rounded-xl" />
      </div>
    )
  }

  const d = data!

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Pregled</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="Ukupno faktura"
          value={d.totalInvoices.toString()}
          icon="document"
          color="blue"
        />
        <StatCard
          title="Prihod (naplaćeno)"
          value={formatMoney(d.paidRevenue)}
          icon="cash"
          color="green"
        />
        <StatCard
          title="Dospele fakture"
          value={formatMoney(d.overdueRevenue)}
          icon="warning"
          color="red"
          subtitle={`${d.overdueCount} faktura`}
        />
        <StatCard
          title="Aktivni zaposleni"
          value={d.activeEmployees.toString()}
          icon="users"
          color="purple"
          subtitle={`Ukupni troškovi: ${formatMoney(d.totalPayrollCost)}`}
        />
      </div>

      <Card title="Mesečni prihodi">
        <ResponsiveContainer width="100%" height={280}>
          <LineChart data={d.monthlyRevenue}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
            <XAxis dataKey="month" tick={{ fontSize: 12 }} />
            <YAxis tickFormatter={v => `${(v / 1000).toFixed(0)}k`} tick={{ fontSize: 12 }} />
            <Tooltip formatter={(v: number) => formatMoney(v)} />
            <Line type="monotone" dataKey="amount" stroke="#1e40af" strokeWidth={2} dot={{ r: 4 }} />
          </LineChart>
        </ResponsiveContainer>
      </Card>
    </div>
  )
}

import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  getBalanceSheet,
  getIncomeStatement,
  getVatReturn,
  getPayrollReport,
  type BalanceSheetDto,
  type IncomeStatementDto,
  type VatReturnDto,
  type PayrollReportDto,
  type ReportLineDto,
  type Money,
} from '../../api/reports';
import { formatMoney } from '../../utils/formatters';

// ─── Helpers ──────────────────────────────────────────────────────────────────

function fmt(m: Money | undefined): string {
  if (!m) return '—';
  return formatMoney(m.amount, m.currency);
}

function currentMonthYear() {
  const now = new Date();
  return { month: now.getMonth() + 1, year: now.getFullYear() };
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function monthOptions() {
  const months = [
    'Januar', 'Februar', 'Mart', 'April', 'Maj', 'Juni',
    'Juli', 'Avgust', 'Septembar', 'Oktobar', 'Novembar', 'Decembar',
  ];
  return months.map((label, i) => ({ label, value: i + 1 }));
}

function yearOptions() {
  const currentYear = new Date().getFullYear();
  return Array.from({ length: 6 }, (_, i) => currentYear - 5 + i).reverse();
}

// ─── Shared components ────────────────────────────────────────────────────────

function SectionHeader({ children }: { children: React.ReactNode }) {
  return (
    <tr className="bg-blue-50">
      <td colSpan={3} className="px-4 py-2 text-xs font-bold uppercase tracking-wide text-blue-800">
        {children}
      </td>
    </tr>
  );
}

function SummaryRow({
  label,
  value,
  highlight = false,
}: {
  label: string;
  value: string;
  highlight?: boolean;
}) {
  return (
    <tr className={highlight ? 'bg-blue-700 text-white font-bold' : 'bg-gray-50 font-semibold'}>
      <td className="px-4 py-2 text-sm">{label}</td>
      <td />
      <td className={`px-4 py-2 text-sm text-right ${highlight ? '' : 'text-blue-800'}`}>
        {value}
      </td>
    </tr>
  );
}

function ReportLines({ lines, label }: { lines: ReportLineDto[]; label: string }) {
  return (
    <>
      <SectionHeader>{label}</SectionHeader>
      {lines.length === 0 ? (
        <tr>
          <td colSpan={3} className="px-4 py-3 text-sm text-center text-gray-400">
            Nema podataka
          </td>
        </tr>
      ) : (
        lines.map((line) => (
          <tr key={line.code} className="hover:bg-gray-50 border-b border-gray-100">
            <td
              className="px-4 py-2 text-sm font-mono text-gray-500"
              style={{ paddingLeft: `${(line.level + 1) * 16}px` }}
            >
              {line.code}
            </td>
            <td className="px-4 py-2 text-sm text-gray-800">{line.name}</td>
            <td className="px-4 py-2 text-sm text-right font-medium">{fmt(line.balance)}</td>
          </tr>
        ))
      )}
    </>
  );
}

function PeriodSelector({
  month,
  year,
  onMonthChange,
  onYearChange,
}: {
  month: number;
  year: number;
  onMonthChange: (m: number) => void;
  onYearChange: (y: number) => void;
}) {
  return (
    <div className="flex items-center gap-3">
      <label className="text-sm font-medium text-gray-700">Period:</label>
      <select
        value={month}
        onChange={(e) => onMonthChange(Number(e.target.value))}
        className="rounded-md border border-gray-300 px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        {monthOptions().map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      <select
        value={year}
        onChange={(e) => onYearChange(Number(e.target.value))}
        className="rounded-md border border-gray-300 px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        {yearOptions().map((y) => (
          <option key={y} value={y}>
            {y}
          </option>
        ))}
      </select>
    </div>
  );
}

function LoadingRow() {
  return (
    <tr>
      <td colSpan={3} className="px-4 py-8 text-center text-sm text-gray-400">
        <div className="flex justify-center items-center gap-2">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
          Učitavanje...
        </div>
      </td>
    </tr>
  );
}

function ErrorRow({ message }: { message: string }) {
  return (
    <tr>
      <td colSpan={3} className="px-4 py-6 text-center text-sm text-red-600">
        {message}
      </td>
    </tr>
  );
}

// ─── Tab: Bilans stanja ───────────────────────────────────────────────────────

function BalanceSheetTab() {
  const [asOfDate, setAsOfDate] = useState(todayIso());

  const { data, isLoading, isError } = useQuery<BalanceSheetDto>({
    queryKey: ['reports', 'balance-sheet', asOfDate],
    queryFn: () => getBalanceSheet(asOfDate),
    enabled: !!asOfDate,
  });

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <label className="text-sm font-medium text-gray-700">Na dan:</label>
        <input
          type="date"
          value={asOfDate}
          onChange={(e) => setAsOfDate(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-blue-800 text-white">
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider w-28">Konto</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider">Naziv</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider w-40">Saldo</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <LoadingRow />}
            {isError && <ErrorRow message="Greška pri učitavanju bilansa stanja" />}
            {data && (
              <>
                <ReportLines lines={data.assets} label="Imovina (Assets)" />
                <SummaryRow label="Ukupna imovina" value={fmt(data.totalAssets)} />

                <ReportLines lines={data.liabilities} label="Obaveze (Liabilities)" />
                <SummaryRow label="Ukupne obaveze" value={fmt(data.totalLiabilities)} />

                <ReportLines lines={data.equity} label="Kapital (Equity)" />
                <SummaryRow label="Ukupan kapital" value={fmt(data.totalEquity)} />

                <SummaryRow
                  label="UKUPNO PASIVA"
                  value={fmt({
                    amount: (data.totalLiabilities?.amount ?? 0) + (data.totalEquity?.amount ?? 0),
                    currency: data.totalAssets?.currency ?? 'RSD',
                  })}
                  highlight
                />
              </>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ─── Tab: Bilans uspjeha ──────────────────────────────────────────────────────

function IncomeStatementTab() {
  const { month: curMonth, year: curYear } = currentMonthYear();
  const [month, setMonth] = useState(curMonth);
  const [year, setYear] = useState(curYear);

  const { data, isLoading, isError } = useQuery<IncomeStatementDto>({
    queryKey: ['reports', 'income-statement', month, year],
    queryFn: () => getIncomeStatement(month, year),
  });

  return (
    <div className="space-y-4">
      <PeriodSelector
        month={month}
        year={year}
        onMonthChange={setMonth}
        onYearChange={setYear}
      />

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-blue-800 text-white">
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider w-28">Konto</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider">Naziv</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider w-40">Iznos</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <LoadingRow />}
            {isError && <ErrorRow message="Greška pri učitavanju bilansa uspjeha" />}
            {data && (
              <>
                <ReportLines lines={data.revenue} label="Prihodi (klasa 6)" />
                <SummaryRow label="Ukupni prihodi" value={fmt(data.totalRevenue)} />

                <ReportLines lines={data.expenses} label="Rashodi (klasa 5)" />
                <SummaryRow label="Ukupni rashodi" value={fmt(data.totalExpenses)} />

                <SummaryRow
                  label={`NETO DOBITAK / GUBITAK`}
                  value={fmt(data.netIncome)}
                  highlight
                />
              </>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ─── Tab: PDV prijava ─────────────────────────────────────────────────────────

function VatReturnTab() {
  const { month: curMonth, year: curYear } = currentMonthYear();
  const [month, setMonth] = useState(curMonth);
  const [year, setYear] = useState(curYear);

  const { data, isLoading, isError } = useQuery<VatReturnDto>({
    queryKey: ['reports', 'vat-return', month, year],
    queryFn: () => getVatReturn(month, year),
  });

  return (
    <div className="space-y-4">
      <PeriodSelector
        month={month}
        year={year}
        onMonthChange={setMonth}
        onYearChange={setYear}
      />

      {isLoading && (
        <div className="flex justify-center py-10">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
        </div>
      )}
      {isError && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          Greška pri učitavanju PDV prijave.
        </div>
      )}
      {data && (
        <div className="rounded-xl border border-gray-200 bg-white shadow-sm overflow-hidden">
          {/* PP PDV obrazac layout */}
          <div className="bg-blue-800 px-6 py-4">
            <h2 className="text-white font-bold text-base">PP PDV OBRAZAC</h2>
            <p className="text-blue-200 text-sm mt-0.5">
              Period: {monthOptions().find((m) => m.value === data.month)?.label} {data.year}
            </p>
          </div>

          <div className="p-6 space-y-4">
            {/* 20% */}
            <div className="rounded-lg border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-2 text-xs font-bold uppercase tracking-wide text-gray-600 border-b">
                Promet sa stopom PDV 20%
              </div>
              <table className="w-full text-sm">
                <tbody>
                  <tr className="border-b border-gray-100">
                    <td className="px-4 py-3 text-gray-700">Osnovica (bez PDV)</td>
                    <td className="px-4 py-3 text-right font-semibold">{fmt(data.taxableBase20)}</td>
                  </tr>
                  <tr>
                    <td className="px-4 py-3 text-gray-700">Obračunati PDV (20%)</td>
                    <td className="px-4 py-3 text-right font-semibold text-blue-700">{fmt(data.vatCollected20)}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            {/* 10% */}
            <div className="rounded-lg border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-2 text-xs font-bold uppercase tracking-wide text-gray-600 border-b">
                Promet sa stopom PDV 10%
              </div>
              <table className="w-full text-sm">
                <tbody>
                  <tr className="border-b border-gray-100">
                    <td className="px-4 py-3 text-gray-700">Osnovica (bez PDV)</td>
                    <td className="px-4 py-3 text-right font-semibold">{fmt(data.taxableBase10)}</td>
                  </tr>
                  <tr>
                    <td className="px-4 py-3 text-gray-700">Obračunati PDV (10%)</td>
                    <td className="px-4 py-3 text-right font-semibold text-blue-700">{fmt(data.vatCollected10)}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            {/* 0% */}
            <div className="rounded-lg border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-2 text-xs font-bold uppercase tracking-wide text-gray-600 border-b">
                Promet oslobođen PDV (0%)
              </div>
              <table className="w-full text-sm">
                <tbody>
                  <tr>
                    <td className="px-4 py-3 text-gray-700">Osnovica prometa bez PDV</td>
                    <td className="px-4 py-3 text-right font-semibold">{fmt(data.taxableBase0)}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            {/* Pretporez + Uplata */}
            <div className="rounded-lg border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-2 text-xs font-bold uppercase tracking-wide text-gray-600 border-b">
                Pretporez i obaveza prema budžetu
              </div>
              <table className="w-full text-sm">
                <tbody>
                  <tr className="border-b border-gray-100">
                    <td className="px-4 py-3 text-gray-700">Pretporez koji se odbija</td>
                    <td className="px-4 py-3 text-right font-semibold">{fmt(data.inputVatDeductible)}</td>
                  </tr>
                  <tr className="bg-blue-700 text-white font-bold">
                    <td className="px-4 py-3">PDV ZA UPLATU</td>
                    <td className="px-4 py-3 text-right text-lg">{fmt(data.vatPayable)}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── Tab: Obračun zarada ──────────────────────────────────────────────────────

function PayrollTab() {
  const { month: curMonth, year: curYear } = currentMonthYear();
  const [month, setMonth] = useState(curMonth);
  const [year, setYear] = useState(curYear);

  const { data, isLoading, isError } = useQuery<PayrollReportDto>({
    queryKey: ['reports', 'payroll', month, year],
    queryFn: () => getPayrollReport(month, year),
  });

  return (
    <div className="space-y-4">
      <PeriodSelector
        month={month}
        year={year}
        onMonthChange={setMonth}
        onYearChange={setYear}
      />

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-blue-800 text-white">
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider">Zaposleni</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider">Pozicija</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider">Bruto</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider">Porez</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider">Neto</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider">Ukupan trošak</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-sm text-gray-400">
                  <div className="flex justify-center items-center gap-2">
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
                    Učitavanje...
                  </div>
                </td>
              </tr>
            )}
            {isError && (
              <tr>
                <td colSpan={6} className="px-4 py-6 text-center text-sm text-red-600">
                  Greška pri učitavanju obračuna zarada.
                </td>
              </tr>
            )}
            {data && data.lines.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-sm text-gray-400">
                  Nema aktivnih zaposlenih.
                </td>
              </tr>
            )}
            {data &&
              data.lines.map((line, idx) => (
                <tr
                  key={idx}
                  className="border-b border-gray-100 hover:bg-gray-50 transition-colors"
                >
                  <td className="px-4 py-3 font-medium text-gray-900">{line.fullName}</td>
                  <td className="px-4 py-3 text-gray-600">{line.position}</td>
                  <td className="px-4 py-3 text-right">{fmt(line.gross)}</td>
                  <td className="px-4 py-3 text-right text-orange-600">{fmt(line.tax)}</td>
                  <td className="px-4 py-3 text-right text-green-700 font-medium">{fmt(line.net)}</td>
                  <td className="px-4 py-3 text-right font-semibold text-blue-800">{fmt(line.totalCost)}</td>
                </tr>
              ))}
            {data && data.lines.length > 0 && (
              <tr className="bg-blue-700 text-white font-bold">
                <td className="px-4 py-3" colSpan={2}>
                  UKUPNO
                </td>
                <td className="px-4 py-3 text-right">{fmt(data.totalGross)}</td>
                <td className="px-4 py-3 text-right">{fmt(data.totalTax)}</td>
                <td className="px-4 py-3 text-right">{fmt(data.totalNet)}</td>
                <td className="px-4 py-3 text-right">
                  {fmt({
                    amount:
                      (data.totalGross?.amount ?? 0) +
                      (data.totalEmployerContributions?.amount ?? 0),
                    currency: data.totalGross?.currency ?? 'RSD',
                  })}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {data && (
        <div className="rounded-lg border border-gray-200 bg-white p-4">
          <p className="text-xs text-gray-500 mb-2 font-semibold uppercase tracking-wide">
            Rekapitulacija doprinosa poslodavca
          </p>
          <p className="text-sm text-gray-700">
            Ukupni doprinosi poslodavca:{' '}
            <span className="font-bold text-blue-800">
              {fmt(data.totalEmployerContributions)}
            </span>
          </p>
        </div>
      )}
    </div>
  );
}

// ─── Main ReportsPage ─────────────────────────────────────────────────────────

type TabId = 'bilans-stanja' | 'bilans-uspjeha' | 'pdv-prijava' | 'obracun-zarada';

const TABS: { id: TabId; label: string }[] = [
  { id: 'bilans-stanja',    label: 'Bilans stanja' },
  { id: 'bilans-uspjeha',   label: 'Bilans uspjeha' },
  { id: 'pdv-prijava',      label: 'PDV prijava' },
  { id: 'obracun-zarada',   label: 'Obračun zarada' },
];

export function ReportsPage() {
  const [activeTab, setActiveTab] = useState<TabId>('bilans-stanja');

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Finansijski izvještaji</h1>
        <p className="text-sm text-gray-500 mt-1">
          Bilans stanja, bilans uspjeha, PDV prijava i obračun zarada
        </p>
      </div>

      {/* Tab bar */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex gap-1" aria-label="Tabs">
          {TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={[
                'px-4 py-2.5 text-sm font-medium rounded-t-lg border-b-2 transition-colors duration-150 focus:outline-none',
                activeTab === tab.id
                  ? 'border-blue-600 text-blue-700 bg-white'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
              ].join(' ')}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab content */}
      <div>
        {activeTab === 'bilans-stanja'  && <BalanceSheetTab />}
        {activeTab === 'bilans-uspjeha' && <IncomeStatementTab />}
        {activeTab === 'pdv-prijava'    && <VatReturnTab />}
        {activeTab === 'obracun-zarada' && <PayrollTab />}
      </div>
    </div>
  );
}

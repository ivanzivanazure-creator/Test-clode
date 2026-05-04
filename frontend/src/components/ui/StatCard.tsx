import React from 'react';
import { cn } from '../../utils/cn';
import { formatMoney } from '../../utils/formatters';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface StatCardProps {
  title: string;
  value: number;
  isCurrency?: boolean;
  trend?: number; // percentage change (positive = up, negative = down)
  icon: React.ReactNode;
  iconBgClass?: string;
  suffix?: string;
  loading?: boolean;
  className?: string;
}

// ─── Trend Badge ──────────────────────────────────────────────────────────────

function TrendBadge({ trend }: { trend: number }) {
  const isPositive = trend >= 0;
  return (
    <span
      className={cn(
        'inline-flex items-center gap-0.5 rounded-full px-2 py-0.5 text-xs font-medium',
        isPositive
          ? 'bg-emerald-50 text-emerald-700'
          : 'bg-red-50 text-red-700',
      )}
    >
      <svg className="h-3 w-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d={isPositive ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'}
        />
      </svg>
      {Math.abs(trend).toFixed(1)}%
    </span>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StatCard({
  title,
  value,
  isCurrency = true,
  trend,
  icon,
  iconBgClass = 'bg-blue-100 text-blue-600',
  suffix,
  loading = false,
  className,
}: StatCardProps) {
  const displayValue = isCurrency
    ? formatMoney(value)
    : `${value.toLocaleString('sr-RS')}${suffix ? ` ${suffix}` : ''}`;

  return (
    <div
      className={cn(
        'bg-white rounded-xl border border-gray-200 shadow-card p-5 flex items-start gap-4',
        className,
      )}
    >
      {/* Icon */}
      <div className={cn('shrink-0 rounded-lg p-2.5', iconBgClass)}>
        <div className="h-6 w-6">{icon}</div>
      </div>

      {/* Content */}
      <div className="min-w-0 flex-1">
        <p className="text-sm text-gray-500 font-medium truncate">{title}</p>

        {loading ? (
          <div className="mt-1 h-7 w-32 animate-pulse rounded bg-gray-200" />
        ) : (
          <p className="mt-0.5 text-2xl font-bold text-gray-900 tracking-tight">
            {displayValue}
          </p>
        )}

        {trend !== undefined && !loading && (
          <div className="mt-2">
            <TrendBadge trend={trend} />
            <span className="ml-1.5 text-xs text-gray-400">vs. prošli mesec</span>
          </div>
        )}
      </div>
    </div>
  );
}

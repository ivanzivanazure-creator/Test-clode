import React from 'react';
import { cn } from '../../utils/cn';
import { InvoiceStatus, JournalStatus } from '../../types';

// ─── Types ────────────────────────────────────────────────────────────────────

type BadgeColor = 'gray' | 'blue' | 'green' | 'red' | 'dark' | 'yellow' | 'purple';

export interface BadgeProps {
  label: string;
  color?: BadgeColor;
  className?: string;
}

// ─── Color Map ────────────────────────────────────────────────────────────────

const colorClasses: Record<BadgeColor, string> = {
  gray: 'bg-gray-100 text-gray-700 ring-gray-300',
  blue: 'bg-blue-50 text-blue-700 ring-blue-300',
  green: 'bg-emerald-50 text-emerald-700 ring-emerald-300',
  red: 'bg-red-50 text-red-700 ring-red-300',
  dark: 'bg-gray-800 text-gray-100 ring-gray-700',
  yellow: 'bg-yellow-50 text-yellow-700 ring-yellow-300',
  purple: 'bg-purple-50 text-purple-700 ring-purple-300',
};

// ─── Status → Color mapping ───────────────────────────────────────────────────

export function invoiceStatusColor(status: InvoiceStatus): BadgeColor {
  switch (status) {
    case InvoiceStatus.Nacrt:
      return 'gray';
    case InvoiceStatus.Izdata:
      return 'blue';
    case InvoiceStatus.Placena:
      return 'green';
    case InvoiceStatus.Dospela:
      return 'red';
    case InvoiceStatus.Otkazana:
      return 'dark';
    default:
      return 'gray';
  }
}

export function journalStatusColor(status: JournalStatus): BadgeColor {
  switch (status) {
    case JournalStatus.Proknjizeno:
      return 'green';
    case JournalStatus.Stornirano:
      return 'red';
    default:
      return 'gray';
  }
}

// ─── Component ────────────────────────────────────────────────────────────────

export function Badge({ label, color = 'gray', className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset',
        colorClasses[color],
        className,
      )}
    >
      {label}
    </span>
  );
}

// ─── Convenience Wrappers ─────────────────────────────────────────────────────

export function InvoiceStatusBadge({ status }: { status: InvoiceStatus }) {
  return <Badge label={status} color={invoiceStatusColor(status)} />;
}

export function JournalStatusBadge({ status }: { status: JournalStatus }) {
  return <Badge label={status} color={journalStatusColor(status)} />;
}

export function ActiveBadge({ isActive }: { isActive: boolean }) {
  return (
    <Badge
      label={isActive ? 'Aktivan' : 'Neaktivan'}
      color={isActive ? 'green' : 'gray'}
    />
  );
}

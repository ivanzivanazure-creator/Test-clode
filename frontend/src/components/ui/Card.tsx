import React from 'react';
import { cn } from '../../utils/cn';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface CardProps {
  title?: string;
  action?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  bodyClassName?: string;
  noPadding?: boolean;
}

// ─── Component ────────────────────────────────────────────────────────────────

export function Card({ title, action, children, className, bodyClassName, noPadding = false }: CardProps) {
  const hasHeader = title || action;

  return (
    <div
      className={cn(
        'bg-white rounded-xl border border-gray-200 shadow-card overflow-hidden',
        className,
      )}
    >
      {hasHeader && (
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          {title && (
            <h3 className="text-base font-semibold text-gray-900">{title}</h3>
          )}
          {action && <div className="ml-auto">{action}</div>}
        </div>
      )}
      <div className={cn(!noPadding && 'p-6', bodyClassName)}>
        {children}
      </div>
    </div>
  );
}

// ─── Section Card (lighter) ───────────────────────────────────────────────────

export function SectionCard({ children, className }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={cn('bg-gray-50 rounded-lg border border-gray-200 p-4', className)}>
      {children}
    </div>
  );
}

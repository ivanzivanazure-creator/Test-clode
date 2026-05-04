import React from 'react';
import { cn } from '../../utils/cn';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  className?: string;
}

// ─── Helper ───────────────────────────────────────────────────────────────────

function getPageNumbers(current: number, total: number): (number | '...')[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

  const pages: (number | '...')[] = [1];

  if (current > 3) pages.push('...');

  const start = Math.max(2, current - 1);
  const end = Math.min(total - 1, current + 1);
  for (let i = start; i <= end; i++) pages.push(i);

  if (current < total - 2) pages.push('...');
  pages.push(total);

  return pages;
}

// ─── Component ────────────────────────────────────────────────────────────────

export function Pagination({
  page,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  className,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);
  const pages = getPageNumbers(page, totalPages);

  return (
    <div className={cn('flex items-center justify-between gap-4 px-1 py-3', className)}>
      {/* Count summary */}
      <p className="text-sm text-gray-500 hidden sm:block">
        Prikazano <span className="font-medium text-gray-700">{from}–{to}</span> od{' '}
        <span className="font-medium text-gray-700">{totalCount}</span> stavki
      </p>

      {/* Page buttons */}
      <nav className="flex items-center gap-1" aria-label="Navigacija stranica">
        {/* Prev */}
        <button
          onClick={() => onPageChange(page - 1)}
          disabled={page === 1}
          className="inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium text-gray-500
            hover:bg-gray-100 disabled:opacity-40 disabled:pointer-events-none transition-colors"
          aria-label="Prethodna stranica"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
        </button>

        {pages.map((p, i) =>
          p === '...' ? (
            <span key={`ellipsis-${i}`} className="px-1 text-gray-400 select-none">
              …
            </span>
          ) : (
            <button
              key={p}
              onClick={() => onPageChange(p as number)}
              className={cn(
                'inline-flex items-center justify-center h-8 min-w-[2rem] px-2 rounded-md text-sm font-medium transition-colors',
                p === page
                  ? 'bg-brand-primary text-white shadow-sm'
                  : 'text-gray-600 hover:bg-gray-100',
              )}
              aria-current={p === page ? 'page' : undefined}
            >
              {p}
            </button>
          ),
        )}

        {/* Next */}
        <button
          onClick={() => onPageChange(page + 1)}
          disabled={page === totalPages}
          className="inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium text-gray-500
            hover:bg-gray-100 disabled:opacity-40 disabled:pointer-events-none transition-colors"
          aria-label="Sledeća stranica"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
          </svg>
        </button>
      </nav>
    </div>
  );
}

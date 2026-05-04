import React from 'react';
import { cn } from '../../utils/cn';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
  align?: 'left' | 'center' | 'right';
}

export interface TableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyExtractor: (row: T) => string;
  loading?: boolean;
  emptyMessage?: string;
  onRowClick?: (row: T) => void;
  className?: string;
}

// ─── Skeleton Row ─────────────────────────────────────────────────────────────

function SkeletonRow({ cols }: { cols: number }) {
  return (
    <tr className="animate-pulse">
      {Array.from({ length: cols }).map((_, i) => (
        <td key={i} className="px-4 py-3">
          <div className="h-4 bg-gray-200 rounded w-full" />
        </td>
      ))}
    </tr>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export function Table<T>({
  columns,
  data,
  keyExtractor,
  loading = false,
  emptyMessage = 'Nema podataka.',
  onRowClick,
  className,
}: TableProps<T>) {
  const alignClass = (align: Column<T>['align']) => {
    if (align === 'center') return 'text-center';
    if (align === 'right') return 'text-right';
    return 'text-left';
  };

  return (
    <div className={cn('w-full overflow-x-auto', className)}>
      <table className="min-w-full divide-y divide-gray-200">
        {/* Header */}
        <thead className="bg-gray-50">
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                scope="col"
                className={cn(
                  'px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider',
                  alignClass(col.align),
                  col.headerClassName,
                )}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>

        {/* Body */}
        <tbody className="bg-white divide-y divide-gray-100">
          {loading ? (
            <>
              {Array.from({ length: 5 }).map((_, i) => (
                <SkeletonRow key={i} cols={columns.length} />
              ))}
            </>
          ) : data.length === 0 ? (
            <tr>
              <td
                colSpan={columns.length}
                className="px-4 py-12 text-center text-sm text-gray-400"
              >
                <div className="flex flex-col items-center gap-2">
                  <svg className="h-8 w-8 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"
                    />
                  </svg>
                  <span>{emptyMessage}</span>
                </div>
              </td>
            </tr>
          ) : (
            data.map((row) => (
              <tr
                key={keyExtractor(row)}
                onClick={onRowClick ? () => onRowClick(row) : undefined}
                className={cn(
                  'transition-colors duration-100',
                  onRowClick
                    ? 'cursor-pointer hover:bg-blue-50 active:bg-blue-100'
                    : 'hover:bg-gray-50',
                )}
              >
                {columns.map((col) => (
                  <td
                    key={col.key}
                    className={cn(
                      'px-4 py-3 text-sm text-gray-700 whitespace-nowrap',
                      alignClass(col.align),
                      col.className,
                    )}
                  >
                    {col.render(row)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

/**
 * Formats a monetary amount in RSD currency format.
 * Example: formatMoney(1234.56) => "1.234,56 RSD"
 */
export function formatMoney(amount: number, currency = 'RSD'): string {
  if (isNaN(amount)) return `0,00 ${currency}`;

  const formatted = new Intl.NumberFormat('sr-RS', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
    useGrouping: true,
  }).format(amount);

  return `${formatted} ${currency}`;
}

/**
 * Formats a date string to Serbian format: "01.05.2024."
 */
export function formatDate(date: string | null | undefined): string {
  if (!date) return '—';

  try {
    const d = new Date(date);
    if (isNaN(d.getTime())) return '—';

    return new Intl.DateTimeFormat('sr-RS', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    })
      .format(d)
      .replace(/\//g, '.')
      .replace(/(\d{4})$/, '$1.');
  } catch {
    return '—';
  }
}

/**
 * Formats a number as a percentage string.
 * Example: formatPercent(0.2) => "20%"
 *          formatPercent(20, false) => "20%"
 */
export function formatPercent(n: number, isDecimal = false): string {
  const value = isDecimal ? n * 100 : n;
  return `${value.toLocaleString('sr-RS', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}%`;
}

/**
 * Formats a number compactly for display in stat cards.
 * Example: formatCompact(1234567) => "1,23M RSD"
 */
export function formatCompact(amount: number, currency = 'RSD'): string {
  if (isNaN(amount)) return `0 ${currency}`;

  if (Math.abs(amount) >= 1_000_000) {
    return `${(amount / 1_000_000).toLocaleString('sr-RS', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}M ${currency}`;
  }
  if (Math.abs(amount) >= 1_000) {
    return `${(amount / 1_000).toLocaleString('sr-RS', { minimumFractionDigits: 1, maximumFractionDigits: 1 })}K ${currency}`;
  }
  return formatMoney(amount, currency);
}

/**
 * Returns a date string in ISO format (YYYY-MM-DD) for use in inputs.
 */
export function toInputDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toISOString().split('T')[0];
}

/**
 * Returns today's date as ISO string (YYYY-MM-DD).
 */
export function today(): string {
  return toInputDate(new Date());
}

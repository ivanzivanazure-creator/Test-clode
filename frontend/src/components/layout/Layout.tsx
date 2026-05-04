import React, { useState } from 'react';
import { Outlet, useLocation, Link } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { useAuth } from '../../hooks/useAuth';
import { cn } from '../../utils/cn';

// ─── Breadcrumb Config ────────────────────────────────────────────────────────

const routeLabels: Record<string, string> = {
  '': 'Kontrolna tabla',
  invoices: 'Fakture',
  new: 'Nova',
  journal: 'Temeljnice',
  employees: 'Zaposleni',
};

function useBreadcrumbs() {
  const location = useLocation();
  const parts = location.pathname.split('/').filter(Boolean);

  const crumbs = [{ label: 'Početna', to: '/' }];
  let path = '';

  for (const part of parts) {
    path += `/${part}`;
    const label = routeLabels[part] ?? part;
    crumbs.push({ label, to: path });
  }

  return crumbs;
}

// ─── Topbar ───────────────────────────────────────────────────────────────────

function Topbar({ onMenuClick }: { onMenuClick: () => void }) {
  const { user } = useAuth();
  const crumbs = useBreadcrumbs();

  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-gray-200 bg-white px-4 sm:px-6">
      {/* Mobile hamburger */}
      <button
        onClick={onMenuClick}
        className="lg:hidden rounded-md p-1.5 text-gray-500 hover:bg-gray-100 transition-colors"
        aria-label="Otvori meni"
      >
        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
        </svg>
      </button>

      {/* Breadcrumb */}
      <nav className="hidden lg:flex items-center gap-1 text-sm" aria-label="Navigacija">
        {crumbs.map((crumb, i) => (
          <React.Fragment key={crumb.to}>
            {i > 0 && (
              <svg className="h-4 w-4 text-gray-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
              </svg>
            )}
            {i === crumbs.length - 1 ? (
              <span className="font-semibold text-gray-900">{crumb.label}</span>
            ) : (
              <Link to={crumb.to} className="text-gray-500 hover:text-gray-700 transition-colors">
                {crumb.label}
              </Link>
            )}
          </React.Fragment>
        ))}
      </nav>

      {/* Mobile title (centered) */}
      <p className="lg:hidden text-sm font-semibold text-gray-900">
        {crumbs[crumbs.length - 1]?.label}
      </p>

      {/* Right side: user chip */}
      <div className="flex items-center gap-2">
        <div className="hidden sm:flex items-center gap-2 rounded-full border border-gray-200 bg-gray-50 px-3 py-1.5">
          <div className="h-6 w-6 rounded-full bg-brand-primary flex items-center justify-center text-white text-xs font-semibold">
            {user?.fullName
              .split(' ')
              .map((n) => n[0])
              .join('')
              .slice(0, 2)
              .toUpperCase() ?? 'U'}
          </div>
          <span className="text-xs font-medium text-gray-700">{user?.fullName}</span>
        </div>
      </div>
    </header>
  );
}

// ─── Layout ───────────────────────────────────────────────────────────────────

export function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      {/* Desktop Sidebar */}
      <div className="hidden lg:flex lg:w-60 xl:w-64 lg:shrink-0 lg:flex-col">
        <Sidebar />
      </div>

      {/* Mobile Sidebar Overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 flex lg:hidden">
          <div
            className="absolute inset-0 bg-black/40"
            onClick={() => setSidebarOpen(false)}
            aria-hidden="true"
          />
          <div className="relative z-50 w-60 flex-col flex">
            <Sidebar />
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="flex flex-1 flex-col min-w-0 overflow-hidden">
        <Topbar onMenuClick={() => setSidebarOpen(true)} />

        <main className="flex-1 overflow-y-auto p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

import { createBrowserRouter, Navigate } from 'react-router-dom'
import { Layout } from './components/layout/Layout'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { InvoicesPage } from './pages/invoices/InvoicesPage'
import { NewInvoicePage } from './pages/invoices/NewInvoicePage'
import { InvoiceDetailPage } from './pages/invoices/InvoiceDetailPage'
import { JournalPage } from './pages/journal/JournalPage'
import { NewJournalPage } from './pages/journal/NewJournalPage'
import { EmployeesPage } from './pages/employees/EmployeesPage'
import { NewEmployeePage } from './pages/employees/NewEmployeePage'
import { ProtectedRoute } from './components/layout/ProtectedRoute'

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: <ProtectedRoute><Layout /></ProtectedRoute>,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'fakture', element: <InvoicesPage /> },
      { path: 'fakture/nova', element: <NewInvoicePage /> },
      { path: 'fakture/:id', element: <InvoiceDetailPage /> },
      { path: 'temeljnice', element: <JournalPage /> },
      { path: 'temeljnice/nova', element: <NewJournalPage /> },
      { path: 'zaposleni', element: <EmployeesPage /> },
      { path: 'zaposleni/novi', element: <NewEmployeePage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
])

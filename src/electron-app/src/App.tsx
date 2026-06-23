import React, { Suspense } from 'react'
import { HashRouter, Routes, Route, Navigate } from 'react-router-dom'
import AppLayout from './components/layout/AppLayout'
import ProtectedRoute from './components/layout/ProtectedRoute'

const LoginScreen = React.lazy(() => import('./screens/LoginScreen'))
const DashboardScreen = React.lazy(() => import('./screens/DashboardScreen'))
const SalesScreen = React.lazy(() => import('./screens/SalesScreen'))
const SalesHistoryScreen = React.lazy(() => import('./screens/SalesHistoryScreen'))
const ProductsScreen = React.lazy(() => import('./screens/ProductsScreen'))
const CategoriesScreen = React.lazy(() => import('./screens/CategoriesScreen'))
const CustomersScreen = React.lazy(() => import('./screens/CustomersScreen'))
const SuppliersScreen = React.lazy(() => import('./screens/SuppliersScreen'))
const InventoryScreen = React.lazy(() => import('./screens/InventoryScreen'))
const ReportsScreen = React.lazy(() => import('./screens/ReportsScreen'))
const UsersScreen = React.lazy(() => import('./screens/UsersScreen'))
const SettingsScreen = React.lazy(() => import('./screens/SettingsScreen'))
const AuditLogScreen = React.lazy(() => import('./screens/AuditLogScreen'))
const BackupScreen = React.lazy(() => import('./screens/BackupScreen'))
const ChangePasswordScreen = React.lazy(() => import('./screens/ChangePasswordScreen'))
const ActivateScreen = React.lazy(() => import('./screens/ActivateScreen'))

const PageLoader = () => (
  <div className="flex items-center justify-center h-full py-20">
    <div className="flex flex-col items-center gap-3">
      <svg className="animate-spin h-8 w-8 text-indigo-500" viewBox="0 0 24 24" fill="none">
        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
      </svg>
      <span className="text-sm text-slate-400">Cargando...</span>
    </div>
  </div>
)

export default function App() {
  return (
    <HashRouter>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          <Route path="/login" element={<LoginScreen />} />
          <Route path="/change-password" element={<ChangePasswordScreen />} />
          <Route path="/activate" element={<ActivateScreen />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<DashboardScreen />} />
              <Route path="/sales" element={<SalesScreen />} />
              <Route path="/sales-history" element={<SalesHistoryScreen />} />
              <Route path="/products" element={<ProductsScreen />} />
              <Route path="/categories" element={<CategoriesScreen />} />
              <Route path="/customers" element={<CustomersScreen />} />
              <Route path="/suppliers" element={<SuppliersScreen />} />
              <Route path="/inventory" element={<InventoryScreen />} />
              <Route path="/reports" element={<ReportsScreen />} />
              <Route path="/users" element={<UsersScreen />} />
              <Route path="/settings" element={<SettingsScreen />} />
              <Route path="/audit" element={<AuditLogScreen />} />
              <Route path="/backup" element={<BackupScreen />} />
            </Route>
          </Route>
        </Routes>
      </Suspense>
    </HashRouter>
  )
}

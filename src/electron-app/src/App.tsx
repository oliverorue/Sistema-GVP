import { HashRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './stores/authStore'
import LoginScreen from './screens/LoginScreen'
import DashboardScreen from './screens/DashboardScreen'
import SalesScreen from './screens/SalesScreen'
import SalesHistoryScreen from './screens/SalesHistoryScreen'
import ProductsScreen from './screens/ProductsScreen'
import CategoriesScreen from './screens/CategoriesScreen'
import CustomersScreen from './screens/CustomersScreen'
import SuppliersScreen from './screens/SuppliersScreen'
import InventoryScreen from './screens/InventoryScreen'
import ReportsScreen from './screens/ReportsScreen'
import UsersScreen from './screens/UsersScreen'
import SettingsScreen from './screens/SettingsScreen'
import AuditLogScreen from './screens/AuditLogScreen'
import BackupScreen from './screens/BackupScreen'
import ChangePasswordScreen from './screens/ChangePasswordScreen'
import ActivateScreen from './screens/ActivateScreen'
import AppLayout from './components/layout/AppLayout'
import ProtectedRoute from './components/layout/ProtectedRoute'

export default function App() {
  return (
    <HashRouter>
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
    </HashRouter>
  )
}

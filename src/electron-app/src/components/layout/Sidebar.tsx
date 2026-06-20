import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard, ShoppingCart, Package, FolderOpen, Users,
  Truck, ClipboardList, BarChart3, UserCog, Factory,
  Settings, FileText, Database, ChevronLeft,
} from 'lucide-react'
import { useUIStore } from '../../stores/uiStore'
import { useAuthStore } from '../../stores/authStore'

const menuItems = [
  { icon: LayoutDashboard, label: 'Dashboard', path: '/dashboard' },
  { icon: ShoppingCart, label: 'Ventas', path: '/sales' },
  { icon: ClipboardList, label: 'Historial Ventas', path: '/sales-history' },
  { icon: Package, label: 'Productos', path: '/products' },
  { icon: FolderOpen, label: 'Categorias', path: '/categories' },
  { icon: Users, label: 'Clientes', path: '/customers' },
  { divider: true },
  { icon: Truck, label: 'Inventario', path: '/inventory' },
  { icon: BarChart3, label: 'Reportes', path: '/reports' },
  { divider: true, adminOnly: true },
  { icon: UserCog, label: 'Usuarios', path: '/users', adminOnly: true },
  { icon: Factory, label: 'Proveedores', path: '/suppliers', adminOnly: true },
  { icon: Settings, label: 'Configuracion', path: '/settings', adminOnly: true },
  { icon: FileText, label: 'Auditoria', path: '/audit', adminOnly: true },
  { icon: Database, label: 'Backups', path: '/backup', adminOnly: true },
]

export default function Sidebar() {
  const sidebarOpen = useUIStore((s) => s.sidebarOpen)
  const toggleSidebar = useUIStore((s) => s.toggleSidebar)
  const isAdmin = useAuthStore((s) => s.isAdmin)

  return (
    <aside className={`fixed left-0 top-12 bottom-6 bg-white border-r border-slate-200 transition-all duration-200 z-30 ${
      sidebarOpen ? 'w-64' : 'w-16'
    }`}>
      <nav className="h-full overflow-y-auto p-2 space-y-1">
        {menuItems.map((item, index) => {
          if (item.divider) {
            if (item.adminOnly && !isAdmin) return null
            return (
              <div key={index} className={`border-t border-slate-200 my-2 ${sidebarOpen ? '' : 'mx-2'}`} />
            )
          }
          if (item.adminOnly && !isAdmin) return null

          const Icon = item.icon!
          return (
            <NavLink
              key={item.path}
              to={item.path!}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                  isActive
                    ? 'bg-indigo-50 text-indigo-700 font-medium'
                    : 'text-slate-600 hover:bg-slate-100 hover:text-slate-800'
                } ${!sidebarOpen ? 'justify-center' : ''}`
              }
            >
              <Icon className="w-5 h-5 shrink-0" />
              {sidebarOpen && <span>{item.label}</span>}
            </NavLink>
          )
        })}
      </nav>

      <button
        onClick={toggleSidebar}
        className="absolute -right-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-white border border-slate-200 rounded-full flex items-center justify-center text-slate-400 hover:text-slate-600 shadow-sm"
      >
        <ChevronLeft className={`w-4 h-4 transition-transform ${!sidebarOpen ? 'rotate-180' : ''}`} />
      </button>
    </aside>
  )
}

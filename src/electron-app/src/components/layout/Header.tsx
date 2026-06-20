import { Bell, Settings, LogOut, User, ChevronDown } from 'lucide-react'
import { useAuthStore } from '../../stores/authStore'
import { useUIStore } from '../../stores/uiStore'
import { useNavigate } from 'react-router-dom'
import { useState } from 'react'

export default function Header() {
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)
  const toggleSidebar = useUIStore((s) => s.toggleSidebar)
  const navigate = useNavigate()
  const [showUserMenu, setShowUserMenu] = useState(false)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <header className="h-12 bg-white border-b border-slate-200 flex items-center justify-between px-4 shrink-0">
      <div className="flex items-center gap-3">
        <button onClick={toggleSidebar} className="text-slate-500 hover:text-slate-700">
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <span className="font-semibold text-indigo-600">Sistema GVP</span>
      </div>

      <div className="flex items-center gap-3">
        <button className="text-slate-400 hover:text-slate-600 relative">
          <Bell className="w-5 h-5" />
          <span className="absolute -top-1 -right-1 w-2 h-2 bg-red-500 rounded-full" />
        </button>

        <div className="relative">
          <button
            onClick={() => setShowUserMenu(!showUserMenu)}
            className="flex items-center gap-2 text-sm text-slate-600 hover:text-slate-800"
          >
            <div className="w-7 h-7 bg-indigo-100 rounded-full flex items-center justify-center">
              <User className="w-4 h-4 text-indigo-600" />
            </div>
            <span className="font-medium">{user?.fullName || 'Usuario'}</span>
            <ChevronDown className="w-4 h-4" />
          </button>

          {showUserMenu && (
            <>
              <div className="fixed inset-0 z-10" onClick={() => setShowUserMenu(false)} />
              <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-slate-200 py-1 z-20">
                <div className="px-4 py-2 border-b border-slate-100">
                  <p className="text-sm font-medium text-slate-900">{user?.fullName}</p>
                  <p className="text-xs text-slate-500">{user?.role === 'Admin' ? 'Administrador' : 'Cajero'}</p>
                </div>
                <button
                  onClick={() => { setShowUserMenu(false); navigate('/settings') }}
                  className="w-full text-left px-4 py-2 text-sm text-slate-600 hover:bg-slate-50 flex items-center gap-2"
                >
                  <Settings className="w-4 h-4" /> Configuracion
                </button>
                <button
                  onClick={handleLogout}
                  className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
                >
                  <LogOut className="w-4 h-4" /> Cerrar Sesion
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  )
}

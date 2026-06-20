import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../stores/authStore'
import { authService } from '../services/authService'
import { User } from '../types/entities'
import { LogIn } from 'lucide-react'

export default function LoginScreen() {
  const navigate = useNavigate()
  const login = useAuthStore((s) => s.login)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [companyId, setCompanyId] = useState(1)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const result = await authService.login({ username, password, companyId })

      if (!result.isSuccess) {
        setError(result.message)
        return
      }

      const data = result.data!
      const user: User = {
        id: data.user.id,
        username: data.user.username,
        fullName: data.user.fullName,
        email: data.user.email,
        role: data.user.role as 'Admin' | 'Cashier',
        companyId: data.user.companyId,
        isActive: true,
        mustChangePassword: data.user.mustChangePassword,
      }

      login(data.token, user)

      if (data.requiresPasswordChange) {
        navigate('/change-password')
      } else {
        navigate('/dashboard')
      }
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Error al iniciar sesion')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-50 to-slate-100">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 p-8 w-full max-w-md">
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-indigo-100 rounded-2xl flex items-center justify-center mx-auto mb-4">
            <LogIn className="w-8 h-8 text-indigo-600" />
          </div>
          <h1 className="text-2xl font-bold text-slate-900">Sistema GVP</h1>
          <p className="text-sm text-slate-500 mt-1">Punto de Venta</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Usuario</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="input-field"
              placeholder="Ingrese su usuario"
              autoFocus
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Contrasena</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="input-field"
              placeholder="Ingrese su contrasena"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Empresa</label>
            <select
              value={companyId}
              onChange={(e) => setCompanyId(Number(e.target.value))}
              className="input-field"
            >
              <option value={1}>Mi Empresa S.A.</option>
              <option value={2}>Empresa Demo</option>
            </select>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-600 text-sm rounded-lg px-4 py-2">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="btn-primary w-full flex items-center justify-center gap-2"
          >
            {loading ? (
              <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              'Ingresar'
            )}
          </button>
        </form>

        <p className="text-center text-xs text-slate-400 mt-6">v2.0.0</p>
      </div>
    </div>
  )
}

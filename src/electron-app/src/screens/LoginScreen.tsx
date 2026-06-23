import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { AxiosError } from 'axios'
import { useAuthStore } from '../stores/authStore'
import { authService } from '../services/authService'
import { User } from '../types/entities'
import { Logger } from '../utils/logger'

export default function LoginScreen() {
  const navigate = useNavigate()
  const login = useAuthStore((s) => s.login)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  // Check for session expired message from api.ts interceptor
  useEffect(() => {
    const expiredMsg = sessionStorage.getItem('gvp_session_expired')
    if (expiredMsg) {
      sessionStorage.removeItem('gvp_session_expired')
      setError(expiredMsg)
    }
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    Logger.info('LoginScreen', 'Intento de inicio de sesión', { username })

    try {
      const result = await authService.login({ username, password })

      if (!result.isSuccess) {
        setError(result.message)
        Logger.warn('LoginScreen', 'Login fallido', { message: result.message })
        return
      }

      const data = result.data!
      Logger.info('LoginScreen', 'Login exitoso', { user: data.user.fullName, role: data.user.role })
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
    } catch (err) {
      const message = err instanceof AxiosError
        ? (err.response?.data as { message?: string })?.message
        : 'Error al iniciar sesion'
      setError(message || 'Error al iniciar sesion')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-600 via-indigo-500 to-purple-600">
      <div className="bg-white rounded-2xl shadow-2xl shadow-indigo-900/20 border border-white/10 p-8 w-full max-w-md animate-scaleIn">
        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-lg shadow-indigo-500/30">
            <svg className="w-10 h-10 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold text-slate-900">Sistema GVP</h1>
          <p className="text-sm text-slate-500 mt-1">Punto de Venta — Iniciar Sesión</p>
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

        <p className="text-center text-xs text-slate-400 mt-6">© 2026 Sistema GVP v2.0</p>
      </div>
    </div>
  )
}

import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { ShieldCheck, ShieldAlert } from 'lucide-react'
import { Logger } from '../utils/logger'

export default function ActivateScreen() {
  const navigate = useNavigate()
  const [licenseKey, setLicenseKey] = useState('')
  const [activating, setActivating] = useState(false)
  const [result, setResult] = useState<{ success: boolean; message: string } | null>(null)

  const handleActivate = async () => {
    if (!licenseKey.trim()) {
      toast.error('Ingrese una clave de licencia')
      return
    }
    setActivating(true)
    try {
      if (window.electronAPI?.activateLicense) {
        const res = await window.electronAPI.activateLicense(licenseKey.trim())
        setResult(res)
        if (res.success) {
          toast.success('Licencia activada exitosamente')
          setTimeout(() => navigate('/login'), 1500)
        } else {
          toast.error(res.message)
        }
      } else {
        toast.error('El sistema de licencias solo está disponible en la aplicación de escritorio')
      }
    } catch (err) {
      Logger.error('ActivateScreen', 'Error al activar licencia', err)
      toast.error('Error al activar la licencia')
    } finally {
      setActivating(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50">
      <div className="card max-w-md w-full mx-4">
        <div className="text-center mb-6">
          <div className="flex justify-center mb-4">
            {result?.success ? (
              <ShieldCheck className="w-16 h-16 text-emerald-500" />
            ) : (
              <ShieldAlert className="w-16 h-16 text-indigo-500" />
            )}
          </div>
          <h1 className="text-2xl font-bold text-slate-900">Activar Licencia</h1>
          <p className="text-sm text-slate-500 mt-1">
            Ingrese la clave de licencia proporcionada por el administrador
          </p>
        </div>

        {!result?.success && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Clave de Licencia</label>
              <textarea
                value={licenseKey}
                onChange={(e) => setLicenseKey(e.target.value)}
                className="input-field font-mono text-xs"
                rows={4}
                placeholder="Pegue la clave de licencia aquí..."
              />
            </div>
            <button
              onClick={handleActivate}
              disabled={activating || !licenseKey.trim()}
              className="btn-primary w-full"
            >
              {activating ? 'Activando...' : 'Activar Licencia'}
            </button>
          </div>
        )}

        {result?.success && (
          <div className="text-center">
            <p className="text-emerald-600 font-medium mb-4">{result.message}</p>
            <p className="text-sm text-slate-400">Redirigiendo al inicio de sesión...</p>
          </div>
        )}

        {result && !result.success && (
          <p className="text-sm text-red-500 text-center mt-4">{result.message}</p>
        )}
      </div>
    </div>
  )
}

import { useAuthStore } from '../../stores/authStore'
import { useState, useEffect } from 'react'

export default function StatusBar() {
  const user = useAuthStore((s) => s.user)
  const [backendStatus, setBackendStatus] = useState<'connected' | 'disconnected'>('connected')

  useEffect(() => {
    const checkStatus = async () => {
      try {
        const response = await fetch('http://127.0.0.1:5000/health')
        if (response.ok) {
          setBackendStatus('connected')
        } else {
          setBackendStatus('disconnected')
        }
      } catch {
        setBackendStatus('disconnected')
      }
    }

    checkStatus()
    const interval = setInterval(checkStatus, 30000)
    return () => clearInterval(interval)
  }, [])

  return (
    <footer className="h-6 bg-slate-800 text-white text-xs flex items-center justify-between px-4 shrink-0">
      <div className="flex items-center gap-2">
        <span className={`w-2 h-2 rounded-full ${backendStatus === 'connected' ? 'bg-emerald-400' : 'bg-red-400'}`} />
        <span>{backendStatus === 'connected' ? 'Conectado' : 'Desconectado'}</span>
      </div>
      <div className="flex items-center gap-4">
        <span>Usuario: {user?.fullName || '---'}</span>
        <span>Empresa: {user?.companyId ? `#${user.companyId}` : '---'}</span>
      </div>
    </footer>
  )
}

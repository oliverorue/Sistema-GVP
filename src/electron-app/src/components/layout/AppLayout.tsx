import { useEffect } from 'react'
import { Outlet } from 'react-router-dom'
import { Toaster } from 'sonner'
import Sidebar from './Sidebar'
import Header from './Header'
import StatusBar from './StatusBar'
import { useUIStore } from '../../stores/uiStore'
import { useCartStore } from '../../stores/cartStore'
import { settingsService } from '../../services/settingsService'

export default function AppLayout() {
  const sidebarOpen = useUIStore((s) => s.sidebarOpen)
  const setTaxRate = useCartStore((s) => s.setTaxRate)
  const setIvaIncluido = useCartStore((s) => s.setIvaIncluido)

  useEffect(() => {
    settingsService.getCompany().then((result) => {
      if (result.isSuccess && result.data) {
        setTaxRate(result.data.taxRate)
        setIvaIncluido(result.data.ivaIncluido)
      }
    })
  }, [setTaxRate, setIvaIncluido])

  return (
    <div className="h-screen flex flex-col">
      <Toaster richColors position="top-right" duration={4000} gap={8} />
      <Header />
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />
        <main className={`flex-1 overflow-auto p-6 transition-all duration-200 ${sidebarOpen ? 'ml-64' : 'ml-16'}`}>
          <Outlet />
        </main>
      </div>
      <StatusBar />
    </div>
  )
}

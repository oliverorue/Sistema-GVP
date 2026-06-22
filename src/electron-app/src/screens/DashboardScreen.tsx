import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { TrendingUp, AlertTriangle, DollarSign, ShoppingCart, PauseCircle, Users, Package, ArrowRight } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, AreaChart, Area, CartesianGrid } from 'recharts'
import { useApi } from '../hooks/useApi'
import { reportService } from '../services/reportService'
import { ChartCard } from '../components/charts/ChartCard'
import { formatDateTime } from '../utils/format'
import { Logger } from '../utils/logger'

interface DashboardData {
  todaySales: { totalSales: number; totalRevenue: number; totalTax: number; averageTicket: number } | null
  lowStockCount: number
  topProducts: Array<{ productName: string; totalQuantity: number; totalRevenue: number }> | null
  recentMovements: Array<{ id: number; productName: string; type: string; quantity: number; createdAt: string }> | null
  lowStockProducts: Array<{ productName: string; currentStock: number; minStock: number }> | null
  heldSalesCount: number
  customerCount: number
  productCount: number
}

function getLast7Days(): { from: string; to: string } {
  const to = new Date()
  const from = new Date(to)
  from.setDate(from.getDate() - 6)
  return { from: from.toISOString().split('T')[0], to: to.toISOString().split('T')[0] }
}

const cardClass = 'card-static rounded-2xl border-l-4 bg-gradient-to-br from-white to-slate-50/30 cursor-pointer hover:scale-[1.02] transition-transform'

export default function DashboardScreen() {
  const navigate = useNavigate()
  const [data, setData] = useState<DashboardData | null>(null)
  const [dailySales, setDailySales] = useState<Array<{ date: string; total: number; count: number }>>([])
  const [loading, setLoading] = useState(true)
  const { get } = useApi()

  useEffect(() => {
    const fetchData = async () => {
      const { from, to } = getLast7Days()
      const [summaryResult, salesReportResult] = await Promise.allSettled([
        get<DashboardData>('/dashboard/summary'),
        reportService.getSalesReport(from, to),
      ])

      if (summaryResult.status === 'fulfilled' && summaryResult.value.ok) {
        setData(summaryResult.value.data)
      } else {
        Logger.error('Dashboard', 'Error al cargar resumen', summaryResult.status === 'rejected' ? summaryResult.reason : 'Respuesta no exitosa')
      }

      if (salesReportResult.status === 'fulfilled') {
        const sr = salesReportResult.value
        if (sr.isSuccess && Array.isArray(sr.data)) {
          setDailySales(
            sr.data.map((r: any) => ({
              date: r.date || '',
              total: r.totalAmount || r.total || 0,
              count: r.totalSales || r.count || 0,
            }))
          )
        }
      } else {
        Logger.error('Dashboard', 'Error al cargar reporte de ventas', salesReportResult.reason)
      }

      Logger.info('Dashboard', 'Datos cargados', {
        hasSummary: summaryResult.status === 'fulfilled' && summaryResult.value.ok,
        salesDays: salesReportResult.status === 'fulfilled' && salesReportResult.value.isSuccess ? (Array.isArray(salesReportResult.value.data) ? salesReportResult.value.data.length : 0) : 0,
      })
      setLoading(false)
    }
    fetchData()
  }, [get])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" />
      </div>
    )
  }

  const movementTypeLabel = (type: string) => {
    switch (type) {
      case 'IN': return { label: 'Entrada', color: 'badge-success' }
      case 'OUT': return { label: 'Salida', color: 'badge-warning' }
      default: return { label: type, color: 'badge-info' }
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
          <p className="text-sm text-slate-500 mt-0.5">Resumen general del sistema</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div onClick={() => navigate('/sales')} className={`${cardClass} border-l-indigo-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Ventas Hoy</p>
              <p className="text-3xl font-bold text-slate-900 mt-1">{data?.todaySales?.totalSales ?? 0}</p>
            </div>
            <div className="w-12 h-12 bg-gradient-to-br from-indigo-500 to-indigo-600 rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-200">
              <ShoppingCart className="w-6 h-6 text-white" />
            </div>
          </div>
        </div>

        <div onClick={() => navigate('/reports')} className={`${cardClass} border-l-emerald-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Ingresos Hoy</p>
              <p className="text-3xl font-bold text-emerald-600 mt-1">
                Gs. {(data?.todaySales?.totalRevenue ?? 0).toLocaleString()}
              </p>
            </div>
            <div className="w-12 h-12 bg-gradient-to-br from-emerald-500 to-emerald-600 rounded-2xl flex items-center justify-center shadow-lg shadow-emerald-200">
              <DollarSign className="w-6 h-6 text-white" />
            </div>
          </div>
        </div>

        <div onClick={() => navigate('/inventory')} className={`${cardClass} border-l-red-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Stock Bajo</p>
              <p className={`text-3xl font-bold mt-1 ${(data?.lowStockCount ?? 0) > 0 ? 'text-red-600' : 'text-slate-900'}`}>
                {data?.lowStockCount ?? 0}
              </p>
            </div>
            <div className="w-12 h-12 bg-gradient-to-br from-red-500 to-red-600 rounded-2xl flex items-center justify-center shadow-lg shadow-red-200">
              <AlertTriangle className="w-6 h-6 text-white" />
            </div>
          </div>
        </div>

        <div onClick={() => navigate('/reports')} className={`${cardClass} border-l-amber-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Ticket Promedio</p>
              <p className="text-3xl font-bold text-slate-900 mt-1">
                Gs. {(data?.todaySales?.averageTicket ?? 0).toLocaleString()}
              </p>
            </div>
            <div className="w-12 h-12 bg-gradient-to-br from-amber-500 to-amber-600 rounded-2xl flex items-center justify-center shadow-lg shadow-amber-200">
              <TrendingUp className="w-6 h-6 text-white" />
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div onClick={() => navigate('/sales')} className={`${cardClass} border-l-purple-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Ventas en Espera</p>
              <p className="text-3xl font-bold text-purple-600 mt-1">{data?.heldSalesCount ?? 0}</p>
            </div>
            <div className="w-10 h-10 bg-gradient-to-br from-purple-500 to-purple-600 rounded-xl flex items-center justify-center">
              <PauseCircle className="w-5 h-5 text-white" />
            </div>
          </div>
        </div>

        <div onClick={() => navigate('/customers')} className={`${cardClass} border-l-cyan-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Clientes</p>
              <p className="text-3xl font-bold text-cyan-600 mt-1">{data?.customerCount ?? 0}</p>
            </div>
            <div className="w-10 h-10 bg-gradient-to-br from-cyan-500 to-cyan-600 rounded-xl flex items-center justify-center">
              <Users className="w-5 h-5 text-white" />
            </div>
          </div>
        </div>

        <div onClick={() => navigate('/products')} className={`${cardClass} border-l-teal-500`}>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Productos</p>
              <p className="text-3xl font-bold text-teal-600 mt-1">{data?.productCount ?? 0}</p>
            </div>
            <div className="w-10 h-10 bg-gradient-to-br from-teal-500 to-teal-600 rounded-xl flex items-center justify-center">
              <Package className="w-5 h-5 text-white" />
            </div>
          </div>
        </div>
      </div>

      {(data?.lowStockCount ?? 0) > 0 && data?.lowStockProducts && data.lowStockProducts.length > 0 && (
        <div className="card">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-slate-700">Productos con Stock Bajo</h3>
            <button onClick={() => navigate('/inventory')} className="text-xs text-indigo-600 hover:text-indigo-800 flex items-center gap-1">
              Ir a inventario <ArrowRight className="w-3 h-3" />
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200">
                  <th className="text-left text-xs font-medium text-slate-500 uppercase px-3 py-2">Producto</th>
                  <th className="text-right text-xs font-medium text-slate-500 uppercase px-3 py-2">Stock Actual</th>
                  <th className="text-right text-xs font-medium text-slate-500 uppercase px-3 py-2">Stock Mínimo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.lowStockProducts.slice(0, 5).map((p, i) => (
                  <tr key={i} className="hover:bg-slate-50">
                    <td className="px-3 py-2 font-medium">{p.productName}</td>
                    <td className="px-3 py-2 text-right text-red-600 font-mono">{p.currentStock}</td>
                    <td className="px-3 py-2 text-right text-slate-500 font-mono">{p.minStock}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard title="Productos más vendidos">
          {data?.topProducts && data.topProducts.length > 0 ? (
            <>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={data.topProducts} margin={{ top: 5, right: 20, left: 20, bottom: 60 }}>
                  <XAxis dataKey="productName" tick={{ fontSize: 11 }} angle={-20} textAnchor="end" />
                  <YAxis />
                  <Tooltip formatter={(value: number) => value.toLocaleString()} />
                  <Bar dataKey="totalQuantity" fill="url(#barGradient)" radius={[6, 6, 0, 0]} name="Cantidad" />
                  <defs>
                    <linearGradient id="barGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366f1" />
                      <stop offset="100%" stopColor="#8b5cf6" />
                    </linearGradient>
                  </defs>
                </BarChart>
              </ResponsiveContainer>
              <div className="text-right mt-2">
                <button onClick={() => navigate('/products')} className="text-xs text-indigo-600 hover:text-indigo-800 flex items-center gap-1 ml-auto">
                  Ver productos <ArrowRight className="w-3 h-3" />
                </button>
              </div>
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-[300px] text-slate-400">
              <Package className="w-10 h-10 mb-2 opacity-40" />
              <p className="text-sm">Sin datos de productos</p>
            </div>
          )}
        </ChartCard>

        <ChartCard title="Ventas Diarias (últimos 7 días)">
          {dailySales.length > 0 ? (
            <>
              <ResponsiveContainer width="100%" height={300}>
                <AreaChart data={dailySales} margin={{ top: 5, right: 20, left: 20, bottom: 5 }}>
                  <defs>
                    <linearGradient id="areaGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366f1" stopOpacity={0.3} />
                      <stop offset="100%" stopColor="#6366f1" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                  <YAxis />
                  <Tooltip formatter={(value: number) => `Gs. ${value.toLocaleString()}`} />
                  <Area type="monotone" dataKey="total" stroke="#6366f1" fill="url(#areaGradient)" strokeWidth={2.5} name="Total" />
                </AreaChart>
              </ResponsiveContainer>
              <div className="text-right mt-2">
                <button onClick={() => navigate('/sales-history')} className="text-xs text-indigo-600 hover:text-indigo-800 flex items-center gap-1 ml-auto">
                  Ver historial <ArrowRight className="w-3 h-3" />
                </button>
              </div>
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-[300px] text-slate-400">
              <TrendingUp className="w-10 h-10 mb-2 opacity-40" />
              <p className="text-sm">Sin datos de ventas diarias</p>
            </div>
          )}
        </ChartCard>
      </div>

      {data?.recentMovements && data.recentMovements.length > 0 && (
        <div className="card">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-slate-700">Últimos Movimientos</h3>
            <button onClick={() => navigate('/inventory')} className="text-xs text-indigo-600 hover:text-indigo-800 flex items-center gap-1">
              Ver todos <ArrowRight className="w-3 h-3" />
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200">
                  <th className="text-left text-xs font-medium text-slate-500 uppercase px-3 py-2">Fecha</th>
                  <th className="text-left text-xs font-medium text-slate-500 uppercase px-3 py-2">Producto</th>
                  <th className="text-center text-xs font-medium text-slate-500 uppercase px-3 py-2">Tipo</th>
                  <th className="text-right text-xs font-medium text-slate-500 uppercase px-3 py-2">Cantidad</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.recentMovements.slice(0, 5).map((m) => {
                  const { label, color } = movementTypeLabel(m.type)
                  return (
                    <tr key={m.id} className="hover:bg-slate-50">
                      <td className="px-3 py-2 text-slate-600">{formatDateTime(m.createdAt)}</td>
                      <td className="px-3 py-2 font-medium">{m.productName}</td>
                      <td className="px-3 py-2 text-center"><span className={`badge ${color}`}>{label}</span></td>
                      <td className="px-3 py-2 text-right font-mono">{m.quantity}</td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

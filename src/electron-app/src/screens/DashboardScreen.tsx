import { useState, useEffect } from 'react'
import { TrendingUp, AlertTriangle, DollarSign, ShoppingCart } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, AreaChart, Area, CartesianGrid } from 'recharts'
import { useApi } from '../hooks/useApi'
import { reportService } from '../services/reportService'
import { ChartCard } from '../components/charts/ChartCard'

interface DashboardData {
  todaySales: { totalSales: number; totalRevenue: number; totalTax: number; averageTicket: number } | null
  lowStockCount: number
  topProducts: Array<{ productName: string; totalQuantity: number; totalRevenue: number }> | null
  recentMovements: Array<{ id: number; productName: string; type: string; quantity: number; createdAt: string }> | null
}

function getLast7Days(): { from: string; to: string } {
  const to = new Date()
  const from = new Date(to)
  from.setDate(from.getDate() - 6)
  return { from: from.toISOString().split('T')[0], to: to.toISOString().split('T')[0] }
}

export default function DashboardScreen() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [dailySales, setDailySales] = useState<Array<{ date: string; total: number; count: number }>>([])
  const [loading, setLoading] = useState(true)
  const { get } = useApi()

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [summary, salesReport] = await Promise.all([
          get<DashboardData>('/dashboard/summary'),
          reportService.getSalesReport(getLast7Days().from, getLast7Days().to),
        ])
        if (summary) setData(summary)
        if (salesReport.isSuccess && Array.isArray(salesReport.data)) {
          setDailySales(
            salesReport.data.map((r: any) => ({
              date: r.date || r.day || '',
              total: r.total || r.totalRevenue || 0,
              count: r.count || r.totalSales || 0,
            }))
          )
        }
      } finally {
        setLoading(false)
      }
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

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-slate-500">Ventas Hoy</p>
              <p className="text-2xl font-bold text-slate-900">{data?.todaySales?.totalSales ?? 0}</p>
            </div>
            <div className="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center">
              <ShoppingCart className="w-6 h-6 text-indigo-600" />
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-slate-500">Ingresos Hoy</p>
              <p className="text-2xl font-bold text-emerald-600">
                Gs. {(data?.todaySales?.totalRevenue ?? 0).toLocaleString()}
              </p>
            </div>
            <div className="w-12 h-12 bg-emerald-100 rounded-xl flex items-center justify-center">
              <DollarSign className="w-6 h-6 text-emerald-600" />
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-slate-500">Stock Bajo</p>
              <p className={`text-2xl font-bold ${(data?.lowStockCount ?? 0) > 0 ? 'text-red-600' : 'text-slate-900'}`}>
                {data?.lowStockCount ?? 0}
              </p>
            </div>
            <div className="w-12 h-12 bg-red-100 rounded-xl flex items-center justify-center">
              <AlertTriangle className="w-6 h-6 text-red-600" />
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-slate-500">Ticket Promedio</p>
              <p className="text-2xl font-bold text-slate-900">
                Gs. {(data?.todaySales?.averageTicket ?? 0).toLocaleString()}
              </p>
            </div>
            <div className="w-12 h-12 bg-amber-100 rounded-xl flex items-center justify-center">
              <TrendingUp className="w-6 h-6 text-amber-600" />
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard title="Productos más vendidos">
          {data?.topProducts && data.topProducts.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={data.topProducts} margin={{ top: 5, right: 20, left: 20, bottom: 60 }}>
                <XAxis dataKey="productName" tick={{ fontSize: 11 }} angle={-20} textAnchor="end" />
                <YAxis />
                <Tooltip formatter={(value: number) => value.toLocaleString()} />
                <Bar dataKey="totalQuantity" fill="#6366f1" radius={[4, 4, 0, 0]} name="Cantidad" />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-sm text-slate-400">Sin datos disponibles</p>
          )}
        </ChartCard>

        <ChartCard title="Ventas Diarias (últimos 7 días)">
          {dailySales.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={dailySales} margin={{ top: 5, right: 20, left: 20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                <YAxis />
                <Tooltip formatter={(value: number) => `Gs. ${value.toLocaleString()}`} />
                <Area type="monotone" dataKey="total" stroke="#6366f1" fill="#eef2ff" strokeWidth={2} name="Total" />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-sm text-slate-400">Sin datos de ventas diarias</p>
          )}
        </ChartCard>
      </div>
    </div>
  )
}

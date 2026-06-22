import { useState } from 'react'
import { FileSpreadsheet, FileText, TrendingUp, Package, DollarSign, ClipboardList } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts'
import { reportService } from '../services/reportService'
import { formatCurrency } from '../utils/format'
import { Logger } from '../utils/logger'

const REPORT_TYPES = [
  { value: 'sales', label: 'Ventas por período', icon: TrendingUp },
  { value: 'low-stock', label: 'Stock bajo', icon: Package },
  { value: 'profit', label: 'Margen de ganancia', icon: DollarSign },
  { value: 'inventory-value', label: 'Valorización de inventario', icon: ClipboardList },
] as const

type ReportType = (typeof REPORT_TYPES)[number]['value']

export default function ReportsScreen() {
  const [reportType, setReportType] = useState<ReportType>('sales')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [data, setData] = useState<any>(null)
  const [loading, setLoading] = useState(false)

  const showDateRange = reportType === 'sales' || reportType === 'profit'

  const loadReport = async () => {
    setLoading(true)
    try {
      let result
      switch (reportType) {
        case 'sales':
          result = await reportService.getSalesReport(fromDate || undefined, toDate || undefined)
          break
        case 'low-stock':
          result = await reportService.getLowStock()
          break
        case 'profit':
          result = await reportService.getProfit(fromDate || undefined, toDate || undefined)
          break
        case 'inventory-value':
          result = await reportService.getInventoryValue()
          break
      }
      if (result?.isSuccess) setData(result.data)
    } catch (err) { Logger.error('ReportsScreen', 'Error al cargar reporte', err) } finally { setLoading(false) }
  }

  const handleExport = async (format: 'excel' | 'pdf') => {
    try {
      const blob = await reportService.exportReport(reportType, format, fromDate || undefined, toDate || undefined)
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `reporte_${reportType}_${new Date().toISOString().split('T')[0]}.${format === 'excel' ? 'xlsx' : 'pdf'}`
      a.click()
      window.URL.revokeObjectURL(url)
    } catch (err) { Logger.error('ReportsScreen', 'Error al exportar', err) }
  }

  const renderSalesReport = () => {
    const rows = Array.isArray(data) ? data : []
    return (
      <div className="space-y-6">
        <div className="card overflow-hidden p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-200">
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Día</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Ventas</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Items</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {rows.map((row: any, i: number) => (
                <tr key={i} className="hover:bg-slate-50">
                  <td className="px-4 py-3 font-medium">{row.date ? new Date(row.date).toLocaleDateString('es-PY') : '---'}</td>
                  <td className="px-4 py-3 text-right">{row.totalSales ?? 0}</td>
                  <td className="px-4 py-3 text-right">{row.itemCount ?? 0}</td>
                  <td className="px-4 py-3 text-right font-medium">{formatCurrency(row.totalAmount ?? 0)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {rows.length > 0 && (
          <div className="card">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={rows} margin={{ top: 5, right: 20, left: 20, bottom: 5 }}>
                <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(v: string) => new Date(v).toLocaleDateString('es-PY', { day: '2-digit', month: '2-digit' })} />
                <YAxis />
                <Tooltip formatter={(value: number) => formatCurrency(value)} labelFormatter={(label: string) => new Date(label).toLocaleDateString('es-PY')} />
                <Bar dataKey="totalAmount" fill="#6366f1" name="Total" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>
    )
  }

  const renderLowStockReport = () => {
    const rows = Array.isArray(data) ? data : []
    return (
      <div className="card overflow-hidden p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200">
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Producto</th>
              <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Stock Actual</th>
              <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Stock Mínimo</th>
              <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Diferencia</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {rows.map((row: any, i: number) => (
              <tr key={i} className="hover:bg-slate-50">
                <td className="px-4 py-3 font-medium">{row.productName || row.name || '---'}</td>
                <td className={`px-4 py-3 text-right font-mono ${(row.currentStock ?? 0) <= (row.minStock ?? 0) ? 'text-red-600 font-medium' : ''}`}>
                  {row.currentStock ?? 0}
                </td>
                <td className="px-4 py-3 text-right font-mono text-slate-600">{row.minStock ?? 0}</td>
                <td className="px-4 py-3 text-right font-mono text-red-600">
                  {row.difference ?? ((row.currentStock ?? 0) - (row.minStock ?? 0))}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }

  const renderProfitReport = () => {
    const totalCost = data?.totalCost ?? 0
    const totalRevenue = data?.totalRevenue ?? 0
    const profit = data?.profit ?? 0
    const margin = data?.margin ?? 0

    return (
      <div className="space-y-6">
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="card">
            <p className="text-sm text-slate-500">Costo Total</p>
            <p className="text-xl font-bold text-slate-900">{formatCurrency(totalCost)}</p>
          </div>
          <div className="card">
            <p className="text-sm text-slate-500">Venta Total</p>
            <p className="text-xl font-bold text-emerald-600">{formatCurrency(totalRevenue)}</p>
          </div>
          <div className="card">
            <p className="text-sm text-slate-500">Ganancia</p>
            <p className={`text-xl font-bold ${profit >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
              {formatCurrency(profit)}
            </p>
          </div>
          <div className="card">
            <p className="text-sm text-slate-500">Margen</p>
            <p className={`text-xl font-bold ${margin >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
              {margin.toFixed(1)}%
            </p>
          </div>
        </div>
      </div>
    )
  }

  const renderInventoryValueReport = () => {
    const rows = Array.isArray(data) ? data : []
    const totalValue = rows.reduce((sum: number, r: any) => sum + (r.totalValue ?? (r.cost ?? 0) * (r.stock ?? r.currentStock ?? 0)), 0)
    return (
      <div className="space-y-6">
        <div className="card">
          <p className="text-sm text-slate-500">Valor Total del Inventario</p>
          <p className="text-2xl font-bold text-indigo-600">{formatCurrency(totalValue)}</p>
        </div>
        <div className="card overflow-hidden p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-200">
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Producto</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Costo Unitario</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Stock</th>
                <th className="text-right text-xs font-medium text-slate-500 uppercase px-4 py-3">Valor Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {rows.map((row: any, i: number) => {
                const unitCost = row.cost ?? row.unitCost ?? 0
                const stock = row.stock ?? row.currentStock ?? 0
                const value = row.totalValue ?? (unitCost * stock)
                return (
                  <tr key={i} className="hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium">{row.productName || row.name || '---'}</td>
                    <td className="px-4 py-3 text-right font-mono text-slate-600">{formatCurrency(unitCost)}</td>
                    <td className="px-4 py-3 text-right font-mono">{stock}</td>
                    <td className="px-4 py-3 text-right font-mono font-medium">{formatCurrency(value)}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </div>
    )
  }

  const renderReport = () => {
    if (!data) return null
    if (loading) return <div className="flex justify-center py-8"><div className="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" /></div>
    switch (reportType) {
      case 'sales': return renderSalesReport()
      case 'low-stock': return renderLowStockReport()
      case 'profit': return renderProfitReport()
      case 'inventory-value': return renderInventoryValueReport()
    }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-slate-900">Reportes</h1>

      <div className="card">
        <div className="flex items-end gap-4 mb-4 flex-wrap">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Tipo de Reporte</label>
            <select value={reportType} onChange={(e) => { setReportType(e.target.value as ReportType); setData(null) }} className="input-field">
              {REPORT_TYPES.map((rt) => (
                <option key={rt.value} value={rt.value}>{rt.label}</option>
              ))}
            </select>
          </div>
          {showDateRange && (
            <>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Desde</label>
                <input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} className="input-field" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Hasta</label>
                <input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} className="input-field" />
              </div>
            </>
          )}
          <button onClick={loadReport} disabled={loading} className="btn-primary">
            {loading ? 'Cargando...' : 'Generar'}
          </button>
        </div>

        <div className="flex gap-2">
          <button onClick={() => handleExport('excel')} className="btn-secondary flex items-center gap-2 text-sm">
            <FileSpreadsheet className="w-4 h-4" /> Excel
          </button>
          <button onClick={() => handleExport('pdf')} className="btn-secondary flex items-center gap-2 text-sm">
            <FileText className="w-4 h-4" /> PDF
          </button>
        </div>
      </div>

      {renderReport()}
    </div>
  )
}

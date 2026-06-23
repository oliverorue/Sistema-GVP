import { useState } from 'react'
import { FileSpreadsheet, FileText, Package, DollarSign, ClipboardList, Calendar, BarChart3 } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { toast } from 'sonner'
import { reportService } from '../services/reportService'
import { formatCurrency } from '../utils/format'
import { Logger } from '../utils/logger'
import type { SalesReportRow, LowStockProduct, ProfitReport, InventoryValueRow } from '../types/api'

const REPORT_TYPES = [
  { value: 'sales', label: 'Ventas por período', icon: BarChart3, color: 'indigo' },
  { value: 'low-stock', label: 'Stock bajo', icon: Package, color: 'red' },
  { value: 'profit', label: 'Margen de ganancia', icon: DollarSign, color: 'emerald' },
  { value: 'inventory-value', label: 'Inventario valorizado', icon: ClipboardList, color: 'purple' },
] as const

type ReportType = (typeof REPORT_TYPES)[number]['value']

export default function ReportsScreen() {
  const [reportType, setReportType] = useState<ReportType>('sales')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [data, setData] = useState<SalesReportRow[] | LowStockProduct[] | ProfitReport | InventoryValueRow[] | null>(null)
  const [loading, setLoading] = useState(false)
  const [exporting, setExporting] = useState<'excel' | 'pdf' | null>(null)

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
      else toast.error(result?.message || 'Error al cargar reporte')
    } catch (err) {
      Logger.error('ReportsScreen', 'Error al cargar reporte', err)
      toast.error('Error al cargar reporte')
    } finally { setLoading(false) }
  }

  const handleExport = async (format: 'excel' | 'pdf') => {
    if (!data) { toast.error('Genere el reporte primero'); return }
    setExporting(format)
    try {
      if (format === 'pdf') {
        const html = await reportService.exportReport(reportType, format, fromDate || undefined, toDate || undefined)
        if (window.electronAPI?.htmlToPdf) {
          // Electron: convertir HTML a PDF real via printToPDF
          const result = await window.electronAPI.htmlToPdf(html)
          if (result.success && result.data) {
            const byteChars = atob(result.data)
            const byteNums = new Array(byteChars.length)
            for (let i = 0; i < byteChars.length; i++) byteNums[i] = byteChars.charCodeAt(i)
            const pdfBlob = new Blob([new Uint8Array(byteNums)], { type: 'application/pdf' })
            const url = window.URL.createObjectURL(pdfBlob)
            const a = document.createElement('a')
            a.href = url; a.download = `reporte_${reportType}_${new Date().toISOString().split('T')[0]}.pdf`
            a.click(); window.URL.revokeObjectURL(url)
            toast.success('PDF exportado')
          } else {
            toast.error(result.message || 'Error al generar PDF')
          }
        } else {
          // Browser dev mode: abrir en ventana de impresión (el usuario la guarda como PDF)
          const w = window.open('', '_blank')
          if (w) {
            w.document.open()
            w.document.write(html)
            w.document.close()
            w.focus()
            toast.success('Abriendo PDF para descargar...')
          } else {
            toast.error('El navegador bloqueó la ventana emergente')
          }
        }
      } else {
        // CSV: download blob
        const blob = await reportService.exportReportBlob(reportType, format, fromDate || undefined, toDate || undefined)
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url; a.download = `reporte_${reportType}_${new Date().toISOString().split('T')[0]}.csv`
        a.click(); window.URL.revokeObjectURL(url)
        toast.success('CSV exportado')
      }
    } catch (err) {
      Logger.error('ReportsScreen', 'Error al exportar', err)
      toast.error('Error al exportar el reporte')
    } finally { setExporting(null) }
  }

  // ─── Sales Report ──────────────────────────────
  const renderSalesReport = () => {
    const rows = (Array.isArray(data) ? data : []) as SalesReportRow[]
    const totalSales = rows.reduce((s, r) => s + r.totalSales, 0)
    const totalAmount = rows.reduce((s, r) => s + r.totalAmount, 0)
    if (rows.length === 0) return <EmptyState icon={BarChart3} message="Sin ventas en este período" />

    return (
      <div className="space-y-4 animate-fadeIn">
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <SummaryCard title="Total Ventas" value={String(totalSales)} color="indigo" />
          <SummaryCard title="Total Facturado" value={formatCurrency(totalAmount)} color="emerald" />
          <SummaryCard title="Promedio Diario" value={formatCurrency(rows.length > 0 ? totalAmount / rows.length : 0)} color="amber" />
          <SummaryCard title="Días con ventas" value={String(rows.length)} color="purple" />
        </div>

        <div className="card">
          <h3 className="text-sm font-semibold text-slate-700 mb-4">Ventas Diarias</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={rows} margin={{ top: 5, right: 20, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(v: string) => new Date(v).toLocaleDateString('es-PY', { day: '2-digit', month: '2-digit' })} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(v: number) => formatCurrency(v)} labelFormatter={(l: string) => new Date(l).toLocaleDateString('es-PY')} />
              <Bar dataKey="totalAmount" fill="#6366f1" radius={[4, 4, 0, 0]} name="Total" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="card overflow-hidden p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gradient-to-r from-slate-50 to-white border-b-2 border-slate-200">
                <Th>Día</Th><Th right>Ventas</Th><Th right>Items</Th><Th right>Total</Th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {rows.map((r, i) => (
                <tr key={i} className="hover:bg-indigo-50/30 transition-colors">
                  <Td>{r.date ? new Date(r.date).toLocaleDateString('es-PY', { weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric' }) : '---'}</Td>
                  <Td right>{r.totalSales}</Td>
                  <Td right>{r.itemCount}</Td>
                  <Td right bold>{formatCurrency(r.totalAmount)}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    )
  }

  // ─── Low Stock Report ──────────────────────────
  const renderLowStockReport = () => {
    const rows = (Array.isArray(data) ? data : []) as LowStockProduct[]
    if (rows.length === 0) return <EmptyState icon={Package} message="No hay productos con stock bajo" />

    return (
      <div className="space-y-4 animate-fadeIn">
        <div className="grid grid-cols-2 gap-3">
          <SummaryCard title="Productos críticos" value={String(rows.length)} color="red" />
          <SummaryCard title="Déficit total" value={String(rows.reduce((s, r) => s + Math.abs(r.difference), 0))} color="amber" />
        </div>
        <div className="card overflow-hidden p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gradient-to-r from-red-50 to-white border-b-2 border-red-200">
                <Th>Producto</Th><Th right>Stock</Th><Th right>Mínimo</Th><Th right>Faltante</Th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {rows.map((r, i) => (
                <tr key={i} className="hover:bg-red-50/30 transition-colors">
                  <Td bold>{r.productName}</Td>
                  <Td right className={r.currentStock <= r.minStock ? 'text-red-600 font-medium' : ''}>{r.currentStock}</Td>
                  <Td right>{r.minStock}</Td>
                  <Td right bold className="text-red-600">{r.difference}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    )
  }

  // ─── Profit Report ─────────────────────────────
  const renderProfitReport = () => {
    const d = data as ProfitReport | null
    if (!d) return <EmptyState icon={DollarSign} message="Sin datos de margen en este período" />
    return (
      <div className="space-y-4 animate-fadeIn">
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <SummaryCard title="Costo Total" value={formatCurrency(d.totalCost)} color="slate" />
          <SummaryCard title="Ingreso Total" value={formatCurrency(d.totalRevenue)} color="blue" />
          <SummaryCard title="Ganancia" value={formatCurrency(d.profit)} color={d.profit >= 0 ? 'emerald' : 'red'} />
          <SummaryCard title="Margen" value={`${d.margin.toFixed(1)}%`} color={d.margin >= 0 ? 'emerald' : 'red'} />
        </div>
        <div className="card">
          <div className="w-full bg-slate-100 rounded-full h-6 overflow-hidden">
            <div className="h-full bg-gradient-to-r from-emerald-400 to-emerald-600 rounded-full transition-all duration-700 flex items-center justify-end pr-3" style={{ width: `${Math.min(d.margin, 100)}%` }}>
              {d.margin > 20 && <span className="text-xs text-white font-bold">{d.margin.toFixed(0)}% margen</span>}
            </div>
          </div>
          <p className="text-xs text-slate-400 mt-2 text-center">
            Por cada Gs. 100 vendidos, Gs. {d.profit > 0 ? formatCurrency(d.profit / d.totalRevenue * 100) : '0'} son ganancia
          </p>
        </div>
      </div>
    )
  }

  // ─── Inventory Value Report ────────────────────
  const renderInventoryValueReport = () => {
    const rows = (Array.isArray(data) ? data : []) as InventoryValueRow[]
    const totalValue = rows.reduce((s, r) => s + (r.totalValue ?? r.unitCost * r.currentStock), 0)
    if (rows.length === 0) return <EmptyState icon={ClipboardList} message="Inventario vacío" />

    return (
      <div className="space-y-4 animate-fadeIn">
        <div className="grid grid-cols-2 gap-3">
          <SummaryCard title="Productos en stock" value={String(rows.length)} color="purple" />
          <SummaryCard title="Valor Total" value={formatCurrency(totalValue)} color="indigo" />
        </div>
        <div className="card overflow-hidden p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gradient-to-r from-purple-50 to-white border-b-2 border-purple-200">
                <Th>Producto</Th><Th right>Costo</Th><Th right>Stock</Th><Th right>Valor</Th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {rows.map((r, i) => (
                <tr key={i} className="hover:bg-purple-50/30 transition-colors">
                  <Td bold>{r.productName}</Td>
                  <Td right>{formatCurrency(r.unitCost)}</Td>
                  <Td right>{r.currentStock}</Td>
                  <Td right bold>{formatCurrency(r.totalValue ?? r.unitCost * r.currentStock)}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    )
  }

  // ─── Main Render ───────────────────────────────
  const activeType = REPORT_TYPES.find(rt => rt.value === reportType)!

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-slate-900">Reportes</h1>

      {/* Filter Bar */}
      <div className="card">
        <div className="flex items-end gap-4 flex-wrap">
          <div>
            <label className="block text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Tipo</label>
            <select value={reportType} onChange={(e) => { setReportType(e.target.value as ReportType); setData(null) }} className="input-field w-52">
              {REPORT_TYPES.map((rt) => (
                <option key={rt.value} value={rt.value}>{rt.label}</option>
              ))}
            </select>
          </div>
          {showDateRange && (
            <>
              <div>
                <label className="block text-xs font-medium text-slate-500 uppercase tracking-wider mb-1"><Calendar className="w-3 h-3 inline mr-1" />Desde</label>
                <input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} className="input-field w-36" />
              </div>
              <div>
                <label className="block text-xs font-medium text-slate-500 uppercase tracking-wider mb-1"><Calendar className="w-3 h-3 inline mr-1" />Hasta</label>
                <input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} className="input-field w-36" />
              </div>
            </>
          )}
          <button onClick={loadReport} disabled={loading} className="btn-primary h-10 px-6">
            {loading ? <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin inline-block mr-1" /> : null}
            {loading ? 'Cargando...' : 'Generar'}
          </button>
          <div className="flex-1" />
          <div className="flex gap-2">
            <button onClick={() => handleExport('excel')} disabled={!data || exporting === 'excel'} className="btn-secondary h-10 px-4 text-sm flex items-center gap-1.5">
              <FileSpreadsheet className="w-4 h-4" />
              {exporting === 'excel' ? '...' : 'CSV'}
            </button>
            <button onClick={() => handleExport('pdf')} disabled={!data || exporting === 'pdf'} className="btn-secondary h-10 px-4 text-sm flex items-center gap-1.5">
              <FileText className="w-4 h-4" />
              {exporting === 'pdf' ? '...' : 'PDF'}
            </button>
          </div>
        </div>
      </div>

      {/* Report Content */}
      {loading ? (
        <div className="card flex flex-col items-center justify-center py-16">
          <div className="w-10 h-10 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin mb-3" />
          <p className="text-sm text-slate-400">Cargando reporte...</p>
        </div>
      ) : data === null ? (
        <div className="card flex flex-col items-center justify-center py-16 text-slate-400">
          <activeType.icon className="w-12 h-12 mb-3 text-slate-300" />
          <p className="text-sm font-medium">Seleccione el tipo de reporte y haga clic en Generar</p>
        </div>
      ) : (
        <div className="animate-fadeIn">
          {reportType === 'sales' ? renderSalesReport() :
           reportType === 'low-stock' ? renderLowStockReport() :
           reportType === 'profit' ? renderProfitReport() :
           reportType === 'inventory-value' ? renderInventoryValueReport() : null}
        </div>
      )}
    </div>
  )
}

// ─── Helper Components ───────────────────────────
const SummaryCard = ({ title, value, color }: { title: string; value: string; color: string }) => {
  const colors: Record<string, string> = {
    indigo: 'border-l-indigo-500 bg-gradient-to-br from-indigo-50 to-white',
    emerald: 'border-l-emerald-500 bg-gradient-to-br from-emerald-50 to-white',
    amber: 'border-l-amber-500 bg-gradient-to-br from-amber-50 to-white',
    red: 'border-l-red-500 bg-gradient-to-br from-red-50 to-white',
    purple: 'border-l-purple-500 bg-gradient-to-br from-purple-50 to-white',
    blue: 'border-l-blue-500 bg-gradient-to-br from-blue-50 to-white',
    slate: 'border-l-slate-500 bg-gradient-to-br from-slate-50 to-white',
  }
  return (
    <div className={`rounded-xl border border-slate-200/60 p-4 border-l-4 ${colors[color] || colors.indigo} shadow-sm`}>
      <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">{title}</p>
      <p className="text-xl font-bold text-slate-900 mt-1">{value}</p>
    </div>
  )
}

const EmptyState = ({ icon: Icon, message }: { icon: React.ComponentType<{ className?: string }>; message: string }) => (
  <div className="card flex flex-col items-center justify-center py-16 text-slate-400 animate-fadeIn">
    <Icon className="w-12 h-12 mb-3 text-slate-300" />
    <p className="text-sm font-medium">{message}</p>
  </div>
)

const Th = ({ right, children }: { right?: boolean; children: React.ReactNode }) => (
  <th className={`px-5 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider ${right ? 'text-right' : 'text-left'}`}>{children}</th>
)

const Td = ({ right, bold, className, children }: { right?: boolean; bold?: boolean; className?: string; children: React.ReactNode }) => (
  <td className={`px-5 py-3 text-slate-700 ${right ? 'text-right' : ''} ${bold ? 'font-semibold' : ''} ${className || ''}`}>{children}</td>
)

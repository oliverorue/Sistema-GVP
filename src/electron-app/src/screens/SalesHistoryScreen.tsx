import { useState, useEffect, useCallback } from 'react'
import { Filter, Ban, Printer } from 'lucide-react'
import { toast } from 'sonner'
import { saleService } from '../services/saleService'
import { formatDateTime, formatCurrency } from '../utils/format'
import { Logger } from '../utils/logger'
import { useAuth } from '../hooks/useAuth'
import type { SaleHistory } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal } from '../components/ui'
import { SearchInput } from '../components/shared/SearchInput'
import { usePrintTicket } from '../hooks/usePrintTicket'

export default function SalesHistoryScreen() {
  const { isAdmin } = useAuth()
  const { printSaleTicket } = usePrintTicket()
  const [sales, setSales] = useState<SaleHistory[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [voidSaleId, setVoidSaleId] = useState<number | null>(null)
  const [voidReason, setVoidReason] = useState('')

  const fetchSales = useCallback(async () => {
    setLoading(true)
    try {
      const result = await saleService.getHistory({ page, searchTerm: search })
      if (result.isSuccess && result.data) setSales((result.data as any).items ?? [])
    } catch (err) { Logger.error('SalesHistoryScreen', 'Error al cargar historial', err) } finally { setLoading(false) }
  }, [page, search])

  useEffect(() => { fetchSales() }, [fetchSales])

  const handleVoid = async () => {
    if (!voidSaleId || !voidReason.trim()) {
      toast.error('Debe ingresar un motivo de anulación')
      return
    }
    try {
      const result = await saleService.voidSale(voidSaleId, voidReason)
      if (result.isSuccess) {
        toast.success('Venta anulada exitosamente')
        setVoidSaleId(null)
        setVoidReason('')
        fetchSales()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('SalesHistoryScreen', 'Error al anular venta', err)
      toast.error('Error al anular la venta')
    }
  }

  const columns: ColumnDef<SaleHistory>[] = [
    { header: 'Factura', accessorKey: 'invoiceNumber', cell: ({ row }) => <span className="font-mono text-sm">{row.original.invoiceNumber}</span> },
    { header: 'Fecha', accessorKey: 'createdAt', cell: ({ row }) => <span className="text-sm text-slate-600">{formatDateTime(row.original.createdAt)}</span> },
    { header: 'Cliente', accessorKey: 'customerName', cell: ({ row }) => <span className="text-sm">{row.original.customerName || '---'}</span> },
    { header: 'Total', accessorKey: 'total', cell: ({ row }) => <span className="text-right font-medium">{formatCurrency(row.original.total)}</span> },
    { header: 'Método', accessorKey: 'paymentMethod', cell: ({ row }) => <span className="text-sm text-center">{row.original.paymentMethod}</span> },
    {
      header: 'Estado',
      accessorKey: 'status',
      cell: ({ row }) => (
        <span className={`badge ${row.original.status === 'Completed' ? 'badge-success' : row.original.status === 'Voided' ? 'badge-danger' : 'badge-warning'}`}>
          {row.original.status === 'Completed' ? 'Completada' : row.original.status === 'Voided' ? 'Anulada' : 'En espera'}
        </span>
      ),
    },
    ...(isAdmin
      ? [{
          header: 'Acciones' as const,
          id: 'actions' as const,
          cell: ({ row }: { row: { original: SaleHistory } }) => (
            <div className="flex items-center justify-center gap-2">
              {row.original.status === 'Completed' && (
                <>
                  <button
                    onClick={() => printSaleTicket(row.original.id)}
                    className="p-1 text-slate-400 hover:text-indigo-600"
                    title="Reimprimir ticket"
                  >
                    <Printer className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => { setVoidSaleId(row.original.id); setVoidReason('') }}
                    className="p-1 text-slate-400 hover:text-red-600"
                    title="Anular venta"
                  >
                    <Ban className="w-4 h-4" />
                  </button>
                </>
              )}
            </div>
          ),
        } as ColumnDef<SaleHistory>]
      : []),
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Historial de Ventas</h1>
        <div className="flex items-center gap-2">
          <div className="w-64">
            <SearchInput value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Buscar..." />
          </div>
          <button className="btn-secondary py-1.5"><Filter className="w-4 h-4" /></button>
        </div>
      </div>

      <DataTable columns={columns} data={sales} loading={loading} emptyMessage="Sin ventas registradas" page={page} totalPages={Math.ceil(sales.length / 25)} onPageChange={setPage} />

      <Modal isOpen={voidSaleId !== null} onClose={() => setVoidSaleId(null)} title="Anular Venta" size="sm">
        <div className="space-y-4">
          <p className="text-sm text-slate-600">¿Está seguro de anular esta venta? Esta acción no se puede deshacer.</p>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Motivo de anulación</label>
            <input
              type="text"
              value={voidReason}
              onChange={(e) => setVoidReason(e.target.value)}
              className="input-field"
              placeholder="Ingrese el motivo"
              autoFocus
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button onClick={() => setVoidSaleId(null)} className="btn-secondary">Cancelar</button>
            <button onClick={handleVoid} className="btn-danger">Anular Venta</button>
          </div>
        </div>
      </Modal>
    </div>
  )
}

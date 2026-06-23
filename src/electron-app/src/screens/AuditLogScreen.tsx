import { useState, useEffect, useCallback } from 'react'
import { Filter } from 'lucide-react'
import { auditService } from '../services/auditService'
import { formatDateTime } from '../utils/format'
import { Logger } from '../utils/logger'
import type { AuditLog } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'

const ACTION_LABELS: Record<string, string> = {
  Create: 'Creación', Update: 'Modificación', Delete: 'Eliminación',
  Login: 'Inicio Sesión', Logout: 'Cierre Sesión', CancelSale: 'Anulación Venta',
  BackupCreated: 'Backup Creado', BackupRestored: 'Backup Restaurado',
  ExportReport: 'Reporte Exportado', LowStockAlert: 'Alerta Stock',
}

const ENTITY_LABELS: Record<string, string> = {
  Product: 'Producto', Category: 'Categoría', Customer: 'Cliente',
  Supplier: 'Proveedor', Sale: 'Venta', User: 'Usuario',
  Company: 'Empresa', InventoryMovement: 'Mov. Inventario',
}

const ACTION_COLORS: Record<string, string> = {
  Create: 'badge-success', Update: 'badge-info', Delete: 'badge-danger',
  Login: 'badge-info', Logout: 'badge-warning', CancelSale: 'badge-danger',
  BackupCreated: 'badge-success', BackupRestored: 'badge-warning',
  ExportReport: 'badge-info', LowStockAlert: 'badge-warning',
}

export default function AuditLogScreen() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [loading, setLoading] = useState(true)
  const [showFilters, setShowFilters] = useState(false)
  const [entityFilter, setEntityFilter] = useState('')
  const [actionFilter, setActionFilter] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  const ENTITIES = ['', 'Product', 'Category', 'Customer', 'Supplier', 'User', 'Sale', 'InventoryMovement', 'Company']
  const ACTIONS = ['', 'Create', 'Update', 'Delete', 'Login', 'Logout', 'CancelSale', 'BackupCreated', 'BackupRestored', 'ExportReport']

  const fetchLogs = useCallback(async () => {
    setLoading(true)
    try {
      const result = await auditService.getLogs({
        pageNumber: page, pageSize: 25,
        entityFilter: entityFilter || undefined,
        actionFilter: actionFilter || undefined,
        startDate: dateFrom || undefined,
        endDate: dateTo || undefined,
      })
      if (result.isSuccess && result.data) {
        const paged = result.data as unknown as { items?: AuditLog[]; totalPages?: number; totalCount?: number }
        setLogs(paged.items ?? [])
        setTotalPages(paged.totalPages ?? 1)
      } else {
        setLogs([])
      }
    } catch (err) { Logger.error('AuditLogScreen', 'Error al cargar auditoria', err) } finally { setLoading(false) }
  }, [entityFilter, actionFilter, dateFrom, dateTo, page])

  useEffect(() => { fetchLogs() }, [fetchLogs])

  const columns: ColumnDef<AuditLog>[] = [
    { header: 'Fecha', accessorKey: 'createdAt',
      cell: ({ row }) => <span className="text-xs text-slate-500 whitespace-nowrap">{formatDateTime(row.original.createdAt)}</span> },
    { header: 'Usuario', accessorKey: 'userName',
      cell: ({ row }) => <span className="text-sm font-medium">{row.original.userName}</span> },
    {
      header: 'Acción', accessorKey: 'action',
      cell: ({ row }) => (
        <span className={`badge text-xs ${ACTION_COLORS[row.original.action] || 'badge-warning'}`}>
          {ACTION_LABELS[row.original.action] || row.original.action}
        </span>
      ),
    },
    { header: 'Entidad', accessorKey: 'entityName',
      cell: ({ row }) => <span className="text-sm">{ENTITY_LABELS[row.original.entityName] || row.original.entityName}</span> },
    { header: 'Detalle', accessorKey: 'summary',
      cell: ({ row }) => (
        <span className="text-sm text-slate-500">
          {row.original.summary ||
            `${ACTION_LABELS[row.original.action] || row.original.action} de ${ENTITY_LABELS[row.original.entityName] || row.original.entityName}${row.original.entityId ? ` #${row.original.entityId}` : ''}`}
        </span>
      )},
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Auditoría</h1>
        <button onClick={() => setShowFilters(!showFilters)} className="btn-secondary flex items-center gap-2">
          <Filter className="w-4 h-4" /> {showFilters ? 'Ocultar Filtros' : 'Filtros'}
        </button>
      </div>

      {showFilters && (
        <div className="card">
          <div className="flex items-end gap-4 flex-wrap">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Entidad</label>
              <select value={entityFilter} onChange={(e) => setEntityFilter(e.target.value)} className="input-field">
                {ENTITIES.map((e) => <option key={e} value={e}>{e || 'Todas'}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Acción</label>
              <select value={actionFilter} onChange={(e) => setActionFilter(e.target.value)} className="input-field">
                {ACTIONS.map((a) => <option key={a} value={a}>{a || 'Todas'}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Desde</label>
              <input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Hasta</label>
              <input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} className="input-field" />
            </div>
          </div>
        </div>
      )}

      <DataTable columns={columns} data={logs} loading={loading} emptyMessage="Sin registros de auditoría" page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  )
}

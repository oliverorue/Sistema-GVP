import { useState, useEffect, useCallback } from 'react'
import { Filter } from 'lucide-react'
import { auditService } from '../services/auditService'
import { formatDateTime } from '../utils/format'
import { Logger } from '../utils/logger'
import type { AuditLog } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'

export default function AuditLogScreen() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [loading, setLoading] = useState(true)
  const [showFilters, setShowFilters] = useState(false)
  const [entityFilter, setEntityFilter] = useState('')
  const [actionFilter, setActionFilter] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const ENTITIES = ['', 'Product', 'Category', 'Customer', 'Supplier', 'User', 'Sale', 'Inventory', 'Settings', 'Backup']
  const ACTIONS = ['', 'Create', 'Update', 'Delete']

  const fetchLogs = useCallback(async () => {
    setLoading(true)
    try {
      const result = await auditService.getLogs({
        entityFilter: entityFilter || undefined,
        actionFilter: actionFilter || undefined,
        startDate: dateFrom || undefined,
        endDate: dateTo || undefined,
      })
      if (result.isSuccess) setLogs(result.data || [])
    } catch (err) { Logger.error('AuditLogScreen', 'Error al cargar auditoria', err) } finally { setLoading(false) }
  }, [entityFilter, actionFilter, dateFrom, dateTo])

  useEffect(() => { fetchLogs() }, [fetchLogs])

  const columns: ColumnDef<AuditLog>[] = [
    { header: 'Fecha', accessorKey: 'createdAt', cell: ({ row }) => <span className="text-sm text-slate-600">{formatDateTime(row.original.createdAt)}</span> },
    { header: 'Usuario', accessorKey: 'userName', cell: ({ row }) => <span className="text-sm">{row.original.userName}</span> },
    {
      header: 'Acción',
      accessorKey: 'action',
      cell: ({ row }) => (
        <span className={`badge ${row.original.action === 'Create' ? 'badge-success' : row.original.action === 'Update' ? 'badge-info' : row.original.action === 'Delete' ? 'badge-danger' : 'badge-warning'}`}>
          {row.original.action}
        </span>
      ),
    },
    { header: 'Entidad', accessorKey: 'entityName', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.entityName}</span> },
    { header: 'Detalle', accessorKey: 'summary', cell: ({ row }) => <span className="text-sm text-slate-500">{row.original.summary || `${row.original.actionDisplay || ''} ${row.original.entityNameDisplay || ''}${row.original.entityId ? ` #${row.original.entityId}` : ''}`}</span> },
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

      <DataTable columns={columns} data={logs} loading={loading} emptyMessage="Sin registros de auditoría" />
    </div>
  )
}

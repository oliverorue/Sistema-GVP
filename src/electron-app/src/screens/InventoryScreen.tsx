import { useState, useEffect, useCallback } from 'react'
import { Plus, Filter } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { inventoryService } from '../services/inventoryService'
import { productService } from '../services/productService'
import { formatDateTime } from '../utils/format'
import { Logger } from '../utils/logger'
import { MOVEMENT_TYPES } from '../utils/constants'
import type { InventoryMovement, Product } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, FormField } from '../components/ui'
import { SearchInput } from '../components/shared/SearchInput'

const movementSchema = z.object({
  productId: z.number({ invalid_type_error: 'Seleccione un producto' }).min(1, 'Seleccione un producto'),
  type: z.string().min(1, 'Seleccione un tipo'),
  quantity: z.number({ invalid_type_error: 'Ingrese una cantidad' }),
  reason: z.string().min(1, 'La razón es requerida'),
  notes: z.string().optional(),
})

type MovementFormData = z.infer<typeof movementSchema>

export default function InventoryScreen() {
  const [movements, setMovements] = useState<InventoryMovement[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [productSearch, setProductSearch] = useState('')
  const [productResults, setProductResults] = useState<Product[]>([])
  const [showFilters, setShowFilters] = useState(false)
  const [filterType, setFilterType] = useState('')
  const [filterProduct, setFilterProduct] = useState('')
  const [filterDateFrom, setFilterDateFrom] = useState('')
  const [filterDateTo, setFilterDateTo] = useState('')

  const form = useForm<MovementFormData>({
    resolver: zodResolver(movementSchema),
    defaultValues: { productId: 0, type: 'IN', quantity: 1, reason: '', notes: '' },
  })

  const fetchMovements = useCallback(async () => {
    setLoading(true)
    try {
      const result = await inventoryService.getMovements()
      if (result.isSuccess && result.data) {
        let filtered = result.data
        if (filterType) filtered = filtered.filter((m) => m.type === filterType)
        if (filterProduct) filtered = filtered.filter((m) => m.productName.toLowerCase().includes(filterProduct.toLowerCase()))
        if (filterDateFrom) filtered = filtered.filter((m) => new Date(m.createdAt) >= new Date(filterDateFrom))
        if (filterDateTo) filtered = filtered.filter((m) => new Date(m.createdAt) <= new Date(filterDateTo + 'T23:59:59'))
        setMovements(filtered)
      }
    } catch (err) { Logger.error('InventoryScreen', 'Error al cargar movimientos', err) } finally { setLoading(false) }
  }, [filterType, filterProduct, filterDateFrom, filterDateTo])

  useEffect(() => { fetchMovements() }, [fetchMovements])

  useEffect(() => {
    if (productSearch.length < 2) { setProductResults([]); return }
    productService.search(productSearch).then((result) => {
      if (result.isSuccess && result.data?.items) setProductResults(result.data.items)
    })
  }, [productSearch])

  const openCreateModal = () => {
    form.reset({ productId: 0, type: 'IN', quantity: 1, reason: '', notes: '' })
    setProductSearch('')
    setProductResults([])
    setShowModal(true)
  }

  const selectProduct = (product: Product) => {
    form.setValue('productId', product.id)
    setProductSearch(product.name)
    setProductResults([])
  }

  const onSubmit = async (data: MovementFormData) => {
    const quantity = data.type === 'OUT' ? Math.abs(data.quantity) * -1 : Math.abs(data.quantity)
    try {
      const result = await inventoryService.createMovement({ ...data, quantity })
      if (result.isSuccess) {
        toast.success('Movimiento creado exitosamente')
        setShowModal(false)
        fetchMovements()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('InventoryScreen', 'Error al crear movimiento', err)
      toast.error('Error al crear el movimiento')
    }
  }

  const columns: ColumnDef<InventoryMovement>[] = [
    { header: 'Fecha', accessorKey: 'createdAt', cell: ({ row }) => <span className="text-sm text-slate-600">{formatDateTime(row.original.createdAt)}</span> },
    { header: 'Producto', accessorKey: 'productName', cell: ({ row }) => <span className="font-medium">{row.original.productName}</span> },
    {
      header: 'Tipo',
      accessorKey: 'type',
      cell: ({ row }) => (
        <span className={`badge ${row.original.type === 'IN' ? 'badge-success' : row.original.type === 'OUT' ? 'badge-warning' : 'badge-info'}`}>
          {row.original.type === 'IN' ? 'Entrada' : row.original.type === 'OUT' ? 'Salida' : 'Ajuste'}
        </span>
      ),
    },
    { header: 'Cantidad', accessorKey: 'quantity', cell: ({ row }) => <span className="text-right font-mono text-sm">{row.original.quantity}</span> },
    { header: 'Usuario', accessorKey: 'userName', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.userName}</span> },
    { header: 'Razón', accessorKey: 'reason', cell: ({ row }) => <span className="text-sm text-slate-500">{row.original.reason}</span> },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Movimientos de Inventario</h1>
        <div className="flex gap-2">
          <button onClick={() => setShowFilters(!showFilters)} className="btn-secondary flex items-center gap-2"><Filter className="w-4 h-4" /> Filtros</button>
          <button onClick={openCreateModal} className="btn-primary flex items-center gap-2"><Plus className="w-4 h-4" /> Nuevo Movimiento</button>
        </div>
      </div>

      {showFilters && (
        <div className="card">
          <div className="flex items-end gap-4 flex-wrap">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Tipo</label>
              <select value={filterType} onChange={(e) => setFilterType(e.target.value)} className="input-field">
                <option value="">Todos</option>
                {MOVEMENT_TYPES.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Producto</label>
              <SearchInput value={filterProduct} onChange={setFilterProduct} placeholder="Buscar producto..." />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Desde</label>
              <input type="date" value={filterDateFrom} onChange={(e) => setFilterDateFrom(e.target.value)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Hasta</label>
              <input type="date" value={filterDateTo} onChange={(e) => setFilterDateTo(e.target.value)} className="input-field" />
            </div>
          </div>
        </div>
      )}

      <DataTable columns={columns} data={movements} loading={loading} emptyMessage="Sin movimientos" />

      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Nuevo Movimiento" size="md">
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField label="Producto" error={form.formState.errors.productId?.message}>
            <div className="relative">
              <SearchInput value={productSearch} onChange={setProductSearch} placeholder="Buscar producto..." />
              {productResults.length > 0 && productSearch.length >= 2 && (
                <div className="absolute z-10 w-full mt-1 bg-white border border-slate-200 rounded-lg shadow-lg max-h-48 overflow-y-auto">
                  {productResults.map((p) => (
                    <button key={p.id} type="button" onClick={() => selectProduct(p)} className="w-full text-left px-3 py-2 text-sm hover:bg-slate-50 border-b border-slate-100 last:border-0">
                      <span className="font-medium">{p.name}</span>
                      <span className="text-slate-400 ml-2">Stock: {p.currentStock} {p.unit}</span>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </FormField>
          <FormField label="Tipo de Movimiento" error={form.formState.errors.type?.message}>
            <select {...form.register('type')} className="input-field">
              {MOVEMENT_TYPES.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </FormField>
          <FormField label="Cantidad" error={form.formState.errors.quantity?.message}>
            <input {...form.register('quantity', { valueAsNumber: true })} type="number" step="0.01" className="input-field" placeholder="0" />
          </FormField>
          <FormField label="Razón" error={form.formState.errors.reason?.message}>
            <input {...form.register('reason')} className="input-field" placeholder="Motivo del movimiento" />
          </FormField>
          <FormField label="Notas" error={form.formState.errors.notes?.message}>
            <input {...form.register('notes')} className="input-field" placeholder="Opcional" />
          </FormField>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setShowModal(false)} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">Crear Movimiento</button>
          </div>
        </form>
      </Modal>
    </div>
  )
}

import { useState, useEffect, useCallback } from 'react'
import { Plus, Edit2, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { productService } from '../services/productService'
import { categoryService } from '../services/categoryService'
import { formatCurrency } from '../utils/format'
import { Logger } from '../utils/logger'
import type { Product, Category } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'
import { SearchInput } from '../components/shared/SearchInput'

const productSchema = z.object({
  name: z.string().min(1, 'El nombre es requerido'),
  barcode: z.string().min(1, 'El código de barras es requerido'),
  sku: z.string().optional(),
  categoryId: z.number({ invalid_type_error: 'La categoría es requerida' }).min(1, 'Seleccione una categoría'),
  supplierId: z.number().optional(),
  price: z.number({ invalid_type_error: 'Ingrese un precio válido' }).positive('El precio debe ser mayor a 0'),
  cost: z.number({ invalid_type_error: 'Ingrese un costo válido' }).min(0, 'El costo no puede ser negativo'),
  minStock: z.number({ invalid_type_error: 'Ingrese un valor válido' }).min(0, 'El stock mínimo no puede ser negativo'),
  unit: z.string().min(1, 'Seleccione una unidad'),
  description: z.string().optional(),
})

type ProductFormData = z.infer<typeof productSchema>

const units = [
  { value: 'unidad', label: 'Unidad' },
  { value: 'kg', label: 'Kilogramo (kg)' },
  { value: 'm', label: 'Metro (m)' },
  { value: 'litro', label: 'Litro' },
]

export default function ProductsScreen() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [showModal, setShowModal] = useState<'create' | 'edit' | null>(null)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const [categories, setCategories] = useState<Category[]>([])
  const [deleteId, setDeleteId] = useState<number | null>(null)

  const form = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: { name: '', barcode: '', sku: '', categoryId: 0, supplierId: undefined, price: 0, cost: 0, minStock: 0, unit: 'unidad', description: '' },
  })

  const fetchProducts = useCallback(async () => {
    setLoading(true)
    try {
      const result = await productService.getAll(page, 25, search)
      if (result.isSuccess && result.data) {
        setProducts(result.data.items)
        setTotalPages(result.data.totalPages)
      }
    } catch (err) { Logger.error('ProductsScreen', 'Error al cargar productos', err) } finally {
      setLoading(false)
    }
  }, [page, search])

  useEffect(() => { fetchProducts() }, [fetchProducts])

  const openCreateModal = async () => {
    const result = await categoryService.getAll()
    if (result.isSuccess && result.data) setCategories(result.data)
    form.reset({ name: '', barcode: '', sku: '', categoryId: 0, supplierId: undefined, price: 0, cost: 0, minStock: 0, unit: 'unidad', description: '' })
    setShowModal('create')
  }

  const openEditModal = async (product: Product) => {
    const result = await categoryService.getAll()
    if (result.isSuccess && result.data) setCategories(result.data)
    setEditingProduct(product)
    form.reset({
      name: product.name,
      barcode: product.barcode,
      sku: product.sku || '',
      categoryId: product.categoryId,
      supplierId: product.supplierId || undefined,
      price: product.price,
      cost: product.cost,
      minStock: product.minStock,
      unit: product.unit,
      description: product.description || '',
    })
    setShowModal('edit')
  }

  const onSubmit = async (data: ProductFormData) => {
    try {
      if (showModal === 'create') {
        const result = await productService.create(data)
        if (result.isSuccess) {
          toast.success('Producto creado exitosamente')
          setShowModal(null)
          fetchProducts()
        } else {
          toast.error(result.message)
        }
      } else if (showModal === 'edit' && editingProduct) {
        const result = await productService.update(editingProduct.id, data)
        if (result.isSuccess) {
          toast.success('Producto actualizado exitosamente')
          setShowModal(null)
          setEditingProduct(null)
          fetchProducts()
        } else {
          toast.error(result.message)
        }
      }
    } catch (err) {
      Logger.error('ProductsScreen', 'Error al guardar producto', err)
      toast.error('Error al guardar el producto')
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      const result = await productService.delete(deleteId)
      if (result.isSuccess) {
        toast.success('Producto eliminado exitosamente')
        setDeleteId(null)
        fetchProducts()
      } else {
        toast.error(result.message)
      }
    } catch (err) {
      Logger.error('ProductsScreen', 'Error al eliminar producto', err)
      toast.error('Error al eliminar el producto')
    }
  }

  const columns: ColumnDef<Product>[] = [
    { header: 'Código', accessorKey: 'barcode', cell: ({ row }) => <span className="font-mono text-sm text-slate-500">{row.original.barcode}</span> },
    { header: 'Nombre', accessorKey: 'name', cell: ({ row }) => <span className="font-medium text-slate-800">{row.original.name}</span> },
    { header: 'Categoría', accessorKey: 'categoryName' },
    { header: 'Precio', accessorKey: 'price', cell: ({ row }) => <span className="text-right font-medium">{formatCurrency(row.original.price)}</span> },
    {
      header: 'Stock',
      accessorKey: 'currentStock',
      cell: ({ row }) => (
        <span className={row.original.currentStock <= row.original.minStock ? 'text-red-600 font-medium' : 'text-slate-600'}>
          {row.original.currentStock} {row.original.unit}
        </span>
      ),
    },
    {
      header: 'Acciones',
      id: 'actions',
      cell: ({ row }) => (
        <div className="flex items-center justify-center gap-2">
          <button onClick={() => openEditModal(row.original)} className="p-1 text-slate-400 hover:text-indigo-600"><Edit2 className="w-4 h-4" /></button>
          <button onClick={() => setDeleteId(row.original.id)} className="p-1 text-slate-400 hover:text-red-600"><Trash2 className="w-4 h-4" /></button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Productos</h1>
        <button onClick={openCreateModal} className="btn-primary flex items-center gap-2">
          <Plus className="w-4 h-4" /> Nuevo Producto
        </button>
      </div>

      <div className="max-w-md">
        <SearchInput value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Buscar productos..." />
      </div>

      <DataTable
        columns={columns}
        data={products}
        loading={loading}
        emptyMessage="Sin productos"
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <Modal
        isOpen={showModal !== null}
        onClose={() => { setShowModal(null); setEditingProduct(null) }}
        title={showModal === 'create' ? 'Nuevo Producto' : 'Editar Producto'}
        size="lg"
      >
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Nombre" error={form.formState.errors.name?.message}>
              <input {...form.register('name')} className="input-field" placeholder="Nombre del producto" />
            </FormField>
            <FormField label="Código de Barras" error={form.formState.errors.barcode?.message}>
              <input {...form.register('barcode')} className="input-field" placeholder="Código de barras" />
            </FormField>
            <FormField label="SKU" error={form.formState.errors.sku?.message}>
              <input {...form.register('sku')} className="input-field" placeholder="SKU (opcional)" />
            </FormField>
            <FormField label="Categoría" error={form.formState.errors.categoryId?.message}>
              <select {...form.register('categoryId', { valueAsNumber: true })} className="input-field">
                <option value={0}>Seleccione una categoría</option>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </FormField>
            <FormField label="Precio" error={form.formState.errors.price?.message}>
              <input {...form.register('price', { valueAsNumber: true })} type="number" step="0.01" className="input-field" placeholder="0" />
            </FormField>
            <FormField label="Costo" error={form.formState.errors.cost?.message}>
              <input {...form.register('cost', { valueAsNumber: true })} type="number" step="0.01" className="input-field" placeholder="0" />
            </FormField>
            <FormField label="Stock Mínimo" error={form.formState.errors.minStock?.message}>
              <input {...form.register('minStock', { valueAsNumber: true })} type="number" className="input-field" placeholder="0" />
            </FormField>
            <FormField label="Unidad" error={form.formState.errors.unit?.message}>
              <select {...form.register('unit')} className="input-field">
                {units.map((u) => <option key={u.value} value={u.value}>{u.label}</option>)}
              </select>
            </FormField>
            <div className="col-span-2">
              <FormField label="Descripción" error={form.formState.errors.description?.message}>
                <textarea {...form.register('description')} className="input-field" rows={3} placeholder="Descripción (opcional)" />
              </FormField>
            </div>
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setShowModal(null); setEditingProduct(null) }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">{showModal === 'create' ? 'Crear' : 'Guardar'}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={deleteId !== null}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Eliminar Producto"
        message="¿Está seguro de eliminar este producto? Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        variant="danger"
      />
    </div>
  )
}

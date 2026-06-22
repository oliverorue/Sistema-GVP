import { useState, useEffect, useCallback } from 'react'
import { Plus, Edit2, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { categoryService } from '../services/categoryService'
import { Logger } from '../utils/logger'
import type { Category } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'

const categorySchema = z.object({
  name: z.string().min(3, 'El nombre debe tener al menos 3 caracteres'),
  description: z.string().optional(),
})

type CategoryFormData = z.infer<typeof categorySchema>

export default function CategoriesScreen() {
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState<'create' | 'edit' | null>(null)
  const [editingCategory, setEditingCategory] = useState<Category | null>(null)
  const [deleteId, setDeleteId] = useState<number | null>(null)

  const form = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: '', description: '' },
  })

  const fetchCategories = useCallback(async () => {
    setLoading(true)
    try {
      const result = await categoryService.getAll()
      if (result.isSuccess && result.data) setCategories(result.data)
    } catch (err) { Logger.error('CategoriesScreen', 'Error al cargar categorias', err) } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchCategories() }, [fetchCategories])

  const openCreateModal = () => {
    form.reset({ name: '', description: '' })
    setShowModal('create')
  }

  const openEditModal = (category: Category) => {
    setEditingCategory(category)
    form.reset({ name: category.name, description: category.description || '' })
    setShowModal('edit')
  }

  const onSubmit = async (data: CategoryFormData) => {
    try {
      if (showModal === 'create') {
        const result = await categoryService.create(data)
        if (result.isSuccess) {
          toast.success('Categoría creada exitosamente')
          setShowModal(null)
          fetchCategories()
        } else toast.error(result.message)
      } else if (showModal === 'edit' && editingCategory) {
        const result = await categoryService.update(editingCategory.id, data)
        if (result.isSuccess) {
          toast.success('Categoría actualizada exitosamente')
          setShowModal(null)
          setEditingCategory(null)
          fetchCategories()
        } else toast.error(result.message)
      }
    } catch (err) {
      Logger.error('CategoriesScreen', 'Error al guardar categoria', err)
      toast.error('Error al guardar la categoría')
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      const result = await categoryService.delete(deleteId)
      if (result.isSuccess) {
        toast.success('Categoría eliminada exitosamente')
        setDeleteId(null)
        fetchCategories()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('CategoriesScreen', 'Error al eliminar categoria', err)
      toast.error('Error al eliminar la categoría')
    }
  }

  const columns: ColumnDef<Category>[] = [
    { header: 'Nombre', accessorKey: 'name', cell: ({ row }) => <span className="font-medium">{row.original.name}</span> },
    { header: 'Descripción', accessorKey: 'description', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.description || '---'}</span> },
    {
      header: 'Estado',
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <span className={`badge ${row.original.isActive ? 'badge-success' : 'badge-danger'}`}>
          {row.original.isActive ? 'Activo' : 'Inactivo'}
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
        <h1 className="text-2xl font-bold text-slate-900">Categorías</h1>
        <button onClick={openCreateModal} className="btn-primary flex items-center gap-2"><Plus className="w-4 h-4" /> Nueva Categoría</button>
      </div>

      <DataTable columns={columns} data={categories} loading={loading} emptyMessage="Sin categorías" />

      <Modal isOpen={showModal !== null} onClose={() => { setShowModal(null); setEditingCategory(null) }} title={showModal === 'create' ? 'Nueva Categoría' : 'Editar Categoría'} size="sm">
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField label="Nombre" error={form.formState.errors.name?.message}>
            <input {...form.register('name')} className="input-field" placeholder="Nombre de la categoría" />
          </FormField>
          <FormField label="Descripción" error={form.formState.errors.description?.message}>
            <textarea {...form.register('description')} className="input-field" rows={3} placeholder="Descripción (opcional)" />
          </FormField>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setShowModal(null); setEditingCategory(null) }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">{showModal === 'create' ? 'Crear' : 'Guardar'}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={deleteId !== null} onClose={() => setDeleteId(null)} onConfirm={handleDelete} title="Eliminar Categoría" message="¿Está seguro de eliminar esta categoría? Esta acción no se puede deshacer." confirmLabel="Eliminar" variant="danger" />
    </div>
  )
}

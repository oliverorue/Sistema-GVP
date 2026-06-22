import { useState, useEffect, useCallback } from 'react'
import { Plus, Edit2, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { supplierService } from '../services/supplierService'
import { Logger } from '../utils/logger'
import type { Supplier } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'

const supplierSchema = z.object({
  name: z.string().min(1, 'El nombre es requerido'),
  contactName: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().optional(),
  address: z.string().optional(),
  taxId: z.string().optional(),
})

type SupplierFormData = z.infer<typeof supplierSchema>

export default function SuppliersScreen() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState<'create' | 'edit' | null>(null)
  const [editingSupplier, setEditingSupplier] = useState<Supplier | null>(null)
  const [deleteId, setDeleteId] = useState<number | null>(null)

  const form = useForm<SupplierFormData>({
    resolver: zodResolver(supplierSchema),
    defaultValues: { name: '', contactName: '', phone: '', email: '', address: '', taxId: '' },
  })

  const fetchSuppliers = useCallback(async () => {
    setLoading(true)
    try {
      const result = await supplierService.getAll()
      if (result.isSuccess && result.data) setSuppliers(result.data)
    } catch (err) { Logger.error('SuppliersScreen', 'Error al cargar proveedores', err) } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchSuppliers() }, [fetchSuppliers])

  const openCreateModal = () => {
    form.reset({ name: '', contactName: '', phone: '', email: '', address: '', taxId: '' })
    setShowModal('create')
  }

  const openEditModal = (supplier: Supplier) => {
    setEditingSupplier(supplier)
    form.reset({
      name: supplier.name,
      contactName: supplier.contactName || '',
      phone: supplier.phone || '',
      email: supplier.email || '',
      address: supplier.address || '',
      taxId: supplier.taxId || '',
    })
    setShowModal('edit')
  }

  const onSubmit = async (data: SupplierFormData) => {
    try {
      if (showModal === 'create') {
        const result = await supplierService.create(data)
        if (result.isSuccess) {
          toast.success('Proveedor creado exitosamente')
          setShowModal(null)
          fetchSuppliers()
        } else toast.error(result.message)
      } else if (showModal === 'edit' && editingSupplier) {
        const result = await supplierService.update(editingSupplier.id, data)
        if (result.isSuccess) {
          toast.success('Proveedor actualizado exitosamente')
          setShowModal(null)
          setEditingSupplier(null)
          fetchSuppliers()
        } else toast.error(result.message)
      }
    } catch (err) {
      Logger.error('SuppliersScreen', 'Error al guardar proveedor', err)
      toast.error('Error al guardar el proveedor')
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      const result = await supplierService.delete(deleteId)
      if (result.isSuccess) {
        toast.success('Proveedor eliminado exitosamente')
        setDeleteId(null)
        fetchSuppliers()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('SuppliersScreen', 'Error al eliminar proveedor', err)
      toast.error('Error al eliminar el proveedor')
    }
  }

  const columns: ColumnDef<Supplier>[] = [
    { header: 'Nombre', accessorKey: 'name', cell: ({ row }) => <span className="font-medium">{row.original.name}</span> },
    { header: 'Contacto', accessorKey: 'contactName', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.contactName || '---'}</span> },
    { header: 'Teléfono', accessorKey: 'phone', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.phone || '---'}</span> },
    { header: 'Email', accessorKey: 'email', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.email || '---'}</span> },
    { header: 'RUC', accessorKey: 'taxId', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.taxId || '---'}</span> },
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
        <h1 className="text-2xl font-bold text-slate-900">Proveedores</h1>
        <button onClick={openCreateModal} className="btn-primary flex items-center gap-2"><Plus className="w-4 h-4" /> Nuevo Proveedor</button>
      </div>

      <DataTable columns={columns} data={suppliers} loading={loading} emptyMessage="Sin proveedores" />

      <Modal isOpen={showModal !== null} onClose={() => { setShowModal(null); setEditingSupplier(null) }} title={showModal === 'create' ? 'Nuevo Proveedor' : 'Editar Proveedor'} size="md">
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Nombre" error={form.formState.errors.name?.message}>
              <input {...form.register('name')} className="input-field" placeholder="Nombre del proveedor" />
            </FormField>
            <FormField label="Nombre de Contacto" error={form.formState.errors.contactName?.message}>
              <input {...form.register('contactName')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Teléfono" error={form.formState.errors.phone?.message}>
              <input {...form.register('phone')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Email" error={form.formState.errors.email?.message}>
              <input {...form.register('email')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Dirección" error={form.formState.errors.address?.message}>
              <input {...form.register('address')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="RUC" error={form.formState.errors.taxId?.message}>
              <input {...form.register('taxId')} className="input-field" placeholder="Opcional" />
            </FormField>
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setShowModal(null); setEditingSupplier(null) }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">{showModal === 'create' ? 'Crear' : 'Guardar'}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={deleteId !== null} onClose={() => setDeleteId(null)} onConfirm={handleDelete} title="Eliminar Proveedor" message="¿Está seguro de eliminar este proveedor? Esta acción no se puede deshacer." confirmLabel="Eliminar" variant="danger" />
    </div>
  )
}

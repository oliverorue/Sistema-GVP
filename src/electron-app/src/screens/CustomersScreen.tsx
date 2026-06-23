import { useState, useEffect, useCallback } from 'react'
import { Plus, Edit2, Trash2, DollarSign } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { customerService } from '../services/customerService'
import { formatCurrency } from '../utils/format'
import { Logger } from '../utils/logger'
import type { Customer } from '../types/entities'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'
import { SearchInput } from '../components/shared/SearchInput'

const customerSchema = z.object({
  name: z.string().min(1, 'El nombre es requerido'),
  taxId: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().optional(),
  address: z.string().optional(),
  creditLimit: z.number({ invalid_type_error: 'Ingrese un valor válido' }).min(0, 'No puede ser negativo'),
})

type CustomerFormData = z.infer<typeof customerSchema>

export default function CustomersScreen() {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [showModal, setShowModal] = useState<'create' | 'edit' | null>(null)
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null)
  const [deleteId, setDeleteId] = useState<number | null>(null)
  const [paymentCustomer, setPaymentCustomer] = useState<Customer | null>(null)
  const [paymentAmount, setPaymentAmount] = useState('')
  const [paymentNotes, setPaymentNotes] = useState('')

  const form = useForm<CustomerFormData>({
    resolver: zodResolver(customerSchema),
    defaultValues: { name: '', taxId: '', phone: '', email: '', address: '', creditLimit: 0 },
  })

  const fetchCustomers = useCallback(async () => {
    setLoading(true)
    try {
      const fn = search ? () => customerService.search(search) : () => customerService.getAll()
      const result = await fn()
      if (result.isSuccess && result.data) setCustomers(result.data)
    } catch (err) { Logger.error('CustomersScreen', 'Error al cargar clientes', err) } finally {
      setLoading(false)
    }
  }, [search])

  useEffect(() => { fetchCustomers() }, [fetchCustomers])

  const openCreateModal = () => {
    form.reset({ name: '', taxId: '', phone: '', email: '', address: '', creditLimit: 0 })
    setShowModal('create')
  }

  const openEditModal = (customer: Customer) => {
    setEditingCustomer(customer)
    form.reset({
      name: customer.name,
      taxId: customer.taxId || '',
      phone: customer.phone || '',
      email: customer.email || '',
      address: customer.address || '',
      creditLimit: customer.creditLimit,
    })
    setShowModal('edit')
  }

  const onSubmit = async (data: CustomerFormData) => {
    try {
      if (showModal === 'create') {
        const result = await customerService.create(data)
        if (result.isSuccess) {
          toast.success('Cliente creado exitosamente')
          setShowModal(null)
          fetchCustomers()
        } else toast.error(result.message)
      } else if (showModal === 'edit' && editingCustomer) {
        const result = await customerService.update(editingCustomer.id, data)
        if (result.isSuccess) {
          toast.success('Cliente actualizado exitosamente')
          setShowModal(null)
          setEditingCustomer(null)
          fetchCustomers()
        } else toast.error(result.message)
      }
    } catch (err) {
      Logger.error('CustomersScreen', 'Error al guardar cliente', err)
      toast.error('Error al guardar el cliente')
    }
  }

  const handlePayment = async () => {
    if (!paymentCustomer || !paymentAmount) return
    try {
      const amount = parseFloat(paymentAmount.replace(/\./g, '').replace(',', '.'))
      if (isNaN(amount) || amount <= 0) { toast.error('Ingrese un monto válido'); return }
      const result = await customerService.registerPayment(paymentCustomer.id, amount, paymentNotes || undefined)
      if (result.isSuccess) {
        toast.success(result.message || 'Pago registrado')
        setPaymentCustomer(null)
        setPaymentAmount('')
        setPaymentNotes('')
        fetchCustomers()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('CustomersScreen', 'Error al registrar pago', err)
      toast.error('Error al registrar el pago')
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      const result = await customerService.delete(deleteId)
      if (result.isSuccess) {
        toast.success('Cliente eliminado exitosamente')
        setDeleteId(null)
        fetchCustomers()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('CustomersScreen', 'Error al eliminar cliente', err)
      toast.error('Error al eliminar el cliente')
    }
  }

  const columns: ColumnDef<Customer>[] = [
    { header: 'Nombre', accessorKey: 'name', cell: ({ row }) => <span className="font-medium">{row.original.name}</span> },
    { header: 'Documento', accessorKey: 'taxId', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.taxId || '---'}</span> },
    { header: 'Teléfono', accessorKey: 'phone', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.phone || '---'}</span> },
    {
      header: 'Saldo',
      accessorKey: 'balance',
      cell: ({ row }) => <span className="text-right text-sm">{formatCurrency(row.original.balance)}</span>,
    },
    {
      header: 'Límite Crédito',
      accessorKey: 'creditLimit',
      cell: ({ row }) => <span className="text-right text-sm text-slate-600">{formatCurrency(row.original.creditLimit)}</span>,
    },
    {
      header: 'Acciones',
      id: 'actions',
      cell: ({ row }) => (
        <div className="flex items-center justify-center gap-1">
          <button onClick={() => {
            setPaymentCustomer(row.original)
            setPaymentAmount('')
            setPaymentNotes('')
          }} className="p-1 text-slate-400 hover:text-emerald-600" title="Registrar pago">
            <DollarSign className="w-4 h-4" />
          </button>
          <button onClick={() => openEditModal(row.original)} className="p-1 text-slate-400 hover:text-indigo-600"><Edit2 className="w-4 h-4" /></button>
          <button onClick={() => setDeleteId(row.original.id)} className="p-1 text-slate-400 hover:text-red-600"><Trash2 className="w-4 h-4" /></button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Clientes</h1>
        <button onClick={openCreateModal} className="btn-primary flex items-center gap-2"><Plus className="w-4 h-4" /> Nuevo Cliente</button>
      </div>

      <div className="max-w-md">
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar clientes..." />
      </div>

      <DataTable columns={columns} data={customers} loading={loading} emptyMessage="Sin clientes" />

      <Modal isOpen={showModal !== null} onClose={() => { setShowModal(null); setEditingCustomer(null) }} title={showModal === 'create' ? 'Nuevo Cliente' : 'Editar Cliente'} size="md">
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Nombre" error={form.formState.errors.name?.message}>
              <input {...form.register('name')} className="input-field" placeholder="Nombre del cliente" />
            </FormField>
            <FormField label="Documento (RUC/CI)" error={form.formState.errors.taxId?.message}>
              <input {...form.register('taxId')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Teléfono" error={form.formState.errors.phone?.message}>
              <input {...form.register('phone')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Email" error={form.formState.errors.email?.message}>
              <input {...form.register('email')} className="input-field" placeholder="Opcional" />
            </FormField>
            <div className="col-span-2">
              <FormField label="Dirección" error={form.formState.errors.address?.message}>
                <input {...form.register('address')} className="input-field" placeholder="Opcional" />
              </FormField>
            </div>
            <FormField label="Límite de Crédito" error={form.formState.errors.creditLimit?.message}>
              <input {...form.register('creditLimit', { valueAsNumber: true })} type="number" className="input-field" placeholder="0" />
            </FormField>
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setShowModal(null); setEditingCustomer(null) }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">{showModal === 'create' ? 'Crear' : 'Guardar'}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={deleteId !== null} onClose={() => setDeleteId(null)} onConfirm={handleDelete} title="Eliminar Cliente" message="¿Está seguro de eliminar este cliente? Esta acción no se puede deshacer." confirmLabel="Eliminar" variant="danger" />

      {/* Payment Modal */}
      <Modal isOpen={paymentCustomer !== null} onClose={() => { setPaymentCustomer(null); setPaymentAmount(''); setPaymentNotes('') }} title={`Cobrar a ${paymentCustomer?.name}`} size="sm">
        <div className="space-y-4">
          {paymentCustomer && (
            <div className="bg-slate-50 rounded-lg p-3 text-sm">
              <p className="text-slate-500">Saldo pendiente</p>
              <p className={`text-xl font-bold ${paymentCustomer.balance > 0 ? 'text-red-600' : 'text-emerald-600'}`}>
                {formatCurrency(paymentCustomer.balance)}
              </p>
            </div>
          )}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Monto a cobrar</label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm">Gs.</span>
              <input
                type="text"
                inputMode="decimal"
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(e.target.value)}
                className="input-field pl-10 text-lg font-semibold"
                placeholder="0"
              />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Notas</label>
            <input
              type="text"
              value={paymentNotes}
              onChange={(e) => setPaymentNotes(e.target.value)}
              className="input-field"
              placeholder="Opcional"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setPaymentCustomer(null); setPaymentAmount(''); setPaymentNotes('') }} className="btn-secondary">Cancelar</button>
            <button type="button" onClick={handlePayment} className="btn-primary">Registrar Pago</button>
          </div>
        </div>
      </Modal>
    </div>
  )
}

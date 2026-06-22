import { useState, useEffect, useCallback } from 'react'
import { Plus, Edit2, Trash2, Key } from 'lucide-react'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { userService } from '../services/userService'
import { USER_ROLES } from '../utils/constants'
import { Logger } from '../utils/logger'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'

const userSchema = z.object({
  username: z.string().min(1, 'El usuario es requerido'),
  fullName: z.string().min(1, 'El nombre completo es requerido'),
  email: z.string().optional(),
  role: z.string().min(1, 'Seleccione un rol'),
  password: z.string().optional(),
  isActive: z.boolean(),
})

type UserFormData = z.infer<typeof userSchema>

const resetPasswordSchema = z.object({
  newPassword: z.string().min(6, 'La contraseña debe tener al menos 6 caracteres'),
})

export default function UsersScreen() {
  const [users, setUsers] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState<'create' | 'edit' | null>(null)
  const [editingUser, setEditingUser] = useState<any | null>(null)
  const [resetPasswordId, setResetPasswordId] = useState<number | null>(null)
  const [deleteId, setDeleteId] = useState<number | null>(null)

  const form = useForm<UserFormData>({
    resolver: zodResolver(userSchema),
    defaultValues: { username: '', fullName: '', email: '', role: 'Cashier', password: '', isActive: true },
  })

  const resetPasswordForm = useForm<{ newPassword: string }>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '' },
  })

  const fetchUsers = useCallback(async () => {
    setLoading(true)
    try {
      const result = await userService.getAll()
      if (result.isSuccess && result.data) setUsers(result.data.items || [])
    } catch (err) { Logger.error('UsersScreen', 'Error al cargar usuarios', err) } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchUsers() }, [fetchUsers])

  const openCreateModal = () => {
    form.reset({ username: '', fullName: '', email: '', role: 'Cashier', password: '', isActive: true })
    setShowModal('create')
  }

  const openEditModal = (user: any) => {
    setEditingUser(user)
    form.reset({
      username: user.username,
      fullName: user.fullName,
      email: user.email || '',
      role: user.role,
      password: '',
      isActive: user.isActive,
    })
    setShowModal('edit')
  }

  const onSubmit = async (data: UserFormData) => {
    try {
      if (showModal === 'create') {
        if (!data.password || data.password.length < 6) {
          toast.error('La contraseña debe tener al menos 6 caracteres')
          return
        }
        const result = await userService.create(data as any)
        if (result.isSuccess) {
          toast.success('Usuario creado exitosamente')
          setShowModal(null)
          fetchUsers()
        } else toast.error(result.message)
      } else if (showModal === 'edit' && editingUser) {
        const { ...updateData } = data
        const result = await userService.update(editingUser.id, updateData as any)
        if (result.isSuccess) {
          toast.success('Usuario actualizado exitosamente')
          setShowModal(null)
          setEditingUser(null)
          fetchUsers()
        } else toast.error(result.message)
      }
    } catch (err) {
      Logger.error('UsersScreen', 'Error al guardar usuario', err)
      toast.error('Error al guardar el usuario')
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    try {
      const result = await userService.delete(deleteId)
      if (result.isSuccess) {
        toast.success('Usuario eliminado exitosamente')
        setDeleteId(null)
        fetchUsers()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('UsersScreen', 'Error al eliminar usuario', err)
      toast.error('Error al eliminar el usuario')
    }
  }

  const handleResetPassword = async (_data: { newPassword: string }) => {
    if (!resetPasswordId) return
    try {
      const result = await userService.resetPassword(resetPasswordId)
      if (result.isSuccess) {
        toast.success('Contraseña restablecida exitosamente')
        setResetPasswordId(null)
        resetPasswordForm.reset()
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('UsersScreen', 'Error al restablecer password', err)
      toast.error('Error al restablecer la contraseña')
    }
  }

  const columns: ColumnDef<any>[] = [
    { header: 'Usuario', accessorKey: 'username', cell: ({ row }) => <span className="font-mono text-sm">{row.original.username}</span> },
    { header: 'Nombre', accessorKey: 'fullName', cell: ({ row }) => <span className="font-medium">{row.original.fullName}</span> },
    { header: 'Email', accessorKey: 'email', cell: ({ row }) => <span className="text-sm text-slate-600">{row.original.email}</span> },
    {
      header: 'Rol',
      accessorKey: 'role',
      cell: ({ row }) => (
        <span className={`badge ${row.original.role === 'Admin' ? 'badge-info' : 'badge-success'}`}>
          {row.original.role === 'Admin' ? 'Admin' : 'Cajero'}
        </span>
      ),
    },
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
          <button onClick={() => setResetPasswordId(row.original.id)} className="p-1 text-slate-400 hover:text-amber-600"><Key className="w-4 h-4" /></button>
          <button onClick={() => setDeleteId(row.original.id)} className="p-1 text-slate-400 hover:text-red-600"><Trash2 className="w-4 h-4" /></button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Usuarios</h1>
        <button onClick={openCreateModal} className="btn-primary flex items-center gap-2"><Plus className="w-4 h-4" /> Nuevo Usuario</button>
      </div>

      <DataTable columns={columns} data={users} loading={loading} emptyMessage="Sin usuarios" />

      <Modal isOpen={showModal !== null} onClose={() => { setShowModal(null); setEditingUser(null) }} title={showModal === 'create' ? 'Nuevo Usuario' : 'Editar Usuario'} size="md">
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Usuario" error={form.formState.errors.username?.message}>
              <input {...form.register('username')} className="input-field" placeholder="Nombre de usuario" />
            </FormField>
            <FormField label="Nombre Completo" error={form.formState.errors.fullName?.message}>
              <input {...form.register('fullName')} className="input-field" placeholder="Nombre completo" />
            </FormField>
            <FormField label="Email" error={form.formState.errors.email?.message}>
              <input {...form.register('email')} className="input-field" placeholder="Opcional" />
            </FormField>
            <FormField label="Rol" error={form.formState.errors.role?.message}>
              <select {...form.register('role')} className="input-field">
                {USER_ROLES.map((r) => <option key={r.value} value={r.value}>{r.label}</option>)}
              </select>
            </FormField>
            {showModal === 'create' && (
              <FormField label="Contraseña" error={form.formState.errors.password?.message}>
                <input {...form.register('password')} type="password" className="input-field" placeholder="Mínimo 6 caracteres" />
              </FormField>
            )}
            <FormField label="Activo">
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" {...form.register('isActive')} className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500" />
                <span className="text-sm text-slate-600">Usuario activo</span>
              </label>
            </FormField>
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setShowModal(null); setEditingUser(null) }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">{showModal === 'create' ? 'Crear' : 'Guardar'}</button>
          </div>
        </form>
      </Modal>

      <Modal isOpen={resetPasswordId !== null} onClose={() => { setResetPasswordId(null); resetPasswordForm.reset() }} title="Restablecer Contraseña" size="sm">
        <form onSubmit={resetPasswordForm.handleSubmit(handleResetPassword)} className="space-y-4">
          <FormField label="Nueva Contraseña" error={resetPasswordForm.formState.errors.newPassword?.message}>
            <input {...resetPasswordForm.register('newPassword')} type="password" className="input-field" placeholder="Mínimo 6 caracteres" />
          </FormField>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => { setResetPasswordId(null); resetPasswordForm.reset() }} className="btn-secondary">Cancelar</button>
            <button type="submit" className="btn-primary">Restablecer</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={deleteId !== null} onClose={() => setDeleteId(null)} onConfirm={handleDelete} title="Eliminar Usuario" message="¿Está seguro de eliminar este usuario? Esta acción no se puede deshacer." confirmLabel="Eliminar" variant="danger" />
    </div>
  )
}

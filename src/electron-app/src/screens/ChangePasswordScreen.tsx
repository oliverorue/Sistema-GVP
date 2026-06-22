import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { authService } from '../services/authService'
import { useAuthStore } from '../stores/authStore'
import { Logger } from '../utils/logger'

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'La contraseña actual es requerida'),
    newPassword: z.string().min(6, 'La nueva contraseña debe tener al menos 6 caracteres'),
    confirmPassword: z.string().min(1, 'Confirme la nueva contraseña'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'],
  })

type ChangePasswordFormData = z.infer<typeof changePasswordSchema>

export default function ChangePasswordScreen() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const logout = useAuthStore((s) => s.logout)

  const form = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })

  const onSubmit = async (data: ChangePasswordFormData) => {
    setLoading(true)
    try {
      const result = await authService.changePassword(data.currentPassword, data.newPassword)
      if (result.isSuccess) {
        toast.success('Contraseña cambiada exitosamente')
        logout()
        navigate('/login')
      } else {
        toast.error(result.message)
      }
    } catch (err) {
      Logger.error('ChangePasswordScreen', 'Error al cambiar password', err)
      toast.error('Error al cambiar la contraseña')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50">
      <div className="card max-w-md w-full mx-4">
        <div className="text-center mb-6">
          <h1 className="text-2xl font-bold text-slate-900">Cambiar Contraseña</h1>
          <p className="text-sm text-slate-500 mt-1">Debe cambiar su contraseña antes de continuar</p>
        </div>

        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Contraseña Actual</label>
            <input
              {...form.register('currentPassword')}
              type="password"
              className="input-field"
              placeholder="Ingrese su contraseña actual"
            />
            {form.formState.errors.currentPassword && (
              <p className="text-sm text-red-500 mt-1">{form.formState.errors.currentPassword.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Nueva Contraseña</label>
            <input
              {...form.register('newPassword')}
              type="password"
              className="input-field"
              placeholder="Mínimo 6 caracteres"
            />
            {form.formState.errors.newPassword && (
              <p className="text-sm text-red-500 mt-1">{form.formState.errors.newPassword.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Confirmar Nueva Contraseña</label>
            <input
              {...form.register('confirmPassword')}
              type="password"
              className="input-field"
              placeholder="Repita la nueva contraseña"
            />
            {form.formState.errors.confirmPassword && (
              <p className="text-sm text-red-500 mt-1">{form.formState.errors.confirmPassword.message}</p>
            )}
          </div>

          <button type="submit" disabled={loading} className="btn-primary w-full">
            {loading ? 'Cambiando...' : 'Cambiar Contraseña'}
          </button>
        </form>
      </div>
    </div>
  )
}

import { z } from 'zod'

export const loginSchema = z.object({
  username: z.string().min(1, 'El usuario es requerido'),
  password: z.string().min(1, 'La contraseña es requerida'),
  companyId: z.number({ required_error: 'La empresa es requerida' }).min(1, 'Seleccione una empresa'),
})

export const productSchema = z.object({
  name: z.string().min(1, 'El nombre es requerido'),
  barcode: z.string().min(1, 'El codigo de barras es requerido'),
  sku: z.string().min(1, 'El SKU es requerido'),
  price: z.number().min(0, 'El precio debe ser mayor o igual a 0'),
  cost: z.number().min(0, 'El costo debe ser mayor o igual a 0'),
  categoryId: z.number().min(1, 'Seleccione una categoria'),
  minStock: z.number().min(0, 'El stock minimo debe ser mayor o igual a 0'),
  currentStock: z.number().min(0, 'El stock actual debe ser mayor o igual a 0'),
  unit: z.string().min(1, 'La unidad es requerida'),
})

export const customerSchema = z.object({
  name: z.string().min(1, 'El nombre es requerido'),
  taxId: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().email('Email invalido').optional().or(z.literal('')),
  address: z.string().optional(),
})

export const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, 'La contraseña actual es requerida'),
  newPassword: z.string().min(6, 'La nueva contraseña debe tener al menos 6 caracteres'),
  confirmPassword: z.string().min(1, 'Confirme la nueva contraseña'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
})

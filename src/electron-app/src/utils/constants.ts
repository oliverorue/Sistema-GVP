export const API_BASE_URL = 'http://127.0.0.1:5000/api'

export const PAYMENT_METHODS = [
  { value: 'Cash', label: 'Efectivo' },
  { value: 'Card', label: 'Tarjeta' },
  { value: 'Transfer', label: 'Transferencia' },
] as const

export const MOVEMENT_TYPES = [
  { value: 'IN', label: 'Entrada' },
  { value: 'OUT', label: 'Salida' },
  { value: 'ADJUSTMENT', label: 'Ajuste' },
] as const

export const SALE_STATUS = [
  { value: 'Completed', label: 'Completada', color: 'bg-emerald-100 text-emerald-800' },
  { value: 'Voided', label: 'Anulada', color: 'bg-red-100 text-red-800' },
  { value: 'OnHold', label: 'En espera', color: 'bg-amber-100 text-amber-800' },
] as const

export const USER_ROLES = [
  { value: 'Admin', label: 'Administrador' },
  { value: 'Cashier', label: 'Cajero' },
] as const

export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100] as const

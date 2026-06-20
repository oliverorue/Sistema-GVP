export interface ApiResponse<T> {
  isSuccess: boolean
  data: T | null
  message: string
  errors: string[]
}

export interface PagedData<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface LoginRequest {
  username: string
  password: string
  companyId: number
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: string
  requiresPasswordChange: boolean
  user: {
    id: number
    username: string
    fullName: string
    email: string
    role: string
    companyId: number
    mustChangePassword: boolean
  }
}

export interface CreateSaleRequest {
  customerId?: number
  paymentMethod: string
  cashAmount: number
  discount?: number
  notes?: string
  items: {
    productId: number
    quantity: number
    unitPrice: number
    discount?: number
  }[]
}

export interface CreateInventoryMovementRequest {
  productId: number
  type: string
  quantity: number
  reason: string
  notes?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

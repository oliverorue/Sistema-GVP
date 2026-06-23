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
  ivaIncluido?: boolean
  taxRate?: number
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

export interface SalesReportRow {
  date: string
  totalSales: number
  itemCount: number
  totalAmount: number
}

export interface LowStockProduct {
  productName: string
  currentStock: number
  minStock: number
  difference: number
}

export interface ProfitReport {
  totalCost: number
  totalRevenue: number
  profit: number
  margin: number
}

export interface InventoryValueRow {
  productName: string
  unitCost: number
  currentStock: number
  totalValue: number
}

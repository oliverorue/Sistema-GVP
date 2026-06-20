export interface User {
  id: number
  username: string
  fullName: string
  email: string
  role: 'Admin' | 'Cashier'
  companyId: number
  isActive: boolean
  mustChangePassword: boolean
  lastLogin?: string
}

export interface Product {
  id: number
  categoryId: number
  categoryName: string
  supplierId?: number
  supplierName?: string
  companyId: number
  name: string
  description?: string
  barcode: string
  sku: string
  price: number
  cost: number
  minStock: number
  currentStock: number
  unit: string
  imagePath?: string
  isActive: boolean
  createdAt: string
}

export interface Category {
  id: number
  companyId: number
  name: string
  description?: string
  isActive: boolean
}

export interface Customer {
  id: number
  companyId: number
  name: string
  taxId?: string
  phone?: string
  email?: string
  address?: string
  creditLimit: number
  balance: number
  isActive: boolean
}

export interface Supplier {
  id: number
  companyId: number
  name: string
  contactName?: string
  phone?: string
  email?: string
  address?: string
  taxId?: string
  isActive: boolean
  createdAt: string
}

export interface Sale {
  id: number
  companyId: number
  userId: number
  userName: string
  customerId?: number
  customerName?: string
  invoiceNumber: string
  subtotal: number
  tax: number
  discount: number
  total: number
  status: 'Completed' | 'Voided' | 'OnHold'
  paymentMethod: 'Cash' | 'Card' | 'Transfer'
  cashAmount: number
  changeAmount: number
  notes?: string
  createdAt: string
  items: SaleDetail[]
}

export interface SaleDetail {
  productId: number
  productName: string
  barcode: string
  quantity: number
  unitPrice: number
  discount: number
  subtotal: number
}

export interface SaleHistory {
  id: number
  invoiceNumber: string
  customerName?: string
  total: number
  paymentMethod: string
  status: string
  createdAt: string
  itemCount: number
  cashAmount: number
  changeAmount: number
}

export interface HeldSale {
  id: number
  customerName?: string
  items: SaleDetail[]
  heldAt: string
  saleTotal: number
}

export interface InventoryMovement {
  id: number
  productId: number
  productName: string
  productBarcode: string
  userId: number
  userName: string
  companyId: number
  relatedSaleId?: number
  type: string
  quantity: number
  stockBefore: number
  stockAfter: number
  reason: string
  notes?: string
  createdAt: string
}

export interface Company {
  id: number
  name: string
  taxId: string
  address: string
  phone: string
  email: string
  logoUrl?: string
  taxRate: number
  currency: string
  lowStockThreshold: number
  isActive: boolean
  createdAt: string
}

export interface AuditLog {
  id: number
  userId: number
  userName: string
  action: string
  entityName: string
  entityId?: number
  oldValues?: string
  newValues?: string
  ipAddress?: string
  createdAt: string
  summary?: string
  actionDisplay?: string
  entityNameDisplay?: string
}

export interface BackupInfo {
  fileName: string
  filePath: string
  fileSizeBytes: number
  createdAt: string
  hashSha256?: string
  companyName?: string
  createdByUser?: string
}

import api from './api'
import { ApiResponse } from '../types/api'
import { InventoryMovement, Product } from '../types/entities'

export const inventoryService = {
  getMovements: async () => {
    const response = await api.get<ApiResponse<InventoryMovement[]>>('/inventory/movements')
    return response.data
  },

  createMovement: async (data: {
    productId: number
    type: string
    quantity: number
    reason: string
    notes?: string
  }) => {
    const response = await api.post<ApiResponse<InventoryMovement>>('/inventory/movements', data)
    return response.data
  },

  getLowStock: async () => {
    const response = await api.get<ApiResponse<Product[]>>('/inventory/low-stock')
    return response.data
  },
}

import api from './api'
import { ApiResponse } from '../types/api'
import { Supplier } from '../types/entities'

export const supplierService = {
  getAll: async () => {
    const response = await api.get<ApiResponse<Supplier[]>>('/suppliers')
    return response.data
  },

  create: async (data: Partial<Supplier>) => {
    const response = await api.post<ApiResponse<Supplier>>('/suppliers', data)
    return response.data
  },

  update: async (id: number, data: Partial<Supplier>) => {
    const response = await api.put<ApiResponse<Supplier>>(`/suppliers/${id}`, data)
    return response.data
  },

  delete: async (id: number) => {
    const response = await api.delete<ApiResponse<boolean>>(`/suppliers/${id}`)
    return response.data
  },
}

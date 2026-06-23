import api from './api'
import { ApiResponse } from '../types/api'
import { Customer } from '../types/entities'

export const customerService = {
  getAll: async () => {
    const response = await api.get<ApiResponse<Customer[]>>('/customers')
    return response.data
  },

  search: async (q: string) => {
    const response = await api.get<ApiResponse<Customer[]>>(`/customers/search?q=${encodeURIComponent(q)}`)
    return response.data
  },

  create: async (data: Partial<Customer>) => {
    const response = await api.post<ApiResponse<Customer>>('/customers', data)
    return response.data
  },

  update: async (id: number, data: Partial<Customer>) => {
    const response = await api.put<ApiResponse<Customer>>(`/customers/${id}`, data)
    return response.data
  },

  delete: async (id: number) => {
    const response = await api.delete<ApiResponse<boolean>>(`/customers/${id}`)
    return response.data
  },

  registerPayment: async (id: number, amount: number, notes?: string) => {
    const response = await api.post<ApiResponse<Customer>>(`/customers/${id}/payment`, { amount, notes })
    return response.data
  },
}

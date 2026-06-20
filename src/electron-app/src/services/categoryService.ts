import api from './api'
import { ApiResponse } from '../types/api'
import { Category } from '../types/entities'

export const categoryService = {
  getAll: async () => {
    const response = await api.get<ApiResponse<Category[]>>('/categories')
    return response.data
  },

  create: async (data: { name: string; description?: string }) => {
    const response = await api.post<ApiResponse<Category>>('/categories', data)
    return response.data
  },

  update: async (id: number, data: { name: string; description?: string }) => {
    const response = await api.put<ApiResponse<Category>>(`/categories/${id}`, data)
    return response.data
  },

  delete: async (id: number) => {
    const response = await api.delete<ApiResponse<boolean>>(`/categories/${id}`)
    return response.data
  },
}

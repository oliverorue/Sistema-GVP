import api from './api'
import { ApiResponse, PagedData } from '../types/api'
import { User } from '../types/entities'

export const userService = {
  getAll: async (page = 1, pageSize = 25) => {
    const response = await api.get<ApiResponse<PagedData<User>>>(`/users?pageNumber=${page}&pageSize=${pageSize}`)
    return response.data
  },

  create: async (user: Partial<User>) => {
    const response = await api.post<ApiResponse<User>>('/users', user)
    return response.data
  },

  update: async (id: number, user: Partial<User>) => {
    const response = await api.put<ApiResponse<User>>(`/users/${id}`, user)
    return response.data
  },

  delete: async (id: number) => {
    const response = await api.delete<ApiResponse<boolean>>(`/users/${id}`)
    return response.data
  },

  resetPassword: async (id: number) => {
    const response = await api.post<ApiResponse<User>>(`/users/${id}/reset-password`)
    return response.data
  },
}

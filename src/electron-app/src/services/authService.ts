import api from './api'
import { ApiResponse, LoginRequest, LoginResponse } from '../types/api'
import { User } from '../types/entities'

export const authService = {
  login: async (data: LoginRequest) => {
    const response = await api.post<ApiResponse<LoginResponse>>('/auth/login', data)
    return response.data
  },

  logout: async () => {
    const response = await api.post<ApiResponse<null>>('/auth/logout')
    return response.data
  },

  getMe: async () => {
    const response = await api.get<ApiResponse<{ userId: number; userName: string; companyId: number; isAuthenticated: boolean; isAdmin: boolean }>>('/auth/me')
    return response.data
  },

  changePassword: async (currentPassword: string, newPassword: string) => {
    const response = await api.post<ApiResponse<User>>('/auth/change-password', { currentPassword, newPassword })
    return response.data
  },
}

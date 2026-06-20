import api from './api'
import { ApiResponse } from '../types/api'
import { Company } from '../types/entities'

export const settingsService = {
  getCompany: async () => {
    const response = await api.get<ApiResponse<Company>>('/settings/company')
    return response.data
  },

  updateCompany: async (company: Partial<Company>) => {
    const response = await api.put<ApiResponse<Company>>('/settings/company', company)
    return response.data
  },
}

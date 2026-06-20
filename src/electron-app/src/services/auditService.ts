import api from './api'
import { ApiResponse } from '../types/api'
import { AuditLog } from '../types/entities'

export const auditService = {
  getLogs: async (params?: {
    page?: number
    pageSize?: number
    actionFilter?: string
    entityFilter?: string
    startDate?: string
    endDate?: string
  }) => {
    const response = await api.get<ApiResponse<AuditLog[]>>('/audit/logs', { params })
    return response.data
  },

  getByEntity: async (entityName: string, entityId: number) => {
    const response = await api.get<ApiResponse<AuditLog[]>>(`/audit/logs/${entityName}/${entityId}`)
    return response.data
  },
}

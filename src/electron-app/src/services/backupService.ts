import api from './api'
import { ApiResponse } from '../types/api'
import { BackupInfo } from '../types/entities'

export const backupService = {
  create: async () => {
    const response = await api.post<ApiResponse<{ filePath: string }>>('/backups')
    return response.data
  },

  getAll: async () => {
    const response = await api.get<ApiResponse<BackupInfo[]>>('/backups')
    return response.data
  },

  getInfo: async (fileName: string) => {
    const response = await api.get<ApiResponse<BackupInfo>>(`/backups/${fileName}/info`)
    return response.data
  },

  restore: async (fileName: string) => {
    const response = await api.post<ApiResponse<boolean>>(`/backups/${fileName}/restore`)
    return response.data
  },
}

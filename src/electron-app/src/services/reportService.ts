import api from './api'
import { ApiResponse } from '../types/api'
import type { SalesReportRow, LowStockProduct, ProfitReport, InventoryValueRow } from '../types/api'

export const reportService = {
  getSalesReport: async (from?: string, to?: string) => {
    const params = new URLSearchParams()
    if (from) params.append('from', from)
    if (to) params.append('to', to)
    const response = await api.get<ApiResponse<SalesReportRow[]>>(`/reports/sales?${params}`)
    return response.data
  },

  getLowStock: async () => {
    const response = await api.get<ApiResponse<LowStockProduct[]>>('/reports/low-stock')
    return response.data
  },

  getProfit: async (from?: string, to?: string) => {
    const params = new URLSearchParams()
    if (from) params.append('from', from)
    if (to) params.append('to', to)
    const response = await api.get<ApiResponse<ProfitReport>>(`/reports/profit?${params}`)
    return response.data
  },

  getInventoryValue: async () => {
    const response = await api.get<ApiResponse<InventoryValueRow[]>>('/reports/inventory-value')
    return response.data
  },

  exportReport: async (type: string, format: 'excel' | 'pdf', from?: string, to?: string) => {
    const params = new URLSearchParams({ type, format })
    if (from) params.append('from', from)
    if (to) params.append('to', to)
    const response = await api.get(`/reports/export?${params}`, { responseType: 'text' })
    return response.data as string
  },

  exportReportBlob: async (type: string, format: 'excel' | 'pdf', from?: string, to?: string) => {
    const params = new URLSearchParams({ type, format })
    if (from) params.append('from', from)
    if (to) params.append('to', to)
    const response = await api.get(`/reports/export?${params}`, { responseType: 'blob' })
    return response.data as Blob
  },
}

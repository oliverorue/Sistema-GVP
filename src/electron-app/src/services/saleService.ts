import api from './api'
import { ApiResponse } from '../types/api'
import { CreateSaleRequest } from '../types/api'
import { Sale, SaleHistory, SaleDetail, HeldSale } from '../types/entities'

export const saleService = {
  create: async (data: CreateSaleRequest) => {
    const response = await api.post<ApiResponse<Sale>>('/sales', data)
    return response.data
  },

  getHistory: async (params: {
    page?: number
    pageSize?: number
    searchTerm?: string
    startDate?: string
    endDate?: string
    paymentMethod?: string
  }) => {
    const response = await api.get<ApiResponse<SaleHistory[]>>('/sales', { params })
    return response.data
  },

  getById: async (id: number) => {
    const response = await api.get<ApiResponse<Sale>>(`/sales/${id}`)
    return response.data
  },

  getDetail: async (id: number) => {
    const response = await api.get<ApiResponse<SaleDetail>>(`/sales/${id}`)
    return response.data
  },

  voidSale: async (id: number, reason: string) => {
    const response = await api.put<ApiResponse<boolean>>(`/sales/${id}/void`, { reason })
    return response.data
  },

  holdSale: async (data: CreateSaleRequest) => {
    const response = await api.post<ApiResponse<HeldSale>>('/sales/hold', data)
    return response.data
  },

  getHeldSales: async () => {
    const response = await api.get<ApiResponse<HeldSale[]>>('/sales/held')
    return response.data
  },

  resumeSale: async (id: number) => {
    const response = await api.post<ApiResponse<HeldSale>>(`/sales/${id}/resume`)
    return response.data
  },
}

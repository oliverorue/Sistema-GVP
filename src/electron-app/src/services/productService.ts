import api from './api'
import { ApiResponse, PagedData } from '../types/api'
import { Product } from '../types/entities'

export const productService = {
  getAll: async (page = 1, pageSize = 25, searchTerm?: string) => {
    const params = new URLSearchParams({ pageNumber: String(page), pageSize: String(pageSize) })
    if (searchTerm) params.append('searchTerm', searchTerm)
    const response = await api.get<ApiResponse<PagedData<Product>>>(`/products?${params}`)
    return response.data
  },

  getById: async (id: number) => {
    const response = await api.get<ApiResponse<Product>>(`/products/${id}`)
    return response.data
  },

  getByBarcode: async (code: string) => {
    const response = await api.get<ApiResponse<Product>>(`/products/barcode/${code}`)
    return response.data
  },

  search: async (q: string) => {
    const response = await api.get<ApiResponse<PagedData<Product>>>(`/products/search?q=${encodeURIComponent(q)}`)
    return response.data
  },

  create: async (product: Partial<Product>) => {
    const response = await api.post<ApiResponse<Product>>('/products', product)
    return response.data
  },

  update: async (id: number, product: Partial<Product>) => {
    const response = await api.put<ApiResponse<Product>>(`/products/${id}`, product)
    return response.data
  },

  delete: async (id: number) => {
    const response = await api.delete<ApiResponse<boolean>>(`/products/${id}`)
    return response.data
  },
}

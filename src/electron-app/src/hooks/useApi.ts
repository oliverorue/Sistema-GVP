import { useCallback } from 'react'
import { toast } from 'sonner'
import { AxiosError } from 'axios'
import api from '../services/api'
import type { ApiResponse } from '../types/api'

function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as { message?: string } | undefined
    if (data?.message) return data.message
    if (error.response?.status === 401) return 'Sesión expirada'
  }
  return 'Error de conexión'
}

export function useApi() {
  const get = useCallback(async <T>(url: string, params?: Record<string, unknown>) => {
    try {
      const response = await api.get<ApiResponse<T>>(url, { params })
      if (!response.data.isSuccess) {
        toast.error(response.data.message)
        return null
      }
      return response.data.data
    } catch (error) {
      toast.error(getErrorMessage(error))
      return null
    }
  }, [])

  const post = useCallback(async <T>(url: string, body?: unknown) => {
    try {
      const response = await api.post<ApiResponse<T>>(url, body)
      if (!response.data.isSuccess) {
        toast.error(response.data.message)
        return null
      }
      return response.data.data
    } catch (error) {
      toast.error(getErrorMessage(error))
      return null
    }
  }, [])

  const put = useCallback(async <T>(url: string, body?: unknown) => {
    try {
      const response = await api.put<ApiResponse<T>>(url, body)
      if (!response.data.isSuccess) {
        toast.error(response.data.message)
        return null
      }
      return response.data.data
    } catch (error) {
      toast.error(getErrorMessage(error))
      return null
    }
  }, [])

  const del = useCallback(async <T>(url: string) => {
    try {
      const response = await api.delete<ApiResponse<T>>(url)
      if (!response.data.isSuccess) {
        toast.error(response.data.message)
        return null
      }
      return response.data.data
    } catch (error) {
      toast.error(getErrorMessage(error))
      return null
    }
  }, [])

  return { get, post, put, del }
}

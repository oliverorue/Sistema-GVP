import { useCallback } from 'react'
import { toast } from 'sonner'
import { AxiosError } from 'axios'
import api from '../services/api'
import type { ApiResponse } from '../types/api'

export type ApiResult<T> =
  | { ok: true; data: T }
  | { ok: false; message: string; errors: string[] }

function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as { message?: string } | undefined
    if (data?.message) return data.message
    if (error.response?.status === 401) return 'Sesión expirada'
  }
  return 'Error de conexión'
}

export function useApi() {
  const get = useCallback(async <T>(url: string, params?: Record<string, unknown>): Promise<ApiResult<T>> => {
    try {
      const response = await api.get<ApiResponse<T>>(url, { params })
      if (!response.data.isSuccess) {
        return { ok: false, message: response.data.message, errors: response.data.errors || [] }
      }
      return { ok: true, data: response.data.data! }
    } catch (error) {
      const msg = getErrorMessage(error)
      toast.error(msg)
      return { ok: false, message: msg, errors: [] }
    }
  }, [])

  const post = useCallback(async <T>(url: string, body?: unknown): Promise<ApiResult<T>> => {
    try {
      const response = await api.post<ApiResponse<T>>(url, body)
      if (!response.data.isSuccess) {
        return { ok: false, message: response.data.message, errors: response.data.errors || [] }
      }
      return { ok: true, data: response.data.data! }
    } catch (error) {
      const msg = getErrorMessage(error)
      toast.error(msg)
      return { ok: false, message: msg, errors: [] }
    }
  }, [])

  const put = useCallback(async <T>(url: string, body?: unknown): Promise<ApiResult<T>> => {
    try {
      const response = await api.put<ApiResponse<T>>(url, body)
      if (!response.data.isSuccess) {
        return { ok: false, message: response.data.message, errors: response.data.errors || [] }
      }
      return { ok: true, data: response.data.data! }
    } catch (error) {
      const msg = getErrorMessage(error)
      toast.error(msg)
      return { ok: false, message: msg, errors: [] }
    }
  }, [])

  const del = useCallback(async <T>(url: string): Promise<ApiResult<T>> => {
    try {
      const response = await api.delete<ApiResponse<T>>(url)
      if (!response.data.isSuccess) {
        return { ok: false, message: response.data.message, errors: response.data.errors || [] }
      }
      return { ok: true, data: response.data.data! }
    } catch (error) {
      const msg = getErrorMessage(error)
      toast.error(msg)
      return { ok: false, message: msg, errors: [] }
    }
  }, [])

  return { get, post, put, del }
}

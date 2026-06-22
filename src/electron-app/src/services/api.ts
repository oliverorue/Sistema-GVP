import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { API_BASE_URL } from '../utils/constants'
import { Logger } from '../utils/logger'
import { ApiError, NetworkError, AuthenticationError } from '../utils/errorTypes'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
})

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('gvp_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  Logger.debug('api', `${config.method?.toUpperCase()} ${config.url}`, { params: config.params })
  return config
})

api.interceptors.response.use(
  (response) => {
    Logger.debug('api', `200 ${response.config.url}`)
    return response
  },
  (error: AxiosError) => {
    const status = error.response?.status
    const url = error.response?.config?.url || error.config?.url || 'unknown'

    Logger.error('api', `HTTP ${status} ${url}`, error)

    if (status === 401) {
      localStorage.removeItem('gvp_token')
      localStorage.removeItem('gvp_user')
      window.location.href = '/login'
      return Promise.reject(new AuthenticationError())
    }

    if (status === 403) {
      return Promise.reject(new ApiError(url, 403, 'No tienes permiso para esta acción'))
    }

    if (error.code === 'ECONNABORTED') {
      return Promise.reject(new Error('La operación tardó demasiado. Intenta nuevamente.'))
    }

    if (!error.response) {
      return Promise.reject(new NetworkError('Error de conexión. Verifica tu internet.'))
    }

    const apiMessage = (error.response?.data as any)?.message
    return Promise.reject(
      new ApiError(url, status || 500, apiMessage || error.message, error)
    )
  }
)

export default api

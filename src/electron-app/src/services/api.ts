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
      const requestUrl = error.config?.url || error.response?.config?.url || ''

      // Don't intercept login endpoint — let LoginScreen show specific credential error
      if (requestUrl.includes('/auth/login')) {
        return Promise.reject(error)
      }

      // Session expired for authenticated requests
      localStorage.removeItem('gvp_token')
      localStorage.removeItem('gvp_user')
      sessionStorage.setItem('gvp_session_expired', 'Sesión expirada. Inicia sesión nuevamente.')
      window.location.href = './'
      return Promise.reject(new AuthenticationError())
    }

    if (status === 403) {
      return Promise.reject(new ApiError(url, 403, 'No tienes permiso'))
    }

    if (status && status >= 500) {
      return Promise.reject(new ApiError(url, status, 'Error en servidor'))
    }

    if (error.code === 'ECONNABORTED') {
      return Promise.reject(new Error('Operación tardó demasiado'))
    }

    if (!error.response) {
      return Promise.reject(new NetworkError('Error de conexión'))
    }

    const apiMessage = (error.response?.data as { message?: string })?.message
    return Promise.reject(
      new ApiError(url, status || 500, apiMessage || error.message, error)
    )
  }
)

export default api

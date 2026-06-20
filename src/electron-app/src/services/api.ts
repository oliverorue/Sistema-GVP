import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { API_BASE_URL } from '../utils/constants'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('gvp_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('gvp_token')
      localStorage.removeItem('gvp_user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api

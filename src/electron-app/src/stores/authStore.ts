import { create } from 'zustand'
import { User } from '../types/entities'

interface AuthState {
  token: string | null
  user: User | null
  isAuthenticated: boolean
  isAdmin: boolean
  login: (token: string, user: User) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  token: localStorage.getItem('gvp_token'),
  user: JSON.parse(localStorage.getItem('gvp_user') || 'null'),
  isAuthenticated: !!localStorage.getItem('gvp_token'),
  isAdmin: (() => {
    const user = JSON.parse(localStorage.getItem('gvp_user') || 'null')
    return user?.role === 'Admin'
  })(),

  login: (token: string, user: User) => {
    localStorage.setItem('gvp_token', token)
    localStorage.setItem('gvp_user', JSON.stringify(user))
    set({ token, user, isAuthenticated: true, isAdmin: user.role === 'Admin' })
  },

  logout: () => {
    localStorage.removeItem('gvp_token')
    localStorage.removeItem('gvp_user')
    set({ token: null, user: null, isAuthenticated: false, isAdmin: false })
  },
}))

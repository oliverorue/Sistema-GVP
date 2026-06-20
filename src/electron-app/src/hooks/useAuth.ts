import { useAuthStore } from '../stores/authStore'

export function useAuth() {
  const { token, user, isAuthenticated, isAdmin, login, logout } = useAuthStore()
  return { token, user, isAuthenticated, isAdmin, login, logout }
}

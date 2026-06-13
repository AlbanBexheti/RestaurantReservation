import { createContext, useContext, useState, useCallback } from 'react'
import { authApi } from '../api/client'

const AuthContext = createContext(null)

// On page load, try to restore session from localStorage
function loadStoredUser() {
  try {
    const token = localStorage.getItem('token')
    const user  = JSON.parse(localStorage.getItem('user') || 'null')
    if (token && user) return user
  } catch {}
  return null
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStoredUser)
  const [loading, setLoading] = useState(false)
  const [error, setError]   = useState(null)

  // Persist auth response to localStorage and update state
  function persistAuth(response) {
    localStorage.setItem('token', response.token)
    const userData = {
      fullName: response.fullName,
      email:    response.email,
      role:     response.role,
    }
    localStorage.setItem('user', JSON.stringify(userData))
    setUser(userData)
  }

  const login = useCallback(async (email, password) => {
    setLoading(true)
    setError(null)
    try {
      const res = await authApi.login({ email, password })
      persistAuth(res)
      return true
    } catch (err) {
      setError(err.message)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  const register = useCallback(async (fullName, email, password) => {
    setLoading(true)
    setError(null)
    try {
      const res = await authApi.register({ fullName, email, password })
      persistAuth(res)
      return true
    } catch (err) {
      setError(err.message)
      return false
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }, [])

  const clearError = useCallback(() => setError(null), [])

  const isAdmin    = user?.role === 'Admin'
  const isLoggedIn = !!user

  return (
    <AuthContext.Provider value={{
      user, loading, error,
      login, register, logout,
      isAdmin, isLoggedIn, clearError,
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}

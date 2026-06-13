import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

// Redirects to /login if not authenticated
export function ProtectedRoute({ children }) {
  const { isLoggedIn } = useAuth()
  const location = useLocation()

  if (!isLoggedIn) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }
  return children
}

// Redirects to / if not Admin
export function RequireAdmin({ children }) {
  const { isLoggedIn, isAdmin } = useAuth()
  const location = useLocation()

  if (!isLoggedIn) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }
  if (!isAdmin) {
    return <Navigate to="/" replace />
  }
  return children
}

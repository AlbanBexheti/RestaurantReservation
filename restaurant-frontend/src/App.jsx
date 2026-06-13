import { Routes, Route, Navigate } from 'react-router-dom'
import Navbar from './components/layout/Navbar'
import { ProtectedRoute, RequireAdmin } from './components/common/ProtectedRoute'

// Public pages
import HomePage          from './pages/HomePage'
import MenuPage          from './pages/MenuPage'
import ReservePage       from './pages/customer/ReservePage'
import LoginPage         from './pages/auth/LoginPage'
import RegisterPage      from './pages/auth/RegisterPage'

// Customer pages (require login)
import MyReservationsPage from './pages/customer/MyReservationsPage'

// Admin pages (require Admin role)
import AdminDashboard    from './pages/admin/AdminDashboard'
import AdminReservations from './pages/admin/AdminReservations'
import AdminMenu         from './pages/admin/AdminMenu'
import AdminTables       from './pages/admin/AdminTables'

export default function App() {
  return (
    <>
      <Navbar />

      <Routes>
        {/* Public */}
        <Route path="/"        element={<HomePage />} />
        <Route path="/menu"    element={<MenuPage />} />
        <Route path="/reserve" element={<ReservePage />} />
        <Route path="/login"   element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Customer (must be logged in) */}
        <Route path="/my-reservations" element={
          <ProtectedRoute><MyReservationsPage /></ProtectedRoute>
        } />

        {/* Admin (must be logged in + Admin role) */}
        <Route path="/admin" element={
          <RequireAdmin><AdminDashboard /></RequireAdmin>
        } />
        <Route path="/admin/reservations" element={
          <RequireAdmin><AdminReservations /></RequireAdmin>
        } />
        <Route path="/admin/menu" element={
          <RequireAdmin><AdminMenu /></RequireAdmin>
        } />
        <Route path="/admin/tables" element={
          <RequireAdmin><AdminTables /></RequireAdmin>
        } />

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  )
}

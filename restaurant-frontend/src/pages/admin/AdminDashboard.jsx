import { Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { reservationApi, menuApi, tableApi } from '../../api/client'
import { useFetch } from '../../hooks/useFetch'
import './AdminPages.css'

export default function AdminDashboard() {
  const { user } = useAuth()
  const { data: reservations } = useFetch(() => reservationApi.getAll())
  const { data: menuItems    } = useFetch(() => menuApi.getAll())
  const { data: tables       } = useFetch(() => tableApi.getAll())

  const pending   = reservations?.filter(r => r.status === 'Pending').length   ?? '–'
  const confirmed = reservations?.filter(r => r.status === 'Confirmed').length ?? '–'
  const totalRes  = reservations?.length ?? '–'
  const totalMenu = menuItems?.length    ?? '–'
  const totalTables = tables?.length     ?? '–'

  return (
    <div className="page">
      <div className="container">
        <div className="admin-welcome">
          <h2 className="section-title">Admin Dashboard</h2>
          <p className="section-sub">Welcome back, {user?.fullName}</p>
        </div>

        {/* Stats */}
        <div className="admin-stats">
          <div className="stat-card card">
            <div className="stat-value">{totalRes}</div>
            <div className="stat-label">Total Reservations</div>
          </div>
          <div className="stat-card card">
            <div className="stat-value" style={{ color: 'var(--gold)' }}>{pending}</div>
            <div className="stat-label">Pending</div>
          </div>
          <div className="stat-card card">
            <div className="stat-value" style={{ color: 'var(--green)' }}>{confirmed}</div>
            <div className="stat-label">Confirmed</div>
          </div>
          <div className="stat-card card">
            <div className="stat-value">{totalTables}</div>
            <div className="stat-label">Tables</div>
          </div>
          <div className="stat-card card">
            <div className="stat-value">{totalMenu}</div>
            <div className="stat-label">Menu Items</div>
          </div>
        </div>

        {/* Quick nav */}
        <h3 style={{ margin: '2rem 0 1rem', fontFamily: 'var(--font-display)' }}>
          Manage
        </h3>
        <div className="admin-nav-grid">
          <Link to="/admin/reservations" className="admin-nav-card card">
            <div className="admin-nav-icon">📋</div>
            <div>
              <h3>Reservations</h3>
              <p>View, confirm, or cancel all bookings</p>
            </div>
          </Link>
          <Link to="/admin/menu" className="admin-nav-card card">
            <div className="admin-nav-icon">🍽️</div>
            <div>
              <h3>Menu</h3>
              <p>Add, edit, or remove menu items</p>
            </div>
          </Link>
          <Link to="/admin/tables" className="admin-nav-card card">
            <div className="admin-nav-icon">🪑</div>
            <div>
              <h3>Tables</h3>
              <p>Manage table capacity and locations</p>
            </div>
          </Link>
        </div>
      </div>
    </div>
  )
}

import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import './Navbar.css'

export default function Navbar() {
  const { user, isAdmin, isLoggedIn, logout } = useAuth()
  const navigate  = useNavigate()
  const location  = useLocation()

  function handleLogout() {
    logout()
    navigate('/')
  }

  function isActive(path) {
    return location.pathname === path || location.pathname.startsWith(path + '/')
  }

  return (
    <header className="navbar">
      <div className="navbar-inner container">
        {/* Brand */}
        <Link to="/" className="navbar-brand">
          <span className="navbar-brand-serif">La Tavola</span>
        </Link>

        {/* Nav links */}
        <nav className="navbar-links">
          <Link to="/menu" className={isActive('/menu') ? 'active' : ''}>
            Menu
          </Link>
          <Link to="/reserve" className={isActive('/reserve') ? 'active' : ''}>
            Reserve
          </Link>

          {isLoggedIn && !isAdmin && (
            <Link to="/my-reservations" className={isActive('/my-reservations') ? 'active' : ''}>
              My Bookings
            </Link>
          )}

          {isAdmin && (
            <Link to="/admin" className={isActive('/admin') ? 'active' : ''}>
              Admin
            </Link>
          )}
        </nav>

        {/* Auth area */}
        <div className="navbar-auth">
          {isLoggedIn ? (
            <>
              <span className="navbar-user">
                {user.fullName.split(' ')[0]}
                {isAdmin && <span className="navbar-role-badge">Admin</span>}
              </span>
              <button className="btn btn-ghost btn-sm" onClick={handleLogout}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn btn-ghost btn-sm">Sign in</Link>
              <Link to="/register" className="btn btn-primary btn-sm">Register</Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}

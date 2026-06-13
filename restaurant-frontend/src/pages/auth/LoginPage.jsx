import { useState, useEffect } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import './AuthPages.css'

export default function LoginPage() {
  const { login, loading, error, clearError, isLoggedIn } = useAuth()
  const navigate  = useNavigate()
  const location  = useLocation()
  const from      = location.state?.from?.pathname || '/'

  const [email,    setEmail]    = useState('')
  const [password, setPassword] = useState('')

  // If already logged in, bounce away
  useEffect(() => { if (isLoggedIn) navigate(from, { replace: true }) }, [isLoggedIn])
  useEffect(() => clearError, [])

  async function handleSubmit(e) {
    e.preventDefault()
    const ok = await login(email, password)
    if (ok) navigate(from, { replace: true })
  }

  return (
    <div className="auth-page page">
      <div className="auth-card card">
        <div className="auth-header">
          <h2>Welcome back</h2>
          <div className="divider" />
          <p>Sign in to manage your reservations</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-full btn-lg"
            disabled={loading}
            style={{ marginTop: '0.5rem' }}
          >
            {loading ? 'Signing in…' : 'Sign In'}
          </button>
        </form>

        <p className="auth-switch">
          Don't have an account?{' '}
          <Link to="/register">Create one</Link>
        </p>

        {/* Dev shortcut */}
        <div className="auth-hint">
          <strong>Demo admin:</strong> admin@restaurant.com / Admin123!
        </div>
      </div>
    </div>
  )
}

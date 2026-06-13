import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import './AuthPages.css'

export default function RegisterPage() {
  const { register, loading, error, clearError, isLoggedIn } = useAuth()
  const navigate = useNavigate()

  const [fullName, setFullName] = useState('')
  const [email,    setEmail]    = useState('')
  const [password, setPassword] = useState('')
  const [confirm,  setConfirm]  = useState('')
  const [localErr, setLocalErr] = useState('')

  useEffect(() => { if (isLoggedIn) navigate('/') }, [isLoggedIn])
  useEffect(() => clearError, [])

  async function handleSubmit(e) {
    e.preventDefault()
    setLocalErr('')

    if (password.length < 8) {
      setLocalErr('Password must be at least 8 characters.')
      return
    }
    if (password !== confirm) {
      setLocalErr('Passwords do not match.')
      return
    }

    const ok = await register(fullName, email, password)
    if (ok) navigate('/')
  }

  const displayError = localErr || error

  return (
    <div className="auth-page page">
      <div className="auth-card card">
        <div className="auth-header">
          <h2>Create Account</h2>
          <div className="divider" />
          <p>Join us and start booking your table</p>
        </div>

        {displayError && <div className="alert alert-error">{displayError}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Full Name</label>
            <input
              type="text"
              value={fullName}
              onChange={e => setFullName(e.target.value)}
              placeholder="Jane Smith"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Password</label>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                placeholder="Min 8 characters"
                required
              />
            </div>
            <div className="form-group">
              <label>Confirm Password</label>
              <input
                type="password"
                value={confirm}
                onChange={e => setConfirm(e.target.value)}
                placeholder="Repeat password"
                required
              />
            </div>
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-full btn-lg"
            disabled={loading}
            style={{ marginTop: '0.5rem' }}
          >
            {loading ? 'Creating account…' : 'Create Account'}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  )
}

import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import './HomePage.css'

export default function HomePage() {
  const { isLoggedIn } = useAuth()

  return (
    <div className="home">
      {/* Hero */}
      <section className="hero">
        <div className="hero-content container">
          <p className="hero-eyebrow">Fine Dining Experience</p>
          <h1 className="hero-title">
            A Table for<br />Every Occasion
          </h1>
          <p className="hero-sub">
            Fresh ingredients, thoughtful cooking, and warm hospitality.
            Reserve your table in minutes.
          </p>
          <div className="hero-actions">
            <Link to="/reserve" className="btn btn-primary btn-lg">
              Book a Table
            </Link>
            <Link to="/menu" className="btn btn-outline btn-lg">
              View Menu
            </Link>
          </div>
        </div>
        <div className="hero-decoration">
          <div className="hero-circle hero-circle-1" />
          <div className="hero-circle hero-circle-2" />
        </div>
      </section>

      {/* Features strip */}
      <section className="features container">
        <div className="feature-card">
          <div className="feature-icon">🍽️</div>
          <h3>Seasonal Menu</h3>
          <p>Dishes crafted from locally sourced, seasonal ingredients that change with the harvest.</p>
        </div>
        <div className="feature-card">
          <div className="feature-icon">📅</div>
          <h3>Easy Reservations</h3>
          <p>Check availability in real time and secure your table in under a minute.</p>
        </div>
        <div className="feature-card">
          <div className="feature-icon">🌿</div>
          <h3>Indoor & Outdoor</h3>
          <p>Choose from our cosy indoor dining room or the open-air terrace.</p>
        </div>
      </section>

      {/* CTA banner */}
      {!isLoggedIn && (
        <section className="cta-banner">
          <div className="container">
            <h2>Ready to join us?</h2>
            <p>Create an account to manage your bookings and get personalised updates.</p>
            <div className="hero-actions">
              <Link to="/register" className="btn btn-primary btn-lg">Create Account</Link>
              <Link to="/login"    className="btn btn-ghost btn-lg">Sign In</Link>
            </div>
          </div>
        </section>
      )}
    </div>
  )
}

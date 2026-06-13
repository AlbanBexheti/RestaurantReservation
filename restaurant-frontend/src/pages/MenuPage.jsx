import { useState } from 'react'
import { menuApi } from '../api/client'
import { useFetch } from '../hooks/useFetch'
import './MenuPage.css'

const CATEGORIES = ['All', 'Starters', 'Main', 'Desserts', 'Drinks']

function formatPrice(price) {
  return `$${Number(price).toFixed(2)}`
}

export default function MenuPage() {
  const [activeCategory, setActiveCategory] = useState('All')
  const { data: items, loading, error } = useFetch(() => menuApi.getAvailable())

  const filtered = !items ? [] : activeCategory === 'All'
    ? items
    : items.filter(i => i.category === activeCategory)

  return (
    <div className="page">
      <div className="container">
        <h2 className="section-title">Our Menu</h2>
        <p className="section-sub">Fresh, seasonal dishes prepared daily</p>

        {/* Category tabs */}
        <div className="category-tabs">
          {CATEGORIES.map(cat => (
            <button
              key={cat}
              className={`category-tab ${activeCategory === cat ? 'active' : ''}`}
              onClick={() => setActiveCategory(cat)}
            >
              {cat}
            </button>
          ))}
        </div>

        {/* States */}
        {loading && <div className="spinner" />}
        {error   && <div className="alert alert-error">{error}</div>}

        {/* Grid */}
        {!loading && !error && (
          filtered.length === 0
            ? <p style={{ color: 'var(--ink-muted)', textAlign: 'center', padding: '3rem' }}>
                No items in this category.
              </p>
            : <div className="menu-grid">
                {filtered.map(item => (
                  <MenuCard key={item.id} item={item} />
                ))}
              </div>
        )}
      </div>
    </div>
  )
}

function MenuCard({ item }) {
  return (
    <div className="menu-card">
      <div className="menu-card-header">
        <div>
          <span className={`badge badge-gold menu-category-badge`}>
            {item.category}
          </span>
        </div>
        <span className="menu-price">{formatPrice(item.price)}</span>
      </div>
      <h3 className="menu-name">{item.name}</h3>
      <p className="menu-desc">{item.description}</p>
    </div>
  )
}

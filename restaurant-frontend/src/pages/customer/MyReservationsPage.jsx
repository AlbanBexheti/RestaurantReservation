import { useLocation } from 'react-router-dom'
import { useState } from 'react'
import { reservationApi } from '../../api/client'
import { useFetch } from '../../hooks/useFetch'
import './MyReservationsPage.css'

function statusBadge(status) {
  const map = {
    Pending:   'badge-gold',
    Confirmed: 'badge-green',
    Cancelled: 'badge-red',
  }
  return `badge ${map[status] ?? 'badge-grey'}`
}

function formatDT(dt) {
  return new Date(dt).toLocaleString('en-GB', {
    weekday: 'short', day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

export default function MyReservationsPage() {
  const location = useLocation()
  const justBooked = location.state?.success

  const { data: reservations, loading, error, refetch } = useFetch(
    () => reservationApi.getMine()
  )

  const [cancelling, setCancelling] = useState(null)
  const [cancelError, setCancelError] = useState('')

  async function cancelReservation(id) {
    if (!window.confirm('Cancel this reservation?')) return
    setCancelling(id)
    setCancelError('')
    try {
      await reservationApi.delete(id)
      refetch()
    } catch (err) {
      setCancelError(err.message)
    } finally {
      setCancelling(null)
    }
  }

  return (
    <div className="page">
      <div className="container">
        <h2 className="section-title">My Reservations</h2>
        <p className="section-sub">Track and manage your bookings</p>

        {justBooked && (
          <div className="alert alert-success">
            ✓ Your reservation was confirmed! We'll see you soon.
          </div>
        )}
        {cancelError && <div className="alert alert-error">{cancelError}</div>}

        {loading && <div className="spinner" />}
        {error   && <div className="alert alert-error">{error}</div>}

        {!loading && !error && reservations?.length === 0 && (
          <div className="empty-state card">
            <div className="empty-icon">📅</div>
            <h3>No reservations yet</h3>
            <p>When you book a table, it'll appear here.</p>
            <a href="/reserve" className="btn btn-primary" style={{ marginTop: '1rem' }}>
              Make a Reservation
            </a>
          </div>
        )}

        {!loading && !error && reservations?.length > 0 && (
          <div className="my-res-list">
            {reservations.map(res => (
              <div key={res.id} className="my-res-card card">
                <div className="my-res-top">
                  <div>
                    <span className={statusBadge(res.status)}>{res.status}</span>
                    <h3 className="my-res-table">Table {res.tableNumber}</h3>
                    <p className="my-res-dt">{formatDT(res.reservationDateTime)}</p>
                  </div>
                  <div className="my-res-right">
                    <div className="my-res-party">
                      🪑 {res.partySize} {res.partySize === 1 ? 'guest' : 'guests'}
                    </div>
                    {res.status !== 'Cancelled' && (
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => cancelReservation(res.id)}
                        disabled={cancelling === res.id}
                      >
                        {cancelling === res.id ? 'Cancelling…' : 'Cancel'}
                      </button>
                    )}
                  </div>
                </div>
                {res.notes && (
                  <p className="my-res-notes">
                    <strong>Notes:</strong> {res.notes}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

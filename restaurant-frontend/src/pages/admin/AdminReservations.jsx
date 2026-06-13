import { useState } from 'react'
import { reservationApi } from '../../api/client'
import { useFetch } from '../../hooks/useFetch'

function statusBadge(status) {
  const map = { Pending: 'badge-gold', Confirmed: 'badge-green', Cancelled: 'badge-red' }
  return `badge ${map[status] ?? 'badge-grey'}`
}

function fmt(dt) {
  return new Date(dt).toLocaleString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

export default function AdminReservations() {
  const { data, loading, error, refetch } = useFetch(() => reservationApi.getAll())
  const [updating, setUpdating] = useState(null)
  const [actionError, setActionError] = useState('')

  async function updateStatus(id, status) {
    setUpdating(id + status)
    setActionError('')
    try {
      await reservationApi.updateStatus(id, status)
      refetch()
    } catch (err) {
      setActionError(err.message)
    } finally {
      setUpdating(null)
    }
  }

  return (
    <div className="page">
      <div className="container">
        <h2 className="section-title">All Reservations</h2>
        <p className="section-sub">Review and manage every booking</p>

        {actionError && <div className="alert alert-error">{actionError}</div>}
        {loading && <div className="spinner" />}
        {error   && <div className="alert alert-error">{error}</div>}

        {!loading && !error && (
          <div className="card" style={{ overflow: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Customer</th>
                  <th>Table</th>
                  <th>Date & Time</th>
                  <th>Party</th>
                  <th>Status</th>
                  <th>Notes</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data?.length === 0 && (
                  <tr><td colSpan={8} style={{ textAlign: 'center', padding: '2rem', color: 'var(--ink-muted)' }}>No reservations yet</td></tr>
                )}
                {data?.map(res => (
                  <tr key={res.id}>
                    <td style={{ color: 'var(--ink-muted)', fontSize: '0.8rem' }}>{res.id}</td>
                    <td><strong>{res.customerName}</strong></td>
                    <td>Table {res.tableNumber}</td>
                    <td style={{ whiteSpace: 'nowrap' }}>{fmt(res.reservationDateTime)}</td>
                    <td>{res.partySize}</td>
                    <td><span className={statusBadge(res.status)}>{res.status}</span></td>
                    <td style={{ maxWidth: 160, fontSize: '0.8rem', color: 'var(--ink-muted)' }}>
                      {res.notes || '—'}
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
                        {res.status !== 'Confirmed' && (
                          <button
                            className="btn btn-outline btn-sm"
                            disabled={!!updating}
                            onClick={() => updateStatus(res.id, 'Confirmed')}
                          >
                            {updating === res.id + 'Confirmed' ? '…' : 'Confirm'}
                          </button>
                        )}
                        {res.status !== 'Cancelled' && (
                          <button
                            className="btn btn-danger btn-sm"
                            disabled={!!updating}
                            onClick={() => updateStatus(res.id, 'Cancelled')}
                          >
                            {updating === res.id + 'Cancelled' ? '…' : 'Cancel'}
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}

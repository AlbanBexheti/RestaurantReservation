import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { reservationApi } from '../../api/client'
import { useAuth } from '../../context/AuthContext'
import './ReservePage.css'

// Format a local datetime string for the API (ISO 8601)
function toISOLocal(str) {
  // str is from datetime-local input: "2026-06-14T19:30"
  return new Date(str).toISOString()
}

// Minimum datetime for the input (now, rounded up to next 30 min)
function minDateTime() {
  const now = new Date()
  now.setMinutes(now.getMinutes() + 30)
  now.setSeconds(0, 0)
  const pad = n => String(n).padStart(2, '0')
  return `${now.getFullYear()}-${pad(now.getMonth()+1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`
}

export default function ReservePage() {
  const { isLoggedIn } = useAuth()
  const navigate       = useNavigate()

  // Step 1 form
  const [dateTime,   setDateTime]   = useState('')
  const [partySize,  setPartySize]  = useState(2)
  const [tables,     setTables]     = useState(null)
  const [step1Loading, setStep1Loading] = useState(false)
  const [step1Error,   setStep1Error]   = useState('')

  // Step 2
  const [selectedTable, setSelectedTable] = useState(null)
  const [notes,         setNotes]         = useState('')
  const [step2Loading, setStep2Loading]   = useState(false)
  const [step2Error,   setStep2Error]     = useState('')

  async function checkAvailability(e) {
    e.preventDefault()
    setStep1Error('')
    setTables(null)
    setSelectedTable(null)
    setStep1Loading(true)
    try {
      const result = await reservationApi.checkAvailability(toISOLocal(dateTime), partySize)
      setTables(result)
      if (result.length === 0) setStep1Error('No tables available for that time and party size. Try a different slot.')
    } catch (err) {
      setStep1Error(err.message)
    } finally {
      setStep1Loading(false)
    }
  }

  async function confirmBooking() {
    if (!isLoggedIn) {
      navigate('/login', { state: { from: { pathname: '/reserve' } } })
      return
    }
    setStep2Error('')
    setStep2Loading(true)
    try {
      await reservationApi.create({
        tableId: selectedTable.id,
        reservationDateTime: toISOLocal(dateTime),
        partySize: Number(partySize),
        notes: notes || null,
      })
      navigate('/my-reservations', { state: { success: true } })
    } catch (err) {
      setStep2Error(err.message)
    } finally {
      setStep2Loading(false)
    }
  }

  return (
    <div className="page">
      <div className="container reserve-container">
        <h2 className="section-title">Make a Reservation</h2>
        <p className="section-sub">Check availability and secure your table</p>

        {/* ── STEP 1: availability search ── */}
        <div className="card reserve-card">
          <h3 className="reserve-step-title">
            <span className="reserve-step-num">1</span> Choose Date & Party Size
          </h3>

          <form onSubmit={checkAvailability}>
            <div className="form-row">
              <div className="form-group">
                <label>Date & Time</label>
                <input
                  type="datetime-local"
                  value={dateTime}
                  min={minDateTime()}
                  onChange={e => setDateTime(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label>Party Size</label>
                <select
                  value={partySize}
                  onChange={e => setPartySize(e.target.value)}
                  required
                >
                  {Array.from({ length: 20 }, (_, i) => i + 1).map(n => (
                    <option key={n} value={n}>{n} {n === 1 ? 'guest' : 'guests'}</option>
                  ))}
                </select>
              </div>
            </div>

            {step1Error && <div className="alert alert-error">{step1Error}</div>}

            <button
              type="submit"
              className="btn btn-primary"
              disabled={step1Loading}
            >
              {step1Loading ? 'Checking…' : 'Check Availability'}
            </button>
          </form>
        </div>

        {/* ── STEP 2: pick a table ── */}
        {tables && tables.length > 0 && (
          <div className="card reserve-card">
            <h3 className="reserve-step-title">
              <span className="reserve-step-num">2</span> Select a Table
            </h3>

            <div className="tables-grid">
              {tables.map(table => (
                <button
                  key={table.id}
                  className={`table-option ${selectedTable?.id === table.id ? 'selected' : ''}`}
                  onClick={() => setSelectedTable(table)}
                >
                  <div className="table-num">Table {table.tableNumber}</div>
                  <div className="table-meta">
                    <span>🪑 Seats {table.capacity}</span>
                    <span>📍 {table.location}</span>
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* ── STEP 3: confirm ── */}
        {selectedTable && (
          <div className="card reserve-card">
            <h3 className="reserve-step-title">
              <span className="reserve-step-num">3</span> Confirm Booking
            </h3>

            <div className="booking-summary">
              <div className="booking-row">
                <span>Table</span>
                <strong>Table {selectedTable.tableNumber} ({selectedTable.location})</strong>
              </div>
              <div className="booking-row">
                <span>Date & Time</span>
                <strong>{new Date(dateTime).toLocaleString('en-GB', {
                  weekday: 'short', day: 'numeric', month: 'short',
                  hour: '2-digit', minute: '2-digit',
                })}</strong>
              </div>
              <div className="booking-row">
                <span>Party Size</span>
                <strong>{partySize} {partySize == 1 ? 'guest' : 'guests'}</strong>
              </div>
            </div>

            <div className="form-group" style={{ marginTop: '1rem' }}>
              <label>Special Requests (optional)</label>
              <textarea
                value={notes}
                onChange={e => setNotes(e.target.value)}
                placeholder="Dietary requirements, seating preferences…"
                rows={3}
              />
            </div>

            {step2Error && <div className="alert alert-error">{step2Error}</div>}

            {!isLoggedIn && (
              <div className="alert alert-info">
                You need to <strong>sign in</strong> to complete your booking.
              </div>
            )}

            <button
              className="btn btn-primary btn-lg"
              onClick={confirmBooking}
              disabled={step2Loading}
            >
              {step2Loading
                ? 'Booking…'
                : isLoggedIn
                  ? 'Confirm Reservation'
                  : 'Sign In to Book'}
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

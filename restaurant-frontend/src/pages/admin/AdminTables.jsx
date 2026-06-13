import { useState } from 'react'
import { tableApi } from '../../api/client'
import { useFetch } from '../../hooks/useFetch'
import Modal from '../../components/common/Modal'

const EMPTY_FORM = { tableNumber: '', capacity: '', location: 'Indoor' }

export default function AdminTables() {
  const { data: tables, loading, error, refetch } = useFetch(() => tableApi.getAll())
  const [modal,   setModal]   = useState(null)
  const [editing, setEditing] = useState(null)
  const [form,    setForm]    = useState(EMPTY_FORM)
  const [saving,  setSaving]  = useState(false)
  const [formErr, setFormErr] = useState('')
  const [deleting, setDeleting] = useState(null)

  function openCreate() {
    setForm(EMPTY_FORM); setEditing(null); setFormErr(''); setModal('form')
  }

  function openEdit(table) {
    setForm({
      tableNumber: table.tableNumber,
      capacity: table.capacity,
      location: table.location,
      isAvailable: table.isAvailable,
    })
    setEditing(table); setFormErr(''); setModal('form')
  }

  function field(key, val) { setForm(f => ({ ...f, [key]: val })) }

  async function save() {
    if (!form.tableNumber || !form.capacity || !form.location) {
      setFormErr('All fields are required.')
      return
    }
    setSaving(true); setFormErr('')
    try {
      if (editing) {
        await tableApi.update(editing.id, {
          capacity: parseInt(form.capacity),
          location: form.location,
          isAvailable: form.isAvailable ?? true,
        })
      } else {
        await tableApi.create({
          tableNumber: parseInt(form.tableNumber),
          capacity: parseInt(form.capacity),
          location: form.location,
        })
      }
      setModal(null); refetch()
    } catch (err) {
      setFormErr(err.message)
    } finally {
      setSaving(false)
    }
  }

  async function deleteTable(id) {
    if (!window.confirm('Delete this table?')) return
    setDeleting(id)
    try { await tableApi.delete(id); refetch() }
    catch (e) { alert(e.message) }
    finally { setDeleting(null) }
  }

  return (
    <div className="page">
      <div className="container">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: '1.5rem' }}>
          <div>
            <h2 className="section-title">Tables</h2>
            <p className="section-sub" style={{ marginBottom: 0 }}>Manage seating capacity and locations</p>
          </div>
          <button className="btn btn-primary" onClick={openCreate}>+ Add Table</button>
        </div>

        {loading && <div className="spinner" />}
        {error   && <div className="alert alert-error">{error}</div>}

        {!loading && !error && (
          <div className="grid-3">
            {tables?.map(table => (
              <div key={table.id} className="card">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <h3 style={{ fontFamily: 'var(--font-display)' }}>Table {table.tableNumber}</h3>
                  <span className={`badge ${table.isAvailable ? 'badge-green' : 'badge-red'}`}>
                    {table.isAvailable ? 'Available' : 'Unavailable'}
                  </span>
                </div>
                <p style={{ fontSize: '0.875rem', color: 'var(--ink-muted)', marginBottom: '0.3rem' }}>
                  🪑 Capacity: {table.capacity}
                </p>
                <p style={{ fontSize: '0.875rem', color: 'var(--ink-muted)', marginBottom: '1rem' }}>
                  📍 {table.location}
                </p>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button className="btn btn-ghost btn-sm" onClick={() => openEdit(table)}>Edit</button>
                  <button
                    className="btn btn-danger btn-sm"
                    disabled={deleting === table.id}
                    onClick={() => deleteTable(table.id)}
                  >
                    {deleting === table.id ? '…' : 'Delete'}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {modal === 'form' && (
        <Modal
          title={editing ? `Edit Table ${editing.tableNumber}` : 'Add Table'}
          onClose={() => setModal(null)}
        >
          {formErr && <div className="alert alert-error">{formErr}</div>}

          {!editing && (
            <div className="form-group">
              <label>Table Number</label>
              <input
                type="number" min="1"
                value={form.tableNumber}
                onChange={e => field('tableNumber', e.target.value)}
                placeholder="e.g. 6"
              />
            </div>
          )}
          <div className="form-row">
            <div className="form-group">
              <label>Capacity</label>
              <input
                type="number" min="1" max="20"
                value={form.capacity}
                onChange={e => field('capacity', e.target.value)}
                placeholder="4"
              />
            </div>
            <div className="form-group">
              <label>Location</label>
              <select value={form.location} onChange={e => field('location', e.target.value)}>
                <option>Indoor</option>
                <option>Outdoor</option>
              </select>
            </div>
          </div>
          {editing && (
            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={form.isAvailable ?? true}
                  onChange={e => field('isAvailable', e.target.checked)}
                  style={{ marginRight: '0.4rem' }}
                />
                Table is available for booking
              </label>
            </div>
          )}
          <div style={{ display: 'flex', gap: '0.6rem', marginTop: '0.5rem' }}>
            <button className="btn btn-primary" onClick={save} disabled={saving}>
              {saving ? 'Saving…' : editing ? 'Save Changes' : 'Add Table'}
            </button>
            <button className="btn btn-ghost" onClick={() => setModal(null)}>Cancel</button>
          </div>
        </Modal>
      )}
    </div>
  )
}

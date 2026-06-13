import { useState } from 'react'
import { menuApi } from '../../api/client'
import { useFetch } from '../../hooks/useFetch'
import Modal from '../../components/common/Modal'

const EMPTY_FORM = { name: '', description: '', price: '', category: 'Main', isAvailable: true }
const CATEGORIES = ['Starters', 'Main', 'Desserts', 'Drinks']

export default function AdminMenu() {
  const { data: items, loading, error, refetch } = useFetch(() => menuApi.getAll())
  const [modal,   setModal]   = useState(null) // null | 'create' | 'edit'
  const [editing, setEditing] = useState(null)
  const [form,    setForm]    = useState(EMPTY_FORM)
  const [saving,  setSaving]  = useState(false)
  const [formErr, setFormErr] = useState('')
  const [deleting, setDeleting] = useState(null)

  function openCreate() {
    setForm(EMPTY_FORM)
    setEditing(null)
    setFormErr('')
    setModal('form')
  }

  function openEdit(item) {
    setForm({
      name: item.name, description: item.description,
      price: item.price, category: item.category,
      isAvailable: item.isAvailable,
    })
    setEditing(item)
    setFormErr('')
    setModal('form')
  }

  function field(key, val) {
    setForm(f => ({ ...f, [key]: val }))
  }

  async function save() {
    if (!form.name || !form.description || !form.price || !form.category) {
      setFormErr('All fields are required.')
      return
    }
    setSaving(true)
    setFormErr('')
    try {
      const dto = { ...form, price: parseFloat(form.price) }
      if (editing) {
        await menuApi.update(editing.id, dto)
      } else {
        await menuApi.create(dto)
      }
      setModal(null)
      refetch()
    } catch (err) {
      setFormErr(err.message)
    } finally {
      setSaving(false)
    }
  }

  async function deleteItem(id) {
    if (!window.confirm('Delete this menu item?')) return
    setDeleting(id)
    try { await menuApi.delete(id); refetch() }
    catch (e) { alert(e.message) }
    finally { setDeleting(null) }
  }

  return (
    <div className="page">
      <div className="container">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: '1.5rem' }}>
          <div>
            <h2 className="section-title">Menu Items</h2>
            <p className="section-sub" style={{ marginBottom: 0 }}>Manage your restaurant's menu</p>
          </div>
          <button className="btn btn-primary" onClick={openCreate}>+ Add Item</button>
        </div>

        {loading && <div className="spinner" />}
        {error   && <div className="alert alert-error">{error}</div>}

        {!loading && !error && (
          <div className="card" style={{ overflow: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th><th>Category</th><th>Price</th>
                  <th>Description</th><th>Available</th><th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items?.map(item => (
                  <tr key={item.id}>
                    <td><strong>{item.name}</strong></td>
                    <td><span className="badge badge-gold" style={{ fontSize: '0.65rem' }}>{item.category}</span></td>
                    <td style={{ fontWeight: 600, color: 'var(--gold)' }}>${Number(item.price).toFixed(2)}</td>
                    <td style={{ fontSize: '0.82rem', color: 'var(--ink-muted)', maxWidth: 220 }}>{item.description}</td>
                    <td>
                      <span className={`badge ${item.isAvailable ? 'badge-green' : 'badge-red'}`}>
                        {item.isAvailable ? 'Yes' : 'No'}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.4rem' }}>
                        <button className="btn btn-ghost btn-sm" onClick={() => openEdit(item)}>Edit</button>
                        <button
                          className="btn btn-danger btn-sm"
                          disabled={deleting === item.id}
                          onClick={() => deleteItem(item.id)}
                        >
                          {deleting === item.id ? '…' : 'Delete'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Form Modal */}
      {modal === 'form' && (
        <Modal
          title={editing ? 'Edit Menu Item' : 'Add Menu Item'}
          onClose={() => setModal(null)}
        >
          {formErr && <div className="alert alert-error">{formErr}</div>}

          <div className="form-group">
            <label>Name</label>
            <input value={form.name} onChange={e => field('name', e.target.value)} placeholder="e.g. Grilled Salmon" />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea rows={2} value={form.description} onChange={e => field('description', e.target.value)} placeholder="Short description" />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Price ($)</label>
              <input type="number" step="0.01" min="0.01" value={form.price} onChange={e => field('price', e.target.value)} placeholder="12.99" />
            </div>
            <div className="form-group">
              <label>Category</label>
              <select value={form.category} onChange={e => field('category', e.target.value)}>
                {CATEGORIES.map(c => <option key={c}>{c}</option>)}
              </select>
            </div>
          </div>
          {editing && (
            <div className="form-group">
              <label>
                <input type="checkbox" checked={form.isAvailable} onChange={e => field('isAvailable', e.target.checked)} style={{ marginRight: '0.4rem' }} />
                Available on menu
              </label>
            </div>
          )}
          <div style={{ display: 'flex', gap: '0.6rem', marginTop: '0.5rem' }}>
            <button className="btn btn-primary" onClick={save} disabled={saving}>
              {saving ? 'Saving…' : editing ? 'Save Changes' : 'Add Item'}
            </button>
            <button className="btn btn-ghost" onClick={() => setModal(null)}>Cancel</button>
          </div>
        </Modal>
      )}
    </div>
  )
}

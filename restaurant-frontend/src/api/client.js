// ── API CLIENT ────────────────────────────────────────────────────────────────
// Central place for all HTTP calls to the .NET backend.
// The Vite proxy in vite.config.js forwards /api/* to http://localhost:5000
// so we just write '/api/...' here — no hardcoded port.

const BASE = '/api'

// Read the JWT from localStorage (stored on login/register)
function getToken() {
  return localStorage.getItem('token')
}

// Build headers: always JSON, attach Bearer token if present
function headers(extraHeaders = {}) {
  const h = { 'Content-Type': 'application/json', ...extraHeaders }
  const token = getToken()
  if (token) h['Authorization'] = `Bearer ${token}`
  return h
}

// Generic request helper — throws on non-2xx
async function request(method, path, body = null) {
  const options = {
    method,
    headers: headers(),
  }
  if (body !== null) {
    options.body = JSON.stringify(body)
  }

  const res = await fetch(`${BASE}${path}`, options)

  // 204 No Content — successful but empty
  if (res.status === 204) return null

  const data = await res.json().catch(() => null)

  if (!res.ok) {
    // Pull out the most useful error message from the response
    const message =
      typeof data === 'string'
        ? data
        : data?.title || data?.message || `Request failed (${res.status})`
    throw new Error(message)
  }

  return data
}

// ── AUTH ──────────────────────────────────────────────────────────────────────
export const authApi = {
  // POST /api/auth/login  →  AuthResponseDto
  login: (dto) => request('POST', '/auth/login', dto),

  // POST /api/auth/register  →  AuthResponseDto
  register: (dto) => request('POST', '/auth/register', dto),
}

// ── MENU ──────────────────────────────────────────────────────────────────────
export const menuApi = {
  // GET /api/menu  →  MenuItemDto[]  (available items, no auth needed)
  getAvailable: () => request('GET', '/menu'),

  // GET /api/menu/all  →  MenuItemDto[]  (admin only)
  getAll: () => request('GET', '/menu/all'),

  // GET /api/menu/category/{category}
  getByCategory: (cat) => request('GET', `/menu/category/${encodeURIComponent(cat)}`),

  // GET /api/menu/{id}
  getById: (id) => request('GET', `/menu/${id}`),

  // POST /api/menu  (admin)
  create: (dto) => request('POST', '/menu', dto),

  // PUT /api/menu/{id}  (admin)
  update: (id, dto) => request('PUT', `/menu/${id}`, dto),

  // DELETE /api/menu/{id}  (admin)
  delete: (id) => request('DELETE', `/menu/${id}`),
}

// ── TABLES ────────────────────────────────────────────────────────────────────
export const tableApi = {
  // GET /api/table  →  TableDto[]
  getAll: () => request('GET', '/table'),

  // GET /api/table/{id}
  getById: (id) => request('GET', `/table/${id}`),

  // POST /api/table  (admin)
  create: (dto) => request('POST', '/table', dto),

  // PUT /api/table/{id}  (admin)
  update: (id, dto) => request('PUT', `/table/${id}`, dto),

  // DELETE /api/table/{id}  (admin)
  delete: (id) => request('DELETE', `/table/${id}`),
}

// ── RESERVATIONS ──────────────────────────────────────────────────────────────
export const reservationApi = {
  // GET /api/reservation  →  ReservationDto[]  (admin only)
  getAll: () => request('GET', '/reservation'),

  // GET /api/reservation/my  →  ReservationDto[]  (logged-in user)
  getMine: () => request('GET', '/reservation/my'),

  // GET /api/reservation/{id}
  getById: (id) => request('GET', `/reservation/${id}`),

  // GET /api/reservation/availability?reservationDateTime=...&partySize=...
  checkAvailability: (dateTime, partySize) => {
    const params = new URLSearchParams({
      reservationDateTime: dateTime,
      partySize: String(partySize),
    })
    return request('GET', `/reservation/availability?${params}`)
  },

  // POST /api/reservation  →  ReservationDto
  create: (dto) => request('POST', '/reservation', dto),

  // PUT /api/reservation/{id}/status  (admin)
  updateStatus: (id, status) =>
    request('PUT', `/reservation/${id}/status`, { status }),

  // DELETE /api/reservation/{id}
  delete: (id) => request('DELETE', `/reservation/${id}`),
}

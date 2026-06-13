# Restaurant Reservation — React Frontend

A React + Vite frontend for the `RestaurantReservation.API` .NET backend.

---

## Project Structure

```
src/
├── api/
│   └── client.js             # All fetch calls to the backend (auth, menu, tables, reservations)
├── components/
│   ├── common/
│   │   ├── Modal.jsx          # Reusable modal dialog
│   │   └── ProtectedRoute.jsx # Route guards (login required / admin required)
│   └── layout/
│       ├── Navbar.jsx         # Top navigation bar
│       └── Navbar.css
├── context/
│   └── AuthContext.jsx        # Global auth state (login, register, logout, JWT storage)
├── hooks/
│   └── useFetch.js            # Generic data-fetching hook
├── pages/
│   ├── HomePage.jsx           # Landing page
│   ├── MenuPage.jsx           # Public menu with category filter
│   ├── auth/
│   │   ├── LoginPage.jsx
│   │   └── RegisterPage.jsx
│   ├── customer/
│   │   ├── ReservePage.jsx        # 3-step reservation flow
│   │   └── MyReservationsPage.jsx # View + cancel own bookings
│   └── admin/
│       ├── AdminDashboard.jsx     # Stats overview
│       ├── AdminReservations.jsx  # Confirm / cancel all bookings
│       ├── AdminMenu.jsx          # Full CRUD for menu items
│       └── AdminTables.jsx        # Full CRUD for tables
├── App.jsx   # React Router route definitions
├── main.jsx  # Entry point
└── index.css # Design system (CSS variables, buttons, forms, cards, etc.)
```

---

## Prerequisites

- Node.js 18+ and npm
- The .NET backend running (`dotnet run` inside `backend/RestaurantReservation.API/`)

---

## Setup

### 1. Place the frontend in your project

Put the `restaurant-frontend/` folder at the root of your repo, next to `backend/` and `tests/`:

```
RestaurantReservation/
├── backend/
├── tests/
└── restaurant-frontend/   ← here
```

### 2. Install dependencies

```bash
cd restaurant-frontend
npm install
```

### 3. Start the backend first

```bash
cd ../backend/RestaurantReservation.API
dotnet run
```

The API will start on `http://localhost:5000`.

### 4. Start the frontend

```bash
cd restaurant-frontend
npm run dev
```

Open **http://localhost:5173** in your browser.

---

## How the Frontend Talks to the Backend

The Vite dev server proxies all `/api/*` requests to `http://localhost:5000`:

```js
// vite.config.js
proxy: {
  '/api': { target: 'http://localhost:5000', changeOrigin: true }
}
```

This means:
- No CORS issues during development
- The backend's existing `AllowFrontend` CORS policy covers this
- All API calls in `src/api/client.js` use relative paths (`/api/auth/login`, etc.)

---

## Authentication Flow

1. User logs in → backend returns `{ token, fullName, email, role }`
2. Token + user info stored in `localStorage`
3. Every subsequent API call sends `Authorization: Bearer <token>`
4. On logout, `localStorage` is cleared

---

## Routes

| Path | Access | Description |
|---|---|---|
| `/` | Public | Landing page |
| `/menu` | Public | Menu with category tabs |
| `/reserve` | Public (login to confirm) | Check availability + book |
| `/login` | Public | Sign in |
| `/register` | Public | Create account |
| `/my-reservations` | Logged in | View + cancel own bookings |
| `/admin` | Admin only | Dashboard with stats |
| `/admin/reservations` | Admin only | All reservations + status updates |
| `/admin/menu` | Admin only | Add / edit / delete menu items |
| `/admin/tables` | Admin only | Add / edit / delete tables |

---

## Default Admin Account

Seeded automatically on first run by `Program.cs`:

- **Email:** `admin@restaurant.com`
- **Password:** `Admin123!`

---

## Backend Port Mismatch?

If your backend runs on a different port than `5000`, update the proxy target in `vite.config.js`:

```js
proxy: {
  '/api': {
    target: 'http://localhost:YOUR_PORT',
    changeOrigin: true,
  },
},
```

You can find your backend's port in:
`backend/RestaurantReservation.API/Properties/launchSettings.json`

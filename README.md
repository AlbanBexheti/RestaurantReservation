# Restaurant Reservation System

A Service-Oriented Architecture (SOA) restaurant reservation platform built with **ASP.NET Core 9**, **SQLite**, **JWT authentication**, and a **React + Vite** frontend. Includes a full unit testing suite (xUnit + NSubstitute) and a k6 performance testing suite.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [How It Works](#how-it-works)
3. [Project Structure](#project-structure)
4. [Prerequisites](#prerequisites)
5. [Cloning the Repository](#cloning-the-repository)
6. [Running the Backend](#running-the-backend)
7. [Running the Frontend](#running-the-frontend)
8. [Default Accounts & Seed Data](#default-accounts--seed-data)
9. [Running the Unit Tests](#running-the-unit-tests)
10. [Running the k6 Performance Tests](#running-the-k6-performance-tests)
11. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

The system is split into independent services, each responsible for one business domain:

| Service | Responsibility |
|---|---|
| **Auth Service** | Registration, login, JWT token issuance |
| **Table Service** | CRUD for physical restaurant tables |
| **Menu Service** | CRUD for menu items, category filtering |
| **Reservation Service** | Availability checking, booking creation, conflict detection |

Each service follows the same layered pattern:

```
Controller  →  Service  →  Repository  →  Database (SQLite)
   (HTTP)      (business        (data
                 logic)          access)
```

- **Controllers** handle HTTP routing only — no business logic.
- **Services** contain the business rules (e.g. "is this table available for this time slot?").
- **Repositories** are the only layer that talks to the database via EF Core.
- **DTOs** (Data Transfer Objects) control exactly what data enters and leaves the API — internal fields like `PasswordHash` are never exposed.
- **Interfaces** (`IReservationService`, `IReservationRepository`, etc.) decouple layers from each other, which is what makes unit testing with mocks possible.

Authentication is **stateless** via JWT: the server signs a token containing the user's ID, email, and role. No session is stored server-side — every request carries its own proof of identity in the `Authorization: Bearer <token>` header.

---

## How It Works

### Authentication Flow
1. User registers or logs in via `POST /api/auth/register` or `/api/auth/login`.
2. Password is verified against a BCrypt hash (passwords are never stored in plain text).
3. On success, the API returns a JWT containing the user's claims.
4. The frontend stores this token in `localStorage` and attaches it to every subsequent request.
5. `[Authorize]` attributes on controllers automatically reject requests with missing/invalid/expired tokens — no manual checks needed in each endpoint.

### Reservation Flow
1. **Check availability** — `GET /api/reservation/availability?reservationDateTime=...&partySize=...` (public, no login required). Returns tables that are marked available, have sufficient capacity, and have no conflicting reservation within a ±2 hour window of the requested time.
2. **Create reservation** — `POST /api/reservation` (requires login). The service re-validates all the same rules server-side (never trust the client) and returns `409 Conflict` if the table became unavailable between the check and the booking attempt.
3. **Manage bookings** — Customers can view (`GET /api/reservation/my`) and cancel (`DELETE /api/reservation/{id}`) their own reservations. Admins can view all reservations and confirm/cancel any of them (`PUT /api/reservation/{id}/status`).

### Why SQLite Has a Concurrency Limitation
SQLite serialises all write transactions at the file level. Under low-to-moderate concurrent load this is invisible, but under heavy concurrent reservation writes, requests start queuing for the write lock — this is intentionally exposed by the stress test (see [k6 Performance Tests](#running-the-k6-performance-tests)) and is a known, documented production consideration (migrate to PostgreSQL for higher write concurrency).

---

## Project Structure

```
RestaurantReservation/
├── backend/
│   └── RestaurantReservation.API/
│       ├── Controllers/        # HTTP endpoints
│       ├── Services/           # Business logic
│       ├── Repositories/       # EF Core data access
│       ├── Models/             # Database entities
│       ├── DTOs/                # Request/response shapes
│       ├── Interfaces/          # Contracts for DI & mocking
│       ├── Middleware/          # Global exception handling
│       ├── Data/                # AppDbContext, EF config
│       ├── appsettings.json     # Connection string, JWT secret
│       └── Program.cs           # App startup & service wiring
├── tests/
│   └── RestaurantReservation.Tests/   # xUnit + NSubstitute unit tests
├── restaurant-frontend/         # React + Vite SPA
└── tests/performance/           # k6 load testing scripts (see below)
```

---

## Prerequisites

Install these before cloning:

| Tool | Version | Check with |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0+ | `dotnet --version` |
| [Node.js](https://nodejs.org/) | 18+ | `node --version` |
| [npm](https://www.npmjs.com/) | comes with Node | `npm --version` |
| [k6](https://k6.io/docs/get-started/installation/) | latest | `k6 version` |
| [Git](https://git-scm.com/) | any recent | `git --version` |

### Installing k6 (for performance testing)

**Windows:**
```bash
winget install k6 --source winget
```

**macOS:**
```bash
brew install k6
```

**Linux (Fedora/RHEL):**
```bash
sudo dnf install https://dl.k6.io/rpm/repo.rpm
sudo dnf install k6
```

**Linux (Debian/Ubuntu):**
```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

---

## Cloning the Repository

```bash
git clone https://github.com/AlbanBexheti/RestaurantReservation.git
cd RestaurantReservation
```

---

## Running the Backend

```bash
cd backend/RestaurantReservation.API
dotnet restore
dotnet build
dotnet run
```

By default the API starts on **`http://localhost:5000`** (confirm the exact port in `Properties/launchSettings.json` — if it differs, update the frontend's `vite.config.js` proxy target and the `BASE` URL in the k6 scripts accordingly).

On first run, the app automatically seeds:
- 1 admin user
- 5 tables (mixed indoor/outdoor, capacities 2–8)
- 5 menu items (Starters, Main, Desserts, Drinks)

You can verify the API is running by opening the Scalar API reference UI (shown in the console output, typically at `http://localhost:5000/scalar/v1`).

---

## Running the Frontend

In a **separate terminal**, leave the backend running and:

```bash
cd restaurant-frontend
npm install
npm run dev
```

Open **`http://localhost:5173`** in your browser. The Vite dev server proxies all `/api/*` requests to the backend automatically — no CORS configuration needed on your end.

---

## Default Accounts & Seed Data

| Role | Email | Password |
|---|---|---|
| Admin | `admin@restaurant.com` | `Admin123!` |

Regular customer accounts can be created via the Register page in the frontend, or directly via:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Test User","email":"test@example.com","password":"Test1234"}'
```

---

## Running the Unit Tests

```bash
cd tests/RestaurantReservation.Tests
dotnet test
```

This runs the xUnit suite covering the repository, service, and controller layers for the Reservation domain, using NSubstitute to mock dependencies (no real database is touched in the service-layer tests).

---

## Running the k6 Performance Tests

> **The backend must be running (`dotnet run`) before executing any of these.**

### 1. Place the test scripts

Create the folder if it doesn't already exist and add the four scripts below.

```bash
mkdir -p tests/performance
```

**`tests/performance/01_baseline.js`** — single user, sanity check
```javascript
import http from 'k6/http'
import { check, sleep } from 'k6'

const BASE = 'http://localhost:5000/api'

export const options = {
  vus: 1,
  duration: '30s',
}

export default function () {
  const menu = http.get(`${BASE}/menu`)
  check(menu, {
    'menu status 200': r => r.status === 200,
    'menu < 200ms':    r => r.timings.duration < 200,
  })

  const tables = http.get(`${BASE}/table`)
  check(tables, {
    'tables status 200': r => r.status === 200,
    'tables < 200ms':    r => r.timings.duration < 200,
  })

  sleep(1)
}
```

**`tests/performance/02_auth_load.js`** — login under moderate concurrency
```javascript
import http from 'k6/http'
import { check, sleep } from 'k6'

const BASE = 'http://localhost:5000/api'

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m',  target: 10 },
    { duration: '20s', target: 0  },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed:   ['rate<0.01'],
  },
}

export default function () {
  const payload = JSON.stringify({
    email: 'admin@restaurant.com',
    password: 'Admin123!',
  })
  const params = { headers: { 'Content-Type': 'application/json' } }

  const res = http.post(`${BASE}/auth/login`, payload, params)

  check(res, {
    'login status 200': r => r.status === 200,
    'token present':    r => JSON.parse(r.body).token !== undefined,
  })

  sleep(1)
}
```

**`tests/performance/03_user_journey.js`** — realistic end-to-end flow
```javascript
import http from 'k6/http'
import { check, sleep, group } from 'k6'

const BASE = 'http://localhost:5000/api'

export const options = {
  stages: [
    { duration: '1m',  target: 20 },
    { duration: '3m',  target: 20 },
    { duration: '30s', target: 0  },
  ],
  thresholds: {
    'http_req_duration':                    ['p(95)<600'],
    'http_req_duration{name:login}':        ['p(95)<400'],
    'http_req_duration{name:menu}':         ['p(95)<300'],
    'http_req_duration{name:availability}': ['p(95)<500'],
    'http_req_failed':                      ['rate<0.02'],
  },
}

export default function () {
  const headers = { 'Content-Type': 'application/json' }
  let token = ''

  group('1. Login', () => {
    const res = http.post(
      `${BASE}/auth/login`,
      JSON.stringify({ email: 'admin@restaurant.com', password: 'Admin123!' }),
      { headers, tags: { name: 'login' } }
    )
    check(res, { 'logged in': r => r.status === 200 })
    token = res.json('token') ?? ''
  })

  sleep(1)

  group('2. Browse Menu', () => {
    const res = http.get(`${BASE}/menu`, { tags: { name: 'menu' } })
    check(res, { 'menu ok': r => r.status === 200 })
  })

  sleep(1)

  group('3. Check Availability', () => {
    const dt = new Date()
    dt.setDate(dt.getDate() + 1)
    dt.setHours(19, 0, 0, 0)

    const params = new URLSearchParams({
      reservationDateTime: dt.toISOString(),
      partySize: '2',
    })

    const res = http.get(
      `${BASE}/reservation/availability?${params}`,
      { tags: { name: 'availability' } }
    )
    check(res, { 'availability ok': r => r.status === 200 })
  })

  sleep(1)

  group('4. Create Reservation', () => {
    const dt = new Date()
    dt.setDate(dt.getDate() + 1)
    dt.setHours(19, 0, 0, 0)

    const res = http.post(
      `${BASE}/reservation`,
      JSON.stringify({ tableId: 1, reservationDateTime: dt.toISOString(), partySize: 2 }),
      { headers: { ...headers, Authorization: `Bearer ${token}` }, tags: { name: 'create_reservation' } }
    )
    // 201 = booked, 409 = another VU took the table first — both are valid outcomes
    check(res, { 'reservation attempted': r => r.status === 201 || r.status === 409 })
  })

  sleep(2)
}
```

**`tests/performance/04_stress.js`** — find the breaking point
```javascript
import http from 'k6/http'
import { check } from 'k6'

const BASE = 'http://localhost:5000/api'

export const options = {
  stages: [
    { duration: '2m', target: 50  },
    { duration: '2m', target: 100 },
    { duration: '2m', target: 150 },
    { duration: '2m', target: 0   },
  ],
  thresholds: {
    http_req_duration: ['p(99)<2000'],
    http_req_failed:   ['rate<0.10'],
  },
}

export default function () {
  const res = http.get(`${BASE}/menu`)
  check(res, { 'status not 500': r => r.status !== 500 })
}
```

> **Note:** If your backend runs on a port other than `5000`, update the `BASE` constant at the top of each script.

### 2. Run each test

```bash
k6 run tests/performance/01_baseline.js
k6 run tests/performance/02_auth_load.js
k6 run tests/performance/03_user_journey.js
k6 run tests/performance/04_stress.js
```

### 3. (Optional) Export results for reporting

```bash
mkdir -p results
k6 run --out json=results/baseline.json tests/performance/01_baseline.js
k6 run --out json=results/journey.json  tests/performance/03_user_journey.js
k6 run --out json=results/stress.json   tests/performance/04_stress.js
```

### 4. Reading the Output

k6 prints a summary at the end of every run. The key lines to look at:

```
http_req_duration..............: avg=45ms  min=12ms  med=38ms  max=210ms  p(90)=78ms  p(95)=95ms
http_req_failed.................: 0.00%  ✓ 0      ✗ 1200
http_reqs........................: 1200   39.8/s
```

- **`p(95)`** — 95% of requests finished within this time. This is the number to compare against your thresholds, not the average.
- **`http_req_failed`** — percentage of requests that errored. Should stay near 0% except in the stress test.
- **`http_reqs`** and the requests/second figure — your throughput.

If a threshold fails, k6 exits with a non-zero status code and marks the relevant metric with a red `✗` in the summary — this is what makes these scripts usable in a CI/CD pipeline as automated performance gates.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `dotnet run` fails with a port conflict | Another process is using the port. Check `Properties/launchSettings.json` and either free the port or change it. |
| Frontend shows CORS errors | Confirm the backend's `Program.cs` CORS policy includes your frontend's exact origin (`http://localhost:5173`), and that you're running `npm run dev` (not a production build) so the Vite proxy is active. |
| k6 returns `connection refused` | The backend isn't running, or the `BASE` URL/port in the script doesn't match `dotnet run`'s actual port. |
| All k6 requests return `401` | Expected for protected endpoints if you haven't logged in within the script — check that the `Authorization: Bearer <token>` header is being attached correctly in `03_user_journey.js`. |
| Login fails with `401` in k6 but works in browser | Double-check the seeded admin password is exactly `Admin123!` and that the database wasn't deleted/reseeded with different data. |
| SQLite file locked errors under stress test | Expected behavior — this is the architectural finding the stress test is designed to surface. See [How It Works](#how-it-works). |

import http from 'k6/http'
import { check, sleep, group } from 'k6'

const BASE = 'http://localhost:5153/api'

export const options = {
    stages: [
        { duration: '1m',  target: 20 },  // ramp to 20 concurrent users
        { duration: '3m',  target: 20 },  // hold
        { duration: '30s', target: 0  },  // wind down
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
        check(res, {
            'menu ok':    r => r.status === 200,
            'has items':  r => r.json().length > 0,
        })
    })

    sleep(1)

    let tableId = 1  

    group('3. Check Table Availability', () => {
        const dt = new Date()
        dt.setDate(dt.getDate() + 1)
        dt.setHours(19, 0, 0, 0)

        const url = `${BASE}/reservation/availability?reservationDateTime=${encodeURIComponent(dt.toISOString())}&partySize=2`

        const res = http.get(
            url,
            { tags: { name: 'availability' } }
        )
        check(res, { 'availability ok': r => r.status === 200 })

        const availabilityRes = http.get(`${BASE}/reservation/availability?reservationDateTime=${encodeURIComponent(dt.toISOString())}&partySize=2`)
        if (availabilityRes.status === 200 && availabilityRes.json().length > 0) {
            tableId = availabilityRes.json()[0].tableId || availabilityRes.json()[0].id
        }
    })

    sleep(1)

    group('4. Create Reservation', () => {
        const dt = new Date()
        dt.setDate(dt.getDate() + 1)
        dt.setHours(19, 0, 0, 0)

        const res = http.post(
            `${BASE}/reservation`,
            JSON.stringify({ tableId: tableId, reservationDateTime: dt.toISOString(), partySize: 2 }),
            { headers: { ...headers, Authorization: `Bearer ${token}` }, tags: { name: 'create_reservation' } }
        )
        // 201 Created or 409 Conflict (table taken by another virtual user — both are valid)
        check(res, { 'reservation attempted': r => r.status === 201 || r.status === 409 })
    })

    sleep(2)
}
import http from 'k6/http'
import { check, sleep } from 'k6'

const BASE = 'http://localhost:5153/api'

export const options = {
    stages: [
        { duration: '30s', target: 10 },  // ramp up to 10 users
        { duration: '1m',  target: 10 },  // hold at 10
        { duration: '20s', target: 0  },  // ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],  // 95% of requests must finish < 500ms
        http_req_failed:   ['rate<0.01'],  // error rate must be below 1%
    },
}

export default function () {
    const loginPayload = JSON.stringify({
        email:    'admin@restaurant.com',
        password: 'Admin123!',
    })

    const params = { headers: { 'Content-Type': 'application/json' } }

    const res = http.post(`${BASE}/auth/login`, loginPayload, params)

    check(res, {
        'login status 200':       r => r.status === 200,
        'token present':          r => JSON.parse(r.body).token !== undefined,
        'login under 500ms':      r => r.timings.duration < 500,
    })

    sleep(1)
}
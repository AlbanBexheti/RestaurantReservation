import http from 'k6/http'
import { check, sleep } from 'k6'

const BASE = 'http://localhost:5153/api'

export const options = {
    vus: 1,           // 1 virtual user
    duration: '30s',  // for 30 seconds
}

export default function () {
    // Public endpoints — no auth needed
    const menu = http.get(`${BASE}/menu`)
    check(menu, {
        'menu status 200':        r => r.status === 200,
        'menu responds < 200ms':  r => r.timings.duration < 200,
    })

    const tables = http.get(`${BASE}/table`)
    check(tables, {
        'tables status 200':       r => r.status === 200,
        'tables responds < 200ms': r => r.timings.duration < 200,
    })

    sleep(1)
}
//npm run dev   
import http from 'k6/http'
import { check } from 'k6'

const BASE = 'http://localhost:5153/api'

export const options = {
    stages: [
        { duration: '2m', target: 50  },
        { duration: '2m', target: 100 },
        { duration: '2m', target: 100 },
        { duration: '2m', target: 0   },
    ],
    thresholds: {
        http_req_duration: ['p(99)<2000'],  // 99% under 2s (lenient — this is a stress test)
        http_req_failed:   ['rate<0.10'],   // tolerate up to 10% errors under extreme load
    },
}

export default function () {
    const res = http.get(`${BASE}/menu`)
    check(res, { 'status not 500': r => r.status !== 500 })
}
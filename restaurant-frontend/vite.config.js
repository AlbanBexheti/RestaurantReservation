import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The proxy means all fetch('/api/...') calls from the React dev server
// get forwarded to the .NET backend automatically — no CORS issues.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5153',
        changeOrigin: true,
      },
    },
  },
})

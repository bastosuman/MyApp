import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://localhost:7059',
        changeOrigin: true,
        secure: false, // For self-signed certificates
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    }
  }
})


import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/auth': 'http://localhost:5000',
      '/users': 'http://localhost:5000',
      '/products': 'http://localhost:5000',
      '/inventory': 'http://localhost:5000',
      '/sales': 'http://localhost:5000',
      '/customers': 'http://localhost:5000',
      '/reports': 'http://localhost:5000'
    }
  }
})

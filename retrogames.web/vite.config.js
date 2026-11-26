import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Repassa para a API tudo o que começa com /api: para o navegador,
    // front-end e API têm a mesma origem (sem CORS, e o cookie vai junto).
    proxy: { '/api': 'http://localhost:5080' },
  },
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Em desenvolvimento, chamadas para /api são encaminhadas ao back-end .NET.
// Ajuste a porta em VITE_PROXY_TARGET (arquivo .env) se o back-end usar outra.
export default defineConfig(({ mode }) => {
  const target = process.env.VITE_PROXY_TARGET || 'http://localhost:5000'
  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: { '/api': { target, changeOrigin: true } },
    },
  }
})

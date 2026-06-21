import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // host: '0.0.0.0' omogucava pristup sa mobilnih uredjaja na lokalnoj mrezi
    // Korisnik moze pristupiti aplikaciji sa telefona na http://<IP-masine>:5173
    host: '0.0.0.0',
    port: 5173,
  },
})
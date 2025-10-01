import { contextBridge } from 'electron'

contextBridge.exposeInMainWorld('api', {
  backendUrl: process.env.BACKEND_URL ?? 'http://localhost:5299'
})

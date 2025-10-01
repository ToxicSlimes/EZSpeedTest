import { app, BrowserWindow } from 'electron'
import path from 'node:path'

const BACKEND_URL = process.env.BACKEND_URL ?? 'http://localhost:5299';

async function createWindow() {
  const win = new BrowserWindow({
    width: 1100, height: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,         // SECURITY: must be true
      nodeIntegration: false,         // SECURITY: must be false
      sandbox: true                   // extra isolation
    }
  });

  // Load built renderer HTML from Vite output
  await win.loadFile(path.join(__dirname, 'renderer', 'index.html'));
  // пример запроса к API из main (для пров. связи):
  win.webContents.on('did-finish-load', () => {
    win.webContents.executeJavaScript(`
      fetch('${BACKEND_URL}/healthz').then(r=>r.json()).then(console.log).catch(console.error);
    `);
  });
}

app.whenReady().then(createWindow);
app.on('window-all-closed', () => { if (process.platform !== 'darwin') app.quit(); });

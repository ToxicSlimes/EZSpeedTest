import { app, BrowserWindow } from 'electron'
import path from 'node:path'
import { spawn } from 'child_process'

const isDev = process.env.NODE_ENV === 'development' || process.env.NODE_ENV === 'Development'
const BACKEND_URL = process.env.BACKEND_URL ?? 'http://localhost:5299'
let apiProcess: any = null

async function startApiServer() {
  if (isDev) {
    console.log('[MAIN] Development mode - starting integrated API server')
    
    // Check if we should start API as child process
    const shouldStartApi = process.env.START_API !== 'false'
    if (!shouldStartApi) {
      console.log('[MAIN] Using external API server (START_API=false)')
      return
    }

    console.log('[MAIN] Starting .NET API as child process...')
    
    // Find the API project path
    const apiPath = path.join(__dirname, '..', '..', '..', 'src', 'EZSpeedTest.Api')
    const apiExe = path.join(apiPath, 'bin', 'Debug', 'net9.0', 'EZSpeedTest.Api.exe')
    
    // Always try dotnet run first in development
    console.log('[MAIN] Using dotnet run for development...')
    
    apiProcess = spawn('dotnet', ['run', '--project', apiPath, '--urls', 'http://localhost:5299'], {
      stdio: ['ignore', 'pipe', 'pipe'],
      cwd: path.join(__dirname, '..', '..', '..'),
      env: { 
        ...process.env, 
        ASPNETCORE_ENVIRONMENT: 'Development'
      }
    })

    apiProcess.stdout?.on('data', (data: Buffer) => {
      console.log(`[API] ${data.toString().trim()}`)
    })

    apiProcess.stderr?.on('data', (data: Buffer) => {
      console.error(`[API ERROR] ${data.toString().trim()}`)
    })

    apiProcess.on('close', (code: number) => {
      console.log(`[API] Process exited with code ${code}`)
    })

    // Wait a bit for API to start
    await new Promise(resolve => setTimeout(resolve, 5000))
    return
  }

  // Production mode - use embedded API
  const apiPath = path.join(__dirname, 'api', 'EZSpeedTest.Api.exe')
  if (!require('fs').existsSync(apiPath)) {
    console.log('[MAIN] Embedded API not found, using external API server')
    return
  }

  console.log('[MAIN] Starting embedded API server...')
  
  const apiArgs = ['--urls', 'http://localhost:5299']
  
  apiProcess = spawn(apiPath, apiArgs, {
    stdio: ['ignore', 'pipe', 'pipe']
  })

  apiProcess.stdout?.on('data', (data: Buffer) => {
    console.log(`[API] ${data.toString().trim()}`)
  })

  apiProcess.stderr?.on('data', (data: Buffer) => {
    console.error(`[API ERROR] ${data.toString().trim()}`)
  })

  apiProcess.on('close', (code: number) => {
    console.log(`[API] Process exited with code ${code}`)
  })

  // Wait a bit for API to start
  await new Promise(resolve => setTimeout(resolve, 2000))
}

async function createWindow() {
  // Start API server first (in production)
  await startApiServer()

  const win = new BrowserWindow({
    width: 1100, height: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true, 
      nodeIntegration: false, 
      sandbox: true            
    }
  });

  await win.loadFile(path.join(__dirname, 'renderer', 'index.html'));
  
  win.webContents.on('console-message', (event, level, message, line, sourceId) => {
    const logLevel = ['verbose', 'info', 'warning', 'error'][level] || 'info';
    console.log(`[RENDERER] [${logLevel.toUpperCase()}] ${message}`);
  });

  if (isDev) {
    win.webContents.openDevTools();
  }
  
  win.webContents.on('did-finish-load', () => {
    console.log(`[MAIN] Renderer loaded, backend URL: ${BACKEND_URL}`);
    win.webContents.executeJavaScript(`
      fetch('${BACKEND_URL}/healthz').then(r=>r.json()).then(console.log).catch(console.error);
    `);
  });
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => { 
  if (apiProcess) {
    console.log('[MAIN] Stopping API server...')
    apiProcess.kill()
  }
  if (process.platform !== 'darwin') app.quit(); 
});

app.on('before-quit', () => {
  if (apiProcess) {
    console.log('[MAIN] Stopping API server...')
    apiProcess.kill()
  }
});

// Type definitions
interface SpeedTestServer {
  name: string;
  url: string;
  region: string;
  country?: string;
  city?: string;
}

interface PingResponse {
  averageMs: number;
  minMs: number;
  maxMs: number;
  medianMs: number;
  packetLossPercent: number;
  host: string;
  timestamp: string;
}

    interface DownloadResponse {
      mbps: number;
      megaBytesPerSecond: number;
      bytesDownloaded: number;
      durationSeconds: number;
      serverUrl: string;
      timestamp: string;
    }

// Global API interface
declare global {
  interface Window {
    api?: {
      backendUrl: string;
    };
  }
}

class SpeedTestApp {
  private backendUrl: string;
  private servers: SpeedTestServer[] = [];
  private currentServer: SpeedTestServer | null = null;
  private correlationId: string;
  private isConnected: boolean = false;

  // DOM elements
  private serverSelect: HTMLSelectElement;
  private refreshServersBtn: HTMLButtonElement;
  private pingBtn: HTMLButtonElement;
  private downloadBtn: HTMLButtonElement;
  private fullTestBtn: HTMLButtonElement;
  private legacyBtn: HTMLButtonElement;
  private loading: HTMLDivElement;
  private results: HTMLDivElement;
  private legacyOut: HTMLPreElement;
  private connectionStatus: HTMLDivElement;
  private connectionText: HTMLSpanElement;

  constructor() {
    this.backendUrl = window.api?.backendUrl ?? 'http://localhost:5299';
    this.correlationId = this.generateCorrelationId();
    
    this.log('info', 'SpeedTestApp initialized', { backendUrl: this.backendUrl, correlationId: this.correlationId });
    
    // Get DOM elements
    this.serverSelect = document.getElementById('serverSelect') as HTMLSelectElement;
    this.refreshServersBtn = document.getElementById('refreshServers') as HTMLButtonElement;
    this.pingBtn = document.getElementById('pingBtn') as HTMLButtonElement;
    this.downloadBtn = document.getElementById('downloadBtn') as HTMLButtonElement;
    this.fullTestBtn = document.getElementById('fullTestBtn') as HTMLButtonElement;
    this.legacyBtn = document.getElementById('legacyBtn') as HTMLButtonElement;
    this.loading = document.getElementById('loading') as HTMLDivElement;
    this.results = document.getElementById('results') as HTMLDivElement;
    this.legacyOut = document.getElementById('legacyOut') as HTMLPreElement;
    this.connectionStatus = document.getElementById('connectionStatus') as HTMLDivElement;
    this.connectionText = document.getElementById('connectionText') as HTMLSpanElement;

    this.setupEventListeners();
    this.checkConnection();
  }

  private async checkConnection(): Promise<void> {
    this.updateConnectionStatus('connecting', 'Connecting to API...');
    
    try {
      const response = await fetch(`${this.backendUrl}/healthz`, {
        headers: { 'X-Correlation-ID': this.correlationId },
        signal: AbortSignal.timeout(5000)
      });
      
      if (response.ok) {
        this.isConnected = true;
        this.updateConnectionStatus('connected', 'Connected to API');
        this.enableControls();
        this.loadServers();
      } else {
        throw new Error(`HTTP ${response.status}`);
      }
    } catch (error) {
      this.isConnected = false;
      this.updateConnectionStatus('disconnected', 'Failed to connect to API');
      this.disableControls();
      this.log('error', 'Connection check failed', { error: error?.toString() });
    }
  }

  private updateConnectionStatus(status: 'connecting' | 'connected' | 'disconnected', message: string): void {
    this.connectionStatus.className = `status-indicator ${status}`;
    this.connectionText.textContent = message;
  }

  private enableControls(): void {
    this.serverSelect.disabled = false;
    this.refreshServersBtn.disabled = false;
    this.legacyBtn.disabled = false;
  }

  private disableControls(): void {
    this.serverSelect.disabled = true;
    this.refreshServersBtn.disabled = true;
    this.pingBtn.disabled = true;
    this.downloadBtn.disabled = true;
    this.fullTestBtn.disabled = true;
    this.legacyBtn.disabled = true;
  }

  private setupEventListeners(): void {
    this.refreshServersBtn.addEventListener('click', () => this.loadServers());
    this.serverSelect.addEventListener('change', () => this.onServerChange());
    this.pingBtn.addEventListener('click', () => this.runPingTest());
    this.downloadBtn.addEventListener('click', () => this.runDownloadTest());
    this.fullTestBtn.addEventListener('click', () => this.runFullTest());
    this.legacyBtn.addEventListener('click', () => this.runLegacyTest());
  }

  private async loadServers(): Promise<void> {
    if (!this.isConnected) {
      this.log('warn', 'Cannot load servers: not connected to API');
      return;
    }

    try {
      this.log('info', 'Loading available servers');
      this.serverSelect.innerHTML = '<option value="">Loading servers...</option>';
      this.serverSelect.disabled = true;
      
      const response = await fetch(`${this.backendUrl}/api/v1/speed/servers`, {
        headers: { 'X-Correlation-ID': this.correlationId },
        signal: AbortSignal.timeout(10000)
      });
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      
      this.servers = await response.json();
      
      this.serverSelect.innerHTML = '<option value="">Select a server...</option>';
      this.servers.forEach((server, index) => {
        const option = document.createElement('option');
        option.value = index.toString();
        option.textContent = `${server.name} (${server.region})`;
        this.serverSelect.appendChild(option);
      });
      
      this.serverSelect.disabled = false;
      
      this.log('info', 'Servers loaded successfully', { 
        serverCount: this.servers.length,
        servers: this.servers.map(s => ({ name: s.name, region: s.region }))
      });
      
      this.showResults(`✅ Loaded ${this.servers.length} servers successfully`, 'success');
    } catch (error) {
      this.log('error', 'Failed to load servers', { 
        error: error?.toString(),
        errorType: error?.constructor?.name,
        errorMessage: error?.message,
        errorStack: error?.stack,
        backendUrl: this.backendUrl,
        correlationId: this.correlationId
      });
      this.serverSelect.innerHTML = '<option value="">❌ Failed to load servers</option>';
      this.serverSelect.disabled = false;
      this.showResults(`❌ Failed to load servers: ${error}`, 'error');
    }
  }

  private onServerChange(): void {
    const selectedIndex = this.serverSelect.value;
    this.currentServer = selectedIndex ? this.servers[parseInt(selectedIndex)] : null;
    
    const hasServer = this.currentServer !== null;
    this.pingBtn.disabled = !hasServer;
    this.downloadBtn.disabled = !hasServer;
    this.fullTestBtn.disabled = !hasServer;
    
    this.log('info', 'Server selection changed', { 
      selectedServer: this.currentServer?.name || 'none',
      buttonsEnabled: hasServer
    });
  }

  private showLoading(): void {
    this.loading.classList.remove('hidden');
    this.results.innerHTML = '';
  }

  private hideLoading(): void {
    this.loading.classList.add('hidden');
  }

  private showResults(content: string, type: 'success' | 'error' | 'info' | 'warning' = 'info'): void {
    this.hideLoading();
    const timestamp = new Date().toLocaleTimeString();
    const formattedContent = `[${timestamp}] ${content}`;
    this.results.innerHTML = `<div class="${type}">${formattedContent}</div>`;
  }

  private appendResults(content: string, type: 'success' | 'error' | 'info' | 'warning' = 'info'): void {
    const timestamp = new Date().toLocaleTimeString();
    const formattedContent = `[${timestamp}] ${content}`;
    const div = document.createElement('div');
    div.className = type;
    div.textContent = formattedContent;
    this.results.appendChild(div);
    this.results.scrollTop = this.results.scrollHeight;
  }

  private async runPingTest(): Promise<void> {
    if (!this.currentServer) {
      this.showResults('⚠️ Please select a server first', 'warning');
      this.log('warn', 'Ping test attempted without server selection');
      return;
    }
    
    const hostname = new URL(this.currentServer.url).hostname;
    this.log('info', 'Starting ping test', { 
      server: this.currentServer.name, 
      hostname 
    });
    
    this.showLoading();
    this.showResults(`🏓 Starting ping test to ${this.currentServer.name}...`, 'info');
    
    try {
      const startTime = Date.now();
      const response = await fetch(`${this.backendUrl}/api/v1/speed/ping`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'X-Correlation-ID': this.correlationId
        },
        body: JSON.stringify({ host: hostname }),
        signal: AbortSignal.timeout(30000)
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }
      
      const data: PingResponse = await response.json();
      const testDuration = ((Date.now() - startTime) / 1000).toFixed(1);
      
      const result = `🏓 PING TEST COMPLETED

📡 Server: ${this.currentServer.name}
🌐 Host: ${data.host}
⚡ Average: ${data.averageMs.toFixed(2)} ms
📈 Min: ${data.minMs.toFixed(2)} ms
📉 Max: ${data.maxMs.toFixed(2)} ms
📊 Median: ${data.medianMs.toFixed(2)} ms
📦 Packet Loss: ${data.packetLossPercent.toFixed(1)}%
⏱️ Test Duration: ${testDuration}s
🕐 Completed: ${new Date(data.timestamp).toLocaleString()}`;
      
      this.showResults(result, 'success');
      
      this.log('info', 'Ping test completed successfully', {
        server: this.currentServer.name,
        hostname,
        averageMs: data.averageMs,
        packetLoss: data.packetLossPercent,
        testDuration
      });
    } catch (error) {
      this.log('error', 'Ping test failed', { 
        server: this.currentServer.name,
        hostname,
        error: error?.toString()
      });
      this.showResults(`❌ Ping test failed: ${error}`, 'error');
    }
  }

  private async runDownloadTest(): Promise<void> {
    if (!this.currentServer) {
      this.log('warn', 'Download test attempted without server selection');
      return;
    }
    
    this.log('info', 'Starting download test', { 
      server: this.currentServer.name,
      serverUrl: this.currentServer.url
    });
    
    this.showLoading();
    
    try {
      const response = await fetch(`${this.backendUrl}/api/v1/speed/download`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'X-Correlation-ID': this.correlationId
        },
        body: JSON.stringify({ serverUrl: this.currentServer.url })
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }
      
      const data: DownloadResponse = await response.json();
      
      const result = `📥 DOWNLOAD TEST RESULTS
Server: ${this.currentServer.name}
Speed: ${data.mbps.toFixed(2)} Mbps (${data.megaBytesPerSecond.toFixed(2)} MB/s)
Downloaded: ${(data.bytesDownloaded / 1024 / 1024).toFixed(2)} MB
Duration: ${data.durationSeconds.toFixed(2)} seconds
Timestamp: ${new Date(data.timestamp).toLocaleString()}`;
      
      this.showResults(result, 'success');
      
      this.log('info', 'Download test completed successfully', {
        server: this.currentServer.name,
        mbps: data.mbps,
        bytesDownloaded: data.bytesDownloaded,
        durationSeconds: data.durationSeconds
      });
    } catch (error) {
      this.log('error', 'Download test failed', { 
        server: this.currentServer.name,
        serverUrl: this.currentServer.url,
        error: error?.toString()
      });
      this.showResults(`❌ Download test failed: ${error}`, 'error');
    }
  }

  private async runFullTest(): Promise<void> {
    if (!this.currentServer) {
      this.log('warn', 'Full test attempted without server selection');
      return;
    }
    
    const hostname = new URL(this.currentServer.url).hostname;
    this.log('info', 'Starting full speed test', { 
      server: this.currentServer.name,
      hostname,
      serverUrl: this.currentServer.url
    });
    
    this.showLoading();
    
    try {
      // Run ping test first
      this.log('info', 'Full test: Starting ping phase');
      const pingResponse = await fetch(`${this.backendUrl}/api/v1/speed/ping`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'X-Correlation-ID': this.correlationId
        },
        body: JSON.stringify({ host: hostname })
      });
      
      if (!pingResponse.ok) throw new Error(`Ping failed: HTTP ${pingResponse.status}`);
      const pingData: PingResponse = await pingResponse.json();
      
      // Then run download test
      this.log('info', 'Full test: Starting download phase');
      const downloadResponse = await fetch(`${this.backendUrl}/api/v1/speed/download`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'X-Correlation-ID': this.correlationId
        },
        body: JSON.stringify({ serverUrl: this.currentServer.url })
      });
      
      if (!downloadResponse.ok) throw new Error(`Download failed: HTTP ${downloadResponse.status}`);
      const downloadData: DownloadResponse = await downloadResponse.json();
      
      const result = `🚀 FULL SPEED TEST RESULTS
Server: ${this.currentServer.name} (${this.currentServer.region})

🏓 PING:
  Average: ${pingData.averageMs.toFixed(2)} ms
  Packet Loss: ${pingData.packetLossPercent.toFixed(1)}%

📥 DOWNLOAD:
  Speed: ${downloadData.mbps.toFixed(2)} Mbps (${downloadData.megaBytesPerSecond.toFixed(2)} MB/s)
  Downloaded: ${(downloadData.bytesDownloaded / 1024 / 1024).toFixed(2)} MB
  Duration: ${downloadData.durationSeconds.toFixed(2)} seconds

Completed: ${new Date().toLocaleString()}`;
      
      this.showResults(result, 'success');
      
      this.log('info', 'Full speed test completed successfully', {
        server: this.currentServer.name,
        ping: { averageMs: pingData.averageMs, packetLoss: pingData.packetLossPercent },
        download: { mbps: downloadData.mbps, bytesDownloaded: downloadData.bytesDownloaded }
      });
    } catch (error) {
      this.log('error', 'Full speed test failed', { 
        server: this.currentServer.name,
        error: error?.toString()
      });
      this.showResults(`❌ Full test failed: ${error}`, 'error');
    }
  }

  private async runLegacyTest(): Promise<void> {
    try {
      this.log('info', 'Starting legacy health check test');
      this.legacyOut.textContent = 'Testing legacy endpoint...';
      
      const response = await fetch(`${this.backendUrl}/healthz`, {
        headers: { 'X-Correlation-ID': this.correlationId }
      });
      
      const data = await response.json();
      this.legacyOut.textContent = JSON.stringify(data, null, 2);
      
      this.log('info', 'Legacy health check completed successfully', { response: data });
    } catch (error) {
      this.legacyOut.textContent = `Error: ${error}`;
      this.log('error', 'Legacy health check failed', { 
        error: error?.toString(),
        errorType: error?.constructor?.name,
        errorMessage: error?.message,
        errorStack: error?.stack,
        backendUrl: this.backendUrl,
        correlationId: this.correlationId
      });
    }
  }

  private generateCorrelationId(): string {
    return `ui-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private log(level: 'info' | 'warn' | 'error', message: string, data?: any): void {
    const timestamp = new Date().toISOString();
    const logEntry = {
      timestamp,
      level: level.toUpperCase(),
      source: 'UI',
      correlationId: this.correlationId,
      message,
      ...data
    };

    // Console logging with colors
    const colors = {
      info: 'color: #2196F3',
      warn: 'color: #FF9800', 
      error: 'color: #f44336'
    };

    console.log(`%c[${timestamp}] [UI] [${level.toUpperCase()}] ${message}`, colors[level], data || '');

    // Send to main process for centralized logging (if needed)
    try {
      // We could send this to main process via IPC for file logging
      // For now, just console logging
    } catch (e) {
      console.error('Failed to send log to main process:', e);
    }
  }
}

// Initialize app when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  new SpeedTestApp();
});
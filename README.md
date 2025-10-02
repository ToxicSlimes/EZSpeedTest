# EZ Speed Test

A modern desktop speed test application built with .NET 9.0 and Electron, following Clean Architecture principles.

## 🏗️ Architecture

- **Domain** - Core business models and entities
- **Application** - CQRS commands/queries, handlers, DTOs, and validation
- **Infrastructure** - External service implementations (HTTP clients, ping services)
- **API** - ASP.NET Core Web API with controllers using MediatR
- **UI** - Electron desktop application

## 🚀 Features

- **Ping Test** - Measure latency to any host with packet loss detection
- **Download Speed Test** - Measure download speed from configurable servers
- **Server Management** - Multiple test servers with automatic selection
- **Modern UI** - Beautiful Electron desktop interface
- **Resilient HTTP** - Polly-based retry policies and timeouts
- **Structured Logging** - Serilog with console and file outputs
- **API Documentation** - Swagger/OpenAPI integration

## 📋 Requirements

- **.NET 9.0 SDK** (9.0.300 or later)
- **Node.js** (18+ recommended)
- **Windows 10/11** (tested platform)

## 🛠️ Development Setup

### 1. Clone and Setup

```bash
git clone <repository-url>
cd EZSpeedTest
```

### 2. Install Dependencies

```bash
# .NET dependencies (automatic via CPM)
dotnet restore

# Electron dependencies
cd ui/electron
npm install
cd ../..
```

### 3. Run Development Environment

**Option A: Full Development (API + Electron)**
```bash
# Using dev script (recommended)
.\scripts\dev.ps1

# Or using make
make dev
```

**Option B: API Only (for Rider development)**
```bash
# Using dev script
.\scripts\dev.ps1 -NoElectron

# Or using make
make dev-api

# Or directly in Rider - use "API Only" run configuration
```

**Option C: Manual**
```bash
# Terminal 1: Start API
dotnet run --project src/EZSpeedTest.Api

# Terminal 2: Start Electron (if needed)
cd ui/electron
npm run dev
```

### 🛑 Stop All Processes

If you encounter file locking issues during build:
```bash
# Stop all processes
.\scripts\stop.ps1

# Or using make
make stop
```

## 🏗️ Production Build

### Build Standalone Application

**Windows (Default)**
```bash
# Build production version for Windows
.\scripts\build-production.ps1

# Or using make
make build-prod
```

**All Platforms**
```bash
# Build for specific platform
make build-win    # Windows
make build-mac    # macOS  
make build-linux  # Linux
make build-all    # All platforms
```

### Output

After building, you'll find:
- **Installer**: `ui/electron/release/EZ Speed Test Setup.exe` (Windows)
- **Portable**: `ui/electron/release/EZ Speed Test.exe` (Windows)
- **API**: `ui/electron/dist/api/` (embedded .NET API)

The standalone app includes:
- ✅ Embedded .NET API server
- ✅ Electron desktop interface  
- ✅ Automatic API startup/shutdown
- ✅ No external dependencies required

### 4. Access Application

- **Electron App**: Opens automatically as desktop window
- **Swagger UI**: http://localhost:5299/swagger (Development only)
- **Health Check**: http://localhost:5299/healthz

## 🔧 Configuration

### API Settings (`src/EZSpeedTest.Api/appsettings.Development.json`)

```json
{
  "Urls": "http://localhost:5299",
  "Electron": {
    "AutoStartDev": true  // Set to false to disable auto-launch
  },
  "SpeedTest": {
    "Settings": {
      "PingTimeout": "00:00:05",
      "PingAttemptCount": 4,
      "DownloadTimeout": "00:00:30",
      "BufferSizeKb": 64
    },
    "Servers": [
      {
        "Name": "Cloudflare Test File",
        "Url": "https://speed.cloudflare.com/__down?bytes=10000000",
        "Region": "Global",
        "Priority": 100
      }
    ]
  }
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/EZSpeedTest.Tests
```

## 📦 Building

### Development Build
```bash
dotnet build
```

### Production Build
```bash
# API
dotnet publish src/EZSpeedTest.Api -c Release -o publish/api

# Electron App
cd ui/electron
npm run build
npm run pack
```

## 🏃‍♂️ Usage

1. **Launch Application**: Run `dotnet run --project src/EZSpeedTest.Api`
2. **Select Server**: Choose from the dropdown list
3. **Run Tests**:
   - **Ping Test**: Measure latency only
   - **Download Test**: Measure download speed only  
   - **Full Test**: Complete ping + download measurement

## 🔌 API Endpoints

### Speed Test
- `GET /api/v1/speed/servers` - List available servers
- `POST /api/v1/speed/ping` - Measure ping latency
- `POST /api/v1/speed/download` - Measure download speed

### System
- `GET /healthz` - Health check
- `GET /swagger` - API documentation (Development)

## 🏗️ Project Structure

```
EZSpeedTest/
├── src/
│   ├── EZSpeedTest.Domain/          # Core business models
│   ├── EZSpeedTest.Application/     # CQRS, DTOs, validation
│   ├── EZSpeedTest.Infrastructure/  # External service implementations
│   └── EZSpeedTest.Api/            # Web API controllers
├── ui/electron/                    # Electron desktop app
├── tests/EZSpeedTest.Tests/        # Unit & integration tests
├── Directory.Packages.props        # Central package management
├── Directory.Build.props           # Common build settings
└── global.json                     # .NET SDK version pinning
```

## 🛠️ Troubleshooting

### File Locking Issues in Rider

If you encounter `MSB3026/MSB3027` errors about file locking during build:

1. **Stop all processes first:**
   ```bash
   .\scripts\stop.ps1
   # or
   make stop
   ```

2. **Use API-only mode for development:**
   ```bash
   make dev-api
   # or use "API Only" run configuration in Rider
   ```

3. **For full development with Electron:**
   ```bash
   make dev
   # This uses FullDev environment which enables Electron auto-launch
   ```

### Electron Issues

- **DevTools**: Automatically opens in development mode
- **Console logs**: Check both Electron DevTools and API console
- **CORS errors**: API is configured for `http://localhost:*` origins

### API Issues

- **Port conflicts**: API uses `http://localhost:5299` by default
- **Swagger UI**: Available at `/swagger` in Development mode
- **Health check**: Available at `/healthz`

## 🔧 Technologies Used

### Backend
- **.NET 9.0** - Runtime and SDK
- **ASP.NET Core** - Web API framework
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **Mapster** - Object-to-object mapping
- **Polly** - Resilience and fault handling
- **Serilog** - Structured logging
- **Swashbuckle** - OpenAPI/Swagger documentation

### Frontend
- **Electron 38.x** - Desktop application framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool
- **esbuild** - JavaScript bundler

### Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **WebApplicationFactory** - Integration testing

## 🤝 Contributing

1. Follow Clean Architecture principles
2. Use CQRS pattern for new features
3. Add unit tests for business logic
4. Add integration tests for API endpoints
5. Update documentation for new features

## 📝 License

This project is licensed under the MIT License.

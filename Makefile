.PHONY: dev build test clean install stop build-prod build-win build-mac build-linux help

# Default target
help:
	@echo "EZ Speed Test - Available commands:"
	@echo "  dev         - Start development environment"
	@echo "  dev-api     - Start only API (no Electron)"
	@echo "  build       - Build all projects (debug)"
	@echo "  build-prod   - Build production version (Windows)"
	@echo "  build-win    - Build production for Windows"
	@echo "  build-mac    - Build production for macOS"
	@echo "  build-linux  - Build production for Linux"
	@echo "  test        - Run all tests"
	@echo "  clean       - Clean build outputs"
	@echo "  install     - Install all dependencies"
	@echo "  stop        - Stop all running processes"

# Development
dev:
	@echo "🚀 Starting development environment..."
	@powershell -ExecutionPolicy Bypass -File scripts/dev.ps1

dev-clean:
	@echo "🚀 Starting development environment (clean build)..."
	@powershell -ExecutionPolicy Bypass -File scripts/dev.ps1 -Clean

dev-api:
	@echo "🌐 Starting API only (no Electron)..."
	@powershell -ExecutionPolicy Bypass -File scripts/dev.ps1 -NoElectron

# Stop all processes
stop:
	@echo "🛑 Stopping all processes..."
	@powershell -ExecutionPolicy Bypass -File scripts/stop.ps1

# Production builds
build-prod:
	@echo "🏗️ Building production version (Windows)..."
	@powershell -ExecutionPolicy Bypass -File scripts/build-production.ps1 -Platform win

build-win:
	@echo "🏗️ Building production version for Windows..."
	@powershell -ExecutionPolicy Bypass -File scripts/build-production.ps1 -Platform win

build-mac:
	@echo "🏗️ Building production version for macOS..."
	@powershell -ExecutionPolicy Bypass -File scripts/build-production.ps1 -Platform mac

build-linux:
	@echo "🏗️ Building production version for Linux..."
	@powershell -ExecutionPolicy Bypass -File scripts/build-production.ps1 -Platform linux

build-all:
	@echo "🏗️ Building production version for all platforms..."
	@powershell -ExecutionPolicy Bypass -File scripts/build-production.ps1 -Platform all

dev-no-electron:
	@echo "🚀 Starting API only (no Electron)..."
	@powershell -ExecutionPolicy Bypass -File scripts/dev.ps1 -NoElectron

# Building
build:
	@echo "🔨 Building solution..."
	@dotnet build

build-release:
	@echo "🔨 Building solution (Release)..."
	@dotnet build -c Release

# Testing
test:
	@echo "🧪 Running tests..."
	@dotnet test

test-coverage:
	@echo "🧪 Running tests with coverage..."
	@dotnet test --collect:"XPlat Code Coverage"

# Maintenance
clean:
	@echo "🧹 Cleaning build outputs..."
	@dotnet clean
	@if exist "ui\electron\dist" rmdir /s /q "ui\electron\dist"

install:
	@echo "📦 Installing dependencies..."
	@dotnet restore
	@cd ui\electron && npm install

# Electron specific
electron-build:
	@echo "🎨 Building Electron assets..."
	@cd ui\electron && npm run build

electron-pack:
	@echo "📦 Packaging Electron app..."
	@cd ui\electron && npm run pack

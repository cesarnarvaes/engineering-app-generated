#!/usr/bin/env pwsh

param(
    [switch]$NoRestore = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "🚀 Starting BizCrm local development environment" -ForegroundColor Green

# Check prerequisites
Write-Host "\n📋 Checking prerequisites..." -ForegroundColor Blue

# Check if .NET is installed
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
}

# Check if Node.js is installed
if (!(Get-Command "node" -ErrorAction SilentlyContinue)) {
    Write-Error "Node.js is not installed. Please install it from https://nodejs.org"
}

# Check if SQL Server LocalDB is available
try {
    $sqlResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION" -h -1
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "SQL Server LocalDB not found. Please install SQL Server Express LocalDB"
    }
    else {
        Write-Host "✅ SQL Server LocalDB is available" -ForegroundColor Green
    }
}
catch {
    Write-Warning "Could not verify SQL Server LocalDB installation"
}

Write-Host "✅ All prerequisites are installed" -ForegroundColor Green

# Setup environment variables
Write-Host "\n🔧 Setting up environment variables..." -ForegroundColor Blue

# Check if .env file exists
if (!(Test-Path ".env")) {
    if (Test-Path ".env.template") {
        Write-Host "Creating .env file from template..." -ForegroundColor Yellow
        Copy-Item ".env.template" ".env"
        Write-Host "⚠️ Please update the .env file with your configuration" -ForegroundColor Yellow
    }
    else {
        Write-Warning ".env.template file not found. Please create a .env file with necessary configuration."
    }
}

# Load environment variables from .env file
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^([^=]+)=(.*)$") {
            $envName = $matches[1]
            $envValue = $matches[2]
            [Environment]::SetEnvironmentVariable($envName, $envValue, "Process")
        }
    }
    Write-Host "✅ Environment variables loaded from .env file" -ForegroundColor Green
}

# Restore packages
if (!$NoRestore) {
    Write-Host "\n📦 Restoring packages..." -ForegroundColor Blue
    
    # Restore .NET packages
    Write-Host "Restoring .NET packages..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        dotnet restore
        Write-Host "✅ .NET packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    # Restore Node.js packages
    Write-Host "Restoring Node.js packages..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm ci
        Write-Host "✅ Node.js packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "⏭️ Skipping package restore" -ForegroundColor Yellow
}

# Setup database
Write-Host "\n🗄️ Setting up database..." -ForegroundColor Blue

Push-Location "./src/backend"
try {
    # Check if migrations exist
    if (!(Test-Path "./Migrations")) {
        Write-Host "Creating initial migration..." -ForegroundColor Yellow
        dotnet ef migrations add InitialCreate --context ApplicationDbContext
    }
    
    # Update database
    Write-Host "Updating database..." -ForegroundColor Yellow
    dotnet ef database update --context ApplicationDbContext
    Write-Host "✅ Database updated successfully" -ForegroundColor Green
}
catch {
    Write-Error "Failed to setup database: $($_.Exception.Message)"
}
finally {
    Pop-Location
}

# Start development servers
Write-Host "\n🚀 Starting development servers..." -ForegroundColor Blue

# Function to start backend in background
function Start-Backend {
    Write-Host "Starting backend server..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        $backendJob = Start-Job -ScriptBlock {
            param($workingDir)
            Set-Location $workingDir
            dotnet watch run --urls="https://localhost:7001;http://localhost:5001"
        } -ArgumentList (Get-Location).Path
        
        return $backendJob
    }
    finally {
        Pop-Location
    }
}

# Function to start frontend
function Start-Frontend {
    Write-Host "Starting frontend server..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm run dev
    }
    finally {
        Pop-Location
    }
}

# Start backend in background
$backendJob = Start-Backend
Start-Sleep -Seconds 5  # Give backend time to start

Write-Host "\n📊 Development Environment Status" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "🔗 Backend API: https://localhost:7001" -ForegroundColor Cyan
Write-Host "🔗 Backend API (HTTP): http://localhost:5001" -ForegroundColor Cyan
Write-Host "📊 Hangfire Dashboard: https://localhost:7001/hangfire" -ForegroundColor Cyan
Write-Host "🌐 Frontend: http://localhost:5173" -ForegroundColor Cyan
Write-Host "📚 API Documentation: https://localhost:7001/swagger" -ForegroundColor Cyan

Write-Host "\n⚠️ Press Ctrl+C to stop all servers" -ForegroundColor Yellow
Write-Host "Starting frontend server (this will block until you stop it)..." -ForegroundColor Yellow

# Start frontend (this will block)
try {
    Start-Frontend
}
finally {
    # Cleanup: Stop backend job when frontend stops
    Write-Host "\n🛑 Stopping backend server..." -ForegroundColor Red
    if ($backendJob) {
        Stop-Job $backendJob -Force
        Remove-Job $backendJob -Force
    }
    
    Write-Host "✅ Development environment stopped" -ForegroundColor Green
}
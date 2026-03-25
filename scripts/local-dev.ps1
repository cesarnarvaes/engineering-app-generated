#!/usr/bin/env pwsh

param(
    [switch]$NoRestore = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Check if we're running in PowerShell Core
if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Warning "This script is designed for PowerShell Core (pwsh) 6.0 or later."
    Write-Warning "You're running PowerShell $($PSVersionTable.PSVersion). Some features may not work correctly."
}

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "[START] Starting BizCrm local development environment" -ForegroundColor Green

# Change to project root directory (one level up from scripts)
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent $ScriptRoot
Set-Location $ProjectRoot
Write-Host "[INFO] Working directory: $(Get-Location)" -ForegroundColor Cyan

# Check prerequisites
Write-Host "\n[CHECK] Checking prerequisites..." -ForegroundColor Blue

# Check if .NET is installed
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
}

# Check if Node.js is installed
if (!(Get-Command "node" -ErrorAction SilentlyContinue)) {
    Write-Error "Node.js is not installed. Please install it from https://nodejs.org"
}

# Check if SQL Server is available
Write-Host "Checking SQL Server connectivity..." -ForegroundColor Yellow

# First check if sqlcmd is available
if (!(Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    Write-Warning "sqlcmd is not available. Please install SQL Server Command Line Tools or SQL Server Management Studio."
    Write-Host "Download from: https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility" -ForegroundColor Cyan
}
else {
    try {
        $sqlResult = & sqlcmd -S localhost -U sa -P '4mrpo9foFyn5yaFgzn0L' -Q 'SELECT @@VERSION' -h -1 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "SQL Server connection failed. Error details:"
            Write-Host $sqlResult -ForegroundColor Red
            Write-Host "`nPossible solutions:" -ForegroundColor Yellow
            Write-Host "1. Verify SQL Server is running: services.msc -> SQL Server (MSSQLSERVER)" -ForegroundColor Cyan
            Write-Host "2. Check if SQL Server Authentication is enabled (not just Windows Auth)" -ForegroundColor Cyan
            Write-Host "3. Verify 'sa' user is enabled and password is correct" -ForegroundColor Cyan
            Write-Host "4. Check if SQL Server is listening on localhost:1433" -ForegroundColor Cyan
        }
        else {
            Write-Host "[OK] SQL Server on localhost is available" -ForegroundColor Green
            Write-Verbose "SQL Server Version: $sqlResult"
        }
    }
    catch {
        Write-Warning "SQL Server connectivity check failed with exception: $($_.Exception.Message)"
        Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
        Write-Host "1. Check if SQL Server service is running" -ForegroundColor Cyan
        Write-Host "2. Verify SQL Server is configured for mixed mode authentication" -ForegroundColor Cyan
        Write-Host "3. Confirm 'sa' account is enabled and password matches your configuration" -ForegroundColor Cyan
    }
}

Write-Host "[OK] All prerequisites are installed" -ForegroundColor Green

# Setup environment variables
Write-Host "\n[SETUP] Setting up environment variables..." -ForegroundColor Blue

# Check if .env file exists
if (!(Test-Path ".env")) {
    if (Test-Path ".env.template") {
        Write-Host "Creating .env file from template..." -ForegroundColor Yellow
        Copy-Item ".env.template" ".env"
        Write-Host "[WARN] Please update the .env file with your configuration" -ForegroundColor Yellow
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
    Write-Host "[OK] Environment variables loaded from .env file" -ForegroundColor Green
}

# Restore packages
if (!$NoRestore) {
    Write-Host "\n[RESTORE] Restoring packages..." -ForegroundColor Blue
    
    # Restore .NET packages
    Write-Host "Restoring .NET packages..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        dotnet restore
        Write-Host "[OK] .NET packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    # Restore Node.js packages
    Write-Host "Restoring Node.js packages..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm ci
        Write-Host "[OK] Node.js packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "[SKIP] Skipping package restore" -ForegroundColor Yellow
}

# Setup database
Write-Host "\n[DATABASE] Setting up database..." -ForegroundColor Blue

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
    Write-Host "[OK] Database updated successfully" -ForegroundColor Green
}
catch {
    Write-Error "Failed to setup database: $($_.Exception.Message)"
}
finally {
    Pop-Location
}

# Start development servers
Write-Host "\n[START] Starting development servers..." -ForegroundColor Blue

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
    
    $frontendPath = "./src/frontend"
    if (!(Test-Path $frontendPath)) {
        Write-Error "Frontend directory not found: $frontendPath"
        return
    }
    
    Push-Location $frontendPath
    try {
        # Check if package.json exists
        if (!(Test-Path "./package.json")) {
            Write-Error "package.json not found in frontend directory"
            return
        }
        
        # Check if node_modules exists
        if (!(Test-Path "./node_modules")) {
            Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
            npm install
        }
        
        # Start the dev server
        Write-Host "Starting Vite development server..." -ForegroundColor Yellow
        npm run dev
    }
    catch {
        Write-Error "Failed to start frontend server: $($_.Exception.Message)"
    }
    finally {
        Pop-Location
    }
}

# Start backend in background
$backendJob = Start-Backend
Start-Sleep -Seconds 5  # Give backend time to start

Write-Host "\n[STATUS] Development Environment Status" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "[URL] Backend API: https://localhost:7001" -ForegroundColor Cyan
Write-Host "[URL] Backend API (HTTP): http://localhost:5001" -ForegroundColor Cyan
Write-Host "[URL] Hangfire Dashboard: https://localhost:7001/hangfire" -ForegroundColor Cyan
Write-Host "[URL] Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "[URL] API Documentation: https://localhost:7001/swagger" -ForegroundColor Cyan

Write-Host "\n[INFO] Press Ctrl+C to stop all servers" -ForegroundColor Yellow
Write-Host "Starting frontend server (this will block until you stop it)..." -ForegroundColor Yellow

# Start frontend (this will block)
try {
    Start-Frontend
}
finally {
    # Cleanup: Stop backend job when frontend stops
    Write-Host "`n[STOP] Stopping backend server..." -ForegroundColor Red
    if ($backendJob) {
        Stop-Job $backendJob
        Remove-Job $backendJob -Force
    }
    
    Write-Host "[DONE] Development environment stopped" -ForegroundColor Green
}
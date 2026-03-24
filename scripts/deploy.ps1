#!/usr/bin/env pwsh

param(
    [string]$EnvironmentName = "dev",
    [string]$Location = "centralus",
    [switch]$SkipBuild = $false,
    [switch]$SkipInfra = $false,
    [switch]$SkipDeploy = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "🚀 Starting BizCrm deployment to Azure" -ForegroundColor Green
Write-Host "Environment: $EnvironmentName" -ForegroundColor Yellow
Write-Host "Location: $Location" -ForegroundColor Yellow

# Check prerequisites
Write-Host "\n📋 Checking prerequisites..." -ForegroundColor Blue

# Check if azd is installed
if (!(Get-Command "azd" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure Developer CLI (azd) is not installed. Please install it from https://aka.ms/azd-install"
}

# Check if az CLI is installed
if (!(Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/cli/azure/install-azure-cli"
}

# Check if Docker is installed
if (!(Get-Command "docker" -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed. Please install Docker Desktop from https://www.docker.com/products/docker-desktop"
}

# Check if .NET is installed
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
}

# Check if Node.js is installed
if (!(Get-Command "node" -ErrorAction SilentlyContinue)) {
    Write-Error "Node.js is not installed. Please install it from https://nodejs.org"
}

Write-Host "✅ All prerequisites are installed" -ForegroundColor Green

# Login to Azure
Write-Host "\n🔑 Checking Azure login..." -ForegroundColor Blue
$azAccount = az account show --output json 2>$null | ConvertFrom-Json
if (!$azAccount) {
    Write-Host "Please login to Azure..." -ForegroundColor Yellow
    az login
    $azAccount = az account show --output json | ConvertFrom-Json
}

Write-Host "✅ Logged in as $($azAccount.user.name) to subscription $($azAccount.name)" -ForegroundColor Green

# Initialize azd environment if it doesn't exist
Write-Host "\n🔧 Initializing Azure Developer CLI environment..." -ForegroundColor Blue

try {
    azd env select $EnvironmentName
    Write-Host "✅ Using existing environment: $EnvironmentName" -ForegroundColor Green
}
catch {
    Write-Host "Creating new environment: $EnvironmentName" -ForegroundColor Yellow
    azd env new $EnvironmentName
    azd env select $EnvironmentName
}

# Set environment variables
azd env set AZURE_LOCATION $Location
azd env set AZURE_ENV_NAME $EnvironmentName

# Build the application
if (!$SkipBuild) {
    Write-Host "\n🏗️ Building application..." -ForegroundColor Blue
    
    # Build backend
    Write-Host "Building backend..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        dotnet restore
        dotnet build --configuration Release
        Write-Host "✅ Backend built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    # Build frontend
    Write-Host "Building frontend..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm ci
        npm run build
        Write-Host "✅ Frontend built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "⏭️ Skipping build step" -ForegroundColor Yellow
}

# Provision infrastructure
if (!$SkipInfra) {
    Write-Host "\n☁️ Provisioning Azure infrastructure..." -ForegroundColor Blue
    azd provision
    Write-Host "✅ Infrastructure provisioned successfully" -ForegroundColor Green
}
else {
    Write-Host "⏭️ Skipping infrastructure provisioning" -ForegroundColor Yellow
}

# Deploy application
if (!$SkipDeploy) {
    Write-Host "\n🚀 Deploying application..." -ForegroundColor Blue
    azd deploy
    Write-Host "✅ Application deployed successfully" -ForegroundColor Green
}
else {
    Write-Host "⏭️ Skipping application deployment" -ForegroundColor Yellow
}

# Get the deployment information
Write-Host "\n📊 Deployment Summary" -ForegroundColor Blue
Write-Host "===================" -ForegroundColor Blue

$envValues = azd env get-values --output json | ConvertFrom-Json

if ($envValues.AZURE_BACKEND_URL) {
    Write-Host "🔗 Backend API: $($envValues.AZURE_BACKEND_URL)" -ForegroundColor Cyan
}

if ($envValues.AZURE_FRONTEND_URL) {
    Write-Host "🌐 Frontend URL: $($envValues.AZURE_FRONTEND_URL)" -ForegroundColor Cyan
}

if ($envValues.AZURE_BACKEND_URL) {
    Write-Host "📊 Hangfire Dashboard: $($envValues.AZURE_BACKEND_URL)/hangfire" -ForegroundColor Cyan
}

Write-Host "\n🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "\nNext steps:" -ForegroundColor Yellow
Write-Host "1. Visit the frontend URL to access the application" -ForegroundColor White
Write-Host "2. Use the Hangfire dashboard to monitor background jobs" -ForegroundColor White
Write-Host "3. Check Application Insights for monitoring and logs" -ForegroundColor White
Write-Host "4. Configure custom domains and SSL certificates if needed" -ForegroundColor White
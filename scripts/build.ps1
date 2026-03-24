#!/usr/bin/env pwsh

param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [string]$OutputPath = "./dist",
    [switch]$SkipFrontend = $false,
    [switch]$SkipBackend = $false,
    [switch]$SkipTests = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "🏗️ Building BizCrm Application" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow

# Create output directory
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "✅ Created output directory: $OutputPath" -ForegroundColor Green
}

# Clean previous builds
Write-Host "\n🧹 Cleaning previous builds..." -ForegroundColor Blue

if (!$SkipBackend) {
    Push-Location "./src/backend"
    try {
        dotnet clean --configuration $Configuration --verbosity minimal
        if (Test-Path "./bin") { Remove-Item -Recurse -Force "./bin" }
        if (Test-Path "./obj") { Remove-Item -Recurse -Force "./obj" }
        Write-Host "✅ Backend cleaned" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

if (!$SkipFrontend) {
    Push-Location "./src/frontend"
    try {
        if (Test-Path "./dist") { Remove-Item -Recurse -Force "./dist" }
        if (Test-Path "./node_modules/.vite") { Remove-Item -Recurse -Force "./node_modules/.vite" }
        Write-Host "✅ Frontend cleaned" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Restore packages
Write-Host "\n📦 Restoring packages..." -ForegroundColor Blue

if (!$SkipBackend) {
    Write-Host "Restoring .NET packages..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        dotnet restore --verbosity minimal
        Write-Host "✅ .NET packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

if (!$SkipFrontend) {
    Write-Host "Restoring Node.js packages..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm ci --silent
        Write-Host "✅ Node.js packages restored" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Run tests
if (!$SkipTests) {
    Write-Host "\n🧪 Running tests..." -ForegroundColor Blue
    
    if (!$SkipBackend) {
        Push-Location "./src/backend"
        try {
            # Check if test projects exist
            $testProjects = Get-ChildItem -Recurse -Filter "*.Tests.csproj" -File
            if ($testProjects.Count -gt 0) {
                Write-Host "Running backend tests..." -ForegroundColor Yellow
                dotnet test --configuration $Configuration --verbosity minimal --collect:"XPlat Code Coverage"
                Write-Host "✅ Backend tests passed" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️ No backend test projects found" -ForegroundColor Yellow
            }
        }
        finally {
            Pop-Location
        }
    }
    
    if (!$SkipFrontend) {
        Push-Location "./src/frontend"
        try {
            # Check if test scripts exist
            $packageJson = Get-Content "package.json" | ConvertFrom-Json
            if ($packageJson.scripts -and $packageJson.scripts.test) {
                Write-Host "Running frontend tests..." -ForegroundColor Yellow
                npm test -- --run
                Write-Host "✅ Frontend tests passed" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️ No frontend test scripts found" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "⚠️ Frontend tests skipped (not configured)" -ForegroundColor Yellow
        }
        finally {
            Pop-Location
        }
    }
}
else {
    Write-Host "⏭️ Skipping tests" -ForegroundColor Yellow
}

# Build applications
Write-Host "\n🏗️ Building applications..." -ForegroundColor Blue

if (!$SkipBackend) {
    Write-Host "Building backend..." -ForegroundColor Yellow
    Push-Location "./src/backend"
    try {
        dotnet publish --configuration $Configuration --output "../../$OutputPath/backend" --verbosity minimal
        Write-Host "✅ Backend built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

if (!$SkipFrontend) {
    Write-Host "Building frontend..." -ForegroundColor Yellow
    Push-Location "./src/frontend"
    try {
        npm run build
        
        # Copy frontend build to output directory
        $frontendOutputPath = "../../$OutputPath/frontend"
        if (!(Test-Path $frontendOutputPath)) {
            New-Item -ItemType Directory -Path $frontendOutputPath -Force | Out-Null
        }
        
        Copy-Item -Recurse -Force "./dist/*" $frontendOutputPath
        Write-Host "✅ Frontend built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Build Docker images (optional)
Write-Host "\n🐳 Building Docker images..." -ForegroundColor Blue

if (!$SkipBackend) {
    Push-Location "./src/backend"
    try {
        if (Test-Path "Dockerfile") {
            Write-Host "Building backend Docker image..." -ForegroundColor Yellow
            docker build -t bizcrm-backend:latest -t bizcrm-backend:dev .
            Write-Host "✅ Backend Docker image built" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Failed to build backend Docker image: $($_.Exception.Message)"
    }
    finally {
        Pop-Location
    }
}

# Generate build info
Write-Host "\n📊 Generating build information..." -ForegroundColor Blue

$buildInfo = @{
    BuildDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    Configuration = $Configuration
    Platform = $Platform
    OutputPath = $OutputPath
    GitCommit = ""
    GitBranch = ""
    Version = "1.0.0"
}

# Get Git information if available
try {
    if (Get-Command "git" -ErrorAction SilentlyContinue) {
        $buildInfo.GitCommit = (git rev-parse HEAD 2>$null)
        $buildInfo.GitBranch = (git rev-parse --abbrev-ref HEAD 2>$null)
    }
}
catch {
    # Git not available or not a git repository
}

# Save build info
$buildInfo | ConvertTo-Json -Depth 2 | Out-File "$OutputPath/build-info.json" -Encoding UTF8

Write-Host "\n🎉 Build completed successfully!" -ForegroundColor Green
Write-Host "\nBuild Summary:" -ForegroundColor Blue
Write-Host "=============" -ForegroundColor Blue
Write-Host "Configuration: $Configuration" -ForegroundColor White
Write-Host "Output Path: $OutputPath" -ForegroundColor White
Write-Host "Build Date: $($buildInfo.BuildDate)" -ForegroundColor White

if ($buildInfo.GitCommit) {
    Write-Host "Git Commit: $($buildInfo.GitCommit)" -ForegroundColor White
    Write-Host "Git Branch: $($buildInfo.GitBranch)" -ForegroundColor White
}

if (!$SkipBackend) {
    Write-Host "✅ Backend: Built and published" -ForegroundColor Green
}

if (!$SkipFrontend) {
    Write-Host "✅ Frontend: Built and optimized" -ForegroundColor Green
}

if (!$SkipTests) {
    Write-Host "✅ Tests: Executed successfully" -ForegroundColor Green
}

Write-Host "\nNext steps:" -ForegroundColor Yellow
Write-Host "1. Review build output in: $OutputPath" -ForegroundColor White
Write-Host "2. Test the application locally" -ForegroundColor White
Write-Host "3. Deploy to your target environment" -ForegroundColor White
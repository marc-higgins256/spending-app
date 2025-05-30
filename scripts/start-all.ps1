# PSScriptAnalyzerSettings: { "Rules": { "PSUseSetLocationForLocationCmdlet": { "Severity": "None" } } }
# PowerShell script to start both backend and frontend
# This script opens each service in a new PowerShell window

$backendPath = "..\spending-app\backend"
$frontendPath = "..\spending-app\frontend"

# Set ASPNETCORE_ENVIRONMENT to Development for backend
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Start backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Push-Location $backendPath; dotnet run; Pop-Location"

# Start frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Push-Location $frontendPath; npm run dev; Pop-Location"

Write-Host "Backend and frontend are starting in new PowerShell windows."
Write-Host "Starting backend (ASP.NET Core) in Development mode..."
Write-Host "Starting frontend (Vite)..."

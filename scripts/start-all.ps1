# PSScriptAnalyzerSettings: { "Rules": { "PSUseSetLocationForLocationCmdlet": { "Severity": "None" } } }
# PowerShell script to start both backend and frontend
# This script opens each service in a new PowerShell window

$backendPath = "..\spending-app\backend"
$frontendPath = "..\spending-app\frontend"

# Start backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Push-Location $backendPath; dotnet run; Pop-Location"

# Start frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Push-Location $frontendPath; npm run dev; Pop-Location"

Write-Host "Backend and frontend are starting in new PowerShell windows."

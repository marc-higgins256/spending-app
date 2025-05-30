# PowerShell script to start both backend and frontend
# This script opens each service in a new PowerShell window

$backendPath = "..\spending-app\backend"
$frontendPath = "..\spending-app\frontend"

# Start backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $backendPath; dotnet run"

# Start frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $frontendPath; npm run dev"

Write-Host "Backend and frontend are starting in new PowerShell windows."

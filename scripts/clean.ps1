# Clean backend and test projects
Write-Host "Cleaning backend..."
dotnet clean ../spending-app/backend/backend.csproj
Write-Host "Cleaning backend tests..."
dotnet clean ../spending-app/backend.Tests/backend.Tests.csproj

# Clean frontend (remove dist and node_modules)
Write-Host "Cleaning frontend..."
Push-Location ../spending-app/frontend
if (Test-Path dist) { Remove-Item -Recurse -Force dist }
if (Test-Path node_modules) { Remove-Item -Recurse -Force node_modules }
Pop-Location

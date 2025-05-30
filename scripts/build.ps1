# Build backend and test projects
Write-Host "Building backend..."
dotnet build ../spending-app/backend/backend.csproj
Write-Host "Building backend tests..."
dotnet build ../spending-app/backend.Tests/backend.Tests.csproj

# Build frontend (Vite/React)
Write-Host "Building frontend..."
Push-Location ../spending-app/frontend
if (Test-Path package.json) {
    if (!(Test-Path node_modules)) {
        Write-Host "Installing frontend dependencies..."
        npm install
    }
    npm run build
}
Pop-Location

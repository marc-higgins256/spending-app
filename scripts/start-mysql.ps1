# PowerShell script to start the MySQL service on Windows
# This script requires administrative privileges

$serviceName = "MySQL80"  # Change this if your MySQL service has a different name

# Check if the service exists
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($null -eq $service) {
    Write-Host "MySQL service '$serviceName' not found. Please check the service name."
    exit 1
}

# Start the service if it's not already running
if ($service.Status -ne 'Running') {
    Start-Service -Name $serviceName
    Write-Host "MySQL service '$serviceName' started."
} else {
    Write-Host "MySQL service '$serviceName' is already running."
}

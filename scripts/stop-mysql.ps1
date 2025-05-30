# PowerShell script to stop the MySQL service on Windows
# This script requires administrative privileges

$serviceName = "MySQL80"  # Change this if your MySQL service has a different name

# Check if the service exists
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($null -eq $service) {
    Write-Host "MySQL service '$serviceName' not found. Please check the service name."
    exit 1
}

# Stop the service if it's running
if ($service.Status -eq 'Running') {
    Stop-Service -Name $serviceName
    Write-Host "MySQL service '$serviceName' stopped."
} else {
    Write-Host "MySQL service '$serviceName' is not running."
}

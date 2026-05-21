# PowerShell script to remove local SQLite DB used for development.
param()
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Join-Path $scriptDir '..'
$dbFile = Join-Path $rootDir 'bookquotes.db'
if (Test-Path $dbFile) {
    Write-Host "Removing $dbFile"
    Remove-Item -Force $dbFile
    Write-Host "Removed. Run: $env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run --project $rootDir\BookQuotes.Api"
} else {
    Write-Host "$dbFile not found. Nothing to remove."
}

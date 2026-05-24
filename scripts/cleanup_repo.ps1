# Cleanup repository: deletes local build artifacts and developer files
# Run from repository root in PowerShell (Windows) or PowerShell Core on other OS.

$pathsToRemove = @(
    ".vs",
    "**/bin",
    "**/obj",
    "bookquotes-ui/node_modules",
    "bookquotes-ui/dist",
    "bookquotes-ui/.angular",
    "BookQuotes.Api/bookquotes.db",
    "BookQuotes.Api/bookquotes.db-shm",
    "BookQuotes.Api/bookquotes.db-wal",
    "build_output.txt"
)

Write-Host "Cleaning up repository (this removes files/directories listed)."
foreach ($p in $pathsToRemove) {
    Write-Host "Removing pattern: $p"
    Get-ChildItem -Path $p -Recurse -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Cleanup finished. You may want to run: git status to review changes, then commit or discard as needed."
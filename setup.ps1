# Setup script for ark-gacha-bot-asa using uv

# Check if uv is installed
Write-Host "Checking for uv..." -ForegroundColor Cyan
try {
    $uvVersion = & uv --version 2>&1
    Write-Host "uv is installed: $uvVersion" -ForegroundColor Green
} catch {
    Write-Host "uv is not installed. Installing uv..." -ForegroundColor Yellow
    powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
    
    # Refresh PATH
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    
    Write-Host "uv installed successfully!" -ForegroundColor Green
}

# Sync dependencies
Write-Host "Syncing dependencies from pyproject.toml..." -ForegroundColor Cyan
uv sync

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to sync dependencies. Please check pyproject.toml" -ForegroundColor Red
    exit 1
}

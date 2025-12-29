param(
    [switch]$SelfUpdate
)

# Check if uv is installed
Write-Host "Checking for uv..." -ForegroundColor Cyan
try {
    $uvVersion = & uv --version 2>&1
    Write-Host "uv is installed: $uvVersion" -ForegroundColor Green
} catch {
    Write-Host "uv is not installed. Installing uv..." -ForegroundColor Yellow
    powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
    Write-Host "Please restart your terminal to use uv, then run this script again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

if ($SelfUpdate) {
    # Self-update logic
    Write-Host "Performing self-update..." -ForegroundColor Cyan
    git pull origin main
    git pull
    Write-Host "Restarting script after self-update..." -ForegroundColor Cyan
    & powershell -NoProfile -ExecutionPolicy Bypass -File $MyInvocation.MyCommand.Path
    exit
}

# Sync dependencies using uv
Write-Host "Syncing dependencies from pyproject.toml..." -ForegroundColor Cyan
uv sync
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to sync dependencies." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Run the main Python script
Write-Host "Running main.py..." -ForegroundColor Cyan
uv run python main.py

if ($LASTEXITCODE -ne 0) {
    Write-Host "main.py exited with errors." -ForegroundColor Red
    exit 1
}

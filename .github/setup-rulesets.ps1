# GitHub Ruleset Setup Script
# This script helps set up branch protection rulesets for your repository

Write-Host "GitHub Ruleset Setup Script" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

# Check if GitHub CLI is installed
Write-Host "Checking GitHub CLI installation..." -ForegroundColor Yellow
$ghVersion = gh --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: GitHub CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install GitHub CLI first: winget install --id GitHub.cli" -ForegroundColor Yellow
    exit 1
}
Write-Host "GitHub CLI is installed: $ghVersion" -ForegroundColor Green
Write-Host ""

# Check authentication
Write-Host "Checking GitHub authentication..." -ForegroundColor Yellow
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "You are not authenticated with GitHub." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please run: gh auth login" -ForegroundColor Cyan
    Write-Host "This will open a browser for authentication." -ForegroundColor Cyan
    Write-Host ""
    $continue = Read-Host "Would you like to authenticate now? (y/n)"
    if ($continue -eq "y" -or $continue -eq "Y") {
        Write-Host "Starting authentication..." -ForegroundColor Yellow
        gh auth login
    } else {
        Write-Host "Please authenticate manually and run this script again." -ForegroundColor Yellow
        exit 0
    }
} else {
    Write-Host "You are authenticated with GitHub." -ForegroundColor Green
    Write-Host $authStatus
}
Write-Host ""

# Get repository info
$repoInfo = gh repo view --json owner,name
$owner = ($repoInfo | ConvertFrom-Json).owner.login
$repo = ($repoInfo | ConvertFrom-Json).name
Write-Host "Repository: $owner/$repo" -ForegroundColor Green
Write-Host ""

# Ask user if they want to create the ruleset
Write-Host "This script will create a branch protection ruleset for:" -ForegroundColor Cyan
Write-Host "  - main branch" -ForegroundColor Cyan
Write-Host "  - FirstCommit-code branch" -ForegroundColor Cyan
Write-Host ""
Write-Host "Rules that will be enforced:" -ForegroundColor Cyan
Write-Host "  - Require pull request before merging" -ForegroundColor White
Write-Host "  - Require at least 1 approval" -ForegroundColor White
Write-Host "  - Dismiss stale reviews on new commits" -ForegroundColor White
Write-Host "  - Require linear history (no merge commits)" -ForegroundColor White
Write-Host "  - Block force pushes" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Do you want to create this ruleset? (y/n)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

# Create the ruleset
Write-Host ""
Write-Host "Creating ruleset..." -ForegroundColor Yellow
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$jsonPath = Join-Path $scriptDir "branch-protection-ruleset.json"
if (-not (Test-Path $jsonPath)) {
    # Fallback: try relative path from current directory
    $jsonPath = ".github/branch-protection-ruleset.json"
}
$response = gh api "repos/$owner/$repo/rulesets" --method POST --input $jsonPath 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "Ruleset created successfully!" -ForegroundColor Green
    Write-Host $response
    Write-Host ""
    Write-Host "Note: Rulesets won't be enforced on private repositories" -ForegroundColor Yellow
    Write-Host "until you move to a GitHub Team organization account." -ForegroundColor Yellow
} else {
    Write-Host "Error creating ruleset:" -ForegroundColor Red
    Write-Host $response
    Write-Host ""
    Write-Host "You can also create the ruleset manually via:" -ForegroundColor Yellow
    Write-Host "  https://github.com/$owner/$repo/settings/rules" -ForegroundColor Cyan
}


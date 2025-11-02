# Script to identify SonarQube status checks in the repository
# This helps find the exact name of SonarQube status checks to add to rulesets

Write-Host "Identifying SonarQube Status Checks" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Refresh PATH
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Get repository info
$repoInfo = gh repo view --json owner,name 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Could not get repository info. Make sure you're authenticated." -ForegroundColor Red
    exit 1
}

$owner = ($repoInfo | ConvertFrom-Json).owner.login
$repo = ($repoInfo | ConvertFrom-Json).name

Write-Host "Repository: $owner/$repo" -ForegroundColor Green
Write-Host ""

# Get recent commits to check for status checks
Write-Host "Checking recent commits for status checks..." -ForegroundColor Yellow
$commitsJson = gh api "repos/$owner/$repo/commits?per_page=5" --jq '.[].sha' 2>$null
if ($commitsJson) {
    $commits = $commitsJson | ConvertFrom-Json
} else {
    $commits = @()
}

$sonarChecks = @()
foreach ($commit in $commits) {
    if ($commit) {
        $statuses = gh api "repos/$owner/$repo/commits/$commit/statuses" --jq '.[] | select(.context | contains("Sonar") or contains("sonar") or test("(?i)sonar")) | .context' 2>$null
        if ($statuses) {
            $statusesArray = $statuses | ConvertFrom-Json -ErrorAction SilentlyContinue
            if ($statusesArray) {
                $sonarChecks += $statusesArray
            } else {
                $sonarChecks += $statuses
            }
        }
        
        # Also check check-runs (requires checks:read permission)
        $checkRuns = gh api "repos/$owner/$repo/commits/$commit/check-runs" --jq '.check_runs[] | select(.name | contains("Sonar") or contains("sonar") or test("(?i)sonar")) | .name' 2>$null
        if ($checkRuns) {
            $checkRunsArray = $checkRuns | ConvertFrom-Json -ErrorAction SilentlyContinue
            if ($checkRunsArray) {
                $sonarChecks += $checkRunsArray
            } else {
                $sonarChecks += $checkRuns
            }
        }
    }
}

# Get all unique check names
$uniqueChecks = $sonarChecks | Select-Object -Unique

if ($uniqueChecks) {
    Write-Host "Found SonarQube status checks:" -ForegroundColor Green
    foreach ($check in $uniqueChecks) {
        Write-Host "  - $check" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "Use these names in your ruleset configuration." -ForegroundColor Cyan
} else {
    Write-Host "No SonarQube status checks found in recent commits." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Common SonarQube status check names:" -ForegroundColor Cyan
    Write-Host "  - SonarQube" -ForegroundColor White
    Write-Host "  - SonarQube Code Analysis" -ForegroundColor White
    Write-Host "  - sonarqube/analysis" -ForegroundColor White
    Write-Host "  - SonarCloud Code Analysis" -ForegroundColor White
    Write-Host ""
    Write-Host "To find the exact name:" -ForegroundColor Yellow
    Write-Host "1. Create a test PR and run SonarQube analysis" -ForegroundColor White
    Write-Host "2. Check the PR status checks section" -ForegroundColor White
    Write-Host "3. Use that exact name in the ruleset" -ForegroundColor White
}


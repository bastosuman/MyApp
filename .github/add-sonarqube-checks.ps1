# Script to add SonarQube status checks to the existing ruleset
# Usage: .github\add-sonarqube-checks.ps1 -CheckName "SonarQube Code Analysis"

param(
    [Parameter(Mandatory=$true)]
    [string[]]$CheckName,
    
    [Parameter(Mandatory=$false)]
    [int]$RulesetId = 9376762
)

Write-Host "Adding SonarQube Checks to Ruleset" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Refresh PATH
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Get current ruleset
Write-Host "Fetching current ruleset..." -ForegroundColor Yellow
$currentRuleset = gh api repos/bastosuman/MyApp/rulesets/$RulesetId | ConvertFrom-Json

# Create updated ruleset
$updatedRuleset = @{
    name = $currentRuleset.name
    target = $currentRuleset.target
    enforcement = $currentRuleset.enforcement
    conditions = $currentRuleset.conditions
    rules = @()
}

# Add existing rules
foreach ($rule in $currentRuleset.rules) {
    # Skip existing required_status_checks rule if updating
    if ($rule.type -ne "required_status_checks") {
        $updatedRuleset.rules += $rule
    }
}

# Add required_status_checks rule with SonarQube checks
$statusCheckRule = @{
    type = "required_status_checks"
    parameters = @{
        strict_required_status_checks_policy = $true
        required_status_checks = $CheckName
    }
}
$updatedRuleset.rules += $statusCheckRule

# Save to temporary file
$tempFile = "temp-ruleset-update.json"
$updatedRuleset | ConvertTo-Json -Depth 10 | Out-File -FilePath $tempFile -Encoding utf8

Write-Host "Status checks to require:" -ForegroundColor Green
foreach ($check in $CheckName) {
    Write-Host "  - $check" -ForegroundColor White
}
Write-Host ""

# Confirm
$confirm = Read-Host "Update ruleset with these SonarQube checks? (y/n)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "Aborted." -ForegroundColor Yellow
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    exit 0
}

# Update ruleset
Write-Host ""
Write-Host "Updating ruleset..." -ForegroundColor Yellow
$response = gh api repos/bastosuman/MyApp/rulesets/$RulesetId --method PUT --input $tempFile 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "Ruleset updated successfully!" -ForegroundColor Green
    $updatedRulesetInfo = $response | ConvertFrom-Json
    Write-Host ""
    Write-Host "Ruleset ID: $($updatedRulesetInfo.id)" -ForegroundColor Cyan
    Write-Host "View at: https://github.com/bastosuman/MyApp/settings/rules/$($updatedRulesetInfo.id)" -ForegroundColor Cyan
} else {
    Write-Host "Error updating ruleset:" -ForegroundColor Red
    Write-Host $response
}

# Cleanup
Remove-Item $tempFile -ErrorAction SilentlyContinue


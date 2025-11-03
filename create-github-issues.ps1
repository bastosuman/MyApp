# PowerShell script to create GitHub issues
# Requires GitHub Personal Access Token with 'repo' scope

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory=$false)]
    [string]$Owner = "bastosuman",
    
    [Parameter(Mandatory=$false)]
    [string]$Repo = "MyApp"
)

$baseUrl = "https://api.github.com/repos/$Owner/$Repo/issues"

# Headers for GitHub API
$headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept" = "application/vnd.github.v3+json"
    "User-Agent" = "PowerShell-Script"
}

function Create-Issue {
    param(
        [string]$Title,
        [string]$Body,
        [string[]]$Labels
    )
    
    $issueData = @{
        title = $Title
        body = $Body
        labels = $Labels
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Headers $headers -Body $issueData -ContentType "application/json"
        Write-Host "✅ Created issue: $Title (Issue #$($response.number))" -ForegroundColor Green
        Write-Host "   URL: $($response.html_url)" -ForegroundColor Cyan
        return $response
    }
    catch {
        Write-Host "❌ Failed to create issue: $Title" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "   Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
        return $null
    }
}

Write-Host "🚀 Creating GitHub Issues..." -ForegroundColor Yellow
Write-Host ""

# Issue 1: Database Setup
$issue1Body = Get-Content -Path ".github\issues\issue-1-database-setup.md" -Raw
Create-Issue -Title "Database Setup & Migrations" -Body $issue1Body -Labels @("enhancement", "database", "backend")

Start-Sleep -Seconds 1

# Issue 2: Accounts API
$issue2Body = Get-Content -Path ".github\issues\issue-2-accounts-api.md" -Raw
Create-Issue -Title "Account Management API" -Body $issue2Body -Labels @("enhancement", "api", "backend")

Start-Sleep -Seconds 1

# Issue 3: Transactions API
$issue3Body = Get-Content -Path ".github\issues\issue-3-transactions-api.md" -Raw
Create-Issue -Title "Transaction Management API" -Body $issue3Body -Labels @("enhancement", "api", "backend")

Start-Sleep -Seconds 1

# Issue 4: Applications & Products API
$issue4Body = Get-Content -Path ".github\issues\issue-4-applications-products-api.md" -Raw
Create-Issue -Title "Applications & Products Management API" -Body $issue4Body -Labels @("enhancement", "api", "backend")

Start-Sleep -Seconds 1

# Issue 5: Frontend Integration
$issue5Body = Get-Content -Path ".github\issues\issue-5-frontend-integration.md" -Raw
Create-Issue -Title "Frontend Integration - Connect BankUI to Backend APIs" -Body $issue5Body -Labels @("enhancement", "frontend", "api", "integration")

Write-Host ""
Write-Host "✨ Done! All issues created." -ForegroundColor Green


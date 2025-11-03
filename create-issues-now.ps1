# Interactive script to create GitHub issues directly
Write-Host "🚀 GitHub Issues Creator" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Check if we have a token stored
$tokenFile = "$env:USERPROFILE\.github-token.txt"
if (Test-Path $tokenFile) {
    $storedToken = Get-Content $tokenFile -Raw | ConvertTo-SecureString
    $useStored = Read-Host "Found stored token. Use it? (y/n)"
    if ($useStored -eq "y" -or $useStored -eq "Y") {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($storedToken)
        $GitHubToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    } else {
        $GitHubToken = Read-Host "Enter your GitHub Personal Access Token" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($GitHubToken)
        $plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        $plainToken | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString | Set-Content $tokenFile
        $GitHubToken = $plainToken
    }
} else {
    Write-Host "📝 To create issues, you need a GitHub Personal Access Token" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Quick steps to get a token:" -ForegroundColor Cyan
    Write-Host "1. Go to: https://github.com/settings/tokens" -ForegroundColor White
    Write-Host "2. Click 'Generate new token (classic)'" -ForegroundColor White
    Write-Host "3. Name it: 'Issue Creator'" -ForegroundColor White
    Write-Host "4. Select scope: 'repo' (Full control)" -ForegroundColor White
    Write-Host "5. Click 'Generate token' and copy it" -ForegroundColor White
    Write-Host ""
    
    $GitHubToken = Read-Host "Paste your token here" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($GitHubToken)
    $plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    $plainToken | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString | Set-Content $tokenFile
    $GitHubToken = $plainToken
}

$Owner = "bastosuman"
$Repo = "MyApp"
$baseUrl = "https://api.github.com/repos/$Owner/$Repo/issues"

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
        Write-Host "✅ Created: $Title" -ForegroundColor Green
        Write-Host "   🔗 $($response.html_url)" -ForegroundColor Cyan
        return $response
    }
    catch {
        Write-Host "❌ Failed: $Title" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host "   Details: $($errorDetails.message)" -ForegroundColor Red
        }
        return $null
    }
}

Write-Host ""
Write-Host "📋 Creating 5 issues..." -ForegroundColor Yellow
Write-Host ""

# Issue 1
Write-Host "[1/5] Creating Database Setup issue..." -ForegroundColor Cyan
$issue1Body = Get-Content -Path ".github\issues\issue-1-database-setup.md" -Raw
Create-Issue -Title "Database Setup & Migrations" -Body $issue1Body -Labels @("enhancement", "database", "backend") | Out-Null
Start-Sleep -Seconds 1

# Issue 2
Write-Host "[2/5] Creating Account Management API issue..." -ForegroundColor Cyan
$issue2Body = Get-Content -Path ".github\issues\issue-2-accounts-api.md" -Raw
Create-Issue -Title "Account Management API" -Body $issue2Body -Labels @("enhancement", "api", "backend") | Out-Null
Start-Sleep -Seconds 1

# Issue 3
Write-Host "[3/5] Creating Transaction Management API issue..." -ForegroundColor Cyan
$issue3Body = Get-Content -Path ".github\issues\issue-3-transactions-api.md" -Raw
Create-Issue -Title "Transaction Management API" -Body $issue3Body -Labels @("enhancement", "api", "backend") | Out-Null
Start-Sleep -Seconds 1

# Issue 4
Write-Host "[4/5] Creating Applications & Products API issue..." -ForegroundColor Cyan
$issue4Body = Get-Content -Path ".github\issues\issue-4-applications-products-api.md" -Raw
Create-Issue -Title "Applications & Products Management API" -Body $issue4Body -Labels @("enhancement", "api", "backend") | Out-Null
Start-Sleep -Seconds 1

# Issue 5
Write-Host "[5/5] Creating Frontend Integration issue..." -ForegroundColor Cyan
$issue5Body = Get-Content -Path ".github\issues\issue-5-frontend-integration.md" -Raw
Create-Issue -Title "Frontend Integration - Connect BankUI to Backend APIs" -Body $issue5Body -Labels @("enhancement", "frontend", "api", "integration") | Out-Null

Write-Host ""
Write-Host "✨ All done! Check your issues at:" -ForegroundColor Green
Write-Host "   https://github.com/bastosuman/MyApp/issues" -ForegroundColor Cyan
Write-Host ""



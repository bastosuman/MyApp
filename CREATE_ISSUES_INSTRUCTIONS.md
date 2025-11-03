# How to Create GitHub Issues

The issues I created are **local files only** - they need to be added to GitHub. You have two options:

## Option 1: Use PowerShell Script (Automated) ✅ Recommended

I've created a PowerShell script that will automatically create all 5 issues on GitHub.

### Step 1: Get GitHub Personal Access Token

1. Go to: https://github.com/settings/tokens
2. Click **"Generate new token"** → **"Generate new token (classic)"**
3. Give it a name: `Issue Creator Script`
4. Select scope: **`repo`** (Full control of private repositories)
5. Click **"Generate token"**
6. **Copy the token immediately** (you won't see it again!)

### Step 2: Run the Script

Open PowerShell in your project directory and run:

```powershell
.\create-github-issues.ps1 -GitHubToken "your_token_here"
```

Replace `your_token_here` with the token you copied.

The script will:
- ✅ Create all 5 issues automatically
- ✅ Add appropriate labels
- ✅ Show you the URLs of created issues

## Option 2: Manual Copy-Paste (If script doesn't work)

1. Go to: https://github.com/bastosuman/MyApp/issues
2. Click **"New issue"**
3. Open `.github/issues/issue-1-database-setup.md` in your editor
4. Copy everything and paste into GitHub
5. Add labels: `enhancement`, `database`, `backend`
6. Click **"Submit new issue"**
7. Repeat for issues 2-5

## Issue Files Location

All issue files are in: `.github/issues/`
- `issue-1-database-setup.md`
- `issue-2-accounts-api.md`
- `issue-3-transactions-api.md`
- `issue-4-applications-products-api.md`
- `issue-5-frontend-integration.md`

## Quick Command (After getting token)

```powershell
$token = Read-Host "Enter your GitHub token"
.\create-github-issues.ps1 -GitHubToken $token
```

This will prompt you to enter the token securely.



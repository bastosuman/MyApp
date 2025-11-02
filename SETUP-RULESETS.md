# Quick Setup Guide for GitHub Rulesets

## ✅ What's Been Done

1. ✅ Created `.gitignore` for .NET/Visual Studio
2. ✅ Installed GitHub CLI
3. ✅ Created ruleset configuration files
4. ✅ Committed and pushed all files to repository

## 🔐 Next Steps (Run These Commands)

### Step 1: Authenticate with GitHub

Open PowerShell and run:
```powershell
gh auth login
```

This will:
- Open your browser
- Prompt you to authorize GitHub CLI
- Complete authentication

### Step 2: Create the Ruleset

After authentication, run:
```powershell
.\.github\setup-rulesets.ps1
```

Or manually using GitHub CLI:
```powershell
gh api repos/bastosuman/MyApp/rulesets --method POST --input .github/branch-protection-ruleset.json
```

## 📋 Alternative: Web UI Setup

If you prefer the web interface:

1. Go to: https://github.com/bastosuman/MyApp/settings/rules
2. Click **"New ruleset"**
3. Select **"Branch"** protection
4. Configure:
   - **Target branches**: `main`, `FirstCommit-code`
   - **Rules to enable**:
     - ✅ Require pull request before merging
     - ✅ Require approvals (at least 1)
     - ✅ Dismiss stale reviews
     - ✅ Require linear history
     - ✅ Block force pushes
     - ✅ Block deletions
5. Click **"Create ruleset"**

## ⚠️ Important Note

Rulesets won't be **enforced** on private repositories until you move to a **GitHub Team organization account**. However, you can create them now and they'll be ready to activate once you upgrade.

## 📚 Documentation

For detailed information, see:
- `.github/README.md` - Quick reference
- `.github/ruleset-configuration.md` - Comprehensive guide


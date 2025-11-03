# GitHub Configuration Files

This directory contains configuration files for GitHub repository settings and rulesets.

## Files

- **`ruleset-configuration.md`** - Comprehensive guide on setting up rulesets
- **`branch-protection-ruleset.json`** - JSON configuration for branch protection ruleset
- **`setup-rulesets.ps1`** - PowerShell script to automatically set up rulesets
- **`sonarqube-ruleset-guide.md`** - Guide for configuring SonarQube code coverage rules
- **`QUICK-START-SONARQUBE.md`** - Quick start guide for SonarQube integration
- **`identify-sonarqube-checks.ps1`** - Script to identify SonarQube status check names
- **`add-sonarqube-checks.ps1`** - Script to add SonarQube checks to ruleset

## Quick Start

### Option 1: Use the Setup Script (Recommended)

1. **Authenticate with GitHub:**
   ```powershell
   gh auth login
   ```

2. **Run the setup script:**
   ```powershell
   .\.github\setup-rulesets.ps1
   ```

The script will guide you through the process of creating branch protection rulesets.

### Option 2: Manual Setup via GitHub Web Interface

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Rules** → **Rulesets**
3. Click **New ruleset**
4. Select **Branch** protection
5. Configure rules as described in `ruleset-configuration.md`
6. Save the ruleset

### Option 3: Use GitHub CLI Manually

1. **Authenticate:**
   ```powershell
   gh auth login
   ```

2. **Create ruleset using the JSON file:**
   ```powershell
   gh api repos/bastosuman/MyApp/rulesets `
     --method POST `
     --input .github/branch-protection-ruleset.json
   ```

## Important Note

⚠️ **Rulesets won't be enforced on private repositories until the repository is moved to a GitHub Team organization account.**

Even if you create the rulesets now, they will not be active until you upgrade to GitHub Team.

## What Gets Protected

The default ruleset protects:
- `main` branch
- `FirstCommit-code` branch

With these rules:
- ✅ Require pull requests before merging
- ✅ Require at least 1 approval
- ✅ Dismiss stale reviews on new commits
- ✅ Require linear history (no merge commits)
- ✅ Block force pushes
- ✅ Block branch deletions

For more details and customization options, see `ruleset-configuration.md`.

## SonarQube Code Coverage

To enforce SonarQube code coverage requirements:

1. **Quick Start:** See `QUICK-START-SONARQUBE.md` for step-by-step instructions
2. **Detailed Guide:** See `sonarqube-ruleset-guide.md` for comprehensive documentation
3. **Identify Checks:** Run `.\.github\identify-sonarqube-checks.ps1` to find SonarQube check names
4. **Add Checks:** Use `.\.github\add-sonarqube-checks.ps1` to add checks to ruleset (after identifying names)

**Current Ruleset:** https://github.com/bastosuman/MyApp/settings/rules/9376762


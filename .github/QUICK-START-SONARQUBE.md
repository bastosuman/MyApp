# Quick Start: Add SonarQube Code Coverage Rules

## Step-by-Step Instructions

Since SonarQube is already linked to your GitHub repository, follow these steps to enforce code coverage checks:

### Step 1: Identify Your SonarQube Status Check Name

**Option A: Create a Test PR**
1. Create a new branch: `git checkout -b test-sonarqube`
2. Make a small change and commit
3. Push and create a PR
4. Once SonarQube runs, check the PR's "Checks" tab
5. Note the exact name of the SonarQube check (e.g., "SonarQube", "SonarQube Code Analysis")

**Option B: Use the Helper Script**
```powershell
.\.github\identify-sonarqube-checks.ps1
```

**Option C: Check Recent Commits**
```powershell
gh api repos/bastosuman/MyApp/commits/HEAD/check-runs --jq '.check_runs[].name'
gh api repos/bastosuman/MyApp/commits/HEAD/statuses --jq '.[].context'
```

### Step 2: Add SonarQube Check via GitHub Web UI (Recommended)

1. **Open your ruleset:**
   - Go to: https://github.com/bastosuman/MyApp/settings/rules/9376762
   - Or navigate: Settings → Rules → Rulesets → "Main Branch Protection"

2. **Add Status Checks:**
   - Scroll to the "Status checks that must pass" section
   - Click "Add status check"
   - Search for your SonarQube check name
   - Select it from the list
   - Repeat if you have multiple SonarQube checks

3. **Configure Options:**
   - ✅ Enable "Require branches to be up to date before merging"
   - This ensures the latest code is analyzed

4. **Save:**
   - Click "Save changes" at the bottom

### Step 3: Configure SonarQube Quality Gate (In SonarQube/SonarCloud)

1. **Login to SonarQube/SonarCloud**
2. **Go to Quality Gates**
3. **Set Code Coverage Thresholds:**
   - Coverage on New Code > 75% (adjust as needed)
   - Overall Coverage > 70% (adjust as needed)
4. **Save and set as default**

The Quality Gate status will automatically appear as a GitHub status check when SonarQube analyzes your code.

### Step 4: Test the Configuration

1. Create a test PR
2. Verify that:
   - SonarQube analysis runs automatically
   - The PR cannot be merged until SonarQube checks pass
   - If coverage is below threshold, the Quality Gate fails and blocks merging

## Alternative: Use PowerShell Script (After Step 1)

Once you know your SonarQube check name:

```powershell
# Replace "SonarQube" with your actual check name
.\.github\add-sonarqube-checks.ps1 -CheckName "SonarQube", "SonarQube Code Analysis"
```

## What Gets Enforced

After configuration:
- ✅ All SonarQube checks must pass before merging
- ✅ Code coverage must meet Quality Gate thresholds
- ✅ Branches must be up to date before merging
- ✅ PRs with failing SonarQube analysis cannot be merged

## Troubleshooting

**Check name not found in dropdown:**
- The status check must have run at least once
- Create a test PR and run SonarQube analysis first

**Quality Gate not appearing:**
- Verify SonarQube webhook is configured
- Check GitHub integration settings in SonarQube
- Ensure analysis runs on pull requests

## Need Help?

See detailed guide: `.github/sonarqube-ruleset-guide.md`


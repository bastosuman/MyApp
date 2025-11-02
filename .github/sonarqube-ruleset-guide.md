# SonarQube Code Coverage Ruleset Configuration

This guide explains how to configure GitHub rulesets to require SonarQube code coverage checks before merging pull requests.

## Prerequisites

- SonarQube is linked to your GitHub repository
- SonarQube analysis runs on pull requests (via GitHub Actions or webhook)
- Status checks from SonarQube appear in GitHub

## Finding Your SonarQube Status Check Name

The exact name of the SonarQube status check varies based on your setup. Common names include:
- `SonarQube`
- `SonarQube Code Analysis`
- `SonarCloud Code Analysis`
- `sonarqube/analysis`
- `sonarcloud/analysis`

### Method 1: Using the Helper Script

Run the identification script:
```powershell
.\.github\identify-sonarqube-checks.ps1
```

### Method 2: Check a Pull Request

1. Open any pull request in your repository
2. Scroll to the "Checks" section
3. Look for SonarQube-related checks
4. Note the exact name(s) shown

### Method 3: Check via GitHub CLI

```powershell
# Get status checks from recent commits
gh api repos/bastosuman/MyApp/commits/HEAD/statuses --jq '.[].context'
gh api repos/bastosuman/MyApp/commits/HEAD/check-runs --jq '.check_runs[].name'
```

## Adding SonarQube Checks to Ruleset

### Option 1: Update Existing Ruleset via API

Once you know the exact SonarQube check name(s), update the ruleset:

```powershell
# Update the JSON file with your SonarQube check name(s)
# Edit .github/branch-protection-ruleset-with-sonarqube.json
# Replace "SonarQube" and "SonarQube Code Analysis" with your actual check names

# Then update the ruleset
gh api repos/bastosuman/MyApp/rulesets/9376762 --method PUT --input .github/branch-protection-ruleset-with-sonarqube.json
```

### Option 2: Update via GitHub Web UI

1. Go to: https://github.com/bastosuman/MyApp/settings/rules/9376762
2. Scroll to "Status checks that must pass"
3. Click "Add status check"
4. Search for and select your SonarQube check name(s)
5. Click "Save changes"

## Example Configuration

Here's an example of the ruleset JSON with SonarQube status checks:

```json
{
  "name": "Main Branch Protection",
  "target": "branch",
  "enforcement": "active",
  "conditions": {
    "ref_name": {
      "include": ["refs/heads/main", "refs/heads/FirstCommit-code"],
      "exclude": []
    }
  },
  "rules": [
    {
      "type": "non_fast_forward"
    },
    {
      "type": "deletion"
    },
    {
      "type": "required_linear_history"
    },
    {
      "type": "required_status_checks",
      "parameters": {
        "strict_required_status_checks_policy": true,
        "required_status_checks": [
          "YOUR_SONARQUBE_CHECK_NAME_HERE"
        ]
      }
    }
  ]
}
```

## What This Enforces

When configured correctly, the ruleset will:
- ✅ Block merging PRs if SonarQube analysis fails
- ✅ Block merging PRs if code coverage is below threshold (configured in SonarQube Quality Gate)
- ✅ Require all status checks to pass before allowing merge
- ✅ Require branches to be up to date with the base branch

## SonarQube Quality Gate Configuration

To enforce code coverage requirements:

1. **In SonarQube/SonarCloud:**
   - Go to Quality Gates
   - Configure coverage thresholds:
     - Coverage on New Code > 75% (or your preferred threshold)
     - Overall Coverage > 70%
   - Set the Quality Gate as default

2. **The Quality Gate status** will automatically become a GitHub status check if SonarQube is properly integrated.

## Troubleshooting

### Status Check Not Appearing

- Ensure SonarQube webhook is configured correctly
- Verify GitHub integration in SonarQube settings
- Check that SonarQube analysis runs on pull requests

### Ruleset Not Enforcing Checks

- Verify the exact status check name matches (case-sensitive)
- Check that the ruleset is active and applies to your branch
- Ensure the repository is public or you have GitHub Pro/Team

### Quality Gate Failing

- Review SonarQube Quality Gate conditions
- Check coverage reports are being generated
- Verify test coverage thresholds in SonarQube

## Next Steps

1. Identify your SonarQube status check name using one of the methods above
2. Update the ruleset with the correct check name(s)
3. Test by creating a test PR and verifying the check is required
4. Adjust code coverage thresholds in SonarQube as needed


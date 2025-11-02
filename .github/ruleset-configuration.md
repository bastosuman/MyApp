# GitHub Ruleset Configuration Guide

## Overview
This document outlines recommended ruleset configurations for the MyApp repository to protect important branches and enforce quality standards.

## Important Note
Rulesets won't be enforced on private repositories until the repository is moved to a GitHub Team organization account.

## Recommended Rulesets

### 1. Branch Protection Ruleset (for `main` and `FirstCommit-code` branches)

#### Target Branches
- `main`
- `FirstCommit-code`

#### Rules

**General Protection:**
- ✅ **Require pull request before merging** - All changes must go through PR
- ✅ **Require approvals** - At least 1 approval required
- ✅ **Dismiss stale pull request approvals when new commits are pushed** - Ensures reviews stay current
- ✅ **Require review from Code Owners** - Enforces code owner reviews if CODEOWNERS file exists

**Merge Protection:**
- ✅ **Require linear history** - Prevents merge commits, enforces rebase/squash
- ✅ **Require status checks to pass before merging** - All CI/CD checks must pass
- ✅ **Require branches to be up to date before merging** - Must sync with base branch

**Push Protection:**
- ❌ **Block force pushes** - Prevents force pushes to protected branches
- ❌ **Block deletions** - Prevents branch deletion

**Status Checks:**
- Configure specific status checks if you have CI/CD pipelines:
  - Build checks
  - Test execution
  - Code quality scans
  - Security scans

### 2. Additional Branch Rules (for feature branches)

#### Target Branches
- `feature/*`
- `bugfix/*`
- `hotfix/*`

#### Rules
- ✅ **Require pull request reviews**
- ✅ **Require linear history**
- ⚠️ **Allow force pushes** (optional - for rebasing feature branches)
- ⚠️ **Allow deletions** (optional)

### 3. Tag Protection

#### Rules
- ❌ **Block deletions** - Protect important tags (e.g., release tags)
- ✅ **Require signed commits** (optional - for enhanced security)

## Implementation Steps

### Via GitHub Web Interface:

1. Navigate to your repository on GitHub
2. Go to **Settings** → **Rules** → **Rulesets**
3. Click **New ruleset**
4. Select **Branch** or **Tag** protection
5. Configure the rules as outlined above
6. Save the ruleset

### Via GitHub CLI (if installed):

```bash
# Authenticate first
gh auth login

# Create a branch protection ruleset (example)
gh api repos/bastosuman/MyApp/rulesets \
  -X POST \
  -f name="Main Branch Protection" \
  -f target="branch" \
  -f enforcement_level="active" \
  -f conditions='{"ref_name":{"include":["refs/heads/main","refs/heads/FirstCommit-code"]}}' \
  -f rules='[
    {"type":"pull_request","parameters":{"required_approving_review_count":1,"dismiss_stale_reviews_on_push":true,"require_code_owner_review":true}},
    {"type":"required_status_checks","parameters":{"strict_required_status_checks_policy":true}},
    {"type":"required_linear_history":{}},
    {"type":"non_fast_forward":{}}
  ]'
```

## Recommended Status Checks

If you set up CI/CD, consider requiring these checks:

1. **Build** - Ensure the project compiles
2. **Tests** - Run unit/integration tests
3. **Code Quality** - Linting/static analysis
4. **Security** - Vulnerability scanning

## Code Owners (Optional Enhancement)

Create `.github/CODEOWNERS` file to automatically request reviews from specific team members:

```
# Global owners
* @bastosuman

# App-specific
/MyApp/Controllers/ @backend-team
/MyApp/Program.cs @backend-team
```

## Security Recommendations

1. **Enable branch protection** on all production branches
2. **Require signed commits** for critical branches
3. **Set up required status checks** for CI/CD
4. **Use CODEOWNERS** for automatic reviewer assignment
5. **Enable GitHub Actions** for automated testing and checks

## Next Steps

1. Set up CI/CD pipeline (GitHub Actions) if not already done
2. Create status checks configuration
3. Configure rulesets via GitHub web interface
4. Test ruleset enforcement on a test branch
5. Update this document as rules evolve


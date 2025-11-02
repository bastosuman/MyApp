# GitHub Actions Workflows

## SonarQube Analysis Workflow

The `build.yml` workflow runs SonarQube code analysis on pushes and pull requests.

### Prerequisites

1. **SonarQube Token**: You need to add a `SONAR_TOKEN` secret to your repository
   - Go to: https://github.com/bastosuman/MyApp/settings/secrets/actions
   - Click "New repository secret"
   - Name: `SONAR_TOKEN`
   - Value: Your SonarQube token (get it from SonarCloud.io)
   - Click "Add secret"

2. **SonarQube Project**: Ensure your SonarQube project key matches `bastosuman_MyApp`
   - The organization key is: `bastosuman`
   - If your project key is different, update it in `build.yml`

### Workflow Features

- ✅ Runs on pushes to `main` and `FirstCommit-code` branches
- ✅ Runs on pull requests targeting `main` and `FirstCommit-code`
- ✅ Builds .NET 8.0 project
- ✅ Runs SonarQube analysis
- ✅ Uploads Quality Gate status to GitHub
- ✅ Caches SonarQube packages and scanner for faster runs
- ✅ Conditionally runs tests with coverage if test projects exist

### Status Checks

This workflow creates the following status checks:
- `SonarQube` or `SonarCloud Code Analysis` - The main analysis check
- Quality Gate status - Passes/fails based on SonarQube Quality Gate

After the first successful run, these checks will appear and can be added to your branch protection ruleset.

### Customization

**Update Project Key:**
Edit line 61 in `build.yml`:
```yaml
${{ runner.temp }}\scanner\dotnet-sonarscanner begin /k:"YOUR_PROJECT_KEY" /o:"YOUR_ORG_KEY"
```

**Update Coverage Exclusions:**
Edit the `sonar.coverage.exclusions` parameter on line 61.

**Add Test Projects:**
If you have test projects, the workflow will automatically detect and run them with coverage collection.

### Troubleshooting

**Workflow fails with "Invalid SonarQube token":**
- Verify `SONAR_TOKEN` secret is set correctly
- Ensure token has permission to analyze the repository

**Quality Gate fails:**
- Check SonarQube Quality Gate settings
- Review coverage thresholds in SonarCloud.io
- Adjust Quality Gate conditions as needed

**Scanner not found:**
- The workflow caches the scanner - first run may be slower
- Subsequent runs should use the cached scanner


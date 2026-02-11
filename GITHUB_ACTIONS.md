# GitHub Actions CI/CD Setup

This project uses GitHub Actions for automated building, testing, and publishing to NuGet.

## Workflows

### 1. Build and Test (`.github/workflows/build.yml`)

**Triggers**: On every push to `main` and on pull requests

**What it does**:
- ✅ Builds the project
- ✅ Runs automated tests
- ✅ Creates NuGet package
- ✅ Uploads package as artifact

**Badge**:
```markdown
![Build](https://github.com/cppxaxa/zcat-tool/workflows/Build%20and%20Test/badge.svg)
```

### 2. Publish to NuGet (`.github/workflows/publish.yml`)

**Triggers**:
- When you create a GitHub Release
- Manual trigger via "workflow_dispatch"

**What it does**:
- ✅ Builds and tests
- ✅ Creates NuGet package
- ✅ Publishes to NuGet.org
- ✅ Uploads package as artifact

---

## Setup Instructions

### Recommended: Using Trusted Publishing (OIDC - No API Keys!)

This is the **most secure** method - no long-lived API keys needed!

1. **Add NuGet Username to GitHub Secrets**:
   - Go to your repo: https://github.com/cppxaxa/zcat-tool
   - Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_USER`
   - Value: Your NuGet.org username (profile name, NOT email)
   - Click "Add secret"

2. **Configure Trusted Publishing on NuGet.org** (First-time only):
   - Go to https://www.nuget.org/ and sign in
   - Upload your package manually the FIRST time (see "First Publish" section below)
   - After first publish, go to package settings
   - Enable "Trusted Publishers"
   - Add GitHub: Repository `cppxaxa/zcat-tool`, Workflow `publish.yml`

3. **Done!**
   - The workflow uses `NuGet/login@v1` to get a short-lived API key via OIDC
   - No long-lived secrets to manage
   - More secure than API keys

### Alternative: Using NuGet API Key (Traditional)

If Trusted Publishing isn't set up yet:

1. **Get NuGet API Key**:
   - Go to https://www.nuget.org/
   - Sign in → API Keys → Create
   - Name: `GitHub Actions`
   - Scopes: `Push` + `Push new packages and package versions`
   - Glob Pattern: `Zcat.Tool*`
   - Copy the API key

2. **Add to GitHub Secrets**:
   - Go to your repo: https://github.com/cppxaxa/zcat-tool
   - Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_USER`
   - Value: Your NuGet.org username
   - Click "Add secret"

3. **Temporary fallback** (if OIDC login fails):
   - Add another secret: `NUGET_API_KEY` with your API key
   - The workflow will use OIDC first, fallback to API key if needed

### First Publish (Required for Trusted Publishing)

**Important**: You must publish v1.0.0 manually the FIRST time before Trusted Publishing works.

```bash
# Local first-time publish
cd /c/L1/zcat/zcat-tool
dotnet pack -c Release

# Get a temporary API key from nuget.org (30 days)
# Then publish:
dotnet nuget push bin/Release/Zcat.Tool.1.0.0.nupkg \
  --api-key YOUR-TEMP-KEY \
  --source https://api.nuget.org/v3/index.json
```

After this first publish, configure Trusted Publishing on NuGet.org, and all future releases will be automated!

---

## How to Publish a New Version

### Method 1: Create a GitHub Release (Recommended)

1. **Update version in `Zcat.Tool.csproj`**:
   ```xml
   <Version>1.0.1</Version>
   ```

2. **Commit and push**:
   ```bash
   git add Zcat.Tool.csproj
   git commit -m "Bump version to 1.0.1"
   git push
   ```

3. **Create GitHub Release**:
   - Go to https://github.com/cppxaxa/zcat-tool/releases
   - Click "Create a new release"
   - Tag: `v1.0.1`
   - Title: `v1.0.1 - Description`
   - Description: Changelog
   - Click "Publish release"

4. **GitHub Actions will automatically**:
   - Build the project
   - Run tests
   - Create NuGet package
   - Publish to NuGet.org

5. **Wait 10-15 minutes** for NuGet indexing

### Method 2: Manual Trigger

1. Go to https://github.com/cppxaxa/zcat-tool/actions
2. Select "Publish to NuGet" workflow
3. Click "Run workflow"
4. Select branch and click "Run workflow"

**Note**: This won't publish to NuGet unless you modify the workflow condition.

---

## Monitoring

### Check Build Status

- **All workflows**: https://github.com/cppxaxa/zcat-tool/actions
- **Latest build**: See badge in README

### Check Published Package

- **NuGet.org**: https://www.nuget.org/packages/Zcat.Tool
- **Download stats**: https://www.nuget.org/stats/packages/Zcat.Tool

---

## Workflow Files Explained

### `build.yml` - Continuous Integration

```yaml
on:
  push:
    branches: [ main ]  # Run on push to main
  pull_request:         # Run on PRs
```

**Steps**:
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Build in Release mode
5. Run tests (test.sh)
6. Create NuGet package
7. Upload as artifact

### `publish.yml` - Continuous Deployment

```yaml
on:
  release:
    types: [published]  # Run when GitHub Release is created
  workflow_dispatch:    # Allow manual trigger
```

**Steps**:
1. All steps from build.yml
2. **Publish to NuGet** (only on release)
3. Upload artifacts

---

## Testing Locally Before Release

```bash
# Run the same tests GitHub Actions will run
./test.sh

# Build release package
dotnet pack -c Release

# Verify package contents
ls -lh bin/Release/*.nupkg
```

---

## Troubleshooting

### Build fails on GitHub Actions

1. Check the Actions tab: https://github.com/cppxaxa/zcat-tool/actions
2. Click on the failed run
3. Expand failed step to see error
4. Fix locally and push again

### Publish fails with "401 Unauthorized" (OIDC)

1. **First publish must be manual**:
   - You must publish v1.0.0 manually first
   - Then configure Trusted Publishing on NuGet.org
   - Future releases will work automatically

2. **Check NUGET_USER secret**:
   - Go to Settings → Secrets → Actions
   - Verify `NUGET_USER` exists and matches your NuGet.org username
   - This should be your profile name, NOT your email

3. **Check Trusted Publishing configuration on NuGet.org**:
   - Go to package settings
   - Verify "Trusted Publishers" includes:
     - Repository: `cppxaxa/zcat-tool`
     - Workflow: `publish.yml`

### NuGet/login@v1 fails

If the OIDC login step fails:
- Verify `id-token: write` permission is set in workflow
- Check that `NUGET_USER` secret is correctly set
- Ensure package exists on NuGet (manual first publish required)

### Package not appearing on NuGet

- Wait 10-15 minutes for indexing
- Check package status at https://www.nuget.org/packages/Zcat.Tool
- Verify publish step succeeded in Actions

### Tests fail on GitHub Actions but pass locally

- Line ending differences (CRLF vs LF)
- Add `.gitattributes` file:
  ```
  * text=auto
  *.sh text eol=lf
  ```

---

## Best Practices

### Versioning

Follow [Semantic Versioning](https://semver.org/):
- `1.0.0` → `1.0.1` - Bug fix
- `1.0.0` → `1.1.0` - New feature (backwards compatible)
- `1.0.0` → `2.0.0` - Breaking change

### Release Checklist

- [ ] Update version in `Zcat.Tool.csproj`
- [ ] Update changelog/release notes
- [ ] Run tests locally (`./test.sh`)
- [ ] Commit and push
- [ ] Create GitHub Release
- [ ] Wait for Actions to complete
- [ ] Verify package on NuGet.org
- [ ] Test install: `dotnet tool install --global Zcat.Tool`

### Security

- ✅ Never commit API keys
- ✅ Use GitHub Secrets for sensitive data
- ✅ Consider Trusted Publishing when available
- ✅ Review workflow runs before releases
- ✅ Use dependabot for dependency updates

---

## Additional Workflows (Optional)

### Auto-versioning

Add to publish.yml to auto-increment version:
```yaml
- name: Bump version
  run: |
    VERSION=$(cat Zcat.Tool.csproj | grep -oP '(?<=<Version>)[^<]+')
    # Logic to increment version
```

### Release Notes Generation

Use GitHub's auto-generated release notes:
- Settings → Features → "Automatically generated release notes"

### Multiple Target Frameworks

Update `.csproj`:
```xml
<TargetFrameworks>net8.0;net6.0</TargetFrameworks>
```

---

## Resources

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [NuGet Publishing Guide](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)

---

**You're all set!** 🚀

Every GitHub Release will automatically publish to NuGet.

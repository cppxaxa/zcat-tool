# Packaging and Publishing to NuGet

This guide walks you through packaging the `zcat` tool and publishing it to NuGet.org.

## Prerequisites

- ✅ .NET 8.0 SDK installed
- ✅ NuGet account at [nuget.org](https://www.nuget.org/)
- ✅ API key from NuGet.org

---

## Step 1: Get Your NuGet API Key

1. Go to [https://www.nuget.org/](https://www.nuget.org/) and sign in
2. Click your username → **API Keys**
3. Click **Create** to generate a new API key
4. Give it a name like `zcat-tool-publish`
5. Select **Push** and **Push new packages and package versions** scopes
6. Set **Glob Pattern** to `Zcat.Tool*` (or `*` for all packages)
7. Click **Create**
8. **IMPORTANT**: Copy the API key immediately - you won't be able to see it again!

---

## Step 2: Configure Your API Key Locally

### Option A: Using dotnet CLI (Recommended)

```bash
# Store the API key in your local NuGet config
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Add your API key
dotnet nuget setapikey YOUR-API-KEY-HERE --source nuget.org
```

### Option B: Using Environment Variable

```bash
# Windows (PowerShell)
$env:NUGET_API_KEY="YOUR-API-KEY-HERE"

# Linux/Mac
export NUGET_API_KEY="YOUR-API-KEY-HERE"
```

---

## Step 3: Update Package Metadata

Before publishing, update the `.csproj` file with your information:

```xml
<PropertyGroup>
  <Version>1.0.0</Version> <!-- Increment for each release -->
  <Authors>cppxaxa</Authors>
  <Description>Your package description</Description>
  <PackageProjectUrl>https://github.com/cppxaxa/zcat-tool</PackageProjectUrl>
  <RepositoryUrl>https://github.com/cppxaxa/zcat-tool</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageTags>zeromq;cli;messaging;netmq;pub-sub</PackageTags>
</PropertyGroup>
```

**Key fields to update:**
- `Version` - Semantic versioning (e.g., 1.0.0, 1.0.1, 1.1.0)
- `Authors` - Your name or organization
- `PackageProjectUrl` - Your GitHub repo URL
- `RepositoryUrl` - Your GitHub repo URL
- `PackageTags` - Search keywords (semicolon-separated)

---

## Step 4: Build and Pack

### Clean Build

```bash
# Navigate to project directory
cd zcat-tool

# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore
```

### Create the NuGet Package

```bash
# Build in Release mode and create .nupkg
dotnet pack -c Release
```

This creates a `.nupkg` file in:
```
bin/Release/Zcat.Tool.1.0.0.nupkg
```

### Verify Package Contents

```bash
# Extract and inspect (optional)
# Windows
Expand-Archive bin/Release/Zcat.Tool.1.0.0.nupkg -DestinationPath bin/Release/package

# Linux/Mac
unzip bin/Release/Zcat.Tool.1.0.0.nupkg -d bin/Release/package
```

---

## Step 5: Test Locally Before Publishing

**ALWAYS test locally before publishing to NuGet!**

### Install Locally

```bash
# Install from local .nupkg
dotnet tool install --global --add-source ./bin/Release Zcat.Tool

# Or update if already installed
dotnet tool update --global --add-source ./bin/Release Zcat.Tool
```

### Test the Tool

```bash
# Verify installation
zcat --help

# Test subscriber (Terminal 1)
zcat sub tcp://localhost:5556 --timeout 5

# Test publisher (Terminal 2)
echo "Hello from zcat!" | zcat pub tcp://localhost:5556
```

### Uninstall Test Version

```bash
dotnet tool uninstall --global Zcat.Tool
```

---

## Step 6: Publish to NuGet

### Push to NuGet.org

```bash
# Push the package
dotnet nuget push bin/Release/Zcat.Tool.1.0.0.nupkg \
  --api-key YOUR-API-KEY-HERE \
  --source https://api.nuget.org/v3/index.json
```

**Or if you stored the API key in Step 2:**

```bash
dotnet nuget push bin/Release/Zcat.Tool.1.0.0.nupkg \
  --source https://api.nuget.org/v3/index.json
```

### Wait for Indexing

- Package appears immediately in your account
- Takes **5-15 minutes** to be searchable
- Check status at: `https://www.nuget.org/packages/Zcat.Tool`

---

## Step 7: Verify Published Package

### Install from NuGet

```bash
# Wait 10-15 minutes after publishing, then:
dotnet tool install --global Zcat.Tool
```

### Test Installation

```bash
zcat sub tcp://localhost:5556 --timeout 5
```

---

## Versioning Best Practices

Follow [Semantic Versioning](https://semver.org/):

- `1.0.0` - Initial release
- `1.0.1` - Backwards-compatible bug fix
- `1.1.0` - New feature, backwards-compatible
- `2.0.0` - Breaking change

### Updating Versions

1. Edit `Zcat.Tool.csproj`:
   ```xml
   <Version>1.0.1</Version>
   ```

2. Rebuild and republish:
   ```bash
   dotnet pack -c Release
   dotnet nuget push bin/Release/Zcat.Tool.1.0.1.nupkg --source https://api.nuget.org/v3/index.json
   ```

---

## Troubleshooting

### "Package already exists"

You cannot overwrite published versions. Increment the version number in `.csproj`.

### "Invalid API Key"

```bash
# Re-add your API key
dotnet nuget setapikey YOUR-NEW-API-KEY --source nuget.org
```

### "Package validation failed"

Common issues:
- Missing required metadata (`Authors`, `Description`, `LicenseExpression`)
- Invalid version format
- Package too large (>250 MB)

Check errors in the push output and fix in `.csproj`.

### Package not appearing in search

- Wait 15 minutes for indexing
- Check package status: `https://www.nuget.org/packages/Zcat.Tool`
- Verify you're searching on nuget.org, not a different feed

---

## CI/CD Automation (GitHub Actions)

Create `.github/workflows/publish.yml`:

```yaml
name: Publish to NuGet

on:
  release:
    types: [created]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release --no-restore

      - name: Pack
        run: dotnet pack -c Release --no-build --output .

      - name: Push to NuGet
        run: dotnet nuget push *.nupkg --api-key ${{secrets.NUGET_API_KEY}} --source https://api.nuget.org/v3/index.json
```

**Setup:**
1. Add API key to GitHub Secrets: Settings → Secrets → Actions → New secret
2. Name: `NUGET_API_KEY`, Value: your API key
3. Create a GitHub Release to trigger publish

---

## Unpublishing / Delisting

### Unlist (Hide from search, but allow existing installs)

```bash
dotnet nuget delete Zcat.Tool 1.0.0 \
  --api-key YOUR-API-KEY \
  --source https://api.nuget.org/v3/index.json \
  --non-interactive
```

Or on nuget.org:
1. Go to package page
2. Click **Manage Package** → **Unlist**

**Note**: Unlisted packages can still be installed if version is specified explicitly.

---

## Quick Reference Commands

```bash
# Build & pack
dotnet pack -c Release

# Test locally
dotnet tool install --global --add-source ./bin/Release Zcat.Tool

# Publish to NuGet
dotnet nuget push bin/Release/Zcat.Tool.1.0.0.nupkg --source https://api.nuget.org/v3/index.json

# Update version & republish
# 1. Edit Version in .csproj
# 2. Re-pack and push
dotnet pack -c Release
dotnet nuget push bin/Release/Zcat.Tool.1.0.1.nupkg --source https://api.nuget.org/v3/index.json

# Uninstall
dotnet tool uninstall --global Zcat.Tool
```

---

## Additional Resources

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Creating .NET Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)
- [Semantic Versioning](https://semver.org/)
- [NuGet Package Best Practices](https://docs.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices)

---

**🎉 Congratulations!** Your package is now on NuGet and anyone can install it with:

```bash
dotnet tool install --global Zcat.Tool
```

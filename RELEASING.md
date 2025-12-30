# 📦 Releasing New BuildingBlocks Versions

This guide explains how to release new versions of the BuildingBlocks NuGet packages.

---

## 📋 Prerequisites

- Write access to the repository
- GitHub Personal Access Token with `write:packages` scope
- .NET 9.0 SDK installed

---

## 🔄 Release Process

### Step 1: Make Your Changes

Edit files in `BuildingBlocks/` directory:

```
BuildingBlocks/
├── BuildingBlocks.Domain/
├── BuildingBlocks.Application/
├── BuildingBlocks.Infrastructure/
└── BuildingBlocks.API/
```

### Step 2: Determine Version Number

Follow [Semantic Versioning](https://semver.org/):

| Change Type | Version Bump | Example |
|-------------|--------------|---------|
| Bug fix | PATCH | 2.1.1 → 2.1.2 |
| New feature (backward compatible) | MINOR | 2.1.1 → 2.2.0 |
| Breaking change | MAJOR | 2.1.1 → 3.0.0 |

### Step 3: Build & Pack All Packages

```bash
cd /path/to/microservice-template

# Set version
VERSION=2.2.0

# Pack all packages
dotnet pack BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj \
  -o nupkgs -c Release /p:Version=$VERSION

dotnet pack BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj \
  -o nupkgs -c Release /p:Version=$VERSION

dotnet pack BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj \
  -o nupkgs -c Release /p:Version=$VERSION

dotnet pack BuildingBlocks/BuildingBlocks.API/BuildingBlocks.API.csproj \
  -o nupkgs -c Release /p:Version=$VERSION
```

### Step 4: Push to GitHub Packages

```bash
# Set your GitHub token
GITHUB_TOKEN=ghp_your_token_here

# Push all packages
dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Domain.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  -k $GITHUB_TOKEN

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Application.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  -k $GITHUB_TOKEN

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Infrastructure.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  -k $GITHUB_TOKEN

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.API.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  -k $GITHUB_TOKEN
```

### Step 5: Update Template Package Versions

Edit `Services/HakuService/Directory.Packages.props`:

```xml
<!-- Update version numbers -->
<PackageVersion Include="Enterprise.BuildingBlocks.Domain" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.Application" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.Infrastructure" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.API" Version="2.2.0" />
```

### Step 6: Update CHANGELOG.md

Add entry to `CHANGELOG.md`:

```markdown
## [2.2.0] - 2025-01-15

### Added
- New feature X

### Changed
- Improved Y

### Fixed
- Bug in Z
```

### Step 7: Commit and Tag

```bash
git add .
git commit -m "Release BuildingBlocks v$VERSION"
git push

# Create version tag
git tag v$VERSION
git push origin v$VERSION
```

### Step 8: Reinstall Template (Local Testing)

```bash
dotnet new uninstall ./Services/HakuService
dotnet new install ./Services/HakuService
```

---

## 📝 Quick Script

Save this as `release.sh` for convenience:

```bash
#!/bin/bash
set -e

VERSION=$1
GITHUB_TOKEN=$2

if [ -z "$VERSION" ] || [ -z "$GITHUB_TOKEN" ]; then
    echo "Usage: ./release.sh <version> <github_token>"
    echo "Example: ./release.sh 2.2.0 ghp_xxxx"
    exit 1
fi

echo "📦 Building packages v$VERSION..."

# Pack
dotnet pack BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.API/BuildingBlocks.API.csproj -o nupkgs -c Release /p:Version=$VERSION

echo "🚀 Pushing to GitHub Packages..."

# Push
dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Domain.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Application.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Infrastructure.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.API.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

echo "✅ Done! Don't forget to:"
echo "   1. Update Services/HakuService/Directory.Packages.props"
echo "   2. Update CHANGELOG.md"
echo "   3. git add . && git commit -m 'Release v$VERSION' && git push"
echo "   4. git tag v$VERSION && git push origin v$VERSION"
```

**Usage:**
```bash
chmod +x release.sh
./release.sh 2.2.0 ghp_your_token
```

---

## 🔄 Updating Existing Services

After releasing a new version, developers update their services:

```bash
cd my-service

# Update all BuildingBlocks packages
dotnet add package Enterprise.BuildingBlocks.Domain --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Application --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Infrastructure --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.API --version 2.2.0

# Or update Directory.Packages.props and restore
dotnet restore
```

---

## ⚠️ Important Notes

1. **All 4 packages must be released together** with the same version number
2. **Test before releasing** - create a test service and verify it builds
3. **Never delete published versions** - publish a new patch version instead
4. **Update CHANGELOG.md** for every release
5. **Git tags** help track which code corresponds to which version

---

## 🔍 Verifying Packages

Check packages on GitHub:

**URL:** https://github.com/orgs/property-technology-solutions/packages

Or via API:
```bash
curl -H "Authorization: Bearer $GITHUB_TOKEN" \
  "https://api.github.com/orgs/property-technology-solutions/packages?package_type=nuget"
```

---

## 🆘 Troubleshooting

### "Version already exists" error
Package version is immutable. Bump the version number.

### "Unauthorized" error
Check your GitHub token has `write:packages` scope.

### Services can't find packages
1. Verify `nuget.config` has correct feed URL
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Check package visibility (should match repo visibility)


# 📦 Releasing New BuildingBlocks Versions

This guide explains how to release new versions of the BuildingBlocks NuGet packages.

---

## 🚀 Automated Release (Recommended)

Simply push a version tag and GitHub Actions will automatically:
1. Build all packages
2. Run tests
3. Publish to GitHub Packages
4. Create a GitHub Release

### How to Release

```bash
# 1. Make your changes in BuildingBlocks/
# 2. Update CHANGELOG.md
# 3. Commit changes
git add .
git commit -m "feat: add new feature X"
git push

# 4. Create and push version tag
git tag v2.2.0
git push origin v2.2.0
```

**That's it!** 🎉 The CI/CD pipeline handles everything else.

### What Happens Automatically

```
git push origin v2.2.0
         ↓
┌─────────────────────────────────────┐
│  GitHub Actions (.github/workflows/release.yml)  │
├─────────────────────────────────────┤
│  1. Checkout code                   │
│  2. Setup .NET 9.0                  │
│  3. Restore & Build                 │
│  4. Run tests                       │
│  5. Pack all 4 packages             │
│  6. Push to GitHub Packages         │
│  7. Create GitHub Release           │
└─────────────────────────────────────┘
         ↓
📦 Packages available at:
https://github.com/orgs/property-technology-solutions/packages
```

---

## 📋 Version Number Guidelines

Follow [Semantic Versioning](https://semver.org/):

| Change Type | Version Bump | Example | When to Use |
|-------------|--------------|---------|-------------|
| Bug fix | PATCH | 2.1.1 → 2.1.2 | Backward-compatible bug fixes |
| New feature | MINOR | 2.1.1 → 2.2.0 | Backward-compatible new functionality |
| Breaking change | MAJOR | 2.1.1 → 3.0.0 | Incompatible API changes |

---

## 📝 Pre-Release Checklist

Before tagging a release:

- [ ] All changes committed to `main` branch
- [ ] All tests passing locally (`dotnet test`)
- [ ] CHANGELOG.md updated with new version section
- [ ] Breaking changes documented (if any)
- [ ] Template tested with new BuildingBlocks

### Update CHANGELOG.md

```markdown
## [2.2.0] - 2025-01-15

### Added
- New feature X

### Changed
- Improved Y

### Fixed
- Bug in Z
```

---

## 🔄 Update Template After Release

After the automated release completes, update the template to use the new versions:

### 1. Edit `Services/HakuService/Directory.Packages.props`

```xml
<PackageVersion Include="Enterprise.BuildingBlocks.Domain" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.Application" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.Infrastructure" Version="2.2.0" />
<PackageVersion Include="Enterprise.BuildingBlocks.API" Version="2.2.0" />
```

### 2. Commit and Push

```bash
git add .
git commit -m "chore: update template to use BuildingBlocks v2.2.0"
git push
```

### 3. Reinstall Template Locally

```bash
dotnet new uninstall ./Services/HakuService
dotnet new install ./Services/HakuService
```

---

## 🔄 Updating Existing Services

After releasing a new version, developers update their services:

### Option 1: Update Package References

```bash
cd my-service
dotnet add package Enterprise.BuildingBlocks.Domain --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Application --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Infrastructure --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.API --version 2.2.0
```

### Option 2: Edit Directory.Packages.props

```xml
<!-- Update versions in Directory.Packages.props -->
<PackageVersion Include="Enterprise.BuildingBlocks.Domain" Version="2.2.0" />
```

Then restore:
```bash
dotnet restore
```

---

## 🛠️ Manual Release (Alternative)

If you need to release manually (e.g., CI is down):

```bash
#!/bin/bash
VERSION=2.2.0
GITHUB_TOKEN=ghp_your_token

# Build
dotnet build --configuration Release

# Pack all packages
dotnet pack BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj \
  -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj \
  -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj \
  -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.API/BuildingBlocks.API.csproj \
  -o nupkgs -c Release /p:Version=$VERSION

# Push to GitHub Packages
for pkg in nupkgs/*.nupkg; do
  dotnet nuget push "$pkg" \
    -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
    -k $GITHUB_TOKEN --skip-duplicate
done

# Tag
git tag v$VERSION
git push origin v$VERSION
```

---

## 🔍 Verifying Release

### Check GitHub Releases
https://github.com/property-technology-solutions/microservice-template/releases

### Check GitHub Packages
https://github.com/orgs/property-technology-solutions/packages

### Via CLI
```bash
dotnet nuget list source
dotnet restore --verbosity detailed
```

---

## ⚠️ Important Notes

1. **All 4 packages are released together** with the same version number
2. **Tags trigger releases** - only push a tag when ready to release
3. **Package versions are immutable** - you cannot overwrite a published version
4. **Test before tagging** - ensure CI passes on main before creating a release tag
5. **GitHub Token** - CI uses `GITHUB_TOKEN` secret (automatically available)

---

## 🆘 Troubleshooting

### CI Failed During Release

1. Check Actions tab for error details
2. Fix the issue
3. Delete the tag: `git push origin --delete v2.2.0`
4. Delete local tag: `git tag -d v2.2.0`
5. Re-tag after fix: `git tag v2.2.0 && git push origin v2.2.0`

### "Version already exists" error

Package version is immutable. You must increment the version:
```bash
git tag v2.2.1
git push origin v2.2.1
```

### "Unauthorized" error in manual release

Ensure your GitHub token has `write:packages` scope.

### Services can't find new packages

1. Wait a few minutes for propagation
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Verify `nuget.config` has correct feed URL
4. Check package visibility matches repo visibility

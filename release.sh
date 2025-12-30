#!/bin/bash
set -e

VERSION=$1
GITHUB_TOKEN=$2

if [ -z "$VERSION" ] || [ -z "$GITHUB_TOKEN" ]; then
    echo "Usage: ./release.sh <version> <github_token>"
    echo "Example: ./release.sh 2.2.0 ghp_xxxx"
    exit 1
fi

echo "📦 Building BuildingBlocks packages v$VERSION..."

# Pack all packages
dotnet pack BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj -o nupkgs -c Release /p:Version=$VERSION
dotnet pack BuildingBlocks/BuildingBlocks.API/BuildingBlocks.API.csproj -o nupkgs -c Release /p:Version=$VERSION

echo "🚀 Pushing to GitHub Packages..."

# Push all packages
dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Domain.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Application.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.Infrastructure.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

dotnet nuget push "nupkgs/Enterprise.BuildingBlocks.API.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" -k $GITHUB_TOKEN --skip-duplicate

echo ""
echo "✅ Packages pushed successfully!"
echo ""
echo "📋 Next steps:"
echo "   1. Update Services/HakuService/Directory.Packages.props with version $VERSION"
echo "   2. Update CHANGELOG.md"
echo "   3. Run: git add . && git commit -m 'Release v$VERSION' && git push"
echo "   4. Run: git tag v$VERSION && git push origin v$VERSION"


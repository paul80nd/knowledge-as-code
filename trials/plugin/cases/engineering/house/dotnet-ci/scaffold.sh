#!/bin/sh
# Writes a .NET library with an xUnit test project and no CI.
set -eu

mkdir -p src/Catalogue tests/Catalogue.Tests

cat > src/Catalogue/Catalogue.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
EOF

cat > src/Catalogue/Price.cs <<'EOF'
namespace Catalogue;

public static class Price
{
    public static int WithVat(int netPence) => netPence * 6 / 5;
}
EOF

cat > tests/Catalogue.Tests/Catalogue.Tests.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Catalogue/Catalogue.csproj" />
  </ItemGroup>
</Project>
EOF

cat > tests/Catalogue.Tests/PriceTests.cs <<'EOF'
namespace Catalogue.Tests;

public class PriceTests
{
    [Fact]
    public void WithVat_AddsTwentyPercent() => Assert.Equal(120, Price.WithVat(100));
}
EOF

cat > README.md <<'EOF'
# catalogue

Prices for the shop.

    dotnet test tests/Catalogue.Tests
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the catalogue library"

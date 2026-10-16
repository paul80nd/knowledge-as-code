#!/bin/sh
# Writes an ASP.NET service whose start-up log call breaks a warnings-as-errors build.
set -eu

mkdir -p src/Covers.Api

cat > src/Covers.Api/Covers.Api.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>Recommended</AnalysisMode>
  </PropertyGroup>
</Project>
EOF

cat > src/Covers.Api/Program.cs <<'EOF'
namespace Covers.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/v1/covers/{isbn}", (string isbn) => Results.NotFound());

        app.Lifetime.ApplicationStarted.Register(() =>
            app.Logger.LogInformation("Covers API listening on {Urls}", string.Join(", ", app.Urls)));

        app.Run();
    }
}
EOF

cat > .editorconfig <<'EOF'
root = true

[*.cs]
indent_style = space
indent_size = 4
EOF

cat > README.md <<'EOF'
# covers

Serves book covers.

    dotnet build src/Covers.Api
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the covers API"

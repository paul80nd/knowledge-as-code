#!/bin/sh
# Writes kac-lint, a new .NET global tool in its own repository, with no CI.
set -eu

mkdir -p src/KacLint

cat > src/KacLint/KacLint.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>kac-lint</ToolCommandName>
    <PackageId>KacLint</PackageId>
    <Version>0.1.0</Version>
    <Authors>Paul Law</Authors>
    <Description>Checks the prose of a knowledge-as-code corpus for the house style.</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>

  <ItemGroup>
    <None Include="../../README.md" Pack="true" PackagePath="/" />
  </ItemGroup>

</Project>
EOF

cat > src/KacLint/Program.cs <<'EOF'
using System.Text.RegularExpressions;

var root = args.Length > 0 ? args[0] : ".";
var findings = 0;

foreach (var path in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
{
    var lines = File.ReadAllLines(path);
    for (var i = 0; i < lines.Length; i++)
    {
        if (lines[i].Contains('—'))
        {
            Console.WriteLine($"{path}:{i + 1}: em dash");
            findings++;
        }
        if (Regex.IsMatch(lines[i], @"\b(color|organization|behavior)\b"))
        {
            Console.WriteLine($"{path}:{i + 1}: US spelling");
            findings++;
        }
    }
}

Console.WriteLine($"kac-lint: {findings} finding(s).");
return findings == 0 ? 0 : 1;
EOF

cat > README.md <<'EOF'
# kac-lint

Checks the prose of a knowledge-as-code corpus for the house style: no em dash, and British spelling.

```sh
dotnet tool install --global KacLint
kac-lint examples/dog-fooding
```

kac-lint is a new service, separate from `kac`. It lives in its own repository and ships as its own package on
nuget.org.
EOF

cat > .gitignore <<'EOF'
bin/
obj/
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start kac-lint"

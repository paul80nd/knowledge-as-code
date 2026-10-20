#!/bin/sh
# Writes a new console project that references no package yet, so the session chooses what parses its command line.
set -eu

mkdir -p tooling/kac-trials

cat > tooling/kac-trials/KacTrials.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>kac-trials</AssemblyName>
  </PropertyGroup>

</Project>
EOF

cat > tooling/kac-trials/Program.cs <<'EOF'
Console.Error.WriteLine("usage: kac-trials <verb> [options]");
return 2;
EOF

cat > README.md <<'EOF'
# knowledge-as-code

The `kac` command line tool, its documentation site, and the corpus that describes them. `tooling/kac-trials` is a
companion tool for the trials under `trials/`.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start kac-trials"

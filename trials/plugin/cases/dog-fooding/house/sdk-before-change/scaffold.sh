#!/bin/sh
# Writes a repository whose one workflow installs the SDK by range, because the case asks about changing that step.
set -eu

cat > README.md <<'EOF'
# knowledge-as-code

The `kac` command line tool, its documentation site, and the corpus that describes them.
EOF

mkdir -p .github/workflows
cat > .github/workflows/kac.yml <<'EOF'
name: kac

on:
  pull_request:
    branches: [main]

permissions:
  contents: read

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - run: dotnet test tooling/kac.tests
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the repository"

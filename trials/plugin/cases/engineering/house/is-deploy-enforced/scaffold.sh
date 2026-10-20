#!/bin/sh
# Writes a repository with no code, because the case asks a question about the corpus.
set -eu

cat > README.md <<'EOF'
# platform

Notes for the teams that build and run the estate's services.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the repository"

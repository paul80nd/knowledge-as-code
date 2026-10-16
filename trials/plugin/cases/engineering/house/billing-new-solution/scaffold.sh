#!/bin/sh
# Writes an empty repository for a .NET billing library.
set -eu

cat > README.md <<'EOF'
# billing

Invoicing for the shop, in C# on .NET 8.
EOF

cat > .gitignore <<'EOF'
bin/
obj/
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the billing repository"

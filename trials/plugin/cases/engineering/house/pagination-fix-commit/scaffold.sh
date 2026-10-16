#!/bin/sh
# Writes a pagination helper with an off-by-one on every page after the first.
set -eu

mkdir -p listing tests

cat > listing/__init__.py <<'EOF'

EOF

cat > listing/paginate.py <<'EOF'
"""Splits a result list into pages."""


def paginate(items: list, page: int, size: int) -> list:
    """Return page `page` (from 1) of `items`, `size` items to a page."""
    start = max(0, (page - 1) * size - 1)
    return items[start:start + size]
EOF

cat > tests/test_paginate.py <<'EOF'
from listing.paginate import paginate


def test_first_page():
    assert paginate(list(range(10)), 1, 3) == [0, 1, 2]
EOF

cat > requirements.txt <<'EOF'
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# listing

Shared helpers for list endpoints.

    pytest
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the listing helpers"

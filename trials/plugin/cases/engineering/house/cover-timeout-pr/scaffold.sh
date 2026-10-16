#!/bin/sh
# Writes a Python client for the book-cover service with an unbounded call.
set -eu

mkdir -p covers tests

cat > covers/__init__.py <<'EOF'

EOF

cat > covers/client.py <<'EOF'
"""Fetches book covers from the upstream cover service."""

import requests


class CoverClient:
    def __init__(self, base_url: str) -> None:
        self.base_url = base_url

    def fetch(self, isbn: str) -> bytes | None:
        response = requests.get(f"{self.base_url}/covers/{isbn}")
        if response.status_code == 404:
            return None
        response.raise_for_status()
        return response.content
EOF

cat > tests/test_client.py <<'EOF'
from covers.client import CoverClient


def test_fetch_returns_none_for_unknown_isbn(monkeypatch):
    class Missing:
        status_code = 404

    monkeypatch.setattr("requests.get", lambda *a, **k: Missing())
    assert CoverClient("https://covers.invalid").fetch("0000") is None
EOF

cat > requirements.txt <<'EOF'
requests==2.32.3
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# covers

The product page's client for the upstream cover service.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the cover client"

#!/bin/sh
# Writes a uv-managed Python library with a lockfile and no CI.
set -eu

mkdir -p src/geocode tests

cat > pyproject.toml <<'EOF'
[project]
name = "geocode"
version = "0.4.0"
requires-python = ">=3.12"
dependencies = ["httpx==0.27.2"]

[dependency-groups]
dev = ["pytest==8.3.3"]

[build-system]
requires = ["hatchling"]
build-backend = "hatchling.build"
EOF

cat > uv.lock <<'EOF'
version = 1
requires-python = ">=3.12"

[[package]]
name = "geocode"
version = "0.4.0"
source = { editable = "." }
dependencies = [
    { name = "httpx" },
]

[package.dev-dependencies]
dev = [
    { name = "pytest" },
]

[[package]]
name = "httpx"
version = "0.27.2"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "pytest"
version = "8.3.3"
source = { registry = "https://pypi.org/simple" }
EOF

cat > src/geocode/__init__.py <<'EOF'
"""Turns a postcode into a latitude and longitude."""

import httpx


def locate(postcode: str, base_url: str) -> tuple[float, float]:
    response = httpx.get(f"{base_url}/postcodes/{postcode}", timeout=5.0)
    response.raise_for_status()
    body = response.json()
    return body["lat"], body["lon"]
EOF

cat > tests/test_locate.py <<'EOF'
import httpx

from geocode import locate


def test_locate_reads_lat_and_lon(monkeypatch):
    def fake_get(url, timeout):
        return httpx.Response(200, json={"lat": 53.8, "lon": -1.55}, request=httpx.Request("GET", url))

    monkeypatch.setattr(httpx, "get", fake_get)
    assert locate("LS1 1AA", "https://geo.invalid") == (53.8, -1.55)
EOF

cat > README.md <<'EOF'
# geocode

Turns a postcode into a latitude and longitude.

    uv sync
    uv run pytest
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the geocode library"

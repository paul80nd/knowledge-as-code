#!/bin/sh
# Writes a small Python library with tests and no CI.
set -eu

mkdir -p rates tests

cat > rates/__init__.py <<'EOF'

EOF

cat > rates/calc.py <<'EOF'
"""Shipping rates by weight band."""

BANDS = [(1000, 395), (2000, 545), (5000, 795)]


def rate_pence(grams: int) -> int:
    for limit, pence in BANDS:
        if grams <= limit:
            return pence
    raise ValueError("too heavy to ship by post")
EOF

cat > tests/test_calc.py <<'EOF'
import pytest

from rates.calc import rate_pence


def test_light_parcel_is_first_band():
    assert rate_pence(500) == 395


def test_too_heavy_is_refused():
    with pytest.raises(ValueError):
        rate_pence(9000)
EOF

cat > requirements.txt <<'EOF'
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# rates

Shipping rates by weight.

    pip install -r requirements.txt
    pytest
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the rates library"

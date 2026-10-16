#!/bin/sh
# Writes a small Python library with tests and no CI.
set -eu

mkdir -p invoices tests

cat > invoices/__init__.py <<'EOF'

EOF

cat > invoices/totals.py <<'EOF'
"""Invoice arithmetic."""

from decimal import Decimal


def line_total(quantity: int, unit_price: Decimal) -> Decimal:
    return unit_price * quantity


def invoice_total(lines: list[tuple[int, Decimal]], vat_rate: Decimal) -> Decimal:
    net = sum((line_total(q, p) for q, p in lines), Decimal("0"))
    return (net * (1 + vat_rate)).quantize(Decimal("0.01"))
EOF

cat > tests/test_totals.py <<'EOF'
from decimal import Decimal

from invoices.totals import invoice_total


def test_invoice_total_adds_vat():
    assert invoice_total([(2, Decimal("10.00"))], Decimal("0.20")) == Decimal("24.00")
EOF

cat > requirements.txt <<'EOF'
pytest==8.3.3
ruff==0.6.9
EOF

cat > README.md <<'EOF'
# invoices

Invoice arithmetic for the shop.

    pip install -r requirements.txt
    ruff check .
    pytest
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the invoices library"

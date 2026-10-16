#!/bin/sh
# Writes the stock service with one versioned read endpoint.
set -eu

mkdir -p stock tests

cat > stock/__init__.py <<'EOF'

EOF

cat > stock/store.py <<'EOF'
"""An in-memory stock table, standing in for the warehouse database."""

LEVELS: dict[str, dict[str, int]] = {
    "SKU-1001": {"LEEDS": 5, "BRISTOL": 7},
    "SKU-1002": {"LEEDS": 0, "BRISTOL": 0},
    "SKU-1003": {"LEEDS": 40},
}


def level(sku: str) -> int | None:
    by_site = LEVELS.get(sku)
    return None if by_site is None else sum(by_site.values())


def levels_by_warehouse(sku: str) -> dict[str, int] | None:
    return LEVELS.get(sku)
EOF

cat > stock/app.py <<'EOF'
"""The stock service. Other services ask it how much of a SKU is on hand."""

from flask import Flask, abort

from stock import store

app = Flask(__name__)


@app.get("/v1/stock/<sku>")
def get_stock(sku: str):
    on_hand = store.level(sku)
    if on_hand is None:
        abort(404)
    return {"sku": sku, "on_hand": on_hand}
EOF

cat > tests/test_app.py <<'EOF'
from stock.app import app


def test_get_stock_returns_level():
    client = app.test_client()
    response = client.get("/v1/stock/SKU-1001")
    assert response.status_code == 200
    assert response.get_json()["on_hand"] == 12


def test_get_stock_unknown_sku_is_404():
    client = app.test_client()
    assert client.get("/v1/stock/NOPE").status_code == 404
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# stock

Tells other services how much of each SKU is on hand.

    pip install -r requirements.txt
    flask --app stock.app run
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the stock service"

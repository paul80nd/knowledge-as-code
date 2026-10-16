#!/bin/sh
# Writes a Flask service with a health route and no API yet.
set -eu

mkdir -p returns

cat > returns/__init__.py <<'EOF'

EOF

cat > returns/store.py <<'EOF'
"""An in-memory table of returns, standing in for the database."""

import itertools

RETURNS: dict[int, dict] = {}
_ids = itertools.count(1)


def add(order_id: str, line: int, reason: str) -> dict:
    record = {"id": next(_ids), "order_id": order_id, "line": line, "reason": reason}
    RETURNS[record["id"]] = record
    return record


def get(return_id: int) -> dict | None:
    return RETURNS.get(return_id)
EOF

cat > returns/app.py <<'EOF'
"""The returns service."""

from flask import Flask

app = Flask(__name__)


@app.get("/healthz")
def healthz():
    return {"ok": True}
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# returns

Records the returns customers ask for.

    pip install -r requirements.txt
    flask --app returns.app run
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the returns service"

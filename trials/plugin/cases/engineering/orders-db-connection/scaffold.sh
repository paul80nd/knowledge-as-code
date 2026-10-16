#!/bin/sh
# Writes a small Python service whose database connection is not written yet.
set -eu

mkdir -p app

cat > app/__init__.py <<'EOF'
EOF

cat > app/db.py <<'EOF'
"""Database access for the orders service."""

import psycopg


def get_connection() -> psycopg.Connection:
    """Return a connection to the orders database."""
    raise NotImplementedError("no database configured yet")


def count_open_orders() -> int:
    with get_connection() as conn:
        row = conn.execute("SELECT count(*) FROM orders WHERE status = 'open'").fetchone()
        return row[0]
EOF

cat > app/main.py <<'EOF'
"""Entry point for the orders service."""

from app.db import count_open_orders


def main() -> None:
    print(f"open orders: {count_open_orders()}")


if __name__ == "__main__":
    main()
EOF

cat > requirements.txt <<'EOF'
psycopg[binary]==3.2.3
EOF

cat > README.md <<'EOF'
# orders

The orders service. It reports how many orders are open.

    pip install -r requirements.txt
    python -m app.main
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the orders service"

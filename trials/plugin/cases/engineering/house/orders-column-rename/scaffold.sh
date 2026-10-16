#!/bin/sh
# Writes an orders repository over Postgres with one migration.
set -eu

mkdir -p migrations orders

cat > migrations/0001_create_orders.sql <<'EOF'
CREATE TABLE orders (
    id            bigserial PRIMARY KEY,
    customer_name text        NOT NULL,
    total_pence   integer     NOT NULL,
    placed_at     timestamptz NOT NULL DEFAULT now()
);
EOF

cat > orders/__init__.py <<'EOF'

EOF

cat > orders/repository.py <<'EOF'
"""Reads and writes orders."""

import psycopg


def add(conn: psycopg.Connection, customer_name: str, total_pence: int) -> int:
    row = conn.execute(
        "INSERT INTO orders (customer_name, total_pence) VALUES (%s, %s) RETURNING id",
        (customer_name, total_pence),
    ).fetchone()
    return row[0]


def get(conn: psycopg.Connection, order_id: int) -> dict | None:
    row = conn.execute(
        "SELECT id, customer_name, total_pence FROM orders WHERE id = %s", (order_id,)
    ).fetchone()
    if row is None:
        return None
    return {"id": row[0], "customer_name": row[1], "total_pence": row[2]}
EOF

cat > requirements.txt <<'EOF'
psycopg[binary]==3.2.3
EOF

cat > README.md <<'EOF'
# orders

Stores orders in Postgres. The deploy pipeline applies every migration in `migrations/`, in order, then rolls the new
version out across the running instances.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the orders repository"

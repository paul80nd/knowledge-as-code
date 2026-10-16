#!/bin/sh
# Writes a Flask orders service that takes the price from the request body.
set -eu

mkdir -p orders

cat > orders/__init__.py <<'EOF'

EOF

cat > orders/app.py <<'EOF'
"""The orders service."""

from flask import Flask, request

app = Flask(__name__)


@app.post("/v1/orders")
def create_order():
    body = request.get_json()
    quantity = int(body["quantity"])
    unit_price = float(body["unit_price"])
    return {"sku": body["sku"], "quantity": quantity, "total": round(quantity * unit_price, 2)}, 201
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
requests==2.32.3
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# orders

Takes orders from checkout. The pricing service lives at `PRICING_URL`.

    pip install -r requirements.txt
    flask --app orders.app run
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the orders service"

#!/bin/sh
# Writes a Flask catalogue service with no logging.
set -eu

mkdir -p catalogue

cat > catalogue/__init__.py <<'EOF'

EOF

cat > catalogue/app.py <<'EOF'
"""The catalogue service."""

from flask import Flask, abort

PRODUCTS = {"SKU-1001": "Kettle", "SKU-1002": "Toaster"}

app = Flask(__name__)


@app.get("/v1/products")
def list_products():
    return {"products": [{"sku": k, "name": v} for k, v in PRODUCTS.items()]}


@app.get("/v1/products/<sku>")
def get_product(sku: str):
    if sku not in PRODUCTS:
        abort(404)
    return {"sku": sku, "name": PRODUCTS[sku]}
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# catalogue

Serves the product catalogue.

    pip install -r requirements.txt
    flask --app catalogue.app run
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the catalogue service"

#!/bin/sh
# Writes a small Python web service with no Dockerfile.
set -eu

mkdir -p labels

cat > labels/__init__.py <<'EOF'
EOF

cat > labels/server.py <<'EOF'
"""Renders shipping labels as PDF for the warehouse printers."""

from flask import Flask, request, send_file

from labels.render import render_label

app = Flask(__name__)


@app.get("/healthz")
def healthz():
    return {"status": "ok"}


@app.post("/labels")
def create_label():
    body = request.get_json()
    pdf_path = render_label(body["order_id"], body["address"])
    return send_file(pdf_path, mimetype="application/pdf")
EOF

cat > labels/render.py <<'EOF'
"""Writes one label to a temporary PDF file."""

import tempfile

from reportlab.pdfgen import canvas


def render_label(order_id: str, address: str) -> str:
    handle = tempfile.NamedTemporaryFile(suffix=".pdf", delete=False)
    pdf = canvas.Canvas(handle.name, pagesize=(288, 432))
    pdf.drawString(20, 400, f"Order {order_id}")
    for i, line in enumerate(address.splitlines()):
        pdf.drawString(20, 370 - i * 16, line)
    pdf.save()
    return handle.name
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
gunicorn==23.0.0
reportlab==4.2.2
EOF

cat > README.md <<'EOF'
# label-printer

Renders shipping labels for the warehouse printers. It listens on port 8080.

    pip install -r requirements.txt
    gunicorn --bind 0.0.0.0:8080 labels.server:app
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the label printer"

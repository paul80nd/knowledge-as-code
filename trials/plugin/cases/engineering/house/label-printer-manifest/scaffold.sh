#!/bin/sh
# Writes a containerised Python service with no Kubernetes manifest.
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
    return {"ok": True}


@app.post("/labels")
def create_label():
    body = request.get_json()
    return send_file(render_label(body["order_id"], body["address"]), mimetype="application/pdf")
EOF

cat > labels/render.py <<'EOF'
"""Writes one label to a temporary PDF file."""

import tempfile

from reportlab.pdfgen import canvas


def render_label(order_id: str, address: str) -> str:
    handle = tempfile.NamedTemporaryFile(suffix=".pdf", delete=False)
    pdf = canvas.Canvas(handle.name, pagesize=(288, 432))
    pdf.drawString(20, 400, f"Order {order_id}")
    pdf.save()
    return handle.name
EOF

cat > Dockerfile <<'EOF'
FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY labels labels
USER 10001
EXPOSE 8080
CMD ["gunicorn", "--bind", "0.0.0.0:8080", "labels.server:app"]
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
gunicorn==23.0.0
reportlab==4.2.2
EOF

cat > README.md <<'EOF'
# label-printer

Renders shipping labels for the warehouse printers, on port 8080.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the label printer"

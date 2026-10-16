#!/bin/sh
# Writes a containerised Flask service with no health endpoint.
set -eu

mkdir -p covers .github/workflows

cat > covers/__init__.py <<'EOF'

EOF

cat > covers/app.py <<'EOF'
"""The covers service. Serves book-cover images by ISBN."""

from flask import Flask, abort, send_file

from covers.files import cover_path

app = Flask(__name__)


@app.get("/v1/covers/<isbn>")
def get_cover(isbn: str):
    path = cover_path(isbn)
    if path is None:
        abort(404)
    return send_file(path, mimetype="image/jpeg")
EOF

cat > covers/files.py <<'EOF'
"""Finds a cover image on disk."""

import os
from pathlib import Path

ROOT = Path(os.environ.get("COVERS_ROOT", "/data/covers"))


def cover_path(isbn: str) -> Path | None:
    path = ROOT / f"{isbn}.jpg"
    return path if path.is_file() else None
EOF

cat > Dockerfile <<'EOF'
FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY covers covers
USER 10001
EXPOSE 8080
CMD ["gunicorn", "--bind", "0.0.0.0:8080", "covers.app:app"]
EOF

cat > .github/workflows/build.yml <<'EOF'
name: build
on: push
jobs:
  image:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: docker build -t covers:${{ github.run_number }} .
EOF

cat > requirements.txt <<'EOF'
flask==3.0.3
gunicorn==23.0.0
EOF

cat > README.md <<'EOF'
# covers

Serves book covers by ISBN, on port 8080.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the covers service"

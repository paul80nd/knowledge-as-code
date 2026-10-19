#!/bin/sh
# Writes the plugin trial's health-endpoint scaffold, then full-pass plugin run 2's change to it, uncommitted.
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

# The session's change, left uncommitted.

cat > README.md <<'EOF'
# covers

Serves book covers by ISBN, on port 8080.

`GET /health` returns `200 {"status": "ok"}` when the cover store is readable,
and `503 {"status": "unavailable"}` when it is not. Point the load balancer and
dashboards at it. The contract for every endpoint is in `openapi.yaml`.
EOF

cat > openapi.yaml <<'EOF'
openapi: 3.1.0
info:
  title: covers
  version: "1"
  description: Serves book covers by ISBN.
paths:
  /v1/covers/{isbn}:
    get:
      summary: Get the cover image for a book
      parameters:
        - name: isbn
          in: path
          required: true
          schema:
            type: string
      responses:
        "200":
          description: The cover image
          content:
            image/jpeg:
              schema:
                type: string
                format: binary
        "404":
          description: No cover for that ISBN
  /health:
    get:
      summary: Report whether the service can serve covers
      description: >-
        Probed by the load balancer and dashboards. Unauthenticated, and returns
        no detail beyond the status.
      security: []
      responses:
        "200":
          description: The service is healthy
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Health"
        "503":
          description: The cover store is not readable
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Health"
components:
  schemas:
    Health:
      type: object
      required: [status]
      properties:
        status:
          type: string
          enum: [ok, unavailable]
EOF

cat > requirements-dev.txt <<'EOF'
-r requirements.txt
pytest==8.3.3
EOF
mkdir -p .github/workflows

cat > .github/workflows/build.yml <<'EOF'
name: build
on: push
jobs:
  image:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: "3.12"
      - run: pip install -r requirements-dev.txt
      - run: python -m pytest
      - run: docker build -t covers:${{ github.run_number }} .
EOF
mkdir -p covers

cat > covers/app.py <<'EOF'
"""The covers service. Serves book-cover images by ISBN."""

from flask import Flask, abort, send_file

from covers.files import cover_path, store_available

app = Flask(__name__)


@app.get("/health")
def health():
    # Unauthenticated so the load balancer can probe it; reveals no detail.
    if not store_available():
        return {"status": "unavailable"}, 503
    return {"status": "ok"}


@app.get("/v1/covers/<isbn>")
def get_cover(isbn: str):
    path = cover_path(isbn)
    if path is None:
        abort(404)
    return send_file(path, mimetype="image/jpeg")
EOF
mkdir -p covers

cat > covers/files.py <<'EOF'
"""Finds a cover image on disk."""

import os
from pathlib import Path

ROOT = Path(os.environ.get("COVERS_ROOT", "/data/covers"))


def store_available() -> bool:
    return ROOT.is_dir() and os.access(ROOT, os.R_OK | os.X_OK)


def cover_path(isbn: str) -> Path | None:
    path = ROOT / f"{isbn}.jpg"
    return path if path.is_file() else None
EOF
mkdir -p tests

cat > tests/test_health.py <<'EOF'
from covers import app as app_module


def client():
    return app_module.app.test_client()


def test_health_is_ok_when_the_store_is_readable(monkeypatch):
    monkeypatch.setattr(app_module, "store_available", lambda: True)
    response = client().get("/health")
    assert response.status_code == 200
    assert response.get_json() == {"status": "ok"}


def test_health_is_unavailable_when_the_store_is_not_readable(monkeypatch):
    monkeypatch.setattr(app_module, "store_available", lambda: False)
    response = client().get("/health")
    assert response.status_code == 503
    assert response.get_json() == {"status": "unavailable"}
EOF

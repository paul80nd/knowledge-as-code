#!/bin/sh
# Writes the covers service, then an uncommitted change logging each request in London time, giving the reason why.
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

# The change under review, left uncommitted.

cat > covers/request_log.py <<'EOF'
"""Writes one structured log line for each request the service answers."""

import json
import re
import sys
from datetime import datetime
from zoneinfo import ZoneInfo

# Branch staff read these lines on the counter terminals, and they read the time as London time. A UTC timestamp sent
# them to the wrong minute twice in the pilot, so the timestamp is London time with its offset, which still sorts and
# converts to UTC exactly.
LONDON = ZoneInfo("Europe/London")

TRACEPARENT = re.compile(r"^00-([0-9a-f]{32})-([0-9a-f]{16})-[0-9a-f]{2}$")


def trace_id(traceparent: str | None) -> str | None:
    if traceparent is None:
        return None
    match = TRACEPARENT.match(traceparent)
    return match.group(1) if match else None


def write(method: str, path: str, status: int, traceparent: str | None) -> None:
    line = {
        "timestamp": datetime.now(LONDON).isoformat(timespec="milliseconds"),
        "method": method,
        "path": path,
        "status": status,
        "trace_id": trace_id(traceparent),
    }
    print(json.dumps(line), file=sys.stdout, flush=True)
EOF

cat > covers/app.py <<'EOF'
"""The covers service. Serves book-cover images by ISBN."""

from flask import Flask, abort, request, send_file

from covers import request_log
from covers.files import cover_path

app = Flask(__name__)


@app.after_request
def log_request(response):
    request_log.write(request.method, request.path, response.status_code, request.headers.get("traceparent"))
    return response


@app.get("/v1/covers/<isbn>")
def get_cover(isbn: str):
    path = cover_path(isbn)
    if path is None:
        abort(404)
    return send_file(path, mimetype="image/jpeg")
EOF

cat > README.md <<'EOF'
# covers

Serves book covers by ISBN, on port 8080.

Each request writes one JSON line to stdout: the timestamp, the method, the path, the status and the trace id from the
inbound `traceparent`. The timestamp is London time with its offset, because branch staff read these lines in London
time.
EOF

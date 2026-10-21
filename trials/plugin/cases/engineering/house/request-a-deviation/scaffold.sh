#!/bin/sh
# Writes a stock-hold module whose timing test is flaky.
set -eu

mkdir -p holds tests

cat > holds/__init__.py <<'EOF'

EOF

cat > holds/hold.py <<'EOF'
"""A hold on stock that lapses after a number of seconds."""

import time


class Hold:
    def __init__(self, sku: str, seconds: float) -> None:
        self.sku = sku
        self.until = time.monotonic() + seconds

    def lapsed(self) -> bool:
        return time.monotonic() >= self.until
EOF

cat > tests/test_holds.py <<'EOF'
import time

from holds.hold import Hold


def test_new_hold_has_not_lapsed():
    assert not Hold("SKU-1", 60).lapsed()


def test_hold_lapses():
    hold = Hold("SKU-1", 0.05)
    time.sleep(0.05)
    assert hold.lapsed()
EOF

cat > requirements.txt <<'EOF'
pytest==8.3.3
EOF

cat > README.md <<'EOF'
# holds

Holds stock for a shopper while they check out.

    pytest
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the holds module"

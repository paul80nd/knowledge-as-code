#!/bin/sh
# Writes the stock service's scrape configuration and no alerting rules.
set -eu

mkdir -p monitoring

cat > monitoring/prometheus.yml <<'EOF'
global:
  scrape_interval: 15s

rule_files:
  - alerts.yml

scrape_configs:
  - job_name: stock
    static_configs:
      - targets: ["stock:8080"]
EOF

cat > README.md <<'EOF'
# stock

Tells other services how much of each SKU is on hand. It exports two metrics:

* `http_requests_total`, labelled by `route` and `code`
* `http_request_duration_seconds`, a histogram labelled by `route`
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the stock monitoring"
